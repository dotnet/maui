using System;
using System.ComponentModel;
using Android.Content;
using Android.Views;
using Android.Widget;
using AButton = Android.Widget.Button;

namespace Xamarin.Forms.Platform.Android
{
	public class StepperRenderer : ViewRenderer<Stepper, LinearLayout>, IStepperRenderer
	{
		AButton _downButton;
		AButton _upButton;

		public StepperRenderer(Context context) : base(context)
		{
			AutoPackage = false;
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use StepperRenderer(Context) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public StepperRenderer()
		{
			AutoPackage = false;
		}

		protected override LinearLayout CreateNativeControl()
		{
			return new LinearLayout(Context)
			{
				Orientation = Orientation.Horizontal,
				Focusable = true,
				DescendantFocusability = DescendantFocusability.AfterDescendants
			};
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Stepper> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement == null)
			{
				var layout = CreateNativeControl();
				StepperRendererManager.CreateStepperButtons(this, out _downButton, out _upButton);
				layout.AddView(_downButton, new LinearLayout.LayoutParams(LayoutParams.WrapContent, LayoutParams.MatchParent));
				layout.AddView(_upButton, new LinearLayout.LayoutParams(LayoutParams.WrapContent, LayoutParams.MatchParent));
				SetNativeControl(layout);
			}

			StepperRendererManager.UpdateButtons(this, _downButton, _upButton);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (this.IsDisposed())
			{
				return;
			}

			base.OnElementPropertyChanged(sender, e);
			StepperRendererManager.UpdateButtons(this, _downButton, _upButton, e);
		}

		Stepper IStepperRenderer.Element => Element;

		AButton IStepperRenderer.UpButton => _upButton;

		AButton IStepperRenderer.DownButton => _downButton;

		AButton IStepperRenderer.CreateButton()
		{
			var button = new AButton(Context);
			button.SetHeight((int)Context.ToPixels(10.0));
			return button;
		}
	}
}