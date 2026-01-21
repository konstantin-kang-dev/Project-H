using UnityEngine;

[RequireComponent (typeof(Animator))]
public class IKAutoHeadRotator : MonoBehaviour
{
    [SerializeField] Transform _lookTarget;
    Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnAnimatorIK(int layerIndex)
    {
        _animator.SetLookAtPosition(_lookTarget.position);

        _animator.SetLookAtWeight(1f,
            0f,
            1f,
            0f,
            0.5f);
    }
}
