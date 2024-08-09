using System.Collections.Generic;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8870, "[Bug] CollectionView with HTML Labels Freeze the Screen on Rotation",
		PlatformAffected.iOS)]
	public class Issue8870 : TestContentPage
	{
		public const string Success = "Success";
		public const string CheckResult = "Check";

		protected override void Init()
		{
			var instructions = new Label { Text = "Rotate the device, then rotate it back 3 times. If the application crashes or hangs, this test has failed." };

			var button = new Button { Text = CheckResult, AutomationId = CheckResult };
			button.Clicked += (sender, args) => { instructions.Text = Success; };

			var source = new List<string>();
			for (int n = 0; n < 100; n++)
			{
				source.Add($"Item: {n}");
			}

			var template = new DataTemplate(() =>
			{
				var label = new Label
				{
					TextType = TextType.Html
				};

				label.SetBinding(Label.TextProperty, new Binding(".", stringFormat: "<p style='background-color:red;'>{0}</p>"));

				return label;
			});

			var cv = new CollectionView()
			{
				ItemsSource = source,
				ItemTemplate = template
			};

			var layout = new StackLayout();

			layout.Children.Add(instructions);
			layout.Children.Add(button);
			layout.Children.Add(cv);

			Content = layout;
		}
	}
}
