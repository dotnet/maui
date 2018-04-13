using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Threading;

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

		public static readonly DependencyProperty PlaceholderForegroundBrushProperty = DependencyProperty.Register("PlaceholderForegroundBrush", typeof(Brush), typeof(FormsTextBox),
			new PropertyMetadata(default(Brush)));

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
			get { return (string)GetValue (PlaceholderTextProperty); }
			set { SetValue (PlaceholderTextProperty, value); }
		}

		public Brush PlaceholderForegroundBrush
		{
			get { return (Brush)GetValue(PlaceholderForegroundBrushProperty); }
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

			string updatedRealText = DetermineTextFromPassword(Text, base.Text);

			if (Text == updatedRealText)
			{
				// Nothing to do
				return;
			}

			Text = updatedRealText;

			// Cancel any pending delayed obfuscation
			_cts?.Cancel();
			_cts = null;

			string newText;

			if (lengthDifference != 1)
			{
				// Either More than one character got added in this text change (e.g., a paste operation)
				// Or characters were removed. Either way, we don't need to do the delayed obfuscation dance
				newText = Obfuscate();
			}
			else
			{
				// Only one character was added; we need to leave it visible for a brief time period
				// Obfuscate all but the last character for now
				newText = Obfuscate(true);

				// Leave the last character visible until a new character is added
				// or sufficient time has passed
				if (_cts == null)
					_cts = new CancellationTokenSource();

				Task.Run(async () =>
				{
					await Task.Delay(TimeSpan.FromSeconds(0.5), _cts.Token);
					_cts.Token.ThrowIfCancellationRequested();
					await Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
					{
						base.Text = Obfuscate();
						SelectionStart = base.Text.Length;
					}));
				}, _cts.Token);
			}

			if (base.Text == newText)
				return;

			base.Text = newText;
			SelectionStart = base.Text.Length;
		}

		static string DetermineTextFromPassword(string realText, string passwordText)
		{
			int firstObfuscationChar = passwordText.IndexOf(ObfuscationCharacter);

			if (firstObfuscationChar > 0)
			{
				// The user is typing faster than we can process, and the text is coming in at the beginning
				// of the textbox instead of the end
				passwordText = passwordText.Substring(firstObfuscationChar, passwordText.Length - firstObfuscationChar) + passwordText.Substring(0, firstObfuscationChar);
			}

			if (realText.Length == passwordText.Length)
				return realText;

			if (passwordText.Length == 0)
				return "";

			if (passwordText.Length < realText.Length)
				return realText.Substring(0, passwordText.Length);

			int lengthDifference = passwordText.Length - realText.Length;

			return realText + passwordText.Substring(passwordText.Length - lengthDifference, lengthDifference);
		}

		string Obfuscate(bool leaveLastVisible = false)
		{
			if (leaveLastVisible && Text.Length == 1)
				return Text;

			if (leaveLastVisible && Text.Length > 1)
				return new string(ObfuscationCharacter, Text.Length - 1) + Text.Substring(Text.Length - 1, 1);

			return new string(ObfuscationCharacter, Text.Length);
		}

		static void OnIsPasswordChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{
			var textBox = (FormsTextBox)dependencyObject;
			textBox.UpdateInputScope();
			textBox.SyncBaseText();
		}

		void OnSelectionChanged(object sender, RoutedEventArgs routedEventArgs)
		{
			if (!IsPassword)
				return;

			// Prevent the user from selecting any text in the password box by forcing all selection
			// to zero-length at the end of the text
			// This simulates the "do not allow clipboard copy" behavior the PasswordBox control has
			if (SelectionLength > 0 || SelectionStart < Text.Length)
			{
				if (SelectionLength != 0)
					SelectionLength = 0;
				SelectionStart = Text.Length;
			}
		}

		void OnTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs textChangedEventArgs)
		{
			if (IsPassword)
				DelayObfuscation();
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

			base.Text = IsPassword ? Obfuscate() : Text;
			DisabledText = base.Text;

			SelectionStart = base.Text.Length;
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