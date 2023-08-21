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

namespace Microsoft.Maui.Controls.ControlGallery
{
	internal class StepperCoreGalleryPage : CoreGalleryPage<Stepper>
	{
		protected override bool SupportsFocus
		{
			get { return false; }
		}

		protected override bool SupportsTapGestureRecognizer
		{
			get { return false; }
		}

		protected override void Build(StackLayout stackLayout)
		{
			base.Build(stackLayout);
			var maximumContainer = new ValueViewContainer<Stepper>(Test.Stepper.Maximum, new Stepper { Maximum = 10 }, "Value", value => value.ToString());
			var minimumContainer = new ValueViewContainer<Stepper>(Test.Stepper.Minimum, new Stepper { Minimum = 2 }, "Value", value => value.ToString());
			var incrememtContainer = new ValueViewContainer<Stepper>(Test.Stepper.Increment, new Stepper { Maximum = 20, Minimum = 10, Increment = 2 }, "Value", value => value.ToString());

			Add(maximumContainer);
			Add(minimumContainer);
			Add(incrememtContainer);
		}
	}
}