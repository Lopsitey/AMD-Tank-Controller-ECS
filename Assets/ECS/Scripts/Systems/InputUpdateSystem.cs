using ECS.Scripts.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ECS.Scripts.Systems
{
    [BurstCompile]
    // Managed system version of the input system, which uses input action events instead of polling
    public partial class InputUpdateSystem : SystemBase
    {
        private ECSPlayerInputs m_PlayerInputs;
        
        [BurstCompile]
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

        [BurstCompile]
        protected override void OnUpdate()
        {
            
        }

        [BurstCompile]
        protected override void OnDestroy()
        {
            m_PlayerInputs.player.Move.performed -= HandlePlayerMove;
            m_PlayerInputs.player.Move.canceled -= HandlePlayerMove;
            m_PlayerInputs.Disable();
            m_PlayerInputs.Dispose();
        }

        [BurstCompile]
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
}