using Microsoft.Maui.Authentication;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests;

[Category("Passkeys")]
public class Passkeys_Windows_Tests
{
	[Theory]
	[InlineData("required", null, true, false)]
	[InlineData("preferred", true, false, true)]
	[InlineData("discouraged", true, false, false)]
	[InlineData(null, true, true, false)]
	[InlineData(null, false, false, false)]
	public void MapResidentKeyPreservesModernModes(
		string residentKey,
		bool? legacyRequireResidentKey,
		bool expectedRequire,
		bool expectedPrefer)
	{
		var selection = new WebAuthn.AuthenticatorSelection
		{
			ResidentKey = residentKey,
			RequireResidentKey = legacyRequireResidentKey,
		};

		var actual = PasskeysImplementation.MapResidentKey(selection);

		Assert.Equal(expectedRequire, actual.Require);
		Assert.Equal(expectedPrefer, actual.Prefer);
	}

	[Theory]
	[InlineData(2u, true, 3u)]
	[InlineData(3u, true, 4u)]
	[InlineData(3u, false, 3u)]
	[InlineData(9u, false, 9u)]
	[InlineData(9u, true, 9u)]
	public void MakeCredentialOptionsVersionMatchesCapabilities(
		uint apiVersion,
		bool preferResidentKey,
		uint expected)
	{
		var actual = WindowsWebAuthn.GetMakeCredentialOptionsVersion(
			apiVersion,
			preferResidentKey);

		Assert.Equal(expected, actual);
	}

	[Theory]
	[InlineData(9u, 2u, 2u)]
	[InlineData(2u, 9u, 2u)]
	[InlineData(9u, 0u, 9u)]
	public void ApiVersionOverrideCanOnlyLowerVersion(uint nativeVersion, uint value, uint expected)
	{
		var actual = WindowsWebAuthn.ApplyApiVersionOverride(nativeVersion, value);

		Assert.Equal(expected, actual);
	}
}
