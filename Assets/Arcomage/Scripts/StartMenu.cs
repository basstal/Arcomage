using NOAH.UI;
using UnityEngine;
using Whiterice;

namespace Arcomage.Scripts.UI.Runtime.Window
{
    public class StartMenu : MonoBehaviour
    {
        public void StartCombat()
        {
            AssetManager.Instance.LoadSceneAsyncTask("GamePlay");
        }

        public void Quit()
        {
            Application.Quit();
        }

        public void BackToStartMenu()
        {
            AssetManager.Instance.LoadSceneAsyncTask("StartMenu");
        }
    }
}