using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	public class FamilyViewModel
	{
		public Guid ProfileId { get; set; }
		public string DisplayName { get; set; }
		public string ImageFilename { get; set; }
		public string BonusBalance { get; set; }
		public string MemberNo { get; set; }

		public FamilyViewModel()
		{
			ProfileId = Guid.Empty;
			DisplayName = "";
			BonusBalance = "";
			MemberNo = "";
			ImageFilename = "";
		}
	}

	[Preserve(AllMembers = true)]
	public class FamilyCell : ViewCell
	{
		public Label FamilyLabel;

		public FamilyCell()
		{
			FamilyLabel = new Label();

			var l1 = new Compatibility.RelativeLayout();

			l1.Children.Add(FamilyLabel,
							 Compatibility.Constraint.Constant(50),
							Compatibility.Constraint.Constant(4),
							Compatibility.Constraint.RelativeToParent(p => p.Width - 10 - 50 - 85)
				);


			View = l1;

			FamilyLabel.SetBinding(Label.TextProperty, "DisplayName");

			// COMMENT LINE BELOW OUT TO MAKE IT WORK!
			AddContextActions();
		}

		void AddContextActions()
		{
			ContextActions.Add(new MenuItem()
			{
				Text = "Delete",
				IsDestructive = true,
				Command = new Command(Delete)
			});

			ContextActions.Add(new MenuItem()
			{
				Text = "More",
				IsDestructive = false,
				Command = new Command(More)
			});
		}

		void Delete()
		{
		}

		void More()
		{
		}
	}

#if UITEST
	[Category(UITestCategories.InputTransparent)]
	[Category(UITestCategories.UwpIgnore)]
	[Category(UITestCategories.Bugzilla)]
	[FailsOnMaui]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 26501, "BindingSource / Context action issue", PlatformAffected.iOS)]
	public class Bugzilla26501 : TestContentPage
	{
		protected override void Init()
		{
			//TODO: Multilanguage
			Title = "Context Action Bug";

			_familyListView = new ListView()
			{
				RowHeight = 50,
				ItemTemplate = new DataTemplate(typeof(FamilyCell)),
				HasUnevenRows = true
			};

			//TODO: Multilanguage
			ToolbarItems.Add(new ToolbarItem("Refresh", "", () =>
			{
				_familyListView.ItemsSource = _demoDataSource2;
			}));


			_familyListView.ItemSelected += (sender, e) => _familyListView.SelectedItem = null;

			Content = _familyListView;

			UpdateData();
		}

		readonly FamilyViewModel[] _demoDataSource = new FamilyViewModel[] {
			new FamilyViewModel {DisplayName = "ZOOMER robothund"},
			new FamilyViewModel {DisplayName = "FROST senget�j"},
			new FamilyViewModel {DisplayName = "BEADOS Quick Dry designstation"},
			new FamilyViewModel {DisplayName = "Redningsstation i junglen"},
		};

		readonly FamilyViewModel[] _demoDataSource2 = new FamilyViewModel[] {
			new FamilyViewModel {DisplayName = "ZOOMER robothund 2"},
			new FamilyViewModel {DisplayName = "FROST senget�j"},
			new FamilyViewModel {DisplayName = "BEADOS Quick Dry designstation"},
			new FamilyViewModel {DisplayName = "Redningsstation i junglen"},
			new FamilyViewModel {DisplayName = "CHAMPIONS LEAGUE 2014/15 boosterpakke"},
			new FamilyViewModel {DisplayName = "NEW BORN BABY luksus�ske med dukke"},
			new FamilyViewModel {DisplayName = "FURBY Boom Festive Sweater elektronisk plysdyr"},
			new FamilyViewModel {DisplayName = "LEGO FRIENDS 41007 Heartlake hundesalon"},
			new FamilyViewModel {DisplayName = "LEGO CITY 4204 Minen"}
		};

		ListView _familyListView;

		void UpdateData()
		{
			Device.BeginInvokeOnMainThread(() => _familyListView.ItemsSource = _demoDataSource);
		}

#if UITEST
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiAndroid]
		[Test]
		public void TestCellsShowAfterRefresh()
		{
			RunningApp.Tap(q => q.Marked("Refresh"));

			RunningApp.WaitForElement(q => q.Marked("ZOOMER robothund 2"));
		}
#endif
	}
}