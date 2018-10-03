using System;
using System.Collections.ObjectModel;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 56298, "Changing ListViews HasUnevenRows at runtime on iOS has no effect", PlatformAffected.iOS)]
	public class Bugzilla56298 : TestContentPage // or TestMasterDetailPage, etc ...
	{

		ListView list;
		Button button;
		StackLayout layoutRoot;

		ObservableCollection<Group> groups;
		public static int Count = 0;

		protected override void Init()
		{
			On<iOS>().SetUseSafeArea(true);
			list = new ListView();

			var template = new DataTemplate(typeof(UnevenViewCell));
			//template.SetBinding(TextCell.TextProperty, "FullName");
			//template.SetBinding(TextCell.DetailProperty, "Address");
			list.ItemTemplate = template;

			groups = new ObservableCollection<Group>();
			list.ItemsSource = groups;
			list.GroupDisplayBinding = new Binding(nameof(Group.Key));
			list.GroupShortNameBinding = new Binding(nameof(Group.Key));
			list.IsGroupingEnabled = true;

			button = new Button { Text = "Add new data", AutomationId = "btnAdd" };
			button.Clicked += Button_Clicked;

			var button1 = new Button { Text = "Toggle Uneven rows", AutomationId = "btnToggle" };
			button1.Clicked += Button_Clicked1;


			layoutRoot = new StackLayout();
			layoutRoot.Children.Add(list);
			layoutRoot.Children.Add(button);
			layoutRoot.Children.Add(button1);

			this.Content = layoutRoot;
		}

		void Button_Clicked(object sender, EventArgs e)
		{

			var group = new Group()
			{
				Key = "A"
			};
			for (int i = 0; i < 59; i++)
			{
				group.Add(new Person1
				{
					FullName = "Andrew",
					Address = "404 Somewhere"
				});
			}

			groups.Add(group);
		}

		private void Button_Clicked1(object sender, EventArgs e)
		{
			list.HasUnevenRows = !list.HasUnevenRows;
		}

		[Preserve(AllMembers = true)]
		class UnevenViewCell : ViewCell
		{
			public UnevenViewCell()
			{

				var label = new Label();
				label.SetBinding(Label.TextProperty, "FullName");
				Height = Bugzilla56298.Count % 2 == 0 ? 50 : 100;
				View = label;
				View.BackgroundColor = Bugzilla56298.Count % 2 == 0 ? Color.Pink : Color.LightYellow;
				Bugzilla56298.Count++;
			}
		}

		[Preserve(AllMembers = true)]
		class Person1
		{
			public string FullName { get; set; }
			public string Address { get; set; }
		}

		class Group : ObservableCollection<Person1>
		{
			public string Key { get; set; }
		}

#if UITEST
		[Test]
		public void Bugzilla56298Test()
		{
			RunningApp.WaitForElement(q => q.Marked("btnAdd"));
			RunningApp.Tap(q => q.Marked("btnAdd"));
			RunningApp.Tap(q => q.Marked("btnToggle"));
			RunningApp.Screenshot("Verify we see uneven rows");
		}
#endif
	}
}
