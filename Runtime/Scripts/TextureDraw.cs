using UnityEngine;

namespace Blep.TextureOps {

public static class TextureDraw {

    private static TextureCompute _compute;
    private static TextureCompute compute =>
        (_compute = _compute ?? new TextureCompute("Shaders/Blep/TextureDraw"));

    // Reset statics while in the editor and play-mode-options are turned on
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void _Init() { _compute = null; }

    // -------------------------------------------------------------------------------

    public static void Circle(Texture src, RenderTexture dst, Vector4 color,
                              Vector2 center, float radius, float falloff=0) =>
        compute.UnaryOp("Circle", "CircleI",
                        src, dst, color,
                        new Vector4(center.x, center.y, radius, falloff));

    public static void Circle(RenderTexture srcDst,
                              Vector4 color, Vector2 center, float radius, float falloff=0) =>
        Circle(srcDst, srcDst, color, center, radius, falloff);


    public static void Line(Texture src, RenderTexture dst, Vector4 color,
                            Vector2 p0, Vector2 p1, float width, float falloff=0) =>
        compute.UnaryOp("Line", "LineI",
                        src, dst, color,
                        new Vector4(p0.x, p0.y, p1.x, p1.y),
                        new Vector2(width, falloff));

    public static void Line(RenderTexture srcDst, Vector4 color,
                            Vector2 p0, Vector2 p1, float width, float falloff=0) =>
        Line(srcDst, srcDst, color, p0, p1, width, falloff);


    public static void Border(Texture src, RenderTexture dst, Vector4 color,
                              float width, float falloff=0) =>
        compute.UnaryOp("Border", "BorderI",
                        src, dst, color, new Vector2(width, falloff));

    public static void Border(RenderTexture srcDst, Vector4 color,
                              float width, float falloff=0) =>
        Border(srcDst, srcDst, color, width, falloff);

}

}
