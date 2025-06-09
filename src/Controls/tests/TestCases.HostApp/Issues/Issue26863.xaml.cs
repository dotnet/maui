using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 26863, "CarouselView with CarouselViewHandler2 make app crash", PlatformAffected.iOS)]
	public partial class Issue26863 : ContentPage
	{
		public Issue26863()
		{
			InitializeComponent();
		}

		public ObservableCollection<string> List { get; } = new();
		public Command ScrollToCommand => new(() => CV.ScrollTo(2, 0, animate: true));

		protected override void OnAppearing()
		{
			List.Clear();
			List.Add("item1");
			List.Add("item2");
			List.Add("item3");
			button.IsVisible = true;
		}
	}
}