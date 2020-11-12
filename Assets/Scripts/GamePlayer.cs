using UnityEngine;
using UnityEngine.UI;
using XLua;

[LuaCallCSharp]
public class GamePlayer : MonoBehaviour
{
    public int brick = 0;
    public int gem = 0;
    public int recruit = 0;
    public int brickIncRate = 1;
    public int gemIncRate = 1;
    public int recruitIncRate = 1;

    public int tower = 0;
    public int wall = 0;
    public string playerName = "Default";
    public int playerID;
    public bool useAI = false;
    // ** control the fillAmount of tower sprite
    public const float TOWER_MAX_FILL_AMOUNT_SCORE = 100;
    public const float TOWER_MIN_FILL_AMOUNT = 0.05f;
    // ** control the fillAmount of wall sprite
    public const float WALL_MAX_FILL_AMOUNT_SCORE = 100;
    public const float WALL_MIN_FILL_AMOUNT = 0.05f;

    public void Init(int playerID)
    {
        this.playerID = playerID; 
    }
}
