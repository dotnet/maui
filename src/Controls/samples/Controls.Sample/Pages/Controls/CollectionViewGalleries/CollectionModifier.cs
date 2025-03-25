using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages.CollectionViewGalleries
{
	internal abstract class CollectionModifier : ContentView
	{
		protected readonly CollectionView _cv;
		protected readonly Entry Entry;

		protected CollectionModifier(CollectionView cv, string buttonText)
		{
			_cv = cv;

			var layout = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.Fill
			};

			var button = new Button { Text = buttonText, AutomationId = $"btn{buttonText}", HeightRequest = 20, FontSize = 10 };
			var label = new Label { Text = LabelText, VerticalTextAlignment = TextAlignment.Center, FontSize = 10 };

			Entry = new Entry { Keyboard = Keyboard.Numeric, Text = InitialEntryText, WidthRequest = 100, FontSize = 10, AutomationId = $"entry{buttonText}" };

			layout.Children.Add(label);
			layout.Children.Add(Entry);
			layout.Children.Add(button);

			button.Clicked += ButtonOnClicked;

			Content = layout;
		}

		void ButtonOnClicked(object? sender, EventArgs e)
		{
			OnButtonClicked();
		}

		protected virtual string LabelText => "Index:";

		protected virtual string InitialEntryText => "0";

		protected virtual void OnButtonClicked()
		{
		}

		protected virtual bool ParseIndexes(out int[] indexes)
		{
			if (!int.TryParse(Entry.Text, out int index))
			{
				indexes = Array.Empty<int>();
				return false;
			}

			indexes = new[] { index };
			return true;
		}
	}
}