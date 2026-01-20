using UnityEditor;
using UnityEngine;

public class EnemyVisuals : MonoBehaviour
{
    [SerializeField] EnemyModel _enemyModelPrefab;
    EnemyModel _enemyModel;

    public void Init()
    {
        _enemyModel = Instantiate(_enemyModelPrefab, transform);
    }
}