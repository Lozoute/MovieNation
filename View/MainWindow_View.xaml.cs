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

namespace MyWindowsMediaPlayer
{
    /// <summary>
    /// Interaction, Initialization
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow                       ()
        {
            InitializeComponent();
            var VM                              = new ViewModel.Video_ViewModel();
            this.DataContext                    = VM;
            this.MouseWheel                     += new MouseWheelEventHandler(VM.EVNT_Mousewheel);
            this.TxtSearch.GotFocus             += new RoutedEventHandler(VM.EVNT_TxtSearchGotFocus);
            this.TxtSearch.LostFocus            += new RoutedEventHandler(VM.EVNT_TxtSearchLostFocus);
            this.VideoBorder.Drop               += new DragEventHandler(VM.EVNT_VideoBorderDrop);
            this.MainVideo.MouseDoubleClick     += new MouseButtonEventHandler(VM.EVNT_FullScreen);
            this.VideoWrapper.MouseMove         += new MouseEventHandler(VM.EVNT_MouseMoveVideo);

            this.InputBindings.Add(new KeyBinding(VM.CMD_PlayPause, new KeyGesture(Key.Space)));
            this.InputBindings.Add(new KeyBinding(VM.CMD_FullScreen, new KeyGesture(Key.Escape)));
        }

        /// <summary>
        ///  Redirection To The ViewModel
        /// </summary>
        private void                            ListView_DoubleClick(object sender, MouseButtonEventArgs e)
        {(this.DataContext as ViewModel.Video_ViewModel).EVNT_ListViewDoubleClick(sender, e, this);}
    }
}
