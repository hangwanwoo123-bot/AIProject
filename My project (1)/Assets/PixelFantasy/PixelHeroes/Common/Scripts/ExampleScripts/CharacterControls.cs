using Assets.PixelFantasy.Common.Scripts;
using Assets.PixelFantasy.PixelHeroes.Common.Scripts.CharacterScripts;
using UnityEngine;

namespace Assets.PixelFantasy.PixelHeroes.Common.Scripts.ExampleScripts
{
    // Unity 6.2 환경에 맞춘 컴포넌트 강제 로드
    [RequireComponent(typeof(Creature))]
    [RequireComponent(typeof(CharacterController2D))]
    [RequireComponent(typeof(CharacterAnimation))]
    public class CharacterControls : MonoBehaviour
    {
        private Creature _character;
        private CharacterController2D _controller;
        private CharacterAnimation _animation;
        private float _fireTime;

        public void Awake() // Start보다 Awake에서 참조를 설정하는 것이 더 안전합니다.
        {
            _character = GetComponent<Creature>();
            _controller = GetComponent<CharacterController2D>();
            _animation = GetComponent<CharacterAnimation>();
        }

        public void Update()
        {
            HandleMovement();
            HandleActions();
        }

        private void HandleMovement()
        {
            Vector2 input = Vector2.zero;

            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            // [수정] .Input 대신 .moveInput으로 변경합니다.
            _controller.moveInput = input;
        }

        private void HandleActions()
        {
            // 공격 관련 테스트 추후 개발시 수정
           // if (Input.GetKeyDown(KeyCode.J)) _animation.Jab();
            //if (Input.GetKeyDown(KeyCode.S)) _animation.Slash();
            //if (Input.GetKeyDown(KeyCode.P)) _animation.Push();
           // if (Input.GetKeyDown(KeyCode.O)) _animation.Shot();
           // if (Input.GetKey(KeyCode.F)) Fire();
            //if (Input.GetKey(KeyCode.Q)) Fire(power: true); //쎄게 쏨

            // 유틸리티/애니메이션 테스트
            //if (Input.GetKeyDown(KeyCode.I)) _animation.Idle();
            //if (Input.GetKeyDown(KeyCode.R)) _animation.Ready();
           // if (Input.GetKeyDown(KeyCode.B)) _animation.Block();
            //if (Input.GetKeyDown(KeyCode.C)) _animation.Climb();
            //if (Input.GetKeyDown(KeyCode.D)) _animation.Die();
           // if (Input.GetKeyDown(KeyCode.N)) _animation.Roll();
           //if (Input.GetKeyDown(KeyCode.H)) _animation.Hit();

            // Blink 효과
            if (Input.GetKeyUp(KeyCode.L)) EffectManager.Instance.Blink(_character);
        }

        public void Fire(bool power = false)
        {
            // 연사 속도 제한 (0.15초)
            if (Time.time - _fireTime < 0.15f) return;
            _fireTime = Time.time;

            if (_animation.GetState() == CharacterState.Idle)
            {
                _animation.Ready();
            }

            // 구체적인 Character 컴포넌트 참조
            var charBase = _character as Character; // Creature를 Character로 캐스팅
            if (charBase == null) return;

            var firearm = charBase.Firearm;

            if (firearm.Detached)
            {
                firearm.Transform.gameObject.SetActive(true);
                firearm.Animator.SetTrigger(power ? "PowerFire" : "Fire");
            }
            else
            {
                _character.Animator.SetTrigger("Fire");
            }

            // 사운드 및 이펙트 처리
            if (_character.AudioSource != null)
            {
                _character.AudioSource.pitch = Random.Range(0.9f, 1.1f);
                _character.AudioSource.PlayOneShot(EffectManager.Instance.FireAudioClip);
            }

            EffectManager.Instance.CreateSpriteEffect(_character, power ? "FireMuzzleM" : "FireMuzzleS", 1, firearm.FireMuzzle);
        }
    }
}