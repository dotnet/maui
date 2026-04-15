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
internal class SearchBarHandler2 : ViewHandler<ISearchBar, MauiMaterialSearchBarTextInputLayout>
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

    protected override MauiMaterialSearchBarTextInputLayout CreatePlatformView()
    {
        var layout = new MauiMaterialSearchBarTextInputLayout(Context);
        layout.BoxBackgroundMode = TextInputLayout.BoxBackgroundFilled;
        layout.AddView(new MauiMaterialSearchBarTextInputEditText(layout.Context!));
        return layout;
    }

    protected override void ConnectHandler(MauiMaterialSearchBarTextInputLayout platformView)
    {
        base.ConnectHandler(platformView);
        if (platformView.EditText is not null)
        {
            platformView.EditText.TextChanged += OnTextChanged;
            platformView.EditText.EditorAction += OnEditorAction;
            platformView.EditText.FocusChange += OnFocusChange;
            if (platformView.EditText is MauiMaterialSearchBarTextInputEditText editText)
            {
                editText.SelectionChanged += OnSelectionChanged;
            }
        }
    }

    protected override void DisconnectHandler(MauiMaterialSearchBarTextInputLayout platformView)
    {
        if (platformView.EditText is not null)
        {
            platformView.EditText.TextChanged -= OnTextChanged;
            platformView.EditText.EditorAction -= OnEditorAction;
            platformView.EditText.FocusChange -= OnFocusChange;

            if (platformView.EditText is MauiMaterialSearchBarTextInputEditText editText)
            {
                editText.SelectionChanged -= OnSelectionChanged;
            }
        }

        platformView.SetEndIconOnClickListener(null);

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
        handler.PlatformView?.EditText?.UpdateIsReadOnly(searchBar);
    }

    public static void MapIsTextPredictionEnabled(SearchBarHandler2 handler, ISearchBar searchBar)
    {
        handler.PlatformView?.EditText?.UpdateIsTextPredictionEnabled(searchBar);
    }

    public static void MapIsSpellCheckEnabled(SearchBarHandler2 handler, ISearchBar searchBar)
    {
        handler.PlatformView?.EditText?.UpdateIsSpellCheckEnabled(searchBar);
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
        var returnType = VirtualView?.ReturnType;

        // Inside android implementations that map events to listeners, the default return value for "Handled" is always true.
        // Setting handled to false here maintains default behavior.
        bool handled = false;

        if (returnType is not null && PlatformView?.EditText is not null)
        {
            var actionId = e.ActionId;
            var evt = e.Event;
            ImeAction currentInputImeFlag = PlatformView.EditText.ImeOptions;

            // On API 34 the issue where actionId is ImeAction.ImeNull when using a hardware keyboard was fixed.
            // Normalize it here so the rest of the logic is consistent across API levels.
            if (actionId == ImeAction.ImeNull && evt?.KeyCode == Keycode.Enter)
            {
                actionId = currentInputImeFlag;
            }

            // Hardware keyboard path: consume Down event, fire on Up event.
            if (evt?.KeyCode == Keycode.Enter && evt?.Action == KeyEventActions.Down)
            {
                handled = true;
            }
            else if (evt?.KeyCode == Keycode.Enter && evt?.Action == KeyEventActions.Up)
            {
                VirtualView?.SearchButtonPressed();
            }
            // Input pane path: fire when the action matches either the default Search action
            // or the ImeAction configured via ReturnType (Go, Send, Done, etc.).
            else if (evt?.KeyCode is null && (actionId == ImeAction.Search || actionId == currentInputImeFlag))
            {
                VirtualView?.SearchButtonPressed();
                // For Search, Go, Send the EditorAction is also invoked for KeyEventActions,
                // which would cause SearchButtonPressed to fire twice — so consume the event.
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

    void OnSelectionChanged(object? sender, EventArgs e)
    {
        if (PlatformView.EditText is null || VirtualView is null)
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
