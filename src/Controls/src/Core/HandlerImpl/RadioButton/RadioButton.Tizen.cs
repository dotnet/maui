using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Shapes;

namespace Microsoft.Maui.Controls
{
	public partial class RadioButton
	{
		static ControlTemplate s_tizenDefaultTemplate;


		public static void MapContent(RadioButtonHandler handler, RadioButton radioButton)
			=> MapContent((IRadioButtonHandler)handler, radioButton);

		public static void MapContent(IRadioButtonHandler handler, RadioButton radioButton)
		{
			if (radioButton.ResolveControlTemplate() == null)
				radioButton.ControlTemplate = s_tizenDefaultTemplate ?? (s_tizenDefaultTemplate = new ControlTemplate(() => BuildTizenDefaultTemplate()));

			RadioButtonHandler.MapContent(handler, radioButton);
		}

		static View BuildTizenDefaultTemplate()
		{
			var frame = new Frame
			{
				HasShadow = false,
				Padding = 6
			};

			BindToTemplatedParent(frame, BackgroundColorProperty, Controls.Frame.BorderColorProperty, Controls.Frame.CornerRadiusProperty, HorizontalOptionsProperty,
				MarginProperty, OpacityProperty, RotationProperty, ScaleProperty, ScaleXProperty, ScaleYProperty,
				TranslationYProperty, TranslationXProperty, VerticalOptionsProperty);

			var grid = new Grid
			{
				ColumnSpacing = 6,
				RowSpacing = 0,
				ColumnDefinitions = new ColumnDefinitionCollection {
					new ColumnDefinition { Width = GridLength.Auto },
					new ColumnDefinition { Width = GridLength.Star }
				},
				RowDefinitions = new RowDefinitionCollection {
					new RowDefinition { Height = GridLength.Auto }
				}
			};

			var normalEllipse = new Ellipse
			{
				Fill = Brush.Transparent,
				Aspect = Stretch.Uniform,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				HeightRequest = 21,
				WidthRequest = 21,
				StrokeThickness = 2,
				Stroke = RadioButtonThemeColor,
				InputTransparent = true
			};

			var checkMark = new Ellipse
			{
				Fill = RadioButtonCheckMarkThemeColor,
				Aspect = Stretch.Uniform,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				HeightRequest = 11,
				WidthRequest = 11,
				Opacity = 0,
				InputTransparent = true
			};

			var contentPresenter = new ContentPresenter
			{
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Center
			};

			contentPresenter.SetBinding(BackgroundColorProperty, new Binding(BackgroundColorProperty.PropertyName,
				source: RelativeBindingSource.TemplatedParent));

			grid.Add(normalEllipse);
			grid.Add(checkMark);
			grid.Add(contentPresenter, 1, 0);

			frame.Content = grid;

			INameScope nameScope = new NameScope();
			NameScope.SetNameScope(frame, nameScope);
			nameScope.RegisterName(TemplateRootName, frame);
			nameScope.RegisterName(UncheckedButton, normalEllipse);
			nameScope.RegisterName(CheckedIndicator, checkMark);
			nameScope.RegisterName("ContentPresenter", contentPresenter);

			VisualStateGroupList visualStateGroups = new VisualStateGroupList();

			var common = new VisualStateGroup() { Name = "Common" };
			common.States.Add(new VisualState() { Name = VisualStateManager.CommonStates.Normal });
			common.States.Add(new VisualState() { Name = VisualStateManager.CommonStates.Disabled });

			visualStateGroups.Add(common);

			var checkedStates = new VisualStateGroup() { Name = "CheckedStates" };

			VisualState checkedVisualState = new VisualState() { Name = CheckedVisualState };
			checkedVisualState.Setters.Add(new Setter() { Property = OpacityProperty, TargetName = CheckedIndicator, Value = 1 });
			checkedVisualState.Setters.Add(new Setter() { Property = Shape.StrokeProperty, TargetName = UncheckedButton, Value = RadioButtonCheckMarkThemeColor });
			checkedStates.States.Add(checkedVisualState);

			VisualState uncheckedVisualState = new VisualState() { Name = UncheckedVisualState };
			uncheckedVisualState.Setters.Add(new Setter() { Property = OpacityProperty, TargetName = CheckedIndicator, Value = 0 });
			uncheckedVisualState.Setters.Add(new Setter() { Property = Shape.StrokeProperty, TargetName = UncheckedButton, Value = RadioButtonThemeColor });
			checkedStates.States.Add(uncheckedVisualState);

			visualStateGroups.Add(checkedStates);

			VisualStateManager.SetVisualStateGroups(frame, visualStateGroups);

			return frame;
		}
	}
}