using System;
using System.Linq;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

#if ! UNITY_6000
using GraphicsFormatUsage = UnityEngine.Experimental.Rendering.FormatUsage;
#endif

namespace Blep.TextureOps {

public class TextureTest : MonoBehaviour {

    public Vector2Int imageSize = new Vector2Int(4096, 4096);
    public GraphicsFormat imageFormat;

    public Texture2D srcIn;
    public RenderTexture dstOut;

    public int iterations = 1;
    public float startupPause = 1;
    public float size = 1;
    public float rangeSigma = 1;

    private TextureCompute _compute;
    private Texture2D _src;
    private RenderTexture _dst;
    private RenderTexture _tmp;
    private Texture2D _flush;
    public int width => _src.width;
    public int height => _src.height;

    public void Awake() {
        if (srcIn != null) {
            imageSize = new Vector2Int(srcIn.width, srcIn.height);
        }

        _compute = new TextureCompute("Shaders/Blep/TextureTest");
        _src = srcIn != null ? srcIn : new Texture2D(imageSize.x, imageSize.y, imageFormat, 0);
        _dst = dstOut != null ? dstOut : TextureCompute.GetTemporary(_src);
        _tmp = TextureCompute.GetTemporary(_src);
        _flush = new Texture2D(1, 1, _dst.graphicsFormat, 0);
    }

    public IEnumerator Start() {
        TextureMath.Set(_dst, Vector4.zero);
        Debug.Log($"Starting");
        // Wait for startup
        yield return new WaitForSeconds(startupPause);
        //TestAccessSpeed();
        //TestReduce();
        //TestFormats();
        // TestGatherSpeed();

        while (true) {
            TextureIP.Bilateral(srcIn, dstOut, size, -1, rangeSigma);
            yield return null;
        }

        //        Debug.Log($"Done");
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
//            TextureDraw.Circle(_dst, new Vector4(0.3f, 0.8f, 0.5f, 1.0f), pos, 2, 0);
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
        UnityEngine.Debug.Log($"{name}:\t{Mathf.RoundToInt(1.0f/elapsed)} Hz\t{elapsed} s");
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

    public void TestGatherSpeed() {
        _Measure("Erode Normal", () => { TextureIP.Erode(_src, _dst); });
        _Measure("Erode Gather", () => { TextureIP.ErodeGather(_src, _dst); });
    }
    public void TestAccessSpeed() {
        _MeasureKernel("TestIndex");
        _MeasureKernel("TestLoad");
        _MeasureKernel("TestLoadOffset");
        _MeasureKernel("TestLoadOffsetClamped");
    }

    public void TestFormats() {
        foreach (TextureFormat tFormat in Enum.GetValues(typeof(TextureFormat))) {
            var gFormat = GraphicsFormatUtility.GetGraphicsFormat(tFormat, false);
            if ((int) gFormat < 0)
                Debug.Log($"{tFormat} => BAD {gFormat}");
            else {
                var gFormatCompat = SystemInfo.GetCompatibleFormat(gFormat, GraphicsFormatUsage.Render);
                var rtFormat = GraphicsFormatUtility.GetRenderTextureFormat(gFormatCompat);
                if (! Enum.IsDefined(typeof(RenderTextureFormat), rtFormat))
                    Debug.Log($"{tFormat} \t => {gFormat} \t => {gFormatCompat} \t = \t <= BAD {rtFormat}");
            }
        }
    }
}
}
