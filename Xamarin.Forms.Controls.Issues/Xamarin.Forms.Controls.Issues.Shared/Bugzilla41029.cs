using System;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;


namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 41029, "Slider default hitbox is larger than the control")]
	public class Bugzilla41029 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		protected override void Init()
		{
			var layout = new StackLayout { VerticalOptions = LayoutOptions.Center };
			var lbl = new Label();
			var slider = new Slider();
			slider.HeightRequest = 50;
			slider.ValueChanged += (object sender, ValueChangedEventArgs e) =>
			{

				lbl.Text = e.NewValue.ToString();
			};
			layout.Children.Add(lbl);
			layout.Children.Add(slider);

			Content = layout;
		}
	}
}
