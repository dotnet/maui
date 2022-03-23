using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Platform
{
	public class ImageConverter : Microsoft.UI.Xaml.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			ImageSource source = value as ImageSource;
			if (source == null)
			{
				return null;
			}

			IMauiContext context = source.FindMauiContext(true);
			IImageSourceServiceProvider imageSourceServiceProvider = context.Services.GetRequiredService<IImageSourceServiceProvider>();
			IImageSourceService imageSourceService = imageSourceServiceProvider.GetImageSourceService(source);
			return imageSourceService.GetImageSourceAsync(source).Result;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotSupportedException();
		}
	}
}