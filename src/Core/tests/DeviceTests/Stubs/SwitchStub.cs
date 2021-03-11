using System;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class SwitchStub : StubBase, ISwitch
	{
		public Action ToggledDelegate;
		Color _thumbColor;
		Color _trackColor;

		public bool IsToggled { get; set; }
		public Color TrackColor
		{
			get => _trackColor;
			set => SetProperty(ref _trackColor, value);
		}

		public Color ThumbColor
		{
			get => _thumbColor;
			set => SetProperty(ref _thumbColor, value);
		}

		public void Toggled() => ToggledDelegate?.Invoke();
	}
}