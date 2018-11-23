#if __ANDROID81__
#else
using System;
using System.ComponentModel;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Xamarin.Forms.Internals;
using AView = Android.Views.View;
using Android.Support.V4.View;
using Xamarin.Forms.Platform.Android.Material;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android.FastRenderers;
using System.Collections.Generic;
using Android.Content.Res;
using Android.Text;
using Android.Text.Method;
using Android.Views.InputMethods;
using Android.Widget;
using Java.Lang;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using MTextInputLayout = Android.Support.Design.Widget.TextInputLayout;
using Android.OS;

[assembly: ExportRenderer(typeof(Xamarin.Forms.Entry), typeof(MaterialEntryRenderer), new[] { typeof(VisualRendererMarker.Material) })]
namespace Xamarin.Forms.Platform.Android.Material
{
	public class MaterialEntryRenderer :
		MTextInputLayout,
		ITextWatcher, TextView.IOnEditorActionListener,
		IVisualElementRenderer, IViewRenderer, IEffectControlProvider

	{
		int? _defaultLabelFor;
		TextColorSwitcher _hintColorSwitcher;
		TextColorSwitcher _textColorSwitcher;
		readonly AutomationPropertiesProvider _automationPropertiesProvider;
		readonly EffectControlProvider _effectControlProvider;
		bool _disposed;
		ImeAction _currentInputImeFlag;

		bool _cursorPositionChangePending;
		bool _selectionLengthChangePending;
		bool _nativeSelectionIsUpdating;
		private MaterialFormsEditText _textInputEditText;
		VisualElementTracker _tracker;
		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

		public MaterialEntryRenderer(Context context) : base(new ContextThemeWrapper(context, Resource.Style.XamarinFormsMaterialTheme))
		{
			VisualElement.VerifyVisualFlagEnabled();
			_automationPropertiesProvider = new AutomationPropertiesProvider(this);
			_effectControlProvider = new EffectControlProvider(this);
		}

		IElementController ElementController => Element as IElementController;
		public VisualElement Element => Entry;
		AView IVisualElementRenderer.View => this;
		ViewGroup IVisualElementRenderer.ViewGroup => null;
		VisualElementTracker IVisualElementRenderer.Tracker => _tracker;

		Entry Entry { get; set; }

		bool TextView.IOnEditorActionListener.OnEditorAction(TextView v, ImeAction actionId, KeyEvent e)
		{
			// Fire Completed and dismiss keyboard for hardware / physical keyboards
			if (actionId == ImeAction.Done || actionId == _currentInputImeFlag || (actionId == ImeAction.ImeNull && e.KeyCode == Keycode.Enter && e.Action == KeyEventActions.Up))
			{
				ClearFocus();
				v.HideKeyboard();
				((IEntryController)Element).SendCompleted();
			}

			return true;
		}

		void IViewRenderer.MeasureExactly()
		{
			ViewRenderer.MeasureExactly(this, Element, Context);
		}

		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			_effectControlProvider.RegisterEffect(effect);
		}

		void IVisualElementRenderer.UpdateLayout()
		{
			_tracker?.UpdateLayout();
		}


		SizeRequest IVisualElementRenderer.GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			AView view = this;
			view.Measure(widthConstraint, heightConstraint);
			return new SizeRequest(new Size(MeasuredWidth, MeasuredHeight), MinimumSize());
		}

		void IVisualElementRenderer.SetLabelFor(int? id)
		{
			if (_defaultLabelFor == null)
			{
				_defaultLabelFor = ViewCompat.GetLabelFor(this);
			}

			ViewCompat.SetLabelFor(this, (int)(id ?? _defaultLabelFor));
		}

		Size MinimumSize()
		{
			return new Size();
		}

		void ITextWatcher.AfterTextChanged(IEditable s)
		{
		}

		void ITextWatcher.BeforeTextChanged(ICharSequence s, int start, int count, int after)
		{
		}

		void ITextWatcher.OnTextChanged(ICharSequence s, int start, int before, int count)
		{
			if (string.IsNullOrEmpty(Entry.Text) && s.Length() == 0)
				return;

			((IElementController)Element).SetValueFromRenderer(Entry.TextProperty, s.ToString());
		}


		public override void OnViewAdded(AView child)
		{
			base.OnViewAdded(child);

			/*AView someView = FindViewById(Resource.Id.materialtextinputlayoutfilledBox_edittext);
			var otherView = GetChildAt(0);
			_textInputEditText = FindViewById<MaterialFormsEditText>(Resource.Id.materialtextinputlayoutfilledBox_edittext);*/
		}

		void IVisualElementRenderer.SetElement(VisualElement element)
		{

			if (element == null)
				throw new ArgumentNullException(nameof(element));

			if (!(element is Entry))
				throw new ArgumentException($"{nameof(element)} must be of type {nameof(Entry)}");

			VisualElement oldElement = Entry;
			Entry = (Entry)element;

			Performance.Start(out string reference);

			if (oldElement != null)
			{
				oldElement.PropertyChanged -= OnElementPropertyChanged;
				oldElement.FocusChangeRequested -= OnFocusChangeRequested;
			}

			if (oldElement == null)
			{
				_textInputEditText = new MaterialFormsEditText(Context);
				AddView(_textInputEditText);

				_textInputEditText.FocusChange += _textInputEditText_FocusChange;
				Hint = Entry.Placeholder;

				_textInputEditText.AddTextChangedListener(this);
				_textInputEditText.SetOnEditorActionListener(this);
				_textInputEditText.OnKeyboardBackPressed += OnKeyboardBackPressed;
				_textInputEditText.SelectionChanged += SelectionChanged;

				var useLegacyColorManagement = Entry.UseLegacyColorManagement();

				_textColorSwitcher = new TextColorSwitcher(_textInputEditText.TextColors, useLegacyColorManagement);
				_hintColorSwitcher = new TextColorSwitcher(_textInputEditText.HintTextColors, useLegacyColorManagement);
			}

			// When we set the control text, it triggers the SelectionChanged event, which updates CursorPosition and SelectionLength;
			// These one-time-use variables will let us initialize a CursorPosition and SelectionLength via ctor/xaml/etc.
			_cursorPositionChangePending = Element.IsSet(Entry.CursorPositionProperty);
			_selectionLengthChangePending = Element.IsSet(Entry.SelectionLengthProperty);


			Hint = Entry.Placeholder;
			_textInputEditText.Text = Entry.Text;
			UpdateInputType();
			UpdateColor();
			UpdateAlignment();
			UpdateFont();
			UpdatePlaceholderColor(true);
			UpdateMaxLength();
			UpdateImeOptions();
			UpdateReturnType();
			UpdateBackgroundColor();

			if (_cursorPositionChangePending || _selectionLengthChangePending)
				UpdateCursorSelection();

			element.PropertyChanged += OnElementPropertyChanged;
			if (_tracker == null)
			{
				_tracker = new VisualElementTracker(this);
			}
			element.SendViewInitialized(this);
			EffectUtilities.RegisterEffectControlProvider(this, oldElement, element);
			Performance.Stop(reference);
			ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(oldElement, element));

			element.FocusChangeRequested += OnFocusChangeRequested;
		}

		// Todo Generalize this from View Renderer
		internal virtual void OnFocusChangeRequested(object sender, VisualElement.FocusRequestArgs e)
		{
			e.Result = true;

			if (e.Focus)
			{
				// use post being BeginInvokeOnMainThread will not delay on android
				Looper looper = Context.MainLooper;
				var handler = new Handler(looper);
				handler.Post(() =>
				{
					_textInputEditText.RequestFocus();
				});
			}
			else
			{
				_textInputEditText.ClearFocus();
			}

			if (e.Focus)
				this.ShowKeyboard();
			else
				this.HideKeyboard();
		}


		protected override void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing)
			{
				_textInputEditText.OnKeyboardBackPressed -= OnKeyboardBackPressed;
				_textInputEditText.SelectionChanged -= SelectionChanged;
			}

			base.Dispose(disposing);
		}


		void _textInputEditText_FocusChange(object sender, FocusChangeEventArgs e)
		{
			// TODO figure out better way to do this
			// this is a hack that changes the active underline color from the accent color to whatever the user 
			// specified
			Device.BeginInvokeOnMainThread(() => UpdatePlaceholderColor(false));
		}

		void UpdatePlaceholderColor(bool reset)
		{
			int[][] States =
			{
				new []{ global::Android.Resource.Attribute.StateFocused  },
				new []{ -global::Android.Resource.Attribute.StateFocused  },
			};

			var placeHolderColor = new ColorStateList(
						States,
						new int[]{
							Entry.PlaceholderColor.ToAndroid(),
							Entry.PlaceholderColor.ToAndroid()
						}
				);

			if (reset)
				DefaultHintTextColor = placeHolderColor;

			ViewCompat.SetBackgroundTintList(_textInputEditText, placeHolderColor);
		}

		void UpdateBackgroundColor()
		{
			SetBoxBackgroundMode(MTextInputLayout.BoxBackgroundFilled);
			BoxBackgroundColor = Element.BackgroundColor.ToAndroid();

			// need to make corner radius settable
			SetBoxCornerRadii(10, 10, 10, 10);
		}


		protected void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Entry.PlaceholderProperty.PropertyName)
				Hint = Entry.Placeholder;
			else if (e.PropertyName == Entry.IsPasswordProperty.PropertyName)
				UpdateInputType();
			else if (e.PropertyName == Entry.TextProperty.PropertyName)
			{
				if (_textInputEditText.Text != Entry.Text)
				{
					_textInputEditText.Text = Entry.Text;
					if (IsFocused)
					{
						_textInputEditText.SetSelection(_textInputEditText.Text.Length);
						this.ShowKeyboard();
					}
				}
			}
			else if (e.PropertyName == Entry.TextColorProperty.PropertyName)
				UpdateColor();
			else if (e.PropertyName == InputView.KeyboardProperty.PropertyName)
				UpdateInputType();
			else if (e.PropertyName == InputView.IsSpellCheckEnabledProperty.PropertyName)
				UpdateInputType();
			else if (e.PropertyName == Entry.IsTextPredictionEnabledProperty.PropertyName)
				UpdateInputType();
			else if (e.PropertyName == Entry.HorizontalTextAlignmentProperty.PropertyName)
				UpdateAlignment();
			else if (e.PropertyName == Entry.FontAttributesProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Entry.FontFamilyProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Entry.FontSizeProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Entry.PlaceholderColorProperty.PropertyName)
			{
				UpdatePlaceholderColor(true);
			}
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateAlignment();
			else if (e.PropertyName == InputView.MaxLengthProperty.PropertyName)
				UpdateMaxLength();
			else if (e.PropertyName == PlatformConfiguration.AndroidSpecific.Entry.ImeOptionsProperty.PropertyName)
				UpdateImeOptions();
			else if (e.PropertyName == Entry.ReturnTypeProperty.PropertyName)
				UpdateReturnType();
			else if (e.PropertyName == Entry.SelectionLengthProperty.PropertyName)
				UpdateCursorSelection();
			else if (e.PropertyName == Entry.CursorPositionProperty.PropertyName)
				UpdateCursorSelection();
			else if (e.PropertyName == Entry.BackgroundColorProperty.PropertyName)
				UpdateBackgroundColor();

			ElementPropertyChanged?.Invoke(this, e);
		}

		protected virtual NumberKeyListener GetDigitsKeyListener(InputTypes inputTypes)
		{
			// Override this in a custom renderer to use a different NumberKeyListener
			// or to filter out input types you don't want to allow
			// (e.g., inputTypes &= ~InputTypes.NumberFlagSigned to disallow the sign)
			return LocalizedDigitsKeyListener.Create(inputTypes);
		}

		protected virtual void UpdateImeOptions()
		{
			if (Element == null)
				return;
			var imeOptions = Entry.OnThisPlatform().ImeOptions();
			_currentInputImeFlag = imeOptions.ToAndroidImeOptions();
			_textInputEditText.ImeOptions = _currentInputImeFlag;
		}

		void UpdateAlignment()
		{
			_textInputEditText.UpdateHorizontalAlignment(Entry.HorizontalTextAlignment, Context.HasRtlSupport());
		}

		void UpdateColor()
		{
			_textColorSwitcher.UpdateTextColor(_textInputEditText, Entry.TextColor);
		}

		void UpdateFont()
		{
			Typeface = Entry.ToTypeface();
			_textInputEditText.SetTextSize(ComplexUnitType.Sp, (float)Entry.FontSize);
		}

		void UpdateInputType()
		{
			Entry model = Entry;
			var keyboard = model.Keyboard;

			_textInputEditText.InputType = keyboard.ToInputType();
			if (!(keyboard is Internals.CustomKeyboard))
			{
				if (model.IsSet(InputView.IsSpellCheckEnabledProperty))
				{
					if ((_textInputEditText.InputType & InputTypes.TextFlagNoSuggestions) != InputTypes.TextFlagNoSuggestions)
					{
						if (!model.IsSpellCheckEnabled)
							_textInputEditText.InputType = _textInputEditText.InputType | InputTypes.TextFlagNoSuggestions;
					}
				}
				if (model.IsSet(Entry.IsTextPredictionEnabledProperty))
				{
					if ((_textInputEditText.InputType & InputTypes.TextFlagNoSuggestions) != InputTypes.TextFlagNoSuggestions)
					{
						if (!model.IsTextPredictionEnabled)
							_textInputEditText.InputType = _textInputEditText.InputType | InputTypes.TextFlagNoSuggestions;
					}
				}
			}

			if (keyboard == Keyboard.Numeric)
			{
				_textInputEditText.KeyListener = GetDigitsKeyListener(_textInputEditText.InputType);
			}

			if (model.IsPassword && ((_textInputEditText.InputType & InputTypes.ClassText) == InputTypes.ClassText))
				_textInputEditText.InputType = _textInputEditText.InputType | InputTypes.TextVariationPassword;
			if (model.IsPassword && ((_textInputEditText.InputType & InputTypes.ClassNumber) == InputTypes.ClassNumber))
				_textInputEditText.InputType = _textInputEditText.InputType | InputTypes.NumberVariationPassword;

			UpdateFont();
		}

		void OnKeyboardBackPressed(object sender, EventArgs eventArgs)
		{
			_textInputEditText.ClearFocus();
		}

		void UpdateMaxLength()
		{
			var currentFilters = new List<IInputFilter>(_textInputEditText?.GetFilters() ?? new IInputFilter[0]);

			for (var i = 0; i < currentFilters.Count; i++)
			{
				if (currentFilters[i] is InputFilterLengthFilter)
				{
					currentFilters.RemoveAt(i);
					break;
				}
			}

			currentFilters.Add(new InputFilterLengthFilter(Entry.MaxLength));

			_textInputEditText?.SetFilters(currentFilters.ToArray());

			var currentControlText = _textInputEditText?.Text;

			if (currentControlText.Length > Entry.MaxLength)
				_textInputEditText.Text = currentControlText.Substring(0, Entry.MaxLength);
		}

		void UpdateReturnType()
		{
			if (Element == null)
				return;

			_textInputEditText.ImeOptions = Entry.ReturnType.ToAndroidImeAction();
			_currentInputImeFlag = _textInputEditText.ImeOptions;
		}

		void SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (_nativeSelectionIsUpdating || Element == null)
				return;

			int cursorPosition = Entry.CursorPosition;
			int selectionStart = _textInputEditText.SelectionStart;

			if (!_cursorPositionChangePending)
			{
				var start = cursorPosition;

				if (selectionStart != start)
					SetCursorPositionFromRenderer(selectionStart);
			}

			if (!_selectionLengthChangePending)
			{
				int elementSelectionLength = System.Math.Min(_textInputEditText.Text.Length - cursorPosition, Entry.SelectionLength);

				var controlSelectionLength = _textInputEditText.SelectionEnd - selectionStart;
				if (controlSelectionLength != elementSelectionLength)
					SetSelectionLengthFromRenderer(controlSelectionLength);
			}
		}

		void UpdateCursorSelection()
		{
			if (_nativeSelectionIsUpdating || Element == null)
				return;

			if (_textInputEditText.RequestFocus())
			{
				try
				{
					int start = GetSelectionStart();
					int end = GetSelectionEnd(start);

					_textInputEditText.SetSelection(start, end);
				}
				catch (System.Exception ex)
				{
					Internals.Log.Warning("Entry", $"Failed to set Control.Selection from CursorPosition/SelectionLength: {ex}");
				}
				finally
				{
					_cursorPositionChangePending = _selectionLengthChangePending = false;
				}
			}
		}

		int GetSelectionEnd(int start)
		{
			int end = start;
			int selectionLength = Entry.SelectionLength;

			if (Element.IsSet(Entry.SelectionLengthProperty))
				end = System.Math.Max(start, System.Math.Min(_textInputEditText.Length(), start + selectionLength));

			int newSelectionLength = System.Math.Max(0, end - start);
			if (newSelectionLength != selectionLength)
				SetSelectionLengthFromRenderer(newSelectionLength);

			return end;
		}

		int GetSelectionStart()
		{
			int start = _textInputEditText.Length();
			int cursorPosition = Entry.CursorPosition;

			if (Element.IsSet(Entry.CursorPositionProperty))
				start = System.Math.Min(_textInputEditText.Text.Length, cursorPosition);

			if (start != cursorPosition)
				SetCursorPositionFromRenderer(start);

			return start;
		}

		void SetCursorPositionFromRenderer(int start)
		{
			try
			{
				_nativeSelectionIsUpdating = true;
				ElementController?.SetValueFromRenderer(Entry.CursorPositionProperty, start);
			}
			catch (System.Exception ex)
			{
				Internals.Log.Warning("Entry", $"Failed to set CursorPosition from renderer: {ex}");
			}
			finally
			{
				_nativeSelectionIsUpdating = false;
			}
		}

		void SetSelectionLengthFromRenderer(int selectionLength)
		{
			try
			{
				_nativeSelectionIsUpdating = true;
				ElementController?.SetValueFromRenderer(Entry.SelectionLengthProperty, selectionLength);
			}
			catch (System.Exception ex)
			{
				Internals.Log.Warning("Entry", $"Failed to set SelectionLength from renderer: {ex}");
			}
			finally
			{
				_nativeSelectionIsUpdating = false;
			}
		}

		//void UpdatePlaceholderColor()
		//{
		//	_hintColorSwitcher.UpdateTextColor(_textInputEditText, Element.PlaceholderColor, _textInputEditText.SetHintTextColor);
		//}
	}
}
#endif