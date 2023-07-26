using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.RefreshView)]
	public partial class RefreshViewTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<RefreshView, RefreshViewHandler>();
				});
			});
		}

		[Fact(DisplayName = "Does Not Leak")]
		public async Task DoesNotLeak()
		{
			SetupBuilder();

			WeakReference viewReference = null;
			WeakReference platformViewReference = null;
			WeakReference handlerReference = null;

			await InvokeOnMainThreadAsync(() =>
			{
				var layout = new Grid();
				var refreshView = new RefreshView();
				layout.Add(refreshView);
				var handler = CreateHandler<LayoutHandler>(layout);
				viewReference = new WeakReference(refreshView);
				handlerReference = new WeakReference(refreshView.Handler);
				platformViewReference = new WeakReference(refreshView.Handler.PlatformView);
			});

			await AssertionExtensions.WaitForGC(viewReference, handlerReference, platformViewReference);
			Assert.False(viewReference.IsAlive, "RefreshView should not be alive!");
			Assert.False(handlerReference.IsAlive, "Handler should not be alive!");
			Assert.False(platformViewReference.IsAlive, "PlatformView should not be alive!");
		}
	}
}

