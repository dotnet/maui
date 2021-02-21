using System;
using Xamarin.Forms;

namespace Xamarin.Platform.Handlers.DeviceTests.Stubs
{
	public partial class SwitchStub : StubBase, ISwitch
	{
		public Action ToggledDelegate;
		private Color _thumbColor;
		private Color _trackColor;

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