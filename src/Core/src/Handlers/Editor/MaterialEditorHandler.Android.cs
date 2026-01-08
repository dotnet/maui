using System;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Microsoft.Maui.Graphics;
using static Android.Views.View;

namespace Microsoft.Maui.Handlers;

internal class MaterialEditorHandler : ViewHandler<IEditor, MauiMaterialEditText>
{
	bool _set;

	public static PropertyMapper<IEditor, MaterialEditorHandler> Mapper = 
		new(ViewMapper)
		{
			[nameof(IEditor.Background)] = MapBackground,
			[nameof(IEditor.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(IEditor.Font)] = MapFont,
			[nameof(IEditor.IsReadOnly)] = MapIsReadOnly,
			[nameof(IEditor.IsTextPredictionEnabled)] = MapIsTextPredictionEnabled,
			[nameof(IEditor.IsSpellCheckEnabled)] = MapIsSpellCheckEnabled,
			[nameof(IEditor.MaxLength)] = MapMaxLength,
			[nameof(IEditor.Placeholder)] = MapPlaceholder,
			[nameof(IEditor.PlaceholderColor)] = MapPlaceholderColor,
			[nameof(IEditor.Text)] = MapText,
			[nameof(IEditor.TextColor)] = MapTextColor,
			[nameof(IEditor.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
			[nameof(IEditor.VerticalTextAlignment)] = MapVerticalTextAlignment,
			[nameof(IEditor.Keyboard)] = MapKeyboard,
			[nameof(IEditor.CursorPosition)] = MapCursorPosition,
			[nameof(IEditor.SelectionLength)] = MapSelectionLength,
		};

	public static CommandMapper<IEditor, MaterialEditorHandler> CommandMapper = 
		new(ViewCommandMapper)
		{
			[nameof(IEditor.Focus)] = MapFocus
		};

	public MaterialEditorHandler() : base(Mapper, CommandMapper)
	{
	}

	protected override MauiMaterialEditText CreatePlatformView()
	{
		var editText = new MauiMaterialEditText(Context)
		{
			ImeOptions = ImeAction.Done,
			Gravity = GravityFlags.Top,
			TextAlignment = global::Android.Views.TextAlignment.ViewStart,
		};

		editText.SetSingleLine(false);
		editText.SetHorizontallyScrolling(false);

		return editText;
	}

	public override void SetVirtualView(IView view)
	{
		base.SetVirtualView(view);

		if (!_set)
		{
			PlatformView.SelectionChanged += OnSelectionChanged;
		}

		_set = true;
	}

	protected override void ConnectHandler(MauiMaterialEditText platformView)
	{
		platformView.TextChanged += OnTextChanged;
		platformView.FocusChange += OnFocusChange;
	}

	protected override void DisconnectHandler(MauiMaterialEditText platformView)
	{
		platformView.TextChanged -= OnTextChanged;
		platformView.FocusChange -= OnFocusChange;

		if (_set)
		{
			platformView.SelectionChanged -= OnSelectionChanged;
		}

		_set = false;
	}

	public static void MapBackground(MaterialEditorHandler handler, IEditor editor)
	{
		handler.PlatformView?.UpdateBackground(editor);
	}

	public static void MapText(MaterialEditorHandler handler, IEditor editor)
	{
		handler.PlatformView?.UpdateText(editor);
	}

	public static void MapTextColor(MaterialEditorHandler handler, IEditor editor)
	{
		handler.PlatformView?.UpdateTextColor(editor);
	}

	public static void MapPlaceholder(MaterialEditorHandler handler, IEditor editor)
	{
		handler.PlatformView?.UpdatePlaceholder(editor);
	}

	public static void MapPlaceholderColor(MaterialEditorHandler handler, IEditor editor)
	{
		handler.PlatformView?.UpdatePlaceholderColor(editor);
	}

	public static void MapCharacterSpacing(MaterialEditorHandler handler, IEditor editor)
	{
		handler.PlatformView?.UpdateCharacterSpacing(editor);
	}

	public static void MapMaxLength(MaterialEditorHandler handler, IEditor editor)
	{
		handler.PlatformView?.UpdateMaxLength(editor);
	}

	public static void MapIsReadOnly(MaterialEditorHandler handler, IEditor editor)
	{
		handler.PlatformView?.UpdateIsReadOnly(editor);
	}

	public static void MapIsTextPredictionEnabled(MaterialEditorHandler handler, IEditor editor)
	{
		handler.PlatformView?.UpdateIsTextPredictionEnabled(editor);
	}

	public static void MapIsSpellCheckEnabled(MaterialEditorHandler handler, IEditor editor)
	{
		handler.PlatformView?.UpdateIsSpellCheckEnabled(editor);
	}

	public static void MapFont(MaterialEditorHandler handler, IEditor editor)
	{
		handler.PlatformView?.UpdateFont(editor, handler.GetRequiredService<IFontManager>());
	}

	public static void MapHorizontalTextAlignment(MaterialEditorHandler handler, IEditor editor)
	{
		handler.PlatformView?.UpdateHorizontalTextAlignment(editor);
	}

	public static void MapVerticalTextAlignment(MaterialEditorHandler handler, IEditor editor)
	{
		handler.PlatformView?.UpdateVerticalTextAlignment(editor);
	}

	public static void MapKeyboard(MaterialEditorHandler handler, IEditor editor)
	{
		handler.UpdateValue(nameof(IEditor.Text));

		handler.PlatformView?.UpdateKeyboard(editor);
	}

	public static void MapCursorPosition(MaterialEditorHandler handler, ITextInput editor)
	{
		handler.PlatformView?.UpdateCursorPosition(editor);
	}

	public static void MapSelectionLength(MaterialEditorHandler handler, ITextInput editor)
	{
		handler.PlatformView?.UpdateSelectionLength(editor);
	}

	static void MapFocus(MaterialEditorHandler handler, IEditor editor, object? args)
	{
		if (args is FocusRequest request)
		{
			handler.PlatformView.Focus(request);
		}
	}

	void OnTextChanged(object? sender, TextChangedEventArgs e)
	{
		// Let the mapping know that the update is coming from changes to the platform control
		DataFlowDirection = DataFlowDirection.FromPlatform;
		VirtualView?.UpdateText(e);

		// Reset to the default direction
		DataFlowDirection = DataFlowDirection.ToPlatform;
	}

	private void OnFocusChange(object? sender, FocusChangeEventArgs e)
	{
		if (!e.HasFocus)
		{
			VirtualView?.Completed();
		}
	}

	void OnSelectionChanged(object? sender, EventArgs e)
	{
		var cursorPosition = PlatformView.GetCursorPosition();
		var selectedTextLength = PlatformView.GetSelectedTextLength();

		if (VirtualView.CursorPosition != cursorPosition)
		{
			VirtualView.CursorPosition = cursorPosition;
		}

		if (VirtualView.SelectionLength != selectedTextLength)
		{
			VirtualView.SelectionLength = selectedTextLength;
		}
	}

	public override void PlatformArrange(Rect frame)
	{
		this.PrepareForTextViewArrange(frame);
		base.PlatformArrange(frame);
	}
}