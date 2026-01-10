#nullable disable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Sets a property value within a <see cref="Style"/> or <see cref="TriggerBase"/>.
	/// </summary>
	[ContentProperty(nameof(Value))]
	[ProvideCompiled("Microsoft.Maui.Controls.XamlC.SetterValueProvider")]
	[RequireService(
		[typeof(IValueConverterProvider),
		 typeof(IXmlLineInfoProvider)])]
	public sealed class Setter : IValueProvider
	{
		/// <summary>
		/// Gets or sets the name of the element to which the setter applies.
		/// </summary>
		public string TargetName { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="BindableProperty"/> to set.
		/// </summary>
		public BindableProperty Property { get; set; }

		/// <summary>
		/// Gets or sets the value to apply to the property.
		/// </summary>
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
				targetObject = element.FindByName(TargetName) as BindableObject ?? throw new XamlParseException($"Cannot resolve '{TargetName}' as Setter Target for '{target}'.");

			if (Property == null)
				return;

			if (Value is BindingBase binding)
				targetObject.SetBinding(Property, binding.Clone(), specificity);
			else if (Value is DynamicResource dynamicResource)
				targetObject.SetDynamicResource(Property, dynamicResource.Key, specificity);
			else if (Value is IList<VisualStateGroup> visualStateGroupCollection)
			{
				//Check if the target has already any visual states
				if (targetObject.GetValue(Property) is VisualStateGroupList parentVisualStateGroups)
				{
					visualStateGroupCollection.MergeWithParent(parentVisualStateGroups);
				}
				targetObject.SetValue(Property, visualStateGroupCollection.Clone(), specificity);
			}
			else
				targetObject.SetValue(Property, Value, specificity: specificity);
		}

		internal void UnApply(BindableObject target, SetterSpecificity specificity)
		{
			if (target == null)
				throw new ArgumentNullException(nameof(target));

			var targetObject = target;

			if (!string.IsNullOrEmpty(TargetName) && target is Element element)
				targetObject = element.FindByName(TargetName) as BindableObject ?? throw new ArgumentNullException(nameof(targetObject));

			if (Property == null)
				return;
			if (Value is BindingBase binding)
				targetObject.RemoveBinding(Property, specificity);
			else if (Value is DynamicResource dynamicResource)
				targetObject.RemoveDynamicResource(Property, specificity);
			targetObject.ClearValue(Property, specificity);
		}
	}
}