namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 38731, "iOS.NavigationRenderer.GetAppearedOrDisappearedTask NullReferenceExceptionObject", PlatformAffected.iOS)]
public class Bugzilla38731 : TestNavigationPage
{
	protected override void Init()
	{
		Navigation.PushAsync(new PageOne());
	}

	public class PageOne : ContentPage
	{
		public PageOne()
		{
			var label = new Label();
			label.Text = "Page one...";
			label.HorizontalTextAlignment = TextAlignment.Center;

			var button = new Button();
			button.AutomationId = "btn1";
			button.Text = "Navigate to page two";
			button.Clicked += Button_Clicked;

			var content = new StackLayout();
			content.Children.Add(label);
			content.Children.Add(button);

			Title = "Page one";
			Content = content;
		}
		void Button_Clicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new PageTwo());
		}
	}

	public class PageTwo : ContentPage
	{
		public PageTwo()
		{
			var label = new Label();
			label.Text = "Page two...";
			label.HorizontalTextAlignment = TextAlignment.Center;

			var button = new Button();
			button.AutomationId = "btn2";
			button.Text = "Navigate to page three";
			button.Clicked += Button_Clicked;

			var content = new StackLayout();
			content.Children.Add(label);
			content.Children.Add(button);

			Title = "Page two";
			Content = content;
		}

		void Button_Clicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new PageThree());
		}
	}

	public class PageThree : ContentPage
	{
		public PageThree()
		{
			var label = new Label();
			label.Text = "Page three...";
			label.HorizontalTextAlignment = TextAlignment.Center;

			var button = new Button();
			button.AutomationId = "btn3";
			button.Text = "Navigate to page four";
			button.Clicked += Button_Clicked;

			var content = new StackLayout();
			content.Children.Add(label);
			content.Children.Add(button);

			Title = "Page three";
			Content = content;
		}

		void Button_Clicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new PageFour());
		}
	}

	public class PageFour : ContentPage
	{
		public PageFour()
		{
			var label = new Label();
			label.Text = "Last page... Tap back very quick";
			label.AutomationId = "FinalPage";
			label.HorizontalTextAlignment = TextAlignment.Center;

			var content = new StackLayout();
			content.Children.Add(label);

			Title = "Page four";
			Content = content;
		}
	}
}