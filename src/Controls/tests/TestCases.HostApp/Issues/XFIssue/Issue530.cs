using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 530, "ListView does not render if source is async", PlatformAffected.iOS)]
public class Issue530 : TestContentPage
{
	ListView _list;
	Button _button;

	protected override void Init()
	{
		_list = new ListView
		{
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, new Binding("."));
				label.SetBinding(Label.AutomationIdProperty, new Binding("."));
				return new ViewCell { View = label };
			})
		};

		_button = new Button
		{
			Text = "Load",
			AutomationId = "Load"
		};

		_button.Clicked += async (sender, e) =>
		{
			await Task.Delay(1000);
			_list.ItemsSource = new[] { "John", "Paul", "George", "Ringo" };
		};
		Content = new StackLayout
		{
			_button,
			_list,
		};
	}
}
