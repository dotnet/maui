using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class CheckBoxStub : StubBase, ICheckBox
	{
		public bool IsChecked { get; set; }
		public Paint Foreground { get; set; }
	}
}