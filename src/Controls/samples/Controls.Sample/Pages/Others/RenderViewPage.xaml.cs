#nullable enable
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Maui.Controls.Sample.ViewModels.Base;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;

namespace Maui.Controls.Sample.Pages
{
	public partial class RenderViewPage
	{
		readonly Stopwatch stopwatch = new Stopwatch();
		readonly RenderBindingModel vm;

		MemoryStream? imageStream;

		public RenderViewPage()
		{
			InitializeComponent();
			BindingContext = vm = new RenderBindingModel();
		}

		async void RenderWindow_Clicked(object? sender, EventArgs e)
		{
			Reset();
			stopwatch.Start();

			var renderImage = await Window.CaptureAsync();

			stopwatch.Stop();

			await RenderView(renderImage);
		}

		async void RenderButton_Clicked(object? sender, EventArgs e)
		{
			Reset();
			stopwatch.Start();

			var renderImage = await RenderButton.CaptureAsync();

			stopwatch.Stop();

			await RenderView(renderImage);
		}

		async void RenderViewSaved_Clicked(object? sender, EventArgs e)
		{
			if (imageStream is not null)
			{
				var extension = vm.RenderType switch
				{
					ScreenshotFormat.Jpeg => ".jpg",
					ScreenshotFormat.Png => ".png",
					_ => ".jpg",
				};

				var fileName = Path.GetTempFileName() + extension;
				var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

				File.WriteAllBytes(filePath, imageStream.ToArray());

				await Share.RequestAsync(new ShareFileRequest
				{
					Title = fileName,
					File = new ShareFile(filePath)
				});
			}
		}

		async Task RenderView(IScreenshotResult? renderImage)
		{
			if (renderImage is not null)
			{
				imageStream = new MemoryStream();
				await renderImage.CopyToAsync(imageStream, vm.RenderType);
				imageStream.Position = 0;

				TestImage.Source = ImageSource.FromStream(() => imageStream);
			}

			StopwatchTime.Text = stopwatch.Elapsed.ToString();
			RenderStats.Text = $"Size: {SizeInBytes(imageStream)}";
		}

		void Reset()
		{
			stopwatch.Reset();

			StopwatchTime.Text = string.Empty;
			RenderStats.Text = string.Empty;

			TestImage.Source = null;
		}

		static string SizeInBytes(Stream? stream)
		{
			if (stream is null)
				return "<null>";

			string[] sizes = { "B", "KB", "MB", "GB", "TB" };

			double len = stream.Length;
			int order = 0;
			while (len >= 1024D && order < sizes.Length - 1)
			{
				order++;
				len /= 1024;
			}

			return string.Format(CultureInfo.CurrentCulture, "{0:0.##} {1}", len, sizes[order]);
		}
	}

	public class RenderBindingModel : BaseViewModel
	{
		string? _selection;

		public string? Selection
		{
			get => _selection;
			set => SetProperty(ref _selection, value);
		}

		public ScreenshotFormat RenderType =>
			Enum.TryParse<ScreenshotFormat>(Selection, out var format)
				? format
				: ScreenshotFormat.Jpeg;
	}
}