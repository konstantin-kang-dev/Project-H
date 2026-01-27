using UnityEngine;

public class GameCanvas : MonoBehaviour
{
    public static GameCanvas Instance { get; private set; }
    Canvas _canvas;
    void Awake()
    {
        _canvas = GetComponent<Canvas>();

        Instance = this;
    }

    public void SetCamera(Camera camera)
    {
        _canvas.worldCamera = camera;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
