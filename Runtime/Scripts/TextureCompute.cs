using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Blep;

#if ! UNITY_6000
using GraphicsFormatUsage = UnityEngine.Experimental.Rendering.FormatUsage;
#endif

namespace Blep.TextureOps {

public class TextureCompute {

    public static readonly ShaderId SrcId = "Src";
    public static readonly ShaderId SrcAId = "SrcA";
    public static readonly ShaderId SrcBId = "SrcB";
    public static readonly ShaderId DstId = "Dst";
    public static readonly ShaderId ScalarAId = "ScalarA";
    public static readonly ShaderId ScalarBId = "ScalarB";
    public static readonly ShaderId ScalarCId = "ScalarC";
    public static readonly ShaderId ScalarDId = "ScalarD";
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

    public TextureCompute(ComputeShader shader) {
        this.shader = shader;
    }

    public int FindKernel(string name) =>
        shader.FindKernel(name);

    public void GetKernelThreadGroupSizes(int kernel, out int xs, out int ys) {
        shader.GetKernelThreadGroupSizes(kernel, out uint uxs, out uint uys, out uint uzs);
        // Dispatch uses ints, so convert here
        xs = (int) uxs;
        ys = (int) uys;
    }

    // Finds the number of thread groups needed to run the given kernel over an
    // image of the given size.
    public void GetKernelThreadGroups(int kernel, int width, int height, out int x, out int y) {
        GetKernelThreadGroupSizes(kernel, out int xs, out int ys);
        x = (width  + xs - 1) / xs; // Round up
        y = (height + ys - 1) / ys; // Round up
    }

    // Finds the number of thread groups needed to run the given kernel over an
    // image sized this.width x this.height
    public void GetKernelThreadGroups(int kernel, out int x, out int y) =>
        GetKernelThreadGroups(kernel, width, height, out x, out y);

    // Dispatches the kernel with the given group sizes
    public void Dispatch(int kernel, int threadGroupsX, int threadGroupsY) =>
        shader.Dispatch(kernel, threadGroupsX, threadGroupsY, 1);

    // Calculates the needed group sizes and dispatches the kernel.
    public void Dispatch(int kernel) {
        GetKernelThreadGroups(kernel, out int x, out int y);
        Dispatch(kernel, x, y);
    }

    // Sets the TextureSize and TexelSize shader properties and stores the size
    // for use in Dispatch etc.
    public virtual void SetSize(int width, int height) {
        this.width = width;
        this.height = height;
        shader.SetInts(TextureSizeId, new [] {width, height});
        shader.SetVector(TexelSizeId, new Vector2(1.0f / width, 1.0f / height));
    }

    // -------------------------------------------------------------------------------

    // Finds a RenderTextureFormat compatible with the given format
    public static RenderTextureFormat GetCompatibleRenderTextureFormat(GraphicsFormat format) {

        // Convert to a compatible GraphicsFormat for rendering
        var compatFormat = SystemInfo.GetCompatibleFormat(format, GraphicsFormatUsage.Render);

        // Convert to a RenderTextureFormat
        var rtFormat = GraphicsFormatUtility.GetRenderTextureFormat(compatFormat);

        return rtFormat;
    }

    // Creates a RenderTexture with the given size and format
    public static RenderTexture CreateRenderTexture(int width, int height, RenderTextureFormat format) {
        var rt = new RenderTexture(width, height, 0, format, RenderTextureReadWrite.Linear);
        rt.enableRandomWrite = true;
        return rt;
    }

    // Creates a RenderTexture with the given size and format
    public static RenderTexture CreateRenderTexture(int width, int height, GraphicsFormat format) =>
        CreateRenderTexture(width, height, GetCompatibleRenderTextureFormat(format));

    // Creates a RenderTexture with the same size as the given texture and with the given format 
    public static RenderTexture CreateRenderTexture(Texture src, GraphicsFormat format) =>
        CreateRenderTexture(src.width, src.height, format);

    // Creates a RenderTexture with the same size and format as the given texture
    public static RenderTexture CreateRenderTexture(Texture src) =>
        CreateRenderTexture(src, src.graphicsFormat);

    // Creates a RenderTexture with the same size and channels as the given
    // texture and but with a float data type
    public static RenderTexture CreateFloatRenderTexture(Texture src) {
        var format = GraphicsFormatUtility.GetComponentCount(src.graphicsFormat) switch {
            1 => GraphicsFormat.R32_SFloat,
            2 => GraphicsFormat.R32G32_SFloat,
            _ => GraphicsFormat.R32G32B32A32_SFloat
        };
        return CreateRenderTexture(src, format);
    }

    // Returns a temporary RenderTexture with the given size and format
    public static RenderTexture GetTemporary(int width, int height, RenderTextureFormat format) {
        var rt = RenderTexture.GetTemporary(width, height, 0, format,
                                            RenderTextureReadWrite.Linear);
        rt.enableRandomWrite = true;
        return rt;
    }

    // Returns a temporary RenderTexture with the given size and format
    public static RenderTexture GetTemporary(int width, int height, GraphicsFormat format) =>
        GetTemporary(width, height, GetCompatibleRenderTextureFormat(format));

    // Returns a temporary RenderTexture with the same size as the given texture
    // and with the given format
    public static RenderTexture GetTemporary(Texture src, RenderTextureFormat format) =>
        GetTemporary(src.width, src.height, format);

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

    // Sets the appropriate shader properties (SrcA, SrcB, Dst, ScalarA,
    // ScalarB, and ScalarC) and dispatches the kernel
    public void BinaryOp(int kernel,
                         Texture srcA, Texture srcB, RenderTexture dst,
                         Vector4? scalarA=null, Vector4? scalarB=null,
                         Vector4? scalarC=null, Vector4? scalarD=null) {

        SetSize(dst.width, dst.height);

        if (srcA != null) shader.SetTexture(kernel, SrcAId, srcA);
        if (srcB != null) shader.SetTexture(kernel, SrcBId, srcB);

        if (scalarA.HasValue) shader.SetVector(ScalarAId, scalarA.Value);
        if (scalarB.HasValue) shader.SetVector(ScalarBId, scalarB.Value);
        if (scalarC.HasValue) shader.SetVector(ScalarCId, scalarC.Value);
        if (scalarD.HasValue) shader.SetVector(ScalarDId, scalarD.Value);

        shader.SetTexture(kernel, DstId, dst);

        Dispatch(kernel);
    }

    public void BinaryOp(string kernelName,
                         Texture srcA, Texture srcB, RenderTexture dst,
                         Vector4? scalarA=null, Vector4? scalarB=null,
                         Vector4? scalarC=null, Vector4? scalarD=null) =>
        BinaryOp(FindKernel(kernelName), srcA, srcB, dst, scalarA, scalarB, scalarC, scalarD);


    public void BinaryOp(string kernelName, string inplaceKernelName,
                         Texture srcA, Texture srcB, RenderTexture dst,
                         Vector4? scalarA=null, Vector4? scalarB=null,
                         Vector4? scalarC=null, Vector4? scalarD=null) {
        if (srcA != dst) {
            BinaryOp(kernelName, srcA, srcB, dst, scalarA, scalarB, scalarC, scalarD);
        }
        else {
            BinaryOp(inplaceKernelName, null, srcB, dst, scalarA, scalarB, scalarC, scalarD);
        }
    }


    public void UnaryOp(int kernel,
                        Texture src, RenderTexture dst,
                        Vector4? scalarA=null, Vector4? scalarB=null,
                        Vector4? scalarC=null, Vector4? scalarD=null) =>
        BinaryOp(kernel, src, null, dst, scalarA, scalarB, scalarC, scalarD);

    public void UnaryOp(string kernelName,
                        Texture src, RenderTexture dst,
                        Vector4? scalarA=null, Vector4? scalarB=null,
                        Vector4? scalarC=null, Vector4? scalarD=null) =>
        UnaryOp(FindKernel(kernelName), src, dst, scalarA, scalarB, scalarC, scalarD);

    public void UnaryOp(string kernelName, string inplaceKernelName,
                        Texture src, RenderTexture dst,
                        Vector4? scalarA=null, Vector4? scalarB=null,
                        Vector4? scalarC=null, Vector4? scalarD=null) {
        if (src != dst) {
            UnaryOp(kernelName, src, dst, scalarA, scalarB, scalarC, scalarD);
        }
        else {
            UnaryOp(inplaceKernelName, null, dst, scalarA, scalarB, scalarC, scalarD);
        }
    }
}

}
