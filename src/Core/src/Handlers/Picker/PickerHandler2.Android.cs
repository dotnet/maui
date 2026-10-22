using System;
using Android.Content.Res;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using AndroidX.Core.View;
using AndroidX.Core.View.Accessibility;
using Google.Android.Material.Dialog;
using Google.Android.Material.TextField;
using Microsoft.Maui.Graphics;
using AppCompatAlertDialog = AndroidX.AppCompat.App.AlertDialog;
using AResource = Android.Resource;

namespace Microsoft.Maui.Handlers;

// TODO: Material3 - make it public in .net 11
internal partial class PickerHandler2 : ViewHandler<IPicker, MauiMaterialTextInputLayout>
{
	AppCompatAlertDialog? _dialog;
	ColorStateList? _defaultHintTextColors;
	EndIconClickListener? _endIconClickListener;
	PickerAccessibilityDelegate? _accessibilityDelegate;

	MauiMaterialPicker? Picker => PlatformView?.EditText as MauiMaterialPicker;

	public static PropertyMapper<IPicker, PickerHandler2> Mapper =
		new(ViewMapper)
		{
			[nameof(IPicker.Background)] = MapBackground,
			[nameof(IPicker.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(IPicker.Font)] = MapFont,
			[nameof(ITextAlignment.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
			[nameof(IPicker.Items)] = MapItems,
			[nameof(IPicker.SelectedIndex)] = MapSelectedIndex,
			[nameof(IPicker.TextColor)] = MapTextColor,
			[nameof(IPicker.Title)] = MapTitle,
			[nameof(IPicker.TitleColor)] = MapTitleColor,
			[nameof(ITextAlignment.VerticalTextAlignment)] = MapVerticalTextAlignment,
			[nameof(IPicker.IsOpen)] = MapIsOpen,
			[nameof(IView.IsEnabled)] = MapIsEnabled,
			[nameof(IView.FlowDirection)] = MapFlowDirection,
			[nameof(IView.Semantics)] = MapSemantics,
			[nameof(IView.AutomationId)] = MapAutomationId,
		};

	public static CommandMapper<IPicker, PickerHandler2> CommandMapper = new(ViewCommandMapper)
	{
		[nameof(IPicker.Focus)] = MapFocus,
		[nameof(IPicker.Unfocus)] = MapUnfocus,
	};

	public PickerHandler2() : base(Mapper, CommandMapper)
	{
	}

	protected override MauiMaterialTextInputLayout CreatePlatformView()
	{
		var layout = new MauiMaterialTextInputLayout(MauiMaterialContextThemeWrapper.Create(Context))
		{
			BoxBackgroundMode = TextInputLayout.BoxBackgroundOutline,
			EndIconMode = TextInputLayout.EndIconCustom,
			EndIconCheckable = false,
		};
		// Keep TextInputLayout's context overlay and let it supply the child's box background.
		var picker = new MauiMaterialPicker(layout.Context!, wrapContext: false) { Background = null };
		layout.AddView(picker);
		layout.SetEndIconDrawable(Resource.Drawable.mtrl_ic_arrow_drop_down);
		layout.SetEndIconContentDescription(Resource.String.exposed_dropdown_menu_content_description);
		_defaultHintTextColors = layout.DefaultHintTextColor;
		return layout;
	}

	protected override void ConnectHandler(MauiMaterialTextInputLayout platformView)
	{
		platformView.Click += OnClick;
		platformView.EditText!.Click += OnClick;
		_endIconClickListener = new EndIconClickListener(this);
		platformView.SetEndIconOnClickListener(_endIconClickListener);
		// Keep the native text-field delegate and apply MAUI semantics to its child.
		_accessibilityDelegate = new PickerAccessibilityDelegate(ViewCompat.GetAccessibilityDelegate(platformView.EditText!)) { Handler = this };
		ViewCompat.SetAccessibilityDelegate(platformView.EditText!, _accessibilityDelegate);
		platformView.ImportantForAccessibility = ImportantForAccessibility.No;

		base.ConnectHandler(platformView);
	}

	protected override void DisconnectHandler(MauiMaterialTextInputLayout platformView)
	{
		platformView.Click -= OnClick;
		platformView.EditText!.Click -= OnClick;
		platformView.SetEndIconOnClickListener(null);
		_endIconClickListener?.Dispose();
		_endIconClickListener = null;
		if (_accessibilityDelegate is { } accessibilityDelegate)
		{
			accessibilityDelegate.Handler = null;
			if (ViewCompat.GetAccessibilityDelegate(platformView.EditText!) == accessibilityDelegate)
				ViewCompat.SetAccessibilityDelegate(platformView.EditText!, accessibilityDelegate.WrappedDelegate);
			accessibilityDelegate.Dispose();
			_accessibilityDelegate = null;
		}

		if (_dialog is not null)
		{
			_dialog.ShowEvent -= OnDialogShown;
			_dialog.DismissEvent -= OnDialogDismiss;
			_dialog.Dismiss();
			_dialog = null;
		}
		VirtualView.IsFocused = false;
		VirtualView.IsOpen = false;

		base.DisconnectHandler(platformView);
	}

	public static void MapBackground(PickerHandler2 handler, IPicker picker)
	{
		// Paint the container, never the EditText (whose mapper adds an AppCompat underline).
		if (picker.Background.IsNullOrEmpty())
			handler.PlatformView.Background = null;
		else
			handler.PlatformView.UpdateBackground(picker);
	}

	public static void MapItems(PickerHandler2 handler, IPicker picker) => Reload(handler);

	public static void MapSemantics(PickerHandler2 handler, IPicker picker)
	{
		if (handler.Picker is { } platformPicker)
			ViewCompat.SetAccessibilityHeading(platformPicker, picker.Semantics?.IsHeading ?? false);
	}

	public static void MapAutomationId(PickerHandler2 handler, IPicker picker)
	{
		if (handler.Picker is { } platformPicker)
		{
			if (string.IsNullOrWhiteSpace(picker.AutomationId))
				platformPicker.ContentDescription = null;
			else
				platformPicker.UpdateAutomationId(picker);
		}
	}

	public static void MapTitle(PickerHandler2 handler, IPicker picker)
	{
		handler.PlatformView.Hint = picker.Title;
	}

	public static void MapTitleColor(PickerHandler2 handler, IPicker picker)
	{
		handler.PlatformView.DefaultHintTextColor = picker.TitleColor is null
			? handler._defaultHintTextColors
			: ColorStateList.ValueOf(picker.TitleColor.ToPlatform());
	}

	public static void MapSelectedIndex(PickerHandler2 handler, IPicker picker)
	{
		Reload(handler);
	}

	public static void MapCharacterSpacing(PickerHandler2 handler, IPicker picker)
	{
		handler.Picker?.UpdateCharacterSpacing(picker);
	}

	public static void MapFont(PickerHandler2 handler, IPicker picker)
	{
		var fontManager = handler.GetRequiredService<IFontManager>();

		handler.Picker?.UpdateFont(picker, fontManager);
		handler.PlatformView.Typeface = fontManager.GetTypeface(picker.Font);
	}

	public static void MapHorizontalTextAlignment(PickerHandler2 handler, IPicker picker)
	{
		handler.Picker?.UpdateHorizontalAlignment(picker.HorizontalTextAlignment);
	}

	public static void MapTextColor(PickerHandler2 handler, IPicker picker)
	{
		handler.Picker?.UpdateTextColor(picker.TextColor);
	}

	public static void MapVerticalTextAlignment(PickerHandler2 handler, IPicker picker)
	{
		handler.Picker?.UpdateVerticalAlignment(picker.VerticalTextAlignment);
	}

	public static void MapIsEnabled(PickerHandler2 handler, IPicker picker)
	{
		handler.PlatformView.Enabled = picker.IsEnabled;
		if (!picker.IsEnabled)
			handler.DismissPickerDialog();
	}

	public static void MapFlowDirection(PickerHandler2 handler, IPicker picker)
	{
		handler.PlatformView.UpdateFlowDirection(picker);
		handler.Picker?.UpdateFlowDirection(picker);
		if (handler._dialog is not null && handler.Picker is { } platformPicker)
			handler._dialog.UpdateFlowDirection(platformPicker);
	}

	public static void MapIsOpen(PickerHandler2 handler, IPicker picker)
	{
		if (handler.IsConnected())
		{
			if (picker.IsOpen)
			{
				if (picker.IsEnabled)
					handler.ShowDialog();
				else
					picker.IsOpen = false;
			}
			else
			{
				handler.DismissPickerDialog();
			}
		}
	}

	public static void MapFocus(PickerHandler2 handler, IPicker picker, object? args)
	{
		if (handler.IsConnected())
		{
			if (args is FocusRequest request)
				handler.Picker?.Focus(request);
			handler.ShowDialog();
		}
	}

	public static void MapUnfocus(PickerHandler2 handler, IPicker picker, object? args)
	{
		if (handler.IsConnected())
		{
			handler.DismissPickerDialog();
			handler.Picker?.ClearFocus();
			ViewHandler.MapUnfocus(handler, picker, args);
		}
	}

	void ShowDialog()
	{
		OnClick(PlatformView, EventArgs.Empty);
	}

	void OnClick(object? sender, EventArgs e)
	{
		if (_dialog is null && VirtualView is { IsEnabled: true } && this.IsConnected())
		{
			CreatePickerDialog();
			ShowPickerDialog();
		}
	}

	void CreatePickerDialog()
	{
		// Use MauiMaterialContextThemeWrapper to ensure proper Material 3 theming
		// for the dialog when Material 3 is enabled in the project
		using (var builder = new MaterialAlertDialogBuilder(MauiMaterialContextThemeWrapper.Create(Context)))
		{
			SetupDialogTitle(builder);
			SetupDialogItems(builder);
			builder.SetNegativeButton(AResource.String.Cancel, (o, args) => { });

			_dialog = builder.Create();
		}
	}

	void SetupDialogTitle(MaterialAlertDialogBuilder builder)
	{
		if (VirtualView.TitleColor is null)
		{
			builder.SetTitle(VirtualView.Title ?? string.Empty);
		}
		else
		{
			var title = new SpannableString(VirtualView.Title ?? string.Empty);
			title.SetSpan(new ForegroundColorSpan(VirtualView.TitleColor.ToPlatform()), 0, title.Length(), SpanTypes.ExclusiveExclusive);
			builder.SetTitle(title);
		}
	}

	void SetupDialogItems(MaterialAlertDialogBuilder builder)
	{
		string[] items = VirtualView.GetItemsAsArray();

		// Ensure no null items
		for (var i = 0; i < items.Length; i++)
		{
			var item = items[i];
			if (item is null)
			{
				items[i] = string.Empty;
			}
		}

		builder.SetSingleChoiceItems(items, VirtualView.SelectedIndex, (sender, e) =>
		{
			var selectedIndex = e.Which;
			VirtualView.SelectedIndex = selectedIndex;
			Reload(this);

			DismissPickerDialog();
		});
	}

	void ShowPickerDialog()
	{
		if (_dialog is null)
		{
			return;
		}

		if (Picker is { } platformPicker)
			_dialog.UpdateFlowDirection(platformPicker);
		_dialog.SetCanceledOnTouchOutside(true);

		_dialog.ShowEvent += OnDialogShown;
		_dialog.DismissEvent += OnDialogDismiss;

		_dialog.Show();
		VirtualView.IsOpen = true;
	}

	void DismissPickerDialog()
	{
		_dialog?.Dismiss();
	}

	void OnDialogDismiss(object? sender, EventArgs e)
	{
		if (_dialog is null)
		{
			return;
		}

		// Clean up ALL event handlers before nulling the reference
		_dialog.ShowEvent -= OnDialogShown;  // Remove ShowEvent handler to prevent leak
		_dialog.DismissEvent -= OnDialogDismiss;
		VirtualView.IsFocused = false;
		VirtualView.IsOpen = false;
		_dialog = null;
	}

	void OnDialogShown(object? sender, EventArgs e)
	{
		if (_dialog is null)
		{
			return;
		}

		_dialog.ShowEvent -= OnDialogShown;
		VirtualView.IsFocused = true;
	}

	static void Reload(PickerHandler2 handler)
	{
		if (handler.Picker is { } platformPicker)
		{
			var picker = handler.VirtualView;
			platformPicker.Text = picker.SelectedIndex < 0 || picker.SelectedIndex >= picker.GetCount()
				? null
				: picker.GetItem(picker.SelectedIndex);
		}
	}

	internal sealed class PickerAccessibilityDelegate : AccessibilityDelegateCompatWrapper
	{
		public PickerHandler2? Handler { get; set; }

		public PickerAccessibilityDelegate(AccessibilityDelegateCompat? originalDelegate) : base(originalDelegate)
		{
		}

		public override void OnInitializeAccessibilityNodeInfo(View? host, AccessibilityNodeInfoCompat? info)
		{
			base.OnInitializeAccessibilityNodeInfo(host, info);
			if (Handler?.VirtualView is { } picker)
				host?.UpdateSemanticNodeInfo(picker, info);
		}
	}

	sealed class EndIconClickListener : Java.Lang.Object, View.IOnClickListener
	{
		readonly WeakReference<PickerHandler2> _handler;

		public EndIconClickListener(PickerHandler2 handler) => _handler = new(handler);

		public void OnClick(View? view)
		{
			if (_handler.TryGetTarget(out var handler))
				handler.ShowDialog();
		}
	}
}
