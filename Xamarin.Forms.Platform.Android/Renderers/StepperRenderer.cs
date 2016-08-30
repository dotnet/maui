using System.ComponentModel;
using Android.Views;
using Android.Widget;
using Java.Lang;
using AButton = Android.Widget.Button;

namespace Xamarin.Forms.Platform.Android
{
	public class StepperRenderer : ViewRenderer<Stepper, LinearLayout>
	{
		AButton _downButton;
		AButton _upButton;

		public StepperRenderer()
		{
			AutoPackage = false;
		}

		protected override LinearLayout CreateNativeControl()
		{
			return new LinearLayout(Context) { Orientation = Orientation.Horizontal };
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Stepper> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement == null)
			{
				_downButton = new AButton(Context) { Text = "-", Gravity = GravityFlags.Center, Tag = this };

				_downButton.SetOnClickListener(StepperListener.Instance);

				_upButton = new AButton(Context) { Text = "+", Tag = this };

				_upButton.SetOnClickListener(StepperListener.Instance);
				_upButton.SetHeight((int)Context.ToPixels(10.0));

				var layout = CreateNativeControl();

				layout.AddView(_downButton);
				layout.AddView(_upButton);

				SetNativeControl(layout);
			}

			UpdateButtonEnabled();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			switch (e.PropertyName)
			{
				case "Minimum":
					UpdateButtonEnabled();
					break;
				case "Maximum":
					UpdateButtonEnabled();
					break;
				case "Value":
					UpdateButtonEnabled();
					break;
				case "IsEnabled":
					UpdateButtonEnabled();
					break;
			}
		}

		void UpdateButtonEnabled()
		{
			Stepper view = Element;
			_upButton.Enabled = view.IsEnabled ? view.Value < view.Maximum : view.IsEnabled;
			_downButton.Enabled = view.IsEnabled ? view.Value > view.Minimum : view.IsEnabled;
		}

		class StepperListener : Object, IOnClickListener
		{
			public static readonly StepperListener Instance = new StepperListener();

			public void OnClick(global::Android.Views.View v)
			{
				var renderer = v.Tag as StepperRenderer;
				if (renderer == null)
					return;

				Stepper stepper = renderer.Element;
				if (stepper == null)
					return;

				if (v == renderer._upButton)
					((IElementController)stepper).SetValueFromRenderer(Stepper.ValueProperty, stepper.Value + stepper.Increment);
				else if (v == renderer._downButton)
					((IElementController)stepper).SetValueFromRenderer(Stepper.ValueProperty, stepper.Value - stepper.Increment);
			}
		}
	}
}