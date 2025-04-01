namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Bugzilla, 24574, "Tap Double Tap")]
	public class Issue24574 : TestContentPage
	{
		protected override void Init()
		{
			var label = new Label
			{
				AutomationId = "TapLabel",
				Text = "123",
				FontSize = 50
			};

			var rec = new TapGestureRecognizer { NumberOfTapsRequired = 1 };
			rec.Tapped += (s, e) => { label.Text = "Single"; };
			label.GestureRecognizers.Add(rec);

			rec = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
			rec.Tapped += (s, e) => { label.Text = "Double"; };
			label.GestureRecognizers.Add(rec);

			Content = label;
		}
	}
}