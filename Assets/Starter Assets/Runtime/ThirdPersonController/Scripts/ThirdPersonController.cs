using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("2.5D 横スクロール設定")]
        [Tooltip("Z軸（奥行き）の固定基準位置")]
        public float FixedZPosition = 0.0f;

        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 4.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 6.0f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.05f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;
        
        [Header("Ice Physics")]
        [Tooltip("Ice speed change rate (updated from IceBlock)")]
        private float IceSpeedChangeRate = 1.5f;
        private bool _onIce = false;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Header("ジャンプ音（AudioSource）")]
        public AudioSource jumpAudioSource;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.5f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.1f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 90.0f; // 初期向き（右向き 90度）
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;
        private Vector3 _knockbackVelocity;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;

        private bool _hasAnimator;

        private void Start()
        {
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#else
            Debug.LogError("Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            JumpAndGravity();
            GroundedCheck();
            Move();
        }

        private void LateUpdate()
        {
            // Z軸の位置を強制固定（奥行き移動・ブレを遮断）
            Vector3 currentPos = transform.position;
            if (Mathf.Abs(currentPos.z - FixedZPosition) > 0.001f)
            {
                currentPos.z = FixedZPosition;
                transform.position = currentPos;
            }
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void GroundedCheck()
        {
            // Grounded判定位置の計算（ZはFixedZPositionに固定）
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, FixedZPosition);
            
            // プレイヤー自身を「地面」として誤判定しないように、OverlapSphereで取得して自身を除外する
            Collider[] colliders = Physics.OverlapSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
            Grounded = false;
            bool hitIce = false;
            float slipRate = 1.5f;

            foreach (var col in colliders)
            {
                if (col.gameObject != gameObject)
                {
                    Grounded = true;
                    IceBlock ice = col.GetComponentInParent<IceBlock>();
                    if (ice != null)
                    {
                        hitIce = true;
                        slipRate = ice.slipRate;
                    }
                }
            }
            
            // 空中にいる間はジャンプ前の状態（滑るか滑らないか）を維持する
            if (Grounded)
            {
                _onIce = hitIce;
                IceSpeedChangeRate = slipRate;
            }

            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void Move()
        {
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            // X軸（左右）の入力値のみを使用する
            float moveInputX = _input.move.x;
            
            // 目標とする速度（符号付き：右はプラス、左はマイナス）
            float targetVelocityX = moveInputX * targetSpeed;
            
            // 現在の純粋な移動速度（実際の速度からノックバック分を引くことで壁衝突なども考慮）
            float currentVelocityX = _controller.velocity.x - _knockbackVelocity.x;

            if (_onIce)
            {
                // 氷の床：加速度と摩擦（滑り）を分けて計算（Lerpではなく定速のMoveTowardsを使うとスーッと滑る）
                if (Mathf.Abs(moveInputX) > 0.01f)
                {
                    // 切り返し時（逆方向に入力）は滑りながら減速する
                    if (Mathf.Sign(moveInputX) != Mathf.Sign(currentVelocityX) && Mathf.Abs(currentVelocityX) > 0.1f)
                    {
                        _speed = Mathf.MoveTowards(currentVelocityX, targetVelocityX, Time.deltaTime * IceSpeedChangeRate * 4f);
                    }
                    else
                    {
                        // 加速時はモッサリしないように素早くトップスピードに乗せる
                        _speed = Mathf.MoveTowards(currentVelocityX, targetVelocityX, Time.deltaTime * 15.0f);
                    }
                }
                else
                {
                    // 入力なし：一定の摩擦でスーッと滑り続ける
                    _speed = Mathf.MoveTowards(currentVelocityX, 0f, Time.deltaTime * IceSpeedChangeRate * 2f);
                }
            }
            else
            {
                // 通常の床：Lerpでキビキビとした動き
                _speed = Mathf.Lerp(currentVelocityX, targetVelocityX, Time.deltaTime * SpeedChangeRate);
            }

            // アニメーションブレンドの計算（絶対値）
            float targetBlend = Mathf.Abs(targetVelocityX);
            float animRate = _onIce ? 10.0f : SpeedChangeRate; // 氷上でもアニメの切り替わりはモッサリさせない
            _animationBlend = Mathf.Lerp(_animationBlend, targetBlend, Time.deltaTime * animRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // A/D入力がある場合のみ向きを更新
            if (moveInputX > 0.01f)
            {
                _targetRotation = 90.0f; // 右向き
            }
            else if (moveInputX < -0.01f)
            {
                _targetRotation = 270.0f; // 左向き
            }

            // スムーズに振り向く処理
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);

            // ノックバックの減衰処理
            if (_knockbackVelocity.magnitude > 0.1f)
            {
                _knockbackVelocity = Vector3.Lerp(_knockbackVelocity, Vector3.zero, Time.deltaTime * 5f);
            }
            else
            {
                _knockbackVelocity = Vector3.zero;
            }

            // CharacterControllerによる移動処理（Z軸は固定、新しい符号付き速度を適用）
            Vector3 moveMotion = new Vector3(_speed + _knockbackVelocity.x, _verticalVelocity, 0.0f) * Time.deltaTime;
            _controller.Move(moveMotion);

            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, Mathf.Abs(moveInputX));
            }
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                _fallTimeoutDelta = FallTimeout;

                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // ジャンプ（Spaceキー）
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }

                    if (jumpAudioSource != null)
                    {
                        jumpAudioSource.Play();
                    }
                    // ジャンプ入力を消費して、無操作での連続ジャンプを防ぐ
                    _input.jump = false;
                }

                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                _jumpTimeoutDelta = JumpTimeout;

                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                _input.jump = false;
            }

            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, FixedZPosition),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }

        public void Bounce(float bounceForce)
        {
            _verticalVelocity = bounceForce;

            if (_hasAnimator)
            {
                _animator.SetBool(_animIDJump, true);
                _animator.SetBool(_animIDFreeFall, false);
            }
        }

        public void ApplyKnockback(Vector3 direction, float force)
        {
            _knockbackVelocity = direction.normalized * force;
            _verticalVelocity = force * 0.5f; // 少し浮かす
            
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDJump, true);
                _animator.SetBool(_animIDFreeFall, false);
            }
        }

    }
}