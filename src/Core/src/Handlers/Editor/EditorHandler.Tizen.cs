using System;
using Tizen.NUI.BaseComponents;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.NUI;
using TSize = Tizen.UIExtensions.Common.Size;

namespace Microsoft.Maui.Handlers
{
	public partial class EditorHandler : ViewHandler<IEditor, Editor>
	{
		class MauiEditor : Editor, IMeasurable
		{
			TSize IMeasurable.Measure(double availableWidth, double availableHeight)
			{
				if (!string.IsNullOrEmpty(Text))
				{
					if (availableWidth < NaturalSize.Width)
					{
						return new TSize(availableWidth, NaturalSize.Height);
					}
					else if (NaturalSize.Width > 0)
					{
						return new TSize(NaturalSize.Width, NaturalSize.Height);
					}
					else
					{
						// even though text but natural size is zero. it is abnormal state
						return new TSize(Math.Max(Text.Length * PixelSize + 10, availableWidth), PixelSize + 10);
					}
				}
				else
				{
					return new TSize(Math.Max(PixelSize + 10, availableWidth), PixelSize + 10);
				}
				;
			}
		}

		protected override Editor CreatePlatformView() => new MauiEditor
		{
			Focusable = true,
			FocusableInTouch = true,
		};

		protected override void ConnectHandler(Editor platformView)
		{
			platformView.TextChanged += OnTextChanged;
			platformView.FocusLost += OnFocusLost;
			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(Editor platformView)
		{
			if (!platformView.HasBody())
				return;

			platformView.TextChanged -= OnTextChanged;
			platformView.FocusLost -= OnFocusLost;
			base.DisconnectHandler(platformView);
		}

		public static void MapBackground(IEditorHandler handler, IEditor editor)
		{
			handler.UpdateValue(nameof(handler.ContainerView));
			handler.ToPlatform()?.UpdateBackground(editor);
		}

		public static void MapText(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateText(editor);

		public static void MapTextColor(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateTextColor(editor);

		public static void MapPlaceholder(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdatePlaceholder(editor);

		public static void MapPlaceholderColor(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdatePlaceholderColor(editor);

		public static void MapCursorPosition(IEditorHandler handler, ITextInput editor) =>
			handler.PlatformView?.UpdateCursorPosition(editor);

		public static void MapSelectionLength(IEditorHandler handler, ITextInput editor) =>
			handler.PlatformView?.UpdateSelectionLength(editor);

		public static void MapCharacterSpacing(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView.UpdateCharacterSpacing(editor);

		public static void MapMaxLength(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateMaxLength(editor);

		public static void MapIsReadOnly(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateIsReadOnly(editor);

		public static void MapIsTextPredictionEnabled(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateIsTextPredictionEnabled(editor);

		[MissingMapper]
		public static void MapIsSpellCheckEnabled(IEditorHandler handler, IEditor editor) { }

		public static void MapFont(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateFont(editor, handler.GetRequiredService<IFontManager>());

		public static void MapHorizontalTextAlignment(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateHorizontalTextAlignment(editor);

		public static void MapVerticalTextAlignment(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView.UpdateVerticalTextAlignment(editor);

		public static void MapKeyboard(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateKeyboard(editor);

		void OnTextChanged(object? sender, TextEditor.TextChangedEventArgs e)
		{
			if (VirtualView == null || PlatformView == null)
				return;

			VirtualView.Text = PlatformView.Text;
		}
		void OnFocusLost(object? sender, System.EventArgs e)
		{
			if (VirtualView == null || PlatformView == null)
				return;

			VirtualView.Completed();
		}
	}
}