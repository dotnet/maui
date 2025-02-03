using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 27418, "CarouselView Rendering Issue with PeekAreaInsets on Android Starting from .NET MAUI 9.0.21", PlatformAffected.Android)]
	public partial class Issue27418 : ContentPage
	{
		public Issue27418()
		{
			InitializeComponent();
			BindingContext = new Issue27418ViewModel();
		}
	}

	public class Issue27418Model
	{
		public string LabelText { get; set; }
		public string ButtonText { get; set; }
	}

	public class Issue27418ViewModel
	{
		public ObservableCollection<Issue27418Model> CarouselItems { get; set; }

		public Issue27418ViewModel()
		{
			CarouselItems = new ObservableCollection<Issue27418Model>
		{
			new Issue27418Model { LabelText = "Page1",ButtonText = "Button1" },
			new Issue27418Model { LabelText = "Page2",ButtonText = "Button2" },
			new Issue27418Model { LabelText = "Page3",ButtonText = "Button3" },
			new Issue27418Model { LabelText = "Page4",ButtonText = "Button4" },
			new Issue27418Model { LabelText = "Page5",ButtonText = "Button5" },
			new Issue27418Model { LabelText = "Page6",ButtonText = "Button6" },
		};
		}
	}
}