using ECS.Scripts.Components;
using ECS.Scripts.Utility;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;

namespace ECS.Scripts.Jobs
{
    public struct EnemyUpdateDamageJob : ITriggerEventsJob
    {
        [ReadOnly] public ComponentLookup<EnemyComponent> enemyLookup;
        [ReadOnly] public ComponentLookup<PlayerComponent> playerLookup;

        public EntityCommandBuffer ecb;
        
        public void Execute(TriggerEvent triggerEvent)
        {
            var (matched, player, enemy) = PhysicsEventUtils.ComponentLookupMatch(triggerEvent, playerLookup, enemyLookup); 
            
            // Returns early if there was no match
            if (!matched)
                return;
            
            // Return early if the enemy can't attack yet
            if (enemy.component.m_AttackTimer < enemy.component.m_AttackFreq) return;
            
            // Resets the attack timer
            enemy.component.m_AttackTimer = 0f;
            
            // Add damage to the player buffer
            ecb.AppendToBuffer(player.entity, new DamageBufferComponent
            {
                m_Causer = enemy.entity,
                m_Damage = enemy.component.m_MinDamage
            });
            
            // Reset the enemy's attack timer
            ecb.SetComponent(enemy.entity, enemy.component);
        }
    }
}