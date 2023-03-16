#nullable disable
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

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
			Border border = new Border()
			{
				Padding = 6
			};

			BindToTemplatedParent(border, BackgroundColorProperty, HorizontalOptionsProperty,
				MarginProperty, OpacityProperty, RotationProperty, ScaleProperty, ScaleXProperty, ScaleYProperty,
				TranslationYProperty, TranslationXProperty, VerticalOptionsProperty);

			border.SetBinding(Border.StrokeProperty,
				new Binding(BorderColorProperty.PropertyName,
							source: RelativeBindingSource.TemplatedParent));

			border.SetBinding(Border.StrokeShapeProperty,
				new Binding(CornerRadiusProperty.PropertyName, converter: new CornerRadiusToShape(),
							source: RelativeBindingSource.TemplatedParent));

			border.SetBinding(Border.StrokeThicknessProperty,
				new Binding(BorderWidthProperty.PropertyName,
							source: RelativeBindingSource.TemplatedParent));

			var grid = new Grid
			{
				Padding = 2,
				RowSpacing = 0,
				ColumnSpacing = 6,
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
				InputTransparent = true
			};

			var checkMark = new Ellipse
			{
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
				VerticalOptions = LayoutOptions.Fill
			};

			object dynamicOuterEllipseThemeColor = null;
			object dynamicCheckMarkThemeColor = null;
			object outerEllipseVisualStateLight = null;
			object outerEllipseVisualStateDark = null;
			object checkMarkVisualStateLight = null;
			object checkMarkVisualStateDark = null;

			if (!normalEllipse.TrySetDynamicThemeColor(
				RadioButtonThemeColor,
				Ellipse.StrokeProperty,
				out dynamicOuterEllipseThemeColor))
			{
				normalEllipse.TrySetAppTheme(
					RadioButtonOuterEllipseStrokeLight,
					RadioButtonOuterEllipseStrokeDark,
					Ellipse.StrokeProperty,
					SolidColorBrush.White,
					SolidColorBrush.Black,
					out outerEllipseVisualStateLight,
					out outerEllipseVisualStateDark);
			}

			if (!checkMark.TrySetDynamicThemeColor(
				RadioButtonCheckMarkThemeColor,
				Ellipse.StrokeProperty,
				out dynamicCheckMarkThemeColor))
			{
				checkMark.TrySetAppTheme(
					RadioButtonCheckGlyphStrokeLight,
					RadioButtonCheckGlyphStrokeDark,
					Ellipse.StrokeProperty,
					SolidColorBrush.White,
					SolidColorBrush.Black,
					out checkMarkVisualStateLight,
					out checkMarkVisualStateDark);
			}

			if (!checkMark.TrySetDynamicThemeColor(
				RadioButtonCheckMarkThemeColor,
				Ellipse.FillProperty,
				out dynamicCheckMarkThemeColor))
			{
				checkMark.TrySetAppTheme(
					RadioButtonCheckGlyphFillLight,
					RadioButtonCheckGlyphFillDark,
					Ellipse.FillProperty,
					SolidColorBrush.White,
					SolidColorBrush.Black,
					out _,
					out _);
			}

			contentPresenter.SetBinding(MarginProperty, new Binding("Padding", source: RelativeBindingSource.TemplatedParent));
			contentPresenter.SetBinding(BackgroundColorProperty, new Binding(BackgroundColorProperty.PropertyName,
				source: RelativeBindingSource.TemplatedParent));

			grid.Add(normalEllipse);
			grid.Add(checkMark);
			grid.Add(contentPresenter, 1, 0);

			border.Content = grid;

			INameScope nameScope = new NameScope();
			NameScope.SetNameScope(border, nameScope);
			nameScope.RegisterName(TemplateRootName, border);
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
			checkedVisualState.Setters.Add(
				new Setter()
				{
					Property = Shape.StrokeProperty,
					TargetName = UncheckedButton,
					Value = dynamicOuterEllipseThemeColor is not null ? dynamicOuterEllipseThemeColor : new AppThemeBinding() { Light = outerEllipseVisualStateLight, Dark = outerEllipseVisualStateDark }
				});
			checkedVisualState.Setters.Add(
				new Setter()
				{
					Property = Shape.StrokeProperty,
					TargetName = CheckedIndicator,
					Value = dynamicCheckMarkThemeColor is not null ? dynamicCheckMarkThemeColor : new AppThemeBinding() { Light = checkMarkVisualStateLight, Dark = checkMarkVisualStateDark }
				});
			checkedStates.States.Add(checkedVisualState);

			VisualState uncheckedVisualState = new VisualState() { Name = UncheckedVisualState };
			uncheckedVisualState.Setters.Add(new Setter() { Property = OpacityProperty, TargetName = CheckedIndicator, Value = 0 });

			uncheckedVisualState.Setters.Add(
				new Setter()
				{
					Property = Shape.StrokeProperty,
					TargetName = UncheckedButton,
					Value = dynamicOuterEllipseThemeColor is not null ? dynamicOuterEllipseThemeColor : new AppThemeBinding() { Light = outerEllipseVisualStateLight, Dark = outerEllipseVisualStateDark }
				});

			checkedStates.States.Add(uncheckedVisualState);

			visualStateGroups.Add(checkedStates);

			VisualStateManager.SetVisualStateGroups(border, visualStateGroups);

			return border;
		}
	}
}