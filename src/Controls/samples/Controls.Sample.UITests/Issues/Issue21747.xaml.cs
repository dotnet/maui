using System.Diagnostics;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 21747, "TemplateBinding with CornerRadius on Border does not work anymore", PlatformAffected.All)]
	public partial class Issue21747 : ContentPage
	{
		public Issue21747()
		{
			InitializeComponent();
		}
	}

	public class MyButton : TemplatedView
	{
		public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(
			nameof(CornerRadius), typeof(CornerRadius), typeof(MyButton), new CornerRadius(6.0),
			propertyChanged: (bindable, oldValue, newValue) =>
			{
				Debug.WriteLine($"CornerRadius changed from '{oldValue}' to '{newValue}'");
			});

		public static readonly BindableProperty TextProperty = BindableProperty.Create(
			nameof(Text), typeof(string), typeof(MyButton), "Default",
			propertyChanged: (bindable, oldValue, newValue) =>
			{
				Debug.WriteLine($"Text changed from '{oldValue}' to '{newValue}'");
			});

		public CornerRadius CornerRadius
		{
			get => (CornerRadius)GetValue(CornerRadiusProperty);
			set => SetValue(CornerRadiusProperty, value);
		}

		public string Text
		{
			get => (string)GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}
	}
}