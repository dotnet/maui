using System;
using System.ComponentModel;
using Android.Views;
using Android.Widget;
using AView = Android.Views.View;
using AMenuItemCompat = AndroidX.Core.View.MenuItemCompat;
using AToolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace Microsoft.Maui.Controls.Platform
{
	internal static class AutomationPropertiesProvider
	{
		static readonly string s_defaultDrawerId = "drawer";
		static readonly string s_defaultDrawerIdOpenSuffix = "_open";
		static readonly string s_defaultDrawerIdCloseSuffix = "_close";

		internal static void GetDrawerAccessibilityResources(global::Android.Content.Context context, FlyoutPage page, out int resourceIdOpen, out int resourceIdClose)
		{
			resourceIdOpen = 0;
			resourceIdClose = 0;
			if (page == null)
				return;

			var automationIdParent = s_defaultDrawerId;
			var icon = page.Flyout?.IconImageSource;
			if (icon != null && !icon.IsEmpty)
				automationIdParent = page.Flyout.IconImageSource.AutomationId;
			else if (!string.IsNullOrEmpty(page.AutomationId))
				automationIdParent = page.AutomationId;

			resourceIdOpen = context.Resources.GetIdentifier($"{automationIdParent}{s_defaultDrawerIdOpenSuffix}", "string", context.ApplicationInfo.PackageName);
			resourceIdClose = context.Resources.GetIdentifier($"{automationIdParent}{s_defaultDrawerIdCloseSuffix}", "string", context.ApplicationInfo.PackageName);
		}


		public static void SetTitleOrContentDescription(this IMenuItem Control, ToolbarItem Element)
		{
			SetTitleOrContentDescription(Control, (MenuItem)Element);
		}

		public static void SetTitleOrContentDescription(this IMenuItem Control, MenuItem Element)
		{
			if (Element == null)
				return;

			var elemValue = ConcatenateNameAndHint(Element);

			if (string.IsNullOrWhiteSpace(elemValue))
				elemValue = Element.AutomationId;
			else if (!String.IsNullOrEmpty(Element.Text))
				elemValue = String.Join(". ", Element.Text, elemValue);

			if (!string.IsNullOrWhiteSpace(elemValue))
				AMenuItemCompat.SetContentDescription(Control, elemValue);

		}


		public static string SetNavigationContentDescription(this AToolbar Control, Element Element, string _defaultNavigationContentDescription = null)
		{
			if (Element == null)
				return _defaultNavigationContentDescription;

			if (_defaultNavigationContentDescription == null)
				_defaultNavigationContentDescription = Control.NavigationContentDescription;

			var elemValue = ConcatenateNameAndHint(Element);

			if (!string.IsNullOrWhiteSpace(elemValue))
				Control.NavigationContentDescription = elemValue;
			else
				Control.NavigationContentDescription = _defaultNavigationContentDescription;

			return _defaultNavigationContentDescription;
		}

		static string ConcatenateNameAndHint(Element Element)
		{
			string separator;

			var name = (string)Element.GetValue(AutomationProperties.NameProperty);
			var hint = (string)Element.GetValue(AutomationProperties.HelpTextProperty);

			if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(hint))
				separator = "";
			else
				separator = ". ";

			return string.Join(separator, name, hint);
		}

		internal static void SetAutomationId(AView control, Element element, string value = null)
		{
			if (!control.IsAlive() || element == null)
			{
				return;
			}

			SetAutomationId(control, element.AutomationId, value);
		}

		internal static void SetAutomationId(AView control, string automationId, string value = null)
		{
			if (!control.IsAlive())
			{
				return;
			}

			automationId = value ?? automationId;
			if (!string.IsNullOrEmpty(automationId))
			{
				control.ContentDescription = automationId;
			}
		}

		internal static void SetBasicContentDescription(
			AView control,
			BindableObject bindableObject,
			string defaultContentDescription)
		{
			if (bindableObject == null || control == null)
				return;

			string value = ConcatenateNameAndHelpText(bindableObject);

			var contentDescription = !string.IsNullOrWhiteSpace(value) ? value : defaultContentDescription;

			if (String.IsNullOrWhiteSpace(contentDescription) && bindableObject is Element element)
				contentDescription = element.AutomationId;

			control.ContentDescription = contentDescription;
		}

		internal static void SetContentDescription(
			AView control,
			BindableObject element,
			string defaultContentDescription,
			string defaultHint)
		{
			if (element == null || control == null || SetHint(control, element, defaultHint))
				return;

			SetBasicContentDescription(control, element, defaultContentDescription);
		}

		internal static void SetFocusable(AView control, Element element, ref bool? defaultFocusable, ref ImportantForAccessibility? defaultImportantForAccessibility)
		{
			if (element == null || control == null)
			{
				return;
			}

			if (!defaultFocusable.HasValue)
			{
				defaultFocusable = control.Focusable;
			}
			if (!defaultImportantForAccessibility.HasValue)
			{
				defaultImportantForAccessibility = control.ImportantForAccessibility;
			}

			bool? isInAccessibleTree = (bool?)element.GetValue(AutomationProperties.IsInAccessibleTreeProperty);
			control.Focusable = (bool)(isInAccessibleTree ?? defaultFocusable);
			control.ImportantForAccessibility = !isInAccessibleTree.HasValue ? (ImportantForAccessibility)defaultImportantForAccessibility : (bool)isInAccessibleTree ? ImportantForAccessibility.Yes : ImportantForAccessibility.No;
		}

		// TODO MAUI
		// THIS probably isn't how we're going to set LabeledBy insied Maui
		internal static void SetLabeledBy(AView control, Element element)
		{
			if (element == null || control == null)
				return;

			var elemValue = (VisualElement)element.GetValue(AutomationProperties.LabeledByProperty);

			if (elemValue != null)
			{
				var id = control.Id;
				if (id == AView.NoId)
					id = control.Id = AView.GenerateViewId();

				// TODO MAUI
				// THIS probably isn't how we're going to set LabeledBy insied Maui
				//var renderer = elemValue?.GetRenderer();
				//renderer?.SetLabelFor(id);
			}
		}

		static bool SetHint(AView Control, BindableObject Element, string defaultHint)
		{
			if (Element == null || Control == null)
			{
				return false;
			}

			if (Element is Picker || Element is Button)
			{
				return false;
			}

			var textView = Control as TextView;
			if (textView == null)
			{
				return false;
			}

			// TODO: add EntryAccessibilityDelegate to Entry
			// Let the specified Placeholder take precedence, but don't set the ContentDescription (won't work anyway)
			if ((Element as Entry)?.Placeholder != null)
			{
				return true;
			}

			string value = ConcatenateNameAndHelpText(Element);

			textView.Hint = !string.IsNullOrWhiteSpace(value) ? value : defaultHint;

			return true;
		}

		internal static void AccessibilitySettingsChanged(AView control, Element element, string _defaultHint, string _defaultContentDescription, ref bool? _defaultFocusable, ref ImportantForAccessibility? _defaultImportantForAccessibility)
		{
			SetHint(control, element, _defaultHint);
			SetAutomationId(control, element);
			SetContentDescription(control, element, _defaultContentDescription, _defaultHint);
			SetFocusable(control, element, ref _defaultFocusable, ref _defaultImportantForAccessibility);
			SetLabeledBy(control, element);
		}

		internal static void AccessibilitySettingsChanged(AView control, Element element)
		{
			string _defaultHint = String.Empty;
			string _defaultContentDescription = String.Empty;
			bool? _defaultFocusable = null;
			ImportantForAccessibility? _defaultImportantForAccessibility = null;
			AccessibilitySettingsChanged(control, element, _defaultHint, _defaultContentDescription, ref _defaultFocusable, ref _defaultImportantForAccessibility);
		}


		internal static string ConcatenateNameAndHelpText(BindableObject Element)
		{
			var name = (string)Element.GetValue(AutomationProperties.NameProperty);
			var helpText = (string)Element.GetValue(AutomationProperties.HelpTextProperty);

			if (string.IsNullOrWhiteSpace(name))
				return helpText;
			if (string.IsNullOrWhiteSpace(helpText))
				return name;

			return $"{name}. {helpText}";
		}

		internal static void SetupDefaults(AView control, ref string defaultContentDescription)
		{
			string hint = null;
			SetupDefaults(control, ref defaultContentDescription, ref hint);
		}

		internal static void SetupDefaults(AView control, ref string defaultContentDescription, ref string defaultHint)
		{
			if (defaultContentDescription == null)
				defaultContentDescription = control.ContentDescription;

			if (control is TextView textView && defaultHint == null)
			{
				defaultHint = textView.Hint;
			}
		}
	}
}