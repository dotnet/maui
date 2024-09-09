using System;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using Xamarin.UITest.iOS;
using Xamarin.UITest.Queries;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.TableView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 36955, "[iOS] ViewCellRenderer.UpdateIsEnabled referencing null object", PlatformAffected.iOS)]
	public class Bugzilla36955 : TestContentPage
	{
		protected override void Init()
		{
			var ts = new TableSection();
			var tr = new TableRoot { ts };
			var tv = new TableView(tr);

			var sc = new SwitchCell
			{
				Text = "Toggle switch; nothing should crash"
			};

			var button = new Button();
			button.SetBinding(Button.TextProperty, new Binding("On", source: sc));

			var vc = new ViewCell
			{
				View = button
			};
			vc.SetBinding(Cell.IsEnabledProperty, new Binding("On", source: sc));

			ts.Add(sc);
			ts.Add(vc);

			Content = tv;
		}

#if UITEST && __IOS__
		[Ignore("Test failing due to unrelated issue, disable for moment")]
		[Test]
		public void Bugzilla36955Test()
		{
			AppResult[] buttonFalse = RunningApp.Query(q => q.Button().Text("False"));
			Assert.AreEqual(buttonFalse.Length == 1, true);
			RunningApp.Tap(q => q.Class("Switch"));
			AppResult[] buttonTrue = RunningApp.Query(q => q.Button().Text("True"));
			Assert.AreEqual(buttonTrue.Length == 1, true);
		}
#endif
	}
}
