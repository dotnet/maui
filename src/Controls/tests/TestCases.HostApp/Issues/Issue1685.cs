using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Entry = Microsoft.Maui.Controls.Entry;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1685, "Entry clears when upadting text from native with one-way binding", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.WinPhone, NavigationBehavior.PushModalAsync)]
	public class Issue1685 : TestContentPage
	{
		const string ButtonId = "Button1685";
		const string Success = "Success";

		[Preserve(AllMembers = true)]
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

			void OnPropertyChanged(string caller)
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
			}
		}

		protected override void Init()
		{
			Title = "EntryBindingBug";
			On<iOS>().SetUseSafeArea(true);

			BindingContext = new Test();

			var entry = new Entry()
			{
				Placeholder = "Entry"
			};
			entry.SetBinding(Entry.TextProperty, "EntryValue", BindingMode.OneWay);

			var button = new Button()
			{
				Text = "Click me",
				AutomationId = ButtonId
			};

			button.Clicked += (sender, e) =>
			{
				var context = BindingContext as Test;
				context.EntryValue = Success;
			};

#pragma warning disable CS0618 // Type or member is obsolete
			var root = new StackLayout()
			{
				VerticalOptions = LayoutOptions.FillAndExpand,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Children = {
					entry,
					button
				}
			};
#pragma warning restore CS0618 // Type or member is obsolete

			Content = root;
		}
	}
}
