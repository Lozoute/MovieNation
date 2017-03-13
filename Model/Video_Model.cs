using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.Linq;
using Microsoft.Win32;
using System.Threading.Tasks;
using WMPLib;

namespace MyWindowsMediaPlayer.Model
{
    /// <summary>
    /// MediaElement Encapsulation
    /// </summary>
    class Video
    {
        private MediaElement        _Media;

        public MediaElement         Media { get { return _Media; } set { _Media = value; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public Video                ()
        {
            _Media                  = new MediaElement();
            _Media.LoadedBehavior   = MediaState.Manual;
            _Media.UnloadedBehavior = MediaState.Manual;
            _Media.Position         = TimeSpan.FromSeconds(0);

        }
    }


    /// <summary>
    /// Library System
    /// </summary>
    class Library
    {
        private ObservableCollection<Film>      _Films;
        private ObservableCollection<Film>      _FilmsView;
        private readonly string                 _LibFile;
        private bool                            _Ok;

        public ObservableCollection<Film>       Films { get { return _Films; } }
        public ObservableCollection<Film>       FilmsView { get { return _FilmsView; } }
        public bool                             VideoChecked { get; set; }
        public bool                             AudioChecked { get; set; }
        public bool                             ImageChecked { get; set; }
        public bool                             TitleAsc { get; set; }
        public bool                             TypeAsc { get; set; }
        public bool                             DurationAsc { get; set; }


        /// <summary>
        /// Constructor & Dtor
        /// </summary>
        public Library                          (string _File, ViewModel.Video_ViewModel VM)
        {
            _Films                              = new ObservableCollection<Film>();
            _FilmsView                          = new ObservableCollection<Film>();
            _LibFile                            = _File;
            VM.Loading                          = true;
            AudioChecked                        = true;
            VideoChecked                        = true;
            ImageChecked                        = true;
            TitleAsc                            = true;
            DurationAsc                         = true;
            TypeAsc                             = true;
            Task T                              = new Task(() => { _Ok = Model.Xml.LoadXml_Async(_Films, _LibFile, VM); _FilmsView = _Films; });

            T.ContinueWith(antecedent => VM.Loading = false, TaskScheduler.FromCurrentSynchronizationContext());
            T.Start();
        }

        ~Library                                ()
        {
            if (_Ok)
                Xml.SaveXml(_Films, _LibFile);
        }


        public void                             AddFilm(Film NewFilm)
        {
            if (_Films.IndexOf(NewFilm) == -1)
            {
                _Films.Add(NewFilm);
                if (IsChecked(NewFilm.Type) && _FilmsView.IndexOf(NewFilm) == -1)
                    _FilmsView.Add(NewFilm);
                _Ok = true;
            }
        }

        public void                             DeleteIndex(int Index)
        {
            _Films.RemoveAt(Index);
        }

        public Film                             GetIndex(int Index)
        {
            if (_Films.Count == 0)
                throw new System.InvalidOperationException("Operation On Empty Playlist");
            if (Index < 0 || Index > _Films.Count - 1)
                return (null);
            return (_Films[Index]);
        }

        public void                             SawDat(int Index)
        {
            if (_Films.Count == 0 || Index < 0 || Index > _Films.Count - 1)
                throw new System.InvalidOperationException("Operation On Empty Playlist");
            _Films[Index].Saw = true;
        }

        public void                             Remove(Film F)
        {
            _Films.Remove(F);
            _FilmsView.Remove(F);
        }

        public void                             Check(bool Value, Film.MediaType Type)
        {
            if (Value)
            {
                foreach (Film F in _Films)
                    if (F.Type == Type && _FilmsView.IndexOf(F) == -1)
                        _FilmsView.Add(F);
            }
            else
            {
                ObservableCollection<Film> Tmp  = new ObservableCollection<Film>();

                foreach (Film F in _FilmsView)
                    if (F.Type != Type)
                        Tmp.Add(F);
                _FilmsView = Tmp;
            }
        }

        public bool                             IsChecked(Film.MediaType Type)
        {
            if (Type == Film.MediaType.Video)
                return (VideoChecked);
            else if (Type == Film.MediaType.Audio)
                return (AudioChecked);
            else if (Type == Film.MediaType.Image)
                return (ImageChecked);
            return (false);
        }

        public void                             Sort(String Arg)
        {
            switch (Arg)
            {
                case "TITLE":
                    if (TitleAsc)
                        _FilmsView = new ObservableCollection<Film>(_FilmsView.OrderByDescending(a => a.Title));
                    else
                        _FilmsView = new ObservableCollection<Film>(_FilmsView.OrderBy(a => a.Title));
                    TitleAsc = !TitleAsc;
                    break;
                case "DURATION":
                    if (DurationAsc)
                        _FilmsView = new ObservableCollection<Film>(_FilmsView.OrderByDescending(a => a.Duration));
                    else
                        _FilmsView = new ObservableCollection<Film>(_FilmsView.OrderBy(a => a.Duration));
                    DurationAsc = !DurationAsc;
                    break;
                case "TYPE":
                    if (TypeAsc)
                        _FilmsView = new ObservableCollection<Film>(_FilmsView.OrderByDescending(a => a.Type));
                    else
                        _FilmsView = new ObservableCollection<Film>(_FilmsView.OrderBy(a => a.Type));
                    TypeAsc = !TypeAsc;
                    break;
                default:
                    break;
            }

        }

        public void                             Search (String Arg)
        {
            ObservableCollection<Film> Tmp      = new ObservableCollection<Film>();

            foreach (Film F in _Films)
                if (F.Title.ToLower().IndexOf(Arg.ToLower()) != -1 && IsChecked(F.Type))
                    Tmp.Add(F);
            _FilmsView = Tmp;
        }
    }


    /// <summary>
    /// Playlist System
    /// </summary>
    class Playlist
    {
        private ObservableCollection<Film>      _Films;
        private int                             _Pos;

        public ObservableCollection<Film>        Films { get { return _Films; } }
        public int                              Pos { get { return _Pos; } set { _Pos = (_Films.Count > 0 ? (value > 0 ? value : 0) % _Films.Count : 0); } }


        /// <summary>
        /// constructor
        /// </summary>
        public Playlist()
        {
            _Films                              = new ObservableCollection<Film>();
            _Pos                                = 0;
        }


        public void                             AddFilm(Film NewFilm)
        {
            _Films.Add(NewFilm);
        }

        public void                             Clean()
        {
            _Films.Clear();
            _Pos                                = 0;
        }

        public Film                             GetFirst()
        {
            if (_Films.Count == 0)
                throw new System.InvalidOperationException("Operation On Empty Playlist");
            _Pos = 0;
            return (_Films[_Pos]);
        }

        public Film                             GetLast()
        {
            if (_Films.Count == 0)
                throw new System.InvalidOperationException("Operation On Empty Playlist");
            _Pos = _Films.Count - 1;
            return (_Films[_Pos]);
        }

        public Film                             GetNext()
        {
            if (_Films.Count == 0)
                throw new System.InvalidOperationException("Operation On Empty Playlist");
            _Pos = (_Pos + 1) % _Films.Count;
            return (_Films[_Pos]);
        }

        public Film                             GetPrev()
        {
            if (_Films.Count == 0)
                throw new System.InvalidOperationException("Operation On Empty Playlist");
            --_Pos;
            if (_Pos < 0) return (GetLast());
            return (_Films[_Pos]);
        }

        public Film                             GetCurrent()
        {
            if (_Films.Count == 0)
                throw new System.InvalidOperationException("Operation On Empty Playlist");
            return (_Films[_Pos]);
        }

        public Film                             GetIndex(int Index)
        {
            if (_Films.Count == 0)
                throw new System.InvalidOperationException("Operation On Empty Playlist");
            if (Index < 0)
                Index = Index * -1;
            _Pos = Index % _Films.Count;
            return (_Films[_Pos]);
        }

        public void                             DeleteIndex(int Index)
        {
            _Films.RemoveAt(Index);
        }

        public void                             Remove(Film F)
        {
            _Films.Remove(F);
        }
    }


    /// <summary>
    ///  Film Encapsulation
    /// </summary>
    class Film
    {
        public enum MediaType
        {
            Video,
            Audio,
            Image,
            Unknown,
        }
        private readonly TimeSpan               _Duration;
        private readonly MediaType              _Type;

        public bool                             Saw { get; set; }
        public string                           Title { get; set; }
        public string                           Path { get; set; }
        public TimeSpan                         Duration { get { return _Duration; } }
        public MediaType                        Type { get { return _Type; } }
        public string                           TypeImgFile { get {
            if (Type == MediaType.Image) return "pack://siteoforigin:,,,/MyResources/ImageIcon.png";
            else if (Type == MediaType.Audio) return "pack://siteoforigin:,,,/MyResources/AudioIcon.png";
            else return "pack://siteoforigin:,,,/MyResources/VideoIcon.png"; } }

        public bool                             Loaded { get; set; }


        /// <summary>
        /// constructor
        /// </summary>
        public Film                             (string title, TimeSpan duration, string path)
        {
            Title                               = title;
            _Duration                           = duration;
            Path                                = path;
            Saw                                 = false;
        }
        public Film                             (string title, TimeSpan duration, string path, bool saw, MediaType type)
        {
            Title                               = title;
            _Duration                           = duration;
            Path                                = path;
            Saw                                 = saw;
            _Type                               = type;
        }
    }

    /// <summary>
    /// Save & Load
    /// </summary>
    class Xml
    {
        public static bool               LoadXml(ObservableCollection<Film> _List, String _File)
        {
            if (File.Exists(_File))
            {
                try
                {
                    var Player          = new WindowsMediaPlayer();
                    XDocument XmlLoader = XDocument.Load(_File);
                    var xFilm           = from Lvl1 in XmlLoader.Descendants("Film") select new { xPath = Lvl1.Value };

                    foreach (var Lvl1 in xFilm)
                    {
                        string Tmp      = Lvl1.xPath;
                        var Clip        = Player.newMedia(Tmp);

                        if (!File.Exists(Tmp))
                            System.Windows.Forms.MessageBox.Show("Unable To Load " + Tmp, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                        else
                            _List.Add(new Film(Path.GetFileNameWithoutExtension(Tmp), TimeSpan.FromSeconds((Clip.duration == 0 ? 4 : Clip.duration)), Tmp, false, ViewModel.Video_ViewModel.MyGetType(Path.GetExtension(Tmp))));
                    }
                    return (true);
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show("Library Loading Error: " + e.Message.ToString(), "Exception", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                    return (false);
                }
            }
            else
                System.Windows.Forms.MessageBox.Show("Library Loading Error: The File " + _File + " Does Not Exists.", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
            return (false);
        }

        [STAThread]
        public static bool               LoadXml_Async(ObservableCollection<Film> _List, String _File, ViewModel.Video_ViewModel VM)
        {
            if (File.Exists(_File))
            {
                try
                {
                    var Player          = new WindowsMediaPlayer();
                    XDocument XmlLoader = XDocument.Load(_File);
                    var xFilm           = from Lvl1 in XmlLoader.Descendants("Film") select new { xPath = Lvl1.Value };

                    foreach (var Lvl1 in xFilm)
                    {
                        string Tmp      = Lvl1.xPath;
                        var Clip        = Player.newMedia(Tmp);

                        if (!File.Exists(Tmp) || ViewModel.Video_ViewModel.MyGetType(Path.GetExtension(Tmp)) == Film.MediaType.Unknown)
                            System.Windows.Forms.MessageBox.Show("Unable To Load " + Tmp, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                        else
                        {
                            Film NewFilm = new Film(Path.GetFileNameWithoutExtension(Tmp), TimeSpan.FromSeconds((Clip.duration == 0 ? 4 : Clip.duration)), Tmp, false, ViewModel.Video_ViewModel.MyGetType(Path.GetExtension(Tmp)));
                            App.Current.Dispatcher.BeginInvoke((Action)delegate() { _List.Add(NewFilm); VM.NbFilesLoadedInt = VM.NbFilesLoadedInt + 1; });
                        }
                    }
                    return (true);
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show("Library Loading Error: " + e.Message.ToString() + e.Source.ToString(), "Exception", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                    return (false);
                }
            }
            else
                System.Windows.Forms.MessageBox.Show("Library Loading Error: The File " + _File + " Does Not Exists.", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
            return (false);
        }

        public static bool               SaveXml(ObservableCollection<Film> _List, String _File)
        {
            try
            {
                if (!File.Exists(_File))
                    File.Create(_File).Close();

                var xFilms              = new XElement("Films", from film in _List select new XElement("Film", film.Path));
                xFilms.Save(_File);
                return (true);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Library Saving Error: " + e.Message.ToString(), "Exception", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                return (false);
            }
        }
    }
}
