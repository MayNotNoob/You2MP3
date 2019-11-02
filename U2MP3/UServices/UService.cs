using System.Threading.Tasks;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace U2MP3.UServices
{

    public sealed class UService
    {
        private readonly YouTubeService _youTubeService;
        public UService(YouTubeService youTubeService)
        {
            _youTubeService = youTubeService;
        }

        public async Task<SearchListResponse> SearchMusicByKeyWords(string content)
        {
            return await Search(content);
        }

        public async Task<PlaylistItemListResponse> SearchPlayListTask(string id)
        {
            PlaylistItemsResource.ListRequest request = _youTubeService.PlaylistItems.List("");
            request.PlaylistId = id;
            return await request.ExecuteAsync();
        }

        public async Task<SearchListResponse> GetPreOrNextPage(string content, string pageToken)
        {
            return await Search(content, pageToken);
        }


        public async Task<SearchListResponse> Search(string content, string pageToken = null, int maxResult = 50)
        {
            SearchResource.ListRequest request = _youTubeService.Search.List("snippet");
            request.Q = content;
            request.MaxResults = maxResult;
            if (!string.IsNullOrEmpty(pageToken) && !string.IsNullOrWhiteSpace(pageToken))
                request.PageToken = pageToken;
            return await request.ExecuteAsync();
        }


    }
}