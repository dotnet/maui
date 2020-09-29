using System;
using Android.Views;
using AMenuItemCompat = AndroidX.Core.View.MenuItemCompat;
using AToolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace Xamarin.Forms.Platform.Android
{
	public static class AccessibilityExtensions
	{
		public static string SetContentDescription(this global::Android.Views.View Control, Element Element, string _defaultContentDescription = null)
		{
			if (Element == null)
				return _defaultContentDescription;

			if (_defaultContentDescription == null)
				_defaultContentDescription = Control.ContentDescription;

			var elemValue = ConcatenateNameAndHint(Element);

			if (!string.IsNullOrWhiteSpace(elemValue))
				Control.ContentDescription = elemValue;
			else
				Control.ContentDescription = _defaultContentDescription;

			return _defaultContentDescription;
		}

		public static bool? SetFocusable(this global::Android.Views.View Control, Element Element, bool? _defaultFocusable)
		{
			return Control.SetFocusable(Element, _defaultFocusable, null);
		}

		public static bool? SetFocusable(this global::Android.Views.View Control, Element Element, bool? _defaultFocusable = null, ImportantForAccessibility? _defaultImportantForAccessibility = null)
		{
			if (Element == null)
				return _defaultFocusable;

			if (!_defaultFocusable.HasValue)
			{
				_defaultFocusable = Control.Focusable;
			}
			if (!_defaultImportantForAccessibility.HasValue)
			{
				_defaultImportantForAccessibility = Control.ImportantForAccessibility;
			}

			bool? isInAccessibleTree = (bool?)Element.GetValue(AutomationProperties.IsInAccessibleTreeProperty);
			Control.Focusable = (bool)(isInAccessibleTree ?? _defaultFocusable);
			Control.ImportantForAccessibility = !isInAccessibleTree.HasValue ? (ImportantForAccessibility)_defaultImportantForAccessibility : (bool)isInAccessibleTree ? ImportantForAccessibility.Yes : ImportantForAccessibility.No;

			return _defaultFocusable;
		}

		public static string SetHint(this global::Android.Widget.TextView Control, Element Element, string _defaultHint)
		{
			if (Element == null)
				return _defaultHint;

			if (_defaultHint == null)
				_defaultHint = Control.Hint;

			var elemValue = ConcatenateNameAndHint(Element);

			if (!string.IsNullOrWhiteSpace(elemValue))
				Control.Hint = elemValue;
			else
				Control.Hint = _defaultHint;

			return _defaultHint;
		}

		public static void SetLabeledBy(this global::Android.Views.View Control, Element Element)
		{
			if (Element == null)
				return;

			var elemValue = (VisualElement)Element.GetValue(AutomationProperties.LabeledByProperty);

			if (elemValue != null)
			{
				var id = Control.Id;
				if (id == -1)
					id = Control.Id = Platform.GenerateViewId();

				var renderer = elemValue?.GetRenderer();
				renderer?.SetLabelFor(id);
			}
		}

		public static int? SetLabelFor(this global::Android.Views.View Control, int? id, int? _defaultLabelFor = null)
		{
			if (_defaultLabelFor == null)
				_defaultLabelFor = Control.LabelFor;

			Control.LabelFor = (int)(id ?? _defaultLabelFor);

			return _defaultLabelFor;
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
	}
}