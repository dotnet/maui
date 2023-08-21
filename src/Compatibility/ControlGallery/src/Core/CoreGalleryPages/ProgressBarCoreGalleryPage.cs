//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.ControlGallery
{
	internal class ProgressBarCoreGalleryPage : CoreGalleryPage<ProgressBar>
	{
		protected override bool SupportsFocus
		{
			get { return false; }
		}

		protected override void InitializeElement(ProgressBar element)
		{
			base.InitializeElement(element);

			element.Progress = 1;
		}

		protected override void Build(StackLayout stackLayout)
		{
			base.Build(stackLayout);

			var progressContainer = new ViewContainer<ProgressBar>(Test.ProgressBar.Progress, new ProgressBar { Progress = 0.5 });
			var colorContainer = new ViewContainer<ProgressBar>(Test.ProgressBar.ProgressColor, new ProgressBar { ProgressColor = Colors.Lime, Progress = 0.5 });

			Add(progressContainer);
			Add(colorContainer);
		}
	}
}