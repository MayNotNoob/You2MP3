using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using SharpVectors.Converters;

namespace U2MP3.Models
{
    public class Music : INotifyPropertyChanged
    {
        private ImageState _imageState;

        public Music(string id, string title, string thumbnailUrl, string kind)
        {
            Id = id;
            Title = title;
            ThumbnailUrl = thumbnailUrl;
            Kind = kind;
            ImageState = ImageState.STOP;
        }

        public string Id { get; set; }
        public string Title { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Kind { get; set; }
        public string SourceUrl { get; set; }
        public ImageState ImageState
        {
            get => _imageState;
            set
            {
                _imageState = value;
                OnPropertyChanged(nameof(ImageState));
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

    [TypeConverter(typeof(StateToUriConverter))]
    public enum ImageState
    {
        [Description("./Assets/pause-button.svg")]
        PLAYING,
        [Description("./Assets/play-button.svg")]
        STOP,
        [Description("./Assets/play-button.svg")]
        PAUSE
    }

    public class SvgViewboxAttachedProperties : DependencyObject
    {
        public static string GetSource(DependencyObject obj)
        {
            return (string)obj.GetValue(SourceProperty);
        }

        public static void SetSource(DependencyObject obj, string value)
        {
            obj.SetValue(SourceProperty, value);
        }

        private static void OnSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is SvgViewbox svgControl)
            {
                var path = (string)e.NewValue;
                svgControl.Source = new Uri(path, UriKind.Relative);
            }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.RegisterAttached("Source",
                typeof(string), typeof(SvgViewboxAttachedProperties),
                // default value: null
                new PropertyMetadata(null, OnSourceChanged));
    }
}