using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests;

[Category("Permissions")]
public class Microphone_Tests
{
	[Fact]
	[Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
	public async Task Request_Microphone_Denied_Returns_Denied()
	{
		// If microphone permission was previously granted, the OS won't show a new prompt.
		// In that case we can't exercise the prompt flow, so skip the test.
		var initial = await Permissions.CheckStatusAsync<Permissions.Microphone>();
		if (initial == PermissionStatus.Granted)
			return; // nothing to prompt; skip

		// This test requires human interaction: When prompted, choose Deny for microphone access
		var status = await MainThread.InvokeOnMainThreadAsync(Permissions.RequestAsync<Permissions.Microphone>);
		Assert.Equal(PermissionStatus.Denied, status);

		// And subsequent checks should reflect Denied
		var check = await Permissions.CheckStatusAsync<Permissions.Microphone>();
		Assert.Equal(PermissionStatus.Denied, check);
	}

	[Fact]
	[Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
	public async Task Request_Microphone_Allowed_Returns_Granted()
	{
		// If microphone permission was previously granted, the OS won't show a new prompt.
		// In that case we can't exercise the prompt flow, so skip the test.
		var initial = await Permissions.CheckStatusAsync<Permissions.Microphone>();
		if (initial == PermissionStatus.Granted)
			return; // nothing to prompt; skip

		// This test requires human interaction: When prompted, choose Allow for microphone access
		var status = await MainThread.InvokeOnMainThreadAsync(Permissions.RequestAsync<Permissions.Microphone>);
		Assert.Equal(PermissionStatus.Granted, status);

		// And subsequent checks should reflect Granted
		var check = await Permissions.CheckStatusAsync<Permissions.Microphone>();
		Assert.Equal(PermissionStatus.Granted, check);
	}

	[Fact]
	[Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
	public async Task Request_Microphone_Granted_Then_RequestAgain_Returns_Granted()
	{
		// Ensure we have the Granted state first (may require human interaction on first request)
		var status = await Permissions.CheckStatusAsync<Permissions.Microphone>();
		if (status != PermissionStatus.Granted)
		{
			// Human: choose Allow when prompted
			status = await MainThread.InvokeOnMainThreadAsync(Permissions.RequestAsync<Permissions.Microphone>);
			if (status != PermissionStatus.Granted)
				return; // Can't proceed with this test if user denied; treat as skip
		}

		// Second request should short-circuit and still return Granted without a new prompt
		var second = await MainThread.InvokeOnMainThreadAsync(Permissions.RequestAsync<Permissions.Microphone>);
		Assert.Equal(PermissionStatus.Granted, second);

		// Status check remains Granted
		var check = await Permissions.CheckStatusAsync<Permissions.Microphone>();
		Assert.Equal(PermissionStatus.Granted, check);
	}

	[Fact]
	[Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
	public async Task Request_Microphone_Denied_Then_RequestAgain_Returns_Denied()
	{
		// If already granted we can't meaningfully test denied flow again without application/OS settings reset
		var initial = await Permissions.CheckStatusAsync<Permissions.Microphone>();
		if (initial == PermissionStatus.Granted)
			return; // skip

		// First request: Human choose Deny when prompted (if prompt appears)
		var first = await MainThread.InvokeOnMainThreadAsync(Permissions.RequestAsync<Permissions.Microphone>);
		if (first != PermissionStatus.Denied)
			return; // can't assert repeat deny path if user allowed or unknown

		// Second request should return Denied again (idempotent) and not elevate permission
		var second = await MainThread.InvokeOnMainThreadAsync(Permissions.RequestAsync<Permissions.Microphone>);
		Assert.Equal(PermissionStatus.Denied, second);

		var check = await Permissions.CheckStatusAsync<Permissions.Microphone>();
		Assert.Equal(PermissionStatus.Denied, check);
	}
}
