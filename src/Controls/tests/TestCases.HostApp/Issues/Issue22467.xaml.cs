namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 22467, "CollectionView SelectedItem Background Reset After Modal Navigation", PlatformAffected.iOS)]
	public partial class Issue22467 : ContentPage
	{
		public Issue22467()
		{
			InitializeComponent();
			List<string> list = new() { "1", "2", "3", "4", "5" };
			ResultCollectionView.ItemsSource = list;
			ResultCollectionView.SelectedItem = list[3];
		}

		private async void CounterBtn_Clicked(object sender, EventArgs e)
		{
			await Navigation.PushModalAsync(CreateNewPage1());
		}

		private ContentPage CreateNewPage1()
		{
			var newPage = new ContentPage
			{
				Title = "NewPage1",
				Content = new VerticalStackLayout
				{
					Children =
					{
						new Button
						{
							Text = "Pop to Main Page",
							AutomationId = "PopModalAsyncButton",
							Command = new Command(async () =>
							{
								await Navigation.PopModalAsync(true);
							})
						}
					}
				}
			};

			return newPage;
		}
	}
}