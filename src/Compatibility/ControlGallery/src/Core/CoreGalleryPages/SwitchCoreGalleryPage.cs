using Microsoft.Maui.Controls.CustomAttributes;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery
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

		protected override void Build(StackLayout stackLayout)
		{
			base.Build(stackLayout);

			var isToggledContainer = new ValueViewContainer<Switch>(Test.Switch.IsToggled, new Switch(), "IsToggled", value => value.ToString());

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

			var thumbColorSwitch = new Switch() { ThumbColor = Color.Yellow };
			var thumbColorContainer = new ValueViewContainer<Switch>(Test.Switch.ThumbColor, thumbColorSwitch, nameof(Switch.ThumbColor), value => value.ToString());
			var changeThumbColorButton = new Button { Text = "Change ThumbColor", Command = new Command(() => thumbColorSwitch.ThumbColor = Color.Lime) };
			var clearThumbColorButton = new Button { Text = "Clear ThumbColor", Command = new Command(() => thumbColorSwitch.ThumbColor = Color.Default) };
			thumbColorContainer.ContainerLayout.Children.Add(changeThumbColorButton);
			thumbColorContainer.ContainerLayout.Children.Add(clearThumbColorButton);

			Add(isToggledContainer);
			Add(onColorContainer);
			Add(thumbColorContainer);
		}
	}
}