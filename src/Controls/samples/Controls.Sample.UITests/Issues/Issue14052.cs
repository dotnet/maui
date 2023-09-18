using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 14052, "[Android] Multiple Images Loaded From Stream Don't Crash", PlatformAffected.Android)]
	public partial class Issue14052 : TestContentPage
	{

		const string coffeeBase64 = "iVBORw0KGgoAAAANSUhEUgAAADAAAAA4CAYAAAC7UXvqAAAABmJLR0QA/wD/AP+gvaeTAAAACXBIWXMAAAsTAAALEwEAmpwYAAAAB3RJTUUH3gUJADAhwicxqAAAAB1pVFh0Q29tbWVudAAAAAAAQ3JlYXRlZCB3aXRoIEdJTVBkLmUHAAABTklEQVRo3u2ZvUoDQRSFv2g6FZ/A0p/G9PZpJCCWFr6BoAGfxsY3sTZgbAKRYCtBCwkh6aIQmyFGEJPZnd25Y86B6XaX882cvXOHAemHpm6UprXUZ2zlAPJGJHjEFCEBCEAAAlhtAEmSpLiqzLW5SfpPfh+oJpKQ3w5GaiUEIAABCEAAAhDAf++FpsuuwCT1CI0Nent13ehfYwbQNwjQ91mBnkGAJx+AlkGAts/DNb6vf6yMA1/iriHznSwb2a2h+NxkeWkTeDcw+2/ARlbypgGAi7ytRTui+XtgPW/+9oFRBPNDYDfUT3QCfJZofgI0QleC85IgPoCzosrZqWv0ijI/Ao6Lrsl7wGMB5h/ct0s7+FwBgwDGB8BlrMPUNnDtOkVf4123z2yFNFTJ8e4hUAeOXBR25syNgRfg2dX2O5/+JguA1RuahROsm/rY+gI8XGfJmDMlSQAAAABJRU5ErkJggg==";

		protected override void Init()
		{
			var converter = new ByteArrayToImageSourceConverter();
			var collectionView = new CollectionView();

			collectionView.ItemTemplate = new DataTemplate(() =>
			{
				var image = new Image()
				{
					HeightRequest = 50,
					WidthRequest = 50
				};

				image.SetBinding(Image.SourceProperty, ".", converter: converter);

				return
					new HorizontalStackLayout()
					{
						new Label()
						{
							Text = "Text"
						},
						new Border()
						{
							Content = image
						}
					};
			});


			collectionView.ItemsSource =
				Enumerable.Range(0, 70)
				.Select(x =>
				{
					return Convert.FromBase64String(coffeeBase64);
				}).ToList();

			Content = collectionView;
		}

		public class ByteArrayToImageSourceConverter : IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				return ImageSource.FromStream(() => new MemoryStream((byte[])value));
			}

			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			{
				throw new NotImplementedException();
			}
		}
	}
}