using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Windows.Devices.Geolocation;

namespace Xamarin.Essentials
{
    internal static partial class Permissions
    {
        const string appManifestFilename = "AppxManifest.xml";
        const string appManifestXmlns = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";

        static bool PlatformEnsureDeclared(PermissionType permission, bool throwIfMissing)
        {
            var uwpCapabilities = permission.ToUWPCapabilities();

            // If no actual UWP capabilities are required here, just return
            if (uwpCapabilities == null || !uwpCapabilities.Any())
                return true;

            var doc = XDocument.Load(appManifestFilename, LoadOptions.None);
            var reader = doc.CreateReader();
            var namespaceManager = new XmlNamespaceManager(reader.NameTable);
            namespaceManager.AddNamespace("x", appManifestXmlns);
            foreach (var cap in uwpCapabilities)
            {
                // If the manifest doesn't contain a capability we need, throw
                if ((!doc.Root.XPathSelectElements($"//x:DeviceCapability[@Name='{cap}']", namespaceManager)?.Any() ?? false) &&
                    (!doc.Root.XPathSelectElements($"//x:Capability[@Name='{cap}']", namespaceManager)?.Any() ?? false))
                {
                    if (throwIfMissing)
                        throw new PermissionException($"You need to declare the capability `{cap}` in your AppxManifest.xml file");
                    else
                        return false;
                }
            }

            return true;
        }

        static Task<PermissionStatus> PlatformCheckStatusAsync(PermissionType permission)
        {
            switch (permission)
            {
                case PermissionType.LocationWhenInUse:
                    return CheckLocationAsync();
                default:
                    return Task.FromResult(PermissionStatus.Granted);
            }
        }

        static Task<PermissionStatus> PlatformRequestAsync(PermissionType permission) =>
            PlatformCheckStatusAsync(permission);

        static async Task<PermissionStatus> CheckLocationAsync()
        {
            if (!MainThread.IsMainThread)
                throw new PermissionException("Permission request must be invoked on main thread.");

            var accessStatus = await Geolocator.RequestAccessAsync();
            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:
                    return PermissionStatus.Granted;
                case GeolocationAccessStatus.Unspecified:
                    return PermissionStatus.Unknown;
                default:
                    return PermissionStatus.Denied;
            }
        }
    }

    static class PermissionTypeExtensions
    {
        internal static string[] ToUWPCapabilities(this PermissionType permissionType)
        {
            switch (permissionType)
            {
                case PermissionType.LocationWhenInUse:
                    return new[] { "location" };
                default:
                    return null;
            }
        }
    }
}
