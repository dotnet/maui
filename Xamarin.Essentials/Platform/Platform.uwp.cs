using Windows.ApplicationModel.Activation;

namespace Xamarin.Essentials
{
    public static partial class Platform
    {
        internal const string AppManifestFilename = "AppxManifest.xml";
        internal const string AppManifestXmlns = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";

        public static string MapServiceToken { get; set; }
    }
}
