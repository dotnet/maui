namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35399, "VisualElement.ChangeVisualState() gets stuck in Selected state", PlatformAffected.All)]
public class Issue35399 : TestContentPage
{
	const string SelectButtonId = "SelectButton";
	const string DeselectButtonId = "DeselectButton";
	const string StateLabelId = "StateLabel";

	SelectableBox _box = null!;
	Label _stateLabel = null!;

	protected override void Init()
	{
		_stateLabel = new Label
		{
			AutomationId = StateLabelId,
			HorizontalOptions = LayoutOptions.Center,
			FontSize = 14
		};

		_box = new SelectableBox
		{
			HeightRequest = 100,
			WidthRequest = 200,
			HorizontalOptions = LayoutOptions.Center
		};

		Content = new VerticalStackLayout
		{
			Padding = 24,
			Spacing = 16,
			Children =
			{
				_box,
				_stateLabel,
				new HorizontalStackLayout
				{
					HorizontalOptions = LayoutOptions.Center,
					Spacing = 12,
					Children =
					{
						new Button { Text = "Select", AutomationId = SelectButtonId, Command = new Command(OnSelect) },
						new Button { Text = "Deselect", AutomationId = DeselectButtonId, Command = new Command(OnDeselect) }
					}
				}
			}
		};
	}

	void OnSelect()
	{
		_box.IsSelected = true;
		UpdateStateLabel();
	}

	void OnDeselect()
	{
		_box.IsSelected = false;
		UpdateStateLabel();
	}

	void UpdateStateLabel()
	{
		var groups = VisualStateManager.GetVisualStateGroups(_box);
		_stateLabel.Text = groups.Count > 0 ? (groups[0].CurrentState?.Name ?? "None") : "None";
	}

	// Reproduces the ChangeVisualState() pattern from the issue report:
	// when IsSelected goes false, base.ChangeVisualState() is called while the VSM
	// group's CurrentState is still "Selected", causing the base to re-apply it.
	class SelectableBox : ContentView
	{
		public static readonly BindableProperty IsSelectedProperty =
			BindableProperty.Create(
				nameof(IsSelected),
				typeof(bool),
				typeof(SelectableBox),
				defaultValue: false,
				propertyChanged: (b, _, _) => ((SelectableBox)b).ChangeVisualState());

		public bool IsSelected
		{
			get => (bool)GetValue(IsSelectedProperty);
			set => SetValue(IsSelectedProperty, value);
		}

		public SelectableBox()
		{
			VisualStateManager.SetVisualStateGroups(this, new VisualStateGroupList
			{
				new VisualStateGroup
				{
					Name = "CommonStates",
					States =
					{
						new VisualState
						{
							Name = VisualStateManager.CommonStates.Normal,
							Setters = { new Setter { Property = BackgroundColorProperty, Value = Colors.LightGray } }
						},
						new VisualState
						{
							Name = VisualStateManager.CommonStates.Selected,
							Setters = { new Setter { Property = BackgroundColorProperty, Value = Colors.MediumSeaGreen } }
						},
						new VisualState
						{
							Name = VisualStateManager.CommonStates.Disabled,
							Setters = { new Setter { Property = BackgroundColorProperty, Value = Colors.DarkGray } }
						}
					}
				}
			});
		}

		protected internal override void ChangeVisualState()
		{
			if (IsSelected && IsEnabled)
			{
				VisualStateManager.GoToState(this, VisualStateManager.CommonStates.Selected);
			}
			else
			{
				base.ChangeVisualState();
			}
		}
	}
}
