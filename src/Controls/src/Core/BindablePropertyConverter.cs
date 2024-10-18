#nullable disable
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/BindablePropertyConverter.xml" path="Type[@FullName='Microsoft.Maui.Controls.BindablePropertyConverter']/Docs/*" />
	[Xaml.ProvideCompiled("Microsoft.Maui.Controls.XamlC.BindablePropertyConverter")]
	public sealed class BindablePropertyConverter : TypeConverter, IExtendedTypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> true;

		object IExtendedTypeConverter.ConvertFromInvariantString(string value, IServiceProvider serviceProvider)
		{
			if (string.IsNullOrWhiteSpace(value))
				return null;
			if (serviceProvider == null)
				return null;
			if (!(serviceProvider.GetService(typeof(IXamlTypeResolver)) is IXamlTypeResolver typeResolver))
				return null;
			IXmlLineInfo lineinfo = null;
			if (serviceProvider.GetService(typeof(IXmlLineInfoProvider)) is IXmlLineInfoProvider xmlLineInfoProvider)
				lineinfo = xmlLineInfoProvider.XmlLineInfo;
			string[] parts = value.Split('.');
			Type type = null;
			if (parts.Length == 1)
			{
				if (!(serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideParentValues parentValuesProvider))
				{
					string msg = string.Format("Can't resolve {0}", parts[0]);
					throw new XamlParseException(msg, lineinfo);
				}
				object parent = parentValuesProvider.ParentObjects.Skip(1).FirstOrDefault();
				if (parentValuesProvider.TargetObject is Setter)
				{
					if (parent is Style style)
						type = style.TargetType;
					else if (parent is TriggerBase triggerBase)
						type = triggerBase.TargetType;
					else if (parent is VisualState visualState)
						type = FindTypeForVisualState(parentValuesProvider, lineinfo);
				}
				else if (parentValuesProvider.TargetObject is Trigger)
					type = (parentValuesProvider.TargetObject as Trigger).TargetType;
				else if (parentValuesProvider.TargetObject is PropertyCondition && parent is TriggerBase)
					type = (parent as TriggerBase).TargetType;

				if (type == null)
					throw new XamlParseException($"Can't resolve {parts[0]}", lineinfo);

				return ConvertFrom(type, parts[0], lineinfo);
			}
			if (parts.Length == 2)
			{
				if (!typeResolver.TryResolve(parts[0], out type))
				{
					string msg = string.Format("Can't resolve {0}", parts[0]);
					throw new XamlParseException(msg, lineinfo);
				}
				return ConvertFrom(type, parts[1], lineinfo);
			}
			throw new XamlParseException($"Can't resolve {value}. Syntax is [[prefix:]Type.]PropertyName.", lineinfo);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var strValue = value?.ToString();

			if (string.IsNullOrWhiteSpace(strValue))
				return null;
			if (strValue.IndexOf(":", StringComparison.Ordinal) != -1)
			{
				Application.Current?.FindMauiContext()?.CreateLogger<BindablePropertyConverter>()?.LogWarning("Can't resolve properties with xml namespace prefix.");
				return null;
			}
			string[] parts = strValue.Split('.');
			if (parts.Length != 2)
			{
				Application.Current?.FindMauiContext()?.CreateLogger<BindablePropertyConverter>()?.LogWarning($"Can't resolve {value}. Accepted syntax is Type.PropertyName.");
				return null;
			}
			Type type = GetControlType(parts[0]);
			return ConvertFrom(type, parts[1], null);
		}

		BindableProperty ConvertFrom(Type type, string propertyName, IXmlLineInfo lineinfo)
		{
			var name = propertyName + "Property";
			FieldInfo bpinfo = GetPropertyField(type, name);
			if (bpinfo == null || bpinfo.FieldType != typeof(BindableProperty))
				throw new XamlParseException($"Can't resolve {name} on {type.Name}", lineinfo);
			var bp = bpinfo.GetValue(null) as BindableProperty;
			var isObsolete = GetObsoleteAttribute(bpinfo) != null;
			if (bp.PropertyName != propertyName && !isObsolete)
				throw new XamlParseException($"The PropertyName of {type.Name}.{name} is not {propertyName}", lineinfo);
			return bp;
		}

		[UnconditionalSuppressMessage("TrimAnalysis", "IL2045:AttributeRemoval",
			Justification = "ObsoleteAttribute instances are removed by the trimmer in production builds.")]
		static ObsoleteAttribute GetObsoleteAttribute(FieldInfo fieldInfo)
			=> fieldInfo.GetCustomAttribute<ObsoleteAttribute>();

		[UnconditionalSuppressMessage("TrimAnalysis", "IL2057:TypeGetType",
			Justification = "The converter is only used when parsing XAML at runtime. The developer will receive a warning " +
				"saying that parsing XAML at runtime may not work as expected when trimming.")]
		static Type GetControlType(string typeName)
			=> Type.GetType("Microsoft.Maui.Controls." + typeName);

		[UnconditionalSuppressMessage("TrimAnalysis", "IL2070:UnrecognizedReflectionPattern",
			Justification = "The converter is only used when parsing XAML at runtime. The developer will receive a warning " +
				"saying that parsing XAML at runtime may not work as expected when trimming.")]
		static FieldInfo GetPropertyField(Type type, string fieldName)
			=> type.GetField(fieldName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

		Type FindTypeForVisualState(IProvideParentValues parentValueProvider, IXmlLineInfo lineInfo)
		{
			var parents = parentValueProvider.ParentObjects.ToList();

			// Skip 0; we would not be making this check if TargetObject were not a Setter
			// Skip 1; we would not be making this check if the immediate parent were not a VisualState

			// VisualStates must be in a VisualStateGroup
			if (parents[2] is not VisualStateGroup)
				throw new XamlParseException($"Expected {nameof(VisualStateGroup)} but found {parents[2]}.", lineInfo);

			// Are these Visual States directly on a VisualElement?
			if (parents[3] is VisualElement vsTarget)
				return vsTarget.GetType();

			if (parents[3] is not VisualStateGroupList)
				throw new XamlParseException($"Expected {nameof(VisualStateGroupList)} but found {parents[3]}.", lineInfo);

			if (parents[4] is VisualElement veTarget)
				return veTarget.GetType();

			if (parents[4] is not Setter)
				throw new XamlParseException($"Expected {nameof(Setter)} but found {parents[4]}.", lineInfo);

			if (parents[5] is TriggerBase trigger)
				return trigger.TargetType;

			// These must be part of a Style; verify that 
			if (parents[5] is Style style)
				return style.TargetType;

			throw new XamlParseException($"Unable to find a TragetType for the Bindable Property. Try prefixing it with the TargetType.", lineInfo);

		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is not BindableProperty bp)
				throw new NotSupportedException();
			return $"{bp.DeclaringType.Name}.{bp.PropertyName}";
		}
	}
}
