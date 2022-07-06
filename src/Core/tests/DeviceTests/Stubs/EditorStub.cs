using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class EditorStub : StubBase, IEditor, ITextInputStub
	{
		private string _text;

		public string Text
		{
			get => _text;
			set => SetProperty(ref _text, value, onChanged: OnTextChanged);
		}

		public Color TextColor { get; set; }

		public Font Font { get; set; }

		public double CharacterSpacing { get; set; }

		public bool IsReadOnly { get; set; }

		public string Placeholder { get; set; }

		public Color PlaceholderColor { get; set; }

		public int MaxLength { get; set; } = int.MaxValue;

		public bool IsTextPredictionEnabled { get; set; } = true;

		public Keyboard Keyboard { get; set; } = Keyboard.Default;

		public TextAlignment HorizontalTextAlignment { get; set; }

		public TextAlignment VerticalTextAlignment { get; set; }

		public int CursorPosition { get; set; }

		public int SelectionLength { get; set; }

		public event EventHandler<StubPropertyChangedEventArgs<string>> TextChanged;

		void OnTextChanged(string oldValue, string newValue) =>
			TextChanged?.Invoke(this, new StubPropertyChangedEventArgs<string>(oldValue, newValue));

		public event EventHandler Completed;

		void IEditor.Completed() =>
			Completed?.Invoke(this, EventArgs.Empty);
	}
}
