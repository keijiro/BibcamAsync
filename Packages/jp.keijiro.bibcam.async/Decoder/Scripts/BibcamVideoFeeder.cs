using System.Collections.Generic;
using UnityEngine;
#if BIBCAM_HAS_UNITY_VIDEO
using UnityEngine.Video;
#endif

namespace Bibcam.Decoder {

sealed class BibcamVideoFeeder : MonoBehaviour
{
#if BIBCAM_HAS_UNITY_VIDEO

    #region Scene object reference

    [SerializeField] BibcamMetadataDecoder _decoder = null;
    [SerializeField] BibcamTextureDemuxer _demuxer = null;

    #endregion

    #region Frame readback queue

    Queue<(RenderTexture rt, int index)> _queue
      = new Queue<(RenderTexture rt, int index)>();

    int _count;


    #endregion

    #region MonoBehaviour implementation

    void OnDestroy()
    {
        while (_queue.Count > 0)
            RenderTexture.ReleaseTemporary(_queue.Dequeue().rt);
    }

    void Update()
    {
        var player = GetComponent<VideoPlayer>();
        if (player.texture == null) return;

        // Temporary RT copy
        var video = player.texture;
        var tempRT = RenderTexture.GetTemporary(video.width, video.height);
        Graphics.CopyTexture(video, tempRT);

        // Decoding queue
        _decoder.RequestDecode(tempRT);
        _queue.Enqueue((tempRT, _count++));

        // Unused old frame disposal
        while (_queue.Peek().index < _decoder.DecodeCount)
            RenderTexture.ReleaseTemporary(_queue.Dequeue().rt);

        // Last-decoded frame demuxing
        var decoded = _queue.Dequeue().rt;
        _demuxer.Demux(decoded, _decoder.Metadata);
        RenderTexture.ReleaseTemporary(decoded);
    }

    #endregion

#else

    void OnValidate()
      => Debug.LogError("UnityEngine.Video is missing.");

#endif
}

} // namespace Bibcam.Decoder
