using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Medallyon
{
    public class SaveManager : MonoBehaviour
    {
        public static readonly BindingFlags SaveableMemberFields =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        public static DirectoryInfo DataDirectory => new DirectoryInfo(Application.persistentDataPath);
        public static FileInfo DataFile => new FileInfo(Path.Combine(DataDirectory.FullName, "Data.dat"));
        private static FileInfo BackupDataFile => new FileInfo(Path.Combine(DataDirectory.FullName, "Data.bak.dat"));

        /// <summary>
        /// Load all fields & properties of all Components with the Saveable attribute.
        /// </summary>
        public static void SaveAll()
        {
            Dictionary<string, Dictionary<string, ComponentData>> toSave =
                new Dictionary<string, Dictionary<string, ComponentData>>();
            foreach (Saveable obj in FindObjectsOfType<Saveable>())
            {
                foreach (MonoBehaviour component in obj.GetComponents<MonoBehaviour>())
                {
                    MemberInfo[] members = component.GetType().GetMembers(SaveableMemberFields)
                        .Where(m => Attribute.IsDefined(m, typeof(SaveAttribute))).ToArray();
                    foreach (MemberInfo member in members)
                    {
                        if (!toSave.ContainsKey(obj.ID))
                            toSave.Add(obj.ID, new Dictionary<string, ComponentData>());

                        string componentName = component.GetType().Name;
                        if (!toSave[obj.ID].ContainsKey(componentName))
                            toSave[obj.ID].Add(componentName, new ComponentData());

                        toSave[obj.ID][componentName].Variables
                            .Add(new Variable(member.Name, member.GetValue(component)));
                    }
                }
            }

            File.WriteAllText(DataFile.FullName, JsonConvert.SerializeObject(toSave));
        }

        private static IEnumerable<MonoBehaviour> GetAllISaveableMonos()
        {
            List<MonoBehaviour> iSaveables = new List<MonoBehaviour>();
            foreach (Saveable obj in FindObjectsOfType<Saveable>())
            {
                MonoBehaviour[] objectComponents = obj.GetComponents<MonoBehaviour>();
                foreach (MonoBehaviour component in objectComponents)
                {
                    Type componentType = component.GetType();

                    Type iSaveable = componentType.GetInterface(nameof(ISaveable));
                    if (iSaveable == null)
                        continue;

                    iSaveables.Add(component);
                }
            }

            return iSaveables;
        }

        /// <summary>
        /// Restore all fields & properties of all Components with the Saveable attribute.
        /// </summary>
        public static void LoadAll()
        {
            if (!DataFile.Exists)
            {
                // Find all MonoBehaviours implementing the 'ISaveable' Interface
                MonoBehaviour[] iSaveables = GetAllISaveableMonos().ToArray();

                // Stop if there's nothing to process
                if (!iSaveables.Any())
                    return;

                // Get the 'OnFirstLoad' MethodInfo to be invoked from the 'ISaveable' interface
                MethodInfo onFirstLoad = iSaveables[0].GetType().GetInterface(nameof(ISaveable))
                    .GetMethod(nameof(ISaveable.OnFirstLoad));

                // Invoke OnFirstLoad
                foreach (MonoBehaviour mono in iSaveables)
                {
                    try
                    {
                        onFirstLoad?.Invoke(mono, null);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(
                            $"{mono.GetType().Name} failed to invoke {nameof(ISaveable.OnFirstLoad)}: {e.Message}");
                    }
                }

                return;
            }

            Dictionary<string, Dictionary<string, ComponentData>> data =
                JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, ComponentData>>>(
                    File.ReadAllText(DataFile.FullName));

            if (data == null)
            {
                Debug.LogError("The stored GameData is null. Aborting data restoration..");
                return;
            }

            // Iterate over every 'Saveable' component in the Scene that also exists in the Data file
            foreach (Saveable obj in FindObjectsOfType<Saveable>().Where(s => data.ContainsKey(s.ID)))
            {
                Dictionary<string, ComponentData> dataComponents = data[obj.ID];
                MonoBehaviour[] objectComponents = obj.GetComponents<MonoBehaviour>();
                foreach (MonoBehaviour component in objectComponents)
                {
                    Type componentType = component.GetType();
                    if (!dataComponents.ContainsKey(componentType.Name))
                        continue;

                    MethodInfo onRestoreMethod = componentType.GetInterface(nameof(ISaveable))
                        ?.GetMethod(nameof(ISaveable.OnRestore));

                    // This stores all loaded data for use with OnRestore
                    Dictionary<string, object> collection = new Dictionary<string, object>();

                    ComponentData componentData = dataComponents[componentType.Name];

                    List<MemberInfo> componentMembers =
                        new List<MemberInfo>(componentType.GetMembers(SaveableMemberFields));
                    foreach (Variable dataVar in componentData.Variables)
                    {
                        MemberInfo variable = componentMembers.Find(m =>
                            m.Name == dataVar.Name && Attribute.IsDefined(m, typeof(SaveAttribute)));

                        if (variable == null)
                        {
                            Debug.LogWarning(
                                $"Could not find Component Member ({obj.name}.{componentType.Name}.{dataVar.Name}) to restore data for. Has this member been moved?");
                            continue;
                        }

                        object adjustedValue = dataVar.Value;

                        try
                        {
                            Type memberType = variable.GetMemberType();
                            if (memberType == typeof(int))
                                adjustedValue = (int)(long)dataVar.Value;
                            else if (memberType == typeof(float))
                                adjustedValue = (float)(double)dataVar.Value;
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning(e);
                        }

                        collection.Add(dataVar.Name, adjustedValue);

                        try
                        {
                            // Set the value for primitive types. For non-primitives, OnRestore is called.

                            // Following: Special case for Guids
                            if (variable.GetMemberType() == typeof(Guid))
                                variable.SetValue(component, Guid.Parse((string)dataVar.Value));
                            else
                                variable.SetValue(component, dataVar.Value);
                        }
                        catch (ArgumentException e)
                        {
                            if (onRestoreMethod != null)
                                continue;

                            Debug.LogWarning(
                                $"Variable '{dataVar.Name}' ({dataVar.Value}) could not be applied to ({obj.name}.{component}.{dataVar.Name}): {e.Message}. Please enure you implement the {nameof(ISaveable)} interface on your {{{component}}} Component.");
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e);
                        }
                    }

                    // TODO: Analysis: Creates a number of Coroutines before Game Start, which may have a slight performance impact.
                    // Would it be better to have a single Coroutine calling 'OnRestore' for the collection of components?
                    if (onRestoreMethod != null)
                        RestoreThisFrame(component, onRestoreMethod, collection);
                }
            }
        }

        protected static void RestoreThisFrame(Component component, MethodInfo method, Dictionary<string, object> data)
        {
            try
            {
                // Invoke OnRestore for the Collection of variables
                method.Invoke(component, new object[] { data });
            }
            catch (Exception e)
            {
                throw new Exception($"Error restoring data for {component.GetType().Name}.{method.Name}", e);
            }
        }

        protected static IEnumerator RestoreNextFrame(Component component, MethodInfo method,
            Dictionary<string, object> data)
        {
            // Wait until all objects have woken up (processed their Awake)
            yield return new WaitForEndOfFrame();

            try
            {
                // Invoke OnRestore for the Collection of variables
                method.Invoke(component, new object[] { data });
            }
            catch (Exception e)
            {
                throw new Exception($"Error restoring data for {component.GetType().Name}.{method.Name}", e);
            }
        }

        /// <summary>
        /// This stores all <see cref="Variable" />s decorated with the <see cref="SaveAttribute" />.
        /// </summary>
        public class ComponentData
        {
            public List<Variable> Variables = new List<Variable>();
        }

        /// <summary>
        /// A KeyValue-like struct that holds the <see cref="Variable.Name" /> && <see cref="Variable.Value" /> of a
        /// <see cref="Component" /> Member.
        /// </summary>
        public struct Variable
        {
            public Variable(string name, object value)
            {
                Name = name;
                Value = value;
            }

            public string Name;
            public object Value;
        }

#if UNITY_EDITOR

        [MenuItem("Data/Open Data Folder")]
        private static void OpenDataFolder()
        {
            Process.Start(Application.persistentDataPath);
        }

        [MenuItem("Data/Delete Data File")]
        private static void DeleteDataFile()
        {
            BackupDataFile.Delete();
            DataFile.MoveTo(BackupDataFile.FullName);
        }

        [MenuItem("Data/Delete Data File", validate = true)]
        private static bool DeleteDataFileValidate()
        {
            return DataFile.Exists;
        }

        [MenuItem("Data/Restore Data File")]
        private static void RestoreDataFile()
        {
            FileInfo tempDataFile = new FileInfo(Path.Combine(DataDirectory.FullName, "Data.bak1.dat"));

            if (DataFile.Exists)
            {
                tempDataFile.Delete();
                DataFile.CopyTo(tempDataFile.FullName);
                DataFile.Delete();
            }

            BackupDataFile.CopyTo(DataFile.FullName);
            BackupDataFile.Delete();

            if (tempDataFile.Exists)
                tempDataFile.MoveTo(BackupDataFile.FullName);
        }

        [MenuItem("Data/Restore Data File", validate = true)]
        private static bool RestoreDataFileValidate()
        {
            return BackupDataFile.Exists;
        }

#endif
    }
}
