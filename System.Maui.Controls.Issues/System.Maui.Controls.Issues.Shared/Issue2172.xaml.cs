using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2172, "Height of Entry with data binding incorrect on UWP when Entry in ScrollView in Grid", PlatformAffected.UWP)]
	public partial class Issue2172 : ContentPage
	{
		public Issue2172()
		{
#if APP
			InitializeComponent();			
#endif
			BindingContext = new Issue2172ViewModel();
		}

		public class Issue2172ViewModel
		{
			public string Number => "Bound Text";
		}
	}

	public class Issue2172OldEntry : Entry
	{

	}

	public class Issue2172OldEditor : Editor
	{

	}
}