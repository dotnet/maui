#if WINDOWS
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using WButton = Microsoft.UI.Xaml.Controls.Button;
using WImage = Microsoft.UI.Xaml.Controls.Image;

namespace Maui.Controls.Sample;

public partial class ImageButtonControlMainPage
{
	readonly Label _aspectResult = new() { AutomationId = "AspectCaptureIdle" };
	readonly string _aspectDirectory = Path.Combine(FileSystem.CacheDirectory, "Issue29959");

	void ConfigureAspectVerification()
	{
		if (Content is not Grid grid)
			throw new InvalidOperationException("The ImageButton verification grid is missing.");

		grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
		grid.Add(_aspectResult, 0, 5);
		grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
		grid.Add(new Label
		{
			AutomationId = "AspectEvidenceDirectory",
			Text = _aspectDirectory,
			FontSize = 10
		}, 0, 6);

		var capture = new ToolbarItem { Text = "Capture pixels", AutomationId = "CaptureAspectPixels" };
		capture.Clicked += CaptureAspectPixels;
		ToolbarItems.Add(capture);
	}

	async void CaptureAspectPixels(object? sender, EventArgs e)
	{
		_aspectResult.AutomationId = "AspectCaptureRunning";
		_aspectResult.Text = "RUNNING";
		Directory.CreateDirectory(_aspectDirectory);
		File.Delete(Path.Combine(_aspectDirectory, "capture-error.txt"));
		try
		{
			var deadline = DateTime.UtcNow.AddSeconds(20);
			while (TestImageButton.IsLoading && DateTime.UtcNow < deadline)
				await Task.Delay(100);

			if (TestImageButton.IsLoading)
				throw new TimeoutException("The selected image did not finish loading.");
			if (TestImageButton.Handler?.PlatformView is not WButton button || button.Content is not WImage image)
				throw new InvalidOperationException("The Windows ImageButton or its image content is unavailable.");
			if (image.Source is null || image.ActualWidth <= 0 || image.ActualHeight <= 0)
				throw new InvalidOperationException("The native image has no loaded source or positive layout size.");
			if (image.Source is BitmapSource bitmap && (bitmap.PixelWidth <= 0 || bitmap.PixelHeight <= 0))
				throw new InvalidOperationException("The bitmap has not decoded.");

			await Task.Delay(500);
			if (TestImageButton.Handler?.PlatformView != button)
				throw new OperationCanceledException("The ImageButton handler changed during capture.");

			var source = _viewModel.Source switch
			{
				FileImageSource => "File",
				UriImageSource => "Uri",
				StreamImageSource => "Stream",
				FontImageSource => "FontImage",
				_ => throw new InvalidOperationException("Unexpected image-source type.")
			};
			var name = $"{source}_{_viewModel.Aspect}";
			var buttonPixels = await Capture(button, name + "_button.png");
			var imagePixels = await Capture(image, name + "_image.png");
			var origin = image.TransformToVisual(button).TransformPoint(new Windows.Foundation.Point());
			var decoded = image.Source as BitmapSource;
			var evidence = new
			{
				Source = source,
				Aspect = _viewModel.Aspect.ToString(),
				MauiVersion = typeof(ImageButton).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion,
				CoreVersion = typeof(Microsoft.Maui.IImageButton).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion,
				WindowsVersion = Environment.OSVersion.VersionString,
				WindowsAppSdk = typeof(Microsoft.UI.Xaml.Window).Assembly.GetName().Version?.ToString(),
				Density = DeviceDisplay.Current.MainDisplayInfo.Density,
				ButtonWidth = button.ActualWidth,
				ButtonHeight = button.ActualHeight,
				PaddingLeft = button.Padding.Left,
				PaddingTop = button.Padding.Top,
				PaddingRight = button.Padding.Right,
				PaddingBottom = button.Padding.Bottom,
				BorderLeft = button.BorderThickness.Left,
				BorderTop = button.BorderThickness.Top,
				BorderRight = button.BorderThickness.Right,
				BorderBottom = button.BorderThickness.Bottom,
				ImageWidth = image.ActualWidth,
				ImageHeight = image.ActualHeight,
				ImageX = origin.X,
				ImageY = origin.Y,
				ImageMaxWidth = image.MaxWidth.ToString(CultureInfo.InvariantCulture),
				ImageMaxHeight = image.MaxHeight.ToString(CultureInfo.InvariantCulture),
				NativeStretch = image.Stretch.ToString(),
				NativeSource = image.Source.GetType().FullName,
				DecodedWidth = decoded?.PixelWidth,
				DecodedHeight = decoded?.PixelHeight,
				ButtonPixels = buttonPixels,
				ImagePixels = imagePixels
			};
			await File.WriteAllTextAsync(Path.Combine(_aspectDirectory, name + ".json"),
				JsonSerializer.Serialize(evidence, new JsonSerializerOptions { WriteIndented = true }));
			_aspectResult.Text = $"CAPTURED {name}";
		}
		catch (Exception exception)
		{
			System.Diagnostics.Debug.WriteLine(exception);
			await File.WriteAllTextAsync(Path.Combine(_aspectDirectory, "capture-error.txt"), exception.ToString());
			_aspectResult.Text = "ERROR " + exception;
		}
		finally
		{
			_aspectResult.AutomationId = "AspectCaptureReady";
		}
	}

	async Task<object> Capture(Microsoft.UI.Xaml.UIElement element, string name)
	{
		var render = new RenderTargetBitmap();
		await render.RenderAsync(element);
		var buffer = await render.GetPixelsAsync();
		if (render.PixelWidth <= 0 || render.PixelHeight <= 0 || buffer.Length == 0)
			throw new InvalidOperationException("RenderTargetBitmap returned no pixels.");
		var pixels = new byte[checked((int)buffer.Length)];
		using (var reader = DataReader.FromBuffer(buffer))
			reader.ReadBytes(pixels);
		var visible = 0;
		for (var i = 3; i < pixels.Length; i += 4)
			if (pixels[i] > 0)
				visible++;
		if (visible == 0)
			throw new InvalidOperationException("The rendered element is entirely transparent.");

		var folder = await StorageFolder.GetFolderFromPathAsync(_aspectDirectory);
		var file = await folder.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);
		using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
		{
			var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
			encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied,
				(uint)render.PixelWidth, (uint)render.PixelHeight, 96, 96, pixels);
			await encoder.FlushAsync();
		}
		return new
		{
			Width = render.PixelWidth,
			Height = render.PixelHeight,
			VisiblePixels = visible,
			Sha256 = Convert.ToHexString(SHA256.HashData(pixels))
		};
	}
}
#endif
