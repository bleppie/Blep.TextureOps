#define THREADSX 32
#define THREADSY 32

// Size of the texture
uint2 TextureSize;

// Size of each texel (1 / size)
float2 TexelSize;

// Default texture samplers (defined by Unity)
SamplerState PointClampSampler;
SamplerState LinearClampSampler;

// Standard parameters
Texture2D<float4> SrcA;
Texture2D<float4> SrcB;
float4 ScalarA;
float4 ScalarB;
float4 ScalarC;
float4 ScalarD;
RWTexture2D<float4> Dst;

// Returns true if the pixel corresponding to this thread id is in bounds
inline bool InRange(uint2 xy) {
	return all(xy < TextureSize);
}

// Returns xy clamped to texture bounds
inline int2 ClampToRange(int2 xy) {
	return clamp(xy, 0, TextureSize);
}

// Returns the uv coordinates corresponding to the given integer xy coordinates.
// xy coordinates are in the range (0, size-1), but they correspond to the
// center of pixels, so the actual mapping is from (-0.5, size-0.5) to (0, 1)
inline float2 GetUV(uint2 xy) {
  return (xy + 0.5f) * TexelSize;
}

inline float4 SamplePoint(Texture2D src, float2 uv) {
  return src.SampleLevel(PointClampSampler, uv, 0);
}

inline float4 SampleLinear(Texture2D src, float2 uv) {
  return src.SampleLevel(LinearClampSampler, uv, 0);
}

// Simple boilerplate kernel definition 
#define DEFKERNEL(NAME, EXP)                    \
  [numthreads(THREADSX, THREADSY, 1)]           \
  void NAME(uint2 xy : SV_DispatchThreadID) {   \
    if (InRange(xy)) {                          \
      Dst[xy] = EXP;                            \
    }                                           \
  }
