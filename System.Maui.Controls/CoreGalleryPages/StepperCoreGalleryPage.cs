using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Controls
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

		protected override void Build (StackLayout stackLayout)
		{
			base.Build (stackLayout);
			var maximumContainer = new ValueViewContainer<Stepper> (Test.Stepper.Maximum, new Stepper { Maximum = 10 }, "Value", value => value.ToString ());
			var minimumContainer = new ValueViewContainer<Stepper> (Test.Stepper.Minimum, new Stepper { Minimum = 2 }, "Value", value => value.ToString ());
			var incrememtContainer = new ValueViewContainer<Stepper> (Test.Stepper.Increment, new Stepper { Maximum = 20, Minimum = 10, Increment = 2 }, "Value", value => value.ToString ());

			Add (maximumContainer);
			Add (minimumContainer);
			Add (incrememtContainer);
		}
	}
}