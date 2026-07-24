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
	/// The value is resolved by matching the <see cref="Microsoft.Maui.Devices.DeviceInfo.Platform"/> identifier
	/// (the <see cref="Microsoft.Maui.Devices.DevicePlatform"/> struct's <c>ToString()</c> value)
	/// against the per-platform values, so a custom backend whose platform string matches one of the named
	/// properties below (for example <c>GTK</c>) resolves correctly at runtime. Note that the compile-time
	/// optimization (<c>SimplifyOnPlatformVisitor</c>) currently recognizes only the
	/// <c>-android</c>/<c>-ios</c>/<c>-macos</c>/<c>-maccatalyst</c> target frameworks; every other target
	/// framework (including <c>-windows</c> and <c>-tizen</c>) and any custom backend falls back to runtime
	/// resolution.
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
			if (DeviceInfo.Platform == DevicePlatform.Android && Android != s_notset)
			{
				value = Android;
				return true;
			}
			if (DeviceInfo.Platform == DevicePlatform.Create("GTK") && GTK != s_notset)
			{
				value = GTK;
				return true;
			}
			if (DeviceInfo.Platform == DevicePlatform.iOS && iOS != s_notset)
			{
				value = iOS;
				return true;
			}
			if (DeviceInfo.Platform == DevicePlatform.macOS && macOS != s_notset)
			{
				value = macOS;
				return true;
			}
			if (DeviceInfo.Platform == DevicePlatform.MacCatalyst && MacCatalyst != s_notset)
			{
				value = MacCatalyst;
				return true;
			}
			if (DeviceInfo.Platform == DevicePlatform.Tizen && Tizen != s_notset)
			{
				value = Tizen;
				return true;
			}
			if (DeviceInfo.Platform == DevicePlatform.WinUI && WinUI != s_notset)
			{
				value = WinUI;
				return true;
			}
#pragma warning disable CS0618 // Type or member is obsolete
			if (DeviceInfo.Platform == DevicePlatform.WinUI && UWP != s_notset)
			{
				value = UWP;
				return true;
			}
			if (DeviceInfo.Platform == DevicePlatform.Create("UWP") && UWP != s_notset)
			{
				value = UWP;
				return true;
			}
#pragma warning restore CS0618 // Type or member is obsolete
			if (DeviceInfo.Platform == DevicePlatform.Create("WPF") && WPF != s_notset)
			{
				value = WPF;
				return true;
			}
			value = Default;
			return value != s_notset;
		}
	}
}