using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Blep;

namespace Blep.TextureOps {

public static class TextureIP {

    private static TextureCompute _compute;
    public static TextureCompute compute =>
        (_compute = _compute ?? new TextureCompute("Shaders/TextureIP"));

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
        compute.UnaryOp("Grayscale", src, dst);

		public static void Grayscale(RenderTexture srcDst) =>
        Grayscale(srcDst, srcDst);


		public static void GrayscaleGamma(Texture src, RenderTexture dst) =>
        compute.UnaryOp("GrayscaleGamma", src, dst);

		public static void GrayscaleGamma(RenderTexture srcDst) =>
        GrayscaleGamma(srcDst, srcDst);

    public static void Threshold(Texture src, RenderTexture dst, Vector4 threshold) =>
        compute.UnaryOp("Threshold", src, dst, threshold);

    public static void Threshold(RenderTexture srcDst, Vector4 threshold) =>
        Threshold(srcDst, srcDst, threshold);


		public static void ConvertRGB2HSV(Texture src, RenderTexture dst) =>
        compute.UnaryOp("ConvertRGB2HSV", src, dst);

		public static void ConvertRGB2HSV(RenderTexture srcDst) =>
        ConvertRGB2HSV(srcDst, srcDst);


		public static void ConvertHSV2RGB(Texture src, RenderTexture dst) =>
        compute.UnaryOp("ConvertHSV2RGB", src, dst);

		public static void ConvertHSFV2RGB(RenderTexture srcDst) =>
        ConvertHSV2RGB(srcDst, srcDst);


		public static void Swizzle(Texture src, RenderTexture dst, Vector4 channels) =>
        compute.UnaryOp("Swizzle", src, dst, channels);

		public static void Swizzle(RenderTexture srcDst, Vector4 channels) =>
        Swizzle(srcDst, srcDst, channels);

		public static void Swizzle(Texture src, RenderTexture dst, string pattern) {
        Vector4 channels = Vector4.zero;
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
        compute.BinaryOp("Lookup", src, pallete, dst);

    public static void Lookup(RenderTexture srcDst, Texture pallete) =>
        Lookup(srcDst, srcDst, pallete);


		public static void Contrast(Texture src, RenderTexture dst, float amount) {
        // Map negative values to 1/(-amount+1) and positive to amount+1
        amount = amount < 0 ? (1.0f / (1.0f - amount)) : 1.0f + amount;

        // (x - 0.5) * amount + 0.5 := x * amount + 0.5 - 0.5 * amount
        var scale = new Vector4(amount, amount, amount, 1);
        var offset = 0.5f * (Vector4.one - scale);
        TextureMath.MultiplyAdd(src, dst, scale, offset, saturate: true);
    }

    public static void Contrast(RenderTexture srcDst, float amount) =>
        Contrast(srcDst, srcDst, amount);


    // -------------------------------------------------------------------------------
    // Geometric

		public static void FlipHorizontal(Texture src, RenderTexture dst) =>
        compute.UnaryOp("FlipHorizontal", src, dst);

		public static void FlipHorizontal(RenderTexture srcDst) =>
        FlipHorizontal(srcDst, srcDst);

		public static void FlipVertical(Texture src, RenderTexture dst) =>
        compute.UnaryOp("FlipVertical", src, dst);

		public static void FlipVertical(RenderTexture srcDst) =>
        FlipVertical(srcDst, srcDst);

		public static void Rotate180(Texture src, RenderTexture dst) =>
        compute.UnaryOp("Rotate180", src, dst);

		public static void Rotate180(RenderTexture srcDst) =>
        Rotate180(srcDst, srcDst);

    // -------------------------------------------------------------------------------
    // Morphology & Skeletonization

    public static void Erode(Texture src, RenderTexture dst) =>
        compute.UnaryOp("Erode", src, dst);

    public static void Dilate(Texture src, RenderTexture dst) =>
        compute.UnaryOp("Dilate", src, dst);

    public static void Skeletonize(Texture src, RenderTexture dst,
                                   int iterations,
                                   RenderTexture tmp_=null) {
        var tmp = tmp_ ?? TextureCompute.CreateRenderTexture(src);

        int kernel = compute.FindKernel("Skeletonize");
        for (int i = 0; i < iterations; i++) {
            compute.UnaryOp(kernel, i == 0 ? src : dst, tmp, Vector4.zero);
            compute.UnaryOp(kernel, tmp, dst, Vector4.one);
        }
        TextureDraw.Border(dst, Vector4.zero, 1);

        if (tmp != tmp_) Object.Destroy(tmp);
    }

    public static void Skeletonize(RenderTexture srcDst,
                                   int iterations,
                                   RenderTexture tmp_=null) =>
        Skeletonize(srcDst, srcDst, iterations, tmp_);

    public static void Sobel(Texture src, RenderTexture dst) =>
        compute.UnaryOp("Sobel", src, dst);

    public static void Scharr(Texture src, RenderTexture dst) =>
        compute.UnaryOp("Scharr", src, dst);

    // -------------------------------------------------------------------------------
    // Blurring

		public static void BlurGaussian(Texture src, RenderTexture dst,
																		float size, float sigma,
                                    RenderTexture tmp_=null) {
        var tmp = tmp_ ?? TextureCompute.CreateRenderTexture(dst);

        if (sigma < 0) sigma = sigma = size / 4;

        Vector4 incGauss;
        incGauss.x = 1.0f / (Mathf.Sqrt(2.0f * 3.1415f) * sigma);
        incGauss.y = Mathf.Exp(-0.5f / (sigma * sigma));
        incGauss.z = incGauss.y * incGauss.y;
        incGauss.w = size;

        int kernel = compute.FindKernel("BlurGaussian");
        compute.UnaryOp(kernel, src, tmp, incGauss, new Vector2(1, 0));
        compute.UnaryOp(kernel, tmp, dst, incGauss, new Vector2(0, 1));

        if (tmp != tmp_) Object.Destroy(tmp);
    }

		public static void BlurGaussian(RenderTexture srcDst,
																		float size, float sigma,
                                    RenderTexture tmp) =>
        BlurGaussian(srcDst, srcDst, size, sigma, tmp);


		public static void RecursiveConvolve(Texture src, RenderTexture dst,
                                         Vector4 coeffs, RenderTexture tmp_=null) {
        var tmp = tmp_ ?? TextureCompute.CreateRenderTexture(dst);

        var shader = compute.shader;
        var kernel = compute.FindKernel("RecursiveConvolve");

        compute.SetSize(src.width, src.height);
        shader.SetVector(ScalarAId, coeffs);

        int xs, ys;
        compute.GetKernelThreadGroupSizes(kernel, out xs, out ys);
        int threadsX = (src.height + xs - 1) / xs;
        int threadsY = (src.width  + xs - 1) / xs;

        shader.SetTexture(kernel, SrcAId, src);
        shader.SetTexture(kernel, DstId, tmp);
        shader.SetVector(ScalarBId, new Vector2(1, 0));
        compute.Dispatch(kernel, threadsX, 1);

        shader.SetTexture(kernel, SrcAId, tmp);
        shader.SetTexture(kernel, DstId, dst);
        shader.SetVector(ScalarBId, new Vector4(-1, 0));
        compute.Dispatch(kernel, threadsX, 1);

        shader.SetTexture(kernel, SrcAId, dst);
        shader.SetTexture(kernel, DstId, tmp);
        shader.SetVector(ScalarBId, new Vector4(0, 1));
        compute.Dispatch(kernel, threadsY, 1);

        shader.SetTexture(kernel, SrcAId, tmp);
        shader.SetTexture(kernel, DstId, dst);
        shader.SetVector(ScalarBId, new Vector4(0, -1));
        compute.Dispatch(kernel, threadsY, 1);

        if (tmp != tmp_) Object.Destroy(tmp);
    }

    // https://www.researchgate.net/publication/222453003_Recursive_implementation_of_the_Gaussian_filter
    public static Vector4 GetRecursizeGaussianCoeffs(float sigma) {

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

        return new Vector4((float) B, (float) b1, (float) b2, (float) b3);
    }

		public static void BlurGaussianRecursive(Texture src, RenderTexture dst,
                                             float sigma, RenderTexture tmp_=null) =>
        RecursiveConvolve(src, dst, GetRecursizeGaussianCoeffs(sigma), tmp_); //

		public static void BlurGaussianRecursive(RenderTexture srcDst,
                                             float sigma, RenderTexture tmp_=null) =>
        BlurGaussianRecursive(srcDst, srcDst, sigma, tmp_);

    // -------------------------------------------------------------------------------
    // Histogram

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

    public static Vector4[] GetHistogram(Texture src, Vector4[] histogram=null) {
        histogram = histogram ?? new Vector4[256];
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

		public static Color Reduce(string kernelName, Texture src, RenderTexture tmp_=null) {
        var tmp = tmp_ ?? TextureCompute.CreateFloatRenderTexture(src);

        int kernel = compute.FindKernel(kernelName);
        var shader = compute.shader;

        int width = src.width;
        int height = src.height;

        shader.SetTexture(kernel, SrcAId, src);;

        while (width > 0 && height > 0) {
            compute.SetSize(width, height);
            shader.SetTexture(kernel, DstId, tmp);
            compute.Dispatch(kernel);
            width >>= 1;
            height >>= 1;

            // Work in-place after the first iteration
            shader.SetTexture(kernel, SrcAId, tmp);;
        }

        // Read pixel at 0, 0 // TODO/SPEED: cache this
        var data = new Texture2D(1, 1, tmp.graphicsFormat, 0);
        RenderTexture.active = tmp;
        data.ReadPixels(new Rect(0, 0, 1, 1), 0, 0);
        RenderTexture.active = null;
        var pixel = data.GetPixel(0, 0);
        Object.Destroy(data);

        if (tmp != tmp_) Object.Destroy(tmp);
        return pixel;
    }

    public static Color Max(Texture src, RenderTexture tmp=null) =>
        Reduce("MaxReduce", src, tmp);

    public static Color Min(Texture src, RenderTexture tmp=null) =>
        Reduce("MinReduce", src, tmp);

    public static Color Sum(Texture src, RenderTexture tmp=null) =>
        Reduce("SumReduce", src, tmp);

    // -------------------------------------------------------------------------------
    // Composition

		public static void ComposeOver(Texture srcA, Texture srcB, RenderTexture dst) =>
        compute.BinaryOp("ComposeOver", srcA, srcB, dst);

		public static void ComposeIn(Texture srcA, Texture srcB, RenderTexture dst) =>
        compute.BinaryOp("ComposeIn", srcA, srcB, dst);

		public static void ComposeOut(Texture srcA, Texture srcB, RenderTexture dst) =>
        compute.BinaryOp("ComposeOut", srcA, srcB, dst);

		public static void ComposeAtop(Texture srcA, Texture srcB, RenderTexture dst) =>
        compute.BinaryOp("ComposeAtop", srcA, srcB, dst);

		public static void ComposeXor(Texture srcA, Texture srcB, RenderTexture dst) =>
        compute.BinaryOp("ComposeXor", srcA, srcB, dst);

		public static void ComposePlus(Texture srcA, Texture srcB, RenderTexture dst) =>
        compute.BinaryOp("ComposePlus", srcA, srcB, dst);

}

}
