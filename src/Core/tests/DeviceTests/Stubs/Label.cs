using Microsoft.Maui;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class LabelStub : StubBase, ILabel
	{
		public string Text { get; set; }

		public Color TextColor { get; set; }

		public FontAttributes FontAttributes => throw new System.NotImplementedException();

		public string FontFamily => throw new System.NotImplementedException();

		public double FontSize => throw new System.NotImplementedException();
	}
}