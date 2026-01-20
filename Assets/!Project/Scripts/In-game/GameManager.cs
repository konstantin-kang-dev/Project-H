using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {  get; private set; }

    List<Player> _players = new List<Player>();
    public List<Player> Players => _players;

    public bool IsInitialized { get; private set; } = false;
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        Init();
    }

    public void Init()
    {

    }

    public void AddPlayer(Player player)
    {
        _players.Add(player);
    }

    void Update()
    {

    }
}