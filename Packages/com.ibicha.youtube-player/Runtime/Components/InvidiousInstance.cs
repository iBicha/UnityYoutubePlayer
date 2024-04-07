using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using YoutubePlayer.Api;

namespace YoutubePlayer.Components
{
    public class InvidiousInstance : MonoBehaviour
    {
        public enum InvidiousInstanceType
        {
            Public,
            Custom
        }

        public InvidiousInstanceType InstanceType;

        public string CustomInstanceUrl;

        private string m_PublicInstanceUrl;

        public async Task<string> GetInstanceUrl(CancellationToken cancellationToken = default)
        {
            switch (InstanceType)
            {
                case InvidiousInstanceType.Public:
                    if (string.IsNullOrEmpty(m_PublicInstanceUrl))
                    {
                        Debug.Log("Fetching Invidious public instances...");
                        var instances = await InvidiousApi.GetPublicInstances(cancellationToken);
                        m_PublicInstanceUrl = instances[0].Uri;
                        Debug.Log($"Using Invidious public instance: {m_PublicInstanceUrl}");
                    }

                    return m_PublicInstanceUrl;
                case InvidiousInstanceType.Custom:
                    return CustomInstanceUrl;

                default:
                    throw new System.ArgumentOutOfRangeException("InstanceType");
            }
        }

        public async Task<string> GetVideoUrl(string videoId, bool proxyVideo, CancellationToken cancellationToken = default)
        {
            var instanceUrl = await GetInstanceUrl(cancellationToken);

            if (proxyVideo)
            {
                return $"{instanceUrl}/latest_version?id={videoId}&local=true";
            }
            else
            {
                return $"{instanceUrl}/latest_version?id={videoId}";
            }
        }
    }
}
