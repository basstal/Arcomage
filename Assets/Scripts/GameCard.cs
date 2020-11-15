using UnityEngine;
using UnityEngine.UI;

public enum CostType
{
    None,
    Brick,
    Recruit,
    Gem,
}
public class GameCard : MonoBehaviour
{
    public int id = 0;
    public CostType costType = CostType.None;
    public int cost = 0;
}