using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Windows.Devices.Geolocation;

namespace Microsoft.Caboodle
{
    internal static partial class Permissions
    {
        const string appManifestFilename = "AppxManifest.xml";
        const string appManifestXmlns = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";

        static void PlatformEnsureDeclared(PermissionType permission)
        {
            var uwpCapabilities = permission.ToUWPCapabilities();

            // If no actual UWP capabilities are required here, just return
            if (uwpCapabilities == null || !uwpCapabilities.Any())
                return;

            var doc = XDocument.Load(appManifestFilename, LoadOptions.None);
            var reader = doc.CreateReader();
            var namespaceManager = new XmlNamespaceManager(reader.NameTable);
            namespaceManager.AddNamespace("x", appManifestXmlns);
            foreach (var cap in uwpCapabilities)
            {
                // If the manifest doesn't contain a capability we need, throw
                if ((!doc.Root.XPathSelectElements($"//x:DeviceCapability[@Name='{cap}']", namespaceManager)?.Any() ?? false) &&
                    (!doc.Root.XPathSelectElements($"//x:Capability[@Name='{cap}']", namespaceManager)?.Any() ?? false))
                    throw new PermissionException($"You need to declare the capability `{cap}` in your AppxManifest.xml file");
            }
        }

        static Task<PermissionStatus> PlatformCheckStatusAsync(PermissionType permission)
        {
            switch (permission)
            {
                case PermissionType.LocationWhenInUse:
                    return CheckLocationAsync();
            }

            return Task.FromResult(PermissionStatus.Granted);
        }

        static Task<PermissionStatus> PlatformRequestAsync(PermissionType permission) =>
            PlatformCheckStatusAsync(permission);

        static async Task<PermissionStatus> CheckLocationAsync()
        {
            var accessStatus = await Geolocator.RequestAccessAsync();

            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:
                    return PermissionStatus.Granted;
                case GeolocationAccessStatus.Unspecified:
                    return PermissionStatus.Unknown;
            }

            return PermissionStatus.Denied;
        }
    }

    internal static class PermissionTypeExtensions
    {
        internal static string[] ToUWPCapabilities(this PermissionType permissionType)
        {
            switch (permissionType)
            {
                case PermissionType.LocationWhenInUse:
                    return new[] { "location" };
            }

            return null;
        }
    }
}
