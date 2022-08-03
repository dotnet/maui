#nullable enable

using System;
using System.Globalization;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public class ItemDefaultTemplateAdaptor : ItemTemplateAdaptor
	{
		class ToTextConverter : IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				return value?.ToString() ?? string.Empty;
			}

			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
		}

		public ItemDefaultTemplateAdaptor(ItemsView itemsView) : base(itemsView)
		{
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					TextColor = Graphics.Colors.Black,
				};
				label.SetBinding(Label.TextProperty, new Binding(".", converter: new ToTextConverter()));

				return new StackLayout
				{
					BackgroundColor = Graphics.Colors.White,
					Padding = 30,
					Children =
					{
						label
					}
				};
			});
		}
	}
}