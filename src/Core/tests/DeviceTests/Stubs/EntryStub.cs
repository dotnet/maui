namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class EntryStub : StubBase, IEntry
	{
		private string _text;

		public string Text { get => _text; set => this.SetProperty(ref _text, value); }

		public Color TextColor { get; set; }

		public bool IsPassword { get; set; }

		public bool IsTextPredictionEnabled { get; set; }

		public string Placeholder { get; set; }

		public Keyboard Keyboard { get; set; }

		public bool IsSpellCheckEnabled { get; set; }

		public int MaxLength { get; set; }

		public bool IsReadOnly { get; set; }
	}
}