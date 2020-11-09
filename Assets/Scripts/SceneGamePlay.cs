using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


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
public class SceneGamePlay : MonoBehaviour
{
    // ** control the fillAmount of tower sprite
    public const float TOWER_MAX_FILLAMOUNT_SCORE = 100;
    public const float TOWER_MIN_FILLAMOUNT = 0.05f;
    // ** control the fillAmount of wall sprite
    public const float WALL_MAX_FILLAMOUNT_SCORE = 100;
    public const float WALL_MIN_FILLAMOUNT = 0.05f;

    public GamePlayer player1;
    public GamePlayer player2;
    // ** make sure FSM don't change twice in one frame
    private int m_frameCountGameStatusChanged = -1;
    private Action m_currentStatusBehaviour;
    private Dictionary<GameStatus, Action> m_statusBehaviours;
    public GameStatus currentGameStatus
    {
        get => t_currentGameStatus;
        set
        {
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

    public LuaManager lua = null;
    private static bool isInstantiated = false;
    public static SceneGamePlay instance = null;
    T InstantiateManager<T>(T g) where T : MonoBehaviour
    {
        var ig = Instantiate(g);
        ig.name = g.name;
        ig.transform.SetParent(transform);
        return ig;
    }
    void Awake()
    {
        if (!isInstantiated)
        {
            isInstantiated = true;
            SceneGamePlay.instance = this;
            DontDestroyOnLoad(this);
            lua = InstantiateManager<LuaManager>(lua);
            lua.Init();
        }

        var uiParent = GameObject.FindObjectOfType<Canvas>();
        // AssetUtility.InstantiatePrefab("Prefabs/UI/Fight", null, Vector3.zero, Quaternion.identity, uiParent.transform);
    }


    void Start()
    {
        m_statusBehaviours = new Dictionary<GameStatus, Action>(){
            {GameStatus.Start, GameStatusStart},
            {GameStatus.RoundBegin, GameStatusRoundBegin},
            {GameStatus.DrawCard, GameStatusDrawCard},
        };
        m_nextGameStatus = GameStatus.Start;
    }

    // Update is called once per frame
    void Update()
    {
        currentGameStatus = m_nextGameStatus;
        m_currentStatusBehaviour?.Invoke();
    }
    private void OnDestroy()
    {
        lua.Uninit();
    }
    void GameStatusStart()
    {
        player1.Init("player1");
        player2.Init("player2");
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
