using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;
using System.Linq;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Controls
{
	// Once MAUI is capable of packaging styles into XAML files we will have implicit styles
	// that we can add as the base for the DefaultDictionary
	internal static class DefaultStyles
	{
		static Style ButtonDefaultStyle { get; set; }
		static Dictionary<Type, Style> TextElementDefaultStyle { get; } = new Dictionary<Type, Style>();

		static T GetThemeChoice<T>(T light, T dark)
		{
			if (Application.Current?.RequestedTheme == AppTheme.Dark)
				return dark;

			return light;
		}

		class LightTheme
		{
			public static Color TextColor = new Color(1.0f, 1.0f, 1.0f);
#if ANDROID
			public static Color ButtonBackgroundColor = new Color(44, 62, 80);
#else
			public static Color ButtonBackgroundColor => Colors.Black;
#endif
		}

		class DarkTheme
		{
			public static Color TextColor = Colors.Black;
			public static Color ButtonBackgroundColor => Colors.White;
		}

		public static Setter GetTextColor(BindableObject view)
		{
#if ANDROID || IOS
			Style styleToUse = null;

			if (view is ITextElement)
			{
				// trigger VSM creation
				if (view is VisualElement ve)
					_ = VisualStateManager.GetVisualStateGroups(ve);

				var viewType = view.GetType();

				if (TextElementDefaultStyle.TryGetValue(viewType, out Style existing))
				{
					styleToUse = existing;
				}
				else
				{
					var textColorSetting = new Setter();
					textColorSetting.Property = TextElement.TextColorProperty;
					textColorSetting.Value = GetThemeChoice(LightTheme.TextColor, DarkTheme.TextColor);

					styleToUse = new Style(typeof(Button))
					{
						Setters =
						{
							textColorSetting
						}
					};

					TextElementDefaultStyle[viewType] = styleToUse;
				}
			}

			if (styleToUse?.Setters?.Count > 0)
			{
				foreach (var setter in styleToUse.Setters)
					if (setter.Property == TextElement.TextColorProperty)
						return setter;
			}

			return null;
#else
			return null;
#endif
		}


		public static Setter GetBackgroundColor(BindableObject view)
		{
#if ANDROID || IOS
			Style styleToUse = null;

			if (view is Button button)
			{
				// trigger VSM creation
				_ = VisualStateManager.GetVisualStateGroups(button);
				if (ButtonDefaultStyle == null)
				{
					var backgroundColorSetter = new Setter();
					backgroundColorSetter.Property = VisualElement.BackgroundColorProperty;
					backgroundColorSetter.Value = GetThemeChoice(LightTheme.ButtonBackgroundColor, DarkTheme.ButtonBackgroundColor);

					ButtonDefaultStyle = new Style(typeof(Button))
					{
						Setters =
							{
								backgroundColorSetter
							}
					};

				}

				styleToUse = ButtonDefaultStyle;
			}

			if (styleToUse?.Setters?.Count > 0)
			{
				foreach (var setter in styleToUse.Setters)
					if (setter.Property == VisualElement.BackgroundColorProperty)
						return setter;
			}

			return null;
#else
			return null;
#endif
		}

		internal static VisualStateGroupList GetVisualStateManager(BindableObject bindable)
		{
#if IOS || ANDROID
			var visualStateGroup = new VisualStateGroup() { Name = "CommonStates" };
			var disabledSetters = new VisualState()
			{
				Name = "Disabled"
			};

			if (bindable is Button button)
			{
				var disabledBackgroundColor = new Setter()
				{
					Property = Button.BackgroundColorProperty,
					// Update this once AppThemeBindings are fixed
					Value = GetThemeChoice(LightTheme.ButtonBackgroundColor, DarkTheme.ButtonBackgroundColor).WithAlpha(0.12f)
				};

				disabledSetters.Setters.Add(disabledBackgroundColor);
			}

			if (bindable is ITextElement)
			{
				var disabledTextColor = new Setter()
				{
					Property = TextElement.TextColorProperty,
					// Update this once AppThemeBindings are fixed
					Value = new Color(0f, 0f, 0f).WithAlpha(0.38f)
				};
				disabledSetters.Setters.Add(disabledTextColor);
			}

			if (disabledSetters.Setters.Count > 0)
			{
				var visualStateGroupList = new VisualStateGroupList();

				visualStateGroup.States.Add(new VisualState()
				{
					Name = "Normal"
				});

				visualStateGroup.States.Add(disabledSetters);

				return new VisualStateGroupList() { visualStateGroup };
			}
#endif
			return null;
		}
	}
}
