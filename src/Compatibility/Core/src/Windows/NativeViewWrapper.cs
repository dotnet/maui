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
			NativeElement.SetBindingContext(BindingContext,  nv =>  nv.GetChildren<FrameworkElement>());
			base.OnBindingContextChanged();
		}
	}
}