using ECS.Scripts.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ECS.Scripts.Systems
{
    // This ensures that the system runs after the physics system so the player will have actually moved by the time the var is set back to false
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    // No [BurstCompile] needed as this is a managed system.
    // Managed system version of the input system, which uses input action events instead of polling
    public partial class InputUpdateSystem : SystemBase
    {
        private ECSPlayerInputs m_PlayerInputs;
        protected override void OnCreate()
        {
            m_PlayerInputs = new ECSPlayerInputs();
            m_PlayerInputs.player.Move.performed += HandlePlayerMove;
            m_PlayerInputs.player.Move.canceled += HandlePlayerMove;
            m_PlayerInputs.player.Jump.performed += HandlePlayerJump;
            m_PlayerInputs.player.Stop.performed += HandlePlayerStop;
            m_PlayerInputs.player.Stop.canceled += HandlePlayerStop;
            m_PlayerInputs.player.Attack.performed += HandlePlayerAttack;
            m_PlayerInputs.player.Attack.canceled += HandlePlayerAttack;
            m_PlayerInputs.Enable();
            
            // Creates the singleton entity - this keeps the component separate from the player entity 
            Entity singletonEntity = EntityManager.CreateEntity(typeof(InputComponent));
            
            //Set the input component to default values
            EntityManager.SetComponentData(singletonEntity, new InputComponent
            {
                playerMoveDirection = float2.zero,
                jumpPressed = false,
                spacePressed = false,
                m_JumpCooldown = 1f,
                m_JumpCooldownTimer = 0f
            });
        }

        protected override void OnUpdate()
        {
            InputComponent inputComp = SystemAPI.GetSingleton<InputComponent>();
            
            // Decrements the cooldown timer if it is active
            if (inputComp.m_JumpCooldownTimer > 0f)
                inputComp.m_JumpCooldownTimer -= SystemAPI.Time.DeltaTime;
            
            SystemAPI.SetSingleton(inputComp);
        }
        
        protected override void OnDestroy()
        {
            m_PlayerInputs.player.Move.performed -= HandlePlayerMove;
            m_PlayerInputs.player.Move.canceled -= HandlePlayerMove;
            m_PlayerInputs.player.Jump.performed -= HandlePlayerJump;
            m_PlayerInputs.player.Stop.performed -= HandlePlayerStop;
            m_PlayerInputs.player.Stop.canceled -= HandlePlayerStop;
            m_PlayerInputs.player.Attack.performed -= HandlePlayerAttack;
            m_PlayerInputs.player.Attack.canceled -= HandlePlayerAttack;
            m_PlayerInputs.Disable();
            m_PlayerInputs.Dispose();
        }

        private void HandlePlayerAttack(InputAction.CallbackContext obj)
        {
            //
        }

        private void HandlePlayerStop(InputAction.CallbackContext obj)
        {
            //
        }

        private void HandlePlayerJump(InputAction.CallbackContext ctx)
        {
            InputComponent inputComp = SystemAPI.GetSingleton<InputComponent>();

            if (inputComp.m_JumpCooldownTimer <= 0f)
            {
                inputComp.jumpPressed = true;
                // Starts the timer
                inputComp.m_JumpCooldownTimer = inputComp.m_JumpCooldown;
            }
            SystemAPI.SetSingleton(inputComp);
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
    
    // Runs in the physic loop so the player movement job has chance to see the flag before its reset
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    // Runs immediately after the PlayerUpdateSystem so the jumpPressed flag can be reset in the same frame
    [UpdateAfter(typeof(PlayerUpdateSystem))]
    [BurstCompile]
    public partial struct JumpResetSystem : ISystem
    {
        public void OnCreate(ref SystemState state) 
            => state.RequireForUpdate<InputComponent>();
        public void OnUpdate(ref SystemState state)
        {
            var inputComp = SystemAPI.GetSingleton<InputComponent>();
            if (inputComp.jumpPressed)
            {
                // Immediately cancels the jump input after it has been activated
                // This means the jump will only last for one frame.
                inputComp.jumpPressed = false;
                SystemAPI.SetSingleton(inputComp);
            }
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