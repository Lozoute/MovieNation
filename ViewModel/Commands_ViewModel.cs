using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;
using Microsoft.Win32;
using WMPLib;
using Microsoft.WindowsAPICodePack;
using Microsoft.WindowsAPICodePack.Dialogs;
    using Microsoft.WindowsAPICodePack.Shell;
using System.Xml.Linq;
using System.Linq;
using System.Threading.Tasks;

namespace MyWindowsMediaPlayer.ViewModel
{
    /// <summary>
    /// Custom Commands
    /// </summary>
    public class DelegateCommand<T> : System.Windows.Input.ICommand where T : class
    {
        private readonly Predicate<T> _CanExecute;
        private readonly Action<T> _Execute;
        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action<T> Execute)
        {
            _Execute = Execute;
            _CanExecute = null;
        }

        public DelegateCommand(Action<T> Execute, Predicate<T> CanExecute)
        {
            _Execute = Execute;
            _CanExecute = CanExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (_CanExecute == null)
                return (true);
            return (_CanExecute((T)parameter));
        }

        public void Execute(object parameter)
        {
            _Execute((T)parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }
    }

       
    /// <summary>
    /// Everything Commands Related For The ViewModel
    /// </summary>
    partial class Video_ViewModel
    {
        private DelegateCommand<String>         _CMD_Open;
        private DelegateCommand<Button>         _CMD_PlayPause;
        private DelegateCommand<Button>         _CMD_Stop;
        private DelegateCommand<Button>         _CMD_Mute;
        private DelegateCommand<Button>         _CMD_FullScreen;
        private DelegateCommand<Button>         _CMD_Next;
        private DelegateCommand<Button>         _CMD_Prev;
        private DelegateCommand<ListView>       _CMD_PlayIndex;
        private DelegateCommand<ListView>       _CMD_DeleteIndex;
        private DelegateCommand<ListView>       _CMD_AddToPlaylist;
        private DelegateCommand<Button>         _CMD_SavePlayList;
        private DelegateCommand<Button>         _CMD_LoadPlayList;
        private DelegateCommand<ListView>       _CMD_AddToPlaylistAndPlay;
        private DelegateCommand<ListView>       _CMD_ClearPlaylistAndPlay;
        private DelegateCommand<String>         _CMD_OpenFolder;
        private DelegateCommand<String>         _CMD_SortLibrary;

        public DelegateCommand<String>          CMD_Open { get { return _CMD_Open; } }
        public DelegateCommand<Button>          CMD_PlayPause { get { return _CMD_PlayPause; } }
        public DelegateCommand<Button>          CMD_Stop { get { return _CMD_Stop; } }
        public DelegateCommand<Button>          CMD_Mute { get { return _CMD_Mute; } }
        public DelegateCommand<Button>          CMD_FullScreen { get { return _CMD_FullScreen; } }
        public DelegateCommand<Button>          CMD_Next { get { return _CMD_Next; } }
        public DelegateCommand<Button>          CMD_Prev { get { return _CMD_Prev; } }
        public DelegateCommand<ListView>        CMD_PlayIndex { get { return _CMD_PlayIndex; } }
        public DelegateCommand<ListView>        CMD_DeleteIndex { get { return _CMD_DeleteIndex; } }
        public DelegateCommand<ListView>        CMD_AddToPlaylist { get { return _CMD_AddToPlaylist; } }
        public DelegateCommand<Button>          CMD_SavePlayList { get { return _CMD_SavePlayList; } }
        public DelegateCommand<Button>          CMD_LoadPlayList { get { return _CMD_LoadPlayList; } }
        public DelegateCommand<ListView>        CMD_AddToPlaylistAndPlay { get { return _CMD_AddToPlaylistAndPlay; } }
        public DelegateCommand<ListView>        CMD_ClearPlaylistAndPlay { get { return _CMD_ClearPlaylistAndPlay; } }
        public DelegateCommand<String>          CMD_OpenFolder { get { return _CMD_OpenFolder; } }
        public DelegateCommand<String>          CMD_SortLibrary { get { return _CMD_SortLibrary; } }


        /// <summary>
        /// Just Load A Media 
        /// </summary>
        private void                            LoadMedia(string Path)
        {
            if (File.Exists(Path))
            {
                CMD_Stop_Executed(null);
                Video.Media.Source              = new Uri(Path);
                Video.Media.Play();

                IsPlaying = true;
                IsLoaded = true;
                NotifyPropertyChanged("VideoName");
                NotifyPropertyChanged("PlaylistPos");
                NotifyPropertyChanged("BtnPlayPauseImg");
                CMD_PlayPause.RaiseCanExecuteChanged();
                CMD_Stop.RaiseCanExecuteChanged();
                CMD_FullScreen.RaiseCanExecuteChanged();
                CMD_Prev.RaiseCanExecuteChanged();
                CMD_Next.RaiseCanExecuteChanged();
                CMD_SavePlayList.RaiseCanExecuteChanged();
            }
            else
                System.Windows.Forms.MessageBox.Show("Unable To Load " + Path, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Get Type Of File
        /// </summary>
        public static Model.Film.MediaType      MyGetType(string E)
        {
            E = E.ToLower();
            if (E == null || E.Length == 0 || E == "")
                return (Model.Film.MediaType.Unknown);
            if ("*.avi, *.mpeg, *.mpg, *.mp4, *.wmv".IndexOf(E) != -1)
                return (Model.Film.MediaType.Video);
            else if ("*.mp3, *.wav, *.mid, *.midi".IndexOf(E) != -1)
                return (Model.Film.MediaType.Audio);
            else if ("*.jpg, *.jpeg, *.png, *.gif, *.bmp".IndexOf(E) != -1)
                return (Model.Film.MediaType.Image);
            return (Model.Film.MediaType.Unknown);
        }

        /// <summary>
        ///  Opening A New Media File And Playing It
        /// </summary>
        public void                             CMD_Open_Executed(String Arg)
        {
            OpenFileDialog Dialog               = new OpenFileDialog();
            Dialog.Multiselect                  = true;
            Dialog.Title                        = "Select One Or Several Media Files";
            Dialog.Filter                       += "Video Files (*.avi, *.mpeg, *.mpg, *.mp4, *.wmv) | *.avi;*.mpeg;*.mpg;*.mp4;*.wmv;";
            Dialog.Filter                       += " | Audio Files (*.mp3, *.wav, *.mid, *.midi) | *.mp3;*.wav;*.mid;*.midi;";
            Dialog.Filter                       += " | Image Files (*.jpg, *.jpeg, *.png, *.gif, *.bmp) | *.jpg;*.jpeg;*.png;*.gif;*.bmp;";
            Dialog.CheckFileExists              = true;
            Dialog.CheckPathExists              = true;
            var Player                          = new WindowsMediaPlayer();

            if (Dialog.ShowDialog() == true)
            {
                Loading                         = true;
                Task T                          = new Task(() =>
                {
                    if (Arg == "OPEN")
                        App.Current.Dispatcher.BeginInvoke((Action)delegate() { Playlist.Clean(); });

                    foreach (String _File in Dialog.FileNames)
                    {
                        var Clip                = Player.newMedia(_File);
                        Model.Film NewFilm      = new Model.Film(Path.GetFileNameWithoutExtension(_File), TimeSpan.FromSeconds((Clip.duration == 0 ? 4 : Clip.duration)), _File, false, MyGetType(Path.GetExtension(_File)));

                        if (Arg == "LIBRARY")
                            App.Current.Dispatcher.BeginInvoke((Action)delegate() { Library.AddFilm(NewFilm); });
                        else
                            App.Current.Dispatcher.BeginInvoke((Action)delegate() { Playlist.AddFilm(NewFilm); });
                        _NbFilesLoaded++;
                        NotifyPropertyChanged("NbFilesLoaded");
                    }

                    if (Arg == "OPEN")
                        App.Current.Dispatcher.BeginInvoke((Action)delegate() { LoadMedia(Playlist.GetFirst().Path); });
                    App.Current.Dispatcher.BeginInvoke((Action)delegate() { CMD_SavePlayList.RaiseCanExecuteChanged(); });
                });

                T.ContinueWith(antecedent => Loading = false, TaskScheduler.FromCurrentSynchronizationContext());
                T.Start();
            }
        }

        /// <summary>
        /// Opening A Whole Folder
        /// </summary>
        public void                             CMD_OpenFolder_Executed(String Arg)
        {
            CommonOpenFileDialog Dialog         = new CommonOpenFileDialog();
            Dialog.Title                        = "Choose A Folder";
            Dialog.IsFolderPicker               = true;
            Dialog.InitialDirectory             = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Dialog.AddToMostRecentlyUsedList    = false;
            Dialog.AllowNonFileSystemItems      = false;
            Dialog.DefaultDirectory             = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Dialog.EnsureFileExists             = true;
            Dialog.EnsurePathExists             = true;
            Dialog.EnsureReadOnly               = false;
            Dialog.EnsureValidNames             = true;
            Dialog.Multiselect                  = false;
            Dialog.ShowPlacesList               = true;

            if (Dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Loading                         = true;
                Task T                          = new Task(() =>
                   {
                       if (Arg == "OPEN")
                           App.Current.Dispatcher.BeginInvoke((Action)delegate() { Playlist.Clean(); });
                        ScanFolder(Dialog.FileName, Arg);
                        if (Arg == "OPEN" && Playlist.Films.Count > 0)
                            App.Current.Dispatcher.BeginInvoke((Action)delegate() { LoadMedia(Playlist.GetFirst().Path); });
                        App.Current.Dispatcher.BeginInvoke((Action)delegate() { CMD_SavePlayList.RaiseCanExecuteChanged(); });
                   });

                T.ContinueWith(antecedent => Loading = false, TaskScheduler.FromCurrentSynchronizationContext());
                T.Start();
            }
        }


        /// <summary>
        ///  Get Through The Folders Recursively
        /// </summary>
        public void                             ScanFolder(String _Folder, String Arg)
        {
            try
            {
                DirectoryInfo DirBase           = new DirectoryInfo(_Folder);
                var Dirs                        = DirBase.EnumerateDirectories();

                foreach (DirectoryInfo Dir in Dirs)
                    if (!((Dir.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden))
                        ScanFolder(Dir.FullName, Arg);

                var _Files                      = Directory.GetFiles(_Folder);
                var Player                      = new WindowsMediaPlayer();

                foreach (string _File in _Files)
                {
                    if (MyGetType(Path.GetExtension(_File)) != Model.Film.MediaType.Unknown)
                    {
                        var Clip                = Player.newMedia(_File);
                        Model.Film NewFilm      = new Model.Film(Path.GetFileNameWithoutExtension(_File), TimeSpan.FromSeconds((Clip.duration == 0 ? 4 : Clip.duration)), _File, false, MyGetType(Path.GetExtension(_File)));

                        if (Arg == "LIBRARY")
                            App.Current.Dispatcher.BeginInvoke((Action)delegate() { Library.AddFilm(NewFilm); } );
                        else
                            App.Current.Dispatcher.BeginInvoke((Action)delegate() { Playlist.AddFilm(NewFilm); } );
                        _NbFilesLoaded++;
                        NotifyPropertyChanged("NbFilesLoaded");
                    }
                }
            }
            catch (Exception e) {}
        }


        /// <summary>
        ///  Pause The Video / Resume The Video
        /// </summary>
        public bool                             CMD_PlayPause_CanExecute(Button Arg)
        {
            return (IsLoaded);
        }

        public void                             CMD_PlayPause_Executed(Button Arg)
        {
            if (IsPlaying)
                Video.Media.Pause();
            else
                Video.Media.Play();
            IsPlaying = !IsPlaying;
            NotifyPropertyChanged("BtnPlayPauseImg");
        }


        /// <summary>
        ///  Stop The Video
        /// </summary>
        public bool                             CMD_Stop_CanExecute(Button Arg)
        {
            return (IsLoaded);
        }

        public void                             CMD_Stop_Executed(Button Arg)
        {
            Video.Media.Stop();
            Video.Media.Position = TimeSpan.FromSeconds(0);
            Video.Media.Close();
            IsPlaying = false;
            NotifyPropertyChanged("BtnPlayPauseImg");
        }


        /// <summary>
        /// Muting Volume / Demuting Volume
        /// </summary>
        public void                             CMD_Mute_Executed(Button Arg)
        {
            if (Video.Media.Volume == 0)
                Video.Media.Volume = (SaveVolume > 0 ? SaveVolume : 0.5);
            else
            {
                SaveVolume = Video.Media.Volume;
                Video.Media.Volume = 0;
            }
            NotifyPropertyChanged("BtnMuteImg");
        }


        /// <summary>
        /// Going Into FullScreen Mode And Getting Back Into Normal
        /// </summary>
        public bool                             CMD_FullScreen_CanExecute(Button Arg)
        {
            return (IsLoaded);
        }

        public void                             CMD_FullScreen_Executed(Button Arg)
        {
            if (!IsFullScreen)
            {
                SaveWidth                       = Video.Media.Width;
                SaveHeight                      = Video.Media.Height;
                Video.Media.Width               = System.Windows.SystemParameters.PrimaryScreenWidth;
                Video.Media.Height              = System.Windows.SystemParameters.PrimaryScreenHeight;
            }
            else
            {
                Video.Media.Width               = SaveWidth;
                Video.Media.Height              = SaveHeight;
            }
            IsFullScreen = !IsFullScreen;
            NotifyPropertyChanged("WinStyle");
            NotifyPropertyChanged("WinResize");
            NotifyPropertyChanged("WinState");
            NotifyPropertyChanged("FSMargin");
            NotifyPropertyChanged("FSSpan");
            NotifyPropertyChanged("FSRow");
            NotifyPropertyChanged("FSCol");
            NotifyPropertyChanged("TopMost");
            NotifyPropertyChanged("IsFullScreen");
        }

        /// <summary>
        /// CMD Next
        /// </summary>
        public bool                             CMD_Next_CanExecute(Button Arg)
        {
            return (Playlist.Films.Count > 0);
        }

        public void                             CMD_Next_Executed(Button Arg)
        {
            LoadMedia(Playlist.GetNext().Path);
        }

        /// <summary>
        /// CMD Prev
        /// </summary>
        public bool                             CMD_Prev_CanExecute(Button Arg)
        {
            return (Playlist.Films.Count > 0);
        }

        public void                             CMD_Prev_Executed(Button Arg)
        {
            LoadMedia(Playlist.GetPrev().Path);
        }


        /// <summary>
        ///  Playing An Item From Library Or Playlist
        /// </summary>
        public void                             CMD_PlayIndex_Executed(ListView Arg)
        {
            if (Arg != null)
            {
                if (Arg.Name == "PlayList")
                    LoadMedia(Playlist.GetIndex(Arg.SelectedIndex).Path);
            }
        }

        /// <summary>
        /// Remove From a List
        /// </summary>
        public void                             CMD_DeleteIndex_Executed(ListView Arg)
        {
            if (Arg != null)
            {
                if (Arg.Name == "PlayList")
                {
                    bool NeedToNext             = false;
                    int Tmp2                    = PlaylistPos;
                    Model.Film Tmp              = Playlist.GetCurrent();
                    List<Model.Film> DelList    = new List<Model.Film>();

                    foreach (var F in Arg.SelectedItems)
                        DelList.Add(F as Model.Film);
                    foreach (var F in DelList)
                    {
                        if (Tmp == F)
                            NeedToNext = true;
                        Playlist.Remove(F as Model.Film);
                    }
                    if (NeedToNext)
                    {
                        CMD_Stop_Executed(null);
                        if (Playlist.Films.Count > 0)
                            LoadMedia(Playlist.GetIndex(Tmp2).Path);
                    }
                    if (Playlist.Films.Count == 0)
                    {
                        IsLoaded = false;
                        CMD_PlayPause.RaiseCanExecuteChanged();
                        CMD_Stop.RaiseCanExecuteChanged();
                        CMD_FullScreen.RaiseCanExecuteChanged();
                        CMD_Prev.RaiseCanExecuteChanged();
                        CMD_Next.RaiseCanExecuteChanged();
                        NotifyPropertyChanged("IsLoaded");
                    }
                    NotifyPropertyChanged("PlaylistPos");
                    CMD_SavePlayList.RaiseCanExecuteChanged();
                }
                else
                {
                    List<Model.Film> DelList = new List<Model.Film>();
                    foreach (var F in Arg.SelectedItems)
                        DelList.Add(F as Model.Film);
                    foreach (var F in DelList)
                        Library.Remove(F as Model.Film);
                }
            }
        }

        /// <summary>
        ///  Add To PLaylist
        /// </summary>
        public void                             CMD_AddToPlaylist_Executed(ListView Arg)
        {
            if (Arg != null && Arg.Name == "LibraryList")
            {
                foreach (var Item in Arg.SelectedItems)
                    Playlist.AddFilm(Library.GetIndex(Library.Films.IndexOf(Item as Model.Film)));
            }
        }

        /// <summary>
        /// Add To Playlist And Play
        /// </summary>
        public void                             CMD_AddToPlaylistAndPlay_Executed(ListView Arg)
        {
            if (Arg != null && Arg.Name == "LibraryList")
            {
                foreach (var Item in Arg.SelectedItems)
                    Playlist.AddFilm(Library.GetIndex(Library.Films.IndexOf(Item as Model.Film)));
                LoadMedia(Playlist.GetIndex(Playlist.Films.IndexOf(Arg.SelectedItems[0] as Model.Film)).Path);
            }
        }

        /// <summary>
        ///  Clear Playlist And Play
        /// </summary>
        public void                             CMD_ClearPlaylistAndPlay_Executed(ListView Arg)
        {
            if (Arg != null && Arg.Name == "LibraryList")
            {
                CMD_Stop_Executed(null);
                Playlist.Clean();
                foreach (var Item in Arg.SelectedItems)
                    Playlist.AddFilm(Library.GetIndex(Library.Films.IndexOf(Item as Model.Film)));
                LoadMedia(Playlist.GetFirst().Path);
            }
        }


        /// <summary>
        /// Handling Drag n Drop
        /// </summary>
        public void                             CMD_DragNDrop_Executed(DragEventArgs e)
        {
            var Player                          = new WindowsMediaPlayer();
            bool Empty                          = (Playlist.Films.Count == 0);
            String[] FileNames                  = (String[])e.Data.GetData(System.Windows.Forms.DataFormats.FileDrop, true);

            foreach (String _Path in FileNames)
            {
                if (MyGetType(Path.GetExtension(_Path)) != Model.Film.MediaType.Unknown)
                {
                    var Clip = Player.newMedia(_Path);
                    Playlist.AddFilm(new Model.Film(Path.GetFileNameWithoutExtension(_Path), TimeSpan.FromSeconds((Clip.duration == 0 ? 4 : Clip.duration)), _Path));
                }
            }
            if (Empty && Playlist.Films.Count > 0)
                LoadMedia(Playlist.GetFirst().Path);
        }


        /// <summary>
        /// Saving A Playlist
        /// </summary>
        public bool                             CMD_SavePlayList_CanExecute(Button Arg)
        {
            return (Playlist.Films.Count > 0);
        }


        public void                             CMD_SavePlayList_Executed(Button Arg)
        {
            SaveFileDialog Dialog               = new SaveFileDialog();
            Dialog.Filter                       += "Xml File (*.xml) | *.xml;";
            Dialog.InitialDirectory             = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (Dialog.ShowDialog() == true)
                Model.Xml.SaveXml(Playlist.Films, Dialog.FileName);
        }


        /// <summary>
        ///  Loading A Playlist
        /// </summary>
        public void                             CMD_LoadPlayList_Executed(Button Arg)
        {
            OpenFileDialog Dialog               = new OpenFileDialog();
            Dialog.Filter                       += "Xml File (*.xml) | *.xml;";
            Dialog.InitialDirectory             = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (Dialog.ShowDialog() == true)
            {
                Playlist.Clean();
                Loading                         = true;

                Task T = new Task(() =>
                    {
                        Model.Xml.LoadXml_Async(Playlist.Films, Dialog.FileName, this);
                        if (Playlist.Films.Count > 0)
                            App.Current.Dispatcher.BeginInvoke((Action)delegate() { LoadMedia(Playlist.GetFirst().Path); });
                    });

                T.ContinueWith(antecedent => Loading = false, TaskScheduler.FromCurrentSynchronizationContext());
                T.Start();
            }
        }


        /// <summary>
        ///  Sorting The Library
        /// </summary>
        public void                             CMD_SortLibrary_Executed(String Arg)
        {
            Library.Sort(Arg);
            NotifyPropertyChanged("LibraryListView");
        }



        /// <summary>
        ///  Just A Little Event Because The NaturalDuration Is Available Only In This Event (Weird...)
        /// </summary>
        public void                             EVNT_MediaOpened(object sender, RoutedEventArgs e)
        {
            Maximum = Playlist.GetCurrent().Duration.TotalSeconds;
            e.Handled = true;
        }

        /// <summary>
        /// MediaEnded Event
        /// </summary>
        public void                             EVNT_MediaEnded(object sender, RoutedEventArgs e)
        {
           CMD_Next_Executed(null);
        }

        /// <summary>
        ///  Changing Volume With The Wheel
        /// </summary>
        public void                             EVNT_Mousewheel(object sender, MouseWheelEventArgs e)
        {
            Video.Media.Volume += (e.Delta > 0) ? 0.1 : -0.1;
            if (Video.Media.Volume < 0)
                Video.Media.Volume = 0;
            else if (Video.Media.Volume > 1)
                Video.Media.Volume = 1;
            NotifyPropertyChanged("BtnMuteImg");
        }

        /// <summary>
        /// Custom Search
        /// </summary>
        public void                             EVNT_TxtSearchGotFocus(object sender, RoutedEventArgs e)
        {
            TextBox T                           = sender as TextBox;

            if (T != null && T.Text == "Custom Search...")
            {
                _GoSearch = false;
                T.Text = String.Empty;
                _GoSearch = true;
            }
        }

        public void                             EVNT_TxtSearchLostFocus(object sender, RoutedEventArgs e)
        {
            TextBox T                           = sender as TextBox;

            if (T != null && T.Text == "")
            {
                Library.Search(String.Empty);
                _GoSearch = false;
                T.Text = "Custom Search...";
                _GoSearch = true;
                NotifyPropertyChanged("LibraryListView");
            }
        }


        /// <summary>
        ///  Doble Click On A Elem Of A ListView
        /// </summary>
        public void                             EVNT_ListViewDoubleClick(object sender, MouseEventArgs e, MyWindowsMediaPlayer.MainWindow MW)
        {
            ListViewItem Item                   = sender as ListViewItem;

            if (Item != null)
            {
                if (Item.IsDescendantOf(MW.PlayList) && Playlist.GetIndex(MW.PlayList.SelectedIndex) != null)
                    LoadMedia(Playlist.GetIndex(MW.PlayList.SelectedIndex).Path);
                else if (Item.IsDescendantOf(MW.LibraryList) && Library.GetIndex(LibraryPos) != null && LibraryPos < Library.FilmsView.Count)
                {
                    Playlist.Clean();
                    Playlist.AddFilm(Library.FilmsView[LibraryPos]);
                    LoadMedia(Playlist.GetFirst().Path);
                }
            }
        }

        /// <summary>
        ///  FullScreen Double-Click Evenement
        /// </summary>
        public void                             EVNT_FullScreen(object sender, MouseEventArgs e)
        {
            CMD_FullScreen_Executed(null);
        }

        /// <summary>
        ///  Drop MEdia Evenement
        /// </summary>
        public void                             EVNT_VideoBorderDrop(object sender, DragEventArgs e)
        {
            CMD_DragNDrop_Executed(e);
        }


        /// <summary>
        /// Makes The Controls Reappear In FullScreen When Mouse Moves
        /// </summary>
        public void                             EVNT_MouseMoveVideo(object sender, MouseEventArgs e)
        {
            if (IsFullScreen && ControlZindex != 1000)
            {
                Task T = new Task(() =>
                {
                    //App.Current.Dispatcher.BeginInvoke((Action)delegate() { CMD_SavePlayList.RaiseCanExecuteChanged(); });
                    ControlZindex = 1000;
                    NotifyPropertyChanged("ControlZindex");
                    System.Threading.Thread.Sleep(1000);
                });

                T.ContinueWith(antecedent => { ControlZindex = 50; NotifyPropertyChanged("ControlZindex"); }, TaskScheduler.FromCurrentSynchronizationContext());
                T.Start();
            }
        }

        /// <summary>
        /// Initialization Of The Commands
        /// </summary>
        private void                            InitCommands()
        {
            _CMD_Open = new DelegateCommand<String>
                (
                    (Action<String>)(Delegate.CreateDelegate(typeof(Action<String>), this, "CMD_Open_Executed"))
                );
            _CMD_PlayPause = new DelegateCommand<Button>
                (
                    (Action<Button>)(Delegate.CreateDelegate(typeof(Action<Button>), this, "CMD_PlayPause_Executed")),
                    (Predicate<Button>)(Delegate.CreateDelegate(typeof(Predicate<Button>), this, "CMD_PlayPause_CanExecute"))
                );
            _CMD_Stop = new DelegateCommand<Button>
                (
                    (Action<Button>)(Delegate.CreateDelegate(typeof(Action<Button>), this, "CMD_Stop_Executed")),
                    (Predicate<Button>)(Delegate.CreateDelegate(typeof(Predicate<Button>), this, "CMD_Stop_CanExecute"))
                );
            _CMD_Mute = new DelegateCommand<Button>
                (
                    (Action<Button>)(Delegate.CreateDelegate(typeof(Action<Button>), this, "CMD_Mute_Executed"))
                );
            _CMD_FullScreen = new DelegateCommand<Button>
                (
                    (Action<Button>)(Delegate.CreateDelegate(typeof(Action<Button>), this, "CMD_FullScreen_Executed")),
                    (Predicate<Button>)(Delegate.CreateDelegate(typeof(Predicate<Button>), this, "CMD_FullScreen_CanExecute"))
                );
            _CMD_Next = new DelegateCommand<Button>
                (
                    (Action<Button>)(Delegate.CreateDelegate(typeof(Action<Button>), this, "CMD_Next_Executed")),
                    (Predicate<Button>)(Delegate.CreateDelegate(typeof(Predicate<Button>), this, "CMD_Next_CanExecute"))
                );
            _CMD_Prev = new DelegateCommand<Button>
                (
                    (Action<Button>)(Delegate.CreateDelegate(typeof(Action<Button>), this, "CMD_Prev_Executed")),
                    (Predicate<Button>)(Delegate.CreateDelegate(typeof(Predicate<Button>), this, "CMD_Prev_CanExecute"))
                );
            _CMD_PlayIndex = new DelegateCommand<ListView>
                (
                    (Action<ListView>)(Delegate.CreateDelegate(typeof(Action<ListView>), this, "CMD_PlayIndex_Executed"))
                );
            _CMD_DeleteIndex = new DelegateCommand<ListView>
                (
                    (Action<ListView>)(Delegate.CreateDelegate(typeof(Action<ListView>), this, "CMD_DeleteIndex_Executed"))
                );
            _CMD_AddToPlaylist = new DelegateCommand<ListView>
                (
                    (Action<ListView>)(Delegate.CreateDelegate(typeof(Action<ListView>), this, "CMD_AddToPlaylist_Executed"))
                );
            _CMD_SavePlayList = new DelegateCommand<Button>
                (
                    (Action<Button>)(Delegate.CreateDelegate(typeof(Action<Button>), this, "CMD_SavePlayList_Executed")),
                    (Predicate<Button>)(Delegate.CreateDelegate(typeof(Predicate<Button>), this, "CMD_SavePlayList_CanExecute"))
                );
            _CMD_LoadPlayList = new DelegateCommand<Button>
                (
                    (Action<Button>)(Delegate.CreateDelegate(typeof(Action<Button>), this, "CMD_LoadPlayList_Executed"))
                );
            _CMD_AddToPlaylistAndPlay = new DelegateCommand<ListView>
                (
                    (Action<ListView>)(Delegate.CreateDelegate(typeof(Action<ListView>), this, "CMD_AddToPlaylistAndPlay_Executed"))
                );
            _CMD_ClearPlaylistAndPlay = new DelegateCommand<ListView>
                (
                    (Action<ListView>)(Delegate.CreateDelegate(typeof(Action<ListView>), this, "CMD_ClearPlaylistAndPlay_Executed"))
                );
            _CMD_OpenFolder = new DelegateCommand<String>
                (
                    (Action<String>)(Delegate.CreateDelegate(typeof(Action<String>), this, "CMD_OpenFolder_Executed"))
                );
            _CMD_SortLibrary = new DelegateCommand<String>
                (
                    (Action<String>)(Delegate.CreateDelegate(typeof(Action<String>), this, "CMD_SortLibrary_Executed"))
                );
        }
    }
}