using System;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 43516, "Changing FontAttributes on a label to None changes its font size")]
	public class Bugzill43516 : TestContentPage
	{
		protected override void Init()
		{
			var label = new Label
			{
				FontAttributes = FontAttributes.Bold
			};
			label.BindingContext = label;
			label.SetBinding(Label.TextProperty, "FontAttributes");

			Content = new StackLayout
			{
				Children =
				{
					label,
					new Button
					{
						Text = "Click to set FontAttributes.None",
						Command = new Command(() =>
						{
							label.FontAttributes = FontAttributes.None;
						})
					},
					new Button
					{
						Text = "Click to set FontAttributes.Bold",
						Command = new Command(() =>
						{
							label.FontAttributes = FontAttributes.Bold;
						})
					},
					new Button
					{
						Text = "Click to set Font.SystemFontOfSize to NamedSize.Medium",
						Command = new Command(() => label.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)))
					}
				}
			};
		}
	}
}
