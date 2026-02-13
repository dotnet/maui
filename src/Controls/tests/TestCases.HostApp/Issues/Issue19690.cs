using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 19690, "Button VisualStates do not work", PlatformAffected.iOS | PlatformAffected.Android | PlatformAffected.macOS)]
public class Issue19690 : ContentPage
{
	const string CustomStateName = "Custom";

	public Issue19690()
	{
		var button = new Issue19690CustomButton
		{
			Text = "Click Me",
			AutomationId = "TestButton"
		};

		var statusLabel = new Label
		{
			Text = "Initial State",
			AutomationId = "StatusLabel",
			HorizontalOptions = LayoutOptions.Center,
			Margin = new Thickness(0, 20, 0, 0)
		};

		button.Clicked += (s, e) =>
		{
			statusLabel.Text = button.IsCustom ? "Custom State" : "Normal State";
		};

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Children =
			{
				new Label
				{
					Text = "Click the button multiple times. The button should toggle between Normal (default colors) and Custom (Purple background, Yellow text) states.",
					AutomationId = "InstructionLabel"
				},
				button,
				statusLabel
			}
		};
	}

	class Issue19690CustomButton : Button
	{
		public static readonly BindableProperty IsCustomProperty =
			BindableProperty.Create(nameof(IsCustom), typeof(bool), typeof(Issue19690CustomButton), false, propertyChanged: OnIsCustomChanged);

		public bool IsCustom
		{
			get => (bool)GetValue(IsCustomProperty);
			set => SetValue(IsCustomProperty, value);
		}

		public Issue19690CustomButton()
		{
			var visualStateGroups = new VisualStateGroupList();
			var commonStates = new VisualStateGroup { Name = "CommonStates" };

			commonStates.States.Add(new VisualState { Name = "Normal" });
			commonStates.States.Add(new VisualState { Name = "Disabled" });
			commonStates.States.Add(new VisualState { Name = "PointerOver" });
			commonStates.States.Add(new VisualState { Name = "Pressed" });
			commonStates.States.Add(new VisualState { Name = "Focused" });

			var customState = new VisualState { Name = CustomStateName };
			customState.Setters.Add(new Setter
			{
				Property = TextColorProperty,
				Value = Colors.Yellow
			});
			customState.Setters.Add(new Setter
			{
				Property = BackgroundColorProperty,
				Value = Colors.Purple
			});
			commonStates.States.Add(customState);

			visualStateGroups.Add(commonStates);
			VisualStateManager.SetVisualStateGroups(this, visualStateGroups);

			Clicked += OnButtonClicked;
		}

		void OnButtonClicked(object sender, EventArgs e)
		{
			IsCustom = !IsCustom;
		}

		protected internal override void ChangeVisualState()
		{
			if (IsCustom && IsEnabled)
			{
				VisualStateManager.GoToState(this, CustomStateName);
			}
			else
			{
				base.ChangeVisualState();
			}
		}

		static void OnIsCustomChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is Issue19690CustomButton button)
			{
				button.ChangeVisualState();
			}
		}
	}
}
