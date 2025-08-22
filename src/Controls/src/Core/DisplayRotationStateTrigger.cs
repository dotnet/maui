#nullable disable
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/DisplayRotationStateTrigger.xml" path="Type[@FullName='Microsoft.Maui.Controls.DisplayRotationStateTrigger']/Docs/*" />
	public sealed class DisplayRotationStateTrigger : StateTriggerBase
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/DisplayRotationStateTrigger.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public DisplayRotationStateTrigger()
		{
			UpdateState();
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/DisplayRotationStateTrigger.xml" path="//Member[@MemberName='Rotation']/Docs/*" />
		public DisplayRotation Rotation
		{
			get => (DisplayRotation)GetValue(RotationProperty);
			set => SetValue(RotationProperty, value);
		}

		/// <summary>Bindable property for <see cref="Rotation"/>.</summary>
		public static readonly BindableProperty RotationProperty =
			BindableProperty.Create(nameof(Rotation), typeof(DisplayRotation), typeof(DisplayRotationStateTrigger), DisplayRotation.Unknown,
				propertyChanged: OnRotationChanged);

		static void OnRotationChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			((DisplayRotationStateTrigger)bindable).UpdateState();
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
			var currentRotation = DeviceDisplay.MainDisplayInfo.Rotation;
			SetActive(currentRotation == Rotation);
		}
	}
}