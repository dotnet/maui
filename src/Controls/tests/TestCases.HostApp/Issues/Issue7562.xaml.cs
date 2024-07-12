using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 7562, "MeasureFirstItem makes items disappear on Android", PlatformAffected.Android)]
	public partial class Issue7562 : ContentPage
	{
		public ObservableCollection<Issue> Issues { get; } = new ObservableCollection<Issue>();

		public Issue7562()
		{
			InitializeComponent();
			BindingContext = this;
			this.Loaded += OnLoaded;
		}

		async void OnLoaded(object sender, System.EventArgs e)
		{
			await Task.Yield();
			Issues[0].MakeGreen();
			Issues[1].MakeGreen();
			this.Loaded -= OnLoaded;
		}

		public class Issue : BindableObject
		{
			public string Title { get; set; }
			public bool IsRed { get; set; } = true;
			public bool IsGreen => !IsRed;

			public void MakeGreen()
			{
				IsRed = false;
				OnPropertyChanged(nameof(IsGreen));
				OnPropertyChanged(nameof(IsRed));
			}
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			Issues.Add(new Issue { Title = "Issue1" });
			Issues.Add(new Issue { Title = "Issue2" });
			Issues.Add(new Issue { Title = "Issue3" });
		}
	}
}