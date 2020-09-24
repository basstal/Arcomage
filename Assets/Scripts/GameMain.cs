using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    // Start is called before the first frame update
    void Start()
    {
        player1.Init("player1");
        player2.Init("player2");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
