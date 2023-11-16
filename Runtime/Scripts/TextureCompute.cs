using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Blep;

namespace Blep.TextureOps {

public class TextureCompute {

    public static readonly ShaderId SrcId = "Src";
    public static readonly ShaderId SrcAId = "SrcA";
    public static readonly ShaderId SrcBId = "SrcB";
    public static readonly ShaderId DstId = "Dst";
    public static readonly ShaderId ScalarAId = "ScalarA";
    public static readonly ShaderId ScalarBId = "ScalarB";
    public static readonly ShaderId ScalarCId = "ScalarC";
    public static readonly ShaderId TextureSizeId = "TextureSize";
    public static readonly ShaderId TexelSizeId = "TexelSize";

    public ComputeShader shader { get; private set; }
    public int width { get; private set; }
    public int height { get; private set; }

    public TextureCompute(string shaderName) {
				shader = Resources.Load<ComputeShader>(shaderName);
        if (shader == null) {
            throw new System.IO.FileNotFoundException(shaderName);
        }
    }

    public int FindKernel(string name) =>
        shader.FindKernel(name);

    public void GetKernelThreadGroupSizes(int kernel, out int xs, out int ys) {
        shader.GetKernelThreadGroupSizes(kernel, out uint uxs, out uint uys, out uint uzs);
        // Dispatch uses uses ints, so convert here
        xs = (int) uxs;
        ys = (int) uys;
    }

    public void GetKernelThreadGroups(int kernel, int width, int height, out int x, out int y) {
        GetKernelThreadGroupSizes(kernel, out int xs, out int ys);
        x = (width  + xs - 1) / xs; // Round up
        y = (height + ys - 1) / ys; // Round up
    }

    public void GetKernelThreadGroups(int kernel, out int x, out int y) =>
        GetKernelThreadGroups(kernel, width, height, out x, out y);

    public void Dispatch(int kernel, int threadGroupsX, int threadGroupsY) =>
        shader.Dispatch(kernel, threadGroupsX, threadGroupsY, 1);

    public void Dispatch(int kernel) {
        GetKernelThreadGroups(kernel, out int x, out int y);
        Dispatch(kernel, x, y);
    }

    public virtual void SetSize(int width, int height) {
        this.width = width;
        this.height = height;
        shader.SetInts(TextureSizeId, new [] {width, height});
        shader.SetVector(TexelSizeId, new Vector2(1.0f / width, 1.0f / height));
    }

    // Finds a RenderTextureFormat compatible with the given format
    public static RenderTextureFormat GetCompatibleRenderTextureFormat(GraphicsFormat format) {
        // Render texture doesn't have a standard 3-channel format, so
        // converting a typical R8G8B8_SRGB format will give an error. Add a
        // alpha channel to make things work. Annoying that there doesn't seem a
        // better way to do this.
        if (GraphicsFormatUtility.GetColorComponentCount(format) == 3) {
            format = GraphicsFormatUtility.ConvertToAlphaFormat(format);
        }
        return GraphicsFormatUtility.GetRenderTextureFormat(format);
    }

    // Creates a RenderTexture with the given size and format
    public static RenderTexture CreateRenderTexture(int width, int height, GraphicsFormat format) {
        var rtFormat = GetCompatibleRenderTextureFormat(format);
        var rt = new RenderTexture(width, height, 0, rtFormat);
        rt.enableRandomWrite = true;
        return rt;
    }

    // Creates a RenderTexture with the same size as the given texture and with the given format
    public static RenderTexture CreateRenderTexture(Texture src, GraphicsFormat format) =>
        CreateRenderTexture(src.width, src.height, format);

    // Creates a RenderTexture with the same size and format as the given texture
    public static RenderTexture CreateRenderTexture(Texture src) =>
        CreateRenderTexture(src, src.graphicsFormat);

    // Creates a RenderTexture with the same size and channels as the given
    // texture and but with a float data type
		public static RenderTexture CreateFloatRenderTexture(Texture src) {
        var format = GraphicsFormatUtility.GetColorComponentCount(src.graphicsFormat) switch {
            1 => GraphicsFormat.R32_SFloat,
            2 => GraphicsFormat.R32G32_SFloat,
            _ => GraphicsFormat.R32G32B32A32_SFloat
        };
        return CreateRenderTexture(src, format);
    }

    // Returns a temporary RenderTexture with the given size and format
    public static RenderTexture GetTemporary(int width, int height, GraphicsFormat format) {
        var rtFormat = GetCompatibleRenderTextureFormat(format);
        var rt = RenderTexture.GetTemporary(width, height, 0, rtFormat);
        rt.enableRandomWrite = true;
        return rt;
    }

    // Returns a temporary RenderTexture with the same size as the given texture
    // and with the given format
    public static RenderTexture GetTemporary(Texture src, GraphicsFormat format) =>
        GetTemporary(src.width, src.height, format);

    // Returns a temporary RenderTexture with the same size and format as the given texture
    public static RenderTexture GetTemporary(Texture src) =>
        GetTemporary(src, src.graphicsFormat);

    // Returns a temporary RenderTexture with the same size and channels as the
    // given texture and but with a float data type.
		public static RenderTexture GetFloatTemporary(Texture src) {
        var format = GraphicsFormatUtility.GetColorComponentCount(src.graphicsFormat) switch {
            1 => GraphicsFormat.R32_SFloat,
            2 => GraphicsFormat.R32G32_SFloat,
            _ => GraphicsFormat.R32G32B32A32_SFloat
        };
        return GetTemporary(src, format);
    }

    // Releases a temporary texture
    public static void ReleaseTemporary(RenderTexture tmp) =>
        RenderTexture.ReleaseTemporary(tmp);


    // -------------------------------------------------------------------------------

		public void BinaryOp(int kernel,
                         Texture srcA, Texture srcB, RenderTexture dst,
                         Vector4? scalarA=null, Vector4? scalarB=null, Vector4? scalarC=null) {

        SetSize(srcA.width, srcA.height);

        shader.SetTexture(kernel, SrcAId, srcA);
        if (srcB != null) shader.SetTexture(kernel, SrcBId, srcB);

        if (scalarA.HasValue) shader.SetVector(ScalarAId, scalarA.Value);
        if (scalarB.HasValue) shader.SetVector(ScalarBId, scalarB.Value);
        if (scalarC.HasValue) shader.SetVector(ScalarCId, scalarC.Value);

        shader.SetTexture(kernel, DstId, dst);

        Dispatch(kernel);
    }

		public void UnaryOp(int kernel,
                        Texture src, RenderTexture dst,
                        Vector4? scalarA=null, Vector4? scalarB=null, Vector4? scalarC=null) =>
				BinaryOp(kernel, src, null, dst, scalarA, scalarB, scalarC);

		public void BinaryOp(string kernelName,
                         Texture srcA, Texture srcB, RenderTexture dst,
                         Vector4? scalarA=null, Vector4? scalarB=null, Vector4? scalarC=null) =>
        BinaryOp(FindKernel(kernelName), srcA, srcB, dst, scalarA, scalarB, scalarC);

		public void UnaryOp(string kernelName,
                        Texture srcA, RenderTexture dst,
                        Vector4? scalarA=null, Vector4? scalarB=null, Vector4? scalarC=null) =>
        UnaryOp(FindKernel(kernelName), srcA, dst, scalarA, scalarB, scalarC);
}

}
