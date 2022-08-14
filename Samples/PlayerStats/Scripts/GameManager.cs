using UnityEngine;

namespace Medallyon
{
    public class GameManager : MonoBehaviour
    {
        private void Awake()
        {
            // Restore all saved data before 'Start'.
            SaveManager.LoadAll();
        }

        private void OnApplicationQuit()
        {
            // Save all data before quitting the game.
            SaveManager.SaveAll();
        }
    }
}
