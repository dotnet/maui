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
		static Dictionary<Type, Style> DefaultStylesCache { get; } = new Dictionary<Type, Style>();


		static T GetThemeChoice<T>(T light, T dark)
		{
			if (Application.Current?.RequestedTheme == AppTheme.Dark)
				return dark;

			return light;
		}

		class LightTheme
		{
			public static Color ButtonTextColor => Colors.Black;
			public static Color TextColor => Colors.Black;
#if ANDROID
			public static Color ButtonBackgroundColor = new Color(44, 62, 80);
#else
			public static Color ButtonBackgroundColor => Colors.White;
#endif
		}

		class DarkTheme
		{
#if ANDROID
			public static Color ButtonBackgroundColor = new Color(44, 62, 80);
			public static Color TextColor => Colors.White;
			public static Color ButtonTextColor => Colors.Black;
#else
			public static Color ButtonBackgroundColor => Colors.Black;
			public static Color ButtonTextColor => Colors.White;
			public static Color TextColor => Colors.White;
#endif
		}


		static Setter GetSetterFor(Type viewType, BindableProperty bindableProperty, out Style style)
		{
			if (DefaultStylesCache.TryGetValue(viewType, out Style existing))
			{
				style = existing;
				foreach (var setter in style.Setters)
				{
					if (setter.Property == bindableProperty)
						return setter;
				}
			}
			else
			{
				style = new Style(viewType);
				DefaultStylesCache[viewType] = style;
			}

			return null;
		}

		public static Setter GetTextColor(BindableObject view) =>
			GetTextColor(view.GetType());

		public static Setter GetTextColor(Type viewType)
		{
			Setter setterToUse = null;

#if ANDROID || IOS

			if (viewType.IsAssignableTo(typeof(ITextElement)))
			{
				//// trigger VSM creation
				//if (view is VisualElement ve)
				//	_ = VisualStateManager.GetVisualStateGroups(ve);

				setterToUse = GetSetterFor(viewType, TextElement.TextColorProperty, out Style style);
				if (setterToUse == null)
				{
					var textColorSetting = new Setter();
					textColorSetting.Property = TextElement.TextColorProperty;

					if (viewType.IsAssignableFrom(typeof(Button)))
						textColorSetting.Value = GetThemeChoice(LightTheme.ButtonTextColor, DarkTheme.ButtonTextColor);
					else
						textColorSetting.Value = GetThemeChoice(LightTheme.TextColor, DarkTheme.TextColor);

					setterToUse = textColorSetting;
					style.Setters.Add(setterToUse);
				}
			}

#endif
			return setterToUse;
		}

		public static Setter GetBackgroundColor(BindableObject view) =>
			GetBackgroundColor(view.GetType());

		public static Setter GetBackgroundColor(Type viewType)
		{
			Setter setterToUse = null;

#if ANDROID || IOS

			if (viewType.IsAssignableFrom(typeof(Button)))
			{
				// trigger VSM creation
				//_ = VisualStateManager.GetVisualStateGroups(button);
				setterToUse = GetSetterFor(viewType, VisualElement.BackgroundColorProperty, out Style style);
				if (setterToUse == null)
				{
					var backgroundColorSetter = new Setter();
					backgroundColorSetter.Property = VisualElement.BackgroundColorProperty;
					backgroundColorSetter.Value = GetThemeChoice(LightTheme.ButtonBackgroundColor, DarkTheme.ButtonBackgroundColor);

					setterToUse = backgroundColorSetter;
					style.Setters.Add(setterToUse);
				}
			}

#endif
			return setterToUse;
		}

		internal static VisualStateGroupList GetVisualStateManager(BindableObject bindable)
		{
			return GetVisualStateManager(bindable.GetType(), bindable);
		}

		internal static VisualStateGroupList GetVisualStateManager(Type viewType, BindableObject bindable = null)
		{
#if IOS || ANDROID

			// This means we are retrieving this for a style not a specific bindable
			if (bindable == null)
			{
				var existing = GetSetterFor(viewType, VisualStateManager.VisualStateGroupsProperty, out Style style);
				if (existing != null)
				{
					return (VisualStateGroupList)existing.Value;
				}
			}

			var visualStateGroup = new VisualStateGroup() { Name = "CommonStates" };
			var disabledSetters = new VisualState()
			{
				Name = "Disabled"
			};

			if (viewType.IsAssignableFrom(typeof(Button)))
			{
				var disabledBackgroundColor = new Setter()
				{
					Property = Button.BackgroundColorProperty,
					Value = GetThemeChoice(LightTheme.ButtonBackgroundColor, DarkTheme.ButtonBackgroundColor).WithAlpha(0.12f)
				};

				disabledSetters.Setters.Add(disabledBackgroundColor);
			}

			if (viewType.IsAssignableFrom(typeof(ITextElement)))
			{
				var disabledTextColor = new Setter()
				{
					Property = TextElement.TextColorProperty,
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
				if (bindable == null)
				{
					var returnValue = new VisualStateGroupList() { visualStateGroup };
					_ = GetSetterFor(viewType, VisualStateManager.VisualStateGroupsProperty, out Style style);
					style.Setters.Add(new Setter() { Property = VisualStateManager.VisualStateGroupsProperty, Value = returnValue });
					return returnValue;
				}
				else
				{
					var returnValue = new VisualStateGroupList(true) { VisualElement = (VisualElement)bindable };
					returnValue.Add(visualStateGroup);
					return returnValue;
				}
			}
#endif
			return null;
		}


		class DefaultResourceDictionary : ResourceDictionary { }

		public static ResourceDictionary CreateDefaultResourceDictionary()
		{
			Type[] controls = new[]
			{
				typeof(Button)
			};

			var returnValue = new DefaultResourceDictionary();

			foreach (var control in controls)
			{
				var style = CreateStyle(control);
				if (style != null)
					returnValue.Add(style);
			}

			return returnValue;
		}

		public static Style CreateStyle(Type controlType)
		{
			var text = GetTextColor(controlType);
			var background = GetBackgroundColor(controlType);
			var vsm = GetVisualStateManager(controlType);

			var style = new Style(controlType);

			if (text != null)
				style.Setters.Add(text);

			if (background != null)
				style.Setters.Add(background);

			if (vsm != null)
				style.Setters.Add(new Setter() { Property = VisualStateManager.VisualStateGroupsProperty, Value = vsm });

			if (style.Setters.Count > 0)
				return style;

			return null;
		}
	}
}
