#nullable disable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/Setter.xml" path="Type[@FullName='Microsoft.Maui.Controls.Setter']/Docs/*" />
	[ContentProperty(nameof(Value))]
	[ProvideCompiled("Microsoft.Maui.Controls.XamlC.SetterValueProvider")]
	[RequireService(
		[typeof(IValueConverterProvider),
		 typeof(IXmlLineInfoProvider)])]
	public sealed class Setter : IValueProvider
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/Setter.xml" path="//Member[@MemberName='TargetName']/Docs/*" />
		public string TargetName { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/Setter.xml" path="//Member[@MemberName='Property']/Docs/*" />
		public BindableProperty Property { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/Setter.xml" path="//Member[@MemberName='Value']/Docs/*" />
		public object Value { get; set; }

		object IValueProvider.ProvideValue(IServiceProvider serviceProvider)
		{
			if (Property == null)
				throw new XamlParseException("Property not set", serviceProvider);
			var valueconverter = serviceProvider.GetService(typeof(IValueConverterProvider)) as IValueConverterProvider;

			MemberInfo minforetriever()
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
			}

			object value = valueconverter.Convert(Value, Property.ReturnType, minforetriever, serviceProvider);
			Value = value;
			return this;
		}

		internal void Apply(BindableObject target, SetterSpecificity specificity)
		{
			if (target == null)
				throw new ArgumentNullException(nameof(target));

			var targetObject = target;

			if (!string.IsNullOrEmpty(TargetName) && target is Element element)
				targetObject = FindTargetByName(element, TargetName) ?? throw new XamlParseException($"Cannot resolve '{TargetName}' as Setter Target for '{target}'.");

			if (Property == null)
				return;

			if (Value is BindingBase binding)
				targetObject.SetBinding(Property, binding.Clone(), specificity);
			else if (Value is DynamicResource dynamicResource)
				targetObject.SetDynamicResource(Property, dynamicResource.Key, specificity);
			else if (Value is IList<VisualStateGroup> visualStateGroupCollection)
				targetObject.SetValue(Property, visualStateGroupCollection.Clone(), specificity);
			else
				targetObject.SetValue(Property, Value, specificity: specificity);
		}

		internal void UnApply(BindableObject target, SetterSpecificity specificity)
		{
			if (target == null)
				throw new ArgumentNullException(nameof(target));

			var targetObject = target;

			if (!string.IsNullOrEmpty(TargetName) && target is Element element)
				targetObject = FindTargetByName(element, TargetName) ?? throw new ArgumentNullException(nameof(targetObject));

			if (Property == null)
				return;
			if (Value is BindingBase binding)
				targetObject.RemoveBinding(Property, specificity);
			else if (Value is DynamicResource dynamicResource)
				targetObject.RemoveDynamicResource(Property, specificity);
			targetObject.ClearValue(Property, specificity);
		}

		static BindableObject FindTargetByName(Element element, string name)
		{
			// Try standard lookup first (works for same or child namescopes)
			if (element.FindByName(name) is BindableObject target)
				return target;

			// Walk up parent tree to handle ControlTemplate namescope boundaries
			var current = element.Parent;
			while (current != null)
			{
				var namescope = current.GetNameScope();
				if (namescope?.FindByName(name) is BindableObject parentTarget)
					return parentTarget;

				current = current.Parent;
			}

			return null;
		}
	}
}