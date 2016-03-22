using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls.GalleryPages
{
	public class LayoutPerformanceGallery : ContentPage
	{
		public LayoutPerformanceGallery ()
		{
			int size = 3;

			var grid = new Grid {
				VerticalOptions = LayoutOptions.FillAndExpand,
				RowDefinitions = {
					new RowDefinition { Height = new GridLength (100, GridUnitType.Absolute) },
					new RowDefinition { Height = new GridLength (5, GridUnitType.Star) },
					new RowDefinition { Height = new GridLength (7, GridUnitType.Star) },
				},
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (3, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength (100, GridUnitType.Absolute) },
					new ColumnDefinition { Width = new GridLength (7, GridUnitType.Star) },
				}
			};

			for (int i = 0; i < size; i++) {
				for (int j = 0; j < size; j++) {
					var g = new Grid {
						RowDefinitions = {
							new RowDefinition { Height = new GridLength (100, GridUnitType.Absolute) },
							new RowDefinition { Height = new GridLength (5, GridUnitType.Star) },
							new RowDefinition { Height = new GridLength (7, GridUnitType.Star) },
						},
						ColumnDefinitions = {
							new ColumnDefinition { Width = new GridLength (3, GridUnitType.Star) },
							new ColumnDefinition { Width = new GridLength (100, GridUnitType.Absolute) },
							new ColumnDefinition { Width = new GridLength (7, GridUnitType.Star) },
						}
					};

					for (int k = 0; k < size; k++) {
						for (int l = 0; l < size; l++) {
							var label = new Label { Text = "10" };
							g.Children.Add (label, k, l);
							_labelList.Add (label);
						}
					}

					grid.Children.Add (g, i, j);
				}
			}




			Content = new StackLayout {
				Children = {
					(_mainLabel = new Label ()),
					grid
				}
			};
		}

		Label _mainLabel = null;
		List<Label> _labelList = new List<Label> ();
		Stopwatch _sw = new Stopwatch ();
		long _ticks = 0;
		int _count = 0;
		int _cycle = 0;
		bool _flip = true;
		Random _r = new Random (34269027);
		bool _repeat = false;

		protected override void OnAppearing ()
		{
			base.OnAppearing ();
			_repeat = true;
			Device.StartTimer (TimeSpan.FromMilliseconds (10), () => {
				_sw.Start ();
				foreach (var label in _labelList) {
					if (_flip) {
						label.Text = _r.Next (10, 40).ToString ();
					} else {
						label.Text = _r.Next (50, 90).ToString ();
					}
				}
				_sw.Stop ();
				_ticks += _sw.ElapsedMilliseconds;
				_sw.Reset ();

				_cycle = (_cycle + 1) % 100;
				_count++;
				if (_cycle == 0) {
					_mainLabel.Text = string.Format ("Avg Time: {0:0.000}ms", _ticks / (double)_count);
				}

				_flip = !_flip;
				return _repeat;
			});
		}

		protected override void OnDisappearing ()
		{
			_repeat = false;
			_ticks = 0;
			_count = 0;
			_cycle = 0;

			base.OnDisappearing ();
		}
	}
}
