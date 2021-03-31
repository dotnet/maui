using System;

namespace Microsoft.Maui.Controls
{
	public class SemanticProperties
	{
		public static readonly BindableProperty DescriptionProperty = BindableProperty.CreateAttached("Description", typeof(string), typeof(SemanticProperties), default(string), propertyChanged : OnHintPropertyChanged);

		public static readonly BindableProperty HintProperty = BindableProperty.CreateAttached("Hint", typeof(string), typeof(SemanticProperties), default(string), propertyChanged: OnDescriptionPropertyChanged);

		static void OnDescriptionPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			SetupSemanticsProperty(bindable)
				.Description = SemanticProperties.GetHint(bindable);

			UpdateSemanticsProperty(bindable);
		}

		static void OnHintPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			SetupSemanticsProperty(bindable)
				.Hint = SemanticProperties.GetHint(bindable);

			UpdateSemanticsProperty(bindable);
		}

		static Semantics SetupSemanticsProperty(BindableObject bindable)
		{
			VisualElement ve = (VisualElement)bindable;
			Semantics semantics;
			ve.Semantics = semantics = ve.Semantics ?? new Semantics();
			return ve.Semantics;
		}

		static void UpdateSemanticsProperty(BindableObject bindable)
		{
			if (bindable is IFrameworkElement fe)
				fe.Handler?.UpdateValue(nameof(IView.Semantics));
		}

		public static string GetDescription(BindableObject bindable)
		{
			return (string)bindable.GetValue(DescriptionProperty);
		}

		public static void SetDescription(BindableObject bindable, string value)
		{
			bindable.SetValue(DescriptionProperty, value);
		}

		public static string GetHint(BindableObject bindable)
		{
			return (string)bindable.GetValue(HintProperty);
		}

		public static void SetHint(BindableObject bindable, string value)
		{
			bindable.SetValue(HintProperty, value);
		}
	}
}
