#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Primitives;

#if __IOS__ || MACCATALYST
using NativeView = UIKit.UIView;
#elif __ANDROID__
using NativeView = Android.Views.View;
#elif WINDOWS
using NativeView = Microsoft.UI.Xaml.FrameworkElement;
#elif NETSTANDARD
using NativeView = System.Object;
#endif

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class StubBaseHandler : ViewHandler<StubBase, NativeView>
	{
		readonly NativeView _nativeView;


		public static IPropertyMapper<StubBase, StubBaseHandler> StubMapper = new PropertyMapper<StubBase, StubBaseHandler>(ViewMapper)
		{
		};

		public static CommandMapper<StubBase, StubBaseHandler> StubCommandMapper = new(ViewCommandMapper)
		{
		};

		protected StubBaseHandler(NativeView nativeView, IPropertyMapper? mapper = null, CommandMapper? commandMapper = null)
			: base(mapper ?? StubMapper, commandMapper ?? ViewCommandMapper)
		{
			_nativeView = nativeView;
		}

		protected override NativeView CreateNativeView() =>
			_nativeView;
	}
}