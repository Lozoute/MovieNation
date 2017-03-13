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

namespace MyWindowsMediaPlayer.ViewModel
{
    /// <summary>
    /// Converter For The TimeSpan Text In The Main Control
    /// </summary>
    public class TimeSpanToStringConverter : IValueConverter
    {
        public Object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (TimeSpan.FromSeconds((double)(value)).ToString(@"hh\:mm\:ss"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (0);
        }
    }

    /// <summary>
    /// Converter For The Media Source
    /// </summary>
    public class StringToUriConverter : IValueConverter
    {
        public Object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (new Uri((string)(value)));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (value.ToString());
        }
    }

    /// <summary>
    /// Converter For PlayPause Button
    /// </summary>
    public class BoolToPlayPauseConverter : IValueConverter
    {
        public Object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((bool)(value) == true ? "PAUSE" : "PLAY");
        }

        public Object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((string)(value) == "PLAY" ? false : true);
        }
    }
}