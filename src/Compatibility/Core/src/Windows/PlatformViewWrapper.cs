using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public class PlatformViewWrapper : View
	{
		public PlatformViewWrapper(FrameworkElement nativeElement, GetDesiredSizeDelegate getDesiredSizeDelegate = null, ArrangeOverrideDelegate arrangeOverrideDelegate = null,
								 MeasureOverrideDelegate measureOverrideDelegate = null)
		{
			GetDesiredSizeDelegate = getDesiredSizeDelegate;
			ArrangeOverrideDelegate = arrangeOverrideDelegate;
			MeasureOverrideDelegate = measureOverrideDelegate;
			PlatformElement = nativeElement;
			nativeElement.TransferbindablePropertiesToWrapper(this);
		}

		public ArrangeOverrideDelegate ArrangeOverrideDelegate { get; set; }

		public GetDesiredSizeDelegate GetDesiredSizeDelegate { get; }

		public MeasureOverrideDelegate MeasureOverrideDelegate { get; set; }

		public FrameworkElement PlatformElement { get; }

		protected override void OnBindingContextChanged()
		{
			PlatformElement.SetBindingContext(BindingContext,  nv =>  nv.GetChildren<FrameworkElement>());
			base.OnBindingContextChanged();
		}
	}
}