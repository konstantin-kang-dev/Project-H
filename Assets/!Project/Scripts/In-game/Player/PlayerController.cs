using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Rigidbody _rb;
    CapsuleCollider _capsuleCollider;

    [SerializeField] PlayerStatsData _playerStatsData;
    PlayerStats _playerStats;

    [SerializeField] PlayerVisuals _playerVisualsPrefab;
    PlayerVisuals _playerVisuals;

    [SerializeField] PlayerMovementService _playerMovementServicePrefab;
    PlayerMovementService _playerMovementService;

    [SerializeField] CameraController _cameraControllerPrefab;
    CameraController _cameraController;
    private void Start()
    {
        Init();
    }

    public void Init()
    {
        _rb = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();

        _playerStats = new PlayerStats(_playerStatsData);

        _cameraController = Instantiate(_cameraControllerPrefab, transform);
        _cameraController.Init();

        _playerVisuals = Instantiate(_playerVisualsPrefab, transform);
        _playerVisuals.Init();
        _cameraController.OnLookPositionUpdate += _playerVisuals.AnimatorController.SetLookPosition;

        _playerMovementService = Instantiate(_playerMovementServicePrefab, transform);
        _playerMovementService.Init(_playerStats, _rb, _capsuleCollider);
        _playerMovementService.OnWalkStart += _playerVisuals.HandleWalk;
        _playerMovementService.OnWalkUpdate += _playerVisuals.HandleWalk;

        _cameraController.OnRotationUpdate += _playerMovementService.Rotate;
    }

}