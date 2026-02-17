using ECS.Scripts.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ECS.Scripts.Systems
{
    [BurstCompile]
    public partial struct PlayerHealthSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<PlayerComponent>();
        } 
        
        public void OnUpdate(ref SystemState state)
        {
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerComponent>();
            ref var player = ref SystemAPI.GetComponentRW<PlayerComponent>(playerEntity).ValueRW;
            var playerDamageBuffer = SystemAPI.GetBuffer<DamageBufferComponent>(playerEntity);
            
            // Iterates through the damage elements in the damage buffer and applies the damage to the player health
            foreach (var dmg in playerDamageBuffer)
            {
                // If the player's dead don't apply more damage
                if (player.m_CurrentHealth <= 0) break;
                
                player.m_CurrentHealth = math.clamp(player.m_CurrentHealth - dmg.m_Damage, 0, player.m_MaxHealth);
                
                Debug.Log($"Took {dmg.m_Damage} damage from {dmg.m_Causer}. Current health: {player.m_CurrentHealth}");
            }
            // If all the damage has been applied, clear the buffer for the next frame
            playerDamageBuffer.Clear();
            
            if (player.m_CurrentHealth <= 0)
            {
                Debug.Log("Player is dead!");
                var ecb = GetECB(ref state);
                ecb.DestroyEntity(0, playerEntity);
            }
        }
        
        private EntityCommandBuffer.ParallelWriter GetECB(ref SystemState state)
        {
            // Finds the pre-existing singleton ECB system
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            return ecb;
        }
    }
}