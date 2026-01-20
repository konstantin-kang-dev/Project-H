using UnityEngine;

public class Player : MonoBehaviour
{
    public bool IsInvincible = false;
    public bool IsInitialized { get; private set; } = false;
    void Start()
    {
        Init();
    }

    public void Init()
    {
        GameManager.Instance.AddPlayer(this);

        IsInitialized = true;
    }

    void Update()
    {
        
    }
}
