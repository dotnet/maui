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
				};
			}
		}

		protected override Editor CreatePlatformView() => new MauiEditor();

		protected override void ConnectHandler(Editor nativeView)
		{
			nativeView.TextChanged += OnTextChanged;
			nativeView.FocusLost += OnFocusLost;
			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(Editor nativeView)
		{
			nativeView.TextChanged -= OnTextChanged;
			nativeView.FocusLost -= OnFocusLost;
			base.DisconnectHandler(nativeView);
		}

		public static void MapBackground(EditorHandler handler, IEditor editor)
		{
			handler.UpdateValue(nameof(handler.ContainerView));
			handler.GetWrappedNativeView()?.UpdateBackground(editor);
		}
		public static void MapText(EditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateText(editor);

		public static void MapTextColor(EditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateTextColor(editor);

		public static void MapPlaceholder(EditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdatePlaceholder(editor);

		public static void MapPlaceholderColor(EditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdatePlaceholderColor(editor);

		[MissingMapper]
		public static void MapCharacterSpacing(EditorHandler handler, IEditor editor) { }

		public static void MapMaxLength(EditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateMaxLength(editor);

		public static void MapIsReadOnly(EditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateIsReadOnly(editor);

		public static void MapIsTextPredictionEnabled(EditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateIsTextPredictionEnabled(editor);

		public static void MapFont(EditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateFont(editor, handler.GetRequiredService<IFontManager>());

		public static void MapHorizontalTextAlignment(EditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateHorizontalTextAlignment(editor);

		[MissingMapper]
		public static void MapVerticalTextAlignment(EditorHandler handler, IEditor editor) { }

		public static void MapKeyboard(EditorHandler handler, IEditor editor) =>
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