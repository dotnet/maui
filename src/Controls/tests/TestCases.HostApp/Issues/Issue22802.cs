using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 22802, "TemplatedView applies Background to root view", PlatformAffected.All)]
public class Issue22802 : ContentPage
{
	CustomTemplatedView22802 _customView;
	Label _statusLabel;

	public Issue22802()
	{
		var layout = new VerticalStackLayout
		{
			Spacing = 20
		};

		var instructionLabel = new Label
		{
			AutomationId = "InstructionLabel",
			Text = "The custom view below has a red Background. The background should ONLY appear in the ellipse, not as a red square behind it."
		};
		layout.Add(instructionLabel);

		_customView = new CustomTemplatedView22802
		{
			AutomationId = "CustomTemplatedView",
			WidthRequest = 200,
			HeightRequest = 200,
			HorizontalOptions = LayoutOptions.Center
		};
		layout.Add(_customView);

		var changeBackgroundButton = new Button
		{
			AutomationId = "ChangeBackgroundButton",
			Text = "Change Background to Blue"
		};
		changeBackgroundButton.Clicked += OnChangeBackgroundClicked;
		layout.Add(changeBackgroundButton);

		_statusLabel = new Label
		{
			AutomationId = "StatusLabel",
			Text = "Initial state",
			HorizontalOptions = LayoutOptions.Center
		};
		layout.Add(_statusLabel);

		Content = layout;
	}

	void OnChangeBackgroundClicked(object sender, EventArgs e)
	{
		_customView.Background = new SolidColorBrush(Colors.Blue);
		_statusLabel.Text = "Background changed to blue";
	}
}

public class CustomTemplatedView22802 : TemplatedView
{
	static readonly ControlTemplate DefaultControlTemplate;

	static CustomTemplatedView22802()
	{
		DefaultControlTemplate = new ControlTemplate(BuildDefaultTemplate);
	}

	static object BuildDefaultTemplate()
	{
		Grid layoutRoot = new Grid
		{
			AutomationId = "TemplateRootGrid"
		};

		Ellipse ellipse = new Ellipse
		{
			AutomationId = "TemplateEllipse",
			StrokeThickness = 1,
			Stroke = new SolidColorBrush(Colors.Black)
		};

		ellipse.SetBinding(Ellipse.FillProperty, new Binding(nameof(Background), source: RelativeBindingSource.TemplatedParent));

		layoutRoot.Children.Add(ellipse);

		return layoutRoot;
	}

	public CustomTemplatedView22802()
	{
		Background = new SolidColorBrush(Colors.Red);
		ControlTemplate = DefaultControlTemplate;
	}
}
