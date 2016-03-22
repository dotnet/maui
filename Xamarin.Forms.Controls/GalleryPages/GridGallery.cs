using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{

	public class GridGallery : ContentPage
	{
		public GridGallery ()
		{
			var layout = new StackLayout { 
				Orientation = StackOrientation.Vertical
			};

			//ColumnTypes
			layout.Children.Add (new Label { Text = "Column Types:" });
			var grid = new Grid { 
				ColumnDefinitions = {
					new ColumnDefinition { Width = 80 },
					new ColumnDefinition (),
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) }
				},
				BackgroundColor = Color.FromRgb (0xee, 0xee, 0xee)
			};

			grid.Children.Add (new Label { 
				Text = "Absolute Width",
				BackgroundColor = Color.FromRgb (0xcc, 0xcc, 0xcc),
			}, 0, 0);
			grid.Children.Add (new Label { 
				Text = "Auto Width",
				BackgroundColor = Color.FromRgb (0xcc, 0xcc, 0xcc),
			}, 1, 0);
			grid.Children.Add (new Label { 
				Text = "Star",
				BackgroundColor = Color.FromRgb (0xcc, 0xcc, 0xcc),
			}, 2, 0);
			layout.Children.Add (grid);

			//Star
			layout.Children.Add (new Label { Text = "Star Columns:" });
			grid = new Grid { 
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (2, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (3, GridUnitType.Star) },
				},
				BackgroundColor = Color.FromRgb (0xee, 0xee, 0xee),
				ColumnSpacing = 0,
				RowSpacing = 0,
			};

			grid.Children.Add (new Label { 
				Text = "*",
				BackgroundColor = Color.FromRgb (0xcc, 0xcc, 0xcc),
			}, 0, 0);
			grid.Children.Add (new Label { 
				Text = "**",
				BackgroundColor = Color.FromRgb (0xcc, 0xcc, 0xcc),
			}, 1, 0);
			grid.Children.Add (new Label { 
				Text = "***",
				BackgroundColor = Color.FromRgb (0xcc, 0xcc, 0xcc),
			}, 2, 0);
			layout.Children.Add (grid);

			//Alignment
			layout.Children.Add (new Label { Text = "Alignment:" });
			grid = new Grid { 
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
				},
				BackgroundColor = Color.FromRgb (0xee, 0xee, 0xee)
			};

			grid.Children.Add (new Label { 
				Text = "Right",
				BackgroundColor = Color.FromRgb (0xcc, 0xcc, 0xcc),
				HorizontalOptions = LayoutOptions.End,
			}, 0, 0);
			grid.Children.Add (new Label { 
				Text = "Center",
				BackgroundColor = Color.FromRgb (0xcc, 0xcc, 0xcc),
				HorizontalOptions = LayoutOptions.Center,

			}, 1, 0);
			grid.Children.Add (new Label { 
				Text = "Left",
				BackgroundColor = Color.FromRgb (0xcc, 0xcc, 0xcc),
				HorizontalOptions = LayoutOptions.Start,
			}, 2, 0);
			grid.Children.Add (new Label { 
				Text = "Fill",
				BackgroundColor = Color.FromRgb (0xcc, 0xcc, 0xcc),
				HorizontalOptions = LayoutOptions.Fill,
			}, 3, 0);
			layout.Children.Add (grid);

			//Spanning
			layout.Children.Add (new Label { Text = "Spans:" });
			grid = new Grid { 
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
				},
				BackgroundColor = Color.FromRgb (0xee, 0xee, 0xee),
				ColumnSpacing = 0,
				RowSpacing = 0
			};

			for (var i=0;i<5;i++)
				for (var j=0;j<5;j++)
					grid.Children.Add (new Label { 
						Text = "Unit",
						BackgroundColor = Color.FromRgb (0xcc, 0xcc, 0xcc),
					}, i, j);


			grid.Children.Add (new Label { 
				Text = "Spanning 4 columns",
				BackgroundColor = Color.Red,
			}, 0,4,0,1);
			grid.Children.Add (new Label { 
				Text = "Spanning 3 rows",
				BackgroundColor = Color.Gray,
			}, 4,5, 0,3);
			grid.Children.Add (new Label { 
				Text = "a block 3x3",
				BackgroundColor = Color.Green,
			}, 1,4,1,4);
			layout.Children.Add (grid);


			//Change Width
			var col0 = new ColumnDefinition { Width = 40 };
			var col1 = new ColumnDefinition { Width = 80 };

			grid = new Grid {
				ColumnDefinitions = new ColumnDefinitionCollection {
					col0, col1
				}
			};

			grid.Children.Add (new BoxView { BackgroundColor = Color.Red });
			grid.Children.Add (new BoxView { BackgroundColor = Color.Blue },1,0);

			layout.Children.Add (grid);
			layout.Children.Add (new Button { 
				Text = "ChangeWidth",
				Command = new Command (()=>{
					var t = col0.Width;
					col0.Width = col1.Width;
					col1.Width = t;
				})
			});

			Content = new ScrollView {
				Content = layout,
			};



		}
	}
}
