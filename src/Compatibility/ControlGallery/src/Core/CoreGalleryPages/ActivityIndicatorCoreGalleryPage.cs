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
	internal class ActivityIndicatorCoreGalleryPage : CoreGalleryPage<ActivityIndicator>
	{
		protected override bool SupportsTapGestureRecognizer
		{
			get { return true; }
		}

		protected override void InitializeElement(ActivityIndicator element)
		{
			element.IsRunning = true;
		}

		protected override void Build(StackLayout stackLayout)
		{
			base.Build(stackLayout);

			var colorContainer = new ViewContainer<ActivityIndicator>(Test.ActivityIndicator.Color, new ActivityIndicator
			{
				Color = Colors.Lime,
				IsRunning = true

			});

			var isRunningContainer = new StateViewContainer<ActivityIndicator>(Test.ActivityIndicator.IsRunning, new ActivityIndicator
			{
				IsRunning = true
			});

			isRunningContainer.StateChangeButton.Clicked += (sender, args) =>
			{
				isRunningContainer.View.IsRunning = !isRunningContainer.View.IsRunning;
			};

			Add(colorContainer);
			Add(isRunningContainer);
		}
	}
}