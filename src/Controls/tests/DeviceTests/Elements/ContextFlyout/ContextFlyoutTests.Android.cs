using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ContextFlyoutTests
	{
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task ContextFlyoutRestoresLongClickable(bool originalLongClickable)
		{
			SetupBuilder();

			await InvokeOnMainThreadAsync(() =>
			{
				var label = new Label();
				var handler = CreateHandler<LabelHandler>(label);
				var platformView = handler.PlatformView;
				platformView.LongClickable = originalLongClickable;

				FlyoutBase.SetContextFlyout(label, new MenuFlyout());
				Assert.True(platformView.LongClickable);

				FlyoutBase.SetContextFlyout(label, new MenuFlyout());
				Assert.True(platformView.LongClickable);

				FlyoutBase.SetContextFlyout(label, null);
				Assert.Equal(originalLongClickable, platformView.LongClickable);

				FlyoutBase.SetContextFlyout(label, new MenuFlyout());
				((IElementHandler)handler).DisconnectHandler();
				Assert.Equal(originalLongClickable, platformView.LongClickable);
			});
		}
	}
}
