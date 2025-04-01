﻿namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 2728, "[macOS] Label FontAttributes Italic is not working")]
	public class Issue2728 : TestContentPage
	{
		const string _lblHome = "Hello Label";

		protected override void Init()
		{
			var label = new Label { Text = _lblHome, FontAttributes = FontAttributes.Italic };

			Content = new StackLayout
			{
				Children = {
					label
				}
			};
		}
	}
}
