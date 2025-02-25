namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 21648, "KeyboardScrolling is not leaking when window becomes null", PlatformAffected.iOS)]
	public partial class Issue21648 : ContentPage
	{
		WeakReference handlerReference;
		WeakReference platformViewReference;
		// int count;

		public Issue21648()
		{
			InitializeComponent();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			var entryHandler = entry1.Handler;
			handlerReference = new WeakReference(entryHandler);
			platformViewReference = new WeakReference(entryHandler.PlatformView);
		}

		private async void Entry_Unfocused(object sender, FocusEventArgs e)
		{

			try
			{
				await Task.Delay(2000);

				await GarbageCollectionHelper.WaitForGC(5000, handlerReference, platformViewReference);
			}
			catch
			{

			}
			finally
			{

			}
			
		}

		private void Entry_Focused(object sender, FocusEventArgs e)
		{
		}

		private void Button_Pressed(object sender, EventArgs e)
		{
#if IOS
			var button = (Button)sender;
			var platButton = button.Handler.PlatformView;
			if (platButton is UIKit.UIButton uiButton)
			{
				var window = uiButton.Window;
				window?.Dispose();
			}
#endif
		}

		private void entry1_Unloaded(object sender, EventArgs e)
		{
		}
	}
}