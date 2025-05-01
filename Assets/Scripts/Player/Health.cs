using System;
using Unity.Netcode;
using UnityEngine;
using System.Collections;


public class Health : NetworkBehaviour
{
    public NetworkVariable<int> CurrentHealth = new NetworkVariable<int>();
    [field: SerializeField] public int MaxHealth { get; private set; } = 100;

    private bool isDead;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            CurrentHealth.Value = MaxHealth;
            isDead = false;
        }
    }

    public void TakeDamage(int damageValue)
    {
        if (IsServer)
        {
            ApplyDamage(damageValue);
        }
        else
        {
            TakeDamageServerRpc(damageValue);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void TakeDamageServerRpc(int damageValue)
    {
        ApplyDamage(damageValue);
    }

    private void ApplyDamage(int damageValue)
    {
        if (isDead) return;

        ModifyHealth(-damageValue);
        Debug.Log($"🩸 {gameObject.name} โดนโจมตี! เหลือ {CurrentHealth.Value}");
    }

    private void ModifyHealth(int value)
    {
        if (isDead) return;

        int newHealth = CurrentHealth.Value + value;
        CurrentHealth.Value = Mathf.Clamp(newHealth, 0, MaxHealth);

        if (CurrentHealth.Value <= 0 && !isDead)
        {
            isDead = true;
            Die();
        }
    }

    public void ResetHealth()
    {
        if (IsServer)
        {
            isDead = false;
            CurrentHealth.Value = MaxHealth;
        }
    }

    private void Die()
    {
        Debug.Log($"💀 {gameObject.name} ตายแล้ว!");

        if (IsServer)
        {
            if (GameRoundManager.Instance != null)
                GameRoundManager.Instance.OnPlayerDied(OwnerClientId);
            else
                Debug.LogWarning("⚠️ GameRoundManager.Instance is NULL");

            var netObj = GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsSpawned)
            {
                netObj.Despawn(true); // ✅ Only server can despawn
            }
        }
    }




}
