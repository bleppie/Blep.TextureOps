using System.Collections;
using UnityEngine;
using Blep.TextureOps;

public class TextureOpsExample : MonoBehaviour {

    public Texture2D src;
    public Texture2D overlay1;
    public Texture2D overlay2;
    public Texture2D gradient;
    public RenderTexture dst;

    [Range(0,100)]
    public float test;

    public IEnumerator Start() {
        var shortWait = new WaitForSeconds(0.1f);
        var wait = new WaitForSeconds(0.5f);

        float width = src.width;
        float height = src.height;
        var tmp = TextureCompute.GetTemporary(dst);

        while (true) {

            TextureMath.Copy(src, dst);
            yield return wait;

            //// Math

            var minColor = TextureIP.Min(src);
            var maxColor = TextureIP.Max(src);
            TextureMath.Set(dst, (minColor + maxColor) * 0.5f);
            yield return wait;

            TextureMath.SetMasked(dst, maxColor, overlay2);
            yield return wait;

            TextureIP.FlipHorizontal(src, dst);
            TextureMath.Lerp(overlay1, overlay2, dst, 0.5f);
            yield return wait;

            TextureMath.Clamp(src, dst, Vector4.one * 0.3f, Vector4.one * 0.7f);
            yield return wait;

            TextureMath.Remap(src, dst, minColor, maxColor, maxColor, minColor);
            yield return wait;


            //// IP

            TextureIP.ComposeOver(overlay1, src, dst);
            yield return wait;

            TextureIP.ComposeIn(overlay1, overlay2, tmp);
            TextureIP.ComposeOver(tmp, src, dst);
            yield return wait;

            TextureIP.ComposeOut(overlay1, overlay2, tmp);
            TextureIP.ComposeOver(tmp, src, dst);
            yield return wait;

            TextureIP.ComposeAtop(overlay1, overlay2, tmp);
            TextureIP.ComposeOver(tmp, src, dst);
            yield return wait;

            TextureIP.ComposeXor(overlay1, overlay2, tmp);
            TextureIP.ComposeOver(tmp, src, dst);
            yield return wait;

            TextureIP.ComposePlus(overlay1, overlay2, tmp);
            TextureIP.ComposeOver(tmp, src, dst);
            yield return wait;

            TextureIP.FlipHorizontal(src, dst);
            yield return wait;

            TextureIP.FlipVertical(src, dst);
            yield return wait;

            TextureIP.Rotate180(src, dst);
            yield return wait;

            TextureIP.ConvertRGB2HSV(src, dst);
            yield return wait;

            TextureIP.ConvertHSV2RGB(dst);
            yield return wait;

            TextureIP.EqualizeHistogram(src, dst);
            yield return wait;

            TextureIP.Swizzle(src, dst, "bgra");
            yield return wait;

            TextureIP.Swizzle(dst, "rrra");
            yield return wait;

            TextureIP.Grayscale(src, dst);
            yield return wait;

            TextureIP.Lookup(src, dst, gradient);
            yield return wait;

            TextureIP.GrayscaleGamma(src, dst);
            yield return wait;

            TextureIP.Contrast(dst, dst, -0.5f);
            yield return wait;

            TextureIP.Threshold(dst, new Vector4(0.3f, 0.3f, 0.3f, 0.0f));
            yield return wait;

            TextureIP.Erode(dst, tmp);
            TextureIP.Erode(tmp, dst);
            yield return wait;

            for (int i = 3; --i >= 0; ) {
                TextureIP.Dilate(dst, tmp);
                TextureIP.Dilate(tmp, dst);
            }
            yield return wait;

            TextureIP.Skeletonize(dst, 64);
            yield return wait;

            TextureIP.Sobel(src, dst);
            TextureMath.SetMasked(dst, dst, Vector4.one, new Vector4(0, 0, 0, 1));
            yield return wait;

            TextureIP.Scharr(src, dst);
            TextureMath.SetMasked(dst, dst, Vector4.one, new Vector4(0, 0, 0, 1));
            yield return wait;

            TextureIP.BlurGaussian(src, dst, 25, -1);
            yield return wait;

            for (int i = 1; i <= 21; i += 4) {
                TextureIP.BlurGaussianRecursive(src, dst, i);
                yield return wait;

            }

            TextureIP.Median3x3(src, dst);
            yield return wait;

            TextureIP.Median5x5(src, dst);
            yield return wait;

            //// Draw

            TextureMath.Copy(src, dst);
            yield return wait;

            for (int i = 0; i < 25; i++) {
                var center = new Vector2(width * Random.Range(0.2f, 0.8f), height * Random.Range(0.2f, 0.8f));
                var size   = new Vector2(width * Random.Range(0.1f, 0.3f), height * Random.Range(0.1f, 0.3f));
                var hasOutline = Random.value > 0.5f;
                var hasFill = ! hasOutline || Random.value > 0.5f;
                var fillColor = hasFill ? Random.ColorHSV() : Color.clear;
                var outlineWidth = hasOutline ? Random.Range(2, 20): 0;
                var outlineColor = hasOutline ? Random.ColorHSV() : Color.clear;

                switch (i % 4) {
                    case 0:
                        TextureDraw.Circle(dst, center, size.x,
                            fillColor, outlineWidth, outlineColor);
                        break;
                    case 1:
                        TextureDraw.Ellipse(dst, center, size,
                            fillColor, outlineWidth, outlineColor);
                        break;
                    case 2:
                        TextureDraw.Rectangle(dst, new Rect(center, size),
                            fillColor, outlineWidth, outlineColor);
                        break;
                    case 3:
                        TextureDraw.Line( dst, center - size/2, center + size/2,
                            outlineWidth + Random.Range(1, 10),
                            fillColor, outlineWidth, outlineColor);
                        break;
                }
                yield return shortWait;
            }

        }

    }
}
