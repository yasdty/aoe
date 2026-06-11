using AoE.RTS.Combat;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.View
{
    [DisallowMultipleComponent]
    public class UnitAnimationView : MonoBehaviour
    {
        const float FacingTurnSpeed = 12f;
        const float WalkSpeedReference = 5f;

        [SerializeField] Unit unit;
        UnitAnimationProfile profile = UnitAnimationProfile.Villager;
        Animator animator;
        UnitVisualState lastVisualState = (UnitVisualState)(-1);

        public static UnitAnimationView Ensure(GameObject unitRoot)
        {
            if (unitRoot == null)
                return null;

            UnitAnimationView view = unitRoot.GetComponent<UnitAnimationView>();
            if (view == null)
                view = unitRoot.AddComponent<UnitAnimationView>();

            return view;
        }

        void Awake()
        {
            if (unit == null)
                unit = GetComponent<Unit>();

            EnsureAnimator();
            ApplyProfileController();
        }

        void LateUpdate()
        {
            if (unit == null || animator == null)
                return;

            UnitVisualState visualState = UnitVisualStateResolver.Resolve(unit);
            ApplyAnimatorState(visualState);
            UpdateFacing(Time.deltaTime);
        }

        public void BindUnit(Unit boundUnit, UnitData data = null)
        {
            unit = boundUnit != null ? boundUnit : GetComponent<Unit>();
            profile = UnitAnimationProfileResolver.GetProfile(data != null ? data : unit != null ? unit.Data : null);
            EnsureAnimator();
            ApplyProfileController();
            ResetForPool();
        }

        public void ResetForPool()
        {
            lastVisualState = (UnitVisualState)(-1);
            transform.rotation = Quaternion.identity;

            if (animator == null || !isActiveAndEnabled)
                return;

            animator.Rebind();
            animator.Update(0f);
            SetAnimatorParameters(UnitVisualState.Idle, 0f);
        }

        public void ResetForSpawn(UnitData data)
        {
            if (unit == null)
                unit = GetComponent<Unit>();

            profile = UnitAnimationProfileResolver.GetProfile(data);
            EnsureAnimator();
            ApplyProfileController();
            ResetForPool();
        }

        void EnsureAnimator()
        {
            if (animator == null)
                animator = GetComponent<Animator>();
            if (animator == null)
                animator = gameObject.AddComponent<Animator>();

            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        }

        void ApplyProfileController()
        {
            if (animator == null)
                return;

            string resourcePath = UnitAnimationProfileResolver.GetControllerResourcePath(profile);
            RuntimeAnimatorController controller = Resources.Load<RuntimeAnimatorController>(resourcePath);
            if (controller != null && animator.runtimeAnimatorController != controller)
                animator.runtimeAnimatorController = controller;
            else if (controller == null && Application.isPlaying)
                Debug.LogWarning($"UnitAnimationView: missing controller at Resources/{resourcePath}. Run AoE → Setup Unit Animations (Phase55).");
        }

        void ApplyAnimatorState(UnitVisualState visualState)
        {
            float speed = visualState == UnitVisualState.Walk
                ? Mathf.Clamp((unit.Data != null ? unit.Data.moveSpeed : WalkSpeedReference) / WalkSpeedReference, 0.5f, 2f)
                : 0f;

            SetAnimatorParameters(visualState, speed);

            if (visualState != lastVisualState)
            {
                if (visualState == UnitVisualState.Attack)
                    animator.speed = profile == UnitAnimationProfile.Archer ? 1.35f : 1f;
                else if (visualState == UnitVisualState.Walk)
                    animator.speed = profile == UnitAnimationProfile.Archer ? 1.1f : 1f;
                else
                    animator.speed = 1f;

                lastVisualState = visualState;
            }
        }

        void SetAnimatorParameters(UnitVisualState visualState, float speed)
        {
            if (animator.runtimeAnimatorController == null)
                return;

            animator.SetFloat(UnitAnimationParameters.SpeedHash, speed);
            animator.SetBool(UnitAnimationParameters.IsGatheringHash, visualState == UnitVisualState.Gather);
            animator.SetBool(UnitAnimationParameters.IsAttackingHash, visualState == UnitVisualState.Attack);
            animator.SetBool(UnitAnimationParameters.IsDeadHash, visualState == UnitVisualState.Dead);
        }

        void UpdateFacing(float deltaTime)
        {
            if (!TryGetFacingPoint(out Vector3 worldPoint))
                return;

            Vector3 direction = worldPoint - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, deltaTime * FacingTurnSpeed);
        }

        bool TryGetFacingPoint(out Vector3 worldPoint)
        {
            if (unit == null)
            {
                worldPoint = default;
                return false;
            }

            if (AttackManager.TryGetAttackTargetPosition(unit, out worldPoint))
                return true;

            if (BoarAttackManager.TryGetAttackTargetPosition(unit, out worldPoint))
                return true;

            if (unit.TryGetMoveTargetPosition(out worldPoint))
                return true;

            worldPoint = default;
            return false;
        }
    }
}
