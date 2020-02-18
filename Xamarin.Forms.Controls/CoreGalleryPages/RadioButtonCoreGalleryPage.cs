using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Controls
{
	class RadioButtonCoreGalleryPage : CoreGalleryPage<RadioButton>
	{
		protected override bool SupportsFocus => false;
		protected override bool SupportsTapGestureRecognizer => true;
		protected override void InitializeElement(RadioButton element)
		{
			element.Text = "RadioButton";
		}

		protected override void Build(StackLayout stackLayout)
		{
			base.Build(stackLayout);

			IsEnabledStateViewContainer.View.Clicked += (sender, args) => IsEnabledStateViewContainer.TitleLabel.Text += " (Tapped)";

			var borderButtonContainer = new ViewContainer<RadioButton>(Test.Button.BorderColor,
				new RadioButton
				{
					Text = "BorderColor",
					BackgroundColor = Color.Transparent,
					BorderColor = Color.Red,
					BorderWidth = 1,
				}
			);

			var borderRadiusContainer = new ViewContainer<RadioButton>(Test.Button.BorderRadius,
				new RadioButton
				{
					Text = "BorderRadius",
					BackgroundColor = Color.Transparent,
					BorderColor = Color.Red,
					BorderWidth = 1,
				}
			);

			var borderWidthContainer = new ViewContainer<RadioButton>(Test.Button.BorderWidth,
				new RadioButton
				{
					Text = "BorderWidth",
					BackgroundColor = Color.Transparent,
					BorderColor = Color.Red,
					BorderWidth = 15,
				}
			);

			var clickedContainer = new EventViewContainer<RadioButton>(Test.Button.Clicked,
				new RadioButton
				{
					Text = "Clicked"
				}
			);
			clickedContainer.View.Clicked += (sender, args) => clickedContainer.EventFired();

			var pressedContainer = new EventViewContainer<RadioButton>(Test.Button.Pressed,
				new RadioButton
				{
					Text = "Pressed"
				}
			);
			pressedContainer.View.Pressed += (sender, args) => pressedContainer.EventFired();

			var commandContainer = new ViewContainer<RadioButton>(Test.Button.Command,
				new RadioButton
				{
					Text = "Command",
					Command = new Command(() => DisplayActionSheet("Hello Command", "Cancel", "Destroy"))
				}
			);

			var fontContainer = new ViewContainer<RadioButton>(Test.Button.Font,
				new RadioButton
				{
					Text = "Font",
					Font = Font.SystemFontOfSize(NamedSize.Large, FontAttributes.Bold)
				}
			);

			var textContainer = new ViewContainer<RadioButton>(Test.Button.Text,
				new RadioButton
				{
					Text = "Text"
				}
			);

			var textColorContainer = new ViewContainer<RadioButton>(Test.Button.TextColor,
				new RadioButton
				{
					Text = "TextColor",
					TextColor = Color.Pink
				}
			);

			var paddingContainer = new ViewContainer<RadioButton>(Test.Button.Padding,
				new RadioButton
				{
					Text = "Padding",
					BackgroundColor = Color.Red,
					Padding = new Thickness(20, 30, 60, 15)
				}
			);

			var isCheckedContainer = new ValueViewContainer<RadioButton>(Test.RadioButton.IsChecked, new RadioButton() { IsChecked = true, HorizontalOptions = LayoutOptions.Start }, "IsChecked", value => value.ToString());

			var checkedVisualState = new VisualState { Name = "IsChecked" };
			checkedVisualState.Setters.Add(new Setter { Property = RadioButton.ButtonSourceProperty, Value = "rb_checked" });

			var group = new VisualStateGroup();
			group.States.Add(checkedVisualState);

			var normalVisualState = new VisualState{  Name = "Normal" };
			normalVisualState.Setters.Add(new Setter { Property = RadioButton.ButtonSourceProperty, Value = "rb_unchecked" });
			group.States.Add(normalVisualState);

			var groupList = new VisualStateGroupList();
			groupList.Add(group);

			var rbStateManaged = new RadioButton() { HorizontalOptions = LayoutOptions.Start };
			VisualStateManager.SetVisualStateGroups(rbStateManaged, groupList);

			var stateManagedContainer = new ValueViewContainer<RadioButton>(Test.RadioButton.ButtonSource, rbStateManaged, "IsChecked", value => value.ToString());

			Add(borderButtonContainer);
			Add(borderRadiusContainer);
			Add(borderWidthContainer);
			Add(clickedContainer);
			Add(pressedContainer);
			Add(commandContainer);
			Add(fontContainer);
			Add(textContainer);
			Add(textColorContainer);
			Add(paddingContainer);
			Add(isCheckedContainer);
			Add(stateManagedContainer);
		}
	}
}
