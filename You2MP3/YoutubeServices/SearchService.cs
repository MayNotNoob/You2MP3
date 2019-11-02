using System.DirectoryServices;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace You2MP3.YoutubeServices
{

    public sealed class SearchService
    {
        private readonly YouTubeService _youTubeService;
        public SearchService(YouTubeService youTubeService)
        {
            _youTubeService = youTubeService;
        }
        public async Task<SearchListResponse> Search(string content, string part = "snippet", int maxResult = 50)
        {
            SearchResource.ListRequest request = _youTubeService.Search.List(part);
            request.Q = content;
            request.MaxResults = maxResult;
            return await request.ExecuteAsync();
        }

    }
}