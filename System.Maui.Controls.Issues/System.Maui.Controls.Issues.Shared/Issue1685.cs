using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Maui.CustomAttributes;
using System.Maui.Internals;
using System.Maui.PlatformConfiguration;
using System.Maui.PlatformConfiguration.iOSSpecific;

#if UITEST
using NUnit.Framework;
using System.Maui.Core.UITests;
#endif

namespace System.Maui.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1685, "Entry clears when upadting text from native with one-way binding", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.WinPhone, NavigationBehavior.PushModalAsync)]
	public class Issue1685 : TestContentPage
	{
		const string ButtonId = "Button1685";
		const string Success = "Success";

		[Preserve (AllMembers=true)]
		class Test : INotifyPropertyChanged
		{
			public event PropertyChangedEventHandler PropertyChanged;

			string _entryValue = "0";
			public string EntryValue
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
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
			}
		}

		protected override void Init()
		{
			Title = "EntryBindingBug";
            On<iOS>().SetUseSafeArea(true);

			BindingContext = new Test();

			var entry = new Entry() {
				Placeholder = "Entry"
			};
			entry.SetBinding(Entry.TextProperty, "EntryValue", BindingMode.OneWay);

			var button = new Button() {
				Text = "Click me",
				AutomationId = ButtonId
			};

			button.Clicked += (sender, e) => 
			{
				var context = BindingContext as Test;
				context.EntryValue = Success;
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

#if UITEST
		[Test]
		[NUnit.Framework.Category(UITestCategories.Entry)]
		public void EntryOneWayBindingShouldUpdate()
		{
			RunningApp.WaitForElement(ButtonId);
			RunningApp.Tap(ButtonId);
			RunningApp.WaitForElement(c => c.Text(Success));
		}
#endif
	}
}
