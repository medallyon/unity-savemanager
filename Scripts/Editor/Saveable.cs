using System;
using UnityEngine;

namespace Medallyon
{
    [DisallowMultipleComponent]
    public class Saveable : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Do not modify this value.")]
        // This needs to be serialized because otherwise it's re-generated every session
        internal string ID = Guid.NewGuid().ToString();

        internal void RefreshID()
        {
            ID = Guid.NewGuid().ToString();
        }
    }
}
