namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29297, "Crash on shell navigation for Mac Catalyst", PlatformAffected.macOS)]
public class Issue29297 : Shell
{
	public Issue29297()
	{
		CurrentItem = new Issue29297_MainPage();
	}

	public class Issue29297_MainPage : ContentPage
	{
		Label label;
		bool isNavigated;
		public Issue29297_MainPage()
		{
			var stack = new StackLayout();
			label = new Label();
			var Button = new Button();
			Button.Text = "Click to navigate new page";
			Button.AutomationId = "Button";
			Button.Clicked += Clicked;
			stack.Children.Add(Button);
			stack.Children.Add(label);
			this.Content = stack;
			this.Loaded += MainPage_Loaded;
		}

		private async void Clicked(object sender, EventArgs e)
		{
			isNavigated = true;
			await Navigation.PushAsync(new Issue29297_NewPage());
		}

		private void MainPage_Loaded(object sender, EventArgs e)
		{
			if (isNavigated)
			{
				label.Text = $"Successfully navigated back";
			}
		}
	}

	public class Issue29297_NewPage : ContentPage
	{
		public Issue29297_NewPage()
		{
			var stack = new StackLayout();
			var Button = new Button();
			Button.Text = "Click to navigate main page";
			Button.AutomationId = "Button";
			Button.Clicked += Clicked;
			stack.Children.Add(Button);
			this.Unloaded += NewPage_Unloaded;
			this.Content = stack;
		}

		private void Clicked(object sender, EventArgs e)
		{
			Shell.Current.GoToAsync("..");
		}

		private void NewPage_Unloaded(object sender, EventArgs e)
		{
			if (sender is not VisualElement senderElement)
				return;

			var visualTreeElement = (IVisualTreeElement)senderElement;

			Disconnect(visualTreeElement);

			return;

			void Disconnect(IVisualTreeElement element)
			{
				if (element is VisualElement visualElement)
				{

					foreach (IVisualTreeElement childElement in element.GetVisualChildren())
						Disconnect(childElement);

					visualElement.Handler?.DisconnectHandler();
				}
			}
		}
	}
}