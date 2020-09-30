using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	public class FooEffect : RoutingEffect
	{
		public FooEffect() : base("XamControl.FooEffect")
		{
		}
	}

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 9555, "[iOS] Applying an Effect to Frame adds a shadow", PlatformAffected.iOS)]
	public partial class Issue9555 : ContentPage
	{
		public Issue9555()
		{
#if APP
			InitializeComponent();
#endif
		}
	}
}