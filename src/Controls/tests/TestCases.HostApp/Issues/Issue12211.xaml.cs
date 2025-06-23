using System.ComponentModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 12211, "[Android] BoxView Opacity not working", PlatformAffected.Android)]
	public partial class Issue12211 : ContentPage
	{
		private int clicks;
		private Issue12211Settings settings;

		public Issue12211()
		{
			settings = new Issue12211Settings();

			InitializeComponent();
			BindingContext = settings;
			SetCurrentOpacity(0.7);
		}

		private void OnChangedOpacity(object sender, EventArgs e)
		{
			clicks++;

			double opacity = clicks % 2 == 0 ? 0 : 1;

			SetCurrentOpacity(opacity);
		}

		void SetCurrentOpacity(double opacity)
		{
			settings.CurrentOpacity = (float)opacity;
			settings.CurrentOpacityStatus = $"Current opacity is {settings.CurrentOpacity}";
		}
	}

	internal class Issue12211Settings : INotifyPropertyChanged
	{
		private float currentOpacity;
		private string currentOpacityStatus;

		public event PropertyChangedEventHandler PropertyChanged;

		public float CurrentOpacity
		{
			get => currentOpacity;
			set
			{
				currentOpacity = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentOpacity)));
			}
		}

		public string CurrentOpacityStatus
		{
			get => currentOpacityStatus;
			set
			{
				currentOpacityStatus = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentOpacityStatus)));
			}
		}
	}
}