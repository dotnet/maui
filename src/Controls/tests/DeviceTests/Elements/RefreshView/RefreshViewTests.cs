using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
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
					handlers.AddHandler<Label, LabelHandler>();
					handlers.AddHandler<Entry, EntryHandler>();
				});
			});
		}
		[Fact(DisplayName = "Setting the content of RefreshView removes previous platform view from visual tree")]
		public async Task ChangingRefreshViewContentRemovesPreviousContentsPlatformViewFromVisualTree()
		{
			SetupBuilder();

			var refreshView = new RefreshView();
			var initialContent = new Label();
			refreshView.Content = initialContent;

			await AttachAndRun(refreshView, async (handler) =>
			{
				var newContent = new Entry();
				Assert.NotNull(((initialContent as IView).Handler as IPlatformViewHandler).PlatformView.GetParent());
				refreshView.Content = newContent;
				Assert.Null(((initialContent as IView).Handler as IPlatformViewHandler).PlatformView.GetParent());
				await Task.Yield();
				Assert.NotNull(((newContent as IView).Handler as IPlatformViewHandler).PlatformView.GetParent());
			});
		}
	}
}
