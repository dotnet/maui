using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 1891, "Modal dialog scrolls to far when focusing input boxes", PlatformAffected.iOS)]
	public class Issue1891 : TestContentPage
	{
		protected override void Init ()
		{
			var entry = new Entry {
				Text = "Email Address", 
				VerticalOptions = LayoutOptions.End,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				HeightRequest = 60
			};
			var btn = new Button { Text = "focus entry" };
			btn.Clicked += async (object sender, EventArgs e) => {
				await Navigation.PushModalAsync (new ModalWithInputPage ());
			};

			Content = new ScrollView {
				Content = new StackLayout {
					VerticalOptions = LayoutOptions.End,
					HorizontalOptions = LayoutOptions.FillAndExpand,
					Children = {
						new StackLayout{ Children = { btn, entry } }
					}
				}
			};
		}
		public class ModalWithInputPage : ContentPage
		{
			public ModalWithInputPage()
			{
				Content = BuildLayout();
			}

			static Layout BuildLayout()
			{
				return new ScrollView 
				{ 
					Content = new StackLayout
					{
						VerticalOptions = LayoutOptions.End,
						HorizontalOptions = LayoutOptions.FillAndExpand,
						Children = {
							new Entry {
								Placeholder = "Email Address", 
								VerticalOptions = LayoutOptions.End,
								HorizontalOptions = LayoutOptions.FillAndExpand,
								HeightRequest = 60
							}
						}
					}
				};
			}
		}

#if UITEST
		[Test]
		[UiTest (typeof(TabbedPage))]
		public void Issue1891Tests ()
		{
			// TODO
			// Please redo, invalid test
			Assert.Inconclusive ("Needs test");
		}
#endif

	}
}
