using Android.Widget;
using AndroidX.AppCompat.Widget;

namespace Microsoft.Maui.Handlers
{
	public partial class RadioButtonHandler : AbstractViewHandler<IRadioButton, AppCompatRadioButton>
	{
		CompoundButtonCheckedChangeListener CheckedChangeListener { get; } = new CompoundButtonCheckedChangeListener();

		protected override AppCompatRadioButton CreateNativeView()
		{
			return new AppCompatRadioButton(Context)
			{
				SoundEffectsEnabled = false
			};
		}

		protected override void ConnectHandler(AppCompatRadioButton nativeView)
		{
			CheckedChangeListener.Handler = this;
			nativeView.SetOnCheckedChangeListener(CheckedChangeListener);
		}

		protected override void DisconnectHandler(AppCompatRadioButton nativeView)
		{
			CheckedChangeListener.Handler = null;
			nativeView.SetOnCheckedChangeListener(null);
		}

		public static void MapIsChecked(RadioButtonHandler handler, IRadioButton radioButton)
		{
			handler.TypedNativeView?.UpdateIsChecked(radioButton);
		}

		void UpdateIsChecked(bool isChecked)
		{
			if (VirtualView == null)
				return;

			VirtualView.IsChecked = isChecked;
		}

		internal class CompoundButtonCheckedChangeListener : Java.Lang.Object, CompoundButton.IOnCheckedChangeListener
		{
			public RadioButtonHandler? Handler { get; set; }

			public void OnCheckedChanged(CompoundButton? buttonView, bool isChecked)
			{
				if (Handler == null || buttonView == null)
					return;

				Handler.UpdateIsChecked(isChecked);
			}
		}
	}
}