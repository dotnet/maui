using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.TestUtils.DeviceTests.Runners;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Gesture)]
	public class GestureTests : HandlerTestBase
	{
		[Fact]
		public async Task UserInteractionEnabledTrueWhenInitializedWithGestureRecognizer()
		{
			var label = new Label();
			label.GestureRecognizers.Add(new TapGestureRecognizer() { NumberOfTapsRequired = 1 });

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<LabelHandler>(label);
				Assert.True(handler.PlatformView.UserInteractionEnabled);
			});
		}
	}
}
