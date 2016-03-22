using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Controls
{
	internal class SwitchCoreGalleryPage : CoreGalleryPage<Switch>
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

			var isToggledContainer = new ValueViewContainer<Switch> (Test.Switch.IsToggled, new Switch (), "IsToggled", value => value.ToString ());
			Add (isToggledContainer);
		}
	}
}