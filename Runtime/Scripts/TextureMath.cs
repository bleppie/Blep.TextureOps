using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Blep.TextureOps {

public static class TextureMath {

    private static TextureCompute _compute;
    public static TextureCompute compute =>
        (_compute = _compute ?? new TextureCompute("Shaders/Blep/TextureMath"));

    // Reset statics while in the editor and play-mode-options are turned on
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void _Init() { _compute = null; }

    // -------------------------------------------------------------------------------

    // Src = Dst, use fast CopyTexture if possible, otherwise Blit
    public static void Copy(Texture src, RenderTexture dst) {
        if (src.width == src.height &&
            dst.width == dst.height &&
            src.graphicsFormat == dst.graphicsFormat) {
            Graphics.CopyTexture(src, dst);
        }
        else {
            Graphics.Blit(src, dst);
        }
    }

    // Dst = 0
    public static void Clear(RenderTexture dst) =>
        Set(dst, 0.0f);

    // Dst = Value
    public static void Set(RenderTexture dst, float4 value) =>
        compute.UnaryOp("SetC", null, dst, value);

    // Dst = Value with channel mask
    public static void Set(RenderTexture dst, float4 value, float4 channelMask) =>
        MultiplyAdd(dst, 1 - channelMask, value * channelMask);

    // Dst = Value with image mask
    public static void Set(RenderTexture dst, float4 value, Texture mask) =>
        compute.BinaryOp("SetCMaskedI", null, mask, dst, value);


    // Dst = 1 - Src
    public static void Invert(Texture src, RenderTexture dst) =>
        compute.UnaryOp("Invert", "InvertI", src, dst);

    // Src = 1 - Src
    public static void Invert(RenderTexture srcDst) =>
        Invert(srcDst, srcDst);


    // Dst = Src + Value
    public static void Add(Texture src, RenderTexture dst, float4 value) =>
        compute.UnaryOp("AddC", "AddCI", src, dst, value);

    // Src += Value
    public static void Add(RenderTexture srcDst, float4 value) =>
        Add(srcDst, srcDst, value);

    // Dst = SrcA + SrcB
    public static void Add(Texture srcA, Texture srcB, RenderTexture dst) {
        // In place operation only works when dst == srcA, so swap if dst == srcB
        if (dst == srcB) {
            (srcA, srcB) = (srcB, srcA);
        }
        compute.BinaryOp("Add", "AddI", srcA, srcB, dst);
    }


    // Dst = Src * Value
    public static void Multiply(Texture src, RenderTexture dst, float4 value) =>
        compute.UnaryOp("MultiplyC", "MultiplyCI", src, dst, value);

    // Src *= Value
    public static void Multiply(RenderTexture srcDst, float4 value) =>
        Multiply(srcDst, srcDst, value);

    // Dst = SrcA * SrcB
    public static void Multiply(Texture srcA, Texture srcB, RenderTexture dst) {
        // In place operation only works when dst == srcA, so swap if dst == srcB
        if (dst == srcB) {
            (srcA, srcB) = (srcB, srcA);
        }
        compute.BinaryOp("Multiply", "MultiplyI", srcA, srcB, dst);
    }


    // Dst = Src * Scale + Offset
    public static void MultiplyAdd(Texture src, RenderTexture dst,
                                   float4 scale, float4 offset) =>
        compute.UnaryOp("MultiplyCAddC", "MultiplyCAddCI",
                        src, dst, scale, offset);

    // Src = Src * Scale + Offset
    public static void MultiplyAdd(RenderTexture srcDst,
                                   float4 scale, float4 offset) =>
        MultiplyAdd(srcDst, srcDst, scale, offset);


    // Dst = lerp(SrcA, SrcB, T)
    public static void Lerp(Texture srcA, Texture srcB, RenderTexture dst, float4 t) {
        // In place operation only works when dst == srcA, so swap if dst == srcB
        if (dst == srcB) {
            (srcA, srcB) = (srcB, srcA);
            t = 1 - t;
        }
        compute.BinaryOp("Lerp", "LerpI", srcB, srcA, dst, 1 - t);
    }

    // Dst = SrcA * WeightA + SrcB * WeightB
    public static void AddWeighted(Texture srcA, Texture srcB, RenderTexture dst,
                                   float4 weightA, float4 weightB) {
        // In place operation only works when dst == srcA, so swap if dst == srcB
        if (dst == srcB) {
            (srcA, srcB) = (srcB, srcA);
            (weightA, weightB) = (weightB, weightA);
        }
        compute.BinaryOp("AddWeighted", "AddWeightedI", srcA, srcB, dst,
                         weightA, weightB);
    }


    // Dst = clamp(Src, min, max)
    public static void Clamp(Texture src, RenderTexture dst, float4 min, float4 max) =>
        compute.UnaryOp("Clamp", "ClampI", src, dst, min, max);

    // Src = clamp(Src, min, max)
    public static void Clamp(RenderTexture srcDst, float4 min, float4 max) =>
        Clamp(srcDst, srcDst, min, max);


    // Dst = min(SrcA, SrcB)
    public static void Min(Texture srcA, Texture srcB, RenderTexture dst) {
        // In place operation only works when dst == srcA, so swap if dst == srcB
        if (dst == srcB) {
            (srcA, srcB) = (srcB, srcA);
        }
        compute.BinaryOp("Min", "MinI", srcA, srcB, dst);
    }

    // Dst = max(SrcA, SrcB)
    public static void Max(Texture srcA, Texture srcB, RenderTexture dst) {
        // In place operation only works when dst == srcA, so swap if dst == srcB
        if (dst == srcB) {
            (srcA, srcB) = (srcB, srcA);
        }
        compute.BinaryOp("Max", "MaxI", srcA, srcB, dst);
    }

    // Dst = saturate(Src)
    public static void Saturate(Texture src, RenderTexture dst) =>
        compute.UnaryOp("Saturate", "SaturateI", src, dst);

    // Src = saturate(Src)
    public static void Saturate(RenderTexture srcDst) =>
        Saturate(srcDst, srcDst);


    // Dst = remap(Src, fromMin, fromMax, toMin, toMax)
    public static void Remap(Texture src, RenderTexture dst,
                             float4 fromMin, float4 fromMax,
                             float4 toMin, float4 toMax) {
        var fromDelta = (fromMax - fromMin);
        var toDelta = (toMax - toMin);
        var scale = select(toDelta / fromDelta, 0, fromDelta == 0);
        var offset = toMin - fromMin * scale;
        TextureMath.MultiplyAdd(src, dst, scale, offset);
    }

    // Src = remap(Src, fromMin, fromMax, toMin, toMax)
    public static void Remap(RenderTexture srcDst,
                             float4 fromMin, float4 fromMax,
                             float4 toMin, float4 toMax) =>
        Remap(srcDst, srcDst, fromMin, fromMax, toMin, toMax);

}

}
