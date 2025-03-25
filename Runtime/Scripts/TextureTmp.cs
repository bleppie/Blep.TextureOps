using UnityEngine;
using UnityEngine.Assertions;

namespace Blep.TextureOps {

// A Reference to a temporary RenderTexture that releases the texture on dispose.
// Can be used in using statements. Example usage:
//    using var tmp = TextureTmp(src);
//    TextureIP.Erode(src, tmp);
//    TextureIP.Dilate(tmp, src);

public class TextureTmp : System.IDisposable {

    public RenderTexture texture { get; private set; }

    public TextureTmp(int width, int height,
                      RenderTextureFormat format = RenderTextureFormat.Default) =>
        texture = TextureCompute.GetTemporary(width, height, format);

    // Create texure compatible with give src, forcing a float format if needed.
    public TextureTmp(Texture src, bool forceFloatFormat=false) =>
        texture = (forceFloatFormat
                   ? TextureCompute.GetTemporary(src) : TextureCompute.GetFloatTemporary(src));

    public TextureTmp(RenderTextureDescriptor desc) =>
        texture = RenderTexture.GetTemporary(desc);

    public TextureTmp(int width, int height, int depthBuffer = 0,
                      RenderTextureFormat format = RenderTextureFormat.Default,
                      RenderTextureReadWrite readWrite = RenderTextureReadWrite.Default,
                      int antiAliasing = 1,
                      RenderTextureMemoryless memorylessMode = RenderTextureMemoryless.None,
                      VRTextureUsage vrUsage = VRTextureUsage.None,
                      bool useDynamicScale = false) =>
        texture = RenderTexture.GetTemporary(width, height, depthBuffer,
                                             format, readWrite, antiAliasing, memorylessMode,
                                             vrUsage, useDynamicScale);

    public void Dispose() {
        Assert.IsNotNull(texture, "RenderTexture that has already been released.");
        TextureCompute.ReleaseTemporary(texture);
        texture = null;
    }

    // Implicitly converts to RenderTexture to it can be used in method calls.
    public static implicit operator RenderTexture(TextureTmp tt) {
        Assert.IsNotNull(tt.texture,
                         "Trying to access a temporary RenderTexture that has been released.");
        return tt.texture;
    }
}

}
