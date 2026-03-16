using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AggroController : MonoBehaviour
{
    [SerializeField] LayerMask _visibilityCheckLayer;
    EnemyStatsConfig _enemyStats;

    Player _aggroPlayer;

    float _aggroTimer = 0f;
    Vector3 _directionOfSight = Vector3.zero;
    float _sightMaxAngleVisibility = 30f;

    public event Action<Player> OnAggroProceed;
    public event Action OnAggroRelease;
    public bool IsInitialized { get; private set; } = false;

    public void Init(EnemyStatsConfig enemyStats)
    {
        _enemyStats = enemyStats;

        IsInitialized = true;
    }

    private void FixedUpdate()
    {
        if (!IsInitialized) return;

        if(_aggroPlayer == null)
        {
            CheckForAggro();
        }
        else
        {
            CheckForAggroRelease();
        }

        if (_aggroPlayer != null)
        {
            _aggroTimer += Time.fixedDeltaTime;
        }
    }

    int GetAggroPointsFromDistance(float distance)
    {
        if (distance > ProjectConstants.ENEMY_MAXIMUM_AGGRO_DISTANCE) return 0;

        float howClose = (ProjectConstants.ENEMY_MAXIMUM_AGGRO_DISTANCE - distance) / ProjectConstants.ENEMY_DISTANCE_FOR_AGGRO_POINT;

        return Mathf.RoundToInt(howClose);
    }

    bool IsVisibleForMe(Vector3 pos)
    {
        Vector3 startPos = transform.position;
        startPos.y += 1.2f;
        pos.y += 1f;

        Vector3 directionToTarget = (pos - startPos).normalized;
        float dot = Vector3.Dot(_directionOfSight.normalized, directionToTarget);

        if (dot >= Mathf.Cos(_sightMaxAngleVisibility * Mathf.Deg2Rad))
        {
            if (Physics.Linecast(startPos, pos, out RaycastHit hit, _visibilityCheckLayer))
            {
                Debug.DrawLine(startPos, pos, Color.red);
                return false;
            }
            else
            {
                Debug.DrawLine(startPos, pos, Color.green);
                return true;
            }
        }

        Debug.DrawLine(startPos, pos, Color.red);
        return false;
    }

    public void UpdateSightDirection(Vector3 lookPosition)
    {
        _directionOfSight = (lookPosition - transform.position).normalized;
    }

    void CheckForAggro()
    {
        List<Player> allPlayers = GameManager.Instance.SERVER_Players.Values.ToList();

        allPlayers = allPlayers
            .Where((x) =>
            x != null 
            && !x.IsKnockedDown
            && !x.IsInvincible)
            .OrderBy((x)=> Vector3.Distance(transform.position, x.transform.position))
            .ToList();

        foreach (var player in allPlayers)
        {
            if (player.IsInvincible) continue;

            bool isVisible = IsVisibleForMe(player.transform.position);
            if (!isVisible) continue;

            float distance = Vector3.Distance(transform.position, player.transform.position);
            
            int distanceAggroPoints = GetAggroPointsFromDistance(distance);
            bool isPlayerCrouching = player.PlayerController.PlayerMovementService.IsCrouching;

            distanceAggroPoints = isPlayerCrouching ? Mathf.FloorToInt(distanceAggroPoints * ProjectConstants.ENEMY_AGGRO_POINTS_PLAYER_CROUCH_MULTIPLIER) : distanceAggroPoints;

            bool isCloseEnough = distanceAggroPoints >= _enemyStats.RequiredPointsToAggro;

            //Debug.Log($"[AggroController] Checking player: {player.PlayerData.PlayerName} distance: {distance} distancePoints: {distanceAggroPoints}/{_enemyStats.RequiredPointsToAggro} isVisible: {isVisible}");
            if (isCloseEnough)
            {
                SetAggro(player);
                break;
            }
        }
    }

    void CheckForAggroRelease()
    {
        if(_aggroPlayer == null) return;

        if (_aggroTimer < _enemyStats.BlindFollowDuration) return;

        float distance = Vector3.Distance(transform.position, _aggroPlayer.transform.position);
        int distanceAggroPoints = GetAggroPointsFromDistance(distance);
        bool isCloseEnough = distanceAggroPoints >= _enemyStats.RequiredPointsToAggro;
        bool isVisible = IsVisibleForMe(_aggroPlayer.transform.position);

        if ((!isCloseEnough && !isVisible))
        {
            ReleaseAggro();
        }
    }

    public void SetAggro(Player player)
    {
        _aggroPlayer = player;
        OnAggroProceed?.Invoke(player);
    }

    public void ReleaseAggro()
    {
        if (_aggroPlayer == null) return;

        _aggroTimer = 0;
        _aggroPlayer = null;
        OnAggroRelease?.Invoke();
    }
}