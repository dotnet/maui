using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 530, "ListView does not render if source is async", PlatformAffected.iOS)]
	public class Issue530 : TestContentPage
	{
		ListView _list;
		Button _button;

		protected override void Init ()
		{
			var dataTemplate = new DataTemplate (typeof (TextCell));
			dataTemplate.SetBinding (TextCell.TextProperty, new Binding ("."));

			_list = new ListView { 
				ItemTemplate = dataTemplate
			};

			_button = new Button {
				Text = "Load",
			};

			_button.Clicked += async (sender, e) => {
				await Task.Delay (1000);
				_list.ItemsSource = new [] {"John","Paul", "George", "Ringo"};
			};
			Content = new StackLayout {
				Children = {
					_button,
					_list,
				}
			};
		}

#if UITEST
		[Test]
		[UiTest (typeof(ListView))]
		public void Issue530TestsLoadAsync ()
		{
			RunningApp.WaitForElement (q => q.Button ("Load"));
			RunningApp.Screenshot ("All elements present");
			RunningApp.Tap (q => q.Button ("Load"));

			RunningApp.WaitForElement (q => q.Marked ("John"));
			RunningApp.WaitForElement (q => q.Marked ("Paul"));
			RunningApp.WaitForElement (q => q.Marked ("George"));
			RunningApp.WaitForElement (q => q.Marked ("Ringo"));

			RunningApp.Screenshot ("List items loaded async");
		}
#endif

	}	
}
