using System;
using System.Globalization;
using System.Windows;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.WinPhone
{
	public class HeightConverter : System.Windows.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var val = (double)value;
			return val > 0 ? val : double.NaN;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	[Obsolete("Deprecated in favor of CellControl")]
	public class CellTemplateSelector : DataTemplateSelector
	{
		public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(object), typeof(CellTemplateSelector),
			new PropertyMetadata((o, e) => ((CellTemplateSelector)o).SetSource(e.OldValue, e.NewValue)));

		public CellTemplateSelector()
		{
			Loaded += (sender, args) => SetBinding(SourceProperty, new System.Windows.Data.Binding());
			Unloaded += (sender, args) =>
			{
				var cell = DataContext as ICellController;
				if (cell != null)
					cell.SendDisappearing();
			};
		}

		public override System.Windows.DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			var cell = item as Cell;
			if (cell == null)
				return null;

			var renderer = Registrar.Registered.GetHandler<ICellRenderer>(cell.GetType());
			return renderer.GetTemplate(cell);
		}

		void SetSource(object oldSource, object newSource)
		{
			var oldCell = oldSource as ICellController;
			var newCell = newSource as ICellController;

			if (oldCell != null)
				oldCell.SendDisappearing();

			if (newCell != null)
				newCell.SendAppearing();
		}
	}
}