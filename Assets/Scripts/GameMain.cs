using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using XLua;
using Object = UnityEngine.Object;

[LuaCallCSharp]
public class GameMain : MonoBehaviour
{
    public int round = 0;
    public int firstPlayer = 1;
    private const int CARD_CACHE_SIZE = 20;
    [SerializeField] private AssetReference player1AssetRef;
    [SerializeField] private AssetReference player2AssetRef;
    [SerializeField] private AssetReference cardAssetRef;
    [SerializeField] private AssetReference brickTextureAssetRef;
    [SerializeField] private AssetReference gemTextureAssetRef;
    [SerializeField] private AssetReference recruitTextureAssetRef;
    private GameObject m_player1Obj;
    private GameObject m_player2Obj;
    public Transform CardObjCacheRoot { get; private set; }
    public Dictionary<int, Sprite> ID2Sprite { get; private set; }
    public async void Init(Action callback)
    {
        round = 0;
        if (m_player1Obj == null)
        {
            m_player1Obj = await player1AssetRef.InstantiateAsync(transform).Task;
        }
        m_player1Obj.GetComponent<GamePlayer>().Init(1);

        if (m_player2Obj == null)
        {
            m_player2Obj = await player2AssetRef.InstantiateAsync(transform).Task;
        }
        m_player2Obj.GetComponent<GamePlayer>().Init(2);

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
        for (var i = 0; i < CardObjCacheRoot.childCount; ++i)
        {
            CardObjCacheRoot.GetChild(i).GetComponent<GameCard>().Init();
        }

        if (ID2Sprite == null)
        {
            ID2Sprite = new Dictionary<int, Sprite>();

            async Task LoadSprites(AssetReference assetRef, int addID)
            {
                var loadedTex = await assetRef.LoadAssetAsync<IList<Sprite>>().Task;
                // ** TODO make real ID
                foreach (var sprite in loadedTex)
                {
                    var splitResult = sprite.name.Split('_');
                    int.TryParse(splitResult[1], out var id);
                    if (id == 34)
                        continue;
                    Debug.Log($" add ID : {id + addID}");
                    ID2Sprite.Add(id + addID, sprite);
                }
            }

            await LoadSprites(brickTextureAssetRef, 0);
            await LoadSprites(gemTextureAssetRef, 34);
            await LoadSprites(recruitTextureAssetRef, 68);
        }
        callback.SafeInvoke();
    }
}