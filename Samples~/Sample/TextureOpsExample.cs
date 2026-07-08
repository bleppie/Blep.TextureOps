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

    public void FillAlpha(RenderTexture dst) => TextureOps.Set(dst, 1, float4(0, 0, 0, 1));

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
        TextureOps.Swizzle(overlay2, mask, "aaaa");
        TextureOps.Threshold(mask, 0.5f);

        // Premultiply overlay1 and 2 to clear out cruft where alpha=0
        // TextureOps.Premultiply(overlay1);
        // TextureOps.Premultiply(overlay2);

        yield return Test("Copy", () => {
            TextureOps.Copy(src, dst);
        });

        //// Math

        yield return Test("Copy", () => {
            TextureOps.Copy(src, dst);
        });
        yield return Test("Clear", () => {
            TextureOps.Copy(src, dst);
            TextureOps.Clear(dst);
        });
        yield return Test("Set", () => {
            TextureOps.Copy(src, dst);
            TextureOps.Set(dst, red);
        });
        yield return Test("Set with Channel Mask", () => {
            TextureOps.Copy(src, dst);
            TextureOps.Set(dst, red, float4(1, 0, 0, 0));
        });
        yield return Test("Set with Image Mask", () => {
            TextureOps.Copy(src, dst);
            TextureOps.Set(dst, red, mask);
        });
        yield return Test("Invert", () => {
            TextureOps.Invert(src, dst);
            FillAlpha(dst);
        });
        yield return Test("Add Constant", () => {
            TextureOps.Add(src, dst, oscillate());
        });
        yield return Test("Add Image", () => {
            TextureOps.Add(src, overlay2, dst);
        });
        yield return Test("Add Image Inplace", () => {
            TextureOps.Copy(src, dst);
            TextureOps.Add(src, dst, dst);
        });
        yield return Test("Add Image Inplace B", () => {
            TextureOps.Copy(src, dst);
            TextureOps.Add(dst, src, dst);
        });
        yield return Test("Multiply Constant", () => {
            TextureOps.Multiply(src, dst, oscillate01());
        });
        yield return Test("Multiply Image", () => {
            TextureOps.Multiply(src, overlay2, dst);
        });
        yield return Test("Multiply Image Inplace", () => {
            TextureOps.Copy(src, dst);
            TextureOps.Multiply(src, dst, dst);
        });
        yield return Test("Multiply Image Inplace B", () => {
            TextureOps.Copy(src, dst);
            TextureOps.Multiply(dst, src, dst);
        });
        yield return Test("Lerp", () => {
            TextureOps.Lerp(overlay1, overlay2, dst, oscillate01());
        });
        yield return Test("Clamp", () => {
            var min = oscillate01() * 0.25f;
            var max = 1 - min;
            TextureOps.Clamp(src, dst, min, max);
        });


        //// IP

        yield return Test($"Min", () => {
            TextureOps.Add(src, dst, oscillate01() * 0.5f);
            var minColor = TextureOps.MinValue(dst);
            SetLabel($"Min {minColor:0.00}");
        });
        yield return Test($"Max", () => {
            TextureOps.Multiply(src, dst, oscillate01());
            var maxColor = TextureOps.MaxValue(dst);
            SetLabel($"Max {maxColor:0.00}");
        });

        yield return Test("ComposeOver", () => {
            TextureOps.ComposeOver(overlay1, src, dst);
        });

        yield return Test("ComposeIn", () => {
            TextureOps.ComposeIn(overlay1, overlay2, tmp);
            TextureOps.ComposeOver(tmp, src, dst);
        });

        yield return Test("ComposeOut", () => {
            TextureOps.ComposeOut(overlay1, overlay2, tmp);
            TextureOps.ComposeOver(tmp, src, dst);
        });

        yield return Test("ComposeAtop", () => {
            TextureOps.ComposeAtop(overlay1, overlay2, tmp);
            TextureOps.ComposeOver(tmp, src, dst);
        });

        yield return Test("ComposeXor", () => {
            TextureOps.ComposeXor(overlay1, overlay2, tmp);
            TextureOps.ComposeOver(tmp, src, dst);
        });

        yield return Test("ComposePlus", () => {
            TextureOps.ComposePlus(overlay1, overlay2, tmp);
            TextureOps.ComposeOver(tmp, src, dst);
        });


        yield return Test("BlendColorBurn", () => {
            TextureOps.BlendColorBurn(overlay1, src, dst);
        });
        yield return Test("BlendLinearBurn", () => {
            TextureOps.BlendLinearBurn(overlay1, src, dst);
        });
        yield return Test("BlendScreen", () => {
            TextureOps.BlendScreen(overlay1, src, dst);
        });
        yield return Test("BlendColorDodge", () => {
            TextureOps.BlendColorDodge(overlay1, src, dst);
        });
        yield return Test("BlendLinearDodge", () => {
            TextureOps.BlendLinearDodge(overlay1, src, dst);
        });
        yield return Test("BlendOverlay", () => {
            TextureOps.BlendOverlay(overlay1, src, dst);
        });
        yield return Test("BlendSoftLight", () => {
            TextureOps.BlendSoftLight(overlay1, src, dst);
        });
        yield return Test("BlendHardLight", () => {
            TextureOps.BlendHardLight(overlay1, src, dst);
        });
        yield return Test("BlendVividLight", () => {
            TextureOps.BlendVividLight(overlay1, src, dst);
        });
        yield return Test("BlendLinearLight", () => {
            TextureOps.BlendLinearLight(overlay1, src, dst);
        });
        yield return Test("BlendPinLight", () => {
            TextureOps.BlendPinLight(overlay1, src, dst);
        });

        yield return Test("ConvertRGB2HSV", () => {
            TextureOps.ConvertRGB2HSV(src, dst);
        });

        yield return Test("ConvertHSV2RGB", () => {
            TextureOps.ConvertRGB2HSV(src, dst);
            TextureOps.ConvertHSV2RGB(dst);
        });

        yield return Test("EqualizeHistogram", () => {
            TextureOps.EqualizeHistogram(src, dst);
        });

        yield return Test("Swizzle", () => {
            TextureOps.Swizzle(src, dst, "bgra");
        });

        yield return Test("Grayscale", () => {
            TextureOps.Grayscale(src, dst);
        });

        yield return Test("GrayscaleGamma", () => {
            TextureOps.GrayscaleGamma(src, dst);
        });

        yield return Test("Lookup", () => {
            TextureOps.Lookup(src, dst, gradient);
        });

        yield return Test("Contrast", () => {
            TextureOps.Contrast(src, dst, oscillate());
        });

        yield return Test("Threshold", () => {
            var a = oscillate01();
            TextureOps.Threshold(src, dst, a);
            FillAlpha(dst);
        });

        yield return Test("Erode", () => {
            TextureOps.Erode(mask, dst);
            for (int i = 0; i < oscillate01() * 5; i++) {
                TextureOps.Erode(dst, tmp);
                TextureOps.Erode(tmp, dst);
            }
            FillAlpha(dst);
        });

        yield return Test("Dilate", () => {
            TextureOps.Dilate(mask, dst);
            for (int i = 0; i < oscillate01() * 5; i++) {
                TextureOps.Dilate(dst, tmp);
                TextureOps.Dilate(tmp, dst);
            }
            FillAlpha(dst);
        });

        yield return Test("Skeletonize", () => {
            TextureOps.Skeletonize(mask, dst, (int) (oscillate01() * 64));
            FillAlpha(dst);
        });

        yield return Test("Sobel", () => {
            TextureOps.Sobel(mask, dst);
            FillAlpha(dst);
        });

        yield return Test("Scharr", () => {
            TextureOps.Scharr(mask, dst);
            FillAlpha(dst);
        });

        yield return Test("BlurGaussian", () => {
            TextureOps.BlurGaussian(src, dst, oscillate01() * 25, -1);
        });

        yield return Test("BlurGaussianRecursive", () => {
            TextureOps.BlurGaussianRecursive(src, dst, oscillate01() * 10);
        });

        yield return Test("Bilateral", () => {
            TextureOps.Bilateral(src, dst, oscillate01() * 25, -1);
        });

        yield return Test("Median3x3", () => {
            TextureOps.Median3x3(src, dst);
        });

        yield return Test("Median5x5", () => {
            TextureOps.Median5x5(src, dst);
        });

        yield return Test("DistanceTransform", () => {
            TextureOps.DistanceTransform(mask, dst);
            TextureOps.Multiply(dst, 0.01f);
            TextureOps.Swizzle(dst, "rrrr");
            FillAlpha(dst);
        });


        //// Geometric

        yield return Test("FlipHorizontal", () => {
            TextureOps.FlipHorizontal(src, dst);
        });

        yield return Test("FlipVertical", () => {
            TextureOps.FlipVertical(src, dst);
        });

        yield return Test("Rotate180", () => {
            TextureOps.Rotate180(src, dst);
        });

        yield return Test("WarpAffine", () => {
            var invMat = TextureOps.GetInverseAffineMatrix
                (size * 0.5f, lerp(1, 0.5f, oscillate01()), oscillate01() * PI, 0);
            TextureOps.WarpAffine(src, dst, invMat);
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

            var invMat = TextureOps.GetInverseCornerWarpMatrix(srcPts, dstPts);
            TextureOps.WarpPerspective(src, dst, invMat);
        });


        //// Draw

        SetLabel("Draw");

        TextureOps.Copy(src, dst);

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
                    TextureOps.Circle(dst, center, extent.x,
                                       fillColor, outlineWidth, outlineColor);
                    break;
                case 1:
                    TextureOps.Ellipse(dst, center, extent,
                                        fillColor, outlineWidth, outlineColor);
                    break;
                case 2:
                    TextureOps.Rectangle(dst, new Rect(center, extent),
                                          fillColor, outlineWidth, outlineColor);
                    break;
                case 3:
                    TextureOps.Line( dst, center - extent/2, center + extent/2,
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
