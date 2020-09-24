using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;
using System.IO;

public class GamePlayMain : MonoBehaviour
{
    private InitGame initGame;

    public int id;
    // Start is called before the first frame update
    void Start()
    {
        initGame = initGame ?? LoadInitGame(id);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    InitGame LoadInitGame(int id)
    {
        return new InitGame();
    }
}
