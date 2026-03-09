using System;
using UnityEngine;

public class PlayerStaminaService
{
    public float Stamina { get; private set; } = 1;

    const float STAMINA_RECOVERY_RATIO = 0.2f;
    float _staminaRecoverySpeed = 0;

    const float STAMINA_RECOVERY_DELAY = 1f;
    float _timer = 0;

    public event Action<float> OnStaminaUpdate;
    public event Action OnStaminaEmpty;
    public PlayerStaminaService(float staminaRecoverySpeed = 1f)
    {
        _staminaRecoverySpeed = staminaRecoverySpeed;
    }

    public void Tick(float deltaTime)
    {
        if(_timer > 0)
        {
            _timer -= deltaTime;
            return;
        }

        RecoverStamina(deltaTime);
    }

    void RecoverStamina(float deltaTime)
    {
        float stamina = Stamina + STAMINA_RECOVERY_RATIO * _staminaRecoverySpeed * deltaTime;

        SetStamina(stamina);
    }

    public void SetStamina(float stamina)
    {
        Stamina = stamina;
        Stamina = Mathf.Clamp(stamina, 0, 1);

        OnStaminaUpdate?.Invoke(Stamina);
        if(Stamina == 0)
        {
            OnStaminaEmpty?.Invoke();
        }

        //Debug.Log($"[PlayerStaminaService] Updated stamina: {Stamina}");
    }
    public void SpendStamina(float staminaToSpend)
    {
        float stamina = Stamina - staminaToSpend;
        _timer = STAMINA_RECOVERY_DELAY;

        SetStamina(stamina);
    }

    public void HandleWalk(Vector2 inputs, bool isSprinting)
    {
        if (isSprinting)
        {
            SpendStamina(0.14f * Time.deltaTime);
        }
    }

    public void HandleJump()
    {
        SpendStamina(0.2f);
    }
}
