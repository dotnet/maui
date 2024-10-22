namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Bugzilla, 32615, "OnAppearing is not called on previous page when modal page is popped")]
	public class Bugzilla32615 : NavigationPage
	{
		public Bugzilla32615() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			int _counter;
			Label _textField;

			public MainPage()
			{
#pragma warning disable CS0618 // Type or member is obsolete
				var btnModal = new Button { AutomationId = "btnModal", Text = "open", HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };
#pragma warning restore CS0618 // Type or member is obsolete
				btnModal.Clicked += async (sender, e) => await Navigation.PushModalAsync(new Bugzilla32615Page2());
				_textField = new Label { AutomationId = "lblCount" };
				var layout = new StackLayout();
				layout.Children.Add(btnModal);
				layout.Children.Add(_textField);
				// Initialize ui here instead of ctor
				Content = layout;
			}

			protected override void OnAppearing()
			{
				_textField.Text = _counter++.ToString();
			}

			class Bugzilla32615Page2 : ContentPage
			{
				public Bugzilla32615Page2()
				{
#pragma warning disable CS0618 // Type or member is obsolete
					var btnPop = new Button { AutomationId = "btnPop", Text = "pop", HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };
#pragma warning restore CS0618 // Type or member is obsolete
					btnPop.Clicked += async (sender, e) => await Navigation.PopModalAsync();
					Content = btnPop;
				}

				protected override void OnDisappearing()
				{
					System.Diagnostics.Debug.WriteLine("Disappearing Modal");
					base.OnDisappearing();
				}

				protected override void OnAppearing()
				{
					System.Diagnostics.Debug.WriteLine("Appearing Modal");
					base.OnAppearing();
				}
			}
		}
	}
}