//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public static class NativeBindingExtensions
	{
		public static void SetBinding(this global::Android.Views.View view, string propertyName, BindingBase binding, string updateSourceEventName = null)
		{
			PlatformBindingHelpers.SetBinding(view, propertyName, binding, updateSourceEventName);
		}

		public static void SetBinding(this global::Android.Views.View view, BindableProperty targetProperty, BindingBase binding)
		{
			PlatformBindingHelpers.SetBinding(view, targetProperty, binding);
		}

		public static void SetValue(this global::Android.Views.View target, BindableProperty targetProperty, object value)
		{
			PlatformBindingHelpers.SetValue(target, targetProperty, value);
		}

		public static void SetBindingContext(this global::Android.Views.View target, object bindingContext, Func<global::Android.Views.View, IEnumerable<global::Android.Views.View>> getChildren = null)
		{
			PlatformBindingHelpers.SetBindingContext(target, bindingContext, getChildren);
		}

		internal static void TransferBindablePropertiesToWrapper(this global::Android.Views.View target, View wrapper)
		{
			PlatformBindingHelpers.TransferBindablePropertiesToWrapper(target, wrapper);
		}
	}
}