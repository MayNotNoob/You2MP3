using Google.Apis.YouTube.v3.Data;
using NAudio.Wave;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using U2MP3.AudioEngine;
using U2MP3.Models;
using U2MP3.UServices;
using You2MP3.ViewModels;

namespace U2MP3.ViewModels
{
    public class MainWindowModel : ViewModelBase
    {
        private readonly UService _searchService;
        private ICommand _searchCommand;
        private ObservableCollection<Music> _searchResults;
        private ICommand _playCommand;
        private ICommand _stopCommand;
        private readonly AudioPlayer _audioPlayer;
        public MainWindowModel(UService searchService, AudioPlayer player)
        {
            _searchService = searchService;
            _audioPlayer = player;
            SearchResults = new ObservableCollection<Music>();
            _audioPlayer.PlaybackStopped += OnWaveOutEventOnPlaybackStopped;
            DoSearchCommand("周杰伦");
        }

        public ICommand SearchCommand
        {
            get { return _searchCommand ??= new RelayCommand(e => DoSearchCommand(Content), p => true); }
        }

        public ICommand StopCommand
        {
            get
            {
                return _stopCommand ??= new RelayCommand(e => DoStopCommand(), p => true);
            }
        }
        public ICommand PlayCommand
        {
            get { return _playCommand ??= new RelayCommand(e => DoPlayCommand(), p => true); }
        }

        private void DoPlayCommand()
        {
            var tmpMusic = SelectedMusic;
            if (Equals(_audioPlayer.Music, tmpMusic))
            {
                switch (_audioPlayer.PlaybackState)
                {
                    case PlaybackState.Playing:
                        _audioPlayer.Pause();
                        break;
                    case PlaybackState.Paused:
                    case PlaybackState.Stopped:
                        _audioPlayer.Play();
                        break;
                }
            }
            else
            {
                if (_audioPlayer.PlaybackState != PlaybackState.Stopped)
                    _audioPlayer.StopWithoutEvent();

                _audioPlayer.Music = tmpMusic;
                _audioPlayer.Play();
            }
        }

        private void DoStopCommand()
        {
            if(_audioPlayer!=null && _audioPlayer.PlaybackState == PlaybackState.Playing && Equals(SelectedMusic, _audioPlayer.Music))
                _audioPlayer.Stop();
        }

        private void OnWaveOutEventOnPlaybackStopped(object sender, StoppedEventArgs args)
        {
            var player = (AudioPlayer)sender;
            if (player.Music != null)
                player.Music.IconKind = PackIconKind.PlayCircleOutline;
        }
        public string Content { get; set; }

        public ObservableCollection<Music> SearchResults
        {
            get => _searchResults;
            set { _searchResults = value; OnPropertyChanged(nameof(SearchResults)); }
        }

        public Music SelectedMusic { get; set; }

        #region private methods
        private async void DoSearchCommand(string content)
        {
            if (SearchResults.Any()) SearchResults.Clear();
            try
            {
                SearchListResponse response = await _searchService.SearchMusicByKeyWords(content);
                if (Application.Current.Dispatcher != null)
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        foreach (SearchResult searchResult in response.Items)
                        {
                            string id = null;
                            switch (searchResult.Id.Kind)
                            {
                                case "youtube#video":
                                    id = searchResult.Id.VideoId;
                                    break;

                                case "youtube#channel":
                                    id = searchResult.Id.ChannelId;
                                    break;

                                    //case "youtube#playlist":
                                    //    id = searchResult.Id.PlaylistId;
                                    //    break;
                            }
                            if (!string.IsNullOrEmpty(id))
                                SearchResults.Add(new Music(id, searchResult.Snippet.Title, searchResult.Snippet.Thumbnails.Default__.Url, searchResult.Id.Kind));
                        }
                    });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to search music {ex.Message}", "ERROR");
            }
        }

        #endregion

    }
}