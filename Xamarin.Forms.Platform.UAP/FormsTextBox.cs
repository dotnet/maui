using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using WVisualStateManager = Windows.UI.Xaml.VisualStateManager;

namespace Xamarin.Forms.Platform.UWP
{
	/// <summary>
	///     An intermediate class for injecting bindings for things the default
	///     textbox doesn't allow us to bind/modify
	/// </summary>
	public class FormsTextBox : TextBox
	{
		const char ObfuscationCharacter = '●';

		public static readonly DependencyProperty PlaceholderForegroundBrushProperty = 
			DependencyProperty.Register(nameof(PlaceholderForegroundBrush), typeof(Brush), typeof(FormsTextBox),
				new PropertyMetadata(default(Brush), FocusPropertyChanged));

		public static readonly DependencyProperty PlaceholderForegroundFocusBrushProperty = 
			DependencyProperty.Register(nameof(PlaceholderForegroundFocusBrush), typeof(Brush), typeof(FormsTextBox),
				new PropertyMetadata(default(Brush), FocusPropertyChanged));

		public static readonly DependencyProperty ForegroundFocusBrushProperty = 
			DependencyProperty.Register(nameof(ForegroundFocusBrush), typeof(Brush), typeof(FormsTextBox), 
				new PropertyMetadata(default(Brush), FocusPropertyChanged));

		public static readonly DependencyProperty BackgroundFocusBrushProperty = 
			DependencyProperty.Register(nameof(BackgroundFocusBrush), typeof(Brush), typeof(FormsTextBox), 
				new PropertyMetadata(default(Brush), FocusPropertyChanged));

		public static readonly DependencyProperty IsPasswordProperty = DependencyProperty.Register(nameof(IsPassword), 
			typeof(bool), typeof(FormsTextBox), new PropertyMetadata(default(bool), OnIsPasswordChanged));

		public new static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), 
			typeof(string), typeof(FormsTextBox), new PropertyMetadata("", TextPropertyChanged));

		InputScope _passwordInputScope;
		Border _borderElement;
		InputScope _cachedInputScope;
		bool _cachedPredictionsSetting;
		bool _cachedSpellCheckSetting;
		CancellationTokenSource _cts;
		bool _internalChangeFlag;
		int _cachedSelectionLength;

		public FormsTextBox()
		{
			TextChanged += OnTextChanged;
			SelectionChanged += OnSelectionChanged;
			IsEnabledChanged += OnIsEnabledChanged;
		}

		void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{
			UpdateEnabled();
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

		internal bool UseFormsVsm { get; set; }

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
				if (_passwordInputScope != null)
				{
					return _passwordInputScope;
				}

				_passwordInputScope = new InputScope();
				var name = new InputScopeName { NameValue = InputScopeNameValue.Default };
				_passwordInputScope.Names.Add(name);

				return _passwordInputScope;
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
			// Cache this value for later use as explained in OnKeyDown below
			_cachedSelectionLength = SelectionLength;
		}

		// Because the implementation of a password entry is based around inheriting from TextBox (via FormsTextBox), there
		// are some inaccuracies in the behavior. OnKeyDown is what needs to be used for a workaround in this case because 
		// there's no easy way to disable specific keyboard shortcuts in a TextBox, so key presses are being intercepted and 
		// handled accordingly.
		protected override void OnKeyDown(KeyRoutedEventArgs e)
		{
			if (IsPassword)
			{
				// The ctrlDown flag is used to track if the Ctrl key is pressed; if it's actively being used and the most recent
				// key to trigger OnKeyDown, then treat it as handled.
				var ctrlDown = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down);

				// The shift, tab, and directional (Home/End/PgUp/PgDown included) keys can be used to select text and should otherwise
				// be ignored.
				if (
					e.Key == VirtualKey.Shift ||
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
				else
				{
					// If the C or X keys (copy/cut) are pressed while Ctrl is active, ignore handing them at all. Undo and Redo (Z/Y) should 
					// be ignored as well as this emulates the regular behavior of a PasswordBox.
					if ((e.Key == VirtualKey.C || e.Key == VirtualKey.X || e.Key == VirtualKey.Z || e.Key == VirtualKey.Y) && ctrlDown)
					{
						e.Handled = false;
						return;
					}

					base.OnKeyDown(e);
					if (_cachedSelectionLength > 0 && !ctrlDown)
						Text = Text.Remove(SelectionStart, _cachedSelectionLength);
				}
			}
			else
				base.OnKeyDown(e);
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

		static void FocusPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			// If we're modifying the properties related to the focus state of the control (e.g., 
			// ForegroundFocusBrush), the changes won't be reflected immediately because they are only applied
			// when the Windows.UI.XAML.VisualStateManager moves to the "Focused" state. So we have to force a 
			// "refresh" of the Focused state by going to that state again

			if (!(dependencyObject is Control control) || control.FocusState == FocusState.Unfocused)
			{
				return;
			}

			WVisualStateManager.GoToState(control, "Focused", false);
		}

		internal void UpdateEnabled()
		{
			if (UseFormsVsm)
			{
				var state = IsEnabled ? "FormsNormal" : "FormsDisabled";
				WVisualStateManager.GoToState(this, state, true);
			}
		}
	}
}