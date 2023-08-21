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

using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public class NativeViewWrapper : View
	{
		public NativeViewWrapper(FrameworkElement nativeElement, GetDesiredSizeDelegate getDesiredSizeDelegate = null, ArrangeOverrideDelegate arrangeOverrideDelegate = null,
								 MeasureOverrideDelegate measureOverrideDelegate = null)
		{
			GetDesiredSizeDelegate = getDesiredSizeDelegate;
			ArrangeOverrideDelegate = arrangeOverrideDelegate;
			MeasureOverrideDelegate = measureOverrideDelegate;
			NativeElement = nativeElement;
			nativeElement.TransferbindablePropertiesToWrapper(this);
		}

		public ArrangeOverrideDelegate ArrangeOverrideDelegate { get; set; }

		public GetDesiredSizeDelegate GetDesiredSizeDelegate { get; }

		public MeasureOverrideDelegate MeasureOverrideDelegate { get; set; }

		public FrameworkElement NativeElement { get; }

		protected override void OnBindingContextChanged()
		{
			NativeElement.SetBindingContext(BindingContext, nv => nv.GetChildren<FrameworkElement>());
			base.OnBindingContextChanged();
		}
	}
}