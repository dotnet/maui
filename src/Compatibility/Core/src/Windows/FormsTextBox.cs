using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WVisualStateManager = Microsoft.UI.Xaml.VisualStateManager;
using System.Text;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	/// <summary>
	///     An intermediate class for injecting bindings for things the default
	///     textbox doesn't allow us to bind/modify
	/// </summary>
	public class FormsTextBox : TextBox
	{
		const char ObfuscationCharacter = '●';

		public static readonly DependencyProperty PlaceholderForegroundBrushProperty =
			DependencyProperty.Register(nameof(PlaceholderForegroundBrush), typeof(WBrush), typeof(FormsTextBox),
				new PropertyMetadata(default(WBrush), FocusPropertyChanged));

		public static readonly DependencyProperty PlaceholderForegroundFocusBrushProperty =
			DependencyProperty.Register(nameof(PlaceholderForegroundFocusBrush), typeof(WBrush), typeof(FormsTextBox),
				new PropertyMetadata(default(WBrush), FocusPropertyChanged));

		public static readonly DependencyProperty ForegroundFocusBrushProperty =
			DependencyProperty.Register(nameof(ForegroundFocusBrush), typeof(WBrush), typeof(FormsTextBox),
				new PropertyMetadata(default(WBrush), FocusPropertyChanged));

		public static readonly DependencyProperty BackgroundFocusBrushProperty =
			DependencyProperty.Register(nameof(BackgroundFocusBrush), typeof(WBrush), typeof(FormsTextBox),
				new PropertyMetadata(default(WBrush), FocusPropertyChanged));

		public static readonly DependencyProperty IsPasswordProperty = DependencyProperty.Register(nameof(IsPassword),
			typeof(bool), typeof(FormsTextBox), new PropertyMetadata(default(bool), OnIsPasswordChanged));

		public new static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text),
			typeof(string), typeof(FormsTextBox), new PropertyMetadata("", TextPropertyChanged));

		public static readonly DependencyProperty ClearButtonVisibleProperty = DependencyProperty.Register(nameof(ClearButtonVisible),
			typeof(bool), typeof(FormsTextBox), new PropertyMetadata(true, ClearButtonVisibleChanged));

		InputScope _passwordInputScope;
		InputScope _numericPasswordInputScope;
		ScrollViewer _scrollViewer;
		Microsoft.UI.Xaml.Controls.Grid _rootGrid;
		Microsoft.UI.Xaml.VisualState _DeleteButtonVisibleState;
		Microsoft.UI.Xaml.VisualStateGroup _DeleteButtonVisibleStateGroups;
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
			SizeChanged += OnSizeChanged;
			RegisterPropertyChangedCallback(VerticalContentAlignmentProperty, OnVerticalContentAlignmentChanged);
		}

		void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{
			UpdateEnabled();
		}

		internal bool UpdateVerticalAlignmentOnLoad { get; set; } = true;

		public bool ClearButtonVisible
		{
			get { return (bool)GetValue(ClearButtonVisibleProperty); }
			set { SetValue(ClearButtonVisibleProperty, value);}
		}

		public WBrush BackgroundFocusBrush
		{
			get { return (WBrush)GetValue(BackgroundFocusBrushProperty); }
			set { SetValue(BackgroundFocusBrushProperty, value); }
		}

		public WBrush ForegroundFocusBrush
		{
			get { return (WBrush)GetValue(ForegroundFocusBrushProperty); }
			set { SetValue(ForegroundFocusBrushProperty, value); }
		}

		public bool IsPassword
		{
			get { return (bool)GetValue(IsPasswordProperty); }
			set { SetValue(IsPasswordProperty, value); }
		}

		internal bool UseFormsVsm { get; set; }

		public WBrush PlaceholderForegroundBrush
		{
			get { return (WBrush)GetValue(PlaceholderForegroundBrushProperty); }
			set { SetValue(PlaceholderForegroundBrushProperty, value); }
		}

		public WBrush PlaceholderForegroundFocusBrush
		{
			get { return (WBrush)GetValue(PlaceholderForegroundFocusBrushProperty); }
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

		InputScope NumericPasswordInputScope
		{
			get
			{
				if (_numericPasswordInputScope != null)
				{
					return _numericPasswordInputScope;
				}

				_numericPasswordInputScope = new InputScope();
				var name = new InputScopeName { NameValue = InputScopeNameValue.NumericPassword };
				_numericPasswordInputScope.Names.Add(name);

				return _numericPasswordInputScope;
			}
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_rootGrid = GetTemplateChild("RootGrid") as Microsoft.UI.Xaml.Controls.Grid;
			if (_rootGrid != null)
			{
				var stateGroups = WVisualStateManager.GetVisualStateGroups(_rootGrid).ToList();
				_DeleteButtonVisibleStateGroups = stateGroups.SingleOrDefault(sg => sg.Name == "ButtonStates");
				if (_DeleteButtonVisibleStateGroups != null)
					_DeleteButtonVisibleState = _DeleteButtonVisibleStateGroups.States.SingleOrDefault(s => s.Name == "ButtonVisible");
				UpdateClearButtonVisible();
			}

			_scrollViewer= GetTemplateChild("ContentElement") as ScrollViewer;
		}

		void OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateTemplateScrollViewerVerticalAlignment();
		}

		void OnVerticalContentAlignmentChanged(DependencyObject sender, DependencyProperty dp)
		{
			UpdateTemplateScrollViewerVerticalAlignment();
		}

		void UpdateTemplateScrollViewerVerticalAlignment()
		{
			// This is used to set the vertical alignment after the text box has a size, setting it before causes rendering issues.
			// But the editor has display issues if you do set the vertical alignment here, so the flag allows renderer using
			// the text box to control this
			if (_scrollViewer != null && UpdateVerticalAlignmentOnLoad)
			{
				_scrollViewer.VerticalAlignment = VerticalContentAlignment;
			}
		}

		void DelayObfuscation()
		{
			int lengthDifference = base.Text.Length - Text.Length;

			string updatedRealText = DetermineTextFromPassword(Text, SelectionStart, base.Text);

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
				newText = Obfuscate(Text);
			}
			else
			{
				// Only one character was added; we need to leave it visible for a brief time period
				// Obfuscate all but the last character for now
				newText = Obfuscate(Text, true);

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
						base.Text = Obfuscate(Text);
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

		static string DetermineTextFromPassword(string realText, int start, string passwordText)
		{
			var lengthDifference = passwordText.Length - realText.Length;
			if (lengthDifference > 0)
				realText = realText.Insert(start - lengthDifference, new string(ObfuscationCharacter, lengthDifference));
			else if (lengthDifference < 0)
				realText = realText.Remove(start, -lengthDifference);

			var sb = new StringBuilder(passwordText.Length);
			for (int i = 0; i < passwordText.Length; i++)
				sb.Append(passwordText[i] == ObfuscationCharacter ? realText[i] : passwordText[i]);

			return sb.ToString();
		}

		string Obfuscate(string text, bool leaveLastVisible = false)
		{
			if (!leaveLastVisible)
				return new string(ObfuscationCharacter, text.Length);

			return text.Length == 1
				? text
				: new string(ObfuscationCharacter, text.Length - 1) + text.Substring(text.Length - 1, 1);
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
				var ctrlDown = Forms.MainWindow.CoreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down);

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

		void OnTextChanged(object sender, Microsoft.UI.Xaml.Controls.TextChangedEventArgs textChangedEventArgs)
		{
			if (IsPassword)
			{
				// If we are on the phone, we might need to delay obfuscating the last character
				if (Device.Idiom == TargetIdiom.Phone)
				{
					DelayObfuscation();
					return;
				}

				// If we're not on a phone, we can just obfuscate any input
				string updatedRealText = DetermineTextFromPassword(Text, SelectionStart, base.Text);
				string updatedText = Obfuscate(updatedRealText);
				var savedSelectionStart = SelectionStart;

				if (Text != updatedRealText)
					Text = updatedRealText;

				if (base.Text != updatedText)
					base.Text = updatedText;

				SelectionStart = savedSelectionStart;
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

			base.Text = IsPassword ? Obfuscate(Text) : Text;

			SelectionStart = base.Text.Length;
		}

		void UpdateClearButtonVisible()
		{
			var visibleState = _DeleteButtonVisibleState;
			var states = _DeleteButtonVisibleStateGroups?.States;

			if (states != null && visibleState != null)
			{
				if (ClearButtonVisible && !states.Contains(visibleState))
					states.Add(visibleState);
				else if(!ClearButtonVisible)
					states.Remove(visibleState);
			}
		}

		static void ClearButtonVisibleChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{
			var textBox = (FormsTextBox)dependencyObject;
			textBox.UpdateClearButtonVisible();
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
			// when the Microsoft.UI.Xaml.VisualStateManager moves to the "Focused" state. So we have to force a 
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

		/*
		 * This was originally in the EditorRenderer, moved here to be shared with the entry renderer.
		 * It also needs to always be applied to the size calculation, not just when the box size could change.
		 *
		 * Purely invalidating the layout as text is added to the TextBox will not cause it to expand.
		 * If the TextBox is set to WordWrap and it is part of the layout it will refuse to Measure itself beyond its established width.
		 * Even giving it infinite constraints will cause it to always set its DesiredSize to the same width but with a vertical growth.
		 * The only way I was able to grow it was by setting layout renderers width explicitly to some value but then it just set its own Width to that Width which is not helpful.
		 * Even vertically it would measure oddly in cases of rapid text changes.
		 * Holding down the backspace key or enter key would cause the final result to be not quite right.
		 * Both of these issues were fixed by just creating a static TextBox that is not part of the layout which let me just measure
		 * the size of the text as it would fit into the TextBox unconstrained and then just return that Size from the GetDesiredSize call.
		 * */
		static FormsTextBox _copyOfTextBox;
		static readonly Windows.Foundation.Size _zeroSize = new Windows.Foundation.Size(0, 0);
		public static Size GetCopyOfSize(FormsTextBox control, Windows.Foundation.Size constraint)
		{
			if (_copyOfTextBox == null)
			{
				_copyOfTextBox = new FormsTextBox
				{
					Style = Microsoft.UI.Xaml.Application.Current.Resources["FormsTextBoxStyle"] as Microsoft.UI.Xaml.Style
				};

				// This causes the copy to be initially setup correctly. 
				// I found that if the first measure of this copy occurs with Text then it will just keep defaulting to a measure with no text.
				_copyOfTextBox.Measure(_zeroSize);
			}

			_copyOfTextBox.MinHeight = control.MinHeight;
			_copyOfTextBox.MaxHeight = control.MaxHeight;
			_copyOfTextBox.MinWidth = control.MinWidth;
			_copyOfTextBox.MaxWidth = control.MaxWidth;
			_copyOfTextBox.TextWrapping = control.TextWrapping;
			_copyOfTextBox.AcceptsReturn = control.AcceptsReturn;
			_copyOfTextBox.Text = control.Text;
			_copyOfTextBox.FontSize = control.FontSize;
			_copyOfTextBox.FontFamily = control.FontFamily;
			_copyOfTextBox.FontStretch = control.FontStretch;
			_copyOfTextBox.FontStyle = control.FontStyle;
			_copyOfTextBox.FontWeight = control.FontWeight;
			_copyOfTextBox.Margin = control.Margin;
			_copyOfTextBox.Padding = control.Padding;

			// have to reset the measure to zero before it will re-measure itself
			_copyOfTextBox.Measure(_zeroSize);
			_copyOfTextBox.Measure(constraint);

			var result = new Size
			(
				Math.Ceiling(_copyOfTextBox.DesiredSize.Width),
				Math.Ceiling(_copyOfTextBox.DesiredSize.Height)
			);

			return result;
		}
	}
}
