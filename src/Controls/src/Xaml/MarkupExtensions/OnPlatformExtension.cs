using System;
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
	/// <remarks>
	/// The value is resolved by matching <see cref="Microsoft.Maui.Devices.DeviceInfo.Platform"/> (a string)
	/// against the per-platform values, so a custom backend whose platform string matches one of the named
	/// properties below (for example <c>GTK</c>) resolves correctly at both runtime and compile time.
	/// <para>
	/// Because a XAML markup extension maps each named argument to a CLR property, the inline
	/// <c>{OnPlatform iOS=…, Android=…}</c> form can only express the platforms exposed as properties here.
	/// To target an <em>arbitrary</em> custom platform (e.g. a backend reporting
	/// <c>DevicePlatform.Create("Web")</c>), use the element form, which accepts any platform string:
	/// <code>
	/// &lt;OnPlatform x:TypeArguments="x:String"&gt;
	///     &lt;On Platform="Web" Value="…" /&gt;
	///     &lt;On Platform="iOS, Android" Value="…" /&gt;
	/// &lt;/OnPlatform&gt;
	/// </code>
	/// </para>
	/// </remarks>
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
			// per-platform values, the same string-based approach the data-driven element
			// form (OnPlatform<T>/On) uses. This lets custom backends (e.g. GTK/Linux) resolve
			// a value as long as DeviceInfo.Platform.ToString() matches one of the keys below.
			// Ordinal (case-sensitive), matching DevicePlatform equality and the compile-time
			// SimplifyOnPlatformVisitor, so runtime and compiled XAML resolve identically.
			var platform = DeviceInfo.Platform.ToString();

			if (Matches(platform, nameof(DevicePlatform.Android)) && Android != s_notset)
			{
				value = Android;
				return true;
			}
			if (Matches(platform, "GTK") && GTK != s_notset)
			{
				value = GTK;
				return true;
			}
			if (Matches(platform, nameof(DevicePlatform.iOS)) && iOS != s_notset)
			{
				value = iOS;
				return true;
			}
			if (Matches(platform, nameof(DevicePlatform.macOS)) && macOS != s_notset)
			{
				value = macOS;
				return true;
			}
			if (Matches(platform, nameof(DevicePlatform.MacCatalyst)) && MacCatalyst != s_notset)
			{
				value = MacCatalyst;
				return true;
			}
			if (Matches(platform, nameof(DevicePlatform.Tizen)) && Tizen != s_notset)
			{
				value = Tizen;
				return true;
			}
			if (Matches(platform, nameof(DevicePlatform.WinUI)) && WinUI != s_notset)
			{
				value = WinUI;
				return true;
			}
#pragma warning disable CS0618 // Type or member is obsolete
			// UWP is a backwards-compatible alias for WinUI: fall back to it for the WinUI
			// platform only when no explicit WinUI value was provided (WinUI takes precedence).
			if (Matches(platform, nameof(DevicePlatform.WinUI)) && UWP != s_notset)
			{
				value = UWP;
				return true;
			}
			// "UWP" still matches a custom backend reporting the legacy "UWP" platform string.
			if (Matches(platform, "UWP") && UWP != s_notset)
			{
				value = UWP;
				return true;
			}
#pragma warning restore CS0618 // Type or member is obsolete
			if (Matches(platform, "WPF") && WPF != s_notset)
			{
				value = WPF;
				return true;
			}

			value = Default;
			return value != s_notset;

			static bool Matches(string platform, string key) => string.Equals(platform, key, StringComparison.Ordinal);
		}
	}
}