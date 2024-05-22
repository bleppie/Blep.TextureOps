using System.Collections;
using UnityEngine;
using Blep.TextureOps;

public class TextureOpsExample : MonoBehaviour {

    public Texture2D src;
    public Texture2D overlay1;
    public Texture2D overlay2;
    public Texture2D gradient;
    public RenderTexture dst;

    [Range(1,100)]
    public float test = 10;

    public IEnumerator Start() {
        var wait = new WaitForSeconds(0.5f);

        var tmp = TextureCompute.GetTemporary(dst);

        while (true) {

            //// Draw

            TextureDraw.Circle(src, dst, new Color(1, 0, 0, 0.5f),
                new Vector2(src.width/2, src.height/2), src.height/4, 20);
            yield return wait;

            TextureDraw.Circle(dst, new Color(0, 1, 0, 0.5f),
                new Vector2(src.width/4, src.height/4), src.height/8, 2);
            yield return wait;

            TextureDraw.Line(src, dst, new Color(1, 0, 0, 0.5f),
                new Vector2(100, 100), new Vector2(src.width - 100, src.height - 100),
                20, 1);
            yield return wait;

            TextureDraw.Line(dst, new Color(0, 1, 0, 0.5f),
                new Vector2(src.width - 100, 100), new Vector2(100, src.height - 100),
                20, 1);
            yield return wait;

            TextureDraw.Border(src, dst, new Color(1, 0, 0, 0.5f), 40, 5);
            yield return wait;

            TextureDraw.Border(dst, new Color(0, 1, 0, 0.5f), 20, 5);
            yield return wait;


            //// Math

            TextureMath.Copy(src, dst);
            yield return wait;

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
        }

    }
}
