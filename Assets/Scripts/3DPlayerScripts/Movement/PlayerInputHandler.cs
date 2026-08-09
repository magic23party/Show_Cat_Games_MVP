using UnityEngine;
using UnityEngine.InputSystem;

namespace AZE.AdvancedFirstPerson
{
    public class PlayerInputHandler : MonoBehaviour
    {
        [Header("Input Actions")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference lookAction;
        [SerializeField] private InputActionReference jumpAction;
        [SerializeField] private InputActionReference sprintAction;
        [SerializeField] private InputActionReference crouchAction;
        [SerializeField] private InputActionReference dodgeAction;

        private float maxDoubleTapTime = 0.2f;

        // Move is a Value action, so it is polled every frame. A worn analog stick
        // never rests at exactly zero, and without a deadzone that residual drift
        // keeps MoveInput non-zero forever: the state machine never falls back to
        // Idle and the player walks off on its own.
        private float moveDeadzone = 0.2f;

        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool JumpTriggered { get; private set; }
        public bool SprintPressed { get; private set; }
        public bool CrouchTriggered { get; private set; }
        public bool DodgeTriggered { get; private set; }
        public Vector2 DodgeDirection { get; private set; }


        private int _dodgeTapCount = 0;
        private float _lastTapTime;
        private Vector2 _lastDodgeInput;

        private void OnEnable()
        {
            moveAction.action.Enable();
            lookAction.action.Enable();
            jumpAction.action.Enable();
            sprintAction.action.Enable();
            crouchAction.action.Enable();
            dodgeAction.action.Enable();

            dodgeAction.action.started += HandleDodgeInput;
            dodgeAction.action.performed += HandleDodgeInput;
        }

        private void OnDisable()
        {
            dodgeAction.action.started -= HandleDodgeInput;
            dodgeAction.action.performed -= HandleDodgeInput;
        }

        private void Update()
        {
            Vector2 rawMove = moveAction.action.ReadValue<Vector2>();
            MoveInput = rawMove.sqrMagnitude < moveDeadzone * moveDeadzone ? Vector2.zero : rawMove;

            LookInput = lookAction.action.ReadValue<Vector2>();

            JumpTriggered = jumpAction.action.WasPressedThisFrame();
            SprintPressed = sprintAction.action.IsPressed();

            // Half-Life style: hold to crouch, release to stand.
            // PlayerCrouchState already gates the stand-up on CanStandUp(),
            // so releasing under a low ceiling won't clip the player into geometry.
            CrouchTriggered = crouchAction.action.IsPressed();
        }

        public InputDevice GetLookDevice => lookAction.action.activeControl?.device;

        public void UseDodge() => DodgeTriggered = false;

        private void HandleDodgeInput(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                float timeSinceLastTap = Time.time - _lastTapTime;
                Vector2 currentInput = context.ReadValue<Vector2>();

                if (_lastDodgeInput == currentInput && timeSinceLastTap <= maxDoubleTapTime)
                    _dodgeTapCount++;
                else
                    _dodgeTapCount = 1;

                _lastTapTime = Time.time;
                _lastDodgeInput = currentInput;
                DodgeDirection = currentInput;
            }

            if (context.performed && _dodgeTapCount >= 2)
            {
                _dodgeTapCount = 0;
                DodgeTriggered = true;
            }
        }
    }
}