using System;
using Android.Content.Res;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Google.Android.Material.TextField;
using static Android.Views.View;
using static Android.Widget.TextView;

namespace Microsoft.Maui.Handlers;

// TODO: Material3: Make it public in .NET 11
internal class EntryHandler2 : ViewHandler<IEntry, MauiMaterialTextInputLayout>
{
	ClearButtonClickListener? _clearButtonClickListener;
	ColorStateList? _defaultHintTextColors;

	public static PropertyMapper<IEntry, EntryHandler2> Mapper =
	  new(ViewMapper)
	  {
		  [nameof(IEntry.Background)] = MapBackground,
		  [nameof(IEntry.Text)] = MapText,
		  [nameof(IEntry.TextColor)] = MapTextColor,
		  [nameof(IEntry.IsPassword)] = MapIsPassword,
		  [nameof(IEntry.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
		  [nameof(IEntry.VerticalTextAlignment)] = MapVerticalTextAlignment,
		  [nameof(IEntry.IsTextPredictionEnabled)] = MapIsTextPredictionEnabled,
		  [nameof(IEntry.IsSpellCheckEnabled)] = MapIsSpellCheckEnabled,
		  [nameof(IEntry.MaxLength)] = MapMaxLength,
		  [nameof(IEntry.Placeholder)] = MapPlaceholder,
		  [nameof(IEntry.PlaceholderColor)] = MapPlaceholderColor,
		  [nameof(IEntry.Font)] = MapFont,
		  [nameof(IEntry.IsReadOnly)] = MapIsReadOnly,
		  [nameof(IEntry.Keyboard)] = MapKeyboard,
		  [nameof(IEntry.ReturnType)] = MapReturnType,
		  [nameof(IEntry.CharacterSpacing)] = MapCharacterSpacing,
		  [nameof(IEntry.CursorPosition)] = MapCursorPosition,
		  [nameof(IEntry.SelectionLength)] = MapSelectionLength,
		  [nameof(IEntry.ClearButtonVisibility)] = MapClearButtonVisibility,
	  };

	public static CommandMapper<IEntry, EntryHandler2> CommandMapper =
	  new(ViewCommandMapper)
	  {
		  [nameof(IEntry.Focus)] = MapFocus
	  };

	public EntryHandler2() : base(Mapper, CommandMapper)
	{
	}

	protected override MauiMaterialTextInputLayout CreatePlatformView()
	{
		var layout = new MauiMaterialTextInputLayout(Context);
		layout.BoxBackgroundMode = TextInputLayout.BoxBackgroundOutline;
		layout.AddView(new MauiMaterialEditText(layout.Context!));

		// Store the original default hint colors before any customization
		_defaultHintTextColors = layout.DefaultHintTextColor;

		return layout;
	}

	public override void SetVirtualView(IView view)
	{
		base.SetVirtualView(view);

		if (PlatformView.EditText is MauiMaterialEditText _editText)
		{
			_editText.SelectionChanged -= OnSelectionChanged;
			_editText.SelectionChanged += OnSelectionChanged;
		}
	}

	protected override void ConnectHandler(MauiMaterialTextInputLayout platformView)
	{
		platformView.ViewAttachedToWindow += OnViewAttachedToWindow;
		platformView.EditText?.TextChanged += OnTextChanged;
		platformView.EditText?.FocusChange += OnFocusedChange;

		_clearButtonClickListener ??= new ClearButtonClickListener(this);
		platformView.SetEndIconOnClickListener(_clearButtonClickListener);

		platformView.EditText?.EditorAction += OnEditorAction;
	}

	protected override void DisconnectHandler(MauiMaterialTextInputLayout platformView)
	{
		platformView.ViewAttachedToWindow -= OnViewAttachedToWindow;

		if (platformView.EditText is MauiMaterialEditText _editText)
		{
			_editText.SelectionChanged -= OnSelectionChanged;
		}

		platformView.EditText?.EditorAction -= OnEditorAction;
		platformView.EditText?.TextChanged -= OnTextChanged;
		platformView.EditText?.FocusChange -= OnFocusedChange;
		platformView.SetEndIconOnClickListener(null);

		_clearButtonClickListener?.Dispose();
		_clearButtonClickListener = null;

		_defaultHintTextColors = null;
	}

	void OnViewAttachedToWindow(object? sender, ViewAttachedToWindowEventArgs e)
	{
		if (PlatformView is null || VirtualView is null)
		{
			return;
		}

		PlatformView.EditText?.UpdateReturnType(VirtualView);
	}

	public static void MapBackground(EntryHandler2 handler, IEntry entry) =>
		handler.PlatformView?.UpdateBackground(entry);

	public static void MapText(EntryHandler2 handler, IEntry entry) =>
		handler.PlatformView.EditText?.UpdateText(entry);

	public static void MapTextColor(EntryHandler2 handler, IEntry entry) =>
		handler.PlatformView.EditText?.UpdateTextColor(entry);

	public static void MapIsPassword(EntryHandler2 handler, IEntry entry)
	{
		handler.UpdateValue(nameof(IEntry.Text));
		handler.PlatformView.EditText?.UpdateIsPassword(entry);

		// Password toggle takes precedence over clear button
		if (entry.IsPassword)
		{
			handler.PlatformView.EndIconMode = TextInputLayout.EndIconPasswordToggle;
		}
		else
		{
			// Re-evaluate clear button visibility
			MapClearButtonVisibility(handler, entry);
		}
	}

	public static void MapHorizontalTextAlignment(EntryHandler2 handler, IEntry entry) =>
		handler.PlatformView.EditText?.UpdateHorizontalTextAlignment(entry);

	public static void MapVerticalTextAlignment(EntryHandler2 handler, IEntry entry) =>
		handler?.PlatformView.EditText?.UpdateVerticalTextAlignment(entry);

	public static void MapIsTextPredictionEnabled(EntryHandler2 handler, IEntry entry) =>
		handler.PlatformView.EditText?.UpdateIsTextPredictionEnabled(entry);

	public static void MapIsSpellCheckEnabled(EntryHandler2 handler, IEntry entry) =>
		handler.PlatformView.EditText?.UpdateIsSpellCheckEnabled(entry);

	public static void MapMaxLength(EntryHandler2 handler, IEntry entry) =>
		handler.PlatformView.EditText?.UpdateMaxLength(entry);

	public static void MapPlaceholder(EntryHandler2 handler, IEntry entry)
	{
		handler.PlatformView.Hint = entry.Placeholder;
	}

	public static void MapPlaceholderColor(EntryHandler2 handler, IEntry entry)
	{
		if (entry.PlaceholderColor is not null)
		{
			// Set both to ensure color applies in both focused (floating) and unfocused (inline) states
			handler.PlatformView.DefaultHintTextColor = ColorStateList.ValueOf(entry.PlaceholderColor.ToPlatform());
		}
		else
		{
			// Reset to the original default Material Design hint colors stored during creation
			handler.PlatformView.DefaultHintTextColor = handler._defaultHintTextColors;
		}
	}

	public static void MapFont(EntryHandler2 handler, IEntry entry) =>
		handler.PlatformView.EditText?.UpdateFont(entry, handler.GetRequiredService<IFontManager>());

	public static void MapIsReadOnly(EntryHandler2 handler, IEntry entry)
	{
		handler.UpdateValue(nameof(IEntry.Text));

		handler.PlatformView.EditText?.UpdateIsReadOnly(entry);
	}

	public static void MapKeyboard(EntryHandler2 handler, IEntry entry)
	{
		handler.UpdateValue(nameof(IEntry.Text));

		handler.PlatformView.EditText?.UpdateKeyboard(entry);
	}

	public static void MapReturnType(EntryHandler2 handler, IEntry entry) =>
		handler.PlatformView.EditText?.UpdateReturnType(entry);

	public static void MapCharacterSpacing(EntryHandler2 handler, IEntry entry) =>
		handler.PlatformView.EditText?.UpdateCharacterSpacing(entry);

	public static void MapCursorPosition(EntryHandler2 handler, IEntry entry) =>
		handler.PlatformView.EditText?.UpdateCursorPosition(entry);

	public static void MapSelectionLength(EntryHandler2 handler, IEntry entry) =>
		handler.PlatformView.EditText?.UpdateSelectionLength(entry);

	public static void MapClearButtonVisibility(EntryHandler2 handler, IEntry entry)
	{
		// Password toggle takes precedence
		if (entry.IsPassword)
		{
			return;
		}

		bool shouldShowClearButton = entry.ClearButtonVisibility == ClearButtonVisibility.WhileEditing &&
			!string.IsNullOrEmpty(entry.Text) &&
			handler.PlatformView.HasFocus;

		var targetMode = shouldShowClearButton
			? TextInputLayout.EndIconClearText
			: TextInputLayout.EndIconNone;

		// Only update if mode actually changed to avoid unnecessary native updates
		if (handler.PlatformView.EndIconMode != targetMode)
		{
			handler.PlatformView.EndIconMode = targetMode;
		}
	}

	static void MapFocus(EntryHandler2 handler, IEntry entry, object? args)
	{
		if (args is FocusRequest request)
		{
			// Focus the EditText directly, not the TextInputLayout container
			handler.PlatformView.EditText?.Focus(request);
		}
	}

	void OnTextChanged(object? sender, TextChangedEventArgs e)
	{
		if (VirtualView is null)
		{
			return;
		}

		// Let the mapping know that the update is coming from changes to the platform control
		DataFlowDirection = DataFlowDirection.FromPlatform;

		VirtualView.UpdateText(e);

		// Reset to the default direction
		DataFlowDirection = DataFlowDirection.ToPlatform;

		MapClearButtonVisibility(this, VirtualView);
	}

	void OnFocusedChange(object? sender, FocusChangeEventArgs e)
	{
		if (VirtualView is null || PlatformView is null)
		{
			return;
		}

		VirtualView.IsFocused = e.HasFocus;
		MapClearButtonVisibility(this, VirtualView);
	}

	void OnEditorAction(object? sender, EditorActionEventArgs e)
	{
		var returnType = VirtualView?.ReturnType;

		// Inside of the android implementations that map events to listeners, the default return value for "Handled" is always true
		// This means, just by subscribing to EditorAction/KeyPressed/etc.. you change the behavior of the control
		// So, we are setting handled to false here in order to maintain default behavior
		bool handled = false;
		if (returnType is not null)
		{
			var actionId = e.ActionId;
			var evt = e.Event;
			ImeAction currentInputImeFlag = PlatformView.EditText?.ImeOptions ?? ImeAction.None;

			// On API 34 it looks like they fixed the issue where the actionId is ImeAction.ImeNull when using a keyboard
			// so I'm just setting the actionId here to the current ImeOptions so the logic can all be simplified
			if (actionId == ImeAction.ImeNull && evt?.KeyCode == Keycode.Enter)
			{
				actionId = currentInputImeFlag;
			}

			// keyboard path
			if (evt?.KeyCode == Keycode.Enter && evt?.Action == KeyEventActions.Down)
			{
				handled = true;
			}
			else if (evt?.KeyCode == Keycode.Enter && evt?.Action == KeyEventActions.Up)
			{
				VirtualView?.Completed();
			}
			// InputPaneView Path
			else if (evt?.KeyCode is null && (actionId == ImeAction.Done || actionId == currentInputImeFlag))
			{
				VirtualView?.Completed();
				// In case of Search, Go, Send the EditorAction will be invoked for KeyEventActions which will cause Completed to invoke twice
				//So for these setting handled to true
				if (actionId == ImeAction.Search ||
				 actionId == ImeAction.Go ||
				  actionId == ImeAction.Send)
				{
					handled = true;
				}
			}
		}

		e.Handled = handled;
	}

	private void OnSelectionChanged(object? sender, EventArgs e)
	{
		var cursorPosition = PlatformView.EditText?.GetCursorPosition() ?? 0;
		var selectedTextLength = PlatformView.EditText?.GetSelectedTextLength() ?? 0;

		if (VirtualView.CursorPosition != cursorPosition)
		{
			VirtualView.CursorPosition = cursorPosition;
		}

		if (VirtualView.SelectionLength != selectedTextLength)
		{
			VirtualView.SelectionLength = selectedTextLength;
		}
	}

	class ClearButtonClickListener : Java.Lang.Object, IOnClickListener
	{
		readonly WeakReference<EntryHandler2> _handlerRef;

		public ClearButtonClickListener(EntryHandler2 handler)
		{
			_handlerRef = new WeakReference<EntryHandler2>(handler);
		}

		public void OnClick(global::Android.Views.View? v)
		{
			if (_handlerRef.TryGetTarget(out var handler) &&
				handler.VirtualView is not null &&
				handler.PlatformView.EditText is not null)
			{
				// Clear the text
				handler.PlatformView.EditText.Text = string.Empty;
				handler.VirtualView.Text = string.Empty;
			}
		}
	}
}