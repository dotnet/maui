using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1685, "Entry clears when upadting text from native with one-way binding", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.WinPhone, NavigationBehavior.PushModalAsync)]
	public class Issue1685 : ContentPage
	{
		class Test : INotifyPropertyChanged
		{
			public event PropertyChangedEventHandler PropertyChanged;

			decimal _entryValue = decimal.Zero;
			public decimal EntryValue
			{
				get
				{
					return _entryValue;
				}
				set
				{
					_entryValue = value;
					OnPropertyChanged("EntryValue");
				}
			}

			void OnPropertyChanged(string caller) {
				var handler = PropertyChanged;
				if (handler != null) 
				{
					handler(this, new PropertyChangedEventArgs(caller));
				}
			}
		}

		public Issue1685()
		{
			Title = "EntryBindingBug";

			BindingContext = new Test();

			var entry = new Entry() {
				Placeholder = "Entry"
			};
			entry.SetBinding(Entry.TextProperty, "EntryValue", BindingMode.OneWay);

			var button = new Button() {
				Text = "Click me"
			};

			button.Clicked += (sender, e) => 
			{
				var context = BindingContext as Test;
				context.EntryValue = context.EntryValue + 1;
			};

			var root = new StackLayout() {
				VerticalOptions = LayoutOptions.FillAndExpand,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Children = {
					entry,
					button
				}
			};

			Content = root;

		}
	}
}
