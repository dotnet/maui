using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class EntryStub : StubBase, IEntry
	{
		private string _text;

		public string Text
		{
			get => _text;
			set => SetProperty(ref _text, value, onChanged: OnTextChanged);
		}

		public Color TextColor { get; set; }

		public double CharacterSpacing { get; set; }

		public bool IsPassword { get; set; }

		public bool IsTextPredictionEnabled { get; set; } = true;

		public string Placeholder { get; set; }

		public bool IsReadOnly { get; set; }

		public Font Font { get; set; }

		public int MaxLength { get; set; } = int.MaxValue;

		public TextAlignment HorizontalTextAlignment { get; set; }

		public ReturnType ReturnType { get; set; }

		public ClearButtonVisibility ClearButtonVisibility { get; set; }

		public Keyboard Keyboard { get; set; } = Keyboard.Default;

		public event EventHandler<StubPropertyChangedEventArgs<string>> TextChanged;

		public event EventHandler Completed;

		void IEntry.Completed() =>
			Completed?.Invoke(this, EventArgs.Empty);

		void OnTextChanged(string oldValue, string newValue) =>
			TextChanged?.Invoke(this, new StubPropertyChangedEventArgs<string>(oldValue, newValue));
	}
}