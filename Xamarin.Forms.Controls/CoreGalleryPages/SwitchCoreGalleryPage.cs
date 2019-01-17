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

			var onColoredSwitch = new Switch() { OnColor = Color.HotPink };

			var onColorContainer = new ValueViewContainer<Switch>(Test.Switch.OnColor, onColoredSwitch, "OnColor", value => value.ToString());
			var changeOnColorButton = new Button
			{
				Text = "Change OnColor"
			};
			var clearOnColorButton = new Button
			{
				Text = "Clear OnColor"
			};
			changeOnColorButton.Clicked += (s, a) => { onColoredSwitch.OnColor = Color.Red; };
			clearOnColorButton.Clicked += (s, a) => { onColoredSwitch.OnColor = Color.Default; };
			onColorContainer.ContainerLayout.Children.Add(changeOnColorButton);
			onColorContainer.ContainerLayout.Children.Add(clearOnColorButton);

			Add(isToggledContainer);
			Add(onColorContainer);
		}
	}
}