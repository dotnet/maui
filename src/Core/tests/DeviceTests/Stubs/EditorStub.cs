namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class EditorStub : StubBase, IEditor
	{
		public string Text { get; set; }

		public Color TextColor { get; set; }

		public Font Font { get; set; }

		public double CharacterSpacing { get; set; }

		public int MaxLength { get; set; } = int.MaxValue;

		public bool IsTextPredictionEnabled { get; set; }
	}
}