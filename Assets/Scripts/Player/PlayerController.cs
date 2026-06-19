using UnityEngine;
using PuppyForMom.Core;
using PuppyForMom.Systems;
using PuppyForMom.Gameplay;
using PuppyForMom.Utils;

namespace PuppyForMom.Player
{
    /// <summary>
    /// The puppy. Auto-runs at a fixed X while the world scrolls past. Controls:
    ///   • Jump  — left-screen touch / Space / Up / Left-mouse (hold = jump higher)
    ///   • Duck  — right-screen touch / Down / S / Right-mouse (hold to stay crouched; ground only)
    ///
    /// Clear states: Run, Jump, Duck, Hit. Ducking lowers the collider so head-height
    /// obstacles pass over; it is ignored in the air. Sprites are real PNGs via
    /// <see cref="AssetLoader"/> with a <see cref="SpriteFactory"/> placeholder fallback.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerController : MonoBehaviour
    {
        private const float TargetHeight = GameConfig.CharacterWorldHeight;
        private const float RunFrameTime = 0.12f;

        private float _verticalVelocity;
        private bool _grounded = true;
        private float _timeSinceGrounded;
        private bool _isJumping;
        private float _holdTime;
        private bool _dead;
        private bool _ducking;
        private int _invincibleFrames;
        private float _restingY;

        private SpriteRenderer _sr;
        private CapsuleCollider2D _capsule;
        private Vector2 _standSize, _standOffset, _duckSize, _duckOffset;

        private Sprite _idle, _run1, _run2, _jump, _hit, _duck;
        private PuppyPose _currentPose = (PuppyPose)(-1);
        private float _runTimer;
        private bool _runFrameA;

        private void Awake()
        {
            var rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = true;

            _sr = GetComponent<SpriteRenderer>();
            _sr.sortingOrder = 10;
            _capsule = GetComponent<CapsuleCollider2D>();

            LoadSprites();
            NormalizeSizeAndCollider();

            _restingY = GameConfig.GroundY + TargetHeight * 0.5f;
            SetPose(PuppyPose.Idle, force: true);
        }

        private void OnEnable()
        {
            _dead = false;
            _ducking = false;
            _verticalVelocity = 0f;
            _grounded = true;
            var p = transform.position;
            transform.position = new Vector3(p.x, _restingY, p.z);
            ApplyColliderForState();
        }

        private void LoadSprites()
        {
            var save = ServiceLocator.Get<SaveManager>();
            string skin = save != null ? save.SelectedSkin : "Loui";
            Color color = skin == "Ver" ? GameConfig.PuppyBlack : GameConfig.PuppyCream;

            _idle = AssetLoader.Get(ArtKeys.Puppy(skin, ArtKeys.StateIdle), () => SpriteFactory.PuppyBody(color, PuppyPose.Idle));
            _run1 = AssetLoader.Get(ArtKeys.Puppy(skin, ArtKeys.StateRun01), () => SpriteFactory.PuppyBody(color, PuppyPose.Run1));
            _run2 = AssetLoader.Get(ArtKeys.Puppy(skin, ArtKeys.StateRun02), () => SpriteFactory.PuppyBody(color, PuppyPose.Run2));
            _jump = AssetLoader.Get(ArtKeys.Puppy(skin, ArtKeys.StateJump), () => SpriteFactory.PuppyBody(color, PuppyPose.Jump));
            _hit = AssetLoader.Get(ArtKeys.Puppy(skin, ArtKeys.StateHit), () => SpriteFactory.PuppyBody(color, PuppyPose.Hit));
            _duck = AssetLoader.Get(ArtKeys.Puppy(skin, ArtKeys.StateDuck), () => SpriteFactory.PuppyBody(color, PuppyPose.Duck));
        }

        /// <summary>Scale so the dog is ~TargetHeight tall regardless of PNG size, and build stand/duck colliders.</summary>
        private void NormalizeSizeAndCollider()
        {
            if (_idle == null) return;
            var b = _idle.bounds;
            float scale = b.size.y > 0.0001f ? TargetHeight / b.size.y : 1f;
            transform.localScale = new Vector3(scale, scale, 1f);

            if (_capsule == null) return;
            _capsule.direction = CapsuleDirection2D.Vertical;
            _capsule.isTrigger = false;

            // Standing hitbox (local space; gets multiplied by transform scale).
            _standSize = new Vector2(b.size.x * 0.55f, b.size.y * 0.82f);
            _standOffset = b.center;

            // Ducked hitbox: lower & shorter so head-height obstacles pass over.
            // Expressed in world units, then converted to local (divide by scale).
            float duckWorldH = GameConfig.DuckColliderWorldHeight;
            float restingAbove = TargetHeight * 0.5f;
            float centerAbove = GameConfig.DuckColliderTopAboveGround - duckWorldH * 0.5f;
            float offsetWorldY = centerAbove - restingAbove;
            _duckSize = new Vector2(b.size.x * 0.72f, duckWorldH / scale);
            _duckOffset = new Vector2(b.center.x, offsetWorldY / scale);

            _capsule.size = _standSize;
            _capsule.offset = _standOffset;
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            // Ready: first input starts the run (jump-side jumps, duck-side just begins).
            if (gm.State == GameState.Ready)
            {
                if (JumpDown()) { gm.BeginPlayingIfReady(); TryJump(); }
                else if (DuckDown()) { gm.BeginPlayingIfReady(); }
                UpdateAnimation(gm.State);
                return;
            }

            if (gm.State == GameState.Playing && !_dead)
            {
                if (_invincibleFrames > 0) _invincibleFrames--;
                HandleJump();
                HandleDuck();
                ApplyGravityAndMove();
                ApplyColliderForState();
            }

            UpdateAnimation(gm.State);
            UpdateInvincibilityBlink();
        }

        private void HandleJump()
        {
            if (JumpDown()) TryJump();

            if (_isJumping && JumpHeld() && _holdTime < GameConfig.MaxHoldTime)
            {
                _verticalVelocity += GameConfig.HoldJumpForce * Time.deltaTime;
                _holdTime += Time.deltaTime;
            }
            else
            {
                _isJumping = false;
            }
        }

        private void HandleDuck()
        {
            // Duck only while grounded; ignored in the air.
            _ducking = _grounded && !_isJumping && DuckHeld();
        }

        private void TryJump()
        {
            bool canJump = _grounded || _timeSinceGrounded <= GameConfig.CoyoteTime;
            if (!canJump) return;
            _ducking = false; // jumping cancels a duck
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

            if (pos.y <= _restingY)
            {
                pos.y = _restingY;
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

        private void ApplyColliderForState()
        {
            if (_capsule == null) return;
            bool duckBox = _ducking && _grounded;
            _capsule.size = duckBox ? _duckSize : _standSize;
            _capsule.offset = duckBox ? _duckOffset : _standOffset;
        }

        // ---------------- Animation ----------------
        private void UpdateAnimation(GameState state)
        {
            PuppyPose pose;
            if (_dead) pose = PuppyPose.Hit;
            else if (!_grounded) pose = PuppyPose.Jump;
            else if (_ducking) pose = PuppyPose.Duck;
            else if (state == GameState.Playing)
            {
                _runTimer += Time.deltaTime;
                if (_runTimer >= RunFrameTime) { _runTimer = 0f; _runFrameA = !_runFrameA; }
                pose = _runFrameA ? PuppyPose.Run1 : PuppyPose.Run2;
            }
            else pose = PuppyPose.Idle;

            SetPose(pose);
        }

        private void SetPose(PuppyPose pose, bool force = false)
        {
            if (!force && pose == _currentPose) return;
            _currentPose = pose;
            _sr.sprite = pose switch
            {
                PuppyPose.Run1 => _run1,
                PuppyPose.Run2 => _run2,
                PuppyPose.Jump => _jump,
                PuppyPose.Hit => _hit,
                PuppyPose.Duck => _duck,
                _ => _idle
            };
        }

        private void UpdateInvincibilityBlink()
        {
            if (_invincibleFrames > 0)
            {
                float a = (_invincibleFrames / 5) % 2 == 0 ? 0.45f : 1f;
                var c = _sr.color; c.a = a; _sr.color = c;
            }
            else if (_sr.color.a != 1f)
            {
                var c = _sr.color; c.a = 1f; _sr.color = c;
            }
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
            _ducking = false;
            _verticalVelocity = 0f;
            ApplyColliderForState();
            SetPose(PuppyPose.Hit, force: true);
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

        // ---------------- Input (legacy Input: touch sides + mouse + keyboard) ----------------
        private static bool TouchOnSide(bool leftSide, bool beganOnly)
        {
            int n = Input.touchCount;
            for (int i = 0; i < n; i++)
            {
                var t = Input.GetTouch(i);
                bool left = t.position.x < Screen.width * 0.5f;
                if (left != leftSide) continue;
                if (beganOnly) { if (t.phase == TouchPhase.Began) return true; }
                else if (t.phase != TouchPhase.Ended && t.phase != TouchPhase.Canceled) return true;
            }
            return false;
        }

        private static bool JumpDown() =>
            TouchOnSide(true, true) || Input.GetMouseButtonDown(0) ||
            Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow);

        private static bool JumpHeld() =>
            TouchOnSide(true, false) || Input.GetMouseButton(0) ||
            Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow);

        private static bool DuckDown() =>
            TouchOnSide(false, true) || Input.GetMouseButtonDown(1) ||
            Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S);

        private static bool DuckHeld() =>
            TouchOnSide(false, false) || Input.GetMouseButton(1) ||
            Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);
    }
}
