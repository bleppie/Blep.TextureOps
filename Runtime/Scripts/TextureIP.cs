using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using Blep;

namespace Blep.TextureOps {

public static class TextureIP {

    private static TextureCompute _compute;
    public static TextureCompute compute =>
        (_compute = _compute ?? new TextureCompute("Shaders/Blep/TextureIP"));

    // Reset statics while in the editor and play-mode-options are turned on
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void _Init() { _compute = null; }

    // For convenience
    public static readonly int SrcAId = TextureCompute.SrcAId;
    public static readonly int SrcBId = TextureCompute.SrcBId;
    public static readonly int ScalarAId = TextureCompute.ScalarAId;
    public static readonly int ScalarBId = TextureCompute.ScalarBId;
    public static readonly int DstId = TextureCompute.DstId;

    // -------------------------------------------------------------------------------
    // Color conversion

    public static void Grayscale(Texture src, RenderTexture dst) =>
        compute.UnaryOp("Grayscale", "GrayscaleI", src, dst);

    public static void Grayscale(RenderTexture srcDst) =>
        Grayscale(srcDst, srcDst);

    public static void GrayscaleGamma(Texture src, RenderTexture dst) =>
        compute.UnaryOp("GrayscaleGamma", "GrayscaleGammaI", src, dst);

    public static void GrayscaleGamma(RenderTexture srcDst) =>
        GrayscaleGamma(srcDst, srcDst);


    public static void Threshold(Texture src, RenderTexture dst, float4 threshold) =>
        compute.UnaryOp("Threshold", "ThresholdI", src, dst, threshold);

    public static void Threshold(RenderTexture srcDst, float4 threshold) =>
        Threshold(srcDst, srcDst, threshold);


    public static void ConvertRGB2HSV(Texture src, RenderTexture dst) =>
        compute.UnaryOp("ConvertRGB2HSV", "ConvertRGB2HSVI", src, dst);

    public static void ConvertRGB2HSV(RenderTexture srcDst) =>
        ConvertRGB2HSV(srcDst, srcDst);


    public static void ConvertHSV2RGB(Texture src, RenderTexture dst) =>
        compute.UnaryOp("ConvertHSV2RGB", "ConvertHSV2RGBI", src, dst);

    public static void ConvertHSV2RGB(RenderTexture srcDst) =>
        ConvertHSV2RGB(srcDst, srcDst);


    public static void Swizzle(Texture src, RenderTexture dst, uint4 channels) =>
        compute.UnaryOp("Swizzle", "SwizzleI", src, dst, (float4) channels);

    public static void Swizzle(RenderTexture srcDst, uint4 channels) =>
        Swizzle(srcDst, srcDst, channels);

    public static void Swizzle(Texture src, RenderTexture dst, string pattern) {
        uint4 channels = 0;
        // Map characters to  channels
        for (int i = Mathf.Min(4, pattern.Length); --i >= 0; ) {
            channels[i] = pattern[i] switch {
                'r' or 'x' => 0,
                'g' or 'y' => 1,
                'b' or 'z' => 2,
                'a' or 'w' => 3,
                _ => 0
            };
        }
        Swizzle(src, dst, channels);
    }

    public static void Swizzle(RenderTexture srcDst, string pattern) =>
        Swizzle(srcDst, srcDst, pattern);


    public static void Lookup(Texture src, RenderTexture dst, Texture pallete) =>
        compute.BinaryOp("Lookup", "LookupI", src, pallete, dst);

    public static void Lookup(RenderTexture srcDst, Texture pallete) =>
        Lookup(srcDst, srcDst, pallete);


    public static void Contrast(Texture src, RenderTexture dst, float amount) {
        // Map negative values to 1/(-amount+1) and positive to amount+1
        amount = amount < 0 ? (1.0f / (1.0f - amount)) : 1.0f + amount;

        // (x - 0.5) * amount + 0.5 == x * amount + 0.5 - 0.5 * amount
        var scale = float4(amount, amount, amount, 1);
        var offset = 0.5f * (1 - scale);
        TextureMath.MultiplyAdd(src, dst, scale, offset);
    }

    public static void Contrast(RenderTexture srcDst, float amount) =>
        Contrast(srcDst, srcDst, amount);


    // -------------------------------------------------------------------------------
    // Geometric

    // When flipping/rotating in place, dispatch on only half the texture and swap pixels
    private static void _PartialDispatch(string kernelName, Texture src, RenderTexture dst,
                                         int dispatchWidth, int dispatchHeight) {
        int kernel = compute.FindKernel(kernelName);
        compute.SetSize(dst.width, dst.height);
        compute.shader.SetTexture(kernel, DstId, dst);
        compute.GetKernelThreadGroups(kernel, dispatchWidth, dispatchHeight, out int x, out int y);
        compute.Dispatch(kernel, x, y);
    }

    public static void FlipHorizontal(Texture src, RenderTexture dst) {
        if (src != dst) {
            compute.UnaryOp("FlipHorizontal", src, dst);
        }
        else {
            _PartialDispatch("FlipHorizontalI", src, dst, (dst.width + 1) / 2, dst.height);
        }
    }

    public static void FlipHorizontal(RenderTexture srcDst) =>
        FlipHorizontal(srcDst, srcDst);

    public static void FlipVertical(Texture src, RenderTexture dst) {
        if (src != dst) {
            compute.UnaryOp("FlipVertical", src, dst);
        }
        else {
            _PartialDispatch("FlipVerticalI", src, dst, dst.width, (dst.height + 1) / 2);
        }
    }

    public static void FlipVertical(RenderTexture srcDst) =>
        FlipVertical(srcDst, srcDst);

    public static void Rotate180(Texture src, RenderTexture dst) {
        if (src != dst) {
            compute.UnaryOp("Rotate180", src, dst);
        }
        else {
            _PartialDispatch("Rotate180I", src, dst, dst.width, (dst.height + 1) / 2);
        }
    }

    public static void Rotate180(RenderTexture srcDst) =>
        Rotate180(srcDst, srcDst);

    // -------------------------------------------------------------------------------
    // Misc/Experimental

    // Calculates distance from each pixel to nearest non-zero pixel. If the
    // desination texture is 4 channels, it will be filled with the distance-to,
    // coordinates-of, and contents-of the nearest non-zero pixel. To avoid the
    // sqrt calculation, pass true to sqrDistance.

    // Distance transform using jump flooding inspired by 
    // https://github.com/alpacasking/JumpFloodingAlgorithm

    public static void DistanceTransform(Texture src, RenderTexture dst, bool sqrDistance=false) {
        using TextureTmp _tmp1 = new(src.width, src.height, RenderTextureFormat.ARGBFloat); 
        using TextureTmp _tmp2 = new(src.width, src.height, RenderTextureFormat.ARGBFloat);
        TextureTmp tmp1 = _tmp1;
        TextureTmp tmp2 = _tmp2;

        compute.UnaryOp("DistanceTransformInit", src, tmp1);

        int stepKernel = compute.FindKernel("DistanceTransformStep");
        int numPasses = (int) Mathf.Ceil(Mathf.Log(Mathf.Max(src.width, src.height), 2));
        int step =  1 << (numPasses - 1);
        for (int i = 0; i < numPasses; i++) {
            compute.UnaryOp(stepKernel, tmp1, tmp2, new float4(step, 0, 0, 0));
            (tmp1, tmp2) = (tmp2, tmp1);
            step >>= 1;
        }

        if (! sqrDistance) {
            compute.UnaryOp("DistanceTransformSqrt", tmp1, tmp2);
            (tmp1, tmp2) = (tmp2, tmp1);
        }
        TextureMath.Copy(tmp1, dst);
    }

    // -------------------------------------------------------------------------------
    // Morphology & Skeletonization

    // Test gathering pixels into groupshared memory. This is the same or slower
    // than the simple approach on OSX/Metal, See comments in TextureIP.compute
    // public static void ErodeGather(Texture src, RenderTexture dst) {
    //     compute.SetSize(src.width, src.height);
    //     var kernel = compute.FindKernel("ErodeGather");
    //     // Overlap tiles by 1 pixel each (2 pixels overlap), so process in groups of THREADS-2
    //     compute.GetKernelThreadGroupSizes(kernel, out int xs, out int ys);
    //     xs -= 2;
    //     ys -= 2;
    //     int x = (src.width  + xs - 1) / xs; // Round up
    //     int y = (src.height + ys - 1) / ys; // Round up
    //     compute.shader.SetTexture(kernel, SrcAId, src);
    //     compute.shader.SetTexture(kernel, DstId, dst);
    //     compute.Dispatch(kernel, x, y);
    // }

    public static void Erode(Texture src, RenderTexture dst) {
        Assert.AreNotEqual(src, dst, "Erode cannot work in place");
        compute.UnaryOp("Erode", src, dst);
    }

    public static void Dilate(Texture src, RenderTexture dst) {
        Assert.AreNotEqual(src, dst, "Dilate cannot work in place");
        compute.UnaryOp("Dilate", src, dst);
    }

    public static void Skeletonize(Texture src, RenderTexture dst, int iterations) {
        using var tmp = new TextureTmp(src);

        int kernel = compute.FindKernel("Skeletonize");
        for (int i = 0; i < iterations; i++) {
            compute.UnaryOp(kernel, i == 0 ? src : dst, tmp, 0);
            compute.UnaryOp(kernel, tmp, dst, 1);
        }
    }

    public static void Skeletonize(RenderTexture srcDst, int iterations) =>
        Skeletonize(srcDst, srcDst, iterations);

    public static void Sobel(Texture src, RenderTexture dst) {
        Assert.AreNotEqual(src, dst, "Sobel cannot work in place");
        compute.UnaryOp("Sobel", src, dst);
    }

    public static void Scharr(Texture src, RenderTexture dst) {
        Assert.AreNotEqual(src, dst, "Scharr cannot work in place");
        compute.UnaryOp("Scharr", src, dst);
    }

    // -------------------------------------------------------------------------------
    // Convolution

    public static void Median3x3(Texture src, RenderTexture dst) {
        Assert.AreNotEqual(src, dst, "Median cannot work in place");
        compute.UnaryOp("Median3x3", src, dst);
    }

    public static void Median5x5(Texture src, RenderTexture dst) {
        Assert.AreNotEqual(src, dst, "Median cannot work in place");
        compute.UnaryOp("Median5x5", src, dst);
    }

    private static float4 _CalculateGaussianCoeffs(float size, float sigma) {
        // Copying what OpenCV does: sigma = 0.3*((size-1)*0.5 - 1) + 0.8 = 0.35*size + 0.35
        // https://docs.opencv.org/2.4/modules/imgproc/doc/filtering.html#getgaussiankernel
        // Interesting explanation here:
        // https://stackoverflow.com/questions/14060017/calculate-the-gaussian-filters-sigma-using-the-kernels-size
        if (sigma <= 0) sigma = 0.15f * size + 0.35f;

        float4 incGauss;
        incGauss.x = 1.0f / (Mathf.Sqrt(2.0f * 3.1415f) * sigma);
        incGauss.y = Mathf.Exp(-0.5f / (sigma * sigma));
        incGauss.z = incGauss.y * incGauss.y;
        incGauss.w = size;
        return incGauss;
    }

    public static void Bilateral(Texture src, RenderTexture dst,
                                 float size, float sigma=-1, float colorSigma=0.1f) {
        Assert.AreNotEqual(src, dst, "Bilateral cannot work in place");
        float4 incGauss = _CalculateGaussianCoeffs(size, sigma);
        compute.UnaryOp("Bilateral", src, dst, incGauss,
                        float4(-0.5f / (colorSigma * colorSigma), 0, 0, 0));
    }

    public static void BlurGaussian(Texture src, RenderTexture dst,
                                    float size, float sigma=-1) {
        using var tmp = new TextureTmp(dst);

        float4 incGauss = _CalculateGaussianCoeffs(size, sigma);
        int kernel = compute.FindKernel("BlurGaussian");

        // Horizontal
        compute.UnaryOp(kernel, src, tmp, incGauss, float4(1, 0, 0, 0));
        // Vertical
        compute.UnaryOp(kernel, tmp, dst, incGauss, float4(0, 1, 0, 0));
    }

    public static void BlurGaussian(RenderTexture srcDst,
                                    float size, float sigma=-1) =>
        BlurGaussian(srcDst, srcDst, size, sigma);


    public static void RecursiveConvolve(Texture src, RenderTexture dst, float4 coeffs) {
        // Tmp has transposed aspect ratio (height x width rather than width x height)
        using var tmp = new TextureTmp(dst.height, dst.width, dst.format);

        var shader = compute.shader;
        var fwdKernelI = compute.FindKernel("RecursiveConvolveFwdI"); // In-place fwd kernel
        var fwdKernel = src == dst ? fwdKernelI : compute.FindKernel("RecursiveConvolveFwd");
        var bakKernel = compute.FindKernel("RecursiveConvolveBak");

        shader.SetVector(ScalarAId, coeffs);

        // Asssume fwdKernel same as bakKernel
        int xs, ys;
        compute.GetKernelThreadGroupSizes(fwdKernel, out xs, out ys);
        int threadsX = (src.height + xs - 1) / xs;
        int threadsY = (src.width  + ys - 1) / ys;

        // Because of the way texture data is stored, it's faster to access
        // pixels by rows than by columns. So, rather than convolving each row
        // forwards then backwards, and then each column forwards then
        // backwards, it's faster to convolve each row forwards then backwards,
        // transpose, amd repeat. The transposition step is folded into the
        // backwards convolution (see the compute shader).

        compute.SetSize(src.width, src.height);

        // Convolve forward src -> dst
        shader.SetTexture(fwdKernel, SrcAId, src);
        shader.SetTexture(fwdKernel, DstId, dst);
        compute.Dispatch(fwdKernel, threadsX, 1);

        // Convolve backward and transpose dst -> tmp
        shader.SetTexture(bakKernel, SrcAId, dst);
        shader.SetTexture(bakKernel, DstId, tmp);
        compute.Dispatch(bakKernel, threadsX, 1);

        compute.SetSize(src.height, src.width);

        // Convolve forward tmp -> tmp
        shader.SetTexture(fwdKernelI, SrcAId, tmp);
        shader.SetTexture(fwdKernelI, DstId, tmp);
        compute.Dispatch(fwdKernelI, threadsY, 1);

        // Convolve backward and transpose tmp -> dst
        shader.SetTexture(bakKernel, SrcAId, tmp);
        shader.SetTexture(bakKernel, DstId, dst);
        compute.Dispatch(bakKernel, threadsY, 1);
    }

    // https://www.researchgate.net/publication/222453003_Recursive_implementation_of_the_Gaussian_filter
    public static float4 GetRecursizeGaussianCoeffs(float sigma) {

        // Calculate filter parameters for a specified sigma
        double q;
        if (sigma >= 2.5) {
            q = 0.98711 * sigma - 0.96330;
        }
        else if (sigma >= 0.5) {
            q = 3.97156 - 4.14554 * System.Math.Sqrt(1.0 - 0.26891 * sigma);
        }
        else {
            q = 0; // No blur
        }
        var q2 = q * q;
        var q3 = q * q2;

        var b0 = 1.57825 + 2.44413 * q + 1.4281 * q2 + 0.422205 * q3;
        var b1 = (2.44413 * q + 2.85619 * q2 + 1.26661 * q3) / b0;
        var b2 = (-1.4281 * q2 - 1.26661 * q3) / b0;
        var b3 = (0.422205 * q3) / b0;
        var B = 1.0 - b1 - b2 - b3;

        return float4((float) B, (float) b1, (float) b2, (float) b3);
    }

    public static void BlurGaussianRecursive(Texture src, RenderTexture dst, float sigma) =>
        RecursiveConvolve(src, dst, GetRecursizeGaussianCoeffs(sigma));

    public static void BlurGaussianRecursive(RenderTexture srcDst, float sigma) =>
        BlurGaussianRecursive(srcDst, srcDst, sigma);

    // -------------------------------------------------------------------------------
    // Histogram

    // Returns the src histogram in a ComputeBuffer. Make sure to release the
    // buffer when done with it.
    public static ComputeBuffer GetHistogramBuffer(Texture src, ComputeBuffer histogramBuffer=null) {

        histogramBuffer = histogramBuffer ?? new ComputeBuffer(256, sizeof(uint) * 4);

        var shader = compute.shader;
        compute.SetSize(src.width, src.height);

        // Clear the histogram
        int clearKernel = shader.FindKernel("HistogramEqClear");
        shader.SetBuffer(clearKernel, "Histogram", histogramBuffer);
        compute.Dispatch(clearKernel, 1, 1);

        // Create the histogram
        int gatherKernel = shader.FindKernel("HistogramEqGather");
        shader.SetBuffer(gatherKernel, "Histogram", histogramBuffer);
        shader.SetTexture(gatherKernel, SrcAId, src);
        compute.Dispatch(gatherKernel);

        return histogramBuffer;
    }

    // Returns the src histogram in a Array
    public static float4[] GetHistogram(Texture src, float4[] histogram=null) {
        histogram = histogram ?? new float4[256];
        var histogramBuffer = GetHistogramBuffer(src);

        uint[] tmp = new uint[256 * 4];
        histogramBuffer.GetData(tmp);

        for (int i = 0; i < 256; i++) {
            histogram[i].x = tmp[i * 4];
            histogram[i].y = tmp[i * 4 + 1];
            histogram[i].z = tmp[i * 4 + 2];
            histogram[i].w = tmp[i * 4 + 3];
        }

        histogramBuffer.Release();
        return histogram;
    }

    public static void EqualizeHistogram(Texture src, RenderTexture dst) {

        var histogramBuffer = GetHistogramBuffer(src);

        var shader = compute.shader;
        compute.SetSize(src.width, src.height);

        // Sum the histogram
        int accumulateKernel = shader.FindKernel("HistogramEqAccumulate");
        shader.SetBuffer(accumulateKernel, "Histogram", histogramBuffer);
        compute.Dispatch(accumulateKernel, 1, 1);

        // Remap input to output
        int mapKernel = shader.FindKernel("HistogramEqMap");
        shader.SetBuffer(mapKernel, "Histogram", histogramBuffer);
        shader.SetTexture(mapKernel, SrcAId, src);
        shader.SetTexture(mapKernel, DstId, dst);
        compute.Dispatch(mapKernel);

        histogramBuffer.Release();
    }

    public static void EqualizeHistogram(RenderTexture srcDst) =>
        EqualizeHistogram(srcDst, srcDst);

    // -------------------------------------------------------------------------------
    // Stats

    // Dispatches the kernel multiple times, starting at half the size of the
    // src image and halving the size each iteration. When down to a 1x1 image,
    // returns the value of the single pixel in this imagew.
    public static float4 Reduce(string kernelName, Texture src) {
        using var tmp = new TextureTmp(src, forceFloatFormat: true);

        int kernel = compute.FindKernel(kernelName);
        var shader = compute.shader;

        // The kernels work in place, so copy src to tmp
        TextureMath.Copy(src, tmp);
        shader.SetTexture(kernel, DstId, tmp);

        int width = src.width;
        int height = src.height;

        while (width > 1 && height > 1) {
            int halfWidth  = (width  + 1) >> 1;
            int halfHeight = (height + 1) >> 1;

            compute.SetSize(width, height);
            compute.GetKernelThreadGroups(kernel, halfWidth, halfHeight, out int xg, out int yg);
            compute.Dispatch(kernel, xg, yg);
            width  = halfWidth;
            height = halfHeight;
        }

        float4 pixel = 0;
        if (SystemInfo.supportsAsyncGPUReadback) {
            var request = AsyncGPUReadback.Request(tmp, 0, 0, 1, 0, 1, 0, 1,
                                                   GraphicsFormat.R32G32B32A32_SFloat, null);
            request.WaitForCompletion();
            pixel = request.GetData<float4>(0)[0];
        }
        else {
            RenderTexture.active = tmp;
            var data = new Texture2D(1, 1, tmp.texture.graphicsFormat, 0);
            data.ReadPixels(new Rect(0, 0, 1, 1), 0, 0);
            pixel = (Vector4) data.GetPixel(0, 0);
            Object.Destroy(data);
            RenderTexture.active = null;
        }

        return pixel;
    }

    // Returns maximum pixel value
    public static float4 MaxValue(Texture src) =>
        Reduce("MaxReduce", src);

    // Returns minimum pixel value
    public static float4 MinValue(Texture src) =>
        Reduce("MinReduce", src);

    // Returns averate pixel value
    public static float4 AverageValue(Texture src) =>
        Reduce("SumReduce", src) / (src.width * src.height);

    // -------------------------------------------------------------------------------
    // Composition

    public static void Premultiply(Texture src, RenderTexture dst) =>
        compute.UnaryOp("Premultiply", "PremultiplyI", src, dst);

    public static void Premultiply(RenderTexture srcDst) =>
        Premultiply(srcDst, srcDst);

    // Over
    // αA*A+(1- αA)* αB*B
    // αA+(1-αA)* αB
    // A occludes B
    public static void ComposeOver(Texture srcA, Texture srcB, RenderTexture dst) =>
        compute.BinaryOp("ComposeOver", srcA, srcB, dst);

    // IN
    // αA*A*αB
    // αA*αB
    // A within B. B acts as a matte for A. A shows only where B is visible.
    public static void ComposeIn(Texture srcA, Texture srcB, RenderTexture dst) =>
        compute.BinaryOp("ComposeIn", srcA, srcB, dst);

    // OUT
    // αA*A*(1-αB)
    // αA*(1-αB)
    // A outside B. NOT-B acts as a matte for A. A shows only where B is not visible.
    public static void ComposeOut(Texture srcA, Texture srcB, RenderTexture dst) =>
        compute.BinaryOp("ComposeOut", srcA, srcB, dst);

    // ATOP
    // αA*A*αB+(1- αA)* αB*B
    // αA*αB+(1- αA)* αB
    public static void ComposeAtop(Texture srcA, Texture srcB, RenderTexture dst) =>
        compute.BinaryOp("ComposeAtop", srcA, srcB, dst);

    // XOR
    // αA*A*(1-αB)+(1- αA)* αB*B = lerp(aB * B, A * (1 - aB), aA) = lerp(aA * A, (1-aA)*B, aB)
    // αA*(1-αB)+(1- αA)* αB =
    // Combination of (A OUT B) and (B OUT A). A and B mutually exclude each other.
    public static void ComposeXor(Texture srcA, Texture srcB, RenderTexture dst) =>
        compute.BinaryOp("ComposeXor", srcA, srcB, dst);

    // PLUS
    // αA*A+αB*B
    // αA+αB
    // Blend without precedence
    public static void ComposePlus(Texture srcA, Texture srcB, RenderTexture dst) =>
        compute.BinaryOp("ComposePlus", srcA, srcB, dst);

}

}
