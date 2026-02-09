using DG.Tweening;
using UnityEngine;

public class InteractableShelfVisuals : MonoBehaviour
{
    [SerializeField] MeshRenderer _meshRenderer;
    [SerializeField] float _animationDuration = 0.5f;

    float _meshLengthZ;
    Vector3 _initialLocalPos;

    private void Awake()
    {
        _initialLocalPos = transform.localPosition;
        _meshLengthZ = _meshRenderer.bounds.size.z * 1.2f;
    }
    public void HandleStateChange(bool value)
    {
        if (value)
        {
            Open();
        }
        else
        {
            Close();
        }
    }

    void Open()
    {
        transform.DOLocalMoveZ(_meshLengthZ, _animationDuration);
    }

    void Close()
    {
        transform.DOLocalMoveZ(_initialLocalPos.z, _animationDuration);
    }
}
