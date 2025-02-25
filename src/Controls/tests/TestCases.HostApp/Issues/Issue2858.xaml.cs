namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 2858, "Transparency Cascading", PlatformAffected.Android)]
	public partial class Issue2858 : TestContentPage
	{
		const string Success = "Success";
		const string Failure = "Fail";
		const string InnerGrid = "InnerGrid";
		const string OuterGrid = "OuterGrid";

#pragma warning disable CS0414
		int _tapCount = 0;
#pragma warning restore CS0414

		public Issue2858()
		{
			InitializeComponent();
		}
		void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
		{
			_tapCount += 1;

			if (_tapCount == 1)
			{
				Result.Text = Success;
			}
			else
			{
				Result.Text = Failure;
			}
		}

		protected override void Init()
		{

		}
	}
}