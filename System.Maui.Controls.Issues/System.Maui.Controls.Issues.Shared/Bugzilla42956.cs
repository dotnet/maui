using System;
using System.Linq;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

// Apply the default category of "Issues" to all of the tests in this assembly
// We use this as a catch-all for tests which haven't been individually categorized
#if UITEST
[assembly: NUnit.Framework.Category("Issues")]
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 42956, "ListView with DataTemplateSelector can have only 17 Templates, even with CachingStrategy=RetainElement", PlatformAffected.Android)]
	public class Bugzilla42956 : TestContentPage
	{
		const string Success = "Success";

		class MyDataTemplateSelector : DataTemplateSelector
		{
			readonly DataTemplate one;
			readonly DataTemplate two;
			readonly DataTemplate three;
			readonly DataTemplate four;
			readonly DataTemplate five;
			readonly DataTemplate six;
			readonly DataTemplate seven;
			readonly DataTemplate eight;
			readonly DataTemplate nine;
			readonly DataTemplate ten;
			readonly DataTemplate eleven;
			readonly DataTemplate twelve;
			readonly DataTemplate thirteen;
			readonly DataTemplate fourteen;
			readonly DataTemplate fifteen;
			readonly DataTemplate sixteen;
			readonly DataTemplate seventeen;
			readonly DataTemplate eighteen;
			readonly DataTemplate nineteen;
			readonly DataTemplate twenty;

			public MyDataTemplateSelector()
			{
				one = new DataTemplate(() => new ViewCell { View = new Label { Text = "I am the one!" } });
				two = new DataTemplate(() => new ViewCell { View = new Label { Text = "I am the two!" } });
				three = new DataTemplate(() => new ViewCell { View = new Label { Text = "I am the three!" } });
				four = new DataTemplate(() => new ViewCell { View = new Label { Text = "I am the four!" } });
				five = new DataTemplate(() => new ViewCell { View = new Label { Text = "I am the five!" } });
				six = new DataTemplate(() => new ViewCell { View = new Label { Text = "I am the six!" } });
				seven = new DataTemplate(() => new ViewCell { View = new Label { Text = "I am the seven!" } });
				eight = new DataTemplate(() => new ViewCell { View = new Label { Text = "I am the eight!" } });
				nine = new DataTemplate(() => new ViewCell { View = new Label { Text = "I am the nine!" } });
				ten = new DataTemplate(() => new ViewCell { View = new Label { Text = "I am the ten!" } });
				eleven = new DataTemplate(() => new ViewCell { View = new Label { Text = "I am the eleven!" } });
				twelve = new DataTemplate(() => new ViewCell { View = new Label { Text = "I am the twelve!" } });
				thirteen = new DataTemplate(() => new ViewCell { View = new Label { Text = "I am the thirteen!" } });
				fourteen = new DataTemplate(() => new ViewCell { View = new Label { Text = "I am the fourteen!" } });
				fifteen = new DataTemplate(() => new ViewCell { View = new Label { Text = "I am the fifteen!" } });
				sixteen = new DataTemplate(() => new ViewCell { View = new Label { Text = "I am the sixteen!" } });
				seventeen = new DataTemplate(() => new ViewCell { View = new Label { Text = "I am the seventeen!" } });
				eighteen = new DataTemplate(() => new ViewCell { View = new Label { Text = "I am the eighteen!" } });
				nineteen = new DataTemplate(() => new ViewCell { View = new Label { Text = "I am the nineteen! Is this how I should be databinding? Whatev." } });
				twenty = new DataTemplate(() => new ViewCell { View = new Label { Text = Success } });
			}

			protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
			{
				var val = (int)item;

				switch (val)
				{
					case 1:
						return one;
					case 2:
						return two;
					case 3:
						return three;
					case 4:
						return four;
					case 5:
						return five;
					case 6:
						return six;
					case 7:
						return seven; //not six
					case 8:
						return eight;
					case 9:
						return nine;
					case 10:
						return ten;
					case 11:
						return eleven;
					case 12:
						return twelve;
					case 13:
						return thirteen;
					case 14:
						return fourteen;
					case 15:
						return fifteen;
					case 16:
						return sixteen;
					case 17:
						return seventeen;
					case 18:
						return eighteen;
					case 19:
					default:
						return nineteen;
					case 75:
						return twenty;
				}
			}
		}

		protected override void Init()
		{
			var dts = new MyDataTemplateSelector();
			var listView = new ListView
			{
				ItemsSource = Enumerable.Range(0, 100),
				ItemTemplate = dts
			};

			var layout = new StackLayout { Children = { listView } };

			Content = layout;

			listView.ScrollTo(75, ScrollToPosition.MakeVisible, true);
		}

#if UITEST
		[Test]
		public void Bugzilla42956Test()
		{
			RunningApp.WaitForElement(q => q.Marked(Success));
		}
#endif
	}
}