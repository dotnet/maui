using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery
{
	[Preserve(AllMembers = true)]
	public class MemoryLeakGallery : FlyoutPage
	{
		List<WeakReference> wref = new List<WeakReference>();

		Label result;

		Button checkResult;

		Button MakeButton(string text, Func<View> getContent)
		{
			return new Button
			{
				Text = text,
				Command = new Command(async () =>
				{
					View content = getContent();
					await Detail.Navigation.PushAsync(new ContentPage { Content = content });
					Detail.Navigation.RemovePage(Detail.Navigation.NavigationStack[0]);

					await Detail.Navigation.PushAsync(new ContentPage());
					var pageToRemove = Detail.Navigation.NavigationStack[0];
					Detail.Navigation.RemovePage(pageToRemove);

					wref.Add(new WeakReference(pageToRemove));
					wref.Add(new WeakReference(content));
					result.Text = string.Empty;
				})
			};
		}

		public MemoryLeakGallery()
		{
			result = new Label
			{
				FontSize = 16,
				Text = string.Empty
			};

			checkResult = new Button
			{
				Text = "Check Result",
				BackgroundColor = Color.DarkRed,
				TextColor = Color.White,
				Command = new Command(() =>
				{
					GC.Collect();
					GC.WaitForPendingFinalizers();
					GC.Collect();

					if (wref.Count < 4)
						return;

					if (wref[0].IsAlive || wref[1].IsAlive)
					{
						Title = result.Text = "Failed";
						result.TextColor = Color.Red;
					}
					else
					{
						Title = result.Text = "Success";
						result.TextColor = Color.DarkGreen;
					}
					wref.Clear();
				})
			};

			Flyout = new ContentPage
			{
				Title = "menu",
				Content = new ScrollView
				{
					Content = new StackLayout
					{
						Children =
						{
							checkResult,
							new Label { Text = "Click on button twice, then check result" },
							result,
							MakeButton("Empty page", () => null),
							MakeButton(nameof(ActivityIndicator), () => new ActivityIndicator { IsRunning = true }),
							MakeButton(nameof(BoxView), () => new BoxView { Color = Color.Azure }),
							MakeButton(nameof(Button), () => new Button { Text = "Button" }),
							MakeButton(nameof(DatePicker), () => new DatePicker ()),
							MakeButton(nameof(Label), () => new Label { Text = "Label" }),
							MakeButton(nameof(Entry), () => new Entry { Text = "Entry" }),
							MakeButton(nameof(Editor), () => new Editor { Text = "Editor" }),
							MakeButton(nameof(Image), () => new Image { BackgroundColor = Color.Azure, HeightRequest = 50 }),
							MakeButton(nameof(ImageButton), () => new ImageButton { BackgroundColor = Color.Azure, HeightRequest = 50 }),
							MakeButton(nameof(WebView), () => new WebView { BackgroundColor = Color.Azure, HeightRequest = 50 }),
							MakeButton(nameof(ProgressBar), () => new ProgressBar { BackgroundColor = Color.Azure, Progress = 0.5 }),
							MakeButton(nameof(Picker), () => new Picker { BackgroundColor = Color.Azure, HeightRequest = 50 }),
							MakeButton(nameof(OpenGLView), () => new OpenGLView ()),
							MakeButton(nameof(SearchBar), () => new SearchBar ()),
							MakeButton(nameof(Slider), () => new Slider ()),
							MakeButton(nameof(Stepper), () => new Stepper ()),
							MakeButton(nameof(Switch), () => new Switch ()),
							MakeButton(nameof(TimePicker), () => new TimePicker ()),
							MakeButton(nameof(TableView), () => new TableView ()),
						}
					}
				}
			};

			Detail = new NavigationPage(new ContentPage());
		}
	}
}