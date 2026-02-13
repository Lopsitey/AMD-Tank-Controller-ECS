using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;

namespace wave_spawning
{
    public struct PlayerComponent : IComponentData
    {
        public float moveSpeed;
    }
    
    public struct InputComponent : IComponentData
    {
        public bool spacePressed;
        public float2 playerMoveDirection;
    }
    
    /*
    // Unmanaged system version of the input system, which uses polling to check for player input.
    public partial struct InputUpdateSystem : ISystem
    {
        private void OnCreate(ref SystemState state)
        {
            //attach component to player entity
            Entity singletonEntity = state.EntityManager.CreateEntity(typeof(InputComponent));

            //Set the input component to default values
            state.EntityManager.SetComponentData(singletonEntity, new InputComponent
            {
                spacePressed = false
            });
        }
        
        private void OnUpdate(ref SystemState state)
        {
            //set data in input component based on player input
            SystemAPI.SetSingleton<InputComponent>(new InputComponent
            {
                //Could also use an input action event instead of polling the input
                spacePressed = Keyboard.current.spaceKey.isPressed
            });
        }
    }
    */
    
    // Managed system version of the input system, which uses input action events instead of polling
    public partial class InputUpdateSystem : SystemBase
    {
        private ECSPlayerInputs m_PlayerInputs;
        
        protected override void OnCreate()
        {
            m_PlayerInputs = new ECSPlayerInputs();
            m_PlayerInputs.player.Move.performed += HandlePlayerMove;
            m_PlayerInputs.player.Move.canceled += HandlePlayerMove;
            m_PlayerInputs.Enable();
            
            //Create the singleton entity
            Entity singletonEntity = EntityManager.CreateEntity(typeof(InputComponent));
            
            //Set the input component to default values
            EntityManager.SetComponentData(singletonEntity, new InputComponent
            {
                playerMoveDirection = float2.zero
            });
        }

        protected override void OnUpdate()
        {
            
        }

        protected override void OnDestroy()
        {
            m_PlayerInputs.player.Move.performed -= HandlePlayerMove;
            m_PlayerInputs.player.Move.canceled -= HandlePlayerMove;
            m_PlayerInputs.Disable();
            m_PlayerInputs.Dispose();
        }

        private void HandlePlayerMove(InputAction.CallbackContext context)
        {
            // Gets the existing input component, and make a copy of it to modify.
            InputComponent inputComponent = SystemAPI.GetSingleton<InputComponent>();
            
            // Reads the move direction - this will be 0,0 when cancelled
            // The value is read as a vector2 but stored as a float 2 through implicit conversion
            float2 moveDir = context.ReadValue<Vector2>();
            
            // Sets the move direction on the input component, thus, updating it
            inputComponent.playerMoveDirection = moveDir;
            
            // Sets the modified component back to the singleton
            SystemAPI.SetSingleton<InputComponent>(inputComponent);
        }
    }

    // Unmanaged system which moves the player based on the input component and the player component.
    public partial struct PlayerUpdateSystem : ISystem
    {
        private void OnCreate(ref SystemState state)
        {
            // Run only when we have a player and input component.
            state.RequireForUpdate<PlayerComponent>();
            state.RequireForUpdate<InputComponent>();
        }

        private void OnUpdate(ref SystemState state)
        {
            InputComponent input = SystemAPI.GetSingleton<InputComponent>();
            foreach(var (player, transform, entity) in SystemAPI.Query<RefRW<PlayerComponent>, RefRW<LocalTransform>>().WithEntityAccess())
            {
                // Gets the move vector as a float3 and cancels y so it moves only xz
                // The .xyy swizzle is used to convert the 2D move direction into a 3D move vector, with y set to 0. 
                float3 moveVec = input.playerMoveDirection.xyy;
                moveVec.y = 0; 
                
                // Distance = speed * time
                float moveDist = player.ValueRO.moveSpeed * SystemAPI.Time.DeltaTime;
                
                // Gets the new position by adding the movement vector multiplied by the movement amount to the current position
                float3 newPos = transform.ValueRO.Position + moveVec * moveDist;
                
                // Build a new transform with the pos
                LocalTransform newLT = LocalTransform.FromPosition(newPos);
                state.EntityManager.SetComponentData(entity, newLT);
            }
        }
    }
    
    public class PlayerAuthoring : MonoBehaviour
    {
        public float m_moveSpeed;
        private class PlayerBaker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                Entity entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);
                AddComponent(entity, new PlayerComponent
                {
                    moveSpeed = authoring.m_moveSpeed
                });
            }
        }
    }
}