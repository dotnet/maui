using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ToolbarTests
	{
		[Fact(DisplayName = "Empty BadgeText Updates To Indicator Badge")]
		public Task EmptyBadgeTextUpdatesToIndicatorBadge()
		{
			if (!IsBarButtonItemBadgeSupported())
				return Task.CompletedTask;

			return InvokeOnMainThreadAsync(() =>
			{
				var item = new ToolbarItem
				{
					Text = "Badge",
					BadgeText = "3"
				};

				using var barButtonItem = item.ToUIBarButtonItem(forceName: true, forcePrimary: true);

				item.BadgeText = string.Empty;

#pragma warning disable CA1416 // Validate platform compatibility
				Assert.NotNull(barButtonItem.Badge);
				Assert.Null(barButtonItem.Badge.StringValue);
#pragma warning restore CA1416
			});
		}

		static bool IsBarButtonItemBadgeSupported()
		{
#if IOS
			return OperatingSystem.IsIOSVersionAtLeast(26);
#elif MACCATALYST
			return OperatingSystem.IsMacCatalystVersionAtLeast(26);
#else
			return false;
#endif
		}
	}
}
