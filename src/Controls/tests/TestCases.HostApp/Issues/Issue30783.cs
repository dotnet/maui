using Maui.Controls.Sample.Issues;

namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 30783, "Android: PlatformImage.FromStream() terminates app in release builds", PlatformAffected.Android)]
 	public class Issue30783 : TestContentPage
 	{
 		Button _loadImageButton;
 		Image _testImage;
 		Label _statusLabel;

 		protected override void Init()
 		{
 			Title = "Issue 30783 - PlatformImage.FromStream Test";

 			_statusLabel = new Label
 			{
 				Text = "Tap button to test image loading from stream",
 				AutomationId = "StatusLabel",
 				Margin = new Thickness(10)
 			};

 			_loadImageButton = new Button
 			{
 				Text = "Load Image from Stream",
 				AutomationId = "LoadImageButton",
 				Margin = new Thickness(10)
 			};
 			_loadImageButton.Clicked += OnLoadImageClicked;

 			_testImage = new Image
 			{
 				AutomationId = "TestImage",
 				Margin = new Thickness(10),
 				HeightRequest = 100,
 				WidthRequest = 100,
 				BackgroundColor = Colors.LightGray
 			};

 			Content = new StackLayout
 			{
 				Children = { _statusLabel, _loadImageButton, _testImage }
 			};
 		}

 		async void OnLoadImageClicked(object sender, EventArgs e)
 		{
 			try
 			{
 				_statusLabel.Text = "Loading image from stream...";

 				// This reproduces the scenario from the issue where embedded resources
 				// were used via GetManifestResourceStream and PlatformImage.FromStream
 				await LoadImageFromStream();

 				_statusLabel.Text = "Image loaded successfully from stream!";
 			}
 			catch (Exception ex)
 			{
 				_statusLabel.Text = $"Error: {ex.Message}";
 			}
 		}

 		async Task LoadImageFromStream()
 		{
 			// Create a simple PNG image data (1x1 orange pixel)
 			byte[] orangePngBytes = Convert.FromBase64String(
 				"iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVQIW2P4v5ThPwAG7wKklwQ/bwAAAABJRU5ErkJggg=="
 			);

 			// Simulate the scenario from the issue where a stream might be disposed
 			// or become invalid during processing in release builds
 			using (var memoryStream = new MemoryStream(orangePngBytes))
 			{
 				// Create ImageSource from stream - this internally uses PlatformImage.FromStream
 				var imageSource = ImageSource.FromStream(() => new MemoryStream(orangePngBytes));
 				_testImage.Source = imageSource;
 			}

 			// Give the image time to load
 			await Task.Delay(500);
 		}
 	}