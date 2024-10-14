using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues;

[Preserve(AllMembers = true)]
[Issue(IssueTracker.Bugzilla, 60122, "LongClick on image not working", PlatformAffected.Android)]
public class Bugzilla60122 : TestContentPage
{
	const string ImageId = "60122Image";
	const string Success = "Success";

	protected override void Init()
	{
		var customImage = new _60122Image
		{
			AutomationId = ImageId,
			Source = "coffee.png"
		};

		var instructions = new Label
		{
			Text = $"Long press the image below; the label below it should change to read {Success}"
		};

		var result = new Label { Text = "Testing..." };

		customImage.LongPress += (sender, args) => { result.Text = Success; };

		Content = new StackLayout
		{
			Children = { instructions, customImage, result }
		};
	}

	public class _60122Image : Image
	{
		public event EventHandler LongPress;

		public void HandleLongPress(object sender, EventArgs e)
		{
			LongPress?.Invoke(this, new EventArgs());
		}
	}
}