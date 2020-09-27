using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XLua;


public enum GameStatus
{
    None,
    Start,
    RoundBegin,
    DrawCard,
    WaitPlayer,
    RoundEnd,
    TurnRound,
    TurnPlayer,
    Finish,
}
public class GameMain : MonoBehaviour
{
    // ** control the fillAmount of tower sprite
    public const float TOWER_MAX_FILLAMOUNT_SCORE = 100;
    public const float TOWER_MIN_FILLAMOUNT = 0.05f;
    // ** control the fillAmount of wall sprite
    public const float WALL_MAX_FILLAMOUNT_SCORE = 100;
    public const float WALL_MIN_FILLAMOUNT = 0.05f;

    public GamePlayer player1;
    public GamePlayer player2;
    public LuaEnv luaEnv 
    { 
        get => t_luaEnv; 
        set => t_luaEnv = value;
    }
    private LuaEnv t_luaEnv;
    private string m_luaPath;
    // ** make sure FSM don't change twice in one frame
    private int m_frameCountGameStatusChanged = -1;
    private Action m_currentStatusBehaviour;
    private Dictionary<GameStatus, Action> m_statusBehaviours;
    public GameStatus currentGameStatus
    {
        get => t_currentGameStatus;
        set {
            if (m_frameCountGameStatusChanged != Time.frameCount)
            {
                t_currentGameStatus = value;
                m_statusBehaviours.TryGetValue(value, out m_currentStatusBehaviour);
                m_frameCountGameStatusChanged = Time.frameCount;
            }
            else
            {
                Debug.LogWarning("Try to change GameStatus twice in one frame. Please check game logic.");
            }
        }
    }
    private GameStatus m_nextGameStatus;
    private GameStatus t_currentGameStatus;
    private GamePlayer m_currentPlayer;
    
    // Start is called before the first frame update
    void Start()
    {
        m_luaPath = Application.dataPath + "/Scripts/Lua/";
        luaEnv = new LuaEnv();
        luaEnv.AddLoader((ref string filePath) => {
            var path = $"{m_luaPath}{filePath}.bytes";
            if(File.Exists(path))
            {
                return File.ReadAllBytes(path);
            }
            else
            {
                Debug.LogWarning($"LuaEnv loader {path} file not found!");
                return null;
            }
        });
        luaEnv.DoString("require(\"Init\")");
        m_statusBehaviours = new Dictionary<GameStatus, Action>(){
            {GameStatus.Start, GameStatusStart},
            {GameStatus.RoundBegin, GameStatusRoundBegin},
            {GameStatus.DrawCard, GameStatusDrawCard},
        };
        m_nextGameStatus = GameStatus.Start;
    }

    private void OnDestroy() {
        player1.genCardDatum = null;
        player2.genCardDatum = null;
        luaEnv.Dispose();
        luaEnv = null;
    }

    // Update is called once per frame
    void Update()
    {
        currentGameStatus = m_nextGameStatus;
        m_currentStatusBehaviour?.Invoke();
    }
    void GameStatusStart()
    {
        player1.Init("player1", luaEnv);
        player2.Init("player2", luaEnv);
        var r = UnityEngine.Random.Range(0, 1.0f);
        m_currentPlayer = r < 0.5f ? player1 : player2;
        Debug.Log($"GameStatus.Start.\nCurrentPlayer : {m_currentPlayer}");
        m_nextGameStatus = GameStatus.RoundBegin;
    }
    void GameStatusRoundBegin()
    {
        Debug.Log($"GameStatus.RoundBegin");
        m_nextGameStatus = GameStatus.DrawCard;
    }
    void GameStatusDrawCard()
    {
        Debug.Log($"GameStatus.DrawCard");
        m_currentPlayer.DrawCard(5);
    }
}
