using System.ComponentModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 15357, "IsVisible binding not showing items again if Shadow is set", PlatformAffected.Android)]
	public partial class Issue15357 : ContentPage
	{
		private int clicks;
		private Issue15357Settings settings;

		public Issue15357()
		{
			settings = new Issue15357Settings();

			InitializeComponent();
			BindingContext = settings;
			SetButtonStatus();
		}

		private void OnButtonClicked(object sender, EventArgs e)
		{
			clicks++;
			settings.ShouldShow = clicks % 2 == 0;
			SetButtonStatus();
		}

		void SetButtonStatus()
		{
			var buttonStatus = ButtonTest.IsVisible ? "is visible" : "is not visible";

			settings.LabelStatus = $"Test Button {buttonStatus}";
		}
	}

	internal class Issue15357Settings : INotifyPropertyChanged
	{
		private bool shouldShow;
		private string labelStatus;

		public Issue15357Settings()
		{
			ShouldShow = true;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public bool ShouldShow
		{
			get => shouldShow;
			set
			{
				shouldShow = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShouldShow)));
			}
		}

		public string LabelStatus
		{
			get => labelStatus;
			set
			{
				labelStatus = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LabelStatus)));
			}
		}
	}
}