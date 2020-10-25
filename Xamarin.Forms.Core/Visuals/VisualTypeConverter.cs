using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms
{
	[Xaml.TypeConversion(typeof(IVisual))]
	public class VisualTypeConverter : TypeConverter
	{
		static Dictionary<string, IVisual> _visualTypeMappings;
		void InitMappings()
		{
			var mappings = new Dictionary<string, IVisual>(StringComparer.OrdinalIgnoreCase);
			Assembly[] assemblies = Device.GetAssemblies();

			// Check for IVisual Types
			foreach (var assembly in assemblies)
				Register(assembly, mappings);

			if (Internals.Registrar.ExtraAssemblies != null)
				foreach (var assembly in Internals.Registrar.ExtraAssemblies)
					Register(assembly, mappings);


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

		static void Register(Assembly assembly, Dictionary<string, IVisual> mappings)
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
				Log.Warning("Visual", $"Cannot scan assembly {assembly.FullName} for Visual types.");
			}
			catch (FileNotFoundException)
			{
				Log.Warning("Visual", $"Unable to load a dependent assembly for {assembly.FullName}. It cannot be scanned for Visual types.");
			}
			catch (ReflectionTypeLoadException)
			{
				Log.Warning("Visual", $"Unable to load a dependent assembly for {assembly.FullName}. Types cannot be loaded.");
			}
		}

		static void Register(Type visual, Dictionary<string, IVisual> mappings)
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

		static IVisual CreateVisual(Type visualType)
		{
			try
			{
				return (IVisual)Activator.CreateInstance(visualType);
			}
			catch
			{
				Internals.Log.Warning("Visual", $"Unable to register {visualType} please add a public default constructor");
			}

			return null;
		}

		public override object ConvertFromInvariantString(string value)
		{
			if (_visualTypeMappings == null)
				InitMappings();

			if (value != null)
			{
				if (_visualTypeMappings.TryGetValue(value, out IVisual returnValue))
					return returnValue;

				return VisualMarker.Default;
			}

			throw new XamlParseException($"Cannot convert \"{value}\" into {typeof(IVisual)}");
		}

		public override string ConvertToInvariantString(object value)
		{
			if (!(value is IVisual visual))
				throw new NotSupportedException();

			if (_visualTypeMappings == null)
				InitMappings();

			if (visual == VisualMarker.Default)
				return "default";

			if (_visualTypeMappings.ContainsValue(visual))
				return _visualTypeMappings.Keys.Skip(_visualTypeMappings.Values.IndexOf(visual)).First();
			throw new NotSupportedException();
		}
	}
}