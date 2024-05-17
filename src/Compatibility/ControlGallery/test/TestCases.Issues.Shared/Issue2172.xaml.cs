using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
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
#if APP
		void Button_OnClicked(object sender, EventArgs e)
		{
			BoundEntry.HeightRequest = 100;
			NestedEntry.HeightRequest = 100;
			EmptyEntry.HeightRequest = 100;

			TextAlignment newAlignment;
			if (BoundEntry.VerticalTextAlignment == TextAlignment.Center)
				newAlignment = TextAlignment.End;
			else if (BoundEntry.VerticalTextAlignment == TextAlignment.End)
				newAlignment = TextAlignment.Start;
			else
				newAlignment = TextAlignment.Center;
			BoundEntry.VerticalTextAlignment = newAlignment;
			NestedEntry.VerticalTextAlignment = newAlignment;
			EmptyEntry.VerticalTextAlignment = newAlignment;
		}
#endif
	}

	public class Issue2172OldEntry : Entry
	{

	}

	public class Issue2172OldEditor : Editor
	{

	}
}