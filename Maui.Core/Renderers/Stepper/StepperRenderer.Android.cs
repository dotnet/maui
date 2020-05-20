using Android.Widget;
using Android.Views;
using AButton = Android.Widget.Button;

namespace System.Maui.Platform
{
	public partial class StepperRenderer : AbstractViewRenderer<IStepper, LinearLayout> , IStepperRenderer
	{
		AButton _downButton;
		AButton _upButton;

		protected override LinearLayout CreateView()
		{
			var aStepper = new LinearLayout(Context)
			{
				Orientation = Android.Widget.Orientation.Horizontal,
				Focusable = true,
				DescendantFocusability = DescendantFocusability.AfterDescendants
			};

			StepperRendererManager.CreateStepperButtons(this, out _downButton, out _upButton);
			aStepper.AddView(_downButton, new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent));
			aStepper.AddView(_upButton, new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent));

			return aStepper;
		}

		public static void MapPropertyMinimum(IViewRenderer renderer, IStepper slider) => (renderer as StepperRenderer)?.UpdateButtons();
		public static void MapPropertyMaximum(IViewRenderer renderer, IStepper slider) => (renderer as StepperRenderer)?.UpdateButtons();
		public static void MapPropertyIncrement(IViewRenderer renderer, IStepper slider) => (renderer as StepperRenderer)?.UpdateButtons();
		public static void MapPropertyValue(IViewRenderer renderer, IStepper slider) =>  (renderer as StepperRenderer)?.UpdateButtons();

		public static void MapPropertyIsEnabled(IViewRenderer renderer, IStepper slider)
		{
			ViewRenderer.MapPropertyIsEnabled(renderer, slider);
			(renderer as StepperRenderer)?.UpdateButtons();
		}

		public virtual void UpdateButtons()
		{
			StepperRendererManager.UpdateButtons(this, _downButton, _upButton);
		}

		IStepper IStepperRenderer.Element => VirtualView;

		AButton IStepperRenderer.UpButton => _upButton;

		AButton IStepperRenderer.DownButton => _downButton;

		AButton IStepperRenderer.CreateButton()
		{
			var button = new AButton(Context);

			return button;
		}
	}
}