namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 23897, "Add a permanent wrapper around ImageButton so it works better with loading and unloading", PlatformAffected.iOS)]
	public class Issue23897 : NavigationPage
	{
		
		public Issue23897() : base(new TestPage())
		{
		}
		
		public class TestPage : ContentPage
		{
            Label _labelLoadedCount;
            Label _labelUnloadedCount;
            ImageButton _imageButton;
			private int _loadedCount;
			private int _unloadedCount;

			public TestPage()
			{
                _imageButton  = new ImageButton()
                {
                    Source = "coffee.png",
                    AutomationId = "ImageButton",
                    HeightRequest = 100,
                    WidthRequest = 100
                };

                _imageButton.Loaded += (s, e) =>
                {
                    _labelLoadedCount.Text = $"{++_loadedCount}";
                };

                _imageButton.Unloaded += (s, e) =>
                {
                    _labelUnloadedCount.Text = $"{++_unloadedCount}";
                };

                _labelUnloadedCount = new Label()
                {
                    AutomationId = "UnloadedCount",
                    Text = "0"
                };

                _labelLoadedCount = new Label()
                {
                    AutomationId = "LoadedCount",
                    Text = "0"
                };
                
				Content = new VerticalStackLayout()
				{
                    new Label() 
                    {
                        Text = "ImageButton Loaded Count:",
                    },
                    _labelLoadedCount,
                    new Label() 
                    {
                        Text = "ImageButton Unloaded Count:",
                    },
                    _labelUnloadedCount,
                    _imageButton,
					new Button()
					{
						Text = "Push And Pop Page to Validate ImageButton Loading/Unloaded",
						AutomationId = "PushAndPopPage",
                        LineBreakMode = LineBreakMode.WordWrap,
						Command = new Command(async () =>
						{
							await Navigation.PushAsync(new ContentPage(), false);
                            await Task.Yield();
                            await Navigation.PopAsync(true);
						})
					}
				};
			}
		}
	}
}
