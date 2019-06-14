using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.StyleSheets;
using System.Reflection;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Xamarin.Forms.Sandbox
{
	public partial class App : Application
	{
		public App()
		{
			Device.SetFlags(new[] { "Shell_Experimental", "CollectionView_Experimental" });
			InitializeMainPage();
		}

		void AddStyleSheet()
		{
			this.Resources.Add(StyleSheet.FromResource(
				"Styles.css",
				IntrospectionExtensions.GetTypeInfo(typeof(App)).Assembly));
		}

		void InitializeLegacyRenderers()
		{
			var flags = new List<String>(Device.Flags);
			flags.Add("UseLegacyRenderers");
			Device.SetFlags(flags.Select(x => x).Distinct().ToArray());
		}
				

		ContentPage CreateContentPage(View view)
		{
			var returnValue = new ContentPage() { Content = view };

			returnValue.On<iOS>().SetUseSafeArea(true);
			return returnValue;
		}


		ContentPage CreateListViewPage(Func<View> template)
		{
			var listView = new ListView(ListViewCachingStrategy.RecycleElement);
			listView.RowHeight = 500;
			listView.ItemsSource = Enumerable.Range(0, 1).ToList();
			listView.ItemTemplate = new DataTemplate(() =>
			{
				ViewCell cell = new ViewCell();
				cell.View = template();
				return cell;
			});

			return CreateContentPage(listView);
		}


		StackLayout CreateStackLayout(IEnumerable<View> children, StackOrientation orientation = StackOrientation.Vertical )
		{
			var sl = new StackLayout() { Orientation = orientation };
			foreach (var child in children)
				sl.Children.Add(child);

			return sl;
		}

		ContentPage CreateStackLayoutPage(IEnumerable<View> children)
		{
			return CreateContentPage(CreateStackLayout(children));
		}
	}
}
