using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Blep.TextureOps {

public static partial class TextureOps {

    private static TextureCompute _drawCompute;
    private static TextureCompute drawCompute =>
        (_drawCompute = _drawCompute ?? new TextureCompute("Shaders/Blep/TextureDraw"));

    public static void InitDraw() { var compute = drawCompute; }
    private static void _ResetDraw() { _drawCompute = null; }

    // -------------------------------------------------------------------------------

    public static void Ellipse(Texture src, RenderTexture dst,
                               float2 center, float2 radius,
                               Color fillColor=default(Color),
                               float outlineWidth=0,
                               Color outlineColor=default(Color)) =>
        drawCompute.UnaryOp("Ellipse", "EllipseI",
                            src, dst,
                            (Vector4) fillColor, (Vector4) outlineColor,
                            new float4(outlineWidth, 0, 0, 0),
                            new float4(center, radius));

    public static void Ellipse(RenderTexture srcDst,
                               float2 center, float2 radius,
                               Color fillColor=default(Color),
                               float outlineWidth=0,
                               Color outlineColor=default(Color)) =>
        Ellipse(srcDst, srcDst, center, radius, fillColor,
                outlineWidth, outlineColor);

    public static void Circle(Texture src, RenderTexture dst,
                              float2 center, float radius,
                              Color fillColor=default(Color),
                              float outlineWidth=0,
                              Color outlineColor=default(Color)) =>
        drawCompute.UnaryOp("Circle", "CircleI",
                            src, dst,
                            (Vector4) fillColor, (Vector4) outlineColor,
                            new float4(outlineWidth, 0, 0, 0),
                            new float4(center, radius, 0));

    public static void Circle(RenderTexture srcDst,
                              float2 center, float radius,
                              Color fillColor=default(Color),
                              float outlineWidth=0,
                              Color outlineColor=default(Color)) =>
        Circle(srcDst, srcDst, center, radius, fillColor,
               outlineWidth, outlineColor);

    public static void Rectangle(Texture src, RenderTexture dst,
                                 Rect rect,
                                 Color fillColor=default(Color),
                                 float outlineWidth=0,
                                 Color outlineColor=default(Color)) =>
        drawCompute.UnaryOp("Rectangle", "RectangleI",
                            src, dst,
                            (Vector4) fillColor, (Vector4) outlineColor,
                            new float4(outlineWidth, 0, 0, 0),
                            new float4(rect.xMin, rect.yMin, rect.xMax, rect.yMax));

    public static void Rectangle(RenderTexture srcDst,
                                 Rect rect,
                                 Color fillColor=default(Color),
                                 float outlineWidth=0,
                                 Color outlineColor=default(Color)) =>
        Rectangle(srcDst, srcDst, rect, fillColor,
                  outlineWidth, outlineColor);

    public static void Line(Texture src, RenderTexture dst,
                            float2 p, float2 q, float width,
                            Color fillColor=default(Color),
                            float outlineWidth=0,
                            Color outlineColor=default(Color)) =>
        drawCompute.UnaryOp("Line", "LineI",
                            src, dst,
                            (Vector4) fillColor, (Vector4) outlineColor,
                            new float4(outlineWidth, width, 0, 0),
                            new float4(p.x, p.y, q.x, q.y));

    public static void Line(RenderTexture srcDst,
                            float2 p, float2 q, float width,
                            Color fillColor=default(Color),
                            float outlineWidth=0,
                            Color outlineColor=default(Color)) =>
        Line(srcDst, srcDst, p, q, width, fillColor,
             outlineWidth, outlineColor);

}

}
