using UnityEngine;

public enum CostType
{
    None,
    Brick,
    Recruit,
    Gems,
}
public class GameCard : MonoBehaviour
{
    public int id = 0;
    public string cardName = null;
    public CostType costType = CostType.None;
    public int cost = 0;

    public void Init()
    {
        id = 0;
        if (string.IsNullOrWhiteSpace(cardName))
            cardName = "Default";
        costType = CostType.None;
        cost = 0;
    }
}