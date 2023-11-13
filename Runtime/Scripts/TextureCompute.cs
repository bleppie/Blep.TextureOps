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
        // Dispatch uses uses ints, so convert here
        shader.GetKernelThreadGroupSizes(kernel, out uint uxs, out uint uys, out uint uzs);
        xs = (int) uxs;
        ys = (int) uys;
    }

    public void GetKernelThreadGroups(int kernel, out int x, out int y) {
        GetKernelThreadGroupSizes(kernel, out int xs, out int ys);
        x = (width  + xs - 1) / xs; // Round up
        y = (height + ys - 1) / ys; // Round up
    }

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

    public static RenderTexture CreateRenderTexture(int width, int height, GraphicsFormat format) {
        // Not all graphics formats are supported, so convert to RTFormat
        var rtFormat = GraphicsFormatUtility.GetRenderTextureFormat(format);
        // TODO/BUG: Unity spits out "RenderTexture color format cannot be set to a depth/stencil format"
        //           even though depth/stencil is set to 0
        var rt = new RenderTexture(width, height, 0, rtFormat);
        rt.enableRandomWrite = true;
        return rt;
    }

		public static RenderTexture CreateRenderTexture(Texture src, GraphicsFormat format) =>
        CreateRenderTexture(src.width, src.height, format);

		public static RenderTexture CreateRenderTexture(Texture src) =>
        CreateRenderTexture(src.width, src.height, src.graphicsFormat);

		public static RenderTexture CreateFloatRenderTexture(Texture src) {
        var format = GraphicsFormatUtility.GetColorComponentCount(src.graphicsFormat) switch {
            1 => GraphicsFormat.R32_SFloat,
            2 => GraphicsFormat.R32G32_SFloat,
            _ => GraphicsFormat.R32G32B32A32_SFloat
        };
        return CreateRenderTexture(src, format);
    }

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
