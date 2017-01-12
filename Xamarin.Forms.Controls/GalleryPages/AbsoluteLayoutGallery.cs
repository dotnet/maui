using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls
{
	[Preserve (AllMembers = true)]
	internal class AbsolutePositioningExplorationViewModel : INotifyPropertyChanged
	{
		double _rectangleX = 0.5;
		double _rectangleY = 0.5;
		double _rectangleWidth = 0.5;
		double _rectangleHeight = 0.5;

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged ([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
				handler (this, new PropertyChangedEventArgs (propertyName));
		}

		public double RectangleX
		{
			get { return _rectangleX; }
			set
			{
				if (_rectangleX == value)
					return;
				_rectangleX = value;
				OnPropertyChanged ();
				OnPropertyChanged ("Rectangle");
			}
		}

		public double RectangleY
		{
			get { return _rectangleY; }
			set
			{
				if (_rectangleY == value)
					return;
				_rectangleY = value;
				OnPropertyChanged ();
				OnPropertyChanged ("Rectangle");
			}
		}

		public double RectangleWidth
		{
			get { return _rectangleWidth; }
			set
			{
				if (_rectangleWidth == value)
					return;
				_rectangleWidth = value;
				OnPropertyChanged ();
				OnPropertyChanged ("Rectangle");
			}
		}

		public double RectangleHeight
		{
			get { return _rectangleHeight; }
			set
			{
				if (_rectangleHeight == value)
					return;
				_rectangleHeight = value;
				OnPropertyChanged ();
				OnPropertyChanged ("Rectangle");
			}
		}

		public Rectangle Rectangle
		{
			get { return new Rectangle(RectangleX, RectangleY, RectangleWidth, RectangleHeight); }
		}
	}
	public class AbsoluteLayoutGallery : ContentPage
	{
		public AbsoluteLayoutGallery ()
		{
			if (Device.RuntimePlatform == Device.iOS && Device.Idiom == TargetIdiom.Tablet)
				Padding = new Thickness(0, 0, 0, 60);

			BindingContext = new AbsolutePositioningExplorationViewModel ();
			var absLayout = new AbsoluteLayout {
				BackgroundColor = Color.Gray,
				VerticalOptions = LayoutOptions.FillAndExpand
			};

			var rect = new BoxView {Color = Color.Lime};

			var xSlider = new Slider ();
			var ySlider = new Slider ();
			var widthSlider = new Slider ();
			var heightSlider = new Slider ();

			var grid = new Grid {
				Padding = 10,
				RowSpacing = 0,
				ColumnDefinitions = {
					new ColumnDefinition {Width = GridLength.Auto},
					new ColumnDefinition {Width = new GridLength (1, GridUnitType.Star)}
				}
			};

			grid.Children.Add (new Label {Text = "X:", VerticalTextAlignment = TextAlignment.Center}, 0, 0);
			grid.Children.Add (xSlider, 1, 0);
			
			grid.Children.Add (new Label {Text = "Y:", VerticalTextAlignment = TextAlignment.Center}, 0, 1);
			grid.Children.Add (ySlider, 1, 1);
			
			grid.Children.Add (new Label {Text = "Width:", VerticalTextAlignment = TextAlignment.Center}, 0, 2);
			grid.Children.Add (widthSlider, 1, 2);

			grid.Children.Add (new Label {Text = "Height:", VerticalTextAlignment = TextAlignment.Center}, 0, 3);
			grid.Children.Add (heightSlider, 1, 3);

			absLayout.Children.Add (rect);

			var mainLayout = new StackLayout {
				Spacing = 0,
				Children = {
					absLayout,
					grid
				}
			};

			rect.SetBinding (AbsoluteLayout.LayoutBoundsProperty, "Rectangle");
			AbsoluteLayout.SetLayoutFlags (rect, AbsoluteLayoutFlags.All);

			xSlider.SetBinding (Slider.ValueProperty, new Binding ("RectangleX", BindingMode.TwoWay));
			ySlider.SetBinding (Slider.ValueProperty, new Binding ("RectangleY", BindingMode.TwoWay));
			widthSlider.SetBinding (Slider.ValueProperty, new Binding ("RectangleWidth", BindingMode.TwoWay));
			heightSlider.SetBinding (Slider.ValueProperty, new Binding ("RectangleHeight", BindingMode.TwoWay));

			//Content = new ScrollView {Content = mainLayout};
			Content = mainLayout;
		}
	}
}
