using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using XLua;

[LuaCallCSharp]
public class GameMain : MonoBehaviour
{
    public int round = 0;
    public int firstPlayer = 1;
    private const int CARD_CACHE_SIZE = 20;
#pragma warning disable 0649
    [SerializeField] private AssetReference player1AssetRef;
    [SerializeField] private AssetReference player2AssetRef;
    [SerializeField] private AssetReference cardAssetRef;
    [SerializeField] private AssetReference cardsAssetRef;
    [SerializeField] private AssetReference brickAssetRef;
    [SerializeField] private AssetReference gemAssetRef;
    [SerializeField] private AssetReference recruitAssetRef;
#pragma warning restore 0649    
    private GameObject m_player1Obj;
    private GameObject m_player2Obj;
    public Transform CardObjCacheRoot { get; private set; }
    public Dictionary<int, Sprite> ID2Sprite { get; private set; }
    
    [HideInInspector]public Sprite brick;
    [HideInInspector]public Sprite gem;
    [HideInInspector]public Sprite recruit;
    public async void Init(Action callback)
    {
        round = 0;
        if (m_player1Obj == null)
        {
            m_player1Obj = await player1AssetRef.InstantiateAsync(transform).Task;
        }

        if (m_player2Obj == null)
        {
            m_player2Obj = await player2AssetRef.InstantiateAsync(transform).Task;
        }

        if (CardObjCacheRoot == null)
        {
            CardObjCacheRoot = transform.Find("CardObjCacheRoot");
            if (CardObjCacheRoot == null)
            {
                CardObjCacheRoot = new GameObject("CardObjCacheRoot").transform;
                CardObjCacheRoot.gameObject.SetActive(false);
                CardObjCacheRoot.parent = transform;
            }
            var cardTemplate = await cardAssetRef.LoadAssetAsync<GameObject>().Task;
            for(var i = 0; i < CARD_CACHE_SIZE; ++i)
            {
                Instantiate(cardTemplate, CardObjCacheRoot);
            }
        }

        if (ID2Sprite == null)
        {
            ID2Sprite = new Dictionary<int, Sprite>();

            var loadedTex = await cardsAssetRef.LoadAssetAsync<IList<Sprite>>().Task;
            foreach (var sprite in loadedTex)
            {
                var splitResult = sprite.name.Split('_');
                int.TryParse(splitResult[1], out var id);
                ID2Sprite.Add(id + 1, sprite);
            }
        }

        if (brick == null)
        {
            brick = await brickAssetRef.LoadAssetAsync<Sprite>().Task;
        }
        if (gem == null)
        {
            gem = await gemAssetRef.LoadAssetAsync<Sprite>().Task;
        }
        if (recruit == null)
        {
            recruit = await recruitAssetRef.LoadAssetAsync<Sprite>().Task;
        }
        callback.SafeInvoke();
    }
}