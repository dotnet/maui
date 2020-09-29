using System;

namespace Xamarin.Forms.Controls
{
	internal class StateViewContainer<T> : ViewContainer<T>
		where T : View
	{
		public Button StateChangeButton { get; private set; }
		public Label ViewInteractionLabel { get; private set; }

		public StateViewContainer(Enum formsMember, T view) : base(formsMember, view)
		{
			var name = formsMember.ToString();

			var stateTitleLabel = new Label
			{
				Text = name + "?"
			};

			ViewInteractionLabel = new Label
			{
				Text = "Interacted? : False"
			};

			var stateValueLabel = new Label
			{
				BindingContext = view,
				AutomationId = name + "StateLabel"
			};

			if (name == "Focus" || name == "Unfocused" || name == "Focused")
				stateValueLabel.SetBinding(Label.TextProperty, "IsFocused", converter: new GenericValueConverter(o => o.ToString()));
			else
				stateValueLabel.SetBinding(Label.TextProperty, name, converter: new GenericValueConverter(o => o.ToString()));

			StateChangeButton = new Button
			{
				Text = "Change State: " + name,
				AutomationId = name + "StateButton"
			};

			var labelLayout = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				Children = {
					stateTitleLabel,
					stateValueLabel
				}
			};

			ContainerLayout.Children.Add(ViewInteractionLabel);
			ContainerLayout.Children.Add(labelLayout);
			ContainerLayout.Children.Add(StateChangeButton);
		}
	}
}