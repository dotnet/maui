#nullable disable
using System.ComponentModel;
using System.Xml.Linq;
using Microsoft.Maui.Controls.StyleSheets;

namespace Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific
{
	using static Microsoft.Maui.ApplicationModel.Permissions;
	using FormsElement = Maui.Controls.VisualElement;

	/// <summary>
	/// Provides access to focus order, styles, and tooltips for visual elements on the Tizen platform.
	/// </summary>
	public static class VisualElement
	{
		/// <summary>Bindable property for <see cref="Style"/>.</summary>
		public static readonly BindableProperty StyleProperty = BindableProperty.Create("ThemeStyle", typeof(string), typeof(VisualElement), default(string));

		/// <summary>Bindable property for attached property <c>IsFocusAllowed</c>.</summary>
		public static readonly BindableProperty IsFocusAllowedProperty = BindableProperty.Create("IsFocusAllowed", typeof(bool?), typeof(VisualElement), null);

		/// <summary>Bindable property for attached property <c>NextFocusDirection</c>.</summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static readonly BindableProperty NextFocusDirectionProperty = BindableProperty.Create("NextFocusDirection", typeof(string), typeof(VisualElement), FocusDirection.None, propertyChanged: OnNextFocusDirectionPropertyChanged);

		/// <summary>Bindable property for attached property <c>NextFocusUpView</c>.</summary>
		public static readonly BindableProperty NextFocusUpViewProperty = BindableProperty.Create("NextFocusUpView", typeof(View), typeof(VisualElement), default(View));

		/// <summary>Bindable property for attached property <c>NextFocusDownView</c>.</summary>
		public static readonly BindableProperty NextFocusDownViewProperty = BindableProperty.Create("NextFocusDownView", typeof(View), typeof(VisualElement), default(View));

		/// <summary>Bindable property for attached property <c>NextFocusLeftView</c>.</summary>
		public static readonly BindableProperty NextFocusLeftViewProperty = BindableProperty.Create("NextFocusLeftView", typeof(View), typeof(VisualElement), default(View));

		/// <summary>Bindable property for attached property <c>NextFocusRightView</c>.</summary>
		public static readonly BindableProperty NextFocusRightViewProperty = BindableProperty.Create("NextFocusRightView", typeof(View), typeof(VisualElement), default(View));

		/// <summary>Bindable property for attached property <c>NextFocusBackView</c>.</summary>
		public static readonly BindableProperty NextFocusBackViewProperty = BindableProperty.Create("NextFocusBackView", typeof(View), typeof(VisualElement), default(View));

		/// <summary>Bindable property for attached property <c>NextFocusForwardView</c>.</summary>
		public static readonly BindableProperty NextFocusForwardViewProperty = BindableProperty.Create("NextFocusForwardView", typeof(View), typeof(VisualElement), default(View));

		/// <summary>Bindable property for <see cref="ToolTip"/>.</summary>
		public static readonly BindableProperty ToolTipProperty = BindableProperty.Create("ToolTip", typeof(string), typeof(VisualElement), default(string));

		/// <summary>
		/// Returns the style for the element.
		/// </summary>
		/// <param name="element">The visual element whose style to get.</param>
		/// <returns>The style for the element.</returns>
		public static string GetStyle(BindableObject element)
		{
			return (string)element.GetValue(StyleProperty);
		}

		/// <summary>
		/// Sets the style on a visual element.
		/// </summary>
		/// <param name="element">The visual element whose style to set.</param>
		/// <param name="value">The new style value.</param>
		public static void SetStyle(BindableObject element, string value)
		{
			element.SetValue(StyleProperty, value);
		}

		/// <summary>
		/// The platform configuration for the visual element whose style to get.
		/// </summary>
		/// <param name="config">The platform configuration for the visual element whose style to get.</param>
		/// <returns>The style for the element.</returns>
		public static string GetStyle(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetStyle(config.Element);
		}

		/// <summary>
		/// Sets the style on a visual element.
		/// </summary>
		/// <param name="config">The platform configuration for the visual element whose style to set.</param>
		/// <param name="value">The new style value.</param>
		/// <returns>A fluent object on which the developer may make further method calls.</returns>
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetStyle(this IPlatformElementConfiguration<Tizen, FormsElement> config, string value)
		{
			SetStyle(config.Element, value);
			return config;
		}

		/// <summary>
		/// Returns <see langword="true" /> if the element may be focused. Otherwise, returns <see langword="false" />.
		/// </summary>
		/// <param name="element">The visual element whose focusability to check.</param>
		/// <returns><see langword="true" /> if the element may be focused. Otherwise, <see langword="false" />.</returns>
		public static bool? IsFocusAllowed(BindableObject element)
		{
			return (bool?)element.GetValue(IsFocusAllowedProperty);
		}

		/// <summary>
		/// Sets the focus participation value for a visual element.
		/// </summary>
		/// <param name="element">The element whose focus participation value to set.</param>
		/// <param name="value">The new focus participation value.</param>
		public static void SetFocusAllowed(BindableObject element, bool value)
		{
			element.SetValue(IsFocusAllowedProperty, value);
		}

		/// <summary>
		/// Returns <see langword="true" /> if the element may be focused. Otherwise, returns <see langword="false" />.
		/// </summary>
		/// <param name="config">The platform configuration for the visual element whose focusability to check.</param>
		/// <returns><see langword="true" /> if the element may be focused. Otherwise, <see langword="false" />.</returns>
		public static bool? IsFocusAllowed(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return IsFocusAllowed(config.Element);
		}

		/// <summary>
		/// Sets the focus participation value for a visual element.
		/// </summary>
		/// <param name="config">The platform configuration for the element whose focus participation value to set.</param>
		/// <param name="value">The new focus participation value.</param>
		/// <returns>A fluent object on which the developer may make further method calls.</returns>
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetFocusAllowed(this IPlatformElementConfiguration<Tizen, FormsElement> config, bool value)
		{
			SetFocusAllowed(config.Element, value);
			return config;
		}

		/// <summary>
		/// Returns the next focus direction.
		/// </summary>
		/// <param name="element">The visual element whose next focus direction to get.</param>
		/// <returns>The next focus direction.</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static string GetNextFocusDirection(BindableObject element)
		{
			return (string)element.GetValue(NextFocusDirectionProperty);
		}

		/// <summary>
		/// The visual element whose next focus direction to set.
		/// </summary>
		/// <param name="element">Sets the direction of the next focus on a visual element.</param>
		/// <param name="value">The new focus direction.</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void SetNextFocusDirection(BindableObject element, string value)
		{
			element.SetValue(NextFocusDirectionProperty, value);
		}

		/// <summary>
		/// Returns the next focus direction.
		/// </summary>
		/// <param name="config">The platform configuration for the visual element whose next focus direction to get.</param>
		/// <returns>The next focus direction.</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static string GetNextFocusDirection(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetNextFocusDirection(config.Element);
		}

		/// <summary>
		/// Sets the direction of the next focus on a visual element.
		/// </summary>
		/// <param name="config">The platform configuration for the visual element whose next focus direction to set.</param>
		/// <param name="value">The new focus direction.</param>
		/// <returns>A fluent object on which the developer may make further method calls.</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetNextFocusDirection(this IPlatformElementConfiguration<Tizen, FormsElement> config, string value)
		{
			SetNextFocusDirection(config.Element, value);
			return config;
		}

		/// <summary>
		/// Changes the focus direction to up.
		/// </summary>
		/// <param name="config">The platform configuration for the visual element whose focus direction to set.</param>
		/// <returns>A fluent object on which the developer may make further method calls.</returns>
		public static IPlatformElementConfiguration<Tizen, FormsElement> MoveFocusUp(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			SetNextFocusDirection(config.Element, FocusDirection.Up);
			return config;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="config">The platform configuration for the visual element whose focus direction to set.</param>
		/// <returns>A fluent object on which the developer may make further method calls.</returns>
		public static IPlatformElementConfiguration<Tizen, FormsElement> MoveFocusDown(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			SetNextFocusDirection(config.Element, FocusDirection.Down);
			return config;
		}

		/// <summary>
		/// Changes the focus direction to left.
		/// </summary>
		/// <param name="config">The platform configuration for the visual element whose focus direction to set.</param>
		/// <returns>A fluent object on which the developer may make further method calls.</returns>
		public static IPlatformElementConfiguration<Tizen, FormsElement> MoveFocusLeft(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			SetNextFocusDirection(config.Element, FocusDirection.Left);
			return config;
		}

		/// <summary>
		/// Changes the focus direction to right.
		/// </summary>
		/// <param name="config">The platform configuration for the visual element whose focus direction to set.</param>
		/// <returns>A fluent object on which the developer may make further method calls.</returns>
		public static IPlatformElementConfiguration<Tizen, FormsElement> MoveFocusRight(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			SetNextFocusDirection(config.Element, FocusDirection.Right);
			return config;
		}

		/// <summary>
		/// Changes the focus direction to back.
		/// </summary>
		/// <param name="config">The platform configuration for the visual element whose focus direction to set.</param>
		/// <returns>A fluent object on which the developer may make further method calls.</returns>
		public static IPlatformElementConfiguration<Tizen, FormsElement> MoveFocusBack(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			SetNextFocusDirection(config.Element, FocusDirection.Back);
			return config;
		}

		/// <summary>
		/// Changes the focus direction to forward.
		/// </summary>
		/// <param name="config">The platform configuration for the visual element whose focus direction to set.</param>
		/// <returns>A fluent object on which the developer may make further method calls.</returns>
		public static IPlatformElementConfiguration<Tizen, FormsElement> MoveFocusForward(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			SetNextFocusDirection(config.Element, FocusDirection.Forward);
			return config;
		}

		/// <summary>
		/// Returns the view that gets the focus when moving up.
		/// </summary>
		/// <param name="element">The visual element whose next focus to get.</param>
		/// <returns>The view that gets the focus when moving up.</returns>
		public static View GetNextFocusUpView(BindableObject element)
		{
			return (View)element.GetValue(NextFocusUpViewProperty);
		}

		/// <summary>
		/// Sets the up focus view for a visual element.
		/// </summary>
		/// <param name="element">The element whose up focus view to set.</param>
		/// <param name="value">The new up focus view.</param>
		public static void SetNextFocusUpView(BindableObject element, View value)
		{
			element.SetValue(NextFocusUpViewProperty, value);
		}

		/// <summary>
		/// Returns the view that gets the focus when moving up.
		/// </summary>
		/// <param name="config">The platform configuration for the visual element whose next focus to get.</param>
		/// <returns>The view that gets the focus when moving up.</returns>
		public static View GetNextFocusUpView(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetNextFocusUpView(config.Element);
		}

		/// <summary>
		/// Sets the up focus view for a visual element.
		/// </summary>
		/// <param name="config">The platform configuration for the element whose up focus view to set.</param>
		/// <param name="value">The new up focus view.</param>
		/// <returns>A fluent object on which the developer may make further method calls.</returns>
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetNextFocusUpView(this IPlatformElementConfiguration<Tizen, FormsElement> config, View value)
		{
			SetNextFocusUpView(config.Element, value);
			return config;
		}

		/// <summary>
		/// Returns the view that gets the focus when moving down.
		/// </summary>
		/// <param name="element">The visual element whose next focus to get.</param>
		/// <returns>The view that gets the focus when moving down.</returns>
		public static View GetNextFocusDownView(BindableObject element)
		{
			return (View)element.GetValue(NextFocusDownViewProperty);
		}

		/// <summary>
		/// Sets the down focus view for a visual element.
		/// </summary>
		/// <param name="element">The element whose down focus view to set.</param>
		/// <param name="value">The new down focus view.</param>
		public static void SetNextFocusDownView(BindableObject element, View value)
		{
			element.SetValue(NextFocusDownViewProperty, value);
		}

		/// <summary>
		/// Returns the view that gets the focus when moving down.
		/// </summary>
		/// <param name="config">The platform configuration for the visual element whose next focus to get.</param>
		/// <returns>The view that gets the focus when moving down.</returns>
		public static View GetNextFocusDownView(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetNextFocusDownView(config.Element);
		}

		/// <summary>
		/// Sets the down focus view for a visual element.
		/// </summary>
		/// <param name="config">The platform configuration for the element whose down focus view to set.</param>
		/// <param name="value">The new down focus view.</param>
		/// <returns>A fluent object on which the developer may make further method calls.</returns>
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetNextFocusDownView(this IPlatformElementConfiguration<Tizen, FormsElement> config, View value)
		{
			SetNextFocusDownView(config.Element, value);
			return config;
		}

		/// <summary>
		/// Returns the view that gets the focus when moving left.
		/// </summary>
		/// <param name="element">The element whose left focus view to get.</param>
		/// <returns>The view that gets the focus when moving left.</returns>
		public static View GetNextFocusLeftView(BindableObject element)
		{
			return (View)element.GetValue(NextFocusLeftViewProperty);
		}

		/// <summary>
		/// Sets the left focus view for a visual element.
		/// </summary>
		/// <param name="element">The element whose left focus view to set.</param>
		/// <param name="value">The new left focus view.</param>
		public static void SetNextFocusLeftView(BindableObject element, View value)
		{
			element.SetValue(NextFocusLeftViewProperty, value);
		}

		/// <summary>
		/// Returns the view that gets the focus when moving left.
		/// </summary>
		/// <param name="config">The platform configuration for the element whose left focus view to get.</param>
		/// <returns>The view that gets the focus when moving left.</returns>
		public static View GetNextFocusLeftView(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetNextFocusLeftView(config.Element);
		}

		/// <summary>
		/// Sets the left focus view for a visual element.
		/// </summary>
		/// <param name="config">The platform configuration for the element whose left focus view to set.</param>
		/// <param name="value">The new left focus view.</param>
		/// <returns>A fluent object on which the developer may make further method calls.</returns>
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetNextFocusLeftView(this IPlatformElementConfiguration<Tizen, FormsElement> config, View value)
		{
			SetNextFocusLeftView(config.Element, value);
			return config;
		}

		/// <summary>
		/// Returns the view that gets the focus when moving right.
		/// </summary>
		/// <param name="element">The visual element whose next focus to get.</param>
		/// <returns>The view that gets the focus when moving right.</returns>
		public static View GetNextFocusRightView(BindableObject element)
		{
			return (View)element.GetValue(NextFocusRightViewProperty);
		}

		/// <summary>
		/// Sets the right focus view for a visual element.
		/// </summary>
		/// <param name="element">The element whose right focus view to set.</param>
		/// <param name="value">The new right focus view.</param>
		public static void SetNextFocusRightView(BindableObject element, View value)
		{
			element.SetValue(NextFocusRightViewProperty, value);
		}

		/// <summary>
		/// Returns the view that gets the focus when moving right.
		/// </summary>
		/// <param name="config">The platform configuration for the visual element whose next focus to get.</param>
		/// <returns>The view that gets the focus when moving right.</returns>
		public static View GetNextFocusRightView(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetNextFocusRightView(config.Element);
		}

		/// <summary>
		/// Sets the right focus view for a visual element.
		/// </summary>
		/// <param name="config">The platform configuration for the element whose right focus view to set.</param>
		/// <param name="value">The new right focus view.</param>
		/// <returns>A fluent object on which the developer may make further method calls.</returns>
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetNextFocusRightView(this IPlatformElementConfiguration<Tizen, FormsElement> config, View value)
		{
			SetNextFocusRightView(config.Element, value);
			return config;
		}

		/// <summary>
		/// Returns the view that gets the focus when moving back.
		/// </summary>
		/// <param name="element">The visual element whose next focus to get.</param>
		/// <returns>The view that gets the focus when moving back.</returns>
		public static View GetNextFocusBackView(BindableObject element)
		{
			return (View)element.GetValue(NextFocusBackViewProperty);
		}

		/// <summary>
		/// Sets the back focus view for a visual element.
		/// </summary>
		/// <param name="element">The element whose back focus view to set.</param>
		/// <param name="value">The new back focus view.</param>
		public static void SetNextFocusBackView(BindableObject element, View value)
		{
			element.SetValue(NextFocusBackViewProperty, value);
		}

		/// <summary>
		/// Returns the view that gets the focus when moving back.
		/// </summary>
		/// <param name="config">The platform configuration for the visual element whose next focus to get.</param>
		/// <returns>The view that gets the focus when moving back.</returns>
		public static View GetNextFocusBackView(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetNextFocusBackView(config.Element);
		}

		/// <summary>
		/// Sets the back focus view for a visual element.
		/// </summary>
		/// <param name="config">The platform configuration for the element whose back focus view to set.</param>
		/// <param name="value">The new back focus view.</param>
		/// <returns>A fluent object on which the developer may make further method calls.</returns>
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetNextFocusBackView(this IPlatformElementConfiguration<Tizen, FormsElement> config, View value)
		{
			SetNextFocusBackView(config.Element, value);
			return config;
		}

		/// <summary>
		/// Returns the view that gets the focus when moving forward.
		/// </summary>
		/// <param name="element">The visual element whose next focus to get.</param>
		/// <returns>The view that gets the focus when moving forward.</returns>
		public static View GetNextFocusForwardView(BindableObject element)
		{
			return (View)element.GetValue(NextFocusForwardViewProperty);
		}

		/// <summary>
		/// Sets the forward focus view for a visual element.
		/// </summary>
		/// <param name="element">The element whose forward focus view to set.</param>
		/// <param name="value">The new forward focus view.</param>
		public static void SetNextFocusForwardView(BindableObject element, View value)
		{
			element.SetValue(NextFocusForwardViewProperty, value);
		}

		/// <summary>
		/// Returns the view that gets the focus when moving forward.
		/// </summary>
		/// <param name="config">The platform configuration for the visual element whose next focus to get.</param>
		/// <returns>The view that gets the focus when moving forward.</returns>
		public static View GetNextFocusForwardView(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetNextFocusForwardView(config.Element);
		}

		/// <summary>
		/// Sets the forward focus view for a visual element.
		/// </summary>
		/// <param name="config">The platform configuration for the element whose forward focus view to set.</param>
		/// <param name="value">The new forward focus view.</param>
		/// <returns>A fluent object on which the developer may make further method calls.</returns>
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetNextFocusForwardView(this IPlatformElementConfiguration<Tizen, FormsElement> config, View value)
		{
			SetNextFocusForwardView(config.Element, value);
			return config;
		}

		static void OnNextFocusDirectionPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			bindable.SetValue(NextFocusDirectionProperty, FocusDirection.None);
		}

		/// <summary>
		/// Returns the tooltip for the element.
		/// </summary>
		/// <param name="element">The visual element whose tooltip to get.</param>
		/// <returns>The tooltip text for the element.</returns>
		public static string GetToolTip(BindableObject element)
		{
			return (string)element.GetValue(ToolTipProperty);
		}

		/// <summary>
		/// Sets the tooltip on a visual element.
		/// </summary>
		/// <param name="element">The visual element whose tooltip to set.</param>
		/// <param name="value">The new tooltip value.</param>
		public static void SetToolTip(BindableObject element, string value)
		{
			element.SetValue(ToolTipProperty, value);
		}

		/// <summary>
		/// Returns the tooltip for the element.
		/// </summary>
		/// <param name="config">The platform configuration for the visual element whose tooltip to get.</param>
		/// <returns>The tooltip text for the element.</returns>
		public static string GetToolTip(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetToolTip(config.Element);
		}

		/// <summary>
		/// Sets the tooltip on a visual element.
		/// </summary>
		/// <param name="config">The platform configuration for the visual element whose tooltip to set.</param>
		/// <param name="value">The new tooltip value.</param>
		/// <returns>A fluent object on which the developer may make further method calls.</returns>
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetToolTip(this IPlatformElementConfiguration<Tizen, FormsElement> config, string value)
		{
			SetToolTip(config.Element, value);
			return config;
		}
	}
}
