// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Interactors;
using UnityEngine;

namespace AstralShift.QTI.Interactions.Demos.Platformer2D
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Demo2DPlayerController : MonoBehaviour, IInteractor
    {
        [Header("References")] public Animator animator;
        public Interaction2DFinder interactionFinder;
        [Header("Movement")] public float speed;
        public float jumpHeight = 2f;
        public float gravityScale = 3f;
        public float fallMultiplier = 2.5f;
        [Header("Ground Detection")] public int numberOfRays;
        public float raysSpacing;
        public LayerMask groundLayerMask;
        [Header("Audio")] public AudioClip jump;

        private Rigidbody2D _rigidBody2D;
        private BoxCollider2D _collider;
        private float _colliderDefaultXOffset;
        private const float RaysLength = 0.05f;
        private readonly int _horizontalVelocityAnimParamHash = Animator.StringToHash("HorizontalVelocity");
        private readonly int _verticalVelocityAnimParamHash = Animator.StringToHash("VerticalVelocity");
        private readonly int _isAirborneAnimParamHash = Animator.StringToHash("IsAirborne");

        private void Reset()
        {
            _rigidBody2D = GetComponent<Rigidbody2D>();
            _collider = GetComponent<BoxCollider2D>();
            _colliderDefaultXOffset = _collider.offset.x;
            _rigidBody2D.gravityScale = gravityScale;
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
        }

        private void Update()
        {
            HandleInputs();
            Move();
            ProcessInteract();
            Animate();
        }

        #region Inputs

        private float _moveInput;
        private bool _jumpInput;
        private bool _interactInput;

        /// <summary>
        /// Process Player Inputs (Keyboard and Mouse)
        /// </summary>
        private void HandleInputs()
        {
            _moveInput = Input.GetAxis("Horizontal");
            _jumpInput = Input.GetKeyDown(KeyCode.Space) || Input.GetButton("Fire2");
            _interactInput = Input.GetKeyDown(KeyCode.Return) || Input.GetButton("Fire1");
        }

        #endregion

        #region Movement

        private float _moveDirection;
        private bool _isGrounded;

        private void Move()
        {
#if UNITY_6000_0_OR_NEWER
            _rigidBody2D.linearVelocity = new Vector2(_moveInput * speed, _rigidBody2D.linearVelocity.y);
#else
            _rigidBody2D.velocity = new Vector2(_moveInput * speed, _rigidBody2D.velocity.y);
#endif
            _isGrounded = IsGrounded();
            TryJump();


#if UNITY_6000_0_OR_NEWER
            if (_rigidBody2D.linearVelocity.y < 0)
            {
                _rigidBody2D.linearVelocity +=
                    Vector2.up * (Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime);
            }
#else
            if (_rigidBody2D.velocity.y < 0)
            {
                _rigidBody2D.velocity += Vector2.up * (Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime);
            }
#endif
        }

        /// <summary>
        /// It performs raycasts to check if the character is grounded.
        /// </summary>
        /// <returns></returns>
        private bool IsGrounded()
        {
            float totalWidth = (numberOfRays - 1) * raysSpacing;
            float startX = transform.position.x - (totalWidth / 2) + _collider.offset.x;

            for (int i = 0; i < numberOfRays; i++)
            {
                Vector2 rayOrigin = new Vector3(startX + (i * raysSpacing), transform.position.y);
                Debug.DrawRay(rayOrigin, Vector2.down * RaysLength, Color.magenta);

                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, RaysLength, groundLayerMask.value);
                if (hit.collider != null)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Interaction

        private void ProcessInteract()
        {
            if (_interactInput)
            {
                _interactInput = false;
                interactionFinder.TryInteract();
            }
        }

        private void TryJump()
        {
            if (!_jumpInput)
            {
                return;
            }

            if (!_isGrounded)
            {
                return;
            }

            PlayJumpSFX();

            float jumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(Physics2D.gravity.y * gravityScale) * jumpHeight);

#if UNITY_6000_0_OR_NEWER
            _rigidBody2D.linearVelocity = new Vector2(_rigidBody2D.linearVelocity.x, jumpVelocity);
#else
            _rigidBody2D.velocity = new Vector2(_rigidBody2D.velocity.x, jumpVelocity);
#endif
        }

        public Transform GetTransform()
        {
            return transform;
        }

        #endregion

        #region Animation

        private void Animate()
        {
#if UNITY_6000_0_OR_NEWER
            if (_rigidBody2D.linearVelocity.x > 0)
#else
            if (_rigidBody2D.velocity.x > 0)
#endif
            {
                animator.transform.localScale = new Vector3(1, 1, 1);
                _collider.offset = new Vector2(_colliderDefaultXOffset, _collider.offset.y);
            }
#if UNITY_6000_0_OR_NEWER
            else if (_rigidBody2D.linearVelocity.x < 0)
#else
            else if (_rigidBody2D.velocity.x < 0)
#endif
            {
                animator.transform.localScale = new Vector3(1, 1, -1);
                _collider.offset = new Vector2(-_colliderDefaultXOffset, _collider.offset.y);
            }
#if UNITY_6000_0_OR_NEWER
            animator.SetFloat(_horizontalVelocityAnimParamHash, Mathf.Abs(_rigidBody2D.linearVelocity.x));
            animator.SetFloat(_verticalVelocityAnimParamHash, _rigidBody2D.linearVelocity.y);
#else
            animator.SetFloat(_horizontalVelocityAnimParamHash, Mathf.Abs(_rigidBody2D.velocity.x));
            animator.SetFloat(_verticalVelocityAnimParamHash, _rigidBody2D.velocity.y);
#endif
            animator.SetBool(_isAirborneAnimParamHash, !_isGrounded);
        }

        #endregion

        #region Audio

        protected void PlayJumpSFX()
        {
            if (jump == null)
            {
                return;
            }

            AudioSource.PlayClipAtPoint(jump, transform.position);
        }

        #endregion

        private void OnGUI()
        {
            float resMult = (Screen.width * Screen.height) / (1920 * 1080);
            resMult = Mathf.Clamp(resMult, 1, 1.75f);

            GUIStyle labelStyle = GUI.skin.label;
            labelStyle.fontSize = (int)(12 * resMult);

            GUI.Box(new Rect(Screen.width - 260 * resMult, 10 * resMult, 250 * resMult, 180 * resMult), "");
            GUI.Box(new Rect(Screen.width - 245 * resMult, 30 * resMult, 250 * resMult, 180 * resMult), "Controls",
                labelStyle);
            GUI.Label(new Rect(Screen.width - 245 * resMult, 60 * resMult, 250 * resMult, 30 * resMult),
                "Use Arrow Keys or WASD to Move.", labelStyle);
            GUI.Label(new Rect(Screen.width - 245 * resMult, 90 * resMult, 250 * resMult, 30 * resMult),
                "Use Space to Jump.", labelStyle);
            GUI.Label(new Rect(Screen.width - 245 * resMult, 120 * resMult, 250 * resMult, 30 * resMult),
                "Use Return to Interact.", labelStyle);
            GUI.Label(new Rect(Screen.width - 245 * resMult, 150 * resMult, 250 * resMult, 30 * resMult),
                "R to restart level.", labelStyle);
        }
    }
}