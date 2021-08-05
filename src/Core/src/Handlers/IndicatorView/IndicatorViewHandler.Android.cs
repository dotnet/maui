using System;
using Android.Views;
using System.ComponentModel;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Widget;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers
{
	public partial class IndicatorViewHandler : ViewHandler<IIndicatorView, LinearLayout>
	{
		protected override LinearLayout CreateNativeView()
		{
			return new LinearLayout(Context);
		}
	}
}
