#if !NETSTANDARD1_0
using System.Drawing;
#endif

namespace Xamarin.Essentials
{
    public class BrowserLaunchOptions
    {
#if !NETSTANDARD1_0
        public Color? PreferredToolbarColor { get; set; }

        public Color? PreferredControlColor { get; set; }
#endif

        public BrowserLaunchMode LaunchMode { get; set; } = BrowserLaunchMode.SystemPreferred;

        public BrowserTitleMode TitleMode { get; set; } = BrowserTitleMode.Default;
    }
}
