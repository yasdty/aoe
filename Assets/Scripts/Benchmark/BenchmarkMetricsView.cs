using Unity.Profiling;
using UnityEngine;

namespace AoE.RTS.Benchmark
{
    public class BenchmarkMetricsView : MonoBehaviour
    {
        static readonly int[] Presets = { 50, 100, 200, 500, 800 };

        const int FrameSampleCount = 30;
        const float Margin = 12f;
        const float PanelWidth = 520f;
        const float LineHeight = 22f;
        const float Padding = 8f;
        const float ButtonWidth = 56f;
        const float ButtonHeight = 24f;
        const float ButtonGap = 6f;

        [SerializeField] BenchmarkSpawner spawner;

        readonly float[] frameTimeSamples = new float[FrameSampleCount];

        ProfilerRecorder gcRecorder;
        bool gcRecorderValid;
        int frameSampleIndex;
        int frameSampleCount;
        float smoothedFrameTimeMs;
        float smoothedFps;

        void OnEnable()
        {
            gcRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
            gcRecorderValid = gcRecorder.Valid;
            if (!gcRecorderValid)
            {
                gcRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC.Alloc");
                gcRecorderValid = gcRecorder.Valid;
            }
        }

        void OnDisable()
        {
            if (gcRecorderValid)
                gcRecorder.Dispose();
        }

        void Update()
        {
            float deltaTime = Time.unscaledDeltaTime;
            frameTimeSamples[frameSampleIndex % FrameSampleCount] = deltaTime;
            frameSampleIndex++;
            frameSampleCount = Mathf.Min(frameSampleIndex, FrameSampleCount);

            float total = 0f;
            for (int i = 0; i < frameSampleCount; i++)
                total += frameTimeSamples[i];

            float averageDelta = frameSampleCount > 0 ? total / frameSampleCount : deltaTime;
            smoothedFrameTimeMs = averageDelta * 1000f;
            smoothedFps = averageDelta > 0f ? 1f / averageDelta : 0f;
        }

        void OnGUI()
        {
            if (spawner == null)
                return;

            int unitCount = spawner.ActiveUnitCount;
            float gcKb = gcRecorderValid ? gcRecorder.LastValue / 1024f : 0f;

            float panelHeight = Padding * 2f + LineHeight * 3f + ButtonHeight + Padding;
            Rect panelRect = new Rect(Margin, Margin, PanelWidth, panelHeight);
            GUI.Box(panelRect, GUIContent.none);

            float x = Margin + Padding;
            float y = Margin + Padding;

            DrawLine(x, ref y, $"FPS: {smoothedFps:0.0}  |  Frame: {smoothedFrameTimeMs:0.1} ms  |  GC/frame: {gcKb:0.1} KB");
            DrawLine(x, ref y, $"Units: {unitCount}  |  Presets: click to spawn (Idle)");

            y += 4f;
            DrawPresetButtons(x, y);
        }

        void DrawPresetButtons(float x, float y)
        {
            for (int i = 0; i < Presets.Length; i++)
            {
                int preset = Presets[i];
                float buttonX = x + i * (ButtonWidth + ButtonGap);
                Rect buttonRect = new Rect(buttonX, y, ButtonWidth, ButtonHeight);
                if (GUI.Button(buttonRect, preset.ToString()))
                    spawner.SpawnCount(preset);
            }

            float clearX = x + Presets.Length * (ButtonWidth + ButtonGap);
            Rect clearRect = new Rect(clearX, y, ButtonWidth + 12f, ButtonHeight);
            if (GUI.Button(clearRect, "Clear"))
                spawner.ClearAll();
        }

        static void DrawLine(float x, ref float y, string text)
        {
            Rect rect = new Rect(x, y, PanelWidth - Padding * 2f, LineHeight);
            GUI.Label(rect, text);
            y += LineHeight;
        }
    }
}
