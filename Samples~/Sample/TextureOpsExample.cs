using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using Random = UnityEngine.Random;
using Blep.TextureOps;

public class TextureOpsExample : MonoBehaviour {

    public Text label;

    public Texture2D src;
    public Texture2D overlay1;
    public Texture2D overlay2;
    public Texture2D gradient;
    public RenderTexture dst;

    private static float oscillate() => sin(Time.time);
    private static float oscillate01() => oscillate() * 0.5f + 0.5f;

    public void SetLabel(string name) => label.text = name;

    public IEnumerator Test(string name, Action action) {
        SetLabel(name);
        while (! Input.GetKeyUp(KeyCode.Space) &&
               ! Input.GetMouseButtonUp(0)) {
            action();
            yield return null;
        }
        yield return null;
    }

    public void FillAlpha(RenderTexture dst) => TextureMath.Set(dst, 1, float4(0, 0, 0, 1));

    public FilterMode filterMode = FilterMode.Point;
    public TextureGeometry.WrapMode wrapMode = TextureGeometry.WrapMode.Transparent;
    public float2 center;
    public float2 scale = 1;
    public float angle;
    public float2 translation;
    public Color borderColor;

    public IEnumerator Start() {
        var shortWait = new WaitForSeconds(0.1f);
        var wait = new WaitForSeconds(0.5f);

        float width = src.width;
        float height = src.height;
        float2 size = float2(width, height);
        var tmp = TextureCompute.GetTemporary(dst);
        var mask = TextureCompute.GetTemporary(dst);
        float4 red = float4(1, 0, 0, 1);

        // Create b&w mask
        TextureIP.Swizzle(overlay2, mask, "aaaa");
        TextureIP.Threshold(mask, 0.5f);

        // Premultiply overlay1 and 2 to clear out cruft where alpha=0
        // TextureIP.Premultiply(overlay1);
        // TextureIP.Premultiply(overlay2);


        yield return Test("Copy", () => {
            TextureMath.Copy(src, dst);
        });

        yield return Test("BlendColorBurn", () => {
            TextureIP.BlendColorBurn(overlay1, src, dst);
        });
        yield return Test("BlendLinearBurn", () => {
            TextureIP.BlendLinearBurn(overlay1, src, dst);
        });
        yield return Test("BlendScreen", () => {
            TextureIP.BlendScreen(overlay1, src, dst);
        });
        yield return Test("BlendColorDodge", () => {
            TextureIP.BlendColorDodge(overlay1, src, dst);
        });
        yield return Test("BlendLinearDodge", () => {
            TextureIP.BlendLinearDodge(overlay1, src, dst);
        });
        yield return Test("BlendOverlay", () => {
            TextureIP.BlendOverlay(overlay1, src, dst);
        });
        yield return Test("BlendSoftLight", () => {
            TextureIP.BlendSoftLight(overlay1, src, dst);
        });
        yield return Test("BlendHardLight", () => {
            TextureIP.BlendHardLight(overlay1, src, dst);
        });
        yield return Test("BlendVividLight", () => {
            TextureIP.BlendVividLight(overlay1, src, dst);
        });
        yield return Test("BlendLinearLight", () => {
            TextureIP.BlendLinearLight(overlay1, src, dst);
        });
        yield return Test("BlendPinLight", () => {
            TextureIP.BlendPinLight(overlay1, src, dst);
        });

        //// Math

        yield return Test("Copy", () => {
            TextureMath.Copy(src, dst);
        });
        yield return Test("Clear", () => {
            TextureMath.Copy(src, dst);
            TextureMath.Clear(dst);
        });
        yield return Test("Set", () => {
            TextureMath.Copy(src, dst);
            TextureMath.Set(dst, red);
        });
        yield return Test("Set with Channel Mask", () => {
            TextureMath.Copy(src, dst);
            TextureMath.Set(dst, red, float4(1, 0, 0, 0));
        });
        yield return Test("Set with Image Mask", () => {
            TextureMath.Copy(src, dst);
            TextureMath.Set(dst, red, mask);
        });
        yield return Test("Invert", () => {
            TextureMath.Invert(src, dst);
            FillAlpha(dst);
        });
        yield return Test("Add Constant", () => {
            TextureMath.Add(src, dst, oscillate());
        });
        yield return Test("Add Image", () => {
            TextureMath.Add(src, overlay2, dst);
        });
        yield return Test("Add Image Inplace", () => {
            TextureMath.Copy(src, dst);
            TextureMath.Add(src, dst, dst);
        });
        yield return Test("Add Image Inplace B", () => {
            TextureMath.Copy(src, dst);
            TextureMath.Add(dst, src, dst);
        });
        yield return Test("Multiply Constant", () => {
            TextureMath.Multiply(src, dst, oscillate01());
        });
        yield return Test("Multiply Image", () => {
            TextureMath.Multiply(src, overlay2, dst);
        });
        yield return Test("Multiply Image Inplace", () => {
            TextureMath.Copy(src, dst);
            TextureMath.Multiply(src, dst, dst);
        });
        yield return Test("Multiply Image Inplace B", () => {
            TextureMath.Copy(src, dst);
            TextureMath.Multiply(dst, src, dst);
        });
        yield return Test("Lerp", () => {
            TextureMath.Lerp(overlay1, overlay2, dst, oscillate01());
        });
        yield return Test("Clamp", () => {
            var min = oscillate01() * 0.25f;
            var max = 1 - min;
            TextureMath.Clamp(src, dst, min, max);
        });


        //// IP

        yield return Test($"Min", () => {
            TextureMath.Add(src, dst, oscillate01() * 0.5f);
            var minColor = TextureIP.MinValue(dst);
            SetLabel($"Min {minColor:0.00}");
        });
        yield return Test($"Max", () => {
            TextureMath.Multiply(src, dst, oscillate01());
            var maxColor = TextureIP.MaxValue(dst);
            SetLabel($"Max {maxColor:0.00}");
        });

        yield return Test("ComposeOver", () => {
            TextureIP.ComposeOver(overlay1, src, dst);
        });

        yield return Test("ComposeIn", () => {
            TextureIP.ComposeIn(overlay1, overlay2, tmp);
            TextureIP.ComposeOver(tmp, src, dst);
        });

        yield return Test("ComposeOut", () => {
            TextureIP.ComposeOut(overlay1, overlay2, tmp);
            TextureIP.ComposeOver(tmp, src, dst);
        });

        yield return Test("ComposeAtop", () => {
            TextureIP.ComposeAtop(overlay1, overlay2, tmp);
            TextureIP.ComposeOver(tmp, src, dst);
        });

        yield return Test("ComposeXor", () => {
            TextureIP.ComposeXor(overlay1, overlay2, tmp);
            TextureIP.ComposeOver(tmp, src, dst);
        });

        yield return Test("ComposePlus", () => {
            TextureIP.ComposePlus(overlay1, overlay2, tmp);
            TextureIP.ComposeOver(tmp, src, dst);
        });

        yield return Test("FlipHorizontal", () => {
            TextureIP.FlipHorizontal(src, dst);
        });

        yield return Test("FlipVertical", () => {
            TextureIP.FlipVertical(src, dst);
        });

        yield return Test("Rotate180", () => {
            TextureIP.Rotate180(src, dst);
        });

        yield return Test("ConvertRGB2HSV", () => {
            TextureIP.ConvertRGB2HSV(src, dst);
        });

        yield return Test("ConvertHSV2RGB", () => {
            TextureIP.ConvertRGB2HSV(src, dst);
            TextureIP.ConvertHSV2RGB(dst);
        });

        yield return Test("EqualizeHistogram", () => {
            TextureIP.EqualizeHistogram(src, dst);
        });

        yield return Test("Swizzle", () => {
            TextureIP.Swizzle(src, dst, "bgra");
        });

        yield return Test("Grayscale", () => {
            TextureIP.Grayscale(src, dst);
        });

        yield return Test("GrayscaleGamma", () => {
            TextureIP.GrayscaleGamma(src, dst);
        });

        yield return Test("Lookup", () => {
            TextureIP.Lookup(src, dst, gradient);
        });

        yield return Test("Contrast", () => {
            TextureIP.Contrast(src, dst, oscillate());
        });

        yield return Test("Threshold", () => {
            var a = oscillate01();
            TextureIP.Threshold(src, dst, a);
            FillAlpha(dst);
        });

        yield return Test("Erode", () => {
            TextureIP.Erode(mask, dst);
            for (int i = 0; i < oscillate01() * 5; i++) {
                TextureIP.Erode(dst, tmp);
                TextureIP.Erode(tmp, dst);
            }
            FillAlpha(dst);
        });

        yield return Test("Dilate", () => {
            TextureIP.Dilate(mask, dst);
            for (int i = 0; i < oscillate01() * 5; i++) {
                TextureIP.Dilate(dst, tmp);
                TextureIP.Dilate(tmp, dst);
            }
            FillAlpha(dst);
        });

        yield return Test("Skeletonize", () => {
            TextureIP.Skeletonize(mask, dst, (int) (oscillate01() * 64));
            FillAlpha(dst);
        });

        yield return Test("Sobel", () => {
            TextureIP.Sobel(mask, dst);
            FillAlpha(dst);
        });

        yield return Test("Scharr", () => {
            TextureIP.Scharr(mask, dst);
            FillAlpha(dst);
        });

        yield return Test("BlurGaussian", () => {
            TextureIP.BlurGaussian(src, dst, oscillate01() * 25, -1);
        });

        yield return Test("BlurGaussianRecursive", () => {
            TextureIP.BlurGaussianRecursive(src, dst, oscillate01() * 10);
        });

        yield return Test("Bilateral", () => {
            TextureIP.Bilateral(src, dst, oscillate01() * 25, -1);
        });

        yield return Test("Median3x3", () => {
            TextureIP.Median3x3(src, dst);
        });

        yield return Test("Median5x5", () => {
            TextureIP.Median5x5(src, dst);
        });

        yield return Test("DistanceTransform", () => {
            TextureIP.DistanceTransform(mask, dst);
            TextureMath.Multiply(dst, 0.01f);
            TextureIP.Swizzle(dst, "rrrr");
            FillAlpha(dst);
        });


        //// Warp

        yield return Test("WarpAffine", () => {
            var invMat = TextureGeometry.GetInverseAffineMatrix
                (size * 0.5f, lerp(1, 0.5f, oscillate01()), oscillate01() * PI, 0);
            TextureGeometry.WarpAffine(src, dst, invMat);
        });

        yield return Test("WarpPerspective", () => {
            var srcPts = new float2[] { float2(0, 0),
                                        float2(width, 0),
                                        float2(width, height),
                                        float2(0, height) };
            var dstPts = new float2[] { oscillate01() * size * 0.5f,
                                        float2(width, 0),
                                        float2(width, height),
                                        float2(0, height) };

            var invMat = TextureGeometry.GetInverseWarpCornersMatrix(srcPts, dstPts);
            TextureGeometry.WarpPerspective(src, dst, invMat);
        });


        //// Draw

        SetLabel("Draw");

        TextureMath.Copy(src, dst);

        for (int i = 0; i < 25; i++) {
            var center = float2(width * Random.Range(0.2f, 0.8f), height * Random.Range(0.2f, 0.8f));
            var extent = float2(width * Random.Range(0.1f, 0.3f), height * Random.Range(0.1f, 0.3f));
            var hasOutline = Random.value > 0.5f;
            var hasFill = ! hasOutline || Random.value > 0.5f;
            var fillColor = hasFill ? Random.ColorHSV() : Color.clear;
            var outlineWidth = hasOutline ? Random.Range(2, 20): 0;
            var outlineColor = hasOutline ? Random.ColorHSV() : Color.clear;

            switch (i % 4) {
                case 0:
                    TextureDraw.Circle(dst, center, extent.x,
                                       fillColor, outlineWidth, outlineColor);
                    break;
                case 1:
                    TextureDraw.Ellipse(dst, center, extent,
                                        fillColor, outlineWidth, outlineColor);
                    break;
                case 2:
                    TextureDraw.Rectangle(dst, new Rect(center, extent),
                                          fillColor, outlineWidth, outlineColor);
                    break;
                case 3:
                    TextureDraw.Line( dst, center - extent/2, center + extent/2,
                                      outlineWidth + Random.Range(1, 10),
                                      fillColor, outlineWidth, outlineColor);
                    break;
            }
            yield return shortWait;
        }

        while (! Input.GetKeyUp(KeyCode.Space) &&
               ! Input.GetMouseButtonUp(0)) {
            yield return null;
        }
    }

}
