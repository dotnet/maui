using System.Maui.Core.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WControl = System.Windows.Controls.Control;

namespace System.Maui.Platform
{
	public partial class SearchRenderer : AbstractViewRenderer<ISearch, MauiTextBox>
	{
		const string DefaultPlaceholder = "Search";
		Brush _defaultPlaceholderColorBrush;
		Brush _defaultTextColorBrush;

		protected override MauiTextBox CreateView()
		{
			var scope = new InputScope();
			var name = new InputScopeName();
			scope.Names.Add(name);

			var control = new MauiTextBox { InputScope = scope };
			control.KeyUp += OnTextBoxKeyUp;
			control.TextChanged += OnTextTextBoxChanged;

			return control;
		}

		protected override void SetupDefaults()
		{
			_defaultPlaceholderColorBrush = (Brush)WControl.ForegroundProperty.GetMetadata(typeof(MauiTextBox)).DefaultValue;
			base.SetupDefaults();
		}

		protected override void DisposeView(MauiTextBox nativeView)
		{
			nativeView.KeyUp -= OnTextBoxKeyUp;
			nativeView.TextChanged -= OnTextTextBoxChanged;
			base.DisposeView(nativeView);
		}

		public static void MapPropertyColor(IViewRenderer renderer, ITextInput entry) => (renderer as SearchRenderer)?.UpdateTextColor();
		public static void MapPropertyPlaceholder(IViewRenderer renderer, ITextInput entry) => (renderer as SearchRenderer)?.UpdatePlaceholder();
		public static void MapPropertyPlaceholderColor(IViewRenderer renderer, ITextInput entry) => (renderer as SearchRenderer)?.UpdatePlaceholderColor();
		public static void MapPropertyText(IViewRenderer renderer, ITextInput entry) => (renderer as SearchRenderer)?.UpdateText();
		public static void MapPropertyCancelColor(IViewRenderer renderer, ISearch search) { }
		public static void MapPropertyMaxLength(IViewRenderer renderer, ITextInput view) { }
		public static void MapPropertyBackgroundColor(IViewRenderer renderer, IView view) => ViewRenderer.MapBackgroundColor(renderer, view);

		public virtual void UpdatePlaceholder()
		{
			TypedNativeView.PlaceholderText = VirtualView.Placeholder ?? DefaultPlaceholder;
		}

		public virtual void UpdatePlaceholderColor()
		{
			Color placeholderColor = VirtualView.PlaceholderColor;

			if (placeholderColor.IsDefault)
			{
				TypedNativeView.PlaceholderForegroundBrush = _defaultPlaceholderColorBrush;
				return;
			}

			TypedNativeView.PlaceholderForegroundBrush = placeholderColor.ToBrush();
		}

		public virtual void UpdateText()
		{
			TypedNativeView.Text = VirtualView.Text ?? "";
		}

		public virtual void UpdateTextColor()
		{
			Color textColor = VirtualView.Color;

			if (textColor.IsDefault)
			{
				if (_defaultTextColorBrush == null)
					return;

				TypedNativeView.Foreground = _defaultTextColorBrush;
			}

			if (_defaultTextColorBrush == null)
				_defaultTextColorBrush = TypedNativeView.Foreground;

			TypedNativeView.Foreground = textColor.ToBrush();
		}

		void OnTextBoxKeyUp(object sender, KeyEventArgs keyEventArgs)
		{
			if (keyEventArgs.Key == Key.Enter)
				VirtualView.Search();
		}

		void OnTextTextBoxChanged(object sender, System.Windows.Controls.TextChangedEventArgs textChangedEventArgs)
		{
			VirtualView.Text = TypedNativeView.Text;
		}

	}
}
