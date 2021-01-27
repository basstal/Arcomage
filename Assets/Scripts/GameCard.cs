using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XLua;

public enum CostType
{
    None,
    Brick,
    Recruit,
    Gem,
}
public class GameCard : MonoBehaviour
{
#if UNITY_EDITOR            
    [BlackList]
    public TextMeshProUGUI debug;
#endif
    private int m_id;
    [ShowInInspector]
    public int id
    {
        get => m_id;
        set
        {
            m_id = value;
#if UNITY_EDITOR            
            if (debug != null)
            {
                debug.text = value.ToString();
            }
#endif
        }
    }
    public CostType costType = CostType.None;
    public int cost = 0;
}