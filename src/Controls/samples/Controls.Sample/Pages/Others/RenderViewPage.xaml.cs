#nullable enable

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Platform;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace Maui.Controls.Sample.Pages
{
	public partial class RenderViewPage
	{
		Stopwatch stopwatch = new Stopwatch();
		RenderBindingModel vm;
		MemoryStream? imageStream;
		byte[]? byteStream;

		public RenderViewPage()
		{
			InitializeComponent();
			this.BindingContext = this.vm = new RenderBindingModel();

		}

		private async void RenderWindow_Clicked(object sender, System.EventArgs e)
		{
			Reset();
			RenderedView? renderImage = null;
			var window = this.GetParentWindow() as IWindow;
			stopwatch.Start();
			try
			{
				if (window is not null)
				{
					renderImage = await window.RenderAsImage(vm.RenderType);
				}

			}
			catch (System.Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
			stopwatch.Stop();

			RenderView(renderImage);
		}

		private async void RenderButton_Clicked(object sender, System.EventArgs e)
		{
			Reset();
			RenderedView? renderImage = null;
			stopwatch.Start();
			try
			{
				renderImage = await this.RenderButton.RenderAsImage(vm.RenderType);
				
			}
			catch (System.Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
			stopwatch.Stop();

			RenderView(renderImage);
		}

		private async void RenderViewSaved_Clicked(object sender, System.EventArgs e)
		{
			if (byteStream is not null)
			{
				var extension = vm.RenderType switch
				{
					RenderType.JPEG => "jpg",
					RenderType.PNG => "png",
					RenderType.BMP => "bmp",
					_ => "jpg",
				};
				string fileName = $"{System.IO.Path.GetTempFileName()}.{extension}";
				string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), fileName);
				File.WriteAllBytes(filePath, byteStream);
				await Share.RequestAsync(new ShareFileRequest() { Title = fileName, File = new ShareFile(filePath) });
			}
		}

		private void RenderView(RenderedView? renderImage)
		{
			if (renderImage?.Render is not null)
			{
				try
				{
					byteStream = renderImage.Render;
					imageStream = new MemoryStream(renderImage.Render);
					this.TestImage.Source = ImageSource.FromStream(() => imageStream);
				}
				catch (System.Exception ex)
				{
					System.Diagnostics.Debug.WriteLine(ex.Message);
				}
			}
			this.StopwatchTime.Text = stopwatch.Elapsed.ToString();
			this.RenderStats.Text = $"Type: {renderImage?.RenderType}; Size: {SizeInBytes(renderImage?.Render)}";
		}

		private void Reset()
		{
			stopwatch.Reset();
			StopwatchTime.Text = string.Empty;
			RenderStats.Text = string.Empty;
			this.TestImage.Source = null;
		}

		private string SizeInBytes(byte[]? array)
		{
			if (array is null)
				return string.Empty;
			string[] sizes = { "B", "KB", "MB", "GB", "TB" };
			double len = System.Convert.ToDouble(array.Length);
			int order = 0;
			while (len >= 1024D && order < sizes.Length - 1)
			{
				order++;
				len /= 1024;
			}

			return string.Format(CultureInfo.CurrentCulture, "{0:0.##} {1}", len, sizes[order]);
		}
	}

	public class RenderBindingModel : INotifyPropertyChanged
	{
		private string? _selection;

		public event PropertyChangedEventHandler? PropertyChanged;

		void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public string? Selection
		{
			get => _selection;
			set
			{
				_selection = value;
				OnPropertyChanged(nameof(Selection));
			}
		}

		public RenderType RenderType
		{
			get
			{
				if (_selection is null)
					return RenderType.JPEG;

				return (RenderType)System.Enum.Parse(typeof(RenderType), _selection);
			}
		}
	}
}