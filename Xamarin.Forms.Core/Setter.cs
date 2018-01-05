using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms
{
	[ContentProperty("Value")]
	[ProvideCompiled("Xamarin.Forms.Core.XamlC.SetterValueProvider")]
	public sealed class Setter : IValueProvider
	{
		readonly ConditionalWeakTable<BindableObject, object> _originalValues = new ConditionalWeakTable<BindableObject, object>();

		public BindableProperty Property { get; set; }

		public object Value { get; set; }

		object IValueProvider.ProvideValue(IServiceProvider serviceProvider)
		{
			if (Property == null)
			{
				var lineInfoProvider = serviceProvider.GetService(typeof(IXmlLineInfoProvider)) as IXmlLineInfoProvider;
				IXmlLineInfo lineInfo = lineInfoProvider != null ? lineInfoProvider.XmlLineInfo : new XmlLineInfo();
				throw new XamlParseException("Property not set", lineInfo);
			}
			var valueconverter = serviceProvider.GetService(typeof(IValueConverterProvider)) as IValueConverterProvider;

			Func<MemberInfo> minforetriever =
				() =>
				(MemberInfo)Property.DeclaringType.GetRuntimeProperty(Property.PropertyName) ?? (MemberInfo)Property.DeclaringType.GetRuntimeMethod("Get" + Property.PropertyName, new[] { typeof(BindableObject) });

			object value = valueconverter.Convert(Value, Property.ReturnType, minforetriever, serviceProvider);
			Value = value;
			return this;
		}

		internal void Apply(BindableObject target, bool fromStyle = false)
		{
			if (target == null)
				throw new ArgumentNullException("target");
			if (Property == null)
				return;

			object originalValue = target.GetValue(Property);
			if (!Equals(originalValue, Property.DefaultValue))
			{
				_originalValues.Remove(target);
				_originalValues.Add(target, originalValue);
			}

			var dynamicResource = Value as DynamicResource;
			var binding = Value as BindingBase;
			if (binding != null)
				target.SetBinding(Property, binding.Clone(), fromStyle);
			else if (dynamicResource != null)
				target.SetDynamicResource(Property, dynamicResource.Key, fromStyle);
			else
			{
				if (Value is IList<VisualStateGroup> visualStateGroupCollection)
					target.SetValue(Property, visualStateGroupCollection.Clone(), fromStyle);
				else
					target.SetValue(Property, Value, fromStyle);
			}
		}

		internal void UnApply(BindableObject target, bool fromStyle = false)
		{
			if (target == null)
				throw new ArgumentNullException(nameof(target));
			if (Property == null)
				return;

			object actual = target.GetValue(Property);
			if (!Equals(actual, Value) && !(Value is Binding) && !(Value is DynamicResource))
			{
				//Do not reset default value if the value has been changed
				_originalValues.Remove(target);
				return;
			}

			object defaultValue;
			if (_originalValues.TryGetValue(target, out defaultValue))
			{
				//reset default value, unapply bindings and dynamicResource
				target.SetValue(Property, defaultValue, fromStyle);
				_originalValues.Remove(target);
			}
			else
				target.ClearValue(Property);
		}
	}
}