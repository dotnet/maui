using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Controls.Xaml
{
	/// <summary>
	/// Provides a XAML markup extension that returns different values depending on the platform the app is running on.
	/// </summary>
	[ContentProperty(nameof(Default))]
	[RequireService(
		[typeof(IProvideValueTarget),
		 typeof(IValueConverterProvider),
		 typeof(IXmlLineInfoProvider),
		 typeof(IConverterOptions)])]
	[RequiresUnreferencedCode("The OnPlatformExtension is not trim safe. Use OnPlatform<T> instead.")]
	public class OnPlatformExtension : IMarkupExtension
	{
		static object s_notset = new object();

		/// <summary>
		/// Gets or sets the default value to use if no platform-specific value is set.
		/// </summary>
		public object Default { get; set; } = s_notset;

		/// <summary>
		/// Gets or sets the value to use on Android.
		/// </summary>
		public object Android { get; set; } = s_notset;

		/// <summary>
		/// Gets or sets the value to use on GTK.
		/// </summary>
		public object GTK { get; set; } = s_notset;

		/// <summary>
		/// Gets or sets the value to use on iOS.
		/// </summary>
		public object iOS { get; set; } = s_notset;

		/// <summary>
		/// Gets or sets the value to use on macOS.
		/// </summary>
		/// <remarks>Note, this is different than <see cref="MacCatalyst"/>.</remarks>
		public object macOS { get; set; } = s_notset;

		/// <summary>
		/// Gets or sets the value to use on Mac Catalyst.
		/// </summary>
		public object MacCatalyst { get; set; } = s_notset;

		/// <summary>
		/// Gets or sets the value to use on Tizen.
		/// </summary>
		public object Tizen { get; set; } = s_notset;

		/// <summary>
		/// Gets or sets the value to use on UWP. Use <see cref="WinUI"/> instead.
		/// </summary>
		[Obsolete("Use WinUI instead.")]
		public object UWP { get; set; } = s_notset;

		/// <summary>
		/// Gets or sets the value to use on WPF.
		/// </summary>
		public object WPF { get; set; } = s_notset;

		/// <summary>
		/// Gets or sets the value to use on Windows (WinUI).
		/// </summary>
		public object WinUI { get; set; } = s_notset;

		/// <summary>
		/// Gets or sets a converter to apply to the platform-specific value.
		/// </summary>
		public IValueConverter Converter { get; set; }

		/// <summary>
		/// Gets or sets a parameter to pass to the converter.
		/// </summary>
		public object ConverterParameter { get; set; }

		public object ProvideValue(IServiceProvider serviceProvider)
		{
			if (Android == s_notset
				&& GTK == s_notset
				&& iOS == s_notset
				&& macOS == s_notset
				&& MacCatalyst == s_notset
				&& Tizen == s_notset
#pragma warning disable CS0618 // Type or member is obsolete
				&& UWP == s_notset
#pragma warning restore CS0618 // Type or member is obsolete
				&& WPF == s_notset
				&& WinUI == s_notset
				&& Default == s_notset)
			{
				throw new XamlParseException("OnPlatformExtension requires a value to be specified for at least one platform or Default.", serviceProvider);
			}

			var valueProvider = serviceProvider?.GetService<IProvideValueTarget>() ?? throw new ArgumentException();

			BindableProperty bp;
			PropertyInfo pi = null;
			Type propertyType = null;

			if (valueProvider.TargetObject is Setter setter)
				bp = setter.Property;
			else
			{
				bp = valueProvider.TargetProperty as BindableProperty;
				pi = valueProvider.TargetProperty as PropertyInfo;
			}
			propertyType = bp?.ReturnType
						?? pi?.PropertyType
						?? throw new InvalidOperationException("Cannot determine property to provide the value for.");

			if (!TryGetValueForPlatform(out var value))
			{
				if (bp != null)
				{
					object targetObject = valueProvider.TargetObject;

					if (targetObject is Setter)
						return null;
					else
						return bp.GetDefaultValue(targetObject as BindableObject);
				}
				if (propertyType.IsValueType)
					return Activator.CreateInstance(propertyType);
				return null;
			}

			if (Converter != null)
				return Converter.Convert(value, propertyType, ConverterParameter, CultureInfo.CurrentUICulture);

			var converterProvider = serviceProvider?.GetService<IValueConverterProvider>();
			if (converterProvider != null)
			{
				MemberInfo minforetriever()
				{
					if (pi != null)
						return pi;

					MemberInfo minfo = null;
					try
					{
						minfo = bp.DeclaringType.GetRuntimeProperty(bp.PropertyName);
					}
					catch (AmbiguousMatchException e)
					{
						throw new XamlParseException($"Multiple properties with name '{bp.DeclaringType}.{bp.PropertyName}' found.", serviceProvider, innerException: e);
					}
					if (minfo != null)
						return minfo;
					try
					{
						return bp.DeclaringType.GetRuntimeMethod("Get" + bp.PropertyName, new[] { typeof(BindableObject) });
					}
					catch (AmbiguousMatchException e)
					{
						throw new XamlParseException($"Multiple methods with name '{bp.DeclaringType}.Get{bp.PropertyName}' found.", serviceProvider, innerException: e);
					}
				}

				return converterProvider.Convert(value, propertyType, minforetriever, serviceProvider);
			}
			var ret = value.ConvertTo(propertyType, () => pi, serviceProvider, out Exception exception);
			if (exception != null)
				throw exception;
			return ret;
		}

		bool TryGetValueForPlatform(out object value)
		{
			// Resolve the value by comparing the current platform string against the
			// per-platform values, the same way the data-driven element form (OnPlatform<T>/On)
			// does. This lets custom backends (e.g. GTK/Linux) resolve a value as long as
			// DeviceInfo.Platform.ToString() matches one of the keys below.
			var lookup = BuildPlatformLookup();
			if (lookup.TryGetValue(DeviceInfo.Platform.ToString(), out value))
				return true;

			value = Default;
			return value != s_notset;
		}

		Dictionary<string, object> BuildPlatformLookup()
		{
			// Keyed by platform string using Ordinal (case-sensitive) comparison, matching
			// DevicePlatform equality, the element form (OnPlatform<T>/On) and the compile-time
			// SimplifyOnPlatformVisitor, so runtime and compiled XAML resolve identically.
			var lookup = new Dictionary<string, object>(StringComparer.Ordinal);

			AddIfSet(lookup, nameof(DevicePlatform.Android), Android);
			AddIfSet(lookup, "GTK", GTK);
			AddIfSet(lookup, nameof(DevicePlatform.iOS), iOS);
			AddIfSet(lookup, nameof(DevicePlatform.macOS), macOS);
			AddIfSet(lookup, nameof(DevicePlatform.MacCatalyst), MacCatalyst);
			AddIfSet(lookup, nameof(DevicePlatform.Tizen), Tizen);
			AddIfSet(lookup, nameof(DevicePlatform.WinUI), WinUI);
			AddIfSet(lookup, "WPF", WPF);

#pragma warning disable CS0618 // Type or member is obsolete
			if (UWP != s_notset)
			{
				// "UWP" still matches a custom backend reporting the legacy "UWP" platform string.
				lookup["UWP"] = UWP;

				// UWP is a backwards-compatible alias for WinUI: only fall back to it for the
				// WinUI platform when no explicit WinUI value was provided (WinUI takes precedence).
				if (!lookup.ContainsKey(nameof(DevicePlatform.WinUI)))
					lookup[nameof(DevicePlatform.WinUI)] = UWP;
			}
#pragma warning restore CS0618 // Type or member is obsolete

			return lookup;

			static void AddIfSet(Dictionary<string, object> lookup, string key, object value)
			{
				if (value != s_notset)
					lookup[key] = value;
			}
		}
	}
}