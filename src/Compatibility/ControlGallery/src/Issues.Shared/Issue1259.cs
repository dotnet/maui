using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.TableView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1259, "Layout issue with SwitchCell", PlatformAffected.Android)]
	public class Issue1259
		: ContentPage
	{
		TableView _table;
		public Issue1259()
		{
			StackLayout st = new StackLayout();
			st.HorizontalOptions = LayoutOptions.FillAndExpand;
			st.VerticalOptions = LayoutOptions.FillAndExpand;

			_table = new TableView
			{
				Intent = TableIntent.Form,
				Root = new TableRoot("") {
					new TableSection
					{
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new SwitchCell
						{
							Text = "SwitchCell:"
						},
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new SwitchCell
						{
							Text = "SwitchCell:"
						},
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new TextCell(),
						new SwitchCell
						{
							Text = "SwitchCell:"
						}
					}
				}
			};

			st.Children.Add(_table);

			Button next = new Button
			{
				Text = "Ok",
			};
			next.Clicked += next_Clicked;

			st.Children.Add(next);

			Content = st;
		}

		void next_Clicked(object sender, EventArgs e)
		{
			var sw = _table.Root[0].OfType<SwitchCell>().First();
			sw.On = !sw.On;
		}
	}
}
