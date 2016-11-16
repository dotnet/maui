using System;

using Xamarin.Forms;
using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue(IssueTracker.Github, 2354, "ListView, ImageCell and disabled source cache and same image url",PlatformAffected.iOS | PlatformAffected.Android)]
	public class Issue2354 : TestContentPage
	{
		protected override void Init ()
		{
			var presidents = new List<President> ();
			for (int i = 0; i < 10; i++) {
				presidents.Add (new President ($"Presidente {44 - i}", 1, $"http://static.c-span.org/assets/images/series/americanPresidents/{43 - i}_400.png"));
			}
						
			var header = new Label {
				Text = "Presidents",
				HorizontalOptions = LayoutOptions.Center
			};

			var cell = new DataTemplate (typeof(CustomCell));

			var listView = new ListView {
				ItemsSource = presidents,
				ItemTemplate = cell,
				RowHeight = 200
			};

		
			Content = new StackLayout {
				Children = {
					header,
					listView
				}
			};
		}

		public class President
		{
			public President (string name, int position, string image)
			{
				Name = name;
				Position = position;
				Image = image;
			}

			public string Name { private set; get; }

			public int Position { private set; get; }

			public string Image { private set; get; }
		}


		[Preserve (AllMembers = true)]
		public class CustomCell : ViewCell
		{
			public CustomCell()
			{
				var image = new Image
				{
					HorizontalOptions = LayoutOptions.Start,
					Aspect = Aspect.AspectFill
				};

				var source = new UriImageSource {
					CachingEnabled = false,
				};

				source.SetBinding(UriImageSource.UriProperty, new Binding("Image", converter: new UriConverter()));

				image.Source = source;


				View = image;
			}
		}

		public class UriConverter : IValueConverter
		{

			public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
			{
				return new Uri((string) value);
			}

			public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
			{
				throw new NotImplementedException();
			}

		}

#if UITEST
		[Test]
		public void TestDoesntCrashWithCachingDisable ()
		{
			RunningApp.ScrollDown ();
			RunningApp.ScrollDown ();
		}
#endif

	}

}


