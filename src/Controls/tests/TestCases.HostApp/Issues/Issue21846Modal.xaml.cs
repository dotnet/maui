namespace Maui.Controls.Sample.Issues
{
	public partial class Issue21846Modal : ContentPage
	{
		public Issue21846Modal()
		{
			InitializeComponent();

			BindingContext = this;
		}

		async void OnButtonClicked(object sender, System.EventArgs e)
		{
			if (TestWebView is IElement visualElement)
			{
				if (visualElement.Handler != null)
				{
					if (visualElement.Handler is IDisposable disposableHandler)
						disposableHandler.Dispose();

					visualElement.Handler.DisconnectHandler();
				}
			}

			await Navigation.PopModalAsync();
		}
	}
}
