using UnityEngine;

namespace FastNoise2Graph.Examples {
  [ExecuteInEditMode]
  [RequireComponent(typeof(MeshFilter))]
  [RequireComponent(typeof(MeshRenderer))]
  public class NoiseTreePreview : MonoBehaviour {
    [Header("Position")]
    public Vector2Int offset;

    [Header("Noise")]
    public int resolution = 256;
    public float scale = 50f;
    public NoiseTree noiseTree;

    [Header("Noise Output")]
    public bool useThreshold;
    public float threshold = 0.5f;

    [Header("Debug")]
    public bool debugTime;

    private MeshRenderer m_meshRenderer;
    private Material material;
    private Texture2D m_texture2d;

    private void Awake() {
      m_meshRenderer = GetComponent<MeshRenderer>();

      material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
      material.hideFlags = HideFlags.DontSave;
      m_meshRenderer.sharedMaterial = material;

      m_texture2d = new Texture2D(resolution, resolution);
      m_texture2d.hideFlags = HideFlags.DontSave;
    }

    private void OnDestroy() {
      DestroyImmediate(material);
      DestroyImmediate(m_texture2d);
    }

    private void Start() {
      Generate();
    }

    public void AssignHeightmap(float[] heightmap) {
      // Resize the texture if necessary
      if (m_texture2d.width != resolution || m_texture2d.height != resolution) {
        m_texture2d.Reinitialize(resolution, resolution);
      }

      // Convert the heightmap into an array of colors
      Color[] colors = new Color[resolution * resolution];
      for (int i = 0; i < colors.Length; i++) {
        float value = heightmap[i];
        Color finalColor = new Color(value, value, value, 1f);
        colors[i] = finalColor;
      }

      // Set the pixels
      m_texture2d.SetPixels(colors);
      m_texture2d.Apply();
      material.SetTexture("_BaseMap", m_texture2d);
    }

    public float[] GetNoise() {
      FastNoise generator = noiseTree?.GetFastNoise();

      if (generator == null) {
        return null;
      }

      // Generate noise texture
      float[] heightmap = new float[resolution * resolution];
      generator.GenUniformGrid2D(heightmap, offset.x, offset.y, resolution, resolution, 1f / scale, 0);

      for (int i = 0; i < heightmap.Length; i++) {
        float value = heightmap[i];

        // Normalize
        value = (value + 1f) / 2f;

        // Threshold
        if (useThreshold) {
          value = value >= threshold ? 1f : 0f;
        }

        // Save value
        heightmap[i] = value;
      }

      return heightmap;
    }

    public void Generate() {
      float[] heightmap = GetNoise();

      // Use a black heightmap as fallback
      if (heightmap == null) {
        heightmap = new float[resolution * resolution];
      }

      AssignHeightmap(heightmap);
    }

    private void OnValidate() {
      if (m_texture2d != null) {
        Generate();
      }
    }

    [ContextMenu("Performance Test")]
    private void PerformanceTest() {
      if (noiseTree == null) {
        Debug.LogWarning("noiseTree is null");
        return;
      }

      System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
      stopwatch.Start();

      FastNoise noise = noiseTree.GetFastNoise();

      stopwatch.Stop();
      Debug.Log($"FastNoise instance generated in: {stopwatch.Elapsed.TotalMilliseconds} ms");

      stopwatch.Restart();

      float[] heightmap = new float[resolution * resolution];
      noise.GenUniformGrid2D(heightmap, offset.x, offset.y, resolution, resolution, 1f / scale, 0);

      stopwatch.Stop();
      Debug.Log($"Noise of {resolution} x {resolution} ({resolution * resolution} pixels) generated in: {stopwatch.Elapsed.TotalMilliseconds} ms");
    }
  }
}