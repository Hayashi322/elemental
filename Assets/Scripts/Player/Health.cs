using System;
using Unity.Netcode;
using UnityEngine;
public class Health : NetworkBehaviour
{
    public NetworkVariable<int> CurrentHealth = new NetworkVariable<int>();
    [field: SerializeField] public int MaxHealth { get; private set; } = 100;
    private bool isDead;
    public Action<Health> OnDie;
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        CurrentHealth.Value = MaxHealth;
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
        Debug.Log($"[ApplyDamage] Called on {(IsServer ? "Server" : "Client")} for {gameObject.name}");
        if (isDead) return;
        ModifyHealth(-damageValue);
        Debug.Log($"{gameObject.name} ถูกโจมตี! เลือดเหลือ {CurrentHealth.Value}");
        if (CurrentHealth.Value == 0)
        {
            Die();
        }
    }
    public void RestoreHealth(int healValue)
    {
        if (IsServer)
        {
            ModifyHealth(healValue);
        }
        else
        {
            RestoreHealthServerRpc(healValue);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void RestoreHealthServerRpc(int healValue)
    {
        ModifyHealth(healValue);
    }
    private void ModifyHealth(int value)
    {
        if (isDead) return;
        int newHealth = CurrentHealth.Value + value;
        CurrentHealth.Value = Mathf.Clamp(newHealth, 0, MaxHealth);
        if (CurrentHealth.Value == 0 && !isDead)
        {
            OnDie?.Invoke(this);
            isDead = true;
        }
    }
    private void Die()
    {
        if (isDead) return;
        Debug.Log($"{gameObject.name} ตายแล้ว!");
        isDead = true;
        if (IsServer)
        {
            NetworkObject netObj = GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Despawn(true); // ลบ GameObject จากทั้ง Server และ Client
            }
            else
            {
                Debug.LogWarning("NetworkObject not found on object with Health.cs");
            }
        }
    }
}