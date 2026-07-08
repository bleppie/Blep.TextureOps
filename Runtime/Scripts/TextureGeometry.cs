using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Blep.TextureOps {

public static class TextureGeometry {

    public static readonly ShaderId MatId = "Mat";
    public static readonly ShaderId SrcATextureSizeId = "SrcATextureSize";
    public static readonly ShaderId SrcATexelSizeId = "SrcATexelSize";

    // For convenience
    public static readonly int SrcAId = TextureCompute.SrcAId;
    public static readonly int SrcBId = TextureCompute.SrcBId;
    public static readonly int ScalarAId = TextureCompute.ScalarAId;
    public static readonly int ScalarBId = TextureCompute.ScalarBId;
    public static readonly int DstId = TextureCompute.DstId;

    private static TextureCompute _compute;
    public static TextureCompute compute =>
        (_compute = _compute ?? new TextureCompute("Shaders/Blep/TextureGeometry"));

    public static int WarpKernel = -1;
    public static int WarpColorBorderKernel = -1;
    public static int WarpTransparentBorderKernel = -1;

    // Reset statics while in the editor and play-mode-options are turned on
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void _Init() {
        _compute = null;
        WarpKernel = -1; // Trigger reinit
    }

    // -------------------------------------------------------------------------------

    public enum WrapMode {
        Repeat = TextureWrapMode.Repeat,
        Clamp =  TextureWrapMode.Clamp,
        Mirror = TextureWrapMode.Mirror,
        MirrorOnce = TextureWrapMode.MirrorOnce,
        Color = TextureWrapMode.MirrorOnce + 1,
        Transparent = TextureWrapMode.MirrorOnce + 2
    };

    public static float2x3 GetAffineMatrix(float2 center,
                                           float2 scale,
                                           float  angle,
                                           float2 translation) {
        float cx = center.x;
        float cy = center.y;
        float sx = scale.x;
        float sy = scale.y;
        float sinA = sin(angle);
        float cosA = cos(angle);
        float tx = translation.x;
        float ty = translation.y;

        return float2x3
            (sx * cosA, -sy * sinA, cx - cx * cosA + cy * sinA + tx,
             sx * sinA,  sy * cosA, cy - cx * sinA - cy * cosA + ty);
    }

    public static float2x3 GetInverseAffineMatrix(float2 center,
                                                  float2 scale,
                                                  float  angle,
                                                  float2 translation) {
        float cx = center.x;
        float cy = center.y;
        float sx = scale.x;
        float sy = scale.y;
        float sinA = sin(-angle);
        float cosA = cos(-angle);
        float tx = translation.x;
        float ty = translation.y;

        float a =  cosA / sx;
        float b =  sinA / sx;
        float c = -sinA / sy;
        float d =  cosA / sy;
        float e = cx - (cosA * (tx + cx) + sinA * (ty + cy)) / sx;
        float f = cy + (sinA * (tx + cx) - cosA * (ty + cy)) / sy;

        return float2x3(a, b, e,
                        c, d, f);
    }

    // From old Healing/math/MVecMat.cpp
    public static float4x4 GetWarpCornersMatrix(float2[] src, float2[] dst) {
        float4x4 warpToSquare   = GetWarpToSquareMatrix(src);
        float4x4 warpFromSquare = GetWarpFromSquareMatrix(dst);

        // Premultiply
        return mul(warpFromSquare, warpToSquare);
    }

    public static float4x4 GetInverseWarpCornersMatrix(float2[] src, float2[] dst) =>
        GetWarpCornersMatrix(dst, src);

    public static float4x4 GetWarpFromSquareMatrix(float2[] dst) {
        float x0 = dst[0].x,	y0 = dst[0].y;
        float x1 = dst[1].x,	y1 = dst[1].y;
        float x2 = dst[2].x,	y2 = dst[2].y;
        float x3 = dst[3].x,	y3 = dst[3].y;

        float dx1 = x1 - x2, 	dy1 = y1 - y2;
        float dx2 = x3 - x2, 	dy2 = y3 - y2;
        float sx = x0 - x1 + x2 - x3;
        float sy = y0 - y1 + y2 - y3;
        float g = (sx * dy2 - dx2 * sy) / (dx1 * dy2 - dx2 * dy1);
        float h = (dx1 * sy - sx * dy1) / (dx1 * dy2 - dx2 * dy1);
        float a = x1 - x0 + g * x1;
        float b = x3 - x0 + h * x3;
        float c = x0;
        float d = y1 - y0 + g * y1;
        float e = y3 - y0 + h * y3;
        float f = y0;

        return float4x4(a, b, 0, c,
                        d, e, 0, f,
                        0, 0, 1, 0,
                        g, h, 0, 1);
    }

    public static float4x4 GetWarpToSquareMatrix(float2[] src) {
        float4x4 m = GetWarpFromSquareMatrix(src);

        // Invert through adjoint

        float a = m[0][0],	d = m[0][1],	/* ignore */		g = m[0][3];
        float b = m[1][0],	e = m[1][1],	/* 3rd col*/		h = m[1][3];
        /* ignore 3rd row */
        float c = m[3][0],	f = m[3][1];

        float A =     e - f * h;
        float B = c * h - b;
        float C = b * f - c * e;
        float D = f * g - d;
        float E =     a - c * g;
        float F = c * d - a * f;
        float G = d * h - e * g;
        float H = b * g - a * h;
        float I = a * e - b * d;

        // Probably unnecessary since 'I' is also scaled by the determinant,
        //   and 'I' scales the homogeneous coordinate, which, in turn,
        //   scales the X,Y coordinates.
        // Determinant  =   a * (e - f * h) + b * (f * g - d) + c * (d * h - e * g);
        float idet = 1.0f / (a * A           + b * D           + c * G);

        return float4x4(A * idet,  B * idet,  0,  C * idet,
                        D * idet,  E * idet,  0,  F * idet,
                        0       ,  0       ,  1,  0       ,
                        G * idet,  H * idet,  0,  I * idet);
    }

    private static void _InitKernels() {
        if (WarpKernel == -1) {
            WarpKernel = compute.FindKernel("Warp");
            WarpColorBorderKernel = compute.FindKernel("WarpColorBorder");
            WarpTransparentBorderKernel = compute.FindKernel("WarpTransparentBorder");
        }
    }

    public static void WarpAffine(Texture src, RenderTexture dst,
                                  float2x3 invMat,
                                  FilterMode filterMode=FilterMode.Bilinear,
                                  WrapMode wrapMode=WrapMode.Transparent,
                                  float4? borderColor=null) {
        // Convert to a 4x4 matrix
        var invMat4x4 = float4x4(float4(invMat.c0, 0, 0),
                                 float4(invMat.c1, 0, 0),
                                 float4(0, 0, 1, 0),
                                 float4(invMat.c2, 0, 1));
        Warp(src, dst, invMat4x4, false,
             filterMode, wrapMode, borderColor);
    }

    public static void WarpPerspective(Texture src, RenderTexture dst,
                                       float4x4 invMat,
                                       FilterMode filterMode=FilterMode.Bilinear,
                                       WrapMode wrapMode=WrapMode.Transparent,
                                       float4? borderColor=null) {
        // Convert to a 4x4 matrix
        // var invMat4x4 = float4x4(float4(invMat.c0, 0),
        //                          float4(invMat.c1, 0),
        //                          float4(invMat.c2, 0),
        //                          float4(0, 0, 0, 1));
        Warp(src, dst, invMat, true,
             filterMode, wrapMode, borderColor);
    }


    public static void Warp(Texture src, RenderTexture dst,
                            float4x4 invMat, bool isPersective,
                            FilterMode filterMode,
                            WrapMode wrapMode,
                            float4? borderColor=null) {

        if (isPersective)
            compute.shader.EnableKeyword("TRANSFORM_PERSPECTIVE");
        else
            compute.shader.DisableKeyword("TRANSFORM_PERSPECTIVE");

        // Get kernel
        _InitKernels();
        var kernel = WarpKernel;
        if (wrapMode == WrapMode.Color) {
            wrapMode = WrapMode.Clamp;
            kernel = WarpColorBorderKernel;
        }
        else if (wrapMode == WrapMode.Transparent) {
            wrapMode = WrapMode.Clamp;
            kernel = WarpTransparentBorderKernel;
        }

        // No way to set a ComputeShader sampler, so temporarily set filterMode
        // and wrapMode on the texture. The sample is available in the shader as
        // samplerSrcA.
        var oldFilterMode = src.filterMode;
        var oldWrapMode = src.wrapMode;
        src.filterMode = filterMode;
        src.wrapMode = (TextureWrapMode) wrapMode;

        var shader = compute.shader;

        // Dst size
        compute.SetSize(dst.width, dst.height);

        // Src Size (need this to handle border correctly)
        shader.SetInts(SrcATextureSizeId, new [] {src.width, src.height});
        shader.SetVector(SrcATexelSizeId, new Vector2(1.0f / src.width, 1.0f / src.height));

        shader.SetTexture(kernel, SrcAId, src);
        shader.SetTexture(kernel, DstId, dst);
        shader.SetVector(ScalarAId, borderColor ?? 0);
        shader.SetMatrix(MatId, invMat);

        compute.Dispatch(kernel);

        // Restore filterMode
        src.filterMode = oldFilterMode;
        src.wrapMode = oldWrapMode;
    }
}

}
