using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardCostType
{
    BRICK,
    GEM,
    RECRUIT
};

public class GameCard : MonoBehaviour
{
    public CardCostType costType;
    public int cost;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
