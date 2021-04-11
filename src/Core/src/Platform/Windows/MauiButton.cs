using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WContentPresenter = Microsoft.UI.Xaml.Controls.ContentPresenter;

namespace Microsoft.Maui
{
	// TODO: CharacterSpacing in Button is not working in WinUI
	// See: https://github.com/microsoft/microsoft-ui-xaml/issues/3490
	public class MauiButton : Button
	{
		public static readonly DependencyProperty LineBreakModeProperty = DependencyProperty.Register(nameof(LineBreakMode),
			typeof(LineBreakMode), typeof(MauiButton), new PropertyMetadata(LineBreakMode.NoWrap, LineBreakModeChanged));

		public LineBreakMode LineBreakMode
		{
			get
			{
				return (LineBreakMode)GetValue(LineBreakModeProperty);
			}
			set
			{
				SetValue(LineBreakModeProperty, value);
			}
		}

		Grid? _rootGrid;

		public MauiButton()
		{
			RegisterPropertyChangedCallback(CharacterSpacingProperty, new DependencyPropertyChangedCallback((obj, dp) => { UpdateCharacterSpacing(); }));
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (_rootGrid == null)
				_rootGrid = VisualTreeHelper.GetChild(this, 0) as Grid;

			UpdateCharacterSpacing();
			UpdateLineBreakMode();
		}

		// Doesn't work at the moment. Left here for future potential fixes regarding WinUI bug.
		public void UpdateCharacterSpacing()
		{
			var contentTextBlock = GetTextBlock();

			if (contentTextBlock != null)
				contentTextBlock.CharacterSpacing = CharacterSpacing;
		}

		static void LineBreakModeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{
			var button = (MauiButton)dependencyObject;
			button.UpdateLineBreakMode();
		}

		public void UpdateLineBreakMode()
		{
			var contentTextBlock = GetTextBlock();

			if (contentTextBlock != null)
				contentTextBlock.UpdateLineBreakMode(LineBreakMode);
		}

		/// <summary>
		/// Returns the TextBlock object inside the base MUXC Button.
		/// </summary>
		public TextBlock? GetTextBlock()
		{
			if (_rootGrid != null)
				return _rootGrid.Children[0] as TextBlock;

			return null;
		}
	}
}
