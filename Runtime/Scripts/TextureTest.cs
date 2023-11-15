using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

namespace Blep.TextureOps {

public class TextureTest : MonoBehaviour {

    public Vector2Int imageSize = new Vector2Int(4096, 4096);
    public GraphicsFormat imageFormat;

    public Texture2D src;
    public RenderTexture dst;

    public int iterations = 1;
    public float startupPause = 1;

    private TextureCompute _compute;
    private Texture2D _src;
    private RenderTexture _dst;
    private RenderTexture _tmp;
    private Texture2D _flush;

    public void Awake() {
        if (src != null) {
            imageSize = new Vector2Int(src.width, src.height);
        }

        _compute = new TextureCompute("Shaders/TextureTest");
        _src = src ?? new Texture2D(imageSize.x, imageSize.y, imageFormat, 0);
        _dst = dst ?? TextureCompute.GetTemporary(src);
        _tmp = TextureCompute.GetTemporary(src);
        _flush = new Texture2D(1, 1, _dst.graphicsFormat, 0);
        TextureMath.Set(_dst, Vector4.zero);
        Debug.Log($"Done");
    }

    public IEnumerator Start() {
        // Wait for startup
        yield return new WaitForSeconds(startupPause);
        //TestAccessSpeed();
        TestReduce();
    }

    // -------------------------------------------------------------------------------

    private void _Flush() {
        // TODO: better way to flush the queue? GL.FLush doesn't do it.
        RenderTexture.active = _dst;
        _flush.ReadPixels(new Rect(0, 0, 1, 1), 0, 0, false);
        RenderTexture.active = null;
    }

    public void TestReduce() {
        for (int i = 0; i < iterations; i++) {
            TextureMath.Set(_dst, Vector4.one * 0.5f);
            var pos = new Vector2(Random.Range(0, imageSize.x),
                                  Random.Range(0, imageSize.y));
            TextureDraw.Circle(_dst, new Vector4(0.3f, 0.8f, 0.5f, 1.0f), pos, 2, 0);
            var min = TextureIP.Min(_dst);
            var max = TextureIP.Max(_dst);
            if (min.x != 0.3f) Debug.Log($"Bad min {min} {pos}");
            if (max.y != 0.8f) Debug.Log($"Bad max {max} {pos}");
        }
    }

    private float _Measure(string name, Action action) {
        action();
        _Flush();
        var clock = new Stopwatch();
        clock.Start();
        for (int i = iterations; --i >= 0; ) {
            action();
        }
        _Flush();
        clock.Stop();

        var elapsed = (float) clock.Elapsed.TotalSeconds / iterations;
        UnityEngine.Debug.Log($"{name}:\t{Mathf.RoundToInt(1.0f/elapsed)}Hz\t{elapsed}s");
        return elapsed;
    }

    private void _MeasureKernel(string kernelName) {
        int kernel = _compute.FindKernel(kernelName);
        _compute.GetKernelThreadGroupSizes(kernel, out int xs, out int ys);
        int tgx = (_src.width  + xs - 1) / xs; // Round up
        int tgy = (_src.height + ys - 1) / ys; // Round up

        _compute.SetSize(_src.width, _src.height);
        _Measure(kernelName, () => {
            _compute.shader.SetTexture(kernel, TextureCompute.SrcAId, _src);
            _compute.shader.SetTexture(kernel, TextureCompute.DstId, _dst);
            _compute.Dispatch(kernel, tgx, tgy);
        });
    }

    public void TestAccessSpeed() {
        _MeasureKernel("TestIndex");
        _MeasureKernel("TestLoad");
        _MeasureKernel("TestLoadOffset");
        _MeasureKernel("TestLoadOffsetClamped");
    }
}

}
