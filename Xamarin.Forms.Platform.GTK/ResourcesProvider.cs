using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.GTK
{
    internal class ResourcesProvider : ISystemResourcesProvider
    {
        private const string TitleStyleKey = "HeaderLabelStyle";
        private const string SubtitleStyleKey = "SubheaderLabelStyle";
        private const string BodyStyleKey = "BodyLabelStyle";
        private const string CaptionStyleKey = "CaptionLabelStyle";
        private const string ListItemDetailTextStyleKey = "BodyLabelStyle";
        private const string ListItemTextStyleKey = "BaseLabelStyle";

        public IResourceDictionary GetSystemResources()
        {
            return new ResourceDictionary
            {
                [Device.Styles.TitleStyleKey] = GetStyle(TitleStyleKey),
                [Device.Styles.SubtitleStyleKey] = GetStyle(SubtitleStyleKey),
                [Device.Styles.BodyStyleKey] = GetStyle(BodyStyleKey),
                [Device.Styles.CaptionStyleKey] = GetStyle(CaptionStyleKey),
                [Device.Styles.ListItemDetailTextStyleKey] = GetStyle(ListItemDetailTextStyleKey),
                [Device.Styles.ListItemTextStyleKey] = GetStyle(ListItemTextStyleKey)
            };
        }

        private Style GetStyle(string nativeKey)
        {
            var result = new Style(typeof(Label));

            switch(nativeKey)
            {
                case TitleStyleKey:
                    result.Setters.Add(new Setter { Property = Label.FontSizeProperty, Value = 24 });
                    break;
                case SubtitleStyleKey:
                    result.Setters.Add(new Setter { Property = Label.FontSizeProperty, Value = 20 });
                    break;
                case BodyStyleKey:
                    result.Setters.Add(new Setter { Property = Label.TextColorProperty, Value = Color.Blue });
                    break;
                case CaptionStyleKey:
                    break;
                case ListItemTextStyleKey:
                    break;
            }

            return result;
        }
    }
}