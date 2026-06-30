
// -------------------------------------------------------------------------------
// Colorspace Functions

// Converts color in linear space  to Unity gamma space
inline float4 Linear2Gamma(float4 color) {
    // Approximation
    // from http://chilliant.blogspot.com.au/2012/08/srgb-approximations-for-hlsl.html?m=1
    color.rgb = max(color.rgb, float3(0, 0, 0));
    color.rgb = max(1.055 * pow(color.rgb, 0.416666667) - 0.055, 0);
    return color;
}

// Converts color in Unity gamma space to linear space
inline float4 Gamma2Linear(float4 color) {
    // Approximation
    // From https://web.archive.org/web/20200207113336/http://lolengine.net/blog/2013/07/27/rgb-to-hsv-in-glsl
    color.rgb = color.rgb * (color.rgb * (color.rgb * 0.305306011 + 0.682171111) + 0.012522878);
    return color;
}

inline float4 RGB2HSV(float4 c) {
    // From https://web.archive.org/web/20200207113336/http://lolengine.net/blog/2013/07/27/rgb-to-hsv-in-glsl
    float4 K = float4(0.0f, -1.0f / 3.0f, 2.0f / 3.0f, -1.0f);
    float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
    float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10f;
    return float4(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x, c.a);
}

inline float4 HSV2RGB(float4 c) {
    //  https://gamedev.stackexchange.com/questions/59797/glsl-shader-change-hue-saturation-brightness
    float4 K = float4(1.0f, 2.0f / 3.0f, 1.0f / 3.0f, 3.0f);
    float3 p = abs(frac(c.xxx + K.xyz) * 6.0f - K.www);
    return float4(c.z * lerp(K.xxx, saturate(p - K.xxx), c.y), c.a);
}

float4 Grayscale(float4 color) {
    float lum = dot(color.rgb, float3(0.2126, 0.7152, 0.0722));
    return float4(lum, lum, lum, color.a);
}

float4 GrayscaleGamma(float4 color) {
    color = Gamma2Linear(color);
    float lum = dot(color.rgb, float3(0.2126, 0.7152, 0.0722));
    return float4(lum, lum, lum, color.a);
}

float4 Swizzle(float4 color, uint4 channels) {
    return float4(color[channels.x],
                  color[channels.y],
                  color[channels.z],
                  color[channels.w]);
}

float4 Lookup(float4 color, Texture2D<float4> pallete) {
  return float4(SampleLinear(pallete, float2(color.r, 0)).r,
                SampleLinear(pallete, float2(color.g, 0)).g,
                SampleLinear(pallete, float2(color.b, 0)).b,
                SampleLinear(pallete, float2(color.a, 0)).a);
}

// -------------------------------------------------------------------------------
// Composition Functions

// Over
// (αA*A+(1- αA)* αB*B, αA+(1-αA)* αB)
// A occludes B
float4 ComposeOver(float4 colorA, float4 colorB) {
  colorA.rgb *= colorA.a;
  colorB.rgb *= colorB.a;
  return lerp(colorB, float4(colorA.rgb, 1), colorA.a);
}

// IN
// (αA*A*αB, αA*αB)
// A within B. B acts as a matte for A. A shows only where B is visible.
float4 ComposeIn(float4 colorA, float4 colorB) {
  colorA.rgb *= colorA.a;
  return colorA * colorB.a;
}

// OUT
// (αA*A*(1-αB), αA*(1-αB))
// A outside B. NOT-B acts as a matte for A. A shows only where B is not visible.
float4 ComposeOut(float4 colorA, float4 colorB) {
  colorA.rgb *= colorA.a;
  return colorA * (1 - colorB.a);
}

// ATOP
// (αA*A*αB+(1- αA)* αB*B, αA*αB+(1- αA)* αB)
float4 ComposeAtop(float4 colorA, float4 colorB) {
  return float4(lerp(colorB.rgb, colorA.rgb, colorA.a) * colorB.a, colorB.a);
}

// XOR
// αA*A*(1-αB)+(1- αA)* αB*B = lerp(aB * B, A * (1 - aB), aA) = lerp(aA * A, (1-aA)*B, aB)
// αA*(1-αB)+(1- αA)* αB
// Combination of (A OUT B) and (B OUT A). A and B mutually exclude each other.
float4 ComposeXor(float4 colorA, float4 colorB) {
  colorA.rgb *= colorA.a;
  colorB.rgb *= colorB.a;
  return saturate(colorA * (1 - colorB.a) + colorB * (1 - colorA.a));
}

// PLUS
// (αA*A+αB*B, αA+αB)
// Blend without precedence
float4 ComposePlus(float4 colorA, float4 colorB) {
        colorA.rgb *= colorA.a;
        colorB.rgb *= colorB.a;
        return saturate(colorA + colorB);
}

// -------------------------------------------------------------------------------
// Blend Functions
// From https://www.ryanjuckett.com/photoshop-blend-modes-in-hlsl/
// vectorized and with alpha additions.
//
// Normal = Copy
// Darken = Min
// Multiply = Multiply
// Lighten = Max

inline float4 BlendLerp(float4 base, float4 result, float4 blend) {
  return lerp(base, float4(result.rgb, 1), blend.a);
}

// Color Burn
// Looks at the color information in each channel and darkens the base color to
// reflect the blend color by increasing the contrast between the two.
float4 BlendColorBurn(float4 base, float4 blend) {
  // Saturate takes care of base >= 1
  float4 result = blend <= 0 ? 0 : 1 - saturate((1 - base) / blend);
	return BlendLerp(base, result, blend);
}

// Linear Burn
// Looks at the color information in each channel and darkens the base color to
// reflect the blend color by decreasing the brightness.
float4 BlendLinearBurn(float4 base, float4 blend) {
  float4 result = max(0, base + blend - 1);
	return BlendLerp(base, result, blend);
}

// Screen
// Looks at each channel’s color information and multiplies the inverse of the
// blend and base colors.
float4 BlendScreen(float4 base, float4 blend) {
	float4 result = base + blend - base * blend;
	return BlendLerp(base, result, blend);
}

// Color Dodge
// Looks at the color information in each channel and brightens the base color
// to reflect the blend color by decreasing contrast between the two.
float4 BlendColorDodge(float4 base, float4 blend) {
  // Saturate takes care of base <= 0
  float4 result = blend >= 1 ? 1 : saturate(base / (1 - blend));
	return BlendLerp(base, result, blend);
}

// Linear Dodge
// Looks at the color information in each channel and brightens the base color
// to reflect the blend color by decreasing contrast between the two.
float4 BlendLinearDodge(float4 base, float4 blend) {
	float4 result = saturate(base + blend);
	return BlendLerp(base, result, blend);
}

// Overlay
// Multiplies or screens the colors, depending on the base color.
float4 BlendOverlay(float4 base, float4 blend) {
	float4 result = base <= 0.5f
    ? 2 * base * blend
    : 1 - 2 * (1 - base) * (1 - blend);
	return BlendLerp(base, result, blend);
}

// Soft Light
// Darkens or lightens the colors, depending on the blend color.
float4 BlendSoftLight(float4 base, float4 blend) {
  float4 resultLo = base - (1 - 2 * blend) * base * (1 - base);
  float4 d = (base <= 0.25f) ? ((16 * base - 12) * base + 4) * base : sqrt(base);
  float4 resultHi = base + (2 * blend - 1) * (d -base);

  float4 result = blend <= 0.5 ? resultLo : resultHi;
	return BlendLerp(base, result, blend);
}

// Hard Light
// Multiplies or screens the colors, depending on the blend color.
float4 BlendHardLight(float4 base, float4 blend) {
	float4 result = blend <= 0.5f
    ? 2 * base * blend
    : 1 - 2 * (1 - base) * (1 - blend);
	return BlendLerp(base, result, blend);
}

// Vivid Light
// Burns or dodges the colors by increasing or decreasing the contrast,
// depending on the blend color.
float4 BlendVividLight(float4 base, float4 blend) {
  return blend <= 0.5
    ? BlendColorBurn (base, float4(2 * (blend.rgb       ), blend.a))
    : BlendColorDodge(base, float4(2 * (blend.rgb - 0.5f), blend.a));
}

// Linear Light
// Burns or dodges the colors by decreasing or increasing the brightness,
// depending on the blend color.
float4 BlendLinearLight(float4 base, float4 blend) {
	return blend <= 0.5f
    ? BlendLinearBurn (base, float4(2 * (blend.rgb       ), blend.a))
    : BlendLinearDodge(base, float4(2 * (blend.rgb - 0.5f), blend.a));
}

// Pin Light
// Replaces the colorsI, depending on the blend color.
float4 BlendPinLight(float4 base, float4 blend) {
  float4 result = blend <= 0.5f
    ? min(base, 2 * blend)
    : max(base, 2 * (blend - 0.5f));
	return BlendLerp(base, result, blend);
}
