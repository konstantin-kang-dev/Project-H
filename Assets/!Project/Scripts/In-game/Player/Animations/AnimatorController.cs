
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AnimatorController : SerializedMonoBehaviour
{
    Animator _animator;

    [SerializeField] Dictionary<AnimatorState, string> _animatorStates = new Dictionary<AnimatorState, string>();

    Vector3 _worldLookPos = Vector3.zero;
    Vector3 _currentLookPos = Vector3.zero;

    public event Action<Vector3> OnLookPositionUpdate;
    public AnimatorState CurrentState { get; private set; } = AnimatorState.None;

    public bool IsInitialized { get; private set; } = false;
    public void Init()
    {
        _animator = GetComponent<Animator>();

        IsInitialized = true;
        PlayAnimation(AnimatorState.Idle);
    }

    private void Update()
    {
        if (!IsInitialized) return;

        _currentLookPos = Vector3.MoveTowards(_currentLookPos, _worldLookPos, 15f * Time.deltaTime);
    }

    public void PlayAnimation(AnimatorState state)
    {
        if (!IsInitialized) return;

        if (!_animatorStates.ContainsKey(state)) throw new System.Exception($"[ScriptedAnimatorController] State is not referenced! {state}");
        
        if(state == CurrentState) return;

        CurrentState = state;
        string targetAnimationKey = _animatorStates[state];

        _animator.CrossFadeInFixedTime(targetAnimationKey, 0.25f);
    }

    public void SetFloat(string key, float value)
    {
        _animator.SetFloat(key, value);
    }

    public void HandleWalk(Vector2 inputs)
    {
        SetFloat("MoveX", inputs.x);
        SetFloat("MoveY", inputs.y);

        if(inputs.x == 0f && inputs.y == 0f)
        {
            PlayAnimation(AnimatorState.Idle);
        }
        else
        {
            PlayAnimation(AnimatorState.Walk);
        }
    }

    public void SetLookPosition(Vector3 worldPos)
    {
        _worldLookPos = worldPos;

        OnLookPositionUpdate?.Invoke(_worldLookPos);
    }

    void OnAnimatorIK(int layerIndex)
    {
        if(_animator == null) return;

        _animator.SetLookAtWeight(
            1f,
            0f,
            0.8f,
            0f,
            0.5f
        );

        _animator.SetLookAtPosition(_currentLookPos);
    }

    private void OnDrawGizmos()
    {
        if (!IsInitialized) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(_worldLookPos, 0.2f);

        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, _worldLookPos);
    }
}