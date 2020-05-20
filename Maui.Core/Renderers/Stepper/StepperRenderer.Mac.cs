using AppKit;

namespace System.Maui.Platform
{
	public partial class StepperRenderer : AbstractViewRenderer<IStepper, NSStepper>
	{
		protected override NSStepper CreateView()
		{
			var nSStepper = new NSStepper();
			nSStepper.Activated += OnStepperActivated;
			return nSStepper;
		}

		protected override void DisposeView(NSStepper nSStepper)
		{
			nSStepper.Activated -= OnStepperActivated;
			base.DisposeView(nSStepper);
		}

		public static void MapPropertyMinimum(IViewRenderer renderer, IStepper slider) => (renderer as StepperRenderer)?.UpdateMinimum();
		public static void MapPropertyMaximum(IViewRenderer renderer, IStepper slider) => (renderer as StepperRenderer)?.UpdateMaximum();
		public static void MapPropertyIncrement(IViewRenderer renderer, IStepper slider) => (renderer as StepperRenderer)?.UpdateIncrement();
		public static void MapPropertyValue(IViewRenderer renderer, IStepper slider) => (renderer as StepperRenderer)?.UpdateValue();

		public virtual void UpdateIncrement()
		{
			TypedNativeView.Increment = VirtualView.Increment;
		}

		public virtual void UpdateMaximum()
		{
			TypedNativeView.MaxValue = VirtualView.Maximum;
		}

		public virtual void UpdateMinimum()
		{
			TypedNativeView.MinValue = VirtualView.Minimum;
		}

		public virtual void UpdateValue()
		{
			if (Math.Abs(TypedNativeView.DoubleValue - VirtualView.Value) > 0)
				TypedNativeView.DoubleValue = VirtualView.Value;
		}

		void OnStepperActivated(object sender, EventArgs e) =>
			VirtualView.Value = TypedNativeView.DoubleValue;
	}
}