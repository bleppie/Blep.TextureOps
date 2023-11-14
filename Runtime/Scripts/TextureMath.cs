using UnityEngine;

namespace Blep.TextureOps {

public static class TextureMath {

    private static TextureCompute _compute;
    public static TextureCompute compute =>
        (_compute = _compute ?? new TextureCompute("Shaders/TextureMath"));

    // Reset statics while in the editor and play-mode-options are turned on
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void _Init() { _compute = null; }

    // -------------------------------------------------------------------------------

    // For consistency
		public static void Copy(Texture src, RenderTexture dst) =>
        Graphics.CopyTexture(src, dst);


		public static void Set(RenderTexture dst, Vector4 value) =>
        compute.UnaryOp("SetC", dst, dst, value);


		public static void SetMasked(Texture src, RenderTexture dst, Vector4 value, Vector4 mask) =>
				compute.UnaryOp("SetCMaskedC", src, dst, value, mask);

		public static void SetMasked(RenderTexture srcDst, Vector4 value, Vector4 mask) =>
        SetMasked(srcDst, srcDst, value, mask);


		public static void SetMasked(Texture src, RenderTexture dst, Vector4 value, Texture mask) =>
				compute.BinaryOp("SetCMasked", src, mask, dst, value);

		public static void SetMasked(RenderTexture srcDst, Vector4 value, Texture mask) =>
        SetMasked(srcDst, srcDst, value, mask);


		public static void Add(Texture src, RenderTexture dst, Vector4 value) =>
				compute.UnaryOp("AddC", src, dst, value);

		public static void Add(RenderTexture srcDst, Vector4 value) =>
        Add(srcDst, srcDst, value);

		public static void Add(Texture srcA, Texture srcB, RenderTexture dst) =>
				compute.BinaryOp("Add", srcA, srcB, dst);


		public static void AddWeighted(Texture srcA, Texture srcB, RenderTexture dst,
                                   Vector4 weightA, Vector4 weightB) =>
				compute.BinaryOp("AddWeighted", srcA, srcB, dst, weightA, weightB);

		public static void AddWeighted(Texture srcA, Texture srcB, RenderTexture dst,
                                   float weightA, float weightB) =>
				compute.BinaryOp("AddWeighted", srcA, srcB, dst,
                         new Vector4(weightA, weightA, weightA, weightA),
                         new Vector4(weightB, weightB, weightB, weightB));

    public static void Lerp(Texture srcA, Texture srcB, RenderTexture dst, float t) =>
        AddWeighted(srcA, srcB, dst, 1 - t, t);


		public static void Multiply(Texture src, RenderTexture dst, Vector4 value) =>
				compute.UnaryOp("MultiplyC", src, dst, value);

		public static void Multiply(RenderTexture srcDst, Vector4 value) =>
        Multiply(srcDst, srcDst, value);

		public static void Multiply(Texture srcA, Texture srcB, RenderTexture dst) =>
				compute.BinaryOp("Multiply", srcA, srcB, dst);


    public static void MultiplyAdd(Texture src, RenderTexture dst,
                                   Vector4 scale, Vector4 offset, bool saturate=false) {
        if (saturate)
            compute.UnaryOp("MultiplyCAddCSat", src, dst, scale, offset);
        else
            compute.UnaryOp("MultiplyCAddC", src, dst, scale, offset);
    }

    public static void MultiplyAdd(RenderTexture srcDst,
                                   Vector4 scale, Vector4 offset, bool saturate=false) =>
        MultiplyAdd(srcDst, srcDst, scale, offset, saturate);


    public static void Clamp(Texture src, RenderTexture dst, Vector4 min, Vector4 max) =>
				compute.UnaryOp("Clamp", src, dst, min, max);

    public static void Clamp(RenderTexture srcDst, Vector4 min, Vector4 max) =>
        Clamp(srcDst, srcDst, min, max);


    public static void Saturate(Texture src, RenderTexture dst) =>
				compute.UnaryOp("Saturate", src, dst);

    public static void Saturate(RenderTexture srcDst) =>
        Saturate(srcDst, srcDst);


		public static void Remap(Texture src, RenderTexture dst,
                             Vector4 fromMin, Vector4 fromMax,
                             Vector4 toMin, Vector4 toMax) {
				var fromDelta = (fromMax - fromMin);
				var toDelta = (toMax - toMin);
				var scale = new Vector4(fromDelta.x == 0 ? 0 : toDelta.x / fromDelta.x,
                                fromDelta.y == 0 ? 0 : toDelta.y / fromDelta.y,
																fromDelta.z == 0 ? 0 : toDelta.z / fromDelta.z,
                                fromDelta.w == 0 ? 0 : toDelta.w / fromDelta.w);
				var offset = toMin - Vector4.Scale(fromMin, scale);
        TextureMath.MultiplyAdd(src, dst, scale, offset);
		}

		public static void Remap(RenderTexture srcDst,
                             Vector4 fromMin, Vector4 fromMax,
                             Vector4 toMin, Vector4 toMax) =>
        Remap(srcDst, srcDst, fromMin, fromMax, toMin, toMax);

}

}
