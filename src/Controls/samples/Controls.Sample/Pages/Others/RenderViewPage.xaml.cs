#nullable enable

using System.Diagnostics;
using System.IO;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;

namespace Maui.Controls.Sample.Pages
{
	public partial class RenderViewPage
	{
		Stopwatch stopwatch = new Stopwatch();

		public RenderViewPage()
		{
			InitializeComponent();
		}

		private async void RenderButton_Clicked(object sender, System.EventArgs e)
		{
			Reset();
			RenderedView? renderImage = null;
			stopwatch.Start();
			try
			{
				renderImage = await this.RenderButton.RenderAsImage(RenderType.JPEG);
				
			}
			catch (System.Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
			stopwatch.Stop();

			if (renderImage?.Render is not null)
			{
				var imageStream = new MemoryStream(renderImage.Render);
				this.TestImage.Source = ImageSource.FromStream(() => imageStream);
			}
			this.StopwatchTime.Text = stopwatch.Elapsed.ToString();
		}

		private void Reset()
		{
			stopwatch.Reset();
			StopwatchTime.Text = string.Empty;
			this.TestImage.Source = null;
		}
	}
}