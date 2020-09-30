using System;
using System.Collections.Generic;
using System.Globalization;
using ElmSharp;

namespace Xamarin.Forms.Platform.Tizen
{
	public class EntryCellRenderer : ViewCellRenderer
	{
		readonly Dictionary<EvasObject, VisualElement> _cacheCandidate = new Dictionary<EvasObject, VisualElement>();

		public EntryCellRenderer()
		{
		}

		protected override EvasObject OnGetContent(Cell cell, string part)
		{
			if (part == MainContentPart)
			{
				var entryCell = cell as EntryCell;
				int pixelHeight = Forms.ConvertToScaledPixel(entryCell.RenderHeight);
				pixelHeight = pixelHeight > 0 ? pixelHeight : this.GetDefaultHeightPixel();

				var label = new Label()
				{
					HorizontalOptions = LayoutOptions.Start,
					VerticalOptions = LayoutOptions.Center,
					VerticalTextAlignment = TextAlignment.Center,
					FontSize = -1
				};
				label.SetBinding(Label.TextProperty, new Binding(EntryCell.LabelProperty.PropertyName));
				label.SetBinding(Label.TextColorProperty, new Binding(EntryCell.LabelColorProperty.PropertyName, converter: new DefaultColorConverter()));

				var entry = new Entry()
				{
					HorizontalOptions = LayoutOptions.FillAndExpand,
					VerticalOptions = LayoutOptions.Center,
					FontSize = -1,
				};
				entry.SetBinding(Entry.TextProperty, new Binding(EntryCell.TextProperty.PropertyName, BindingMode.TwoWay));
				entry.SetBinding(Entry.PlaceholderProperty, new Binding(EntryCell.PlaceholderProperty.PropertyName));
				entry.SetBinding(InputView.KeyboardProperty, new Binding(EntryCell.KeyboardProperty.PropertyName));
				entry.SetBinding(Entry.HorizontalTextAlignmentProperty, new Binding(EntryCell.HorizontalTextAlignmentProperty.PropertyName));

				var layout = new StackLayout()
				{
					Orientation = StackOrientation.Horizontal,
					Children = {
						label,
						entry
					}
				};
				layout.Parent = cell;
				layout.BindingContext = entryCell;
				layout.MinimumHeightRequest = Forms.ConvertToScaledDP(pixelHeight);

				var renderer = Platform.GetOrCreateRenderer(layout);
				(renderer as LayoutRenderer)?.RegisterOnLayoutUpdated();

				var nativeEntry = Platform.GetRenderer(entry)?.NativeView ?? null;
				if (nativeEntry != null)
				{
					nativeEntry.PropagateEvents = false;
				}

				var nativeView = renderer.NativeView;
				nativeView.MinimumHeight = pixelHeight;
				_cacheCandidate[nativeView] = layout;
				nativeView.Deleted += (sender, e) =>
				{
					_cacheCandidate.Remove(sender as EvasObject);
				};

				return nativeView;
			}
			return null;
		}

		protected override EvasObject OnReusableContent(Cell cell, string part, EvasObject old)
		{
			if (!_cacheCandidate.ContainsKey(old))
			{
				return null;
			}

			var layout = _cacheCandidate[old];
			layout.BindingContext = cell;
			int height = Forms.ConvertToScaledPixel(cell.RenderHeight);
			height = height > 0 ? height : this.GetDefaultHeightPixel();
			old.MinimumHeight = height;
			return old;
		}

		class DefaultColorConverter : IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				return ((Color)value).IsDefault ? ThemeConstants.EntryCell.ColorClass.DefaultLabelColor : value;
			}

			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			{
				return value;
			}
		}
	}
}
