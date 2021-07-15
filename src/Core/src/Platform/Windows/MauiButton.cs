#nullable enable
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public class MauiButton : Button
	{
		TextBlock? _textBlock;

		public MauiButton()
		{
			RegisterPropertyChangedCallback(CharacterSpacingProperty, OnCharacterSpacingChanged);
			RegisterPropertyChangedCallback(ContentProperty, OnContentChanged);

			Loaded += OnLoaded;
		}

		static void OnCharacterSpacingChanged(DependencyObject sender, DependencyProperty dp)
		{
			if (sender is MauiButton btn)
				btn.UpdateCharacterSpacing();
		}

		static void OnContentChanged(DependencyObject sender, DependencyProperty dp)
		{
			if (sender is MauiButton btn)
			{
				btn._textBlock = null;
				btn.UpdateCharacterSpacing();
			}
		}

		void OnLoaded(object sender, RoutedEventArgs e)
		{
			UpdateCharacterSpacing();
		}

		void UpdateCharacterSpacing()
		{
			_textBlock ??= this.GetChild<TextBlock>();

			if (_textBlock != null)
				_textBlock.CharacterSpacing = CharacterSpacing;
		}
	}
}