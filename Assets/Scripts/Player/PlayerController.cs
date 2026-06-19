using UnityEngine;
using PuppyForMom.Core;
using PuppyForMom.Systems;
using PuppyForMom.Gameplay;

namespace PuppyForMom.Player
{
    /// <summary>
    /// The puppy. Stays at a fixed X while the world scrolls past; the player only controls
    /// jumping. Tap = jump, keep holding = jump higher (variable height). Uses a kinematic
    /// Rigidbody2D so trigger callbacks fire against obstacles and collectibles.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class PlayerController : MonoBehaviour
    {
        private float _verticalVelocity;
        private bool _grounded = true;
        private float _timeSinceGrounded;
        private bool _isJumping;
        private float _holdTime;
        private bool _dead;
        private int _invincibleFrames;
        private SpriteRenderer _sr;
        private float _baseScaleX;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            var rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = true;
            _baseScaleX = transform.localScale.x;
        }

        private void OnEnable()
        {
            _dead = false;
            _verticalVelocity = 0f;
            _grounded = true;
            var p = transform.position;
            transform.position = new Vector3(p.x, GameConfig.GroundY, p.z);
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            bool pointerDown = PointerDown();
            bool pointerHeld = PointerHeld();

            // First tap starts the run.
            if (gm.State == GameState.Ready && pointerDown)
            {
                gm.BeginPlayingIfReady();
                TryJump();
                return;
            }

            if (gm.State != GameState.Playing || _dead) return;

            if (_invincibleFrames > 0) _invincibleFrames--;

            HandleInput(pointerDown, pointerHeld);
            ApplyGravityAndMove();
            Animate();
        }

        private void HandleInput(bool pointerDown, bool pointerHeld)
        {
            if (pointerDown) TryJump();

            if (_isJumping && pointerHeld && _holdTime < GameConfig.MaxHoldTime)
            {
                _verticalVelocity += GameConfig.HoldJumpForce * Time.deltaTime;
                _holdTime += Time.deltaTime;
            }
            else
            {
                _isJumping = false;
            }
        }

        private void TryJump()
        {
            bool canJump = _grounded || _timeSinceGrounded <= GameConfig.CoyoteTime;
            if (!canJump) return;
            _verticalVelocity = GameConfig.JumpVelocity;
            _grounded = false;
            _isJumping = true;
            _holdTime = 0f;
            ServiceLocator.Get<AudioManager>()?.Play(Sfx.Jump);
        }

        private void ApplyGravityAndMove()
        {
            _verticalVelocity -= GameConfig.Gravity * 9.81f * Time.deltaTime;
            var pos = transform.position;
            pos.y += _verticalVelocity * Time.deltaTime;

            if (pos.y <= GameConfig.GroundY)
            {
                pos.y = GameConfig.GroundY;
                _verticalVelocity = 0f;
                if (!_grounded) _grounded = true;
                _timeSinceGrounded = 0f;
            }
            else
            {
                _timeSinceGrounded += Time.deltaTime;
            }
            transform.position = pos;
        }

        private void Animate()
        {
            // Gentle squash/stretch + bob so the placeholder puppy feels alive.
            float bob = _grounded ? Mathf.Sin(Time.time * 12f) * 0.04f : 0f;
            float stretch = Mathf.Clamp(_verticalVelocity * 0.012f, -0.12f, 0.18f);
            transform.localScale = new Vector3(
                _baseScaleX * (1f - stretch * 0.4f),
                _baseScaleX * (1f + stretch + bob),
                1f);
        }

        // ---------------- Collisions ----------------
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_dead) return;

            if (other.TryGetComponent<Collectible>(out var collectible))
            {
                collectible.Collect();
                return;
            }

            if (other.TryGetComponent<Obstacle>(out _))
            {
                if (_invincibleFrames > 0) return;
                if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;
                Die();
            }
        }

        private void Die()
        {
            _dead = true;
            _verticalVelocity = 0f;
            ServiceLocator.Get<AudioManager>()?.Play(Sfx.Hit);
            GameManager.Instance?.PlayerDied();
        }

        /// <summary>Called by GameManager flow when the player revives via a rewarded ad.</summary>
        public void Revive()
        {
            _dead = false;
            _invincibleFrames = GameConfig.ReviveBonusInvincibleFrames;
            _verticalVelocity = GameConfig.JumpVelocity;
            _grounded = false;
        }

        public bool IsInvincible => _invincibleFrames > 0;

        // ---------------- Input abstraction (legacy Input, touch + mouse) ----------------
        private static bool PointerDown()
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) return true;
            return Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space);
        }

        private static bool PointerHeld()
        {
            if (Input.touchCount > 0)
            {
                var ph = Input.GetTouch(0).phase;
                return ph == TouchPhase.Stationary || ph == TouchPhase.Moved || ph == TouchPhase.Began;
            }
            return Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space);
        }
    }
}
