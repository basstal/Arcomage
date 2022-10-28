using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameScripts
{
    public class StartMenu : MonoBehaviour
    {
        public void OnReload()
        {
#if UNITY_EDITOR
            ClearLog();
#endif
            SceneManager.LoadSceneAsync("GamePlay");
        }
#if UNITY_EDITOR
        static void ClearLog()
        {
            var assembly = Assembly.GetAssembly(typeof(UnityEditor.ActiveEditorTracker));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method?.Invoke(new object(), null);
        }
#endif

        // public bool IsPlayerWin(GamePlayer target)
        // {
        //     return target.tower > 50 || FindEnemyById(target.playerID).tower <= 0;
        // }

        // public void IsGameEnded()
        // {
        //     var player1Win = IsPlayerWin(m_player1);
        //     var player2Win = IsPlayerWin(m_player2);
        //     if (player1Win || player2Win)
        //     {
        //         // m_gameEnd = player1Win && player2Win ? $"Peace End" : (player1Win ? $"player1Win" : "player2Win");
        //         m_isGameEnded = true;
        //         return;
        //     }
        //
        //     currentStage = DisplayHandCards;
        //     m_isGameEnded = false;
        // }
    }
}