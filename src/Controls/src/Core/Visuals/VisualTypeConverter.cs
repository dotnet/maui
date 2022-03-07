using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/VisualTypeConverter.xml" path="Type[@FullName='Microsoft.Maui.Controls.VisualTypeConverter']/Docs" />
	public class VisualTypeConverter : TypeConverter
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls/VisualTypeConverter.xml" path="//Member[@MemberName='CanConvertFrom']/Docs" />
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);

		/// <include file="../../../docs/Microsoft.Maui.Controls/VisualTypeConverter.xml" path="//Member[@MemberName='CanConvertTo']/Docs" />
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(string);

		static Dictionary<string, IVisual> _visualTypeMappings;
		void InitMappings()
		{
			var mappings = new Dictionary<string, IVisual>(StringComparer.OrdinalIgnoreCase);
			foreach (var visual in Internals.Registrar.VisualTypes)
				Register(visual, mappings);

			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

			// Check for visual assembly attributes	after scanning for IVisual Types
			// this will let users replace the default visual names if they want to
			foreach (var assembly in assemblies)
				RegisterFromAttributes(assembly, mappings);

			if (Internals.Registrar.ExtraAssemblies != null)
				foreach (var assembly in Internals.Registrar.ExtraAssemblies)
					RegisterFromAttributes(assembly, mappings);

			_visualTypeMappings = mappings;
		}

		static void RegisterFromAttributes(Assembly assembly, Dictionary<string, IVisual> mappings)
		{
			object[] attributes = assembly.GetCustomAttributesSafe(typeof(VisualAttribute));

			if (attributes != null)
			{
				foreach (VisualAttribute attribute in attributes)
				{
					var visual = CreateVisual(attribute.Visual);
					if (visual != null)
						mappings[attribute.Key] = visual;
				}
			}
		}

		static void Register(
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type visual,
			Dictionary<string, IVisual> mappings)
		{
			IVisual registeredVisual = CreateVisual(visual);
			if (registeredVisual == null)
				return;

			string name = visual.Name;
			string fullName = visual.FullName;

			if (name.EndsWith("Visual", StringComparison.OrdinalIgnoreCase))
			{
				name = name.Substring(0, name.Length - 6);
				fullName = fullName.Substring(0, fullName.Length - 6);
			}

			mappings[name] = registeredVisual;
			mappings[fullName] = registeredVisual;
			mappings[$"{name}Visual"] = registeredVisual;
			mappings[$"{fullName}Visual"] = registeredVisual;
		}

		static IVisual CreateVisual(
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type visualType)
		{
			try
			{
				return (IVisual)Activator.CreateInstance(visualType);
			}
			catch
			{
				Application.Current?.FindMauiContext()?.CreateLogger<IVisual>()?.LogWarning("Unable to register {visualType} please add a public default constructor", visualType.ToString());
			}

			return null;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/VisualTypeConverter.xml" path="//Member[@MemberName='ConvertFrom']/Docs" />
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var strValue = value?.ToString();
			if (_visualTypeMappings == null)
				InitMappings();

			if (strValue != null)
			{
				if (_visualTypeMappings.TryGetValue(strValue, out IVisual returnValue))
					return returnValue;

				Application.Current?.FindMauiContext()?.CreateLogger<IVisual>()?.LogWarning($"Cannot convert \"{strValue}\" into {typeof(IVisual)}");
			}

			return VisualMarker.Default;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/VisualTypeConverter.xml" path="//Member[@MemberName='ConvertTo']/Docs" />
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is not IVisual visual)
				throw new NotSupportedException();

			if (_visualTypeMappings == null)
				InitMappings();

			if (visual == VisualMarker.Default)
				return "default";

			if (_visualTypeMappings.ContainsValue(visual))
				return _visualTypeMappings.Keys.Skip(_visualTypeMappings.Values.IndexOf(visual)).First();
			throw new NotSupportedException();
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/VisualTypeConverter.xml" path="//Member[@MemberName='GetStandardValuesExclusive']/Docs" />
		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
			=> false;

		/// <include file="../../../docs/Microsoft.Maui.Controls/VisualTypeConverter.xml" path="//Member[@MemberName='GetStandardValuesSupported']/Docs" />
		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
			=> true;

		/// <include file="../../../docs/Microsoft.Maui.Controls/VisualTypeConverter.xml" path="//Member[@MemberName='GetStandardValues']/Docs" />
		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
			=> new(new[] { "Default", "Material" });
	}
}