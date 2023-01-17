using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages.Fluent
{
	// --- single item content ---

	public static partial class BorderExtension
	{
		public static T Add<T>(this T element, View content)
			where T : Border
		{
			element.Content = content;
			return element;
		}
	}

	public static class ContentPageExtension
	{
		public static T Add<T>(this T element, View content)
			where T : ContentPage
		{
			element.Content = content;
			return element;
		}
	}

	public static class ContentViewExtension
	{
		public static T Add<T>(this T element, View content)
			where T : ContentView
		{
			element.Content = content;
			return element;
		}
	}

	public static class FlyoutPageExtension
	{
		public static T Add<T>(this T element, Page detail)
			where T : FlyoutPage
		{
			element.Detail = detail;
			return element;
		}
	}

	public static class IndicatorViewExtension
	{
		public static T Add<T>(this T element, IBindableLayout indicatorLayout)
			where T : IndicatorView
		{
			element.IndicatorLayout = indicatorLayout;
			return element;
		}
	}

	public static partial class LabelExtension
	{
		public static T Add<T>(this T element, string text)
			where T : Label
		{
			element.Text = text;
			return element;
		}
	}

	public static class OnExtension
	{
		public static T Add<T>(this T element, object value)
			where T : On
		{
			element.Value = value;
			return element;
		}
	}

	public static class RadioButtonExtension
	{
		public static T Add<T>(this T element, object content)
			where T : RadioButton
		{
			element.Content = content;
			return element;
		}
	}

	public static class ScrollViewExtension
	{
		public static T Add<T>(this T element, View content)
			where T : ScrollView
		{
			element.Content = content;
			return element;
		}
	}

	public static class SetterExtension
	{
		public static Setter Add(this Setter element, object value)
		{
			element.Value = value;
			return element;
		}
	}

	public static class PathExtension
	{
		public static Path Add(this Path element, Geometry data)
		{
			element.Data = data;
			return element;
		}
	}

	public static class ShellContentExtension
	{
		public static T Add<T>(this T element, object content)
			where T : ShellContent
		{
			element.Content = content;
			return element;
		}
	}

	public static class SpanExtension
	{
		public static T Add<T>(this T element, string text)
			where T : Span
		{
			element.Text = text;
			return element;
		}
	}

	public static class ViewCellExtension
	{
		public static T Add<T>(this T element, View view)
			where T : ViewCell
		{
			element.View = view;
			return element;
		}
	}

	public static class WindowExtension
	{
		public static T Add<T>(this T element, Page page)
			where T : Window
		{
			element.Page = page;
			return element;
		}
	}

	// --- multi item content ---

	public static class DataTriggerExtension
	{
		public static DataTrigger Add(this DataTrigger element, Setter setter)
		{
			element.Setters.Add(setter);
			return element;
		}
	}

	public static class EventTriggerExtension
	{
		public static EventTrigger Add(this EventTrigger element, TriggerAction triggerAction)
		{
			element.Actions.Add(triggerAction);
			return element;
		}
	}

	public static class FormattedStringExtension
	{
		public static FormattedString Add(this FormattedString element, Span span)
		{
			element.Spans.Add(span);
			return element;
		}
	}

	public static class GradientBrushExtension
	{
		public static GradientBrush Add(this GradientBrush element, GradientStop gradientStop)
		{
			element.GradientStops.Add(gradientStop);
			return element;
		}
	}

	public static class MultiBindingExtension
	{
		public static MultiBinding Add(this MultiBinding element, BindingBase bindingBase)
		{
			element.Bindings.Add(bindingBase);
			return element;
		}
	}

	public static class MultiPageExtension
	{
		public static TMultiPage Add<TMultiPage, T>(this TMultiPage element, T page)
			where TMultiPage : MultiPage<T>
			where T : Page 
		{
			element.Children.Add(page);
			return element;
		}
	}

	public static class MultiTriggerExtension
	{
		public static MultiTrigger Add(this MultiTrigger element, Setter setter)
		{
			element.Setters.Add(setter);
			return element;
		}
	}

	public static class OnPlatformExtension
	{
		public static OnPlatform<T> Add<T>(this OnPlatform<T> element, On on)
		{
			element.Platforms.Add(on);
			return element;
		}
	}

	public static class GeometryGroupExtension
	{
		public static GeometryGroup Add(this GeometryGroup element, Geometry geometry)
		{
			element.Children.Add(geometry);
			return element;
		}
	}

	public static partial class PathFigureExtension
	{
		public static PathFigure Add(this PathFigure element, PathSegment pathSegment)
		{
			element.Segments.Add(pathSegment);
			return element;
		}
	}

	public static class PathGeometryExtension
	{
		public static PathGeometry Add(this PathGeometry element, PathFigure pathFigure)
		{
			element.Figures.Add(pathFigure);
			return element;
		}
	}

	public static class PolygonExtension
	{
		public static Polygon Add(this Polygon element, Point point)
		{
			element.Points.Add(point);
			return element;
		}
	}

	public static class PolylineExtension
	{
		public static Polyline Add(this Polyline element, Point point)
		{
			element.Points.Add(point);
			return element;
		}
	}

	public static class TransformGroupExtension
	{
		public static TransformGroup Add(this TransformGroup element, Transform transform)
		{
			element.Children.Add(transform);
			return element;
		}
	}

	public static class ShellExtension
	{
		public static Shell Add(this Shell element, ShellItem shellItem)
		{
			element.Items.Add(shellItem);
			return element;
		}
	}

	public static class ShellItemExtension
	{
		public static ShellItem Add(this ShellItem element, ShellSection shellSection)
		{
			element.Items.Add(shellSection);
			return element;
		}
	}

	public static class ShellSectionExtension
	{
		public static ShellSection Add(this ShellSection element, ShellContent shellContent)
		{
			element.Items.Add(shellContent);
			return element;
		}
	}

	public static class StyleExtension
	{
		public static Style Add(this Style element, Setter setter)
		{
			element.Setters.Add(setter);
			return element;
		}
	}

	public static class TableViewExtension
	{
		public static TableView Add(this TableView element, TableSection tableSection)
		{
			element.Root.Add(tableSection);
			return element;
		}
	}

	public static class TriggerExtension
	{
		public static Trigger Add(this Trigger element, Setter setter)
		{
			element.Setters.Add(setter);
			return element;
		}
	}

	public static class VisualStateExtension
	{
		public static VisualState Add(this VisualState element, Setter setter)
		{
			element.Setters.Add(setter);
			return element;
		}
	}

	public static class VisualStateGroupExtension
	{
		public static VisualStateGroup Add(this VisualStateGroup element, VisualState visualState)
		{
			element.States.Add(visualState);
			return element;
		}
	}
}

