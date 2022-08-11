using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Medallyon
{
    public static class SaveManagerValidateUtil
    {
#if UNITY_EDITOR

        private static Dictionary<GameObject, List<MonoBehaviour>> _saveableMonos =
            new Dictionary<GameObject, List<MonoBehaviour>>();

        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            _saveableMonos = GetAllSaveMonos(true);
            EnsureSaveManagerExists();
            EnsureSaveableComponentsExist();
        }

        private static void EnsureSaveManagerExists()
        {
            if (Object.FindObjectsOfType<SaveManager>().Length > 0)
                return;

            int sum = _saveableMonos.Sum(pair => pair.Value.Count);
            if (sum > 0)
                Debug.LogWarning(
                    $"There {(sum == 1 ? "is" : "are")} {sum} MonoBehaviour{(sum != 1 ? "s" : "")} implementing the {nameof(SaveAttribute)}, but no {nameof(SaveManager)} exists in this Scene ({SceneManager.GetActiveScene().name}). Saving/Loading won't work!");
        }

        /// <summary>
        /// Ensures that the 'Saveable' component exists on any <see cref="MonoBehaviour" />s implementing the '[Save]' Attribute.
        /// Called every time Scripts are reloaded/recompiled.
        /// </summary>
        private static void EnsureSaveableComponentsExist()
        {
            foreach (KeyValuePair<GameObject, List<MonoBehaviour>> pair in _saveableMonos)
            {
                // Continue if the 'Saveable' component aöready exists on this object
                if (pair.Key.GetComponent<Saveable>())
                    continue;

                // Add the 'Saveable' component...
                Undo.AddComponent<Saveable>(pair.Key);

                // ... and log this change
                Debug.LogWarning(
                    $"One or more components in the ({pair.Key.name}) GameObject contain Fields or Properties with the '[{nameof(SaveAttribute)}]' Attribute: {string.Join(", ", pair.Value.Select(m => m.name))}. I have automagically added the '{nameof(Saveable)}' Component to these GameObjects for you.");
            }
        }

        /// <summary>
        /// Get all <see cref="MonoBehaviour" />s that implement the <see cref="SaveAttribute" />.
        /// </summary>
        /// <returns>
        /// A Dictionary mapping <see cref="GameObject" />s to a List of <see cref="MonoBehaviour" />s which implement the
        /// <see cref="SaveAttribute" />.
        /// </returns>
        private static Dictionary<GameObject, List<MonoBehaviour>> GetAllSaveMonos(bool includeSaveableObjects)
        {
            // Get all Transforms from the Root GameObjects in the Scene Hierarchy
            List<Transform> allTransforms =
                new List<Transform>(SceneManager.GetActiveScene().GetRootGameObjects().Select(go => go.transform));
            for (int i = allTransforms.Count; i-- > 0;)
            {
                // A reverse 'for' loop here ensures that adding items to the list has no effect on the loop
                Transform rootGO = allTransforms[i];
                allTransforms.AddRange(rootGO.GetChildren());
            }

            // Process every transform
            Dictionary<GameObject, List<MonoBehaviour>> applicableObjects =
                new Dictionary<GameObject, List<MonoBehaviour>>();
            foreach (Transform trans in allTransforms)
            {
                // Continue if this object already contains a 'Saveable' component
                Saveable saveableComp = trans.GetComponent<Saveable>();
                if (saveableComp && !includeSaveableObjects)
                    continue;

                // Deduce which components implement the '[Save]' Attribute and take a note of them
                foreach (MonoBehaviour comp in trans.GetComponents<MonoBehaviour>())
                {
                    // The Component in the Hierarchy may be a missing script (null), check this explicitly
                    if (comp == null)
                        continue;

                    Type type = comp.GetType();
                    bool hasSavedMembers = type.GetMembers(SaveManager.SaveableMemberFields)
                        .Any(m => Attribute.IsDefined(m, typeof(SaveAttribute)));

                    if (!hasSavedMembers)
                        continue;

                    if (!applicableObjects.ContainsKey(trans.gameObject))
                        applicableObjects.Add(trans.gameObject, new List<MonoBehaviour>());
                    applicableObjects[trans.gameObject].Add(comp);
                }
            }

            return applicableObjects;
        }

#endif
    }
}
