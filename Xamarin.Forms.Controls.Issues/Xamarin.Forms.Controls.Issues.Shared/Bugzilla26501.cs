using System;
using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	public class FamilyViewModel
	{
		public Guid ProfileId { get; set; }
		public string DisplayName { get; set; }
		public string ImageFilename { get; set; }
		public string BonusBalance { get; set; }
		public string MemberNo { get; set; }

		public FamilyViewModel ()
		{
			ProfileId = Guid.Empty;
			DisplayName = "";
			BonusBalance = "";
			MemberNo = "";
			ImageFilename = "";
		}
	}

	[Preserve (AllMembers = true)]
	public class FamilyCell : ViewCell
	{
		public Label FamilyLabel;

		public FamilyCell ()
		{
			FamilyLabel = new Label ();

			var l1 = new RelativeLayout ();

			l1.Children.Add (FamilyLabel,
			                 Constraint.Constant (50),
			                 Constraint.Constant (4),
			                 Constraint.RelativeToParent (p => p.Width - 10 - 50 - 85)
				);


			View = l1;

			FamilyLabel.SetBinding (Label.TextProperty, "DisplayName");

			// COMMENT LINE BELOW OUT TO MAKE IT WORK!
			AddContextActions ();
		}

		void AddContextActions ()
		{
			ContextActions.Add (new MenuItem () {
				Text = "Delete",
				IsDestructive = true,
				Command = new Command (Delete)
			});

			ContextActions.Add (new MenuItem () {
				Text = "More",
				IsDestructive = false,
				Command = new Command (More)
			});
		}

		void Delete ()
		{
		}

		void More ()
		{
		}
	}

#if UITEST
	[Category(UITestCategories.InputTransparent)]
	[NUnit.Framework.Category(UITestCategories.UwpIgnore)]
#endif
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Bugzilla, 26501, "BindingSource / Context action issue", PlatformAffected.iOS)]
	public class Bugzilla26501 : TestContentPage
	{
		protected override void Init ()
		{
			//TODO: Multilanguage
			Title = "Context Action Bug";

			_familyListView = new ListView () {
				RowHeight = 50,
				ItemTemplate = new DataTemplate (typeof (FamilyCell)),
				HasUnevenRows = true
			};

			//TODO: Multilanguage
			ToolbarItems.Add (new ToolbarItem ("Refresh", "", () => {
				_familyListView.ItemsSource = _demoDataSource2;
			}));


			_familyListView.ItemSelected += (sender, e) => _familyListView.SelectedItem = null;

			Content = _familyListView;

			UpdateData ();
		}

		readonly FamilyViewModel[] _demoDataSource = new FamilyViewModel[] {
			new FamilyViewModel {DisplayName = "ZOOMER robothund"},
			new FamilyViewModel {DisplayName = "FROST sengetøj"},
			new FamilyViewModel {DisplayName = "BEADOS Quick Dry designstation"},
			new FamilyViewModel {DisplayName = "Redningsstation i junglen"},
		};

		readonly FamilyViewModel[] _demoDataSource2 = new FamilyViewModel[] {
			new FamilyViewModel {DisplayName = "ZOOMER robothund 2"},
			new FamilyViewModel {DisplayName = "FROST sengetøj"},
			new FamilyViewModel {DisplayName = "BEADOS Quick Dry designstation"},
			new FamilyViewModel {DisplayName = "Redningsstation i junglen"},
			new FamilyViewModel {DisplayName = "CHAMPIONS LEAGUE 2014/15 boosterpakke"},
			new FamilyViewModel {DisplayName = "NEW BORN BABY luksusæske med dukke"},
			new FamilyViewModel {DisplayName = "FURBY Boom Festive Sweater elektronisk plysdyr"},
			new FamilyViewModel {DisplayName = "LEGO FRIENDS 41007 Heartlake hundesalon"},
			new FamilyViewModel {DisplayName = "LEGO CITY 4204 Minen"}
		};

		ListView _familyListView;

		List<FamilyViewModel> _itemSource;

		void UpdateData ()
		{
			Device.BeginInvokeOnMainThread (() => _familyListView.ItemsSource = _demoDataSource);
		}

#if UITEST
		[Test]
		public void TestCellsShowAfterRefresh()
		{
			RunningApp.Tap (q => q.Marked ("Refresh"));
		
			RunningApp.WaitForElement (q => q.Marked ("ZOOMER robothund 2"));
		}
#endif
	}
}