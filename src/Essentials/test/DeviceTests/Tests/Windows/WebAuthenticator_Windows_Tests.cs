using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Maui.Authentication;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	[Category("WebAuthenticator")]
	public class WebAuthenticator_Windows_Tests
	{
		[Fact]
		public async Task PlatformValidationPrecedesPreCanceledToken()
		{
			var options = new WebAuthenticatorOptions
			{
				Url = new Uri("https://example.com/auth"),
				CallbackUrl = new Uri("https://example.com/callback"),
			};

			var cancellationToken = new CancellationToken(canceled: true);

			var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
				WebAuthenticator.Default.AuthenticateAsync(options, cancellationToken));

			Assert.Contains("not supported", exception.Message, StringComparison.OrdinalIgnoreCase);
		}

		[Theory]
		[InlineData("maui-auth://", "Microsoft.Maui.WebAuthenticator:maui-auth")]
		[InlineData("MAUI-AUTH://", "Microsoft.Maui.WebAuthenticator:maui-auth")]
		[InlineData("maui-auth://callback", "Microsoft.Maui.WebAuthenticator:maui-auth")]
		[InlineData("maui-auth://other/path?code=123", "Microsoft.Maui.WebAuthenticator:maui-auth")]
		public void CreateCallbackRouteKeyUsesSchemeOnly(string callbackUrl, string expected)
		{
			var routeKey = WebAuthenticatorImplementation.CreateCallbackRouteKey(new Uri(callbackUrl));

			Assert.Equal(expected, routeKey);
		}

		[Theory]
		[InlineData(null, true)]
		[InlineData("", true)]
		[InlineData("Maui.App", false)]
		[InlineData("Microsoft.Maui.WebAuthenticator:maui-auth", true)]
		[InlineData("Microsoft.Maui.WebAuthenticator:other-auth", true)]
		[InlineData("Microsoft.Maui.WebAuthenticator2:maui-auth", false)]
		public void CanRegisterCallbackRoutePreservesApplicationKeys(string currentKey, bool expected)
		{
			var actual = WebAuthenticatorImplementation.CanRegisterCallbackRoute(currentKey);

			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData("maui-auth://callback", "maui-auth://callback", true)]
		[InlineData("maui-auth://callback", "maui-auth://other/path", true)]
		[InlineData("MAUI-AUTH://callback", "maui-auth://callback", true)]
		[InlineData("maui-auth://callback", "other-auth://callback", false)]
		public void IsSameCallbackRouteUsesSchemeOnly(string expectedCallbackUrl, string callbackUrl, bool expected)
		{
			var actual = WebAuthenticatorImplementation.IsSameCallbackRoute(
				new Uri(expectedCallbackUrl),
				new Uri(callbackUrl));

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ManifestProtocolLookupUsesCurrentApplicationAndVersionAgnosticUapNamespace()
		{
			var document = XDocument.Parse("""
				<Package xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10"
				         xmlns:uap5="http://schemas.microsoft.com/appx/manifest/uap/windows10/5">
				  <Applications>
				    <Application Id="OtherApp">
				      <Extensions>
				        <uap5:Extension Category="windows.protocol">
				          <uap5:Protocol Name="other-auth" />
				        </uap5:Extension>
				      </Extensions>
				    </Application>
				    <Application Id="CurrentApp">
				      <Extensions>
				        <uap5:Extension Category="windows.protocol">
				          <uap5:Protocol Name="MAUI-AUTH" />
				        </uap5:Extension>
				      </Extensions>
				    </Application>
				  </Applications>
				</Package>
				""");

			Assert.True(WebAuthenticatorImplementation.IsUriProtocolDeclared(document, "CurrentApp", "maui-auth"));
			Assert.False(WebAuthenticatorImplementation.IsUriProtocolDeclared(document, "CurrentApp", "other-auth"));
			Assert.False(WebAuthenticatorImplementation.IsUriProtocolDeclared(document, null, "maui-auth"));
		}

		[Fact]
		public void ManifestProtocolLookupCanUseTheOnlyApplicationAsIdentityFallback()
		{
			var document = XDocument.Parse("""
				<Package xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10"
				         xmlns:uap="http://schemas.microsoft.com/appx/manifest/uap/windows10">
				  <Applications>
				    <Application Id="OnlyApp">
				      <Extensions>
				        <uap:Extension Category="windows.protocol">
				          <uap:Protocol Name="maui-auth" />
				        </uap:Extension>
				      </Extensions>
				    </Application>
				  </Applications>
				</Package>
				""");

			Assert.True(WebAuthenticatorImplementation.IsUriProtocolDeclared(document, null, "MAUI-AUTH"));
		}

		[Theory]
		[InlineData(null, true)]
		[InlineData("", true)]
		[InlineData("\"C:\\Program Files\\Maui App\\maui.exe\" \"%1\"", true)]
		[InlineData("\"c:\\PROGRAM FILES\\MAUI APP\\MAUI.EXE\" --protocol \"%1\"", true)]
		[InlineData("\"C:\\Other\\other.exe\" \"%1\"", false)]
		[InlineData("C:\\Apps\\maui.exe %1", false)]
		[InlineData("C:\\Program Files\\Maui App\\maui.exe %1", true)]
		[InlineData("maui.exe %1", true)]
		[InlineData("\"unterminated command", true)]
		public void RegistryOwnershipRejectsOnlyCertainOtherExecutables(string command, bool expected)
		{
			var actual = WebAuthenticatorImplementation.IsRegistryCommandOwnedByCurrentExecutable(
				command,
				@"C:\Program Files\Maui App\maui.exe");

			Assert.Equal(expected, actual);
		}
	}
}
