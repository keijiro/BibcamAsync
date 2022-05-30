using UnityEngine;
using UnityEngine.Rendering;
using Bibcam.Common;

namespace Bibcam.Decoder {

[ExecuteInEditMode]
public sealed class BibcamMetadataDecoder : MonoBehaviour
{
    #region Hidden asset references

    [SerializeField, HideInInspector] ComputeShader _shader = null;

    #endregion

    #region Public members

    public Metadata Metadata { get; private set; }
    public int DecodeCount { get; private set; }

    public void RequestDecode(Texture source)
    {
        // Lazy allocation
        if (_readbackBuffer == null)
            _readbackBuffer = GfxUtil.StructuredBuffer(12, sizeof(float));

        // Decoder kernel dispatching
        _shader.SetTexture(0, "Source", source);
        _shader.SetBuffer(0, "Output", _readbackBuffer);
        _shader.Dispatch(0, 1, 1, 1);

        // Async readback request
        AsyncGPUReadback.Request(_readbackBuffer, OnReadback);
    }

    #endregion

    #region Private members

    GraphicsBuffer _readbackBuffer;

    void OnReadback(AsyncGPUReadbackRequest req)
    {
        if (!req.hasError) Metadata = req.GetData<Metadata>()[0];
        DecodeCount++;
    }

    #endregion

    #region MonoBehaviour implementation

    void OnDisable() => OnDestroy();

    void OnDestroy()
    {
        _readbackBuffer?.Dispose();
        _readbackBuffer = null;
    }

    #endregion
}

} // namespace Bibcam.Decoder
