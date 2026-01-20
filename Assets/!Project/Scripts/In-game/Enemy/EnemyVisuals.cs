using UnityEditor;
using UnityEngine;

public class EnemyVisuals : MonoBehaviour
{
    [SerializeField] EnemyModel _enemyModelPrefab;
    EnemyModel _enemyModel;

    public AnimatorController AnimatorController;

    float _headRotationTimer = 0f;
    float _headRotationInterval = 2f;

    public void Init()
    {
        _enemyModel = Instantiate(_enemyModelPrefab, transform);
        AnimatorController = _enemyModel.GetComponent<AnimatorController>();
        AnimatorController.Init();
    }

    public void HandleEnemyMove(bool isFollowingPlayer, Transform target)
    {
        if(isFollowingPlayer)
        {
            AnimatorController.PlayAnimation(AnimatorState.Run);

            Vector3 lookPos = target.position;
            lookPos.y += 1f;
            AnimatorController.SetLookPosition(target.position);
        }
        else
        {
            AnimatorController.PlayAnimation(AnimatorState.Walk);

            Vector3 lookPos = target.position;
            lookPos.y += 1f;
        }
    }

    public void HandleStateUpdate(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Following:
                _headRotationTimer = 0f;
                break;
            default:
                _headRotationTimer += Time.fixedDeltaTime;
                break;
        }

        if(_headRotationTimer >= _headRotationInterval)
        {
            RotateHeadRandomly();
        }
    }

    void RotateHeadRandomly()
    {
        _headRotationTimer = 0f;

        Vector3 randomPosInCone = ProjectUtils.RandomPositionInRectangle(transform.position, transform.forward, 7f, 8f, 0.5f);
        AnimatorController.SetLookPosition(randomPosInCone);
    }
}