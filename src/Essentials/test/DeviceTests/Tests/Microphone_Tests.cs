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
		// If the status is already Allowed, OS will not re-prompt (in WinUI the same if prompted previously, there is not a reliable way to check. DeniedByUser even if not yet prompted).
		// Only first-run shows a consent prompt on Windows.
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
		// If the status is already Allowed, OS will not re-prompt (in WinUI the same if prompted previously, there is not a reliable way to check. DeniedByUser even if not yet prompted).
		// Only first-run shows a consent prompt on Windows.
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
}
