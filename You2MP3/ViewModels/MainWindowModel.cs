using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Google.Apis.YouTube.v3.Data;
using You2MP3.YoutubeServices;

namespace You2MP3.ViewModels
{
    public class MainWindowModel : ViewModelBase
    {
        private readonly SearchService _searchService;

        private ICommand _searchCommand;
        private ObservableCollection<SearchResult> _searchResults;

        public MainWindowModel(SearchService searchService)
        {
            _searchService = searchService;
            SearchResults=new ObservableCollection<SearchResult>();
        }

        public ICommand SearchCommand
        {
            get { return _searchCommand ??= new RelayCommand(e => DoSearchCommand(Content), p => true); }
        }

        public string Content { get; set; }

        public ObservableCollection<SearchResult> SearchResults
        {
            get => _searchResults;
            set { _searchResults = value; OnPropertyChanged(nameof(SearchResults)); }
        }

        private async void DoSearchCommand(string content)
        {
            SearchListResponse response = await _searchService.Search(content);
            if (Application.Current.Dispatcher != null)
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    foreach (SearchResult searchResult in response.Items)
                    {
                        if (searchResult.Id.Kind == "youtube#video" || searchResult.Id.Kind == "youtube#channel" ||
                            searchResult.Id.Kind == "youtube#playlist")
                            SearchResults.Add(searchResult);
                    }
                });
        }
    }
}