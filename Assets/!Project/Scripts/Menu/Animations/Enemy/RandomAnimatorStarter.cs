using Cysharp.Threading.Tasks;
using UnityEngine;

[RequireComponent (typeof(Animator))]
public class AnimatorRandomizer : MonoBehaviour
{
    Animator _animator;
    [SerializeField] Vector2 _startDelayRange = Vector2.one;
    [SerializeField] Vector2 _speedRange = Vector2.one;
    private void Start()
    {
        DelayedStart();
    }

    async void DelayedStart()
    {
        await UniTask.WaitForFixedUpdate();
        await UniTask.WaitForFixedUpdate();

        _animator = GetComponent<Animator>();
        _animator.enabled = false;

        float delay = Random.Range(_startDelayRange.x, _startDelayRange.y);
        float speed = Random.Range(_speedRange.x, _speedRange.y);
        _animator.speed = speed;

        await UniTask.WaitForSeconds(delay);

        _animator.enabled = true;
    }


    void Update()
    {
        
    }
}
