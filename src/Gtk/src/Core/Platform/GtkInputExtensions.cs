namespace Microsoft.Maui.Gtk.Platform
{
	/// <summary>
	/// Maps MAUI keyboard and text-input flags to GTK <c>InputPurpose</c> / <c>InputHints</c>.
	/// </summary>
	public static class GtkInputExtensions
	{
		/// <summary>Maps a <see cref="Keyboard"/> to an input purpose.</summary>
		/// <param name="keyboard">The keyboard; <see langword="null"/> means the default.</param>
		public static global::Gtk.InputPurpose ToInputPurpose(Keyboard? keyboard)
		{
			if (ReferenceEquals(keyboard, Keyboard.Numeric))
				return global::Gtk.InputPurpose.Number;
			if (ReferenceEquals(keyboard, Keyboard.Email))
				return global::Gtk.InputPurpose.Email;
			if (ReferenceEquals(keyboard, Keyboard.Telephone))
				return global::Gtk.InputPurpose.Phone;
			if (ReferenceEquals(keyboard, Keyboard.Url))
				return global::Gtk.InputPurpose.Url;

			return global::Gtk.InputPurpose.FreeForm;
		}

		/// <summary>Maps the prediction/spell-check flags to input hints.</summary>
		/// <param name="input">The text input view.</param>
		public static global::Gtk.InputHints ToInputHints(ITextInput input)
		{
			var hints = global::Gtk.InputHints.None;
			hints |= input.IsSpellCheckEnabled ? global::Gtk.InputHints.Spellcheck : global::Gtk.InputHints.NoSpellcheck;
			if (input.IsTextPredictionEnabled)
				hints |= global::Gtk.InputHints.WordCompletion;
			return hints;
		}

		/// <summary>Applies keyboard, spell-check and prediction settings to an entry.</summary>
		/// <param name="entry">The entry.</param>
		/// <param name="keyboard">The keyboard.</param>
		/// <param name="input">The virtual view.</param>
		public static void UpdateKeyboard(global::Gtk.Entry entry, Keyboard? keyboard, ITextInput input)
		{
			entry.InputPurpose = entry.Visibility ? ToInputPurpose(keyboard) : global::Gtk.InputPurpose.Password;
			entry.InputHints = ToInputHints(input);
		}

		/// <summary>Applies keyboard, spell-check and prediction settings to a text view.</summary>
		/// <param name="view">The text view.</param>
		/// <param name="keyboard">The keyboard.</param>
		/// <param name="input">The virtual view.</param>
		public static void UpdateKeyboard(global::Gtk.TextView view, Keyboard? keyboard, ITextInput input)
		{
			view.InputPurpose = ToInputPurpose(keyboard);
			view.InputHints = ToInputHints(input);
		}
	}
}
