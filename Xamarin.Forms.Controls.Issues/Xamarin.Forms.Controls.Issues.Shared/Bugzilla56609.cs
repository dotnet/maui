using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 56609, "[UWP] Attached effect does not update focused background color", PlatformAffected.UWP)]
	public class Bugzilla56609 : TestContentPage
	{
		protected override void Init()
		{
			var entry = new Entry
			{
				Text = "Effect attached to an Entry - the background should turn white when focused",
				VerticalOptions = LayoutOptions.CenterAndExpand,
				HorizontalOptions = LayoutOptions.CenterAndExpand
			};
			entry.Effects.Add(Effect.Resolve($"{Issues.Effects.ResolutionGroupName}.FocusEffect"));

			Content = new StackLayout
			{
				Padding = new Thickness(0, 20, 0, 0),
				Children = {
					new Label {
						Text = "Effects Demo - Focus Effect",
						FontAttributes = FontAttributes.Bold,
						HorizontalOptions = LayoutOptions.Center
					},
					entry
				}
			};
		}
	}
	public class Bugzilla56609FocusEffect : RoutingEffect
	{
		public Bugzilla56609FocusEffect() : base($"{Effects.ResolutionGroupName}.FocusEffect")
		{
		}
	}
}