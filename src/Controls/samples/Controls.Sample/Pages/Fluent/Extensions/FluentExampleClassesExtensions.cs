#nullable enable

using System;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Pages.Fluent
{
	public static class BindablePropertyExtension
	{
		public static Setter Set(this BindableProperty property, object value)
		{
			return new Setter { Property = property, Value = value };
		}

		public static Setter Set(this BindableProperty property)
		{
			return new Setter { Property = property };
		}
	}

	public static partial class LabelExtension
	{
		public static T Text<T>(this T obj, string text)
			where T : Label
		{
			obj.Text = text;
			return obj;
		}

		public static T FontSize<T>(this T obj, double fontSize)
			where T : Label
		{
			obj.FontSize = fontSize;
			return obj;
		}

		public static T TextColor<T>(this T obj, Color textColor)
			where T : Label
		{
			obj.TextColor = textColor;
			return obj;
		}
	}

	public static class ButtonExtension
	{
		public static T Text<T>(this T obj, string text)
			where T : Button
		{
			obj.Text = text;
			return obj;
		}

		public static T FontSize<T>(this T obj, double fontSize)
			where T : Button
		{
			obj.FontSize = fontSize;
			return obj;
		}

		public static T OnClicked<T>(this T obj, EventHandler handler)
			where T : Button
		{
			obj.Clicked += handler;
			return obj;
		}

		public static T OnClicked<T>(this T obj, Action<T> action)
			where T : Button
		{
			obj.Clicked += (o, arg) => action(obj);
			return obj;
		}
	}

	public static class ViewExtension
	{
		public static T Margin<T>(this T obj, Thickness margin)
			where T : View
		{
			obj.Margin = margin;
			return obj;
		}

		public static T HorizontalOptions<T>(this T obj, LayoutOptions horizontalOptions)
			where T : View
		{
			obj.HorizontalOptions = horizontalOptions;
			return obj;
		}

		public static T VerticalOptions<T>(this T obj, LayoutOptions verticalOptions)
			where T : View
		{
			obj.VerticalOptions = verticalOptions;
			return obj;
		}

		public static T Column<T>(this T obj, int column)
			where T : View
		{
			obj.SetValue(Grid.ColumnProperty, column);
			return obj;
		}

		public static T Row<T>(this T obj, int row)
			where T : View
		{
			obj.SetValue(Grid.RowProperty, row);
			return obj;
		}

		public static T ColumnSpan<T>(this T obj, int columnSpan)
			where T : View
		{
			obj.SetValue(Grid.ColumnSpanProperty, columnSpan);
			return obj;
		}

		public static T RowSpan<T>(this T obj, int rowSpan)
			where T : View
		{
			obj.SetValue(Grid.RowSpanProperty, rowSpan);
			return obj;
		}

		public static T GridSpan<T>(this T obj, int column = 1, int row = 1)
			where T : View
		{
			obj.SetValue(Grid.ColumnSpanProperty, column);
			obj.SetValue(Grid.RowSpanProperty, row);
			return obj;
		}
	}

	public static class ImageExtension
	{
		public static T Source<T>(this T obj, ImageSource source)
			where T : Image
		{
			obj.Source = source;
			return obj;
		}
	}

	public static class GridExtension
	{
		public static Grid RowDefinitions(this Grid obj, System.Func<RowDefinitionBuilder, RowDefinitionBuilder> build)
		{
			var rowDefs = build(new RowDefinitionBuilder());
			foreach (var rowDef in rowDefs)
				obj.RowDefinitions.Add(rowDef);
			return obj;
		}

		public static Grid ColumnDefinitions(this Grid obj, System.Func<ColumnDefinitionBuilder, ColumnDefinitionBuilder> build)
		{
			var colDefs = build(new ColumnDefinitionBuilder());
			foreach (var colDef in colDefs)
				obj.ColumnDefinitions.Add(colDef);
			return obj;
		}
	}

	public static class VisualElementExtension
	{
		public static T SizeRequest<T>(this T obj, double widthRequest, double heightRequest)
			where T : VisualElement
		{
			obj.WidthRequest = widthRequest;
			obj.HeightRequest = heightRequest;
			return obj;
		}

		public static T BackgroundColor<T>(this T obj,
			Color backgroundColor)
			where T : VisualElement
		{
			obj.BackgroundColor = backgroundColor;
			return obj;
		}

		public static T WidthRequest<T>(this T obj, double widthRequest)
			where T : VisualElement
		{
			obj.WidthRequest = widthRequest;
			return obj;
		}

		public static T HeightRequest<T>(this T obj, double heightRequest)
			where T : VisualElement
		{
			obj.HeightRequest = heightRequest;
			return obj;
		}
	}

	public static partial class BorderExtension
	{
		public static T StrokeShape<T>(this T obj, IShape? strokeShape)
			where T : Border
		{
			obj.StrokeShape = strokeShape;
			return obj;
		}
	}

	public static class RoundRectangleExtension
	{
		public static RoundRectangle CornerRadius(this RoundRectangle obj, CornerRadius cornerRadius)
		{
			obj.CornerRadius = cornerRadius;
			return obj;
		}
	}

	public static class ShapeExtension
	{
		public static T Fill<T>(this T obj, Brush fill)
			where T : Shape
		{
			obj.Fill = fill;
			return obj;
		}

		public static T Stroke<T>(this T obj, Brush stroke)
			where T : Shape
		{
			obj.Stroke = stroke;
			return obj;
		}

		public static T StrokeThickness<T>(this T obj, double strokeThickness)
			where T : Shape
		{
			obj.StrokeThickness = strokeThickness;
			return obj;
		}
	}

	public static partial class PathFigureExtension
	{
		public static PathFigure StartPoint(this PathFigure obj, Point startPoint)
		{
			obj.StartPoint = startPoint;
			return obj;
		}
	}
}

#nullable restore