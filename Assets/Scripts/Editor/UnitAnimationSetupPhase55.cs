using AoE.RTS.Units;
using AoE.RTS.View;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace AoE.RTS.EditorTools
{
    public static class UnitAnimationSetupPhase55
    {
        const string ResourceFolder = "Assets/Resources/UnitAnimation";
        const string VisualPath = "Visual";

        [MenuItem("AoE/Setup Unit Animations (Phase55)", true)]
        static bool ValidateSetupUnitAnimationsPhase55() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Setup Unit Animations (Phase55)")]
        public static void SetupUnitAnimationsPhase55()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            EnsureResourceFolder();
            GenerateProfileAssets(UnitAnimationProfile.Villager, 1f, 1f, 1f);
            GenerateProfileAssets(UnitAnimationProfile.Militia, 1.05f, 1.15f, 1.2f);
            GenerateProfileAssets(UnitAnimationProfile.Archer, 0.95f, 1.25f, 1.35f);
            WireSceneUnits();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Unit animation setup complete (Phase55). Clips/controllers saved under Resources/UnitAnimation. Save scene if prompted.");
        }

        static void EnsureResourceFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder(ResourceFolder))
                AssetDatabase.CreateFolder("Assets/Resources", "UnitAnimation");
        }

        static void GenerateProfileAssets(
            UnitAnimationProfile profile,
            float idleScale,
            float walkScale,
            float attackScale)
        {
            string profileName = profile.ToString();
            string folder = $"{ResourceFolder}/{profileName}";
            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder(ResourceFolder, profileName);

            AnimationClip idleClip = SaveClip(folder, $"{profileName}_Idle", CreateLoopScaleClip(0.8f, idleScale, idleScale + 0.06f));
            AnimationClip walkClip = SaveClip(folder, $"{profileName}_Walk", CreateLoopScaleClip(0.45f, walkScale, walkScale + 0.1f));
            AnimationClip gatherClip = SaveClip(folder, $"{profileName}_Gather", CreateGatherClip(0.55f));
            AnimationClip attackClip = SaveClip(folder, $"{profileName}_Attack", CreateAttackClip(0.35f, attackScale));
            AnimationClip deadClip = SaveClip(folder, $"{profileName}_Dead", CreateDeadClip(0.35f));

            string controllerPath = $"{ResourceFolder}/{profileName}.controller";
            CreateController(controllerPath, idleClip, walkClip, gatherClip, attackClip, deadClip);
        }

        static AnimationClip SaveClip(string folder, string clipName, AnimationClip clip)
        {
            string path = $"{folder}/{clipName}.anim";
            AnimationClip existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (existing != null)
                EditorUtility.CopySerialized(clip, existing);
            else
                AssetDatabase.CreateAsset(clip, path);

            return AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        }

        static AnimationClip CreateLoopScaleClip(float duration, float baseScale, float peakScale)
        {
            AnimationClip clip = new AnimationClip { frameRate = 60f };
            clip.wrapMode = WrapMode.Loop;
            SetTransformCurve(clip, VisualPath, "m_LocalScale.x", baseScale, peakScale, duration);
            SetTransformCurve(clip, VisualPath, "m_LocalScale.y", baseScale, peakScale, duration);
            SetTransformCurve(clip, VisualPath, "m_LocalScale.z", baseScale, peakScale, duration);
            return clip;
        }

        static AnimationClip CreateGatherClip(float duration)
        {
            AnimationClip clip = new AnimationClip { frameRate = 60f };
            clip.wrapMode = WrapMode.Loop;
            SetTransformCurve(clip, VisualPath, "m_LocalScale.y", 0.95f, 1.12f, duration);
            SetTransformCurve(clip, VisualPath, "m_LocalPosition.y", 0f, 0.08f, duration);
            SetRotationCurve(clip, VisualPath, 12f, duration);
            return clip;
        }

        static AnimationClip CreateAttackClip(float duration, float punchScale)
        {
            AnimationClip clip = new AnimationClip { frameRate = 60f };
            clip.wrapMode = WrapMode.Once;
            AnimationCurve scaleCurve = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(duration * 0.35f, punchScale),
                new Keyframe(duration, 1f));
            SetCurve(clip, VisualPath, typeof(Transform), "m_LocalScale.x", scaleCurve);
            SetCurve(clip, VisualPath, typeof(Transform), "m_LocalScale.z", scaleCurve);
            SetRotationCurve(clip, VisualPath, 18f, duration, once: true);
            return clip;
        }

        static AnimationClip CreateDeadClip(float duration)
        {
            AnimationClip clip = new AnimationClip { frameRate = 60f };
            clip.wrapMode = WrapMode.ClampForever;
            AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0f, 0f, duration, 85f);
            SetCurve(clip, VisualPath, typeof(Transform), "localEulerAnglesRaw.x", rotationCurve);
            AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 1f, duration, 0.85f);
            SetCurve(clip, VisualPath, typeof(Transform), "m_LocalScale.y", scaleCurve);
            return clip;
        }

        static void SetTransformCurve(AnimationClip clip, string path, string property, float baseValue, float peakValue, float duration)
        {
            AnimationCurve curve = new AnimationCurve(
                new Keyframe(0f, baseValue),
                new Keyframe(duration * 0.5f, peakValue),
                new Keyframe(duration, baseValue));
            SetCurve(clip, path, typeof(Transform), property, curve);
        }

        static void SetRotationCurve(AnimationClip clip, string path, float peakDegrees, float duration, bool once = false)
        {
            AnimationCurve curve = once
                ? new AnimationCurve(
                    new Keyframe(0f, 0f),
                    new Keyframe(duration * 0.35f, peakDegrees),
                    new Keyframe(duration, 0f))
                : new AnimationCurve(
                    new Keyframe(0f, 0f),
                    new Keyframe(duration * 0.5f, peakDegrees),
                    new Keyframe(duration, 0f));
            SetCurve(clip, path, typeof(Transform), "localEulerAnglesRaw.x", curve);
        }

        static void SetCurve(AnimationClip clip, string path, System.Type type, string propertyName, AnimationCurve curve)
        {
            clip.SetCurve(path, type, propertyName, curve);
        }

        static void CreateController(
            string controllerPath,
            AnimationClip idleClip,
            AnimationClip walkClip,
            AnimationClip gatherClip,
            AnimationClip attackClip,
            AnimationClip deadClip)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null)
                controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            else
                ClearController(controller);

            controller.AddParameter(UnitAnimationParameters.Speed, AnimatorControllerParameterType.Float);
            controller.AddParameter(UnitAnimationParameters.IsGathering, AnimatorControllerParameterType.Bool);
            controller.AddParameter(UnitAnimationParameters.IsAttacking, AnimatorControllerParameterType.Bool);
            controller.AddParameter(UnitAnimationParameters.IsDead, AnimatorControllerParameterType.Bool);

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            AnimatorState idle = stateMachine.AddState("Idle", new Vector3(250f, 0f, 0f));
            idle.motion = idleClip;
            stateMachine.defaultState = idle;

            AnimatorState walk = stateMachine.AddState("Walk", new Vector3(450f, 0f, 0f));
            walk.motion = walkClip;

            AnimatorState gather = stateMachine.AddState("Gather", new Vector3(450f, 120f, 0f));
            gather.motion = gatherClip;

            AnimatorState attack = stateMachine.AddState("Attack", new Vector3(450f, -120f, 0f));
            attack.motion = attackClip;

            AnimatorState dead = stateMachine.AddState("Dead", new Vector3(650f, 0f, 0f));
            dead.motion = deadClip;

            AddTransition(idle, walk, AnimatorConditionMode.Greater, 0.05f, UnitAnimationParameters.Speed);
            AddTransition(walk, idle, AnimatorConditionMode.Less, 0.05f, UnitAnimationParameters.Speed);
            AddAnyStateTransition(stateMachine, gather, UnitAnimationParameters.IsGathering, ifNot: false);
            AddTransition(gather, idle, UnitAnimationParameters.IsGathering, ifNot: true);
            AddAnyStateTransition(stateMachine, attack, UnitAnimationParameters.IsAttacking, ifNot: false);
            AddTransition(attack, idle, UnitAnimationParameters.IsAttacking, ifNot: true);
            AddAnyStateTransition(stateMachine, dead, UnitAnimationParameters.IsDead, ifNot: false);
        }

        static void ClearController(AnimatorController controller)
        {
            AnimatorControllerLayer layer = controller.layers[0];
            AnimatorStateMachine stateMachine = layer.stateMachine;

            while (stateMachine.anyStateTransitions.Length > 0)
                stateMachine.RemoveAnyStateTransition(stateMachine.anyStateTransitions[0]);

            while (stateMachine.states.Length > 0)
                stateMachine.RemoveState(stateMachine.states[0].state);

            while (controller.parameters.Length > 0)
                controller.RemoveParameter(0);
        }

        static void AddTransition(AnimatorState from, AnimatorState to, AnimatorConditionMode mode, float threshold, string parameter)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.duration = 0.05f;
            transition.AddCondition(mode, threshold, parameter);
        }

        static void AddTransition(AnimatorState from, AnimatorState to, string parameter, bool ifNot)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.duration = 0.05f;
            transition.AddCondition(ifNot ? AnimatorConditionMode.IfNot : AnimatorConditionMode.If, 0f, parameter);
        }

        static void AddAnyStateTransition(AnimatorStateMachine stateMachine, AnimatorState to, string parameter, bool ifNot)
        {
            AnimatorStateTransition transition = stateMachine.AddAnyStateTransition(to);
            transition.hasExitTime = false;
            transition.duration = 0.05f;
            transition.canTransitionToSelf = true;
            transition.AddCondition(ifNot ? AnimatorConditionMode.IfNot : AnimatorConditionMode.If, 0f, parameter);
        }

        static void WireSceneUnits()
        {
            Unit[] units = Object.FindObjectsByType<Unit>(FindObjectsInactive.Include);
            int wired = 0;
            for (int i = 0; i < units.Length; i++)
            {
                Unit unit = units[i];
                if (unit == null)
                    continue;

                bool wasActive = unit.gameObject.activeSelf;
                if (!wasActive)
                    unit.gameObject.SetActive(true);

                UnitAnimationView animationView = UnitAnimationView.Ensure(unit.gameObject);
                animationView.BindUnit(unit, unit.Data);

                if (!wasActive)
                    unit.gameObject.SetActive(false);

                EditorUtility.SetDirty(unit.gameObject);
                wired++;
            }

            if (wired > 0)
                Debug.Log($"Wired UnitAnimationView on {wired} unit(s) in the open scene.");
        }
    }
}
