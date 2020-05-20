using System.Maui.Core.Controls;
using System.Windows;
using System.Windows.Media;
using WpfScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility;
using WControl = System.Windows.Controls.Control;

namespace System.Maui.Platform
{
	public partial class EditorRenderer : AbstractViewRenderer<IEditor, MauiTextBox>
	{
		Brush _placeholderDefaultBrush;
		Brush _foregroundDefaultBrush;

		protected override MauiTextBox CreateView()
		{
			var control = new MauiTextBox { VerticalScrollBarVisibility = WpfScrollBarVisibility.Visible, TextWrapping = TextWrapping.Wrap, AcceptsReturn = true };
			control.LostFocus += OnTextBoxUnfocused;
			control.TextChanged += OnTextBoxTextChanged;

			return control;
		}

		protected override void SetupDefaults()
		{
			_placeholderDefaultBrush = (Brush)WControl.ForegroundProperty.GetMetadata(typeof(MauiTextBox)).DefaultValue;
			_foregroundDefaultBrush = (Brush)WControl.ForegroundProperty.GetMetadata(typeof(MauiTextBox)).DefaultValue;
			base.SetupDefaults();
		}

		protected override void DisposeView(MauiTextBox nativeView)
		{
			nativeView.LostFocus -= OnTextBoxUnfocused;
			nativeView.TextChanged -= OnTextBoxTextChanged;
			base.DisposeView(nativeView);
		}

		public static void MapPropertyColor(IViewRenderer renderer, IEditor editor) => (renderer as EditorRenderer)?.UpdateTextColor();
		public static void MapPropertyPlaceholder(IViewRenderer renderer, IEditor editor) => (renderer as EditorRenderer)?.UpdatePlaceholder();
		public static void MapPropertyPlaceholderColor(IViewRenderer renderer, IEditor editor) => (renderer as EditorRenderer)?.UpdatePlaceholderColor();
		public static void MapPropertyText(IViewRenderer renderer, IEditor editor) => (renderer as EditorRenderer)?.UpdateText();
		public static void MapPropertyMaxLenght(IViewRenderer renderer, IEditor editor) => (renderer as EditorRenderer)?.UpdateMaxLength();
		public static void MapPropertyAutoSize(IViewRenderer renderer, IEditor editor) { }


		public virtual void UpdatePlaceholder()
		{
			TypedNativeView.PlaceholderText = VirtualView.Placeholder ?? string.Empty;
		}

		public virtual void UpdatePlaceholderColor()
		{
			Color placeholderColor = VirtualView.PlaceholderColor;

			if (placeholderColor.IsDefault)
			{
				// Use the cached default brush
				TypedNativeView.PlaceholderForegroundBrush = _placeholderDefaultBrush;
				return;
			}

			TypedNativeView.PlaceholderForegroundBrush = placeholderColor.ToBrush();
		}

		void OnTextBoxTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs textChangedEventArgs)
		{
			VirtualView.Text = TypedNativeView.Text;
		}

		void OnTextBoxUnfocused(object sender, RoutedEventArgs e)
		{
			
		}

		public virtual void UpdateText()
		{
			string newText = VirtualView.Text ?? "";

			if (TypedNativeView.Text == newText)
				return;

			TypedNativeView.Text = newText;
			TypedNativeView.SelectionStart = TypedNativeView.Text.Length;
		}

		public virtual void UpdateTextColor()
		{
			TypedNativeView.UpdateDependencyColor(System.Windows.Controls.Control.ForegroundProperty, VirtualView.Color);
		}

		public virtual void UpdateMaxLength()
		{
			var maxLength = VirtualView.MaxLength;
			TypedNativeView.MaxLength = maxLength;

			var currentControlText = TypedNativeView.Text;

			if (currentControlText.Length > maxLength)
				TypedNativeView.Text = currentControlText.Substring(0, maxLength);
		}

		public virtual void UpdateIsReadOnly()
		{
			
		}
	}
}