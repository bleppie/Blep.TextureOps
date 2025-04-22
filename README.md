Blep.TextureOps
===============

**Blep.TextureOps** is a Unity plugin that allows you to manipulate Textures
using compute shaders. It can be used as a lightweight alternative to OpenCV
Note: This has been tested on OSX/Metal and
Windows/DirectX, but should work on all platforms that support compute shaders.
If there are problems, please let me know.

Static classes provide all the functionality, for example:
* `TextureMath.Add(srcA, srcB, dst)`
* `TextureIP.Threshold(src, dst, new Vector4(0.5, 0.5, 0.5, 0))`
* `TextureDraw.Circle(src, Color.red, new Vector2(100, 100), 10)`

See more examples in TextureOpsExample.cs.

The TextureCompute class provides some nice utilities for working with Textures
and compute shaders so you can write your own operations.


Installation Instructions
-------------------------

[Install from Git](https://docs.unity3d.com/Manual/upm-ui-giturl.html) or copy
to your Assets folder


Supported Operations
--------------------

Note: most of these can work in-place.

### Math

| Method      | Description                                      |
|:------------|:-------------------------------------------------|
| Copy        | Dst = Src                                        |
| Clear       | Dst = 0                                          |
| Set         | Dst = Constant                                   |
| Set         | Dst = lerp(Dst, Constant, ChannelMask)           |
| Set         | Dst = lerp(Dst, Constant, ImageMask)             |
| Invert      | Dst = 1 - Src                                    |
| Add         | Dst = Src + Constant                             |
| Add         | Dst = SrcA + SrcB                                |
| Multiply    | Dst = Src * Constant                             |
| Multiply    | Dst = SrcA * SrcB                                |
| MultiplyAdd | Dst = Src * ConstantA + ConstantB                |
| AddWeighted | Dst = SrcA * ConstantA + SrcB * ConstantB        |
| Lerp        | Dst = lerp(SrcA, SrcB, Constant)                 |
| Clamp       | Dst = clamp(Src, ConstantA, ConstantB)           |
| Min         | Dst = min(SrcA, SrcB)                            |
| Max         | Dst = max(SrcA, SrcB)                            |
| Saturate    | Dst = saturate(Src)                              |
| Remap       | Dst = remap(Src, fromMin, fromMax, toMin, toMax) |

### Drawing

| Method    | Description                                |
|:----------|:-------------------------------------------|
| Circle    | Draw a circle with an optional outline.    |
| Ellipse   | Draw an ellipse with an optional outline.  |
| Rectangle | Draw a rectangle with an optional outline. |
| Line      | Draw a line with an optional outline.      |

### Image Processing

#### Simple

| Method         | Description                         |
|:---------------|:------------------------------------|
| Grayscale      | Convert to grayscale.               |
| GrayscaleGamma | Convert a gamma image to grayscale. |
| Threshold      | Threshold an image.                 |
| ConvertRGB2HSV | Convert RGB to HSV.                 |
| ConvertHSV2RGB | Convert HSV to RGB.                 |
| Swizzle        | Swizzle an image's channels.        |
| Lookup         | Lookup using a pallete.             |
| Contrast       | Adjust image contrast.              |

#### Geometric

| Method         | Description                                                     |
|:---------------|:----------------------------------------------------------------|
| FlipHorizontal | Flip an image horizontally.                                     |
| FlipVertical   | Flip an image vertically.                                       |
| Rotate180      | Rotate an image by 180 (flip both horizontally and vertically). |
|                |                                                                 |

#### Morphology Etc

| Method            | Description                                 |
|:------------------|:--------------------------------------------|
| Erode             | Erode an image.                             |
| Dilate            | Dilate an image.                            |
| Skeletonize       | Skeletonize an image.                       |
| DistanceTransform | Compute distance to closest non-zero pixel. |

#### Convolution

| Method                | Description                                                       |
|:----------------------|:------------------------------------------------------------------|
| BlurGaussian          | Blur image with a Gaussian kernel.                                |
| BlurGaussianRecursive | Blur image with fast O(n^2) algorithm. Imprecise for small blurs. |
| Bilateral             | Bilateral-filter an image.                                        |
| Median3x3             | Median-filter an image with a 3x3 neighborhood.                   |
| Median5x5             | Median-filter an image with a 5x5 neighborhood.                   |
| Sobel                 | Detect edges with a Sobel filter.                                 |
| Scharr                | Detect edges with a Scharr filter.                                |

#### Stats

| Method                        | Description                                           |
|:------------------------------|:------------------------------------------------------|
| GetHistogram                  | Calculate and return image histogram.                 |
| EqualizeHistogram             | Equalize image histogram.                             |
| Reduce: used by Min, Max, Sum | Reduce: used by MinValue, MaxValue, and AverageValue. |
| MinValue                      | Return minimum pixel value.                           |
| MaxValue                      | Return maximum pixel value.                           |
| AverageValue                  | Return average pixel value.                           |

#### Composition

| Method      | Description                                                                               |
|:------------|:------------------------------------------------------------------------------------------|
| ComposeOver | SrcA appears on top of SrcB, blending them based on their alpha channels.                 |
| ComposeIn   | SrcB acts as a matte for SrcA. SrcA shows only where SrcB is visible.                     |
| ComposeOut  | SrcB acts as a inverse matte for SrcA. SrcA shows only where SrcB is not visible.         |
| ComposeAtop | SrcA is drawn on top of SrcB, but only where SrcB is not transparent.                     |
| ComposeXor  | SrcA and SrcB mutually exclude each other. Where they overlap, the result is transparent. |
| ComposePlus | Adds SrcA and SrcB together; used for additive blending.                                  |

Known Limitations
----------------------------

* Does minimal parameter checking, for example making sure that the
  dimensions of the images in a binary operation are the same size.
* Unless specified, Math operations do not saturate, so they should be used
  either with high-range-count textures (float, int) or carefully.

License
-------

See LICENSE.md and COPYRIGHT.md
