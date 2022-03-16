#nullable enable

using System.Diagnostics;
using System.IO;
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
			byte[]? renderImage = null;
			stopwatch.Start();
			try
			{
				renderImage = await this.RenderButton.RenderAsBMP();
				
			}
			catch (System.Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
			stopwatch.Stop();

			if (renderImage is not null)
			{
				var imageStream = new MemoryStream(renderImage);
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