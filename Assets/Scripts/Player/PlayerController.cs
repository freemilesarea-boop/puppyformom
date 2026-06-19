using UnityEngine;
using PuppyForMom.Core;
using PuppyForMom.Systems;
using PuppyForMom.Gameplay;
using PuppyForMom.Utils;

namespace PuppyForMom.Player
{
    /// <summary>
    /// The puppy. Stays at a fixed X while the world scrolls past; the player only controls
    /// jumping. Tap = jump, keep holding = jump higher (variable height). Uses a kinematic
    /// Rigidbody2D so trigger callbacks fire against obstacles and collectibles.
    ///
    /// Renders a full-body side-view sprite and swaps it per state (idle / 2-frame run /
    /// jump / hit). Real PNGs are loaded via <see cref="AssetLoader"/>; if absent, a
    /// procedural <see cref="SpriteFactory"/> placeholder is used so it always renders.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerController : MonoBehaviour
    {
        // Visual target so the whole dog is comfortably visible on a portrait screen,
        // independent of the source PNG resolution.
        private const float TargetHeight = 1.85f;
        private const float RunFrameTime = 0.12f; // seconds per run frame (simple 2-frame loop)

        private float _verticalVelocity;
        private bool _grounded = true;
        private float _timeSinceGrounded;
        private bool _isJumping;
        private float _holdTime;
        private bool _dead;
        private int _invincibleFrames;
        private float _restingY;

        private SpriteRenderer _sr;
        private Sprite _idle, _run1, _run2, _jump, _hit;
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

            LoadSprites();
            NormalizeSizeAndCollider();

            _restingY = GameConfig.GroundY + TargetHeight * 0.5f;
            SetPose(PuppyPose.Idle, force: true);
        }

        private void OnEnable()
        {
            _dead = false;
            _verticalVelocity = 0f;
            _grounded = true;
            var p = transform.position;
            transform.position = new Vector3(p.x, _restingY, p.z);
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
        }

        /// <summary>Scale the GO so the dog is ~TargetHeight tall regardless of PNG size, and fit the collider.</summary>
        private void NormalizeSizeAndCollider()
        {
            if (_idle == null) return;
            float h = _idle.bounds.size.y;
            float scale = h > 0.0001f ? TargetHeight / h : 1f;
            transform.localScale = new Vector3(scale, scale, 1f);

            var cap = GetComponent<CapsuleCollider2D>();
            if (cap != null)
            {
                cap.direction = CapsuleDirection2D.Horizontal;
                cap.size = new Vector2(_idle.bounds.size.x * 0.60f, _idle.bounds.size.y * 0.82f);
                cap.offset = _idle.bounds.center;
                cap.isTrigger = false;
            }
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            bool pointerDown = PointerDown();
            bool pointerHeld = PointerHeld();

            // Ready: first tap starts the run (and does the first jump). Skip the rest this frame.
            if (gm.State == GameState.Ready)
            {
                if (pointerDown) { gm.BeginPlayingIfReady(); TryJump(); }
                UpdateAnimation(gm.State);
                return;
            }

            if (gm.State == GameState.Playing && !_dead)
            {
                if (_invincibleFrames > 0) _invincibleFrames--;
                HandleInput(pointerDown, pointerHeld);
                ApplyGravityAndMove();
            }

            UpdateAnimation(gm.State);
            UpdateInvincibilityBlink();
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

        // ---------------- Animation (sprite swap, simple 2-frame run) ----------------
        private void UpdateAnimation(GameState state)
        {
            PuppyPose pose;
            if (_dead) pose = PuppyPose.Hit;
            else if (!_grounded) pose = PuppyPose.Jump;
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
            _verticalVelocity = 0f;
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
