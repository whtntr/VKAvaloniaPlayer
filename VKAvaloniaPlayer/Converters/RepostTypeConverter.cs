﻿using Avalonia.Data.Converters;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VKAvaloniaPlayer.Models;

namespace VKAvaloniaPlayer.Converters
{
    public class RepostTypeConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is RepostToType)
            {
                var val = (RepostToType)value;
                return val switch
                {
                    RepostToType.Friend => "Друзья",
                    RepostToType.Dialog => "Диалоги",
                    _ => val.ToString(),
                };
            }
            else return "Неизвестно";
        
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
