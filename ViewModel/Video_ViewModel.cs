using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using Microsoft.Win32;
using WMPLib;

namespace MyWindowsMediaPlayer.ViewModel
{
    partial class Video_ViewModel : INotifyPropertyChanged
    {
        readonly string                             __LibraryFile;

        public event PropertyChangedEventHandler    PropertyChanged;
        DispatcherTimer                             _ProgressTimer;
        private bool                                _IsPlaying;
        private bool                                _IsFullScreen;
        private bool                                _IsLoaded;
        private Model.Library                       _Library;
        private Model.Video                         _Video;
        private Model.Playlist                      _Playlist;
        private double                              _Maximum;
        private double                              _SaveVolume;
        private double                              _SaveWidth;
        private double                              _SaveHeight;
        private int                                 _Choice;
        private bool                                _Loading;
        private int                                 _NbFilesLoaded;
        private String                              _LibrarySearch;
        private bool                                _GoSearch;
        private WindowState                         _WindowStateSave;

        public bool             IsFullScreen { get { return _IsFullScreen; } set { _IsFullScreen = value; NotifyPropertyChanged("IsFullScreen"); } }
        public bool             IsPlaying { get { return _IsPlaying; } set { _IsPlaying = value; NotifyPropertyChanged("IsPlaying"); NotifyPropertyChanged("IsPlayingToStr"); } }
        public string           IsPlayingToStr { get { return (_IsPlaying ? "PAUSE" : "PLAY"); } }
        public Model.Library    Library { get { return _Library; } }
        public Model.Video      Video { get { return _Video; } }
        public Model.Playlist   Playlist { get { return _Playlist; } }
        public bool             IsLoaded { get { return _IsLoaded; } set { _IsLoaded = value; NotifyPropertyChanged("IsLoaded"); } }
        public double           Maximum { get { return _Maximum; } set { _Maximum = value; NotifyPropertyChanged("Maximum"); } }
        public double           Progress { get { return Video.Media.Position.TotalSeconds; } set { Video.Media.Position = TimeSpan.FromSeconds(value); } }
        public double           SaveVolume { get { return _SaveVolume; } set { _SaveVolume = value; } }
        public string           VideoName { get { return (Video.Media.Source == null ? "My Windows Media Player" : Path.GetFileNameWithoutExtension(Video.Media.Source.ToString())); } }
        public WindowState      WinState { get { return (IsFullScreen ? WindowState.Maximized : WindowState.Normal); } set { _WindowStateSave = value; } }
        public WindowStyle      WinStyle { get { return (IsFullScreen ? WindowStyle.None : WindowStyle.SingleBorderWindow); } }
        public ResizeMode       WinResize { get { return (IsFullScreen ? ResizeMode.NoResize : ResizeMode.CanResize); } }
        public bool             TopMost { get { return (IsFullScreen ? true : false); } }
        public double           SaveWidth { get { return _SaveWidth; } set { _SaveWidth = value; } }
        public double           SaveHeight { get { return _SaveHeight; } set { _SaveHeight = value; } }
        public int              Choice { get { return _Choice; } set { _Choice = value; NotifyPropertyChanged("Choice"); NotifyPropertyChanged("Playlistvisible"); NotifyPropertyChanged("LibraryVisible"); } }
        public Visibility       PlaylistVisible { get { return (Choice == 0 ? Visibility.Visible : Visibility.Hidden); } }
        public int              LibraryVisible { get { return (Choice == 1 ? 8 : 6); } }
        public int              PlaylistPos { get { return Playlist.Pos; } }
        public int              LibraryPos { get; set; }
        public int              FSSpan { get { return (IsFullScreen ? 3 : 1); } }
        public int              FSCol { get { return (IsFullScreen ? 0 : 2); } }
        public int              FSRow { get { return (IsFullScreen ? 0 : 1); } }
        public int              FSMargin { get { return (IsFullScreen ? 0 : 2); } }
        public string           BtnPlayPauseImg { get { return (IsPlaying ? "pack://siteoforigin:,,,/MyResources/BtnPause.png" : "pack://siteoforigin:,,,/MyResources/BtnPlay.png"); } }
        public string           BtnMuteImg { get { return (Video.Media.Volume == 0 ? "pack://siteoforigin:,,,/MyResources/BtnMuted.png" : "pack://siteoforigin:,,,/MyResources/BtnMute.png"); } }
        public bool             Loading { get { return _Loading; } set { _Loading = value; _NbFilesLoaded = 0; NotifyPropertyChanged("IsLoading"); NotifyPropertyChanged("LibraryListView"); } }
        public Visibility       IsLoading { get { return (Loading ? Visibility.Visible : Visibility.Hidden); } }
        public string           NbFilesLoaded { get { return _NbFilesLoaded.ToString(); } }
        public int              NbFilesLoadedInt { get { return _NbFilesLoaded; } set { _NbFilesLoaded = value; NotifyPropertyChanged("NbFilesLoaded"); } }
        public bool             VideoChecked { get { return Library.VideoChecked; } set { Library.VideoChecked = value; Library.Check(value, Model.Film.MediaType.Video); NotifyPropertyChanged("LibraryListView"); } }
        public bool             AudioChecked { get { return Library.AudioChecked; } set { Library.AudioChecked = value; Library.Check(value, Model.Film.MediaType.Audio); NotifyPropertyChanged("LibraryListView"); } }
        public bool             ImageChecked { get { return Library.ImageChecked; } set { Library.ImageChecked = value; Library.Check(value, Model.Film.MediaType.Image); NotifyPropertyChanged("LibraryListView"); } }
        public ObservableCollection<Model.Film> LibraryListView { get { return Library.FilmsView; } }
        public String           LibrarySearch { get { return _LibrarySearch; } set { _LibrarySearch = value; if (_GoSearch) { Library.Search(value); NotifyPropertyChanged("LibraryListView"); } } }
        public int              ControlZindex { get; set; }

        /// <summary>
        /// Initialization
        /// </summary>
        public Video_ViewModel                      ()
        {
            __LibraryFile               = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Epitech\\__MyLibrary";
            _NbFilesLoaded              = 0;
            Loading                     = true;
            _ProgressTimer              = new DispatcherTimer();
            _Library                    = new Model.Library(__LibraryFile, this);
            _Playlist                   = new Model.Playlist();
            _Video                      = new Model.Video();
            _IsPlaying                  = false;
            _IsLoaded                   = false;
            _Maximum                    = 10;
            _SaveVolume                 = 0;
            _SaveWidth                  = Video.Media.Width;
            _SaveHeight                 = Video.Media.Height;
            _Choice                     = 0;
            IsFullScreen                = false;
            _LibrarySearch              = "Custom Search...";
            _GoSearch                   = true;
            _WindowStateSave            = WindowState.Normal;
            ControlZindex               = 7;

            _ProgressTimer.Interval     = TimeSpan.FromSeconds(1);
            _ProgressTimer.Tick         += ProgressTimer_Tick;
            Video.Media.MediaOpened     += new RoutedEventHandler(EVNT_MediaOpened);
            Video.Media.MediaEnded      += new RoutedEventHandler(EVNT_MediaEnded);
            Video.Media.ScrubbingEnabled = true;
            Video.Media.IsEnabled        = true;

            _ProgressTimer.Start();
            InitCommands();
        }

        /// <summary>
        ///  Property Changed Trick
        /// </summary>
        public void                                 NotifyPropertyChanged(string Prop)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(Prop));
        }

        /// <summary>
        /// Action Of The Timer Each Seconds
        /// </summary>
        private void                                ProgressTimer_Tick(object sender, EventArgs e)
        {
            if (IsLoaded)
                NotifyPropertyChanged("Progress");
        }
    }
}
