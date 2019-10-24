using System;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Controls;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.WinPhone
{
	internal class NavigationMenuRenderer : ViewRenderer<NavigationMenu, System.Windows.Controls.Grid>
	{
		const int Spacing = 12;

		protected override void OnElementChanged(ElementChangedEventArgs<NavigationMenu> e)
		{
			base.OnElementChanged(e);

			var grid = new System.Windows.Controls.Grid();
			grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = System.Windows.GridLength.Auto });
			grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = System.Windows.GridLength.Auto });

			UpdateItems(grid);
			SetNativeControl(grid);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			switch (e.PropertyName)
			{
				case "Targets":
					UpdateItems(Control);
					break;
			}
		}

		TileSize GetSize()
		{
			return RenderSize.Width >= 210 * 2 + Spacing ? TileSize.Medium : TileSize.Default;
		}

		void UpdateItems(System.Windows.Controls.Grid grid)
		{
			grid.Children.Clear();

			grid.RowDefinitions.Clear();

			var x = 0;
			var y = 0;
			foreach (Page target in Element.Targets)
			{
				if (x > 1)
				{
					x = 0;
					y++;
				}

				if (x == 0)
					grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition());

				var hubTile = new HubTile { Title = target.Title, Source = new BitmapImage(new Uri(target.Icon, UriKind.Relative)), Margin = new System.Windows.Thickness(0, 0, Spacing, Spacing) };

				if (target.BackgroundColor != Color.Default)
					hubTile.Background = target.BackgroundColor.ToBrush();

				Page tmp = target;
				hubTile.Tap += (sender, args) => Element.SendTargetSelected(tmp);

				hubTile.SetValue(System.Windows.Controls.Grid.RowProperty, y);
				hubTile.SetValue(System.Windows.Controls.Grid.ColumnProperty, x);
				hubTile.Size = GetSize();

				var weakRef = new WeakReference(hubTile);
				SizeChanged += (sender, args) =>
				{
					var hTile = (HubTile)weakRef.Target;
					if (hTile != null)
						hTile.Size = GetSize();
					((IVisualElementController)Element).NativeSizeChanged();
				};

				x++;
				grid.Children.Add(hubTile);
			}
		}
	}
}