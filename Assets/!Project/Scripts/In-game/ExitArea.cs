using UnityEngine;
using System.Collections;

public class ExitArea : MonoBehaviour
{
    [SerializeField] LayerMask _playersLayer;
    [SerializeField] Collider _collider;

    private void Awake()
    {
            
    }

    public void Init()
    {
        _collider.enabled = true;
    }

    void HandlePlayerEnter(Player player)
    {
        GameManager.Instance.SERVER_EndGame(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_playersLayer.Contains(other.gameObject.layer))
        {
            Player player = other.gameObject.GetComponent<Player>();
            if(player != null) HandlePlayerEnter(player);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        
    }
}