//--------------------------------------------------------------------------------------------------------------------------------
// Cartoon FX
// (c) 2012-2020 Jean Moreno
//--------------------------------------------------------------------------------------------------------------------------------

// Use the defines below to globally disable features:
#define DISABLE_CAMERA_SHAKE
//#define DISABLE_LIGHTS
//#define DISABLE_CLEAR_BEHAVIOR

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CartoonFX
{
    [RequireComponent(typeof(ParticleSystem))]
    [DisallowMultipleComponent]
    public partial class CFXR_Effect : MonoBehaviour
    {
        const float GLOBAL_CAMERA_SHAKE_MULTIPLIER = 1.0f;

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        static void InitGlobalOptions()
        {
            AnimatedLight.editorPreview = EditorPrefs.GetBool("CFXR Light EditorPreview", true);
        }
#endif

        public enum ClearBehavior
        {
            None,
            Disable,
            Destroy
        }

        [System.Serializable]
        public class AnimatedLight
        {
            static public bool editorPreview = true;
            public Light light;
            public bool loop;
            public bool animateIntensity;
            public float intensityStart = 8f;
            public float intensityEnd = 0f;
            public float intensityDuration = 0.5f;
            public AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
            public bool perlinIntensity;
            public float perlinIntensitySpeed = 1f;
            public bool fadeIn;
            public float fadeInDuration = 0.5f;
            public bool fadeOut;
            public float fadeOutDuration = 0.5f;

            public bool animateRange;
            public float rangeStart = 8f;
            public float rangeEnd = 0f;
            public float rangeDuration = 0.5f;
            public AnimationCurve rangeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
            public bool perlinRange;
            public float perlinRangeSpeed = 1f;

            public bool animateColor;
            public Gradient colorGradient;
            public float colorDuration = 0.5f;
            public AnimationCurve colorCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
            public bool perlinColor;
            public float perlinColorSpeed = 1f;

            public void animate(float time)
            {
#if UNITY_EDITOR
                if (!editorPreview && !EditorApplication.isPlaying) return;
#endif
                if (light != null)
                {
                    if (animateIntensity)
                    {
                        float delta = loop ? Mathf.Clamp01((time % intensityDuration) / intensityDuration) : Mathf.Clamp01(time / intensityDuration);
                        delta = perlinIntensity ? Mathf.PerlinNoise(Time.time * perlinIntensitySpeed, 0f) : intensityCurve.Evaluate(delta);
                        light.intensity = Mathf.LerpUnclamped(intensityEnd, intensityStart, delta);
                        if (fadeIn && time < fadeInDuration) light.intensity *= Mathf.Clamp01(time / fadeInDuration);
                    }
                    if (animateRange)
                    {
                        float delta = loop ? Mathf.Clamp01((time % rangeDuration) / rangeDuration) : Mathf.Clamp01(time / rangeDuration);
                        delta = perlinRange ? Mathf.PerlinNoise(Time.time * perlinRangeSpeed, 10f) : rangeCurve.Evaluate(delta);
                        light.range = Mathf.LerpUnclamped(rangeEnd, rangeStart, delta);
                    }
                    if (animateColor)
                    {
                        float delta = loop ? Mathf.Clamp01((time % colorDuration) / colorDuration) : Mathf.Clamp01(time / colorDuration);
                        delta = perlinColor ? Mathf.PerlinNoise(Time.time * perlinColorSpeed, 0f) : colorCurve.Evaluate(delta);
                        light.color = colorGradient.Evaluate(delta);
                    }
                }
            }

            public void animateFadeOut(float time)
            {
                if (fadeOut && light != null)
                    light.intensity *= 1.0f - Mathf.Clamp01(time / fadeOutDuration);
            }

            public void reset()
            {
                if (light == null) return;
                if (animateIntensity) light.intensity = (fadeIn || fadeOut) ? 0 : intensityEnd;
                if (animateRange) light.range = rangeEnd;
                if (animateColor) light.color = colorGradient.Evaluate(1f);
            }
        }

        public static bool GlobalDisableCameraShake;
        public static bool GlobalDisableLights;
        public ClearBehavior clearBehavior = ClearBehavior.Destroy;
        public AnimatedLight[] animatedLights;
        public ParticleSystem fadeOutReference;

        float time;
        ParticleSystem rootParticleSystem;
        [System.NonSerialized] MaterialPropertyBlock materialPropertyBlock;
        [System.NonSerialized] Renderer particleRenderer;

        public void ResetState()
        {
            time = 0f;
            fadingOutStartTime = 0f;
            isFadingOut = false;
#if !DISABLE_LIGHTS
            if (animatedLights != null)
                foreach (var animLight in animatedLights)
                    animLight.reset();
#endif
        }

#if !DISABLE_CAMERA_SHAKE || !DISABLE_CLEAR_BEHAVIOR
        void Awake()
        {
#if !DISABLE_CLEAR_BEHAVIOR
            startFrameOffset = GlobalStartFrameOffset++;
#endif
            particleRenderer = this.GetComponent<ParticleSystemRenderer>();
            if (particleRenderer.sharedMaterial != null && particleRenderer.sharedMaterial.IsKeywordEnabled("_CFXR_LIGHTING_WPOS_OFFSET"))
                materialPropertyBlock = new MaterialPropertyBlock();
        }
#endif

        void OnEnable()
        {
            foreach (var animLight in animatedLights)
                if (animLight.light != null)
#if !DISABLE_LIGHTS
                    animLight.light.enabled = !GlobalDisableLights;
#else
					animLight.light.enabled = false;
#endif
        }

        void OnDisable() => ResetState();

#if !DISABLE_LIGHTS || !DISABLE_CLEAR_BEHAVIOR
        const int CHECK_EVERY_N_FRAME = 20;
        static int GlobalStartFrameOffset = 0;
        int startFrameOffset;
        void Update()
        {
#if !DISABLE_LIGHTS
            time += Time.deltaTime;
            if (animatedLights != null && !GlobalDisableLights)
                foreach (var animLight in animatedLights)
                    animLight.animate(time);
            if (fadeOutReference != null && !fadeOutReference.isEmitting && (fadeOutReference.isPlaying || isFadingOut))
                FadeOut(time);
#endif
#if !DISABLE_CLEAR_BEHAVIOR
            if (clearBehavior != ClearBehavior.None)
            {
                if (rootParticleSystem == null) rootParticleSystem = this.GetComponent<ParticleSystem>();
                if ((Time.renderedFrameCount + startFrameOffset) % CHECK_EVERY_N_FRAME == 0)
                {
                    if (!rootParticleSystem.IsAlive(true))
                        if (clearBehavior == ClearBehavior.Destroy)
                            Destroy(this.gameObject);
                        else
                            this.gameObject.SetActive(false);
                }
            }
#endif
            if (materialPropertyBlock != null)
            {
                particleRenderer.GetPropertyBlock(materialPropertyBlock);
                materialPropertyBlock.SetVector("_GameObjectWorldPosition", this.transform.position);
                particleRenderer.SetPropertyBlock(materialPropertyBlock);
            }
        }
#endif

#if !DISABLE_LIGHTS
        bool isFadingOut;
        float fadingOutStartTime;
        public void FadeOut(float time)
        {
            if (animatedLights == null) return;
            if (!isFadingOut)
            {
                isFadingOut = true;
                fadingOutStartTime = time;
            }
            foreach (var animLight in animatedLights)
                animLight.animateFadeOut(time - fadingOutStartTime);
        }
#endif
    }
}
