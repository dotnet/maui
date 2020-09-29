using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using WBrush = System.Windows.Media.Brush;

namespace Xamarin.Forms.Platform.WPF
{
	/// <summary>
	///     An intermediate class for injecting bindings for things the default
	///     textbox doesn't allow us to bind/modify
	/// </summary>
	public class FormsTextBox : TextBox
	{
		const char ObfuscationCharacter = '●';

		public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register("PlaceholderText", typeof(string), typeof(FormsTextBox),
			new PropertyMetadata(string.Empty));

		public static readonly DependencyProperty PlaceholderForegroundBrushProperty = DependencyProperty.Register("PlaceholderForegroundBrush", typeof(WBrush), typeof(FormsTextBox),
			new PropertyMetadata(default(WBrush)));

		public static readonly DependencyProperty IsPasswordProperty = DependencyProperty.Register("IsPassword", typeof(bool), typeof(FormsTextBox),
			new PropertyMetadata(default(bool), OnIsPasswordChanged));

		public new static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(FormsTextBox),
			new PropertyMetadata("", TextPropertyChanged));

		protected internal static readonly DependencyProperty DisabledTextProperty = DependencyProperty.Register("DisabledText", typeof(string), typeof(FormsTextBox),
			new PropertyMetadata(""));

		static InputScope s_passwordInputScope;
		InputScope _cachedInputScope;
		CancellationTokenSource _cts;
		bool _internalChangeFlag;
		int _cachedSelectionLength;

		public FormsTextBox()
		{
			TextChanged += OnTextChanged;
			SelectionChanged += OnSelectionChanged;
		}

		public bool IsPassword
		{
			get { return (bool)GetValue(IsPasswordProperty); }
			set { SetValue(IsPasswordProperty, value); }
		}

		public string PlaceholderText
		{
			get { return (string)GetValue(PlaceholderTextProperty); }
			set { SetValue(PlaceholderTextProperty, value); }
		}

		public WBrush PlaceholderForegroundBrush
		{
			get { return (WBrush)GetValue(PlaceholderForegroundBrushProperty); }
			set { SetValue(PlaceholderForegroundBrushProperty, value); }
		}

		public new string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		protected internal string DisabledText
		{
			get { return (string)GetValue(DisabledTextProperty); }
			set { SetValue(DisabledTextProperty, value); }
		}

		static InputScope PasswordInputScope
		{
			get
			{
				if (s_passwordInputScope != null)
					return s_passwordInputScope;

				s_passwordInputScope = new InputScope();
				var name = new InputScopeName { NameValue = InputScopeNameValue.Default };
				s_passwordInputScope.Names.Add(name);

				return s_passwordInputScope;
			}
		}

		void DelayObfuscation()
		{
			int lengthDifference = base.Text.Length - Text.Length;

			var savedSelectionStart = SelectionStart;
			string updatedRealText = DetermineTextFromPassword(Text, SelectionStart, base.Text);

			if (Text == updatedRealText)
			{
				// Nothing to do
				return;
			}

			_internalChangeFlag = true;
			Text = updatedRealText;
			_internalChangeFlag = false;

			// Cancel any pending delayed obfuscation
			_cts?.Cancel();
			_cts = null;

			string newText;

			if (lengthDifference != 1)
			{
				// Either More than one character got added in this text change (e.g., a paste operation)
				// Or characters were removed. Either way, we don't need to do the delayed obfuscation dance
				newText = Obfuscate(Text);
			}
			else
			{
				// Only one character was added; we need to leave it visible for a brief time period
				// Obfuscate all but the character added for now
				newText = Obfuscate(Text, savedSelectionStart - 1);

				// Leave the added character visible until a new character is added
				// or sufficient time has passed
				if (_cts == null)
				{
					_cts = new CancellationTokenSource();
				}

				Task.Run(async () =>
				{
					await Task.Delay(TimeSpan.FromSeconds(0.5), _cts.Token);
					_cts.Token.ThrowIfCancellationRequested();
					await Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
					{
						var ss = SelectionStart;
						var sl = SelectionLength;
						base.Text = Obfuscate(Text);
						SelectionStart = ss;
						SelectionLength = sl;
					}));
				}, _cts.Token);
			}

			if (base.Text != newText)
			{
				base.Text = newText;
			}
			SelectionStart = savedSelectionStart;
		}

		static string DetermineTextFromPassword(string realText, int start, string passwordText)
		{
			var lengthDifference = passwordText.Length - realText.Length;
			if (lengthDifference > 0)
				realText = realText.Insert(start - lengthDifference, new string(ObfuscationCharacter, lengthDifference));
			else if (lengthDifference < 0)
				realText = realText.Remove(start, -lengthDifference);

			var sb = new System.Text.StringBuilder(passwordText.Length);
			for (int i = 0; i < passwordText.Length; i++)
				sb.Append(passwordText[i] == ObfuscationCharacter ? realText[i] : passwordText[i]);

			return sb.ToString();
		}

		string Obfuscate(string text, int visibleSymbolIndex = -1)
		{
			if (visibleSymbolIndex == -1)
				return new string(ObfuscationCharacter, text?.Length ?? 0);

			if (text == null || text.Length == 1)
				return text;
			var prefix = visibleSymbolIndex > 0 ? new string(ObfuscationCharacter, visibleSymbolIndex) : string.Empty;
			var suffix = visibleSymbolIndex == text.Length - 1
				? string.Empty
				: new string(ObfuscationCharacter, text.Length - visibleSymbolIndex - 1);

			return prefix + text.Substring(visibleSymbolIndex, 1) + suffix;
		}

		static void OnIsPasswordChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{
			var textBox = (FormsTextBox)dependencyObject;
			textBox.UpdateInputScope();
			textBox.SyncBaseText();
		}

		void OnSelectionChanged(object sender, RoutedEventArgs routedEventArgs)
		{
			// Cache this value for later use as explained in OnKeyDown below
			_cachedSelectionLength = SelectionLength;
		}

		// Because the implementation of a password entry is based around inheriting from TextBox (via FormsTextBox), there
		// are some inaccuracies in the behavior. OnKeyDown is what needs to be used for a workaround in this case because 
		// there's no easy way to disable specific keyboard shortcuts in a TextBox, so key presses are being intercepted and 
		// handled accordingly.
		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (IsPassword)
			{
				// The ctrlDown flag is used to track if the Ctrl key is pressed; if it's actively being used and the most recent
				// key to trigger OnKeyDown, then treat it as handled.
				var ctrlDown = (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) && e.IsDown;

				// The shift, tab, and directional (Home/End/PgUp/PgDown included) keys can be used to select text and should otherwise
				// be ignored.
				if (
					e.Key == Key.LeftShift ||
					e.Key == Key.RightShift ||
					e.Key == Key.Tab ||
					e.Key == Key.Left ||
					e.Key == Key.Right ||
					e.Key == Key.Up ||
					e.Key == Key.Down ||
					e.Key == Key.Home ||
					e.Key == Key.End ||
					e.Key == Key.PageUp ||
					e.Key == Key.PageDown)
				{
					base.OnKeyDown(e);
					return;
				}
				// For anything else, continue on (calling base.OnKeyDown) and then if Ctrl is still being pressed, do nothing about it.
				// The tricky part here is that the SelectionLength value needs to be cached because in an example where the user entered
				// '123' into the field and selects all of it, the moment that any character key is pressed to replace the entire string,
				// the SelectionLength is equal to zero, which is not what's desired. Entering a key will thus remove the selected number
				// of characters from the Text value. OnKeyDown is fortunately called before OnSelectionChanged which enables this.
				else
				{
					// If the C or X keys (copy/cut) are pressed while Ctrl is active, ignore handing them at all. Undo and Redo (Z/Y) should 
					// be ignored as well as this emulates the regular behavior of a PasswordBox.
					if ((e.Key == Key.C || e.Key == Key.X || e.Key == Key.Z || e.Key == Key.Y) && ctrlDown)
					{
						e.Handled = false;
						return;
					}

					base.OnKeyDown(e);
					if (_cachedSelectionLength > 0 && !ctrlDown)
					{
						var savedSelectionStart = SelectionStart;
						Text = Text.Remove(SelectionStart, _cachedSelectionLength);
						SelectionStart = savedSelectionStart;
					}
				}
			}
			else
				base.OnKeyDown(e);
		}

		void OnTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs textChangedEventArgs)
		{
			if (IsPassword)
			{
				DelayObfuscation();
			}
			else if (base.Text != Text)
			{
				// Not in password mode, so we just need to make the "real" Text match
				// what's in the textbox; the internalChange flag keeps the TextProperty
				// synchronization from happening 
				_internalChangeFlag = true;
				Text = base.Text;
				_internalChangeFlag = false;
			}
		}

		void SyncBaseText()
		{
			if (_internalChangeFlag)
				return;
			var savedSelectionStart = SelectionStart;
			base.Text = IsPassword ? Obfuscate(Text) : Text;
			DisabledText = base.Text;
			var len = base.Text.Length;
			SelectionStart = savedSelectionStart > len ? len : savedSelectionStart;
		}

		static void TextPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{
			var textBox = (FormsTextBox)dependencyObject;
			textBox.SyncBaseText();
		}

		void UpdateInputScope()
		{
			if (IsPassword)
			{
				_cachedInputScope = InputScope;
				InputScope = PasswordInputScope; // We don't want suggestions turned on if we're in password mode
			}
			else
				InputScope = _cachedInputScope;
		}
	}
}