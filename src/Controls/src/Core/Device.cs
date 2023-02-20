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
	/// <include file="../../docs/Microsoft.Maui.Controls/Device.xml" path="Type[@FullName='Microsoft.Maui.Controls.Device']/Docs/*" />
	[Obsolete]
	public static class Device
	{
		// this is just for those cases where the runtime needs to pre-load renderers
		[Obsolete]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static Assembly DefaultRendererAssembly { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/Device.xml" path="//Member[@MemberName='iOS']/Docs/*" />
		[Obsolete("Use Microsoft.Maui.Devices.DevicePlatform.iOS instead.")]
		public const string iOS = "iOS";
		/// <include file="../../docs/Microsoft.Maui.Controls/Device.xml" path="//Member[@MemberName='Android']/Docs/*" />
		[Obsolete("Use Microsoft.Maui.Devices.DevicePlatform.Android instead.")]
		public const string Android = "Android";
		/// <include file="../../docs/Microsoft.Maui.Controls/Device.xml" path="//Member[@MemberName='UWP']/Docs/*" />
		[Obsolete("Use Microsoft.Maui.Devices.DevicePlatform.WinUI instead.")]
		public const string UWP = "WinUI";
		/// <include file="../../docs/Microsoft.Maui.Controls/Device.xml" path="//Member[@MemberName='macOS']/Docs/*" />
		[Obsolete("Use Microsoft.Maui.Devices.DevicePlatform.macOS instead.")]
		internal const string macOS = "macOS";
		/// <include file="../../docs/Microsoft.Maui.Controls/Device.xml" path="//Member[@MemberName='GTK']/Docs/*" />
		[Obsolete("Use Microsoft.Maui.Devices.DevicePlatform.GTK instead.")]
		internal const string GTK = "GTK";
		/// <include file="../../docs/Microsoft.Maui.Controls/Device.xml" path="//Member[@MemberName='Tizen']/Docs/*" />
		[Obsolete("Use Microsoft.Maui.Devices.DevicePlatform.Tizen instead.")]
		public const string Tizen = "Tizen";
		[Obsolete("Use Microsoft.Maui.Devices.DevicePlatform.WinUI instead.")]
		public const string WinUI = "WinUI";
		/// <include file="../../docs/Microsoft.Maui.Controls/Device.xml" path="//Member[@MemberName='WPF']/Docs/*" />
		[Obsolete("Use Microsoft.Maui.Devices.DevicePlatform.WPF instead.")]
		internal const string WPF = "WPF";
		[Obsolete("Use Microsoft.Maui.Devices.DevicePlatform.MacCatalyst instead.")]
		public const string MacCatalyst = "MacCatalyst";
		[Obsolete("Use Microsoft.Maui.Devices.DevicePlatform.tvOS instead.")]
		public const string tvOS = "tvOS";

		/// <include file="../../docs/Microsoft.Maui.Controls/Device.xml" path="//Member[@MemberName='RuntimePlatform']/Docs/*" />
		[Obsolete("Use Microsoft.Maui.Devices.DeviceInfo.Platform instead.")]
		public static string RuntimePlatform => DeviceInfo.Platform.ToString();

		/// <include file="../../docs/Microsoft.Maui.Controls/Device.xml" path="//Member[@MemberName='FlowDirection']/Docs/*" />
		[Obsolete("Use Microsoft.Maui.ApplicationModel.AppInfo.RequestedLayoutDirection instead.")]
		public static FlowDirection FlowDirection =>
			AppInfo.RequestedLayoutDirection == LayoutDirection.RightToLeft
				? FlowDirection.RightToLeft
				: FlowDirection.LeftToRight;

		/// <include file="../../docs/Microsoft.Maui.Controls/Device.xml" path="//Member[@MemberName='IsInvokeRequired']/Docs/*" />
		[Obsolete("Use BindableObject.Dispatcher.IsDispatchRequired instead.")]
		public static bool IsInvokeRequired =>
			Application.Current.FindDispatcher().IsDispatchRequired;

		/// <include file="../../docs/Microsoft.Maui.Controls/Device.xml" path="//Member[@MemberName='BeginInvokeOnMainThread']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/Device.xml" path="//Member[@MemberName='GetMainThreadSynchronizationContextAsync']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/Device.xml" path="//Member[@MemberName='StartTimer']/Docs/*" />
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
