using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1895, "ListView with static BindingContext somehow leaks memory", PlatformAffected.iOS | PlatformAffected.Android)]
	public class Issue1895
		: ContentPage
	{
		public Issue1895()
		{
			var button = new Button { Text = "Push weak page" };
			button.Clicked += async (sender, args) => await Navigation.PushAsync (CreateWeakReferencedPage());
			Content = button;
		}

		static List<WeakReference> s_pageRefs = new List<WeakReference>();
		static FakeProvider s_fakeProvider = new FakeProvider();

		static Page CreateWeakReferencedPage()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			var result = CreatePage();
			s_pageRefs.Add(new WeakReference(result));

			return result;
		}

		class WeakReferencedPage : ContentPage
		{

		}

		static Page CreatePage()
		{
			var page = new WeakReferencedPage();
			var contents = new StackLayout();

			contents.Children.Add (new Button {
				Text = "Next Page",
				Command = new Command(() => page.Navigation.PushAsync(CreateWeakReferencedPage()))
			});

			contents.Children.Add (new Label {
				Text = string.Format ("References alive at time of creation: {0}", s_pageRefs.Count(p => p.IsAlive)),
				HorizontalOptions = LayoutOptions.CenterAndExpand
			});

			var listView = new ListView { BindingContext = s_fakeProvider };
			listView.SetBinding (ListView.ItemsSourceProperty, "Items");
			contents.Children.Add (listView);

			page.Content = contents;
			return page;
		}

		class FakeProvider
		{
			public ObservableCollection<string> Items { get; private set; } 

			public FakeProvider()
			{
				Items = new ObservableCollection<string>();
				Items.Add("Item 1");
				Items.Add("Item 2");
				Items.Add("Item 3");
				Items.Add("Item 4");
			}
		}
	}
}
