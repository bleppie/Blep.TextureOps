Blep.TextureOps
===============

**Blep.TextureOps** is a Unity plugin that allows you to manipulate Textures
using compute shaders. Highlights include an O(n) gaussian blur, affine and
perspective warps, and photoshop blends, It can be used as a lightweight
alternative to OpenCV Note: This has been tested on OSX/Metal and
Windows/DirectX, but should work on all platforms that support compute shaders.
If there are problems, please let me know.

A static class provides all the functionality, for example:
* `TextureOps.Add(srcA, srcB, dst)`
* `TextureOps.Threshold(src, dst, 0.5f)`
* `TextureOps.Circle(src, Color.red, float2(100, 100), 10)`

See more examples in TextureOpsExample.cs.

The TextureCompute class provides some nice utilities for working with Textures
and compute shaders so you can write your own operations.


Installation Instructions
-------------------------

[Install from Git](https://docs.unity3d.com/Manual/upm-ui-giturl.html) or copy
to your Assets folder


Supported Operations
--------------------

Note: many of these can work in-place.

### Math

|             |                                                  |
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

|           |                                            |
|:----------|:-------------------------------------------|
| Circle    | Draw a circle with an optional outline.    |
| Ellipse   | Draw an ellipse with an optional outline.  |
| Rectangle | Draw a rectangle with an optional outline. |
| Line      | Draw a line with an optional outline.      |

### Image Processing

#### Simple

|                |                                     |
|:---------------|:------------------------------------|
| Grayscale      | Convert to grayscale.               |
| GrayscaleGamma | Convert a gamma image to grayscale. |
| ConvertRGB2HSV | Convert RGB to HSV.                 |
| ConvertHSV2RGB | Convert HSV to RGB.                 |
| Swizzle        | Swizzle an image's channels.        |
| Lookup         | Lookup using a pallete.             |
| Contrast       | Adjust image contrast.              |
| Threshold      | Threshold an image.                 |

#### Morphology Etc

|                   |                                             |
|:------------------|:--------------------------------------------|
| Erode             | Erode an image.                             |
| Dilate            | Dilate an image.                            |
| Skeletonize       | Skeletonize an image.                       |
| DistanceTransform | Compute distance to closest non-zero pixel. |

#### Convolution

|                       |                                                                 |
|:----------------------|:----------------------------------------------------------------|
| BlurGaussian          | Blur image with a Gaussian kernel.                              |
| BlurGaussianRecursive | Blur image with fast O(n) algorithm. Imprecise for small blurs. |
| Bilateral             | Bilateral-filter an image.                                      |
| Median3x3             | Median-filter an image with a 3x3 neighborhood.                 |
| Median5x5             | Median-filter an image with a 5x5 neighborhood.                 |
| Sobel                 | Detect edges with a Sobel filter.                               |
| Scharr                | Detect edges with a Scharr filter.                              |

#### Stats

|                   |                                               |
|:------------------|:----------------------------------------------|
| GetHistogram      | Calculate and return image histogram.         |
| EqualizeHistogram | Equalize image histogram.                     |
| Reduce            | Used by MinValue, MaxValue, and AverageValue. |
| MinValue          | Return minimum pixel value.                   |
| MaxValue          | Return maximum pixel value.                   |
| AverageValue      | Return average pixel value.                   |

#### Composition

|             |                                                                                           |
|:------------|:------------------------------------------------------------------------------------------|
| ComposeOver | SrcA appears on top of SrcB, blending them based on their alpha channels.                 |
| ComposeIn   | SrcB acts as a matte for SrcA. SrcA shows only where SrcB is visible.                     |
| ComposeOut  | SrcB acts as a inverse matte for SrcA. SrcA shows only where SrcB is not visible.         |
| ComposeAtop | SrcA is drawn on top of SrcB, but only where SrcB is not transparent.                     |
| ComposeXor  | SrcA and SrcB mutually exclude each other. Where they overlap, the result is transparent. |
| ComposePlus | Adds SrcA and SrcB together; used for additive blending.                                  |

#### Blend

These are based on Photoshop blend modes.

|                  |                                                                                                                                                  |
|:-----------------|:-------------------------------------------------------------------------------------------------------------------------------------------------|
| BlendColorBurn   | Looks at the color information in each channel and darkens the base color to reflect the blend color by increasing the contrast between the two. |
| BlendLinearBurn  | Looks at the color information in each channel and darkens the base color to reflect the blend color by decreasing the brightness.               |
| BlendScreen      | Looks at each channel’s color information and multiplies the inverse of the blend and base colors.                                               |
| BlendColorDodge  | Looks at the color information in each channel and brightens the base color to reflect the blend color by decreasing contrast between the two.   |
| BlendLinearDodge | Looks at the color information in each channel and brightens the base color to reflect the blend color by decreasing contrast between the two.   |
| BlendOverlay     | Multiplies or screens the colors, depending on the base color.                                                                                   |
| BlendSoftLight   | Darkens or lightens the colors, depending on the blend color.                                                                                    |
| BlendHardLight   | Multiplies or screens the colors, depending on the blend color.                                                                                  |
| BlendVividLight  | Burns or dodges the colors by increasing or decreasing the contrast, depending on the blend color.                                               |
| BlendLinearLight | Burns or dodges the colors by decreasing or increasing the brightness, depending on the blend color.                                             |
| BlendPinLight    | Replaces the colorsI, depending on the blend color.                                                                                              |

### Geometric Transformations

Note: Warp functions take an inverse matrix.

|                 |                                                                 |
|:----------------|:----------------------------------------------------------------|
| FlipHorizontal  | Flip an image horizontally.                                     |
| FlipVertical    | Flip an image vertically.                                       |
| Rotate180       | Rotate an image by 180 (flip both horizontally and vertically). |
| WarpAffine      | Warp an image by an affine transformation                       |
| WarpPerspective | Warp an image by an perspective transformation                  |

#### Helper methods

|                            |                                                                   |
|:---------------------------|:------------------------------------------------------------------|
| GetInverseAffineMatrix     | Creates an inverse matrix from a scale, rotation, and translation |
| GetInverseCornerWarpMatrix | Creates an inverse matrix from a four-corner warp                 |

Known Limitations
----------------------------

* Does minimal parameter checking, for example making sure that the
  dimensions of the images in a binary operation are the same size.

License
-------

See LICENSE.md and COPYRIGHT.md
