using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using XLua;

[LuaCallCSharp]
public class GameMain : MonoBehaviour
{
    public int round = 0;
    private const int CardCacheSize = 20;
    [SerializeField] private AssetReference player1AssetRef;
    [SerializeField] private AssetReference player2AssetRef;
    [SerializeField] private AssetReference cardAssetRef;
    private GameObject m_player1Obj;
    private GameObject m_player2Obj;
    public GameObject[] cardCache { get; private set; }
    public async void Init(Action callback)
    {
        round = 0;
        if (m_player1Obj == null)
        {
            m_player1Obj = await player1AssetRef.InstantiateAsync(transform).Task;
        }
        m_player1Obj.GetComponent<GamePlayer>().Init();

        if (m_player2Obj == null)
        {
            m_player2Obj = await player2AssetRef.InstantiateAsync(transform).Task;
        }
        m_player2Obj.GetComponent<GamePlayer>().Init();

        if (cardCache == null)
        {
            cardCache = new GameObject[CardCacheSize];
            var cardTemplate = await cardAssetRef.LoadAssetAsync<GameObject>().Task;
            var cardsObjCache = transform.Find("CardsObjCache");
            if (cardsObjCache == null)
            {
                cardsObjCache = new GameObject("CardsObjCache").transform;
                cardsObjCache.parent = transform;
            }
            for(int i = 0; i < CardCacheSize; ++i)
            {
                cardCache[i] = Instantiate(cardTemplate, cardsObjCache);
            };
        }
        foreach (var card in cardCache)
        {
            card.GetComponent<GameCard>().Init();
        }
        callback.SafeInvoke();
    }
}

// public class SceneGamePlay : MonoBehaviour
// {


//     public GamePlayer player1;
//     public GamePlayer player2;
//     // ** make sure FSM don't change twice in one frame
//     private int m_frameCountGameStatusChanged = -1;
//     private Action m_currentStatusBehaviour;
//     private Dictionary<GameStatus, Action> m_statusBehaviours;
//     public GameStatus currentGameStatus
//     {
//         get => t_currentGameStatus;
//         set
//         {
//             if (m_frameCountGameStatusChanged != Time.frameCount)
//             {
//                 t_currentGameStatus = value;
//                 m_statusBehaviours.TryGetValue(value, out m_currentStatusBehaviour);
//                 m_frameCountGameStatusChanged = Time.frameCount;
//             }
//             else
//             {
//                 Debug.LogWarning("Try to change GameStatus twice in one frame. Please check game logic.");
//             }
//         }
//     }
//     private GameStatus m_nextGameStatus;
//     private GameStatus t_currentGameStatus;
//     private GamePlayer m_currentPlayer;

//     public LuaManager lua = null;
//     private static bool isInstantiated = false;
//     public static SceneGamePlay instance = null;
//     T InstantiateManager<T>(T g) where T : MonoBehaviour
//     {
//         var ig = Instantiate(g);
//         ig.name = g.name;
//         ig.transform.SetParent(transform);
//         return ig;
//     }
//     void Awake()
//     {
//         if (!isInstantiated)
//         {
//             isInstantiated = true;
//             SceneGamePlay.instance = this;
//             DontDestroyOnLoad(this);
//             lua = InstantiateManager<LuaManager>(lua);
//             lua.Init();
//         }

//         var uiParent = GameObject.FindObjectOfType<Canvas>();
//         // AssetUtility.InstantiatePrefab("Prefabs/UI/Fight", null, Vector3.zero, Quaternion.identity, uiParent.transform);
//     }


//     void Start()
//     {
//         m_statusBehaviours = new Dictionary<GameStatus, Action>(){
//             {GameStatus.Start, GameStatusStart},
//             {GameStatus.RoundBegin, GameStatusRoundBegin},
//             {GameStatus.DrawCard, GameStatusDrawCard},
//         };
//         m_nextGameStatus = GameStatus.Start;
//     }

//     // Update is called once per frame
//     void Update()
//     {
//         currentGameStatus = m_nextGameStatus;
//         m_currentStatusBehaviour?.Invoke();
//     }
//     private void OnDestroy()
//     {
//         lua.Uninit();
//     }
//     void GameStatusStart()
//     {
//         player1.Init("player1");
//         player2.Init("player2");
//         var r = UnityEngine.Random.Range(0, 1.0f);
//         m_currentPlayer = r < 0.5f ? player1 : player2;
//         Debug.Log($"GameStatus.Start.\nCurrentPlayer : {m_currentPlayer}");
//         m_nextGameStatus = GameStatus.RoundBegin;
//     }
//     void GameStatusRoundBegin()
//     {
//         Debug.Log($"GameStatus.RoundBegin");
//         m_nextGameStatus = GameStatus.DrawCard;
//     }
//     void GameStatusDrawCard()
//     {
//         Debug.Log($"GameStatus.DrawCard");
//         // m_currentPlayer.DrawCard(5);
//     }
// }
