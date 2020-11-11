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
    public string playerName = null;
    public bool useAI = false;
    // ** control the fillAmount of tower sprite
    public const float TOWER_MAX_FILLAMOUNT_SCORE = 100;
    public const float TOWER_MIN_FILLAMOUNT = 0.05f;
    // ** control the fillAmount of wall sprite
    public const float WALL_MAX_FILLAMOUNT_SCORE = 100;
    public const float WALL_MIN_FILLAMOUNT = 0.05f;

    public void Init()
    {
        brick = 0;
        gem = 0;
        recruit = 0;
        brickIncRate = 1;
        gemIncRate = 1;
        recruitIncRate = 1;
        tower = 0;
        wall = 0;
        useAI = false;
        if (string.IsNullOrWhiteSpace(playerName))
            playerName = "Default";
            
    }
}
