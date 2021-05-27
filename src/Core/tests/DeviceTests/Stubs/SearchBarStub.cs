using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class SearchBarStub : StubBase, ISearchBar
	{
		string _text;

		public string Text { get => _text; set => SetProperty(ref _text, value); }

		public string Placeholder { get; set; }

		public Color TextColor { get; set; }

		public double CharacterSpacing { get; set; }

		public Font Font { get; set; }

		public TextAlignment HorizontalTextAlignment { get; set; }

		public bool IsTextPredictionEnabled { get; set; } = true;

		public bool IsReadOnly { get; set; }

		public int MaxLength { get; set; }

		public Keyboard Keyboard { get; set; }

		public void SearchButtonPressed() { }
	}
}