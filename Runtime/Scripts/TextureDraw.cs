using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Blep.TextureOps {

public static class TextureDraw {

    private static TextureCompute _compute;
    private static TextureCompute compute =>
        (_compute = _compute ?? new TextureCompute("Shaders/Blep/TextureDraw"));

    // Reset statics while in the editor and play-mode-options are turned on
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void _Init() { _compute = null; }

    // -------------------------------------------------------------------------------
    public static void Ellipse(Texture src, RenderTexture dst,
                               float2 center, float2 radius,
                               Color fillColor=default(Color),
                               float outlineWidth=0,
                               Color outlineColor=default(Color)) =>
        compute.UnaryOp("Ellipse", "EllipseI",
                        src, dst, (Vector4) fillColor, (Vector4) outlineColor,
                        new float4(outlineWidth, 0, 0, 0),
                        new float4(center, radius));

    public static void Ellipse(RenderTexture srcDst,
                               float2 center, float2 radius,
                               Color fillColor=default(Color),
                               float outlineWidth=0,
                               Color outlineColor=default(Color)) =>
        Ellipse(srcDst, srcDst, center, radius, fillColor, outlineWidth, outlineColor);

    public static void Circle(Texture src, RenderTexture dst,
                              float2 center, float radius,
                              Color fillColor=default(Color),
                              float outlineWidth=0,
                              Color outlineColor=default(Color)) =>
        compute.UnaryOp("Circle", "CircleI",
                        src, dst, (Vector4) fillColor, (Vector4) outlineColor,
                        new float4(outlineWidth, 0, 0, 0),
                        new float4(center, radius, 0));

    public static void Circle(RenderTexture srcDst,
                              float2 center, float radius,
                              Color fillColor=default(Color),
                              float outlineWidth=0,
                              Color outlineColor=default(Color)) =>
        Circle(srcDst, srcDst, center, radius, fillColor, outlineWidth, outlineColor);

    public static void Rectangle(Texture src, RenderTexture dst,
                                 Rect rect,
                                 Color fillColor=default(Color),
                                 float outlineWidth=0,
                                 Color outlineColor=default(Color)) =>
        compute.UnaryOp("Rectangle", "RectangleI",
                        src, dst, (Vector4) fillColor, (Vector4) outlineColor,
                        new float4(outlineWidth, 0, 0, 0),
                        new float4(rect.xMin, rect.yMin, rect.xMax, rect.yMax));

    public static void Rectangle(RenderTexture srcDst,
                                 Rect rect,
                                 Color fillColor=default(Color),
                                 float outlineWidth=0,
                                 Color outlineColor=default(Color)) =>
        Rectangle(srcDst, srcDst, rect, fillColor, outlineWidth, outlineColor);

    public static void Line(Texture src, RenderTexture dst,
                            float2 p, float2 q, float width,
                            Color fillColor=default(Color),
                            float outlineWidth=0,
                            Color outlineColor=default(Color)) =>
        compute.UnaryOp("Line", "LineI",
                        src, dst, (Vector4) fillColor, (Vector4) outlineColor,
                        new float4(outlineWidth, width, 0, 0),
                        new float4(p.x, p.y, q.x, q.y));

    public static void Line(RenderTexture srcDst,
                            float2 p, float2 q, float width,
                            Color fillColor=default(Color),
                            float outlineWidth=0,
                            Color outlineColor=default(Color)) =>
        Line(srcDst, srcDst, p, q, width, fillColor, outlineWidth, outlineColor);

}

}
