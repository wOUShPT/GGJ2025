// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Interactors;
using UnityEngine;

namespace AstralShift.QTI.Interactions.Demos.TPDemo
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerControllerDemo : MonoBehaviour, IInteractor, IDamageable
    {
        public float moveSpeed = 10.0f;
        public float rotateSpeed = 2.0f;
        public int hp = 100;
        [SerializeField] private InteractionFinder interactor;
        private Rigidbody rb;
        private Animator anim;
        private Transform _cameraTransform;
        private int damageState = Animator.StringToHash("Base Layer.Damage");
        [SerializeField] private bool hasInteractions = false;

        void Start()
        {
            anim = GetComponent<Animator>();
            rb = GetComponent<Rigidbody>();
            _cameraTransform = GameObject.FindWithTag("MainCamera").transform;
            anim.speed = 1.5f;
        }

        private void Update()
        {
            if (Input.GetButtonDown("Jump"))
            {
                interactor.TryInteract();
            }
        }

        void FixedUpdate()
        {
            if (anim.GetCurrentAnimatorStateInfo(0).fullPathHash == damageState)
            {
                return;
            }

            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            Vector3 cameraForwardDirection = Vector3.ProjectOnPlane(_cameraTransform.forward, Vector3.up).normalized;
            Vector3 cameraRightDirection = Vector3.ProjectOnPlane(_cameraTransform.right, Vector3.up).normalized;
            Vector3 MovementDirection = cameraRightDirection * h + cameraForwardDirection * v;
            Vector3 targetRotation = Vector3.Slerp(transform.forward, MovementDirection.normalized,
                rotateSpeed * Time.fixedDeltaTime);
            transform.forward = targetRotation;

            if (MovementDirection.magnitude > 1)
            {
                MovementDirection.Normalize();
            }

            Vector3 targetPosition = transform.position +
                                     transform.forward *
                                     (MovementDirection.magnitude * moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(targetPosition);

            anim.SetFloat("Speed", MovementDirection.magnitude);
            anim.SetFloat("Direction", (targetRotation - transform.forward).magnitude);
        }

        public void TakeDamage(int dmg)
        {
            hp -= dmg;
            anim.SetTrigger("Damage");
        }

        public Transform GetTransform()
        {
            return transform;
        }

        private void OnGUI()
        {
            float resMult = (Screen.width * Screen.height) / (1920 * 1080);
            resMult = Mathf.Clamp(resMult, 1, 1.75f);

            GUIStyle labelStyle = GUI.skin.label;
            labelStyle.fontSize = (int)(12 * resMult);

            GUI.Box(new Rect(Screen.width - 260 * resMult, 10 * resMult, 250 * resMult, 150 * resMult), "");
            GUI.Label(new Rect(Screen.width - 245 * resMult, 30 * resMult, 250 * resMult, 30 * resMult), "Controls",
                labelStyle);
            GUI.Label(new Rect(Screen.width - 245 * resMult, 60 * resMult, 250 * resMult, 30 * resMult),
                "Use Arrow Keys or WASD to move.", labelStyle);
            GUI.Label(new Rect(Screen.width - 245 * resMult, 90 * resMult, 250 * resMult, 30 * resMult),
                "R to restart level.", labelStyle);
            if (hasInteractions)
                GUI.Label(new Rect(Screen.width - 245 * resMult, 120 * resMult, 250 * resMult, 30 * resMult),
                    "SPACE to interact.", labelStyle);
        }
    }
}