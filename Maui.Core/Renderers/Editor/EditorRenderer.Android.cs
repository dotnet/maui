using System.Collections.Generic;
using System.Maui.Core.Controls;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Java.Lang;

namespace System.Maui.Platform
{
	public partial class EditorRenderer : AbstractViewRenderer<IEditor, MauiEditor>
	{
		TextColorSwitcher _hintColorSwitcher;
		TextColorSwitcher _textColorSwitcher;

		protected override MauiEditor CreateView()
		{
			MauiEditor mauiEditor = new MauiEditor(Context)
			{
				ImeOptions = ImeAction.Done
			};

			mauiEditor.SetSingleLine(false);
			mauiEditor.Gravity = GravityFlags.Top;

			if ((int)Build.VERSION.SdkInt > 16)
				mauiEditor.TextAlignment = TextAlignment.ViewStart;

			mauiEditor.SetHorizontallyScrolling(false);

			mauiEditor.AddTextChangedListener(new MauiTextChangedListener(VirtualView));

			_hintColorSwitcher = new TextColorSwitcher(mauiEditor);
			_textColorSwitcher = new TextColorSwitcher(mauiEditor);

			return mauiEditor;
		}

		protected override void DisposeView(MauiEditor nativeView)
		{
			_hintColorSwitcher = null;
			_textColorSwitcher = null;

			base.DisposeView(nativeView);
		}

		public static void MapPropertyColor(IViewRenderer renderer, IEditor editor)
		{
			(renderer as EditorRenderer)?.UpdateTextColor(editor.Color);
		}

		public static void MapPropertyPlaceholder(IViewRenderer renderer, IEditor editor)
		{
			if (!(renderer.NativeView is MauiEditor mauiEditor))
				return;

			if (mauiEditor.Hint == editor.Placeholder)
				return;

			mauiEditor.Hint = editor.Placeholder;
		}

		public static void MapPropertyPlaceholderColor(IViewRenderer renderer, IEditor editor)
		{
			(renderer as EditorRenderer)?.UpdatePlaceholderColor(editor.PlaceholderColor);
		}

		public static void MapPropertyText(IViewRenderer renderer, IEditor editor)
		{
			if (!(renderer.NativeView is MauiEditor mauiEditor))
				return;

			string newText = editor.Text ?? string.Empty;

			if (editor.Text == newText)
				return;

			newText = TrimToMaxLength(newText, editor.MaxLength);
			mauiEditor.Text = newText;
			mauiEditor.SetSelection(newText.Length);
		}

		public static void MapPropertyMaxLenght(IViewRenderer renderer, IEditor editor)
		{
			if (!(renderer.NativeView is MauiEditor mauiEditor))
				return;

			var currentFilters = new List<IInputFilter>(mauiEditor?.GetFilters() ?? new IInputFilter[0]);

			for (var i = 0; i < currentFilters.Count; i++)
			{
				if (currentFilters[i] is InputFilterLengthFilter)
				{
					currentFilters.RemoveAt(i);
					break;
				}
			}

			currentFilters.Add(new InputFilterLengthFilter(editor.MaxLength));

			if (mauiEditor == null)
				return;

			mauiEditor.SetFilters(currentFilters.ToArray());
			mauiEditor.Text = TrimToMaxLength(mauiEditor.Text, editor.MaxLength);
		}

		public static void MapPropertyAutoSize(IViewRenderer renderer, IEditor editor)
		{

		}

		protected virtual void UpdateTextColor(Color color)
		{
			_textColorSwitcher?.UpdateTextColor(color);
		}

		protected virtual void UpdatePlaceholderColor(Color color)
		{
			_hintColorSwitcher?.UpdateHintTextColor(color);
		}

		static string TrimToMaxLength(string currentText, int maxLenght)
		{
			if (currentText == null || currentText.Length <= maxLenght)
				return currentText;

			return currentText.Substring(0, maxLenght);
		}
	}

	internal class MauiTextChangedListener : Java.Lang.Object, ITextWatcher
	{
		readonly IEditor _virtualView;

		public MauiTextChangedListener(IEditor virtualView)
		{
			_virtualView = virtualView;
		}

		public void AfterTextChanged(IEditable s)
		{
	
		}

		public void BeforeTextChanged(ICharSequence s, int start, int count, int after)
		{
	
		}

		public void OnTextChanged(ICharSequence s, int start, int before, int count)
		{
			if (string.IsNullOrEmpty(_virtualView.Text) && s.Length() == 0)
				return;

			var newText = s.ToString();

			if (_virtualView.Text != newText)
				_virtualView.Text = newText;
		}
	}
}