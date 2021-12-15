namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class RadioButtonStub : StubBase, IRadioButton
	{
		public bool IsChecked { get; set; }

		public TextType TextType { get; set; } = TextType.Text;

		public Color TextColor { get; set; }

		public double CharacterSpacing { get; set; }

		public Font Font { get; set; }
	}
}