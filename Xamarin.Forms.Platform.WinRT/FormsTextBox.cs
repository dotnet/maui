using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	/// <summary>
	///     An intermediate class for injecting bindings for things the default
	///     textbox doesn't allow us to bind/modify
	/// </summary>
	public class FormsTextBox : TextBox
	{
		const char ObfuscationCharacter = '●';

		public static readonly DependencyProperty PlaceholderForegroundBrushProperty = DependencyProperty.Register(nameof(PlaceholderForegroundBrush), typeof(Brush), typeof(FormsTextBox),
			new PropertyMetadata(default(Brush)));

		public static readonly DependencyProperty PlaceholderForegroundFocusBrushProperty = DependencyProperty.Register(nameof(PlaceholderForegroundFocusBrush), typeof(Brush), typeof(FormsTextBox),
			new PropertyMetadata(default(Brush)));

		public static readonly DependencyProperty ForegroundFocusBrushProperty = DependencyProperty.Register(nameof(ForegroundFocusBrush), typeof(Brush), typeof(FormsTextBox), new PropertyMetadata(default(Brush)));

		public static readonly DependencyProperty BackgroundFocusBrushProperty = DependencyProperty.Register(nameof(BackgroundFocusBrush), typeof(Brush), typeof(FormsTextBox), new PropertyMetadata(default(Brush)));

		public static readonly DependencyProperty IsPasswordProperty = DependencyProperty.Register(nameof(IsPassword), typeof(bool), typeof(FormsTextBox), new PropertyMetadata(default(bool), OnIsPasswordChanged));

		public new static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(FormsTextBox), new PropertyMetadata("", TextPropertyChanged));

		InputScope passwordInputScope;
		Border _borderElement;
		InputScope _cachedInputScope;
		bool _cachedPredictionsSetting;
		bool _cachedSpellCheckSetting;
		CancellationTokenSource _cts;
		bool _internalChangeFlag;

		public FormsTextBox()
		{
			TextChanged += OnTextChanged;
			SelectionChanged += OnSelectionChanged;
		}

		public Brush BackgroundFocusBrush
		{
			get { return (Brush)GetValue(BackgroundFocusBrushProperty); }
			set { SetValue(BackgroundFocusBrushProperty, value); }
		}

		public Brush ForegroundFocusBrush
		{
			get { return (Brush)GetValue(ForegroundFocusBrushProperty); }
			set { SetValue(ForegroundFocusBrushProperty, value); }
		}

		public bool IsPassword
		{
			get { return (bool)GetValue(IsPasswordProperty); }
			set { SetValue(IsPasswordProperty, value); }
		}

		public Brush PlaceholderForegroundBrush
		{
			get { return (Brush)GetValue(PlaceholderForegroundBrushProperty); }
			set { SetValue(PlaceholderForegroundBrushProperty, value); }
		}

		public Brush PlaceholderForegroundFocusBrush
		{
			get { return (Brush)GetValue(PlaceholderForegroundFocusBrushProperty); }
			set { SetValue(PlaceholderForegroundFocusBrushProperty, value); }
		}

		public new string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		InputScope PasswordInputScope
		{
			get
			{
				if (passwordInputScope != null)
				{
					return passwordInputScope;
				}

				passwordInputScope = new InputScope();
				var name = new InputScopeName { NameValue = InputScopeNameValue.Default };
				passwordInputScope.Names.Add(name);

				return passwordInputScope;
			}
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (Device.Idiom == TargetIdiom.Phone)
			{
				// If we're on the phone, we need to grab this from the template
				// so we can manually handle its background when focused
				_borderElement = (Border)GetTemplateChild("BorderElement");
			}
		}

		protected override void OnGotFocus(RoutedEventArgs e)
		{
			base.OnGotFocus(e);

#if !WINDOWS_UWP
			// If we're on Windows 8.1 phone, the Visual State Manager crashes if we try to 
			// handle alternate background colors in the focus state; we have to do
			// it manually here
			if (Device.Idiom == TargetIdiom.Phone && _borderElement != null)
			{
				_borderElement.Background = BackgroundFocusBrush;
			}
#endif
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
				{
					_cts = new CancellationTokenSource();
				}

				Task.Run(async () =>
				{
					await Task.Delay(TimeSpan.FromSeconds(0.5), _cts.Token);
					_cts.Token.ThrowIfCancellationRequested();
					await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
					{
						base.Text = Obfuscate();
						SelectionStart = base.Text.Length;
					});
				}, _cts.Token);
			}

			if (base.Text == newText)
			{
				return;
			}

			base.Text = newText;
			SelectionStart = base.Text.Length;
		}

		static string DetermineTextFromPassword(string realText, string passwordText)
		{
			if (realText.Length == passwordText.Length)
			{
				return realText;
			}

			if (passwordText.Length == 0)
			{
				return "";
			}

			if (passwordText.Length < realText.Length)
			{
				return realText.Substring(0, passwordText.Length);
			}

			int lengthDifference = passwordText.Length - realText.Length;

			return realText + passwordText.Substring(passwordText.Length - lengthDifference, lengthDifference);
		}

		string Obfuscate(bool leaveLastVisible = false)
		{
			if (leaveLastVisible && Text.Length == 1)
			{
				return Text;
			}

			if (leaveLastVisible && Text.Length > 1)
			{
				return new string(ObfuscationCharacter, Text.Length - 1) + Text.Substring(Text.Length - 1, 1);
			}

			return new string(ObfuscationCharacter, Text.Length);
		}

		static void OnIsPasswordChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			var textBox = (FormsTextBox)dependencyObject;
			textBox.UpdateInputScope();
			textBox.SyncBaseText();
		}

		void OnSelectionChanged(object sender, RoutedEventArgs routedEventArgs)
		{
			if (!IsPassword)
			{
				return;
			}

			// Prevent the user from selecting any text in the password box by forcing all selection
			// to zero-length at the end of the Text
			// This simulates the "do not allow clipboard copy" behavior the PasswordBox control has
			if (SelectionLength > 0 || SelectionStart < Text.Length)
			{
				SelectionLength = 0;
				SelectionStart = Text.Length;
			}
		}

		void OnTextChanged(object sender, Windows.UI.Xaml.Controls.TextChangedEventArgs textChangedEventArgs)
		{
			if (IsPassword)
			{
				// If we're not on a phone, we can just obfuscate any input
				if (Device.Idiom != TargetIdiom.Phone)
				{
					string updatedRealText = DetermineTextFromPassword(Text, base.Text);

					if (Text == updatedRealText)
					{
						// Nothing to do
						return;
					}

					Text = updatedRealText;

					string updatedText = Obfuscate();

					if (base.Text != updatedText)
					{
						base.Text = updatedText;
						SelectionStart = base.Text.Length;
					}

					return;
				}

				// If we are on the phone, we might need to delay obfuscating the last character
				DelayObfuscation();
			}
			else if (base.Text != Text)
			{
				// Not in password mode, so we just need to make the "real" text match
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
			{
				return;
			}

			base.Text = IsPassword ? Obfuscate() : Text;

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
				_cachedSpellCheckSetting = IsSpellCheckEnabled;
				_cachedPredictionsSetting = IsTextPredictionEnabled;
				InputScope = PasswordInputScope; // Change to default input scope so we don't have suggestions, etc.
				IsTextPredictionEnabled = false; // Force the other text modification options off
				IsSpellCheckEnabled = false;
			}
			else
			{
				InputScope = _cachedInputScope;
				IsSpellCheckEnabled = _cachedSpellCheckSetting;
				IsTextPredictionEnabled = _cachedPredictionsSetting;
			}
		}
	}
}