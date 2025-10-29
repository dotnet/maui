#nullable disable
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/OrientationStateTrigger.xml" path="Type[@FullName='Microsoft.Maui.Controls.OrientationStateTrigger']/Docs/*" />
	public sealed class OrientationStateTrigger : StateTriggerBase
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/OrientationStateTrigger.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public OrientationStateTrigger()
		{
			UpdateState();
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/OrientationStateTrigger.xml" path="//Member[@MemberName='Orientation']/Docs/*" />
		public DisplayOrientation Orientation
		{
			get => (DisplayOrientation)GetValue(OrientationProperty);
			set => SetValue(OrientationProperty, value);
		}

		/// <summary>Bindable property for <see cref="Orientation"/>.</summary>
		public static readonly BindableProperty OrientationProperty =
			BindableProperty.Create(nameof(Orientation), typeof(DisplayOrientation), typeof(OrientationStateTrigger), DisplayOrientation.Unknown,
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
			else if (Orientation.IsPortrait())
				SetActive(currentOrientation.IsPortrait());
			else
				SetActive(false);
		}
	}
}