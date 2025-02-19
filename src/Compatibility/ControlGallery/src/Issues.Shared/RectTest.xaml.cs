using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 1, "Using Rect struct to position items")]
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class RectTest : TestContentPage
	{
		public RectTest()
		{
#if APP
			InitializeComponent();
#endif
		}

		protected override void Init()
		{

		}
	}
}