﻿using System;
using Windows.UI.Xaml.Data;

namespace App1.Common
{
    /// <summary>
    /// Convertisseur de valeur qui convertit true en false et vice versa.
    /// </summary>
    public sealed class BooleanNegationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return !(value is bool && (bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return !(value is bool && (bool)value);
        }
    }
}
