using System.Collections;
using NOAH.UI;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Whiterice;

namespace Arcomage.Scripts.UI.Runtime.Window
{
    public class StartMenu : UIWindow
    {
        public void StartCombat()
        {
            AssetManager.Instance.LoadSceneAsyncTask("GamePlay");
        }
    }
}