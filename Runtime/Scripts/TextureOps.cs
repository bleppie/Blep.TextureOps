using UnityEngine;

namespace Blep.TextureOps {

public static partial class TextureOps {

    // For convenience
    public static readonly int SrcAId = TextureCompute.SrcAId;
    public static readonly int SrcBId = TextureCompute.SrcBId;
    public static readonly int ScalarAId = TextureCompute.ScalarAId;
    public static readonly int ScalarBId = TextureCompute.ScalarBId;
    public static readonly int DstId = TextureCompute.DstId;

    // Call to preload compute shaders. Unnecessary to call this.
    public static void Init() {
        InitMath();
        InitIP();
        InitDraw();
        InitGeometry();
    }

    // Reset statics while in the editor and play-mode-options are turned on
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void _Reset() {
        _ResetMath();
        _ResetIP();
        _ResetDraw();
        _ResetGeometry();
    }
}
}
