using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Medallyon
{
    public static class Util
    {
        /// <summary>
        /// Get all child <see cref="Transform" />s of this parent.;
        /// </summary>
        /// <param name="transform"></param>
        /// <returns>A list containing every child <see cref="Transform" />.</returns>
        public static IEnumerable<Transform> GetChildren(this Transform transform)
        {
            List<Transform> children = new List<Transform>();
            for (int i = transform.childCount; i-- > 0;)
            {
                Transform child = transform.GetChild(i);
                children.Add(child);

                if (child.childCount > 0)
                    children.AddRange(GetChildren(child));
            }

            return children;
        }
        
        /// <summary>
        /// Get the value of <paramref name="memberInfo" /> for the specified <paramref name="forObject" />. Note that this only works for
        /// <see cref="PropertyInfo" /> and <see cref="FieldInfo" /> types.
        /// </summary>
        /// <source>https://stackoverflow.com/a/33446914/4672263</source>
        /// <param name="memberInfo"></param>
        /// <param name="forObject"></param>
        /// <returns></returns>
        public static object GetValue(this MemberInfo memberInfo, object forObject)
        {
            return memberInfo.MemberType switch
            {
                MemberTypes.Field => ((FieldInfo)memberInfo).GetValue(forObject),
                MemberTypes.Property => ((PropertyInfo)memberInfo).GetValue(forObject),
                _ => throw new NotImplementedException()
            };
        }

        /// <summary>
        /// Set the value of <paramref name="memberInfo" /> for the specified <paramref name="forObject" /> to
        /// <paramref name="value" />. Note that this only works for <see cref="PropertyInfo" /> and <see cref="FieldInfo" />
        /// types.
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <param name="forObject"></param>
        /// <param name="value"></param>
        public static void SetValue(this MemberInfo memberInfo, object forObject, object value)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    ((FieldInfo)memberInfo).SetValue(forObject, value);
                    break;

                case MemberTypes.Property:
                    ((PropertyInfo)memberInfo).SetValue(forObject, value);
                    break;

                default: throw new NotImplementedException();
            }
        }
        
        /// <source>https://stackoverflow.com/a/16043551/4672263</source>
        /// <param name="member"></param>
        /// <returns></returns>
        public static Type GetMemberType(this MemberInfo member)
        {
            return member.MemberType switch
            {
                MemberTypes.Event => ((EventInfo)member).EventHandlerType,
                MemberTypes.Field => ((FieldInfo)member).FieldType,
                MemberTypes.Method => ((MethodInfo)member).ReturnType,
                MemberTypes.Property => ((PropertyInfo)member).PropertyType,
                _ => throw new ArgumentException(
                    "Input MemberInfo must be of type EventInfo, FieldInfo, MethodInfo, or PropertyInfo")
            };
        }
    }
}