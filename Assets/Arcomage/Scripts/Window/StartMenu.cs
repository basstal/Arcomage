using NOAH.UI;
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