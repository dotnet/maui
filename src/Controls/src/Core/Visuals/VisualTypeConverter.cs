#nullable disable
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
	/// <summary>
	/// Converts between string representations and <see cref="IVisual"/> instances.
	/// </summary>
	public class VisualTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(string);

		static Dictionary<string, IVisual> _visualTypeMappings;
		void InitMappings()
		{
			var mappings = new Dictionary<string, IVisual>(StringComparer.OrdinalIgnoreCase);

			if (RuntimeFeature.IsIVisualAssemblyScanningEnabled)
			{
#if NET8_0
#pragma warning disable IL2026 // FeatureGuardAttribute is not supported on .NET 8
#endif
				ScanAllAssemblies(mappings);
#if NET8_0
#pragma warning restore IL2026 // FeatureGuardAttribute is not supported on .NET 8
#endif
			}
			else
			{
				Register(typeof(VisualMarker.MaterialVisual), mappings);
				Register(typeof(VisualMarker.DefaultVisual), mappings);
				Register(typeof(VisualMarker.MatchParentVisual), mappings);
			}

			_visualTypeMappings = mappings;
		}

		[RequiresUnreferencedCode("The IVisual types might be removed by trimming and automatic registration via assembly scanning may not work as expected.")]
		void ScanAllAssemblies(Dictionary<string, IVisual> mappings)
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

			// Check for IVisual Types
			foreach (var assembly in assemblies)
				RegisterAllIVisualTypesInAssembly(assembly, mappings);

			if (Internals.Registrar.ExtraAssemblies != null)
				foreach (var assembly in Internals.Registrar.ExtraAssemblies)
					RegisterAllIVisualTypesInAssembly(assembly, mappings);


			// Check for visual assembly attributes	after scanning for IVisual Types
			// this will let users replace the default visual names if they want to
			foreach (var assembly in assemblies)
				RegisterFromAttributes(assembly, mappings);

			if (Internals.Registrar.ExtraAssemblies != null)
				foreach (var assembly in Internals.Registrar.ExtraAssemblies)
					RegisterFromAttributes(assembly, mappings);

			static void RegisterAllIVisualTypesInAssembly(Assembly assembly, Dictionary<string, IVisual> mappings)
			{
				if (assembly.IsDynamic)
					return;

				try
				{
					foreach (var type in assembly.GetExportedTypes())
						if (typeof(IVisual).IsAssignableFrom(type) && type != typeof(IVisual))
							Register(type, mappings);
				}
				catch (NotSupportedException)
				{
					MauiLog.LogWarning($"Cannot scan assembly {assembly.FullName} for Visual types.");
				}
				catch (FileNotFoundException)
				{
					MauiLog.LogWarning($"Unable to load a dependent assembly for {assembly.FullName}. It cannot be scanned for Visual types.");
				}
				catch (ReflectionTypeLoadException)
				{
					MauiLog.LogWarning($"Unable to load a dependent assembly for {assembly.FullName}. Types cannot be loaded.");
				}
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
				MauiLog.LogWarning($"Unable to register {visualType} please add a public default constructor");
			}

			return null;
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var strValue = value?.ToString();
			if (_visualTypeMappings == null)
				InitMappings();

			if (strValue != null)
			{
				if (_visualTypeMappings.TryGetValue(strValue, out IVisual returnValue))
					return returnValue;

				if (!RuntimeFeature.IsIVisualAssemblyScanningEnabled)
				{
				MauiLog.LogWarning($"Unable to find visual {strValue}. Automatic discovery of IVisual types is disabled. You can enabled it by setting the $(MauiEnableIVisualAssemblyScanning)=true MSBuild property. Note: automatic registration of IVisual types through assembly scanning is not trimming-compatible and it can lead to slower app startup.");
				}

				return VisualMarker.Default;
			}

			throw new XamlParseException($"Cannot convert \"{strValue}\" into {typeof(IVisual)}");
		}

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

		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
			=> false;

		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
			=> true;

		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
			=> new(new[] {
				nameof(VisualMarker.Default), 
				// nameof(VisualMarker.Material)
			});
	}
}
