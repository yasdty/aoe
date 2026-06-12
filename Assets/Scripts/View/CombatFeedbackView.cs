using System.Collections;
using System.Collections.Generic;
using AoE.RTS.Combat;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.View
{
    [DisallowMultipleComponent]
    public class CombatFeedbackView : MonoBehaviour
    {
        const int AudioPoolSize = 8;
        const int ProjectilePoolSize = 24;
        const int BurstPoolSize = 16;
        const float MaxEffectDistance = 120f;

        [SerializeField] float rangedProjectileDuration = 0.25f;
        [SerializeField] float deathReturnDelay = 0.3f;
        [SerializeField] float hitFlashDuration = 0.15f;
        [SerializeField] float hitFlashScale = 0.55f;

        Material projectileMaterial;
        GameObject hitBurstPrefab;
        GameObject deathPuffPrefab;
        AudioClip meleeHitClip;
        AudioClip rangedHitClip;
        AudioClip unitDeathClip;

        readonly Stack<Transform> projectilePool = new Stack<Transform>();
        readonly Stack<Transform> burstPool = new Stack<Transform>();
        readonly Stack<AudioSource> audioPool = new Stack<AudioSource>();
        readonly List<ProjectileFlight> activeProjectiles = new List<ProjectileFlight>();
        Transform effectRoot;
        Transform audioRoot;
        UnityEngine.Camera mainCamera;

        struct ProjectileFlight
        {
            public Transform transform;
            public Vector3 start;
            public Vector3 end;
            public float elapsed;
            public float duration;
            public bool playRangedAudioOnImpact;
        }

        void Awake()
        {
            mainCamera = UnityEngine.Camera.main;
            EnsureRoots();
            LoadResources();
            WarmPools();
            CombatFeedbackBus.OnFeedback += HandleFeedback;
            CombatDeathScheduler.SetHandler(ScheduleUnitReturn);
            CombatAudioHooks.Bind(this);
        }

        void OnDestroy()
        {
            CombatFeedbackBus.OnFeedback -= HandleFeedback;
            CombatDeathScheduler.SetHandler(null);
            CombatAudioHooks.Bind(null);
        }

        void Update()
        {
            if (activeProjectiles.Count == 0)
                return;

            float deltaTime = Time.deltaTime;
            for (int i = activeProjectiles.Count - 1; i >= 0; i--)
            {
                ProjectileFlight flight = activeProjectiles[i];
                if (flight.transform == null)
                {
                    activeProjectiles.RemoveAt(i);
                    continue;
                }

                flight.elapsed += deltaTime;
                float t = flight.duration <= 0f ? 1f : Mathf.Clamp01(flight.elapsed / flight.duration);
                Vector3 position = Vector3.Lerp(flight.start, flight.end, t);
                flight.transform.position = position;
                activeProjectiles[i] = flight;

                if (t < 1f)
                    continue;

                if (flight.playRangedAudioOnImpact)
                    PlayRangedHit(flight.end);

                SpawnHitBurst(flight.end, CombatFeedbackKind.RangedHit);
                ReturnProjectile(flight.transform);
                activeProjectiles.RemoveAt(i);
            }
        }

        void HandleFeedback(CombatFeedbackEvent feedbackEvent)
        {
            if (!IsNearCamera(feedbackEvent.targetWorldPosition))
                return;

            switch (feedbackEvent.kind)
            {
                case CombatFeedbackKind.RangedHit:
                    LaunchProjectile(
                        feedbackEvent.sourceWorldPosition,
                        feedbackEvent.targetWorldPosition,
                        playRangedAudioOnImpact: true);
                    break;
                case CombatFeedbackKind.MeleeHit:
                    SpawnHitBurst(feedbackEvent.targetWorldPosition, feedbackEvent.kind);
                    PlayMeleeHit(feedbackEvent.targetWorldPosition);
                    break;
                case CombatFeedbackKind.BuildingHit:
                    SpawnHitBurst(feedbackEvent.targetWorldPosition, feedbackEvent.kind);
                    PlayMeleeHit(feedbackEvent.targetWorldPosition);
                    break;
            }
        }

        public void ScheduleUnitReturn(Unit unit, float delaySeconds)
        {
            if (unit == null)
                return;

            if (delaySeconds <= 0f)
            {
                UnitPool.Return(unit);
                return;
            }

            StartCoroutine(ReturnUnitAfterDelay(unit, delaySeconds));
        }

        IEnumerator ReturnUnitAfterDelay(Unit unit, float delaySeconds)
        {
            Vector3 puffPosition = unit.transform.position + Vector3.up * 0.6f;
            SpawnDeathPuff(puffPosition);
            PlayUnitDeath(puffPosition);

            float elapsed = 0f;
            while (elapsed < delaySeconds)
            {
                if (unit == null)
                    yield break;

                elapsed += Time.deltaTime;
                yield return null;
            }

            if (unit != null)
                UnitPool.Return(unit);
        }

        public void PlayMeleeHit(Vector3 worldPosition)
        {
            PlayClipAt(meleeHitClip, worldPosition, 0.55f);
        }

        public void PlayRangedHit(Vector3 worldPosition)
        {
            PlayClipAt(rangedHitClip, worldPosition, 0.5f);
        }

        public void PlayUnitDeath(Vector3 worldPosition)
        {
            PlayClipAt(unitDeathClip, worldPosition, 0.65f);
        }

        void LaunchProjectile(Vector3 sourceWorldPosition, Vector3 targetWorldPosition, bool playRangedAudioOnImpact)
        {
            Transform projectile = RentProjectile();
            Vector3 start = sourceWorldPosition + Vector3.up * 0.15f;
            Vector3 end = targetWorldPosition + Vector3.up * 0.35f;
            projectile.position = start;
            projectile.gameObject.SetActive(true);

            activeProjectiles.Add(new ProjectileFlight
            {
                transform = projectile,
                start = start,
                end = end,
                elapsed = 0f,
                duration = rangedProjectileDuration,
                playRangedAudioOnImpact = playRangedAudioOnImpact
            });
        }

        void SpawnHitBurst(Vector3 worldPosition, CombatFeedbackKind kind)
        {
            Color color = kind == CombatFeedbackKind.BuildingHit
                ? new Color(0.75f, 0.75f, 0.8f, 0.9f)
                : new Color(1f, 0.55f, 0.2f, 0.95f);

            if (TryPlayParticle(hitBurstPrefab, worldPosition))
                return;

            Transform flash = RentBurst();
            flash.position = worldPosition;
            flash.localScale = Vector3.one * 0.15f;
            flash.gameObject.SetActive(true);
            StartCoroutine(AnimateFlash(flash, color, hitFlashScale, hitFlashDuration));
        }

        void SpawnDeathPuff(Vector3 worldPosition)
        {
            if (TryPlayParticle(deathPuffPrefab, worldPosition))
                return;

            Transform flash = RentBurst();
            flash.position = worldPosition;
            flash.localScale = Vector3.one * 0.25f;
            flash.gameObject.SetActive(true);
            StartCoroutine(AnimateFlash(flash, new Color(0.55f, 0.55f, 0.55f, 0.85f), 1.1f, deathReturnDelay));
        }

        IEnumerator AnimateFlash(Transform flash, Color color, float peakScale, float duration)
        {
            if (flash == null)
                yield break;

            Renderer renderer = flash.GetComponent<Renderer>();
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (flash == null)
                    yield break;

                elapsed += Time.deltaTime;
                float t = duration <= 0f ? 1f : elapsed / duration;
                float scale = Mathf.Lerp(0.15f, peakScale, Mathf.Sin(t * Mathf.PI));
                flash.localScale = Vector3.one * scale;

                if (renderer != null)
                {
                    Color current = color;
                    current.a = color.a * (1f - t);
                    renderer.GetPropertyBlock(block);
                    block.SetColor("_BaseColor", current);
                    block.SetColor("_Color", current);
                    renderer.SetPropertyBlock(block);
                }

                yield return null;
            }

            ReturnBurst(flash);
        }

        bool TryPlayParticle(GameObject prefab, Vector3 worldPosition)
        {
            if (prefab == null)
                return false;

            GameObject instance = Instantiate(prefab, worldPosition, Quaternion.identity, effectRoot);
            ParticleSystem particleSystem = instance.GetComponent<ParticleSystem>();
            if (particleSystem == null)
            {
                Destroy(instance);
                return false;
            }

            particleSystem.Play();
            StartCoroutine(DestroyAfterParticle(instance, particleSystem.main.duration + particleSystem.main.startLifetime.constantMax));
            return true;
        }

        IEnumerator DestroyAfterParticle(GameObject instance, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (instance != null)
                Destroy(instance);
        }

        void PlayClipAt(AudioClip clip, Vector3 worldPosition, float volume)
        {
            if (clip == null)
                return;

            AudioSource source = RentAudioSource();
            source.transform.position = worldPosition;
            source.clip = clip;
            source.volume = volume;
            source.Play();
            StartCoroutine(ReturnAudioSourceAfterPlay(source, clip.length));
        }

        IEnumerator ReturnAudioSourceAfterPlay(AudioSource source, float delay)
        {
            yield return new WaitForSeconds(Mathf.Max(0.05f, delay));
            if (source != null)
                ReturnAudioSource(source);
        }

        bool IsNearCamera(Vector3 worldPosition)
        {
            if (mainCamera == null)
                mainCamera = UnityEngine.Camera.main;
            if (mainCamera == null)
                return true;

            Vector3 cameraPosition = mainCamera.transform.position;
            cameraPosition.y = worldPosition.y;
            return (cameraPosition - worldPosition).sqrMagnitude <= MaxEffectDistance * MaxEffectDistance;
        }

        void EnsureRoots()
        {
            effectRoot = new GameObject("CombatEffects").transform;
            effectRoot.SetParent(transform, false);
            audioRoot = new GameObject("CombatAudio").transform;
            audioRoot.SetParent(transform, false);
        }

        void LoadResources()
        {
            projectileMaterial = Resources.Load<Material>(CombatFeedbackPaths.ProjectileMaterialResource);
            hitBurstPrefab = Resources.Load<GameObject>(CombatFeedbackPaths.HitBurstResource);
            deathPuffPrefab = Resources.Load<GameObject>(CombatFeedbackPaths.DeathPuffResource);
            meleeHitClip = Resources.Load<AudioClip>(CombatFeedbackPaths.MeleeHitAudioResource);
            rangedHitClip = Resources.Load<AudioClip>(CombatFeedbackPaths.RangedHitAudioResource);
            unitDeathClip = Resources.Load<AudioClip>(CombatFeedbackPaths.UnitDeathAudioResource);
        }

        void WarmPools()
        {
            for (int i = 0; i < ProjectilePoolSize; i++)
                projectilePool.Push(CreateProjectile());

            for (int i = 0; i < BurstPoolSize; i++)
                burstPool.Push(CreateBurstFlash());

            for (int i = 0; i < AudioPoolSize; i++)
                audioPool.Push(CreateAudioSource());
        }

        Transform CreateProjectile()
        {
            GameObject projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectileObject.name = "CombatProjectile";
            Object.Destroy(projectileObject.GetComponent<Collider>());
            projectileObject.transform.SetParent(effectRoot, false);
            projectileObject.transform.localScale = Vector3.one * 0.18f;

            Renderer renderer = projectileObject.GetComponent<Renderer>();
            if (projectileMaterial != null)
                renderer.sharedMaterial = projectileMaterial;
            else
                renderer.sharedMaterial.color = new Color(0.95f, 0.75f, 0.2f);

            projectileObject.SetActive(false);
            return projectileObject.transform;
        }

        Transform CreateBurstFlash()
        {
            GameObject flashObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flashObject.name = "CombatHitFlash";
            Object.Destroy(flashObject.GetComponent<Collider>());
            flashObject.transform.SetParent(effectRoot, false);
            Renderer renderer = flashObject.GetComponent<Renderer>();
            renderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            flashObject.SetActive(false);
            return flashObject.transform;
        }

        AudioSource CreateAudioSource()
        {
            GameObject audioObject = new GameObject("CombatAudioSource");
            audioObject.transform.SetParent(audioRoot, false);
            AudioSource source = audioObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.loop = false;
            return source;
        }

        Transform RentProjectile()
        {
            if (projectilePool.Count > 0)
                return projectilePool.Pop();

            return CreateProjectile();
        }

        void ReturnProjectile(Transform projectile)
        {
            if (projectile == null)
                return;

            projectile.gameObject.SetActive(false);
            projectilePool.Push(projectile);
        }

        Transform RentBurst()
        {
            if (burstPool.Count > 0)
                return burstPool.Pop();

            return CreateBurstFlash();
        }

        void ReturnBurst(Transform burst)
        {
            if (burst == null)
                return;

            burst.gameObject.SetActive(false);
            burstPool.Push(burst);
        }

        AudioSource RentAudioSource()
        {
            if (audioPool.Count > 0)
                return audioPool.Pop();

            return CreateAudioSource();
        }

        void ReturnAudioSource(AudioSource source)
        {
            if (source == null)
                return;

            source.Stop();
            source.clip = null;
            audioPool.Push(source);
        }

        public static CombatFeedbackView Ensure(GameObject parent = null)
        {
            CombatFeedbackView existing = FindAnyObjectByType<CombatFeedbackView>();
            if (existing != null)
                return existing;

            GameObject viewObject = new GameObject("CombatFeedbackView");
            if (parent != null)
                viewObject.transform.SetParent(parent.transform, false);

            return viewObject.AddComponent<CombatFeedbackView>();
        }
    }
}
