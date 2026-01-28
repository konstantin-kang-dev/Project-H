
using FishNet.Component.Animating;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class AnimatorController : SerializedMonoBehaviour
{
    public Animator Animator;

    [SerializeField] Dictionary<AnimatorState, string> _animatorStates = new Dictionary<AnimatorState, string>();

    Vector3 _worldLookPos = Vector3.zero;
    Vector3 _currentLookPos = Vector3.zero;

    [SerializeField] TwoBoneIKConstraint _handConstraint;
    [SerializeField] Transform _headTransform;
    [SerializeField] GameObject _headMeshGO;
    [SerializeField] Transform _headAimTarget;
    [SerializeField] Transform _handItemTarget;
    Transform _handItemPoint;

    public event Action<AnimatorState> OnAnimatorStateChanged;
    public event Action<Vector3> OnLookPositionUpdate;
    public AnimatorState CurrentState { get; private set; } = AnimatorState.None;

    public bool IsInitialized { get; private set; } = false;
    private void Awake()
    {
        _handItemPoint = _handItemTarget.Find("Point");
    }
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

    public void SetHeadVisibility(bool value)
    {
        if(_headMeshGO == null) return;

        for (int i = 0; i < _headMeshGO.transform.childCount; i++)
        {
            GameObject child = _headMeshGO.transform.GetChild(i).gameObject;
            child.layer = value ? LayerMask.NameToLayer("Default") : LayerMask.NameToLayer("MyHead");
        }
        _headMeshGO.layer = value ? LayerMask.NameToLayer("Default") : LayerMask.NameToLayer("MyHead");
    }

    public void SetLookPosition(Vector3 worldPos)
    {
        _worldLookPos = worldPos;

        OnLookPositionUpdate?.Invoke(_worldLookPos);
    }

    public void SetItemInHand(IPickable item, bool connect)
    {
        if (connect)
        {
            item.Transform.parent = _handItemPoint;
            item.Transform.position = _handItemPoint.position;
            item.Transform.rotation = _handItemPoint.rotation;

            _handConstraint.weight = 1;

            _handItemTarget.localPosition = item.ItemConfig.HandHoldingPosition;
            _handItemTarget.localEulerAngles = item.ItemConfig.HandHoldingRotation;
        }
        else
        {
            item.Transform.parent = null;

            _handConstraint.weight = 0;
        }

        //item.Transform.parent
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