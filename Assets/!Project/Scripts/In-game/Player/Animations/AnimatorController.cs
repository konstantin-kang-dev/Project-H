
using FishNet.Component.Animating;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AnimatorController : SerializedMonoBehaviour
{
    public Animator Animator;

    [SerializeField] Dictionary<AnimatorState, string> _animatorStates = new Dictionary<AnimatorState, string>();

    Vector3 _worldLookPos = Vector3.zero;
    Vector3 _currentLookPos = Vector3.zero;
    [SerializeField] Transform _headAimTarget;

    public event Action<AnimatorState> OnAnimatorStateChanged;
    public event Action<Vector3> OnLookPositionUpdate;
    public AnimatorState CurrentState { get; private set; } = AnimatorState.None;

    public bool IsInitialized { get; private set; } = false;
    public void Init()
    {
        Animator = GetComponent<Animator>();

        IsInitialized = true;
        PlayAnimation(AnimatorState.Idle);
    }

    private void Update()
    {
        if (!IsInitialized) return;

        _currentLookPos = Vector3.Lerp(_currentLookPos, _worldLookPos, 10f * Time.deltaTime);

        _headAimTarget.transform.position = _currentLookPos;
    }

    public void PlayAnimation(AnimatorState state)
    {
        if (!IsInitialized) return;

        if (!_animatorStates.ContainsKey(state)) throw new System.Exception($"[ScriptedAnimatorController] State is not referenced! {state}");
        
        if(state == CurrentState) return;

        CurrentState = state;
        string targetAnimationKey = _animatorStates[state];

        OnAnimatorStateChanged?.Invoke(CurrentState);
        Animator.CrossFadeInFixedTime(targetAnimationKey, 0.25f, 0);

    }

    public void SetFloat(string key, float value)
    {
        Animator.SetFloat(key, value);
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

    public void ResetController()
    {
        OnAnimatorStateChanged = null;
        CurrentState = AnimatorState.None;
        IsInitialized = false;
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