using System.Collections.Generic;
using UnityEngine;

namespace Medallyon
{
    // A simple example of saving & restoring a Guid
    public class PlayerStats : MonoBehaviour, ISaveable
    {
        [Save] public int PlayerId;
        [Save] public string PlayerName;
        [Save] private float Experience { get; set; }

        private void Start()
        {
            Debug.Log($"PlayerId = {PlayerId}, PlayerName = {PlayerName}, Experience = {Experience}");
        }

        // OnFirstLoad is called the first ever time the game is started.
        public void OnFirstLoad()
        {
            // Initialize any variables for the very first time here or simply as part of your constructor.
            PlayerId = 42;
            PlayerName = "Rufus";
            Experience = Random.Range(0f, 1000f);
        }

        // OnRestore is called when variables for this component are loaded and passed through.
        public void OnRestore(Dictionary<string, object> data)
        {
            // Remember: 'data' is a Dictionary mapping the variable name to the last saved value of that variable.

            // Restoring primitives like this is usually not needed - but it's here for example.
            PlayerId = (int)data[nameof(PlayerId)];
            PlayerName = (string)data[nameof(PlayerName)];
            Experience = (float)data[nameof(Experience)];
        }
    }
}
