using System.Collections.Generic;
using ElmSharp;
using EColor = ElmSharp.Color;

namespace Xamarin.Forms.Platform.Tizen
{
	public class SearchResultList : GenList
	{
		GenItemClass _defaultClass = null;

		public SearchResultList() : base(Forms.NativeParent)
		{
			SetAlignment(-1, -1);
			SetWeight(1, 1);
			Homogeneous = true;
			AllowFocus(true);
			SelectionMode = GenItemSelectionMode.Always;
			BackgroundColor = EColor.White;
			_defaultClass = new GenItemClass(ThemeConstants.GenItemClass.Styles.Full)
			{
				GetContentHandler = GetContent,
			};
		}

		public int Height { get; private set; }

		IReadOnlyList<object> _itemsSource;
		public IReadOnlyList<object> ItemsSource
		{
			get => _itemsSource;
			set
			{
				_itemsSource = value;
				Clear();
				Height = 0;
				foreach (var item in _itemsSource)
				{
					Append(item);
				}
			}
		}

		public void UpdateLayout()
		{
			UpdateRealizedItems();
		}

		public DataTemplate ItemTemplate { get; set; }

		EvasObject GetContent(object data, string part)
		{
			var view = data as View;

			var renderer = Platform.GetOrCreateRenderer(view);
			(renderer as LayoutRenderer)?.RegisterOnLayoutUpdated();
			var measured = view.Measure(Forms.ConvertToScaledDP(Geometry.Width), Forms.ConvertToScaledDP(Geometry.Width * 3));
			renderer.NativeView.MinimumHeight = Forms.ConvertToScaledPixel(measured.Request.Height);
			return renderer.NativeView;
		}

		void Append(object data)
		{
			var view = ItemTemplate.CreateContent() as View;
			view.Parent = Shell.Current;
			view.BindingContext = data;
			var measured = view.Measure(Forms.ConvertToScaledDP(Geometry.Width), Forms.ConvertToScaledDP(Geometry.Width * 3));
			Height += Forms.ConvertToScaledPixel(measured.Request.Height);
			Append(_defaultClass, view);
		}

	}
}
