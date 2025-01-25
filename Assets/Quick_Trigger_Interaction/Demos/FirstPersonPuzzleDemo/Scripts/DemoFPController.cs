// Copyright (c) AstralShift. All rights reserved.

using UnityEngine;
using AstralShift.QTI.Interactors;
using AstralShift.QTI.Triggers.Physics;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AstralShift.QTI.Interactions.Demos.FPPuzzleDemo
{
    /// <summary>
    /// An example of a First Person Controller and Interactor implementation
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class DemoFPController : MonoBehaviour, IInputInteractor
    {
        [Header("References")] public CharacterController characterController;
        public new Camera camera;

        [Space, Header("Movement")] [Tooltip("Speed in Units(Meters)/Second")]
        public float moveSpeed = 4;

        public LayerMask groundLayerMask = -1;
        public bool useGravity = true;

        [Space, Header("Camera")] [Tooltip("X Axis Speed in Angle(Degrees)/Second")]
        public float xAxisLookSpeed = 150;

        [Tooltip("Y Axis Speed in Angle(Degrees)/Second")]
        public float yAxisLookSpeed = 80;

        [Space, Header("Camera Analog")] [Tooltip("X Axis Speed in Angle(Degrees)/Second")]
        public float xAxisAnalogLookSpeed = 25;

        [Tooltip("Y Axis Speed in Angle(Degrees)/Second")]
        public float yAxisAnalogLookSpeed = 25;

        public float lookAccelerationX = 10f; // How fast it accelerates
        public float lookDecelerationX = 10f; // How fast it decelerates
        public float maxLookSpeedX = 4f; // Maximum look speed
        public float lookAccelerationY = 10f; // How fast it accelerates
        public float lookDecelerationY = 10f; // How fast it decelerates
        public float maxLookSpeedY = 4f; // Maximum look speed

        [Space, Header("Interaction")] public LayerMask interactionLayerMask;

        [Tooltip("X Axis Speed in Angle(Degrees)/Second")]
        public float interactionDistance = 1.25f;

        public int searchFrameCount = 2;

        private Transform _transform;
        private Transform _parent;
        private Transform _cameraTransform;

        private RaycastHit[] _groundResults;
        private Vector3 _gravityVelocity;
        private Vector3 _gravityFieldVelocityDelta;
        private const float GravityConst = -9.81f;

        private bool _canMove;

        private void Reset()
        {
            characterController = GetComponent<CharacterController>();
            characterController.detectCollisions = true;
        }

        private void Awake()
        {
            Reset();
#if UNITY_2022_3_OR_NEWER
            int targetFramerate = Mathf.CeilToInt((float)Screen.currentResolution.refreshRateRatio.value);
#else
            int targetFramerate = Mathf.CeilToInt(Screen.currentResolution.refreshRate);
#endif
            Application.targetFrameRate = targetFramerate;
            QualitySettings.vSyncCount = 2;

            _cameraTransform = camera.transform;
            _transform = transform;
            _yaw = _transform.localEulerAngles.y;
            _pitch = 0;
        }

        private void Update()
        {
            if (!_canMove)
            {
                return;
            }

            HandleInputs();
            Move();
            ProcessLook();
            ProcessInteract();
        }

        private void FixedUpdate()
        {
            if (!_canMove)
            {
                return;
            }

            SearchForKeyTriggerInteractions();

            Vector3 origin = transform.position + Vector3.up;
            Debug.DrawRay(origin, Vector3.down * characterController.height, Color.red, Time.fixedDeltaTime, false);
            Physics.SphereCast(origin, characterController.radius, Vector3.down, out RaycastHit hitInfo,
                characterController.height, groundLayerMask.value, QueryTriggerInteraction.Ignore);
            if (hitInfo.collider != null && hitInfo.collider.TryGetComponent(out IGravityField gravityField))
            {
                _gravityFieldVelocityDelta = gravityField.GetMovementDelta();
            }
            else
            {
                _gravityFieldVelocityDelta = Vector3.zero;
            }
        }

        private void LateUpdate()
        {
            if (!_canMove)
            {
                return;
            }

            Look();
        }

        #region Inputs

        private Vector2 _moveInput;
        private Vector2 _lookInput;
        private Vector2 _lookAnalogInput;
        private bool _interactInput;

        /// <summary>
        /// Process Player Inputs (Keyboard and Mouse)
        /// </summary>
        private void HandleInputs()
        {
            _moveInput.x = Input.GetAxisRaw("Horizontal");
            _moveInput.y = Input.GetAxisRaw("Vertical");

            _lookAnalogInput.x = Input.GetAxis("RightHorizontal");
            _lookAnalogInput.y = Input.GetAxis("RightVertical");
            _lookInput.x = Input.GetAxisRaw("Mouse X");
            _lookInput.y = Input.GetAxisRaw("Mouse Y");

            _interactInput = Input.GetButtonDown("Fire1");
        }

        public void SetPlayerFreeze(bool state)
        {
            _canMove = !state;
        }

        #endregion

        #region Movement

        private Vector3 _moveDirection;

        private void Move()
        {
            characterController.Move(_gravityFieldVelocityDelta);

            _moveDirection = _moveInput.x * _transform.right + _moveInput.y * _transform.forward;
            if (_moveDirection.magnitude > 1)
            {
                _moveDirection.Normalize();
            }

            characterController.Move(_moveDirection * (Time.deltaTime * moveSpeed));

            if (characterController.isGrounded)
            {
                _gravityVelocity.y = 0f;
            }
            else
            {
                _gravityVelocity.y += GravityConst * Time.deltaTime;
            }

            characterController.Move(_gravityVelocity * Time.deltaTime);
        }

        public void SetGravity(bool state)
        {
            useGravity = state;
        }

        #endregion

        #region Camera

        private float _pitch;
        private float _yaw;

        private Vector2 currentLookSpeed; // Current speed for x (yaw) and y (pitch)

        private void ProcessLook()
        {
            if (_lookInput.magnitude == 0)
            {
                currentLookSpeed.x = Mathf.MoveTowards(currentLookSpeed.x, _lookAnalogInput.x * maxLookSpeedX,
                    lookAccelerationX * Time.smoothDeltaTime);
                currentLookSpeed.y = Mathf.MoveTowards(currentLookSpeed.y, _lookAnalogInput.y * maxLookSpeedY,
                    lookAccelerationY * Time.smoothDeltaTime);

                // Apply deceleration when no input is detected (optional for smooth stop)
                if (_lookAnalogInput.x == 0)
                    currentLookSpeed.x =
                        Mathf.MoveTowards(currentLookSpeed.x, 0, lookDecelerationX * Time.smoothDeltaTime);

                if (_lookAnalogInput.y == 0)
                    currentLookSpeed.y =
                        Mathf.MoveTowards(currentLookSpeed.y, 0, lookDecelerationY * Time.smoothDeltaTime);

                // Update yaw and pitch using the current speed
                _yaw += currentLookSpeed.x * xAxisAnalogLookSpeed * Time.smoothDeltaTime;
                _pitch -= currentLookSpeed.y * yAxisAnalogLookSpeed * Time.smoothDeltaTime;
            }
            else
            {
                currentLookSpeed.x = _lookInput.x;
                currentLookSpeed.y = _lookInput.y;

                // Update yaw and pitch using the current speed
                _yaw += currentLookSpeed.x * xAxisLookSpeed * Time.smoothDeltaTime;
                _pitch -= currentLookSpeed.y * yAxisLookSpeed * Time.smoothDeltaTime;
            }

            // Clamp the pitch
            _pitch = Mathf.Clamp(_pitch, -89, 89);
        }

        private void ProcessInteract()
        {
            if (_interactInput)
            {
                _interactInput = false;
                TryInteract();
            }
        }

        private void Look()
        {
            _transform.rotation = Quaternion.Euler(0, _yaw, 0);
            _cameraTransform.localEulerAngles = new Vector3(_pitch, 0, 0);
        }

        #endregion

        #region Interactor

        private RaycastHit _hitInfo;
        private int _numberOfHits;
        private InputTrigger _currentInteraction;

        public Transform GetTransform()
        {
            return transform;
        }

        public bool TryInteract()
        {
            _currentInteraction?.Interact(this);
            return _currentInteraction != null;
        }

        public InputTrigger GetInteraction()
        {
            Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (!Physics.Raycast(ray, out _hitInfo, interactionDistance, interactionLayerMask.value))
            {
                return null;
            }

            if (_hitInfo.collider.TryGetComponent(out InputTrigger trigger))
            {
                return trigger;
            }

            return null;
        }

        private void SearchForKeyTriggerInteractions()
        {
            if (Time.frameCount % searchFrameCount == 0)
            {
                var interaction = GetInteraction();

                if (interaction is null)
                {
                    _currentInteraction?.ResetVisuals();
                    _currentInteraction = null;
                    return;
                }

                if (interaction != null && _currentInteraction == null)
                {
                    _currentInteraction = interaction;
                    _currentInteraction.HighlightVisuals();
                    return;
                }

                if (interaction != _currentInteraction) //different interaction caught 
                {
                    _currentInteraction.ResetVisuals();
                    _currentInteraction = interaction;
                    _currentInteraction.HighlightVisuals();
                }
            }
        }

        #endregion

        protected void OnGUI()
        {
            float resMult = (Screen.width * Screen.height) / (1920 * 1080);
            resMult = Mathf.Clamp(resMult, 1, 1.75f);

            GUIStyle labelStyle = GUI.skin.label;
            labelStyle.fontSize = (int)(12 * resMult);

            GUI.Box(new Rect(Screen.width - 260 * resMult, 10 * resMult, 250 * resMult, 150 * resMult), "");
            GUI.Box(new Rect(Screen.width - 245 * resMult, 30 * resMult, 250 * resMult, 150 * resMult), "Controls",
                labelStyle);
            GUI.Label(new Rect(Screen.width - 245 * resMult, 60 * resMult, 250 * resMult, 30 * resMult),
                "Use Arrow Keys or WASD to move.", labelStyle);
            GUI.Label(new Rect(Screen.width - 245 * resMult, 90 * resMult, 250 * resMult, 30 * resMult),
                "Use Mouse Left Button to Interact.", labelStyle);
            GUI.Label(new Rect(Screen.width - 245 * resMult, 120 * resMult, 250 * resMult, 30 * resMult),
                "R to restart level.", labelStyle);
        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            // Ground detection gizmo
            Color defaultColor = Handles.color;
            Handles.color = Color.red;
            Vector3 origin = transform.position + Vector3.up;
            Handles.DrawLine(origin, origin + Vector3.down * characterController.height, 2);

            if (_cameraTransform == null)
            {
                Handles.color = defaultColor;
                return;
            }


            Handles.color = Color.red;
            if (camera == null)
            {
                return;
            }

            // Interaction Detection Gizmo
            Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (_hitInfo.collider == null)
            {
                Handles.DrawLine(ray.origin, ray.origin + ray.direction * interactionDistance, 2);
            }
            else
            {
                Handles.DrawLine(ray.origin, _hitInfo.point, 2);
            }

            Handles.color = defaultColor;
        }

#endif
    }
}