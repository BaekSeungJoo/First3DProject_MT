using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GenshinInmpactMovementSystem
{
    public class PlayerMovementState : IState
    {
        protected PlayerMovementStateMachine stateMachine;

        protected Vector2 movementInput;

        protected float baseSpeed = 2.5f;
        protected float speedModifier = 0.5f;

        protected Vector3 currentTargetRotation;
        protected Vector3 timeToReachTargetRotation;
        protected Vector3 dampedTargetRotationCurrentVelocity;
        protected Vector3 dampedTargetRotationPassedTime;
        public PlayerMovementState(PlayerMovementStateMachine playerMovementStateMachine)
        {
            stateMachine = playerMovementStateMachine;

            InitializeData();
        }

        private void InitializeData()
        {
            timeToReachTargetRotation.y = 0.14f;
        }

        #region IState Methods
        public virtual void Enter()
        {
            Debug.Log("State : " + GetType().Name);
        }

        public virtual void Exit()
        {
            
        }

        public virtual void HandleInput()
        {
            ReadMovementInput();
        }

        public virtual void Update()
        {
            
        }

        public virtual void PhysicsUpdate()
        {
            Move();
        }
        #endregion

        #region Main Methods
        private void ReadMovementInput()
        {
            movementInput = stateMachine.GenshinPlayer.Input.PlayerActions.Movement.ReadValue<Vector2>();
        }

        private void Move()
        {
            if (movementInput == Vector2.zero || speedModifier == 0f)
            {
                return;
            }

            Vector3 movementDirection = GetMovementInputDirection();

            float targetRotationYAngle = Rotate(movementDirection);

            Vector3 targetRotationDirection = GetTargetRotationDirection(targetRotationYAngle);

            float movementSpeed = GetMovementSpeed();

            Vector3 currentPlayetHorizontalVelocity = GetPlayerHorizontalVelocity();

            stateMachine.GenshinPlayer.rigidbody.AddForce(targetRotationDirection * movementSpeed * 5f - currentPlayetHorizontalVelocity, ForceMode.VelocityChange);
        }

        

        private float Rotate(Vector3 direction)
        {
            float directionAngle = UpdateTargetRotation(direction);

            RotateTowardTargetRotaion();

            return directionAngle;
        }

        private void UpdateTargetRotationData(float targetAngle)
        {
            currentTargetRotation.y = targetAngle;

            dampedTargetRotationPassedTime.y = 0f;
        }

        private float AddCameraRotationToAngle(float angle)
        {
            angle += stateMachine.GenshinPlayer.MainCameraTransform.eulerAngles.y;

            if (angle > 360f)
            {
                angle -= 360f;
            }

            return angle;
        }

        private static float GetDirectionAngle(Vector3 direction)
        {
            float directionAngle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;

            if (directionAngle < 0f)
            {
                directionAngle += 360f;
            }

            return directionAngle;
        }
        #endregion

        #region Reusable Methods
        protected Vector3 GetMovementInputDirection()
        {
            return new Vector3(movementInput.x, 0f, movementInput.y);
        }

        protected float GetMovementSpeed()
        {
            return baseSpeed * speedModifier;
        }

        protected Vector3 GetPlayerHorizontalVelocity()
        {
            Vector3 playetHorizontalVelocity = stateMachine.GenshinPlayer.rigidbody.velocity;

            playetHorizontalVelocity.y = 0f;

            return playetHorizontalVelocity;
        }

        protected void RotateTowardTargetRotaion()
        {
            float currentYAngle = stateMachine.GenshinPlayer.rigidbody.rotation.eulerAngles.y;
            if(currentYAngle == currentTargetRotation.y)
            {
                return;
            }

            float smoothedYAngle = Mathf.SmoothDampAngle(currentYAngle, currentTargetRotation.y, ref dampedTargetRotationCurrentVelocity.y, timeToReachTargetRotation.y - dampedTargetRotationPassedTime.y);

            dampedTargetRotationPassedTime.y += Time.deltaTime;

            Quaternion targetRotation = Quaternion.Euler(0f, smoothedYAngle, 0f);

            stateMachine.GenshinPlayer.rigidbody.MoveRotation(targetRotation);
        }

        protected float UpdateTargetRotation(Vector3 direction, bool shouldConsiderCameraRotation = true)
        {
            float directionAngle = GetDirectionAngle(direction);

            if(shouldConsiderCameraRotation)
            {
                directionAngle = AddCameraRotationToAngle(directionAngle);
            }

            if (directionAngle != currentTargetRotation.y)
            {
                UpdateTargetRotationData(directionAngle);
            }

            return directionAngle;
        }

        protected Vector3 GetTargetRotationDirection(float targetAngle)
        {
            return Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        }
        #endregion

    }
}

