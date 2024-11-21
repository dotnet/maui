using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.DeviceTests.Stubs;
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

					handlers.AddHandler<Grid, LayoutHandler>();
					handlers.AddHandler<NavigationPage, NavigationViewHandler>();
					handlers.AddHandler<Button, ButtonHandler>();
					handlers.AddHandler(typeof(Toolbar), typeof(ToolbarHandler));
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

		[Fact("Batata")]
		public async Task DoesNotLeak()
		{
			SetupBuilder();

			// Long-lived ICommand, like a Singleton ViewModel
			var command = new MyCommand();
			var c2 = new MyCommand();
			WeakReference reference = null;
			var navPage = new NavigationPage(new ContentPage());

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async window =>
			{
				var layout = new Grid();
				//var refreshView = new RefreshView
				//{
				//	Command = command,
				//};

				var btn = new Button
				{
					Command = command
				};

				var label = new Label();
				//refreshView.Content = label;
				layout.Add(btn);
				//var handler = CreateHandler<LayoutHandler>(layout);
				//((window.VirtualView as Window).Page as ContentPage).Content = layout;

				var page2 = new ContentPage
				{
					Content = layout
				};

				reference = new(btn);
				await navPage.PushAsync(page2);
				await OnLoadedAsync(btn);
				btn.Command = c2;
				await navPage.PopAsync();
				await OnUnloadedAsync(page2);
				_ = 1; 
			});



			//await InvokeOnMainThreadAsync(async () =>
			//{
			//	var layout = new Grid();
			//	var refreshView = new RefreshView
			//	{
			//		Command = command,
			//	};
			//	var label = new Label();
			//	refreshView.Content = label;
			//	layout.Add(refreshView);
			//	var handler = CreateHandler<LayoutHandler>(layout);
			//	await OnLoadedAsync(refreshView);
			//	reference = new(refreshView);
			//});

			Assert.NotNull(reference);

			// Several GCs required on iOS
			await AssertionExtensions.WaitForGC(reference);
		}

		[Fact("Batata 2")]
		public async Task MultiCommandsShouldNotLeak()
		{
			SetupBuilder();

			// Long-lived ICommand, like a Singleton ViewModel
			var command = new MyCommand();
			var command2 = new MyCommand();
			WeakReference reference = null;
			var navPage = new NavigationPage(new ContentPage());

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async window =>
			{
				var layout = new Grid();
				var refreshView = new RefreshView
				{
					Command = command,
				};
				var label = new Label();
				refreshView.Content = label;
				layout.Add(refreshView);

				var page2 = new ContentPage
				{
					Content = refreshView
				};

				reference = new(refreshView);
				await navPage.PushAsync(page2);
				await OnLoadedAsync(refreshView);
				refreshView.Command = command2;
				await navPage.PopAsync();
				await OnUnloadedAsync(page2);
				_ = 1; 
			});

		}

		class MyCommand : ICommand
		{
			public event EventHandler CanExecuteChanged;

			public bool CanExecute(object parameter) => true;

			public void Execute(object parameter) { }

			public void FireCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}
