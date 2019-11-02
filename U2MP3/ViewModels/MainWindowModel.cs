using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Google.Apis.YouTube.v3.Data;
using NAudio.Wave;
using U2MP3.AudioEngine;
using U2MP3.Models;
using U2MP3.UServices;
using You2MP3.ViewModels;
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;

namespace U2MP3.ViewModels
{
    public class MainWindowModel : ViewModelBase
    {
        private readonly UService _searchService;
        private ICommand _searchCommand;
        private ObservableCollection<Music> _searchResults;
        private ICommand _playCommand;
        private readonly AudioPlayer _audioPlayer;
        public MainWindowModel(UService searchService, AudioPlayer player)
        {
            _searchService = searchService;
            _audioPlayer = player;
            SearchResults = new ObservableCollection<Music>();
            _audioPlayer.PlaybackStopped += OnWaveOutEventOnPlaybackStopped;
        }

        public ICommand SearchCommand
        {
            get { return _searchCommand ??= new RelayCommand(e => DoSearchCommand(Content), p => true); }
        }

        public ICommand PlayCommand
        {
            get { return _playCommand ??= new RelayCommand(e => DoPlayCommand(), p => true); }
        }

        private async void DoPlayCommand()
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
                        _audioPlayer.Play();
                        break;
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

        private void OnWaveOutEventOnPlaybackStopped(object sender, StoppedEventArgs args)
        {
            var player = (AudioPlayer)sender;
            if (player.Music != null)
                player.Music.ImageState = ImageState.STOP;
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

        #endregion

    }
}