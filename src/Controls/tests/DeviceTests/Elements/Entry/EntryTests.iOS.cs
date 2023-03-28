using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class EntryTests
	{
		UITextField GetPlatformControl(EntryHandler handler) =>
			(UITextField)handler.PlatformView;

		Task<string> GetPlatformText(EntryHandler handler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformControl(handler).Text);
		}

		void SetPlatformText(EntryHandler entryHandler, string text) =>
			GetPlatformControl(entryHandler).Text = text;

		int GetPlatformCursorPosition(EntryHandler entryHandler)
		{
			var textField = GetPlatformControl(entryHandler);

			if (textField != null && textField.SelectedTextRange != null)
				return (int)textField.GetOffsetFromPosition(textField.BeginningOfDocument, textField.SelectedTextRange.Start);

			return -1;
		}

		int GetPlatformSelectionLength(EntryHandler entryHandler)
		{
			var textField = GetPlatformControl(entryHandler);

			if (textField != null && textField.SelectedTextRange != null)
				return (int)textField.GetOffsetFromPosition(textField.SelectedTextRange.Start, textField.SelectedTextRange.End);

			return -1;
		}

		[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
		public class ScrollTests : ControlsHandlerTestBase
		{
			[Fact]
			public async Task ScrollSkipsAlert()
			{
				var entry = new Entry();

				EnsureHandlerCreated(builder =>
				{
					builder.ConfigureMauiHandlers(handler =>
					{
						handler.AddHandler<VerticalStackLayout, LayoutHandler>();
						handler.AddHandler<Entry, EntryHandler>();
					});
				});

				var layout = new VerticalStackLayout
			{
				entry
			};

				layout.WidthRequest = 300;
				layout.HeightRequest = 800;

				await InvokeOnMainThreadAsync(async () =>
				{
					var contentViewHandler = CreateHandler<LayoutHandler>(layout);
					await contentViewHandler.PlatformView.AttachAndRun(async () =>
					{
						var entryBoxFunc = entry.GetBoundingBox;
						var initialEntryBox = entryBoxFunc.Invoke();
						var layoutPlat = layout.ToPlatform();
						var win = layoutPlat.Window;
						if (win is UIWindow uIWindow)
						{
							var alert = UIAlertController.Create("Popup Alert", "This is a popup", UIAlertControllerStyle.Alert);

							alert.AddTextField((textfield) =>
							{
								textfield.Placeholder = "Placeholder Text";
							});

							await uIWindow.RootViewController.PresentViewControllerAsync(alert, true);

							var finalEntryBoxFunc = entry.GetBoundingBox;
							var finalEntryBox = finalEntryBoxFunc.Invoke();

							var taskCompletion = new TaskCompletionSource<bool>();

							uIWindow.RootViewController.DismissViewController(true, () => { taskCompletion.SetResult(true); });

							await taskCompletion.Task;

							Assert.True(initialEntryBox == finalEntryBox);
							Assert.False(KeyboardAutoManagerScroll.IsKeyboardAutoScrollHandling);
						}
					});
				});
			}
		}
	}
}
