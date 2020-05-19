using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Controls
{
	internal class ProgressBarCoreGalleryPage : CoreGalleryPage<ProgressBar>
	{
		protected override bool SupportsFocus
		{
			get { return false; }
		}

		protected override void InitializeElement (ProgressBar element)
		{
			base.InitializeElement (element);

			element.Progress = 1;
		}

		protected override void Build (StackLayout stackLayout)
		{
			base.Build (stackLayout);

			var progressContainer = new ViewContainer<ProgressBar>(Test.ProgressBar.Progress, new ProgressBar { Progress = 0.5 });
			var colorContainer = new ViewContainer<ProgressBar>(Test.ProgressBar.ProgressColor, new ProgressBar { ProgressColor = Color.Lime, Progress = 0.5 });

			Add (progressContainer);
			Add(colorContainer);
		}
	}
}