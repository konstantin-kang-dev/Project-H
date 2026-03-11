using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    [SerializeField] ChildLayerSetter _modelChildLayerSetter;
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void SetModelXray(bool value)
    {
        if (value)
        {
            _modelChildLayerSetter.SetLayer("XRay");
        }
        else
        {
            _modelChildLayerSetter.ResetLayers();
        }
    }
}
