using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
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
			NativeElement.SetBindingContext(BindingContext,  nv =>  nv.GetChildren<FrameworkElement>());
			base.OnBindingContextChanged();
		}
	}
}