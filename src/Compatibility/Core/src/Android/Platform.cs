using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.DrawerLayout.Widget;
using AndroidX.Legacy.App;
using Microsoft.Maui.Controls.Compatibility.Platform.Android.AppCompat;
using Microsoft.Maui.Controls.Internals;
using AView = Android.Views.View;
using FragmentManager = AndroidX.Fragment.App.FragmentManager;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public class Platform
	{
		public static void ClearRenderer(AView renderedView)
		{
			AppCompat.Platform.ClearRenderer(renderedView);
		}

		public static IVisualElementRenderer CreateRendererWithContext(VisualElement element, Context context)
		{
			// This is an interim method to allow public access to CreateRenderer(element, context), which we 
			// can't make public yet because it will break the previewer
			return AppCompat.Platform.CreateRendererWithContext(element, context);
		}

		public static IVisualElementRenderer GetRenderer(VisualElement bindable)
		{
			return AppCompat.Platform.GetRenderer(bindable);
		}

		public static void SetRenderer(VisualElement bindable, IVisualElementRenderer value)
		{
			AppCompat.Platform.SetRenderer(bindable, value);
		}

		public static SizeRequest GetNativeSize(VisualElement view, double widthConstraint, double heightConstraint)
		{
			return AppCompat.Platform.GetNativeSize(view, widthConstraint, heightConstraint);
		}
	}
}
