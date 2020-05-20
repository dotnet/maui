using System.Drawing;
using UIKit;

namespace System.Maui.Platform
{
	public partial class StepperRenderer : AbstractViewRenderer<IStepper, UIStepper>
	{
		protected override UIStepper CreateView()
		{
			var uIStepper = new UIStepper(RectangleF.Empty);
			uIStepper.ValueChanged += OnValueChanged;
			return uIStepper;
		}

		protected override void DisposeView(UIStepper uIStepper)
		{
			uIStepper.ValueChanged -= OnValueChanged;
			base.DisposeView(uIStepper);
		}

		public static void MapPropertyMinimum(IViewRenderer renderer, IStepper slider) => (renderer as StepperRenderer)?.UpdateMinimum();
		public static void MapPropertyMaximum(IViewRenderer renderer, IStepper slider) => (renderer as StepperRenderer)?.UpdateMaximum();
		public static void MapPropertyIncrement(IViewRenderer renderer, IStepper slider) => (renderer as StepperRenderer)?.UpdateIncrement();
		public static void MapPropertyValue(IViewRenderer renderer, IStepper slider) => (renderer as StepperRenderer)?.UpdateValue();

		void OnValueChanged(object sender, EventArgs e)
			=> VirtualView.Value = TypedNativeView.Value;

		public virtual void UpdateIncrement()
		{
			TypedNativeView.StepValue = VirtualView.Increment;
		}

		public virtual void UpdateMaximum()
		{
			TypedNativeView.MaximumValue = VirtualView.Maximum;
		}

		public virtual void UpdateMinimum()
		{
			TypedNativeView.MinimumValue = VirtualView.Minimum;
		}

		public virtual void UpdateValue()
		{
			if (TypedNativeView.Value != VirtualView.Value)
				TypedNativeView.Value = VirtualView.Value;
		}
	}
}