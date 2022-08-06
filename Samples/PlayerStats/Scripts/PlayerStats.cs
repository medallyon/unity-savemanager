using System;
using System.Collections.Generic;
using UnityEngine;

namespace Medallyon
{
    // A simple example of saving & restoring a Guid
    public class PlayerStats : MonoBehaviour, ISaveable
    {
        [Save] private Guid _playerID = Guid.NewGuid();

        // OnFirstLoad is called the first ever time the game is started.
        public void OnFirstLoad()
        {
            // This is for illustration purposes only.
            _playerID = Guid.NewGuid();
        }

        // OnRestore is called when variables for this component are loaded and passed through.
        public void OnRestore(Dictionary<string, object> data)
        {
            _playerID = Guid.Parse((string)data[nameof(_playerID)]);
        }

        private void Start()
        {
            Debug.Log($"_playerID = {_playerID}");
        }
    }
}
