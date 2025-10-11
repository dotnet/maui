using System.Windows.Input;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 30010, "Loading the captured screenshot from webview content to Image control does not visible", PlatformAffected.Android)]
	public partial class Issue30010 : TestContentPage
	{
		public Issue30010()
		{
			InitializeComponent();
		}

		protected override void Init()
		{
			BindingContext = new Issue30010ViewModel();
		}
	}
	
	class Issue30010ViewModel : BindableObject
	{
		ImageSource _screenshot;

		public Issue30010ViewModel()
		{
			ScreenshotCommand = new Command(async () => await CaptureScreenshot(), () => Screenshot.IsCaptureSupported);
		}

		public ICommand ScreenshotCommand { get; }
		
		public ImageSource Image
		{
			get => _screenshot;
			set
			{
				_screenshot = value;
				OnPropertyChanged();
			}
		}

		async Task CaptureScreenshot()
		{
			var mediaFile = await Screenshot.CaptureAsync();
			var stream = await mediaFile.OpenReadAsync(ScreenshotFormat.Png);

			Image = ImageSource.FromStream(() => stream);
		}
	}
}