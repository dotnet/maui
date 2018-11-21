using System;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Maps;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{

#if UITEST
	[NUnit.Framework.Category(UITestCategories.UwpIgnore)]
#endif
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 26171, "Xamarin.Forms.Maps is not updating VisibleRegion property when layout is changed")]
	public class Bugzilla26171 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		protected override void Init ()
		{
			var map = MakeMap ();

			var label = new Label  { AutomationId = "lblValue" };

			var buttonLayout = new Button { Text = "Change layout" };
			buttonLayout.Clicked += async (a, e) => {
				map.VerticalOptions = LayoutOptions.Start;
				await Print (map,label);
			};

			var stack = new StackLayout {
				Spacing = 0,
				Padding = new Thickness (30, 0)
			};

			stack.Children.Add (label);
			stack.Children.Add (map);
			stack.Children.Add (buttonLayout);

			Content = new ScrollView { Content = stack };

			Appearing += async (sender, e) => await Print (map, label);

		}

		static async Task Print (Map map, Label label)
		{	
			await Task.Delay(500);
			if (map.VisibleRegion != null) {
				label.Text = map.VisibleRegion.Radius.Kilometers.ToString ();
			}
		}

		public static Map MakeMap ()
		{
			Pin colosseum = null;
			Pin pantheon = null;
			Pin chapel = null;

			var map = new Map {
				IsShowingUser = false,
				VerticalOptions = LayoutOptions.FillAndExpand,
				HeightRequest = 100,
				Pins = {
					(colosseum = new Pin {
						Type = PinType.Place,
						Position = new Position (41.890202, 12.492049),
						Label = "Colosseum",
						Address = "Piazza del Colosseo, 00184 Rome, Province of Rome, Italy"
					}),
					(pantheon = new Pin {
						Type = PinType.Place,
						Position = new Position (41.898652, 12.476831),
						Label = "Pantheon",
						Address = "Piazza della Rotunda, 00186 Rome, Province of Rome, Italy"
					}),
					(chapel = new Pin {
						Type = PinType.Place,
						Position = new Position (41.903209, 12.454545),
						Label = "Sistine Chapel",
						Address = "Piazza della Rotunda, 00186 Rome, Province of Rome, Italy"
					})
				}
			};

			return map;
		}

#if UITEST
		[Test]
		public void Bugzilla26171Test ()
		{
			RunningApp.WaitForElement (q => q.Marked ("lblValue"));
		}
#endif
	}
}
