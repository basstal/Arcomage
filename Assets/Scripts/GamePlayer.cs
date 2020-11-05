using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayer : MonoBehaviour
{
    public int brick;
    public int gem;
    public int recruit;
    public int brickIncRate;
    public int gemIncRate;
    public int recruitIncRate;

    public int tower;
    public int wall;
    public TextMeshProUGUI brickTMP;
    public TextMeshProUGUI gemTMP;
    public TextMeshProUGUI recruitTMP;
    public TextMeshProUGUI playerNameTMP;
    public TextMeshProUGUI towerScoreTMP;
    public TextMeshProUGUI wallScoreTMP;
    public RectTransform towerRoof;
    public Image towerBody;
    public RectTransform wallRoof;
    public Image wallBody;
    public GameObject cardBoard;
    
    private void RepositionRoof(RectTransform roof, Image body)
    {
        var position = roof.localPosition;
        var parentImage = roof.parent.GetComponent<Image>();
        position.y = parentImage.preferredHeight / 2 + body.preferredHeight * body.fillAmount;
        roof.localPosition = position;
    }

    public void Init(string playerName)
    {
        brickTMP.text = $"{brick} <size=80%>bricks";
        gemTMP.text = $"{gem} <size=80%>gems";
        recruitTMP.text = $"{recruit} <size=80%>recurits";
        towerScoreTMP.text = $"{tower}";
        wallScoreTMP.text = $"{wall}";
        playerNameTMP.text = playerName;

        var hasTower = tower > 0;
        towerBody.fillAmount = hasTower ? Mathf.Max(tower / SceneGamePlay.TOWER_MAX_FILLAMOUNT_SCORE, SceneGamePlay.TOWER_MIN_FILLAMOUNT) : 0;
        towerRoof.gameObject.SetActive(hasTower);
        if (hasTower)
        {
            RepositionRoof(towerRoof, towerBody);
        }
        

        var hasWall = wall > 0;
        wallBody.fillAmount = hasWall ? Mathf.Max(wall / SceneGamePlay.WALL_MAX_FILLAMOUNT_SCORE, SceneGamePlay.WALL_MIN_FILLAMOUNT) : 0;
        wallRoof.gameObject.SetActive(hasWall);
        if (hasWall)
        {
            RepositionRoof(wallRoof, wallBody);
        }
    }
    public void DrawCard(int count)
    {
        // Sprite defaultCardImage = GetSprite("Assets/Sprites/others.png", "others_8");
        for (int i = 0; i < count; ++i)
        {
            // ** may cause gc alloc
            // LuaTable card = genCardDatum();
            // card.Get("note", out string note);
            // card.Get("id", out int id);
            // id -= 7000;
            // id %= 34;
            // var cardPrefab = cardBoard.transform.GetChild(0).transform.GetChild(i);
            // var cardPrefabImage = cardPrefab.GetComponent<Image>();
            // var targetImage = GetSprite("Assets/Sprites/bricks.png", $"bricks_{id}");
            // cardPrefabImage.sprite = targetImage;

        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
