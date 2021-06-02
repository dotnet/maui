using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls
{
	[ContentProperty(nameof(Value))]
	[ProvideCompiled("Microsoft.Maui.Controls.XamlC.SetterValueProvider")]
	public sealed class Setter : IValueProvider
	{
		readonly ConditionalWeakTable<BindableObject, object> _originalValues = new ConditionalWeakTable<BindableObject, object>();

		public string TargetName { get; set; }

		public BindableProperty Property { get; set; }

		public object Value { get; set; }

		object IValueProvider.ProvideValue(IServiceProvider serviceProvider)
		{
			if (Property == null)
				throw new XamlParseException("Property not set", serviceProvider);
			var valueconverter = serviceProvider.GetService(typeof(IValueConverterProvider)) as IValueConverterProvider;

			MemberInfo minforetriever() => (MemberInfo)Property.DeclaringType.GetRuntimeProperties().FirstOrDefault(pi => pi.Name == Property.PropertyName
																													   && pi.PropertyType == Property.ReturnType)
										?? (MemberInfo)Property.DeclaringType.GetRuntimeMethods().FirstOrDefault(mi => mi.Name == $"Get{Property.PropertyName}"
																													&& mi.ReturnParameter.ParameterType == Property.ReturnType
																													&& mi.GetParameters().Length == 1
																													&& mi.GetParameters()[0].ParameterType == typeof(BindableObject));

			object value = valueconverter.Convert(Value, Property.ReturnType, minforetriever, serviceProvider);
			Value = value;
			return this;
		}

		internal void Apply(BindableObject target, bool fromStyle = false)
		{
			if (target == null)
				throw new ArgumentNullException(nameof(target));

			var targetObject = target;

			if (!string.IsNullOrEmpty(TargetName) && target is Element element)
				targetObject = element.FindByName(TargetName) as BindableObject ?? throw new ArgumentNullException(nameof(targetObject));

			if (Property == null)
				return;

			object originalValue = targetObject.GetValue(Property);
			if (!Equals(originalValue, Property.DefaultValue))
			{
				_originalValues.Remove(targetObject);
				_originalValues.Add(targetObject, originalValue);
			}

			if (Value is BindingBase binding)
				targetObject.SetBinding(Property, binding.Clone(), fromStyle);
			else if (Value is DynamicResource dynamicResource)
				targetObject.SetDynamicResource(Property, dynamicResource.Key, fromStyle);
			else
			{
				if (Value is IList<VisualStateGroup> visualStateGroupCollection)
					targetObject.SetValue(Property, visualStateGroupCollection.Clone(), fromStyle);
				else
					targetObject.SetValue(Property, Value, fromStyle);
			}
		}

		internal void UnApply(BindableObject target, bool fromStyle = false)
		{
			if (target == null)
				throw new ArgumentNullException(nameof(target));

			var targetObject = target;

			if (!string.IsNullOrEmpty(TargetName) && target is Element element)
				targetObject = element.FindByName(TargetName) as BindableObject ?? throw new ArgumentNullException(nameof(targetObject));

			if (Property == null)
				return;

			object actual = targetObject.GetValue(Property);
			if (!Equals(actual, Value) && !(Value is Binding) && !(Value is DynamicResource))
			{
				//Do not reset default value if the value has been changed
				_originalValues.Remove(targetObject);
				return;
			}

			if (_originalValues.TryGetValue(targetObject, out object defaultValue))
			{
				//reset default value, unapply bindings and dynamicResource
				targetObject.SetValue(Property, defaultValue, fromStyle);
				_originalValues.Remove(targetObject);
			}
			else
				targetObject.ClearValue(Property, fromStyle);
		}
	}
}