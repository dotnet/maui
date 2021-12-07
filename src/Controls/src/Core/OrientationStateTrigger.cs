using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Essentials;

namespace Microsoft.Maui.Controls
{
	public sealed class OrientationStateTrigger : StateTriggerBase
	{
		public OrientationStateTrigger()
		{
			UpdateState();
		}

		public DisplayOrientation Orientation
		{
			get => (DisplayOrientation)GetValue(OrientationProperty);
			set => SetValue(OrientationProperty, value);
		}

		public static readonly BindableProperty OrientationProperty =
			BindableProperty.Create(nameof(Orientation), typeof(DisplayOrientation), typeof(OrientationStateTrigger), null,
				propertyChanged: OnOrientationChanged);

		static void OnOrientationChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			((OrientationStateTrigger)bindable).UpdateState();
		}

		protected override void OnAttached()
		{
			base.OnAttached();

			if (!DesignMode.IsDesignModeEnabled)
			{
				UpdateState();
				DeviceDisplay.MainDisplayInfoChanged += OnInfoPropertyChanged;
			}
		}

		protected override void OnDetached()
		{
			base.OnDetached();

			DeviceDisplay.MainDisplayInfoChanged -= OnInfoPropertyChanged;
		}

		void OnInfoPropertyChanged(object sender, DisplayInfoChangedEventArgs e) =>
			UpdateState();

		void UpdateState()
		{
			var currentOrientation = DeviceDisplay.MainDisplayInfo.Orientation;
			if (Orientation.IsLandscape())
				SetActive(currentOrientation.IsLandscape());
			else
				SetActive(currentOrientation.IsPortrait());
		}
	}
}