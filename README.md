Blep.TextureOps
===============

**Blep.TextureOps** is a Unity plugin that allows you to manipulate Textures
using compute shaders. Note: This has been tested nostly on OSX/Metal, but
should work on all platforms that support compute shaders. If there are
problems, please let me know.

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

### Math

* Copy
* Set to Constant
* Set to Constant with a channel mask
* Set to Constant with an alpha (image) mask
* Add Src + Constant
* Add SrcA + SrcB
* AddWeighted SrcA * ConstantA + SrcB * ConstantB
* Lerp(SrcA, SrcB, Constant)
* Multiply Src * Constant
* Multiply SrcA * SrcB
* MultiplyAdd: Src * ConstantA + ConstantB
* Clamp(Src, ConstantA, ConstantB)
* Saturate
* Remap from one range to another

### Drawing

* Circle
* Ellipse
* Rectagle
* Line

### Image Processing

#### Simple
* Grayscale
* GrayscaleGamma: grayscale a gamma image by first converting to linear space
* Threshold
* ConvertRGB2HSV
* ConvertHSV2RGB
* Swizzle
* Lookup using a pallete
* Contrast adjustment

#### Geometric
* FlipHorizontal
* FlipVertical
* Rotate180

#### Morphology Etc
* Erode
* Dilate
* Skeletonize

#### Convolution
* Sobel
* Scharr
* BlurGaussian
* RecursiveConvolve: constant time convolution
* BlurGaussianRecursive: constant time gaussian for big blurs

#### Stats
* GetHistogram as a ComputeBuffer
* GetHistogram as an array
* EqualizeHistogram
* Reduce: used by Min, Max, Sum
* Min
* Max
* Sum

#### Composition
* ComposeOver
* ComposeIn
* ComposeOut
* ComposeAtop
* ComposeXor
* ComposePlus

Known Limitations
----------------------------

* Does absolutely no parameter checking, for example making sure that the
  dimensions of the images in a binary operation are the same size.
* Unless specified, Math operations do not saturate, so they should be used
  either with high-range-count textures (float, int) or carefully.

License
-------

See LICENSE.md
