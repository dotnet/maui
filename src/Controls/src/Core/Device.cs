#nullable disable
using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.Controls
{
	/// <summary>A utility class to interact with the current Device/Platform.</summary>
	[Obsolete]
	public static class Device
	{
		// this is just for those cases where the runtime needs to pre-load renderers
		[Obsolete]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static Assembly DefaultRendererAssembly { get; set; }

		/// <summary>The string "iOS", representing the iOS operating system.</summary>
		[Obsolete("Use Microsoft.Maui.Devices.DevicePlatform.iOS instead.")]
		public const string iOS = "iOS";
		/// <summary>The string "Android", representing the Android operating system.</summary>
		[Obsolete("Use Microsoft.Maui.Devices.DevicePlatform.Android instead.")]
		public const string Android = "Android";
		/// <summary>The string "UWP", representing the UWP operating system.</summary>
		[Obsolete("Use Microsoft.Maui.Devices.DevicePlatform.WinUI instead.")]
		public const string UWP = "WinUI";
		/// <summary>The string "macOS", representing the macOS operating system.</summary>
		[Obsolete("Use Microsoft.Maui.Devices.DevicePlatform.macOS instead.")]
		internal const string macOS = "macOS";
		/// <summary>The string "GTK", representing the Linux operating system.</summary>
		[Obsolete("Use Microsoft.Maui.Devices.DevicePlatform.GTK instead.")]
		internal const string GTK = "GTK";
		/// <summary>The string "Tizen", representing the Tizen operating system.</summary>
		[Obsolete("Use Microsoft.Maui.Devices.DevicePlatform.Tizen instead.")]
		public const string Tizen = "Tizen";
		[Obsolete("Use Microsoft.Maui.Devices.DevicePlatform.WinUI instead.")]
		public const string WinUI = "WinUI";
		/// <summary>The string "WPF", representing the Windows Presentation Foundation framework.</summary>
		[Obsolete("Use Microsoft.Maui.Devices.DevicePlatform.WPF instead.")]
		internal const string WPF = "WPF";
		[Obsolete("Use Microsoft.Maui.Devices.DevicePlatform.MacCatalyst instead.")]
		public const string MacCatalyst = "MacCatalyst";
		[Obsolete("Use Microsoft.Maui.Devices.DevicePlatform.tvOS instead.")]
		public const string tvOS = "tvOS";

		/// <summary>Gets the kind of device that Microsoft.Maui.Controls is currently working on.</summary>
		[Obsolete("Use Microsoft.Maui.Devices.DeviceInfo.Idiom instead.")]
		public static TargetIdiom Idiom
		{
			get
			{
				var idiom = DeviceInfo.Idiom;
				if (idiom == DeviceIdiom.Tablet)
					return TargetIdiom.Tablet;
				if (idiom == DeviceIdiom.Phone)
					return TargetIdiom.Phone;
				if (idiom == DeviceIdiom.Desktop)
					return TargetIdiom.Desktop;
				if (idiom == DeviceIdiom.TV)
					return TargetIdiom.TV;
				if (idiom == DeviceIdiom.Watch)
					return TargetIdiom.Watch;
				return TargetIdiom.Unsupported;
			}
		}

		/// <summary>Gets the kind of device that Microsoft.Maui.Controls is currently working on.</summary>
		[Obsolete("Use Microsoft.Maui.Devices.DeviceInfo.Platform instead.")]
		public static string RuntimePlatform => DeviceInfo.Platform.ToString();

		/// <summary>Gets the flow direction on the device.</summary>
		/// <remarks>The following contains a few important points about flow direction from
		/// The default value of
		/// All</remarks>
		[Obsolete("Use Microsoft.Maui.ApplicationModel.AppInfo.RequestedLayoutDirection instead.")]
		public static FlowDirection FlowDirection =>
			AppInfo.RequestedLayoutDirection == LayoutDirection.RightToLeft
				? FlowDirection.RightToLeft
				: FlowDirection.LeftToRight;

		/// <summary>Internal API for Microsoft.Maui.Controls platform use.</summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		[Obsolete("Use BindableObject.Dispatcher.IsDispatchRequired instead.")]
		public static bool IsInvokeRequired =>
			Application.Current.FindDispatcher().IsDispatchRequired;

		/// <summary>Invokes an Action on the device main (UI) thread.</summary>
		/// <param name="action">The Action to invoke</param>
		/// <remarks>This example shows how to set the Text of Label on the main thread, e.g. in response to an async event.</remarks>
		[Obsolete("Use BindableObject.Dispatcher.Dispatch() instead.")]
		public static void BeginInvokeOnMainThread(Action action) =>
			Application.Current.FindDispatcher().Dispatch(action);

		/// <include file="../../docs/Microsoft.Maui.Controls/Device.xml" path="//Member[@MemberName='InvokeOnMainThreadAsync&lt;T&gt;'][2]/Docs/*" />
		[Obsolete("Use BindableObject.Dispatcher.DispatchAsync() instead.")]
		public static Task<T> InvokeOnMainThreadAsync<T>(Func<T> func) =>
			Application.Current.FindDispatcher().DispatchAsync(func);

		/// <include file="../../docs/Microsoft.Maui.Controls/Device.xml" path="//Member[@MemberName='InvokeOnMainThreadAsync'][1]/Docs/*" />
		[Obsolete("Use BindableObject.Dispatcher.DispatchAsync() instead.")]
		public static Task InvokeOnMainThreadAsync(Action action) =>
			Application.Current.FindDispatcher().DispatchAsync(action);

		/// <include file="../../docs/Microsoft.Maui.Controls/Device.xml" path="//Member[@MemberName='InvokeOnMainThreadAsync&lt;T&gt;'][1]/Docs/*" />
		[Obsolete("Use BindableObject.Dispatcher.DispatchAsync() instead.")]
		public static Task<T> InvokeOnMainThreadAsync<T>(Func<Task<T>> funcTask) =>
			Application.Current.FindDispatcher().DispatchAsync(funcTask);

		/// <include file="../../docs/Microsoft.Maui.Controls/Device.xml" path="//Member[@MemberName='InvokeOnMainThreadAsync'][2]/Docs/*" />
		[Obsolete("Use BindableObject.Dispatcher.DispatchAsync() instead.")]
		public static Task InvokeOnMainThreadAsync(Func<Task> funcTask) =>
			Application.Current.FindDispatcher().DispatchAsync(funcTask);

		/// <summary>Returns the current <see cref="System.Threading.SynchronizationContext"/> from the main thread.</summary>
		/// <returns>The current <see cref="System.Threading.SynchronizationContext"/> from the main thread.</returns>
		[Obsolete("Use BindableObject.Dispatcher.GetSynchronizationContextAsync() instead.")]
		public static Task<SynchronizationContext> GetMainThreadSynchronizationContextAsync() =>
			Application.Current.FindDispatcher().GetSynchronizationContextAsync();

		/// <include file="../../docs/Microsoft.Maui.Controls/Device.xml" path="//Member[@MemberName='GetNamedSize'][2]/Docs/*" />
		[Obsolete]
		public static double GetNamedSize(NamedSize size, Element targetElement)
		{
			return GetNamedSize(size, targetElement.GetType());
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Device.xml" path="//Member[@MemberName='GetNamedSize'][1]/Docs/*" />
		[Obsolete]
		public static double GetNamedSize(NamedSize size, Type targetElementType)
		{
			return GetNamedSize(size, targetElementType, false);
		}

		/// <summary>Starts a recurring timer using the device clock capabilities.</summary>
		/// <param name="interval">The interval between invocations of the callback.</param>
		/// <param name="callback">The action to run when the timer elapses.</param>
		/// <remarks>While the callback returns
		/// If you want the code inside the timer to interact on the UI thread (e.g. setting text of a Label or showing an alert), it should be done within a</remarks>
		[Obsolete("Use BindableObject.Dispatcher.StartTimer() or BindableObject.Dispatcher.DispatchDelayed() instead.")]
		public static void StartTimer(TimeSpan interval, Func<bool> callback)
		{
			_ = callback ?? throw new ArgumentNullException(nameof(callback));

			var dispatcher = Application.Current.FindDispatcher();

			dispatcher.StartTimer(interval, callback);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Device.xml" path="//Member[@MemberName='GetNamedSize'][3]/Docs/*" />
		[Obsolete]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static double GetNamedSize(NamedSize size, Type targetElementType, bool useOldSizes) =>
			DependencyService.Get<IFontNamedSizeService>()?.GetNamedSize(size, targetElementType, useOldSizes) ??
			throw new NotImplementedException("The current platform does not implement the IFontNamedSizeService dependency service.");

		[Obsolete]
		public static class Styles
		{
			public static readonly string TitleStyleKey = "TitleStyle";

			public static readonly string SubtitleStyleKey = "SubtitleStyle";

			public static readonly string BodyStyleKey = "BodyStyle";

			public static readonly string ListItemTextStyleKey = "ListItemTextStyle";

			public static readonly string ListItemDetailTextStyleKey = "ListItemDetailTextStyle";

			public static readonly string CaptionStyleKey = "CaptionStyle";

			public static readonly Style TitleStyle = new Style(typeof(Label)) { BaseResourceKey = TitleStyleKey };

			public static readonly Style SubtitleStyle = new Style(typeof(Label)) { BaseResourceKey = SubtitleStyleKey };

			public static readonly Style BodyStyle = new Style(typeof(Label)) { BaseResourceKey = BodyStyleKey };

			public static readonly Style ListItemTextStyle = new Style(typeof(Label)) { BaseResourceKey = ListItemTextStyleKey };

			public static readonly Style ListItemDetailTextStyle = new Style(typeof(Label)) { BaseResourceKey = ListItemDetailTextStyleKey };

			public static readonly Style CaptionStyle = new Style(typeof(Label)) { BaseResourceKey = CaptionStyleKey };
		}
	}
}
