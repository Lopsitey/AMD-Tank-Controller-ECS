using ECS.Scripts.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace ECS.Scripts.Jobs
{
    [BurstCompile]
    public partial struct EnemyUpdateTimerJob : IJobEntity
    {
        [ReadOnly] public float deltaTime;
        
        public void Execute(ref EnemyComponent enemyComp)
        {
            enemyComp.m_AttackTimer += deltaTime;
        }
    }
}