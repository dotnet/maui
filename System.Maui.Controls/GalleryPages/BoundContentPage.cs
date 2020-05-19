using System;
using System.Globalization;
using System.Windows.Input;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls
{
	[Preserve (AllMembers = true)]
	public class BoundContentPage : ContentPage
	{
        internal class NavWrapperConverter : IValueConverter
		{
			public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
			{
				throw new NotSupportedException ();
			}

			public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
			{
				return new NavigationWrapper ((INavigation) value);
			}
		}

		internal class NavigationWrapper
		{
			// This class is dumb but proves you can wrap the INavigation with a converter to do some MVVM goodness
			// Normally this class would implement pushes with ViewModel then do the conversion
			readonly INavigation _inner;

			public NavigationWrapper (INavigation inner	)
			{
				_inner = inner;
			}

			public void WrappedPush ()
			{
				_inner.PushAsync (new ContentPage {
					Content = new StackLayout {
						BackgroundColor = Color.Red,
						Children = {
							new Label {
								Text = "Second Page"
							}
						}
					}
				});
			}
		}

		[Preserve (AllMembers = true)]
		internal class BoundContentPageViewModel
		{
			public string ButtonText { get { return "Click Me!"; } }

			public ICommand ButtonCommand { get; set; }

			public NavigationWrapper Navigation { get; set; }

			public BoundContentPageViewModel ()
			{
				ButtonCommand = new Command (() => Navigation.WrappedPush ());
			}
		}

		public BoundContentPage ()
		{
			Title ="Bound Gallery";

			BindingContext = new BoundContentPageViewModel ();

			var button = new Button ();
			button.SetBinding (Button.TextProperty, "ButtonText");
			button.SetBinding (Button.CommandProperty, "ButtonCommand");

			SetBinding (NavigationProperty, new Binding ("Navigation", converter: new NavWrapperConverter ()));

			Content = button;
		}
	}
}
