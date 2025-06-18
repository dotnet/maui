using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Security.Authentication.OAuth;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using Windows.Security.Authentication.Web;

namespace Microsoft.Maui.Authentication
{
	partial class WebAuthenticatorImplementation : IWebAuthenticator
	{
		public async Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions)
		{
			var url = webAuthenticatorOptions?.Url;
			var callbackUrl = webAuthenticatorOptions?.CallbackUrl;

			bool isPackaged = global::Windows.ApplicationModel.Package.Current is not null;
			if(isPackaged)
			{
				if (!IsUriProtocolDeclared(callbackUrl.Scheme))
					throw new InvalidOperationException($"You need to declare the windows.protocol usage of the protocol/scheme `{callbackUrl.Scheme}` in your AppxManifest.xml file");
			}
			else
			{
				if (callbackUrl.Scheme == "http" || callbackUrl.Scheme == "https")
					throw new InvalidOperationException($"{callbackUrl.Scheme}:// schemes are not allowed for callbackUri. Use a custom scheme like 'myapp' instead.");
				var value = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(callbackUrl.Scheme);
				if (value is null || value.GetValue("URL Protocol") is null)
				{
					throw new InvalidOperationException($"The URI Scheme '{callbackUrl.Scheme}' is not registered. Call ActivationRegistrationManager.RegisterForProtocolActivation to register protocol activation.");
				}
			}

			var window = WindowStateManager.Default.GetActiveWindow();
			var hwnd = window is null ? IntPtr.Zero : WinRT.Interop.WindowNative.GetWindowHandle(window);
			if (hwnd == IntPtr.Zero)
				throw new InvalidOperationException("No active window found for authentication.");

			var windowId = UI.Win32Interop.GetWindowIdFromWindow(hwnd);

			AuthRequestParams authRequestParams = AuthRequestParams.CreateForAuthorizationCodeRequest("", callbackUrl);
			AuthRequestResult authRequestResult = await OAuth2Manager.RequestAuthWithParamsAsync(windowId, url, authRequestParams);
			if(authRequestResult.Failure is not null)
			{
				throw new UnauthorizedAccessException(authRequestResult.Failure.Error);
			}	
			return new WebAuthenticatorResult(authRequestResult.ResponseUri, webAuthenticatorOptions.ResponseDecoder);
		}

		static bool IsUriProtocolDeclared(string scheme)
		{
			var docPath = FileSystemUtils.PlatformGetFullAppPackageFilePath(PlatformUtils.AppManifestFilename);
			var doc = XDocument.Load(docPath, LoadOptions.None);
			var reader = doc.CreateReader();
			var namespaceManager = new XmlNamespaceManager(reader.NameTable);
			namespaceManager.AddNamespace("x", PlatformUtils.AppManifestXmlns);
			namespaceManager.AddNamespace("uap", "http://schemas.microsoft.com/appx/manifest/uap/windows10");

			// Check if the protocol was declared
			var decl = doc.Root.XPathSelectElements($"//uap:Extension[@Category='windows.protocol']/uap:Protocol[@Name='{scheme}']", namespaceManager);

			return decl != null && decl.Any();
		}
	}
}
