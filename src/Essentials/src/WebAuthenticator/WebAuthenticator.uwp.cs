using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Windows.Security.Authentication.Web;

namespace Microsoft.Maui.Essentials
{
    public static partial class WebAuthenticator
    {
        static async Task<WebAuthenticatorResult> PlatformAuthenticateAsync(Uri url, Uri callbackUrl)
        {
            if (!IsUriProtocolDeclared(callbackUrl.Scheme))
                throw new InvalidOperationException($"You need to declare the windows.protocol usage of the protocol/scheme `{callbackUrl.Scheme}` in your AppxManifest.xml file");

            try
            {
                var r = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, url, callbackUrl);

                switch (r.ResponseStatus)
                {
                    case WebAuthenticationStatus.Success:
                        // For GET requests this is a URI:
                        var resultUri = new Uri(r.ResponseData.ToString());
                        return new WebAuthenticatorResult(resultUri);
                    case WebAuthenticationStatus.UserCancel:
                        throw new TaskCanceledException();
                    case WebAuthenticationStatus.ErrorHttp:
                        throw new HttpRequestException("Error: " + r.ResponseErrorDetail);
                    default:
                        throw new Exception("Response: " + r.ResponseData.ToString() + "\nStatus: " + r.ResponseStatus);
                }
            }
            catch (FileNotFoundException)
            {
                throw new TaskCanceledException();
            }
        }

        static bool IsUriProtocolDeclared(string scheme)
        {
            var doc = XDocument.Load(Platform.AppManifestFilename, LoadOptions.None);
            var reader = doc.CreateReader();
            var namespaceManager = new XmlNamespaceManager(reader.NameTable);
            namespaceManager.AddNamespace("x", Platform.AppManifestXmlns);
            namespaceManager.AddNamespace("uap", "http://schemas.microsoft.com/appx/manifest/uap/windows10");

            // Check if the protocol was declared
            var decl = doc.Root.XPathSelectElements($"//uap:Extension[@Category='windows.protocol']/uap:Protocol[@Name='{scheme}']", namespaceManager);

            return decl != null && decl.Any();
        }
    }
}
