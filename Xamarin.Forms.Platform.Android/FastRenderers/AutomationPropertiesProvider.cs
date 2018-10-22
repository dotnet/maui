using System;
using System.ComponentModel;
using Android.Widget;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android.FastRenderers
{
	internal class AutomationPropertiesProvider : IDisposable 
	{
		static readonly string s_defaultDrawerId = "drawer";
		static readonly string s_defaultDrawerIdOpenSuffix = "_open";
		static readonly string s_defaultDrawerIdCloseSuffix = "_close";

		internal static void GetDrawerAccessibilityResources(global::Android.Content.Context context, MasterDetailPage page, out int resourceIdOpen, out int resourceIdClose)
		{
			resourceIdOpen = 0;
			resourceIdClose = 0;
			if (page == null)
				return;

			var automationIdParent = s_defaultDrawerId;
			if (!string.IsNullOrEmpty(page.Master?.Icon))
				automationIdParent = page.Master.Icon.AutomationId;
			else if (!string.IsNullOrEmpty(page.AutomationId))
				automationIdParent = page.AutomationId;

			resourceIdOpen = context.Resources.GetIdentifier($"{automationIdParent}{s_defaultDrawerIdOpenSuffix}", "string", context.ApplicationInfo.PackageName);
			resourceIdClose = context.Resources.GetIdentifier($"{automationIdParent}{s_defaultDrawerIdCloseSuffix}", "string", context.ApplicationInfo.PackageName);
		}

		internal static void SetAutomationId(AView control, VisualElement element, string value = null)
		{
			if (element == null || control == null)
			{
				return;
			}

			value = element.AutomationId;

			if (!string.IsNullOrEmpty(value))
			{
				control.ContentDescription = value;
			}
		}

		internal static void SetContentDescription(
			AView control, 
			VisualElement element, 
			ref string defaultContentDescription,
			ref string defaultHint)
		{
			if (element == null || control == null)
			{
				return;
			}

			if (SetHint(control, element, ref defaultHint))
			{
				return;
			}

			if (defaultContentDescription == null)
			{
				defaultContentDescription = control.ContentDescription;
			}

			string value = ConcatenateNameAndHelpText(element);

			if (!string.IsNullOrWhiteSpace(value))
			{
				control.ContentDescription = value;
			}
			else
			{
				control.ContentDescription = defaultContentDescription;
			}
		}

		internal static void SetFocusable(AView control, VisualElement element, ref bool? defaultFocusable)
		{
			if (element == null || control == null)
			{
				return;
			}

			if (!defaultFocusable.HasValue)
			{
				defaultFocusable = control.Focusable;
			}

			control.Focusable =
				(bool)((bool?)element.GetValue(AutomationProperties.IsInAccessibleTreeProperty) ?? defaultFocusable);
		}

		internal static void SetLabeledBy(AView control, VisualElement element)
		{
			if (element == null || control == null)
				return;

			var elemValue = (VisualElement)element.GetValue(AutomationProperties.LabeledByProperty);

			if (elemValue != null)
			{
				var id = control.Id;
				if (id == AView.NoId)
					id = control.Id = Platform.GenerateViewId();

				var renderer = elemValue?.GetRenderer();
				renderer?.SetLabelFor(id);
			}
		}

		static bool SetHint(AView Control, VisualElement Element, ref string defaultHint)
		{
			if (Element == null || Control == null)
			{
				return false;
			}

			var textView = Control as TextView;
			if (textView == null)
			{
				return false;
			}

			// Let the specified Title/Placeholder take precedence, but don't set the ContentDescription (won't work anyway)
			if (((Element as Picker)?.Title ?? (Element as Entry)?.Placeholder) != null)
			{
				return true;
			}

			if (defaultHint == null)
			{
				defaultHint = textView.Hint;
			}

			string value = ConcatenateNameAndHelpText(Element);

			textView.Hint = !string.IsNullOrWhiteSpace(value) ? value : defaultHint;

			return true;
		}

		string _defaultContentDescription;
		bool? _defaultFocusable;
		string _defaultHint;
		bool _disposed;

		IVisualElementRenderer _renderer;

		public AutomationPropertiesProvider(IVisualElementRenderer renderer)
		{
			_renderer = renderer;
			_renderer.ElementPropertyChanged += OnElementPropertyChanged;
			_renderer.ElementChanged += OnElementChanged;
		}

		AView Control => _renderer?.View;

		VisualElement Element => _renderer?.Element;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (_renderer != null)
			{
				_renderer.ElementChanged -= OnElementChanged;
				_renderer.ElementPropertyChanged -= OnElementPropertyChanged;

				_renderer = null;
			}
		}

		void SetAutomationId()
			=> SetAutomationId(Control, Element);

		void SetContentDescription()
			=> SetContentDescription(Control, Element, ref _defaultContentDescription, ref _defaultHint);

		void SetFocusable()
			=> SetFocusable(Control, Element, ref _defaultFocusable);

		bool SetHint()
			=> SetHint(Control, Element, ref _defaultHint);

		internal static string ConcatenateNameAndHelpText(Element Element)
		{
			var name = (string)Element.GetValue(AutomationProperties.NameProperty);
			var helpText = (string)Element.GetValue(AutomationProperties.HelpTextProperty);

			if (string.IsNullOrWhiteSpace(name))
				return helpText;
			if (string.IsNullOrWhiteSpace(helpText))
				return name;

			return $"{name}. {helpText}";
		}

		void SetLabeledBy()
			=> SetLabeledBy(Control, Element);

		void OnElementChanged(object sender, VisualElementChangedEventArgs e)
		{
			if (e.OldElement != null)
			{
				e.OldElement.PropertyChanged -= OnElementPropertyChanged;
			}

			if (e.NewElement != null)
			{
				e.NewElement.PropertyChanged += OnElementPropertyChanged;
			}

			SetHint();
			SetAutomationId();
			SetContentDescription();
			SetFocusable();
			SetLabeledBy();
		}

		void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == AutomationProperties.HelpTextProperty.PropertyName)
			{
				SetContentDescription();
			}
			else if (e.PropertyName == AutomationProperties.NameProperty.PropertyName)
			{
				SetContentDescription();
			}
			else if (e.PropertyName == AutomationProperties.IsInAccessibleTreeProperty.PropertyName)
			{
				SetFocusable();
			}
			else if (e.PropertyName == AutomationProperties.LabeledByProperty.PropertyName)
			{
				SetLabeledBy();
			}
		}
	}
}