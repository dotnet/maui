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
	public sealed class Setter : IValueProvider
	{
		//GO AWAY
		readonly ConditionalWeakTable<BindableObject, object> _originalValues = new ConditionalWeakTable<BindableObject, object>();

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
				targetObject = element.FindByName(TargetName) as BindableObject ?? throw new XamlParseException($"Can not resole '{TargetName}' as Setter Target for '{target}'.");

			if (Property == null)
				return;

			//FIXME: use Specificity everywhere
			var fromStyle = specificity.Style > 0;
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
				targetObject = element.FindByName(TargetName) as BindableObject ?? throw new ArgumentNullException(nameof(targetObject));

			if (Property == null)
				return;

			targetObject.ClearValue(Property, specificity);
		}
	}

	//TODO: review this: merge FROM into a single int (vsm, manual, dynamicR, binding), and CSS into another
	internal readonly struct SetterSpecificity : IComparable<SetterSpecificity>
	{
		public static readonly SetterSpecificity DefaultValue = new(-1, 0, 0, 0, -1, 0, 0, 0);
		public static readonly SetterSpecificity VisualStateSetter = new SetterSpecificity(1, 0, 0, 0, 0, 0, 0, 0);
		public static readonly SetterSpecificity FromBinding = new SetterSpecificity(0, 0, 0, 1, 0, 0, 0, 0);

		public static readonly SetterSpecificity ManualValueSetter = new SetterSpecificity(0, 100, 0, 0, 0, 0, 0, 0);
		public static readonly SetterSpecificity Trigger = new SetterSpecificity(0, 101, 0, 0, 0, 0, 0, 0);

		public static readonly SetterSpecificity DynamicResourceSetter = new SetterSpecificity(0, 0, 1, 0, 0, 0, 0, 0);

		//handler always apply, but are removed when anything else comes in. see SetValueActual
		public static readonly SetterSpecificity FromHandler = new SetterSpecificity(1000, 0, 0, 0, 0, 0, 0, 0);

		//100-n: direct VSM (not from Style), n = max(99, distance between the RD and the target)
		public int Vsm { get; }

		//1: SetValue, SetBinding
		public int Manual { get; }

		//1: DynamicResource
		public int DynamicResource { get; }

		//1: SetBinding
		public int Binding { get; }

		//XAML Style specificty
		//100-n: implicit style, n = max(99, distance between the RD and the target)
		//200-n: RD Style, n = max(99, distance between the RD and the target)
		//200: local style, inline css,
		//300-n: VSM, n = max(99, distance between the RD and the target)
		//300: !important (not implemented)
		public int Style { get; } 

		//CSS Specificity, see https://developer.mozilla.org/en-US/docs/Web/CSS/Specificity
		public int Id { get; }
		public int Class { get; }
		public int Type { get;  }

		public SetterSpecificity(int vsm, int manual, int dynamicresource, int binding, int style, int id, int @class, int type)
		{
			Vsm = vsm;
			Manual = manual;
			DynamicResource = dynamicresource;
			Binding = binding;
			Style = style;
			Id = id;
			Class = @class;
			Type = type;
		}

		public SetterSpecificity(int style, int id, int @class, int type) : this(0, 0, 0,0 , style, id, @class, type)
		{
		}

		public int CompareTo(SetterSpecificity other)
		{
			//everything coming from Style has lower priority than something that does not
			if (Style != other.Style && Style == 0) return 1;
			if (Style != other.Style && other.Style == 0) return -1;
			if (Style != other.Style) return Style.CompareTo(other.Style);

			if (Vsm != other.Vsm) return Vsm.CompareTo(other.Vsm);
			if (Manual != other.Manual) return Manual.CompareTo(other.Manual);
			if (DynamicResource != other.DynamicResource) return DynamicResource.CompareTo(other.DynamicResource);
			if (Binding != other.Binding) return Binding.CompareTo(other.Binding);
			if (Id != other.Id) return Id.CompareTo(other.Id);
			if (Class != other.Class) return Class.CompareTo(other.Class);
			return Type.CompareTo(other.Type);
		}

		public override bool Equals(object obj) => Equals((SetterSpecificity)obj);

		bool Equals(SetterSpecificity other) => CompareTo(other) == 0;

		public override int GetHashCode() => (Vsm, Manual, DynamicResource, Binding, Style, Id, Class, Type).GetHashCode();

		public static bool operator ==(SetterSpecificity left, SetterSpecificity right) => left.Equals(right);
		public static bool operator !=(SetterSpecificity left, SetterSpecificity right) => !left.Equals(right);
	}
}