using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using MaterialDesignThemes.Wpf;
using SharpVectors.Converters;

namespace U2MP3.Models
{
    public class Music : INotifyPropertyChanged
    {
        private PackIconKind iconKind;

        public Music(string id, string title, string thumbnailUrl, string kind)
        {
            Id = id;
            Title = title;
            ThumbnailUrl = thumbnailUrl;
            Kind = kind;
            IconKind = PackIconKind.PlayCircleOutline;
        }

        public string Id { get; set; }
        public string Title { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Kind { get; set; }
        public string SourceUrl { get; set; }
        public PackIconKind IconKind
        {
            get => iconKind;
            set
            {
                iconKind = value;
                OnPropertyChanged(nameof(IconKind));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region override

         public override bool Equals(object obj)
        {
            if (!(obj is Music)) return false;
            var music = (Music)obj;
            return music.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        #endregion
       
    }

    public class StateToUriConverter : EnumConverter
    {
        public StateToUriConverter(Type type) : base(type)
        {
        }
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                if (value != null)
                {
                    FieldInfo fi = value.GetType().GetField(value.ToString());
                    if (fi != null)
                    {
                        var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                        return ((attributes.Length > 0) && (!String.IsNullOrEmpty(attributes[0].Description))) ? attributes[0].Description : value.ToString();
                    }
                }
                return string.Empty;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    //[TypeConverter(typeof(StateToUriConverter))]
    //public enum IconKind
    //{
    //    [Description("PlayCircleOutline")]
    //    PLAYING,
    //    [Description("StopCircleOutline")]
    //    STOP,
    //    [Description("PauseCircleOutline")]
    //    PAUSE
    //}

    //public class PackIconAttachedProperties : DependencyObject
    //{
    //    public static string GetSource(DependencyObject obj)
    //    {
    //        return (string)obj.GetValue(SourceProperty);
    //    }

    //    public static void SetSource(DependencyObject obj, string value)
    //    {
    //        obj.SetValue(SourceProperty, value);
    //    }

    //    private static void OnKindChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    //    {
    //        if (obj is PackIcon packIcon)
    //        {
    //            var path = (string)e.NewValue;
    //            packIcon.Kind = PackIconKind.AbTesting;
    //        }
    //    }

    //    public static readonly DependencyProperty SourceProperty =
    //        DependencyProperty.RegisterAttached("Kind",
    //            typeof(string), typeof(PackIconAttachedProperties),
    //            // default value: null
    //            new PropertyMetadata(null, OnKindChanged));
    //}
}