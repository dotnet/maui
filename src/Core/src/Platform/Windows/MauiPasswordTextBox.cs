#nullable enable
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;
using Windows.UI.Core;

namespace Microsoft.Maui.Platform
{
	// TODO: Replace this all with a real PasswordBox and not do this
	//       as we lose many default password box features.
	public class MauiPasswordTextBox : TextBox
	{
		const char ObfuscationCharacter = '●';

		public static readonly DependencyProperty IsPasswordProperty = DependencyProperty.Register(
			nameof(IsPassword), typeof(bool), typeof(MauiPasswordTextBox),
			new PropertyMetadata(default(bool), OnIsPasswordChanged));

		public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register(
			nameof(Password), typeof(string), typeof(MauiPasswordTextBox),
			new PropertyMetadata("", OnPasswordPropertyChanged));

		public static readonly DependencyProperty IsObfuscationDelayedProperty = DependencyProperty.Register(
			nameof(IsObfuscationDelayed), typeof(bool), typeof(MauiPasswordTextBox),
			new PropertyMetadata(false));

		static void OnIsPasswordChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			if (dependencyObject is MauiPasswordTextBox textBox)
			{
				textBox.UpdateInputScope();
				textBox.UpdateVisibleText();
			}
		}

		static void OnPasswordPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			if (dependencyObject is MauiPasswordTextBox textBox)
				textBox.UpdateVisibleText();
		}

		InputScope? _passwordInputScope;
		InputScope? _numericPasswordInputScope;
		InputScope? _cachedInputScope;
		bool _cachedPredictionsSetting;
		bool _cachedSpellCheckSetting;
		CancellationTokenSource? _cts;
		bool _internalChangeFlag;
		int _cachedCursorPosition;
		int _cachedTextLength;

		public MauiPasswordTextBox()
		{
			TextChanging += OnNativeTextChanging;
			TextChanged += OnNativeTextChanged;
		}

		public static bool IsPassword
		{
			get => (bool)GetValue(IsPasswordProperty);
			set => SetValue(IsPasswordProperty, value);
		}

		public static string Password
		{
			get => (string)GetValue(PasswordProperty);
			set => SetValue(PasswordProperty, value);
		}

		public static bool IsObfuscationDelayed
		{
			get => (bool)GetValue(IsObfuscationDelayedProperty);
			set => SetValue(IsObfuscationDelayedProperty, value);
		}

		InputScope PasswordInputScope =>
			_passwordInputScope ??= CreateInputScope(InputScopeNameValue.Password);

		InputScope NumericPasswordInputScope =>
			_numericPasswordInputScope ??= CreateInputScope(InputScopeNameValue.NumericPassword);

		// Because the implementation of a password entry is based around inheriting from TextBox (via MauiPasswordTextBox), there
		// are some inaccuracies in the behavior. OnKeyDown is what needs to be used for a workaround in this case because 
		// there's no easy way to disable specific keyboard shortcuts in a TextBox, so key presses are being intercepted and 
		// handled accordingly.
		protected override void OnKeyDown(KeyRoutedEventArgs e)
		{
			if (!MauiPasswordTextBox.IsPassword)
			{
				base.OnKeyDown(e);
				return;
			}

			// The shift, tab, and directional (Home/End/PgUp/PgDown included) keys can be used to select text and should otherwise
			// be ignored.
			if (e.Key == VirtualKey.Shift ||
				e.Key == VirtualKey.Tab ||
				e.Key == VirtualKey.Left ||
				e.Key == VirtualKey.Right ||
				e.Key == VirtualKey.Up ||
				e.Key == VirtualKey.Down ||
				e.Key == VirtualKey.Home ||
				e.Key == VirtualKey.End ||
				e.Key == VirtualKey.PageUp ||
				e.Key == VirtualKey.PageDown)
			{
				base.OnKeyDown(e);
				return;
			}

			// For anything else, continue on (calling base.OnKeyDown) and then if Ctrl is still being pressed, do nothing about it.
			// The tricky part here is that the SelectionLength value needs to be cached because in an example where the user entered
			// '123' into the field and selects all of it, the moment that any character key is pressed to replace the entire string,
			// the SelectionLength is equal to zero, which is not what's desired. Entering a key will thus remove the selected number
			// of characters from the Text value. OnKeyDown is fortunately called before OnSelectionChanged which enables this.
			// The ctrlDown flag is used to track if the Ctrl key is pressed; if it's actively being used and the most recent
			// key to trigger OnKeyDown, then treat it as handled.
			var ctrlDown = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down);

			// If the C or X keys (copy/cut) are pressed while Ctrl is active, ignore handing them at all. Undo and Redo (Z/Y) should
			// be ignored as well as this emulates the regular behavior of a PasswordBox.
			if ((e.Key == VirtualKey.C || e.Key == VirtualKey.X || e.Key == VirtualKey.Z || e.Key == VirtualKey.Y) && ctrlDown)
			{
				e.Handled = false;
				return;
			}

			base.OnKeyDown(e);
		}

		private void OnNativeTextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
		{
			// We are handling the CursorPosition issue at Platform.TextBoxExtensions.UpdateText method
			// but when IsPassword=true, it is too late to handle it at that moment, as we have already
			// obfuscated the text and have lost the real CursorPosition value. So, let's handle that
			// issue here when IsPassword is enabled.
			if (!MauiPasswordTextBox.IsPassword)
				return;

			// As we are obfuscating the text by ourselves we are setting the Text property directly on code many times.
			// This causes that we invoke the SelectionChanged event many times with SelectionStart = 0, setting the cursor
			// to the beginning of the TextBox.
			// To avoid this behavior let's save the current cursor position of the first time the Text is updated by the user
			// and keep the same cursor position after each Text update until a new Text update by the user happens.
			var updatedPassword = DetermineTextFromPassword(MauiPasswordTextBox.Password, SelectionStart, Text);

			if (MauiPasswordTextBox.Password != updatedPassword)
			{
				_cachedCursorPosition = SelectionStart;
				_cachedTextLength = updatedPassword.Length;
			}
			else
			{
				// Recalculate the cursor position, as the Text could be modified by a user Converter
				_cachedCursorPosition += (updatedPassword.Length - _cachedTextLength);
				SelectionStart = this.GetCursorPosition(_cachedCursorPosition);
			}
		}

		void OnNativeTextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
		{
			if (MauiPasswordTextBox.IsPassword)
			{
				if (MauiPasswordTextBox.IsObfuscationDelayed)
					DelayObfuscation();
				else
					ImmediateObfuscation();
			}
			else if (Text != MauiPasswordTextBox.Password)
			{
				// Not in password mode, so we just need to make the "real" text match
				// what's in the textbox; the internalChange flag keeps the TextProperty
				// synchronization from happening

				_internalChangeFlag = true;
				MauiPasswordTextBox.Password = Text;
				_internalChangeFlag = false;
			}
		}

		void UpdateVisibleText()
		{
			if (_internalChangeFlag)
				return;

			var updatedText = MauiPasswordTextBox.IsPassword ? Obfuscate(MauiPasswordTextBox.Password) : MauiPasswordTextBox.Password;

			if (Text != updatedText)
				Text = updatedText;
		}

		void UpdateInputScope()
		{
			if (!MauiPasswordTextBox.IsPassword)
			{
				InputScope = _cachedInputScope;
				IsSpellCheckEnabled = _cachedSpellCheckSetting;
				IsTextPredictionEnabled = _cachedPredictionsSetting;
			}
			else
			{
				_cachedInputScope = InputScope;
				_cachedSpellCheckSetting = IsSpellCheckEnabled;
				_cachedPredictionsSetting = IsTextPredictionEnabled;

				if (InputScope != null && InputScope.Names.Any(i => i.NameValue == InputScopeNameValue.Number))
				{
					InputScope = NumericPasswordInputScope;
				}
				else
				{
					InputScope = PasswordInputScope; // Change to default input scope so we don't have suggestions, etc.
				}

				IsTextPredictionEnabled = false; // Force the other text modification options off
				IsSpellCheckEnabled = false;
			}
		}

		void ImmediateObfuscation()
		{
			UpdatePasswordIfNeeded();
		}

		void DelayObfuscation()
		{
			var lengthDifference = Text.Length - MauiPasswordTextBox.Password.Length;

			UpdatePasswordIfNeeded();

			// Cancel any pending delayed obfuscation
			_cts?.Cancel();
			_cts = null;

			string updatedVisibleText;
			if (lengthDifference != 1)
			{
				// Either More than one character got added in this text change (e.g., a paste operation)
				// Or characters were removed. Either way, we don't need to do the delayed obfuscation dance
				updatedVisibleText = Obfuscate(MauiPasswordTextBox.Password);
			}
			else
			{
				// Only one character was added; we need to leave it visible for a brief time period
				// Obfuscate all but the last character for now
				updatedVisibleText = Obfuscate(MauiPasswordTextBox.Password, true);

				// Leave the last character visible until a new character is added
				// or sufficient time has passed
				_cts = new CancellationTokenSource();
				StartTimeout(_cts.Token);
			}

			if (Text != updatedVisibleText)
				Text = updatedVisibleText;

			void StartTimeout(CancellationToken token)
			{
				Task.Run(async () =>
				{
					await Task.Delay(TimeSpan.FromSeconds(0.5), token);

					token.ThrowIfCancellationRequested();

					DispatcherQueue.TryEnqueue(UI.Dispatching.DispatcherQueuePriority.Normal, () =>
					{
						UpdateVisibleText();
					});
				}, token);
			}
		}

		void UpdatePasswordIfNeeded()
		{
			var updatedPassword = DetermineTextFromPassword(MauiPasswordTextBox.Password, SelectionStart, Text);

			if (MauiPasswordTextBox.Password != updatedPassword)
				MauiPasswordTextBox.Password = updatedPassword;
		}

		static string Obfuscate(string text, bool leaveLastVisible = false)
		{
			if (string.IsNullOrEmpty(text))
				return string.Empty;

			if (!leaveLastVisible)
				return new string(ObfuscationCharacter, text.Length);

			return text.Length == 1
				? text
				: string.Concat(new string(ObfuscationCharacter, text.Length - 1), text.AsSpan(text.Length - 1, 1));
		}

		static string DetermineTextFromPassword(string realText, int start, string passwordText)
		{
			realText ??= string.Empty;

			var lengthDifference = passwordText.Length - realText.Length;
			if (lengthDifference > 0)
				realText = realText.Insert(start - lengthDifference, new string(ObfuscationCharacter, lengthDifference));
			else if (lengthDifference < 0)
				realText = realText.Remove(start, -lengthDifference);

			var sb = new StringBuilder(passwordText.Length);
			for (int i = 0; i < passwordText.Length; i++)
				sb.Append((char)(passwordText[i] == ObfuscationCharacter ? realText[i] : passwordText[i]));

			return sb.ToString();
		}

		static InputScope CreateInputScope(InputScopeNameValue value) =>
			new InputScope
			{
				Names =
				{
					new InputScopeName { NameValue = value }
				}
			};
	}
}