namespace Maui.Controls.Sample.Pages
{
	using System;
	using Microsoft.Maui.Controls;

	public partial class WindowTitleBar
	{
		public WindowTitleBar()
		{
			InitializeComponent();
		}

		public async void OnPushModalClicked(object? sender, EventArgs args)
		{
			await Navigation.PushModalAsync(new ContentPage()
			{
				Content = new VerticalStackLayout()
				{
					new Label() { Text = "Title Bar still look and work ok?"},
					new Button()
					{
						Text = "Pop Modal",
						Command = new Command(async () =>
						{
							await Navigation.PopModalAsync();
						})
					}
				}
			});
		}
	}
}