using System;
using Android.Views;
using AMenuItemCompat = AndroidX.Core.View.MenuItemCompat;
using AToolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	[Obsolete]
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

		public static void SetImportantForAccessibility(this global::Android.Views.View Control, Element Element, ImportantForAccessibility? _defaultImportantForAccessibility = null)
		{
			if (!_defaultImportantForAccessibility.HasValue)
			{
				_defaultImportantForAccessibility = Control.ImportantForAccessibility;
			}

			bool? isInAccessibleTree = (bool?)Element.GetValue(AutomationProperties.IsInAccessibleTreeProperty);
			Control.ImportantForAccessibility = !isInAccessibleTree.HasValue ? (ImportantForAccessibility)_defaultImportantForAccessibility : (bool)isInAccessibleTree ? ImportantForAccessibility.Yes : ImportantForAccessibility.No;

			bool? excludedWithChildren = (bool?)Element.GetValue(AutomationProperties.ExcludedWithChildrenProperty);
			if (excludedWithChildren == true)
				Control.ImportantForAccessibility = ImportantForAccessibility.NoHideDescendants;
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
