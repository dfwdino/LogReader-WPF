﻿using System.Windows.Media;

namespace LogReader_WPF
{
    public struct ErrorColors
    {
        public static SolidColorBrush Error => Brushes.Red;
        public static SolidColorBrush Warning => Brushes.Yellow;

        public static SolidColorBrush EvenRowColor => Brushes.White;

        public static SolidColorBrush OddRowColor => Brushes.FloralWhite;
    }
}
