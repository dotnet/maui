using System;
using Android.Content.Res;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Google.Android.Material.TextField;
using AView = Android.Views.View;

namespace Microsoft.Maui.Handlers;

// TODO: material3 - make it public in .net 11
internal class SearchBarHandler2 : ViewHandler<ISearchBar, MauiMaterialTextInputLayout>
{
    public static PropertyMapper<ISearchBar, SearchBarHandler2> Mapper =
    new(ViewMapper)
    {
        [nameof(ISearchBar.Background)] = MapBackground,
        [nameof(ISearchBar.CharacterSpacing)] = MapCharacterSpacing,
        [nameof(ISearchBar.Font)] = MapFont,
        [nameof(ITextAlignment.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
        [nameof(ITextAlignment.VerticalTextAlignment)] = MapVerticalTextAlignment,
        [nameof(ISearchBar.IsReadOnly)] = MapIsReadOnly,
        [nameof(ISearchBar.IsTextPredictionEnabled)] = MapIsTextPredictionEnabled,
        [nameof(ISearchBar.IsSpellCheckEnabled)] = MapIsSpellCheckEnabled,
        [nameof(ISearchBar.MaxLength)] = MapMaxLength,
        [nameof(ISearchBar.Placeholder)] = MapPlaceholder,
        [nameof(ISearchBar.PlaceholderColor)] = MapPlaceholderColor,
        [nameof(ISearchBar.Text)] = MapText,
        [nameof(ISearchBar.TextColor)] = MapTextColor,
        [nameof(ISearchBar.CancelButtonColor)] = MapCancelButtonColor,
        [nameof(ISearchBar.SearchIconColor)] = MapSearchIconColor,
        [nameof(ISearchBar.Keyboard)] = MapKeyboard,
        [nameof(ISearchBar.ReturnType)] = MapReturnType,
        [nameof(ISearchBar.FlowDirection)] = MapFlowDirection,
        [nameof(ISearchBar.IsEnabled)] = MapIsEnabled,
        [nameof(ISearchBar.CursorPosition)] = MapCursorPosition,
        [nameof(ISearchBar.SelectionLength)] = MapSelectionLength,
    };

    public static CommandMapper<ISearchBar, SearchBarHandler2> CommandMapper =
            new(ViewCommandMapper)
            {
                [nameof(ISearchBar.Focus)] = MapFocus
            };

    public EditText? QueryEditor => PlatformView?.EditText;

    public SearchBarHandler2() : base(Mapper, CommandMapper)
    {
    }

    protected override MauiMaterialTextInputLayout CreatePlatformView()
    {
        var layout = new MauiMaterialTextInputLayout(Context);
        layout.BoxBackgroundMode = TextInputLayout.BoxBackgroundFilled;
        layout.AddView(new MauiMaterialTextInputEditText(layout.Context!));
        return layout;
    }

    protected override void ConnectHandler(MauiMaterialTextInputLayout platformView)
    {
        base.ConnectHandler(platformView);
        if (platformView.EditText is not null)
        {
            platformView.EditText.TextChanged += OnTextChanged;
            platformView.EditText.EditorAction += OnEditorAction;
            platformView.EditText.FocusChange += OnFocusChange;
            if (platformView.EditText is MauiMaterialTextInputEditText editText)
            {
                editText.SelectionChanged += OnSelectionChanged;
            }
        }
    }

    protected override void DisconnectHandler(MauiMaterialTextInputLayout platformView)
    {
        if (platformView.EditText is not null)
        {
            platformView.EditText.TextChanged -= OnTextChanged;
            platformView.EditText.EditorAction -= OnEditorAction;
            platformView.EditText.FocusChange -= OnFocusChange;

            if (platformView.EditText is MauiMaterialTextInputEditText editText)
            {
                editText.SelectionChanged -= OnSelectionChanged;
            }
        }

        base.DisconnectHandler(platformView);
    }

    public static void MapBackground(SearchBarHandler2 handler, ISearchBar searchBar)
    {
        handler.PlatformView?.UpdateBackground(searchBar);
    }

    public static void MapCharacterSpacing(SearchBarHandler2 handler, ISearchBar searchBar)
    {
        handler.PlatformView?.EditText?.UpdateCharacterSpacing(searchBar);
    }

    public static void MapFont(SearchBarHandler2 handler, ISearchBar searchBar)
    {
        var fontManager = handler.GetRequiredService<IFontManager>();
        handler.PlatformView?.EditText?.UpdateFont(searchBar, fontManager);
    }

    public static void MapHorizontalTextAlignment(SearchBarHandler2 handler, ISearchBar searchBar)
    {
        handler.PlatformView?.EditText?.UpdateHorizontalTextAlignment(searchBar);
    }

    public static void MapVerticalTextAlignment(SearchBarHandler2 handler, ISearchBar searchBar)
    {
        handler.PlatformView?.EditText?.UpdateVerticalTextAlignment(searchBar);
    }

    public static void MapIsReadOnly(SearchBarHandler2 handler, ISearchBar searchBar)
    {
        handler.PlatformView.EditText?.UpdateIsReadOnly(searchBar);
    }

    public static void MapIsTextPredictionEnabled(SearchBarHandler2 handler, ISearchBar searchBar)
    {
        handler.PlatformView?.EditText?.UpdateIsTextPredictionEnabled(searchBar as ITextInput);
    }

    public static void MapIsSpellCheckEnabled(SearchBarHandler2 handler, ISearchBar searchBar)
    {
        handler.PlatformView?.EditText?.UpdateIsSpellCheckEnabled(searchBar as ITextInput);
    }

    public static void MapMaxLength(SearchBarHandler2 handler, ISearchBar searchBar)
    {
        handler.PlatformView?.EditText?.UpdateMaxLength(searchBar.MaxLength);
    }

    public static void MapPlaceholder(SearchBarHandler2 handler, ISearchBar searchBar)
    {
        handler.PlatformView?.EditText?.UpdatePlaceholder(searchBar);
    }

    public static void MapPlaceholderColor(SearchBarHandler2 handler, ISearchBar searchBar)
    {
        handler.PlatformView?.EditText?.UpdatePlaceholderColor(searchBar);
    }

    public static void MapText(SearchBarHandler2 handler, ISearchBar searchBar)
    {
        handler.PlatformView?.EditText?.UpdateText(searchBar);
        handler.PlatformView?.UpdateCloseButtonVisibility(!string.IsNullOrEmpty(searchBar.Text));
    }

    public static void MapTextColor(SearchBarHandler2 handler, ISearchBar searchBar)
    {
        handler.PlatformView?.EditText?.UpdateTextColor(searchBar.TextColor);
    }

    public static void MapCancelButtonColor(SearchBarHandler2 handler, ISearchBar searchBar)
    {
        handler.PlatformView?.UpdateCancelButtonColor(searchBar);
    }

    public static void MapSearchIconColor(SearchBarHandler2 handler, ISearchBar searchBar)
    {
        handler.PlatformView?.UpdateSearchIconColor(searchBar);
    }

    public static void MapKeyboard(SearchBarHandler2 handler, ISearchBar searchBar)
    {
        handler.UpdateValue(nameof(ISearchBar.Text));
        handler.PlatformView?.EditText?.SetInputType(searchBar);
    }

    public static void MapReturnType(SearchBarHandler2 handler, ISearchBar searchBar)
    {
        handler.PlatformView?.EditText?.UpdateReturnType(searchBar);
    }

    public static void MapFlowDirection(SearchBarHandler2 handler, ISearchBar searchBar)
    {
        if (searchBar.FlowDirection == FlowDirection.MatchParent && searchBar.Parent is not null && searchBar.Parent is IView parentView)
        {
            // When FlowDirection is MatchParent, respect the parent's FlowDirection
            if (handler.PlatformView is AView platformView)
            {
                Microsoft.Maui.Platform.ViewExtensions.UpdateFlowDirection(platformView, parentView);
            }

            if (handler.PlatformView?.EditText is TextView textView)
            {
                Microsoft.Maui.Platform.TextViewExtensions.UpdateFlowDirection(textView, parentView);
            }
        }
        else
        {
            // Otherwise, use the SearchBar's own FlowDirection
            handler.PlatformView?.UpdateFlowDirection(searchBar);
            handler.PlatformView?.EditText?.UpdateFlowDirection(searchBar);
        }
    }

    public static void MapIsEnabled(SearchBarHandler2 handler, ISearchBar searchBar)
    {
        handler.PlatformView?.UpdateIsEnabled(searchBar);
    }

    public static void MapFocus(SearchBarHandler2 handler, ISearchBar searchBar, object? args)
    {
        if (args is FocusRequest request)
        {
            handler.PlatformView?.EditText?.Focus(request);
        }
    }

    public static void MapCursorPosition(SearchBarHandler2 handler, ISearchBar searchBar)
    {
        handler.PlatformView?.EditText?.UpdateCursorPosition(searchBar);
    }

    public static void MapSelectionLength(SearchBarHandler2 handler, ISearchBar searchBar)
    {
        handler.PlatformView?.EditText?.UpdateSelectionLength(searchBar);
    }

    void OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (VirtualView is null || sender is not EditText editText)
        {
            return;
        }

        var newText = editText.Text ?? string.Empty;
        VirtualView.UpdateText(newText);

        // Update clear button visibility based on text content
        PlatformView?.UpdateCloseButtonVisibility(!string.IsNullOrEmpty(newText));
    }

    void OnEditorAction(object? sender, TextView.EditorActionEventArgs e)
    {
        if (e.ActionId == ImeAction.Search || e.ActionId == ImeAction.Done)
        {
            VirtualView?.SearchButtonPressed();
            e.Handled = true;
        }
        else
        {
            e.Handled = false;
        }
    }

    void OnSelectionChanged(object? sender, EventArgs e)
    {
        if (PlatformView.EditText is null)
        {
            return;
        }

        var cursorPosition = PlatformView.EditText.GetCursorPosition();
        var selectionLength = PlatformView.EditText.GetSelectedTextLength();

        if (VirtualView.CursorPosition != cursorPosition)
        {
            VirtualView.CursorPosition = cursorPosition;
        }

        if (VirtualView.SelectionLength != selectionLength)
        {
            VirtualView.SelectionLength = selectionLength;
        }
    }

    void OnFocusChange(object? sender, View.FocusChangeEventArgs e)
    {
        VirtualView?.IsFocused = e.HasFocus;
    }
}