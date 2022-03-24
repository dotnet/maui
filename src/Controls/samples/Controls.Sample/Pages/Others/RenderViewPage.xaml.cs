#nullable enable
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
		Stopwatch stopwatch = new Stopwatch();

		RenderBindingModel vm;
		Stream? imageStream;

		public RenderViewPage()
		{
			InitializeComponent();
			BindingContext = vm = new RenderBindingModel();
		}

		async void RenderWindow_Clicked(object sender, EventArgs e)
		{
			Reset();

			stopwatch.Start();
			var renderImage = await Window.CaptureAsync(vm.RenderType);
			stopwatch.Stop();

			RenderView(renderImage);
		}

		async void RenderButton_Clicked(object sender, EventArgs e)
		{
			Reset();

			stopwatch.Start();
			var renderImage = await RenderButton.CaptureAsync(vm.RenderType);
			stopwatch.Stop();

			RenderView(renderImage);
		}

		async void RenderViewSaved_Clicked(object sender, EventArgs e)
		{
			if (imageStream is not null)
			{
				var extension = vm.RenderType switch
				{
					ScreenshotFormat.Jpeg => "jpg",
					ScreenshotFormat.Png => "png",
					_ => "jpg",
				};

				string fileName = $"{Path.GetTempFileName()}.{extension}";
				string filePath = Path.Combine(FileSystem.Current.CacheDirectory, fileName);

				using (var file = File.Create(filePath))
				{
					imageStream.CopyTo(file);
				}

				await Share.Default.RequestAsync(new ShareFileRequest { Title = fileName, File = new ShareFile(filePath) });
			}
		}

		void RenderView(Stream? renderImage)
		{
			if (renderImage is not null)
			{
				imageStream = renderImage;
				TestImage.Source = ImageSource.FromStream(() => imageStream);
			}

			StopwatchTime.Text = stopwatch.Elapsed.ToString();
			RenderStats.Text = $"Size: {SizeInBytes(renderImage)}";
		}

		void Reset()
		{
			stopwatch.Reset();
			StopwatchTime.Text = string.Empty;
			RenderStats.Text = string.Empty;
			TestImage.Source = null;
		}

		string SizeInBytes(Stream? array)
		{
			var sizes = new string[] { "B", "KB", "MB", "GB", "TB" };
			var len = (double)(array?.Length ?? 0);

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