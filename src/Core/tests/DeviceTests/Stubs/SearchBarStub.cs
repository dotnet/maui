using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class SearchBarStub : StubBase, ISearchBar, ITextInputStub
	{
		string _text;

		public string Text
		{
			get => _text;
			set => SetProperty(ref _text, value, onChanged: OnTextChanged);
		}

		public event EventHandler<(string OldValue, string NewValue)> TextChanged;

		void OnTextChanged(string oldValue, string newValue) =>
			TextChanged?.Invoke(this, (oldValue, newValue));

		public string Placeholder { get; set; }

		public Color PlaceholderColor { get; set; }

		public Color TextColor { get; set; }

		public Color CancelButtonColor { get; set; }

		public Color SearchIconColor { get; set; }

		public double CharacterSpacing { get; set; }

		public Font Font { get; set; }

		public TextAlignment HorizontalTextAlignment { get; set; }

		public TextAlignment VerticalTextAlignment { get; set; }

		public int CursorPosition { get; set; }

		public int SelectionLength { get; set; }

		public bool IsTextPredictionEnabled { get; set; } = true;

		public bool IsSpellCheckEnabled { get; set; } = true;

		public bool IsReadOnly { get; set; }

		public int MaxLength { get; set; } = int.MaxValue;

		public Keyboard Keyboard { get; set; }

		public ReturnType ReturnType { get; set; }

		public void SearchButtonPressed() { }
	}
}