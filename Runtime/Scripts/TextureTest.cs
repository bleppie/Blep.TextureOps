using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Blep.TextureOps {

public class TextureTest : MonoBehaviour {

    public Vector2Int imageSize;
    public GraphicsFormat imageFormat;

    public Texture2D src;
    public RenderTexture dst;

    public int iterations;
    public float startupPause = 2;

    private TextureCompute _compute;
    private Texture2D _src;
    private RenderTexture _dst;
    private RenderTexture _tmp;
    private Texture2D _flush;

    public void Awake() {
        _compute = new TextureCompute("Shaders/TextureTest");
        _src = src ?? new Texture2D(imageSize.x, imageSize.y, imageFormat, 0);
        _dst = dst ?? TextureCompute.CreateRenderTexture(_src);
        _tmp = new RenderTexture(dst);
        _flush = new Texture2D(_dst.width, _dst.height, _dst.graphicsFormat, 0);
        TextureMath.Set(_dst, Vector4.zero);
    }

    public IEnumerator Start() {
        // Wait for startup
        yield return new WaitForSeconds(startupPause);
        TestAccessSpeed();
    }

    // -------------------------------------------------------------------------------

    private void _Flush() {
        // TODO: better way to flush the queue? GL.FLush doesn't do it.
        RenderTexture.active = _dst;
        _flush.ReadPixels(new Rect(0, 0, _flush.width, _flush.height), 0, 0, false);
        RenderTexture.active = null;
    }

    private float _TestFlushSpeed() {
        _Flush();
        var clock = new Stopwatch();
        clock.Start();
        for (int i = 10; --i >= 0; ) {
            _Flush();
        }
        clock.Stop();

        var elapsed = (float) clock.Elapsed.TotalSeconds / 10;
        UnityEngine.Debug.Log($"Flush: {Mathf.RoundToInt(1.0f/elapsed)} Hz");
        return elapsed;
    }

    private double _TestAccessSpeed(string kernelName) {
        int kernel = _compute.FindKernel(kernelName);
        _compute.GetKernelThreadGroupSizes(kernel, out int xs, out int ys);
        int tgx = (_src.width  + xs - 1) / xs; // Round up
        int tgy = (_src.height + ys - 1) / ys; // Round up

        UnityEngine.Debug.Log($"Size {_src.width}x{_src.height}, ThreadGroups {tgx}x{tgy}");

        _compute.SetSize(_src.width, _src.height);
        _compute.shader.SetTexture(kernel, TextureCompute.SrcAId, _src);
        _compute.shader.SetTexture(kernel, TextureCompute.DstId, _dst);
        _compute.Dispatch(kernel, tgx, tgy);
        _Flush();

        var clock = new Stopwatch();
        clock.Start();
        for (int i = iterations / 2; --i >= 0; ) {
            _compute.shader.SetTexture(kernel, TextureCompute.SrcAId, _dst);
            _compute.shader.SetTexture(kernel, TextureCompute.DstId, _tmp);
            _compute.Dispatch(kernel, tgx, tgy);
            _compute.shader.SetTexture(kernel, TextureCompute.SrcAId, _tmp);
            _compute.shader.SetTexture(kernel, TextureCompute.DstId, _dst);
            _compute.Dispatch(kernel, tgx, tgy);
        }
        _Flush();
        clock.Stop();

        var elapsed = ((float) clock.Elapsed.TotalSeconds) / iterations;
        UnityEngine.Debug.Log($"{kernelName}:\t{Mathf.RoundToInt(1.0f/elapsed)}Hz\t{clock.Elapsed.TotalSeconds}s");
        return elapsed;
    }

    public void TestAccessSpeed() {
        _TestFlushSpeed();
        _TestAccessSpeed("TestIndex");
        _TestAccessSpeed("TestLoad");
        _TestAccessSpeed("TestLoadOffset");
        _TestAccessSpeed("TestLoadOffsetClamped");
    }
}

}
