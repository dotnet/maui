using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms
{
	[ContentProperty("Value")]
	[ProvideCompiled("Xamarin.Forms.Core.XamlC.SetterValueProvider")]
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

			Func<MemberInfo> minforetriever =
				() =>
				{
					MemberInfo minfo = null;
					try
					{
						minfo = Property.DeclaringType.GetRuntimeProperty(Property.PropertyName);
					}
					catch (AmbiguousMatchException e)
					{
						throw new XamlParseException($"Multiple properties with name '{Property.DeclaringType}.{Property.PropertyName}' found.", serviceProvider, innerException: e);
					}
					if (minfo != null)
						return minfo;
					try
					{
						return Property.DeclaringType.GetRuntimeMethod("Get" + Property.PropertyName, new[] { typeof(BindableObject) });
					}
					catch (AmbiguousMatchException e)
					{
						throw new XamlParseException($"Multiple methods with name '{Property.DeclaringType}.Get{Property.PropertyName}' found.", serviceProvider, innerException: e);
					}
				};

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

			var dynamicResource = Value as DynamicResource;
			if (Value is BindingBase binding)
				targetObject.SetBinding(Property, binding.Clone(), fromStyle);
			else if (dynamicResource != null)
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
				targetObject.ClearValue(Property);
		}
	}
}