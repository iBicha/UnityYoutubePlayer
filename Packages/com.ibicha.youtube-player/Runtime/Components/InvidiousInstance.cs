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

        public string InstanceUrl => InstanceType == InvidiousInstanceType.Public ? m_PublicInstanceUrl : CustomInstanceUrl;

        private string m_PublicInstanceUrl;

        public async Task<string> GetInstanceUrl(CancellationToken cancellationToken = default)
        {
            switch (InstanceType)
            {
                case InvidiousInstanceType.Public:
                    if (string.IsNullOrEmpty(m_PublicInstanceUrl))
                    {
                        Debug.LogWarning("Instance type is set to \"Public\". Fetching public instances every time is slow, and are only used for the sample to work. Please set a custom instance in the InvidiousInstance component.");
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
    }
}
