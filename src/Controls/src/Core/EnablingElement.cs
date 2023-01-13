#nullable enable

namespace Microsoft.Maui.Controls.Internals
{
	class EnablingElement
	{
		public static readonly BindableProperty IsEnabledProperty = BindableProperty.Create("IsEnabled", typeof(bool),
			typeof(VisualElement), true, propertyChanged: OnIsEnabledPropertyChanged, coerceValue: CoerceIsEnabledProperty);

		static object CoerceIsEnabledProperty(BindableObject bindable, object value)
		{
			if (bindable is IEnablingElement visualElement)
			{
				visualElement.IsEnabledExplicit = (bool)value;
				return GetIsEnabledCore(visualElement);
			}

			return false;
		}

		static void OnIsEnabledPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is not VisualElement element)
				return;

			element.ChangeVisualState();

			(bindable as IPropertyPropagationController)?.PropagatePropertyChanged(VisualElement.IsEnabledProperty.PropertyName);
		}

		static bool GetIsEnabledCore(IEnablingElement enablingElement)
		{
			if (enablingElement.IsEnabledExplicit == false)
			{
				// If the explicitly set value is false, then nothing else matters
				// And we can save the effort of a Parent check
				return false;
			}

			if (enablingElement is Element element && element.Parent is IEnablingElement parentElement)
			{
				if (!(bool)element.Parent.GetValue(IsEnabledProperty))
					return false;
			}

			if (enablingElement is ICommandElement commandElement)
				return enablingElement.IsEnabledExplicit && CommandElement.GetCanExecute(commandElement);

			return enablingElement.IsEnabledExplicit;
		}

		public static void RefreshPropertyValue(BindableObject bo)
		{
			if (bo is not IEnablingElement enablingElement)
				return;

			var ctx = bo.GetContext(IsEnabledProperty);
			if (ctx?.Binding is not null)
			{
				// support bound properties
				if (!ctx.Attributes.HasFlag(BindableObject.BindableContextAttributes.IsBeingSet))
					ctx.Binding.Apply(false);
			}
			else
			{
				// support normal/code properties
				bo.SetValue(IsEnabledProperty, enablingElement.IsEnabledExplicit);
			}
		}
	}
}
