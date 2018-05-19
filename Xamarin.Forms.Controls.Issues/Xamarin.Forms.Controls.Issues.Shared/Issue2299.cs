using System.Linq;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2299, "[iOS] ListView does not scroll when a control in listview has a PanGestureRecognizer", PlatformAffected.iOS)]
	public class Issue2299 : TestContentPage
	{
		static Label _label = new Label { Text = "Scroll the list. If you touch one of the Labels marked 'Pan', this label will change." };
		protected override void Init()
		{
			Application.Current.On<iOS>().SetPanGestureRecognizerShouldRecognizeSimultaneously(true);

			Content = new StackLayout
			{
				Children = {
					_label,
					new ListView { ItemTemplate = new DataTemplate(typeof(MyViewCell)), ItemsSource = Enumerable.Range(0, 20).Select(i => new TextContainer { Text1 = $"Normal {i}", Text2 = $"Pan {i}" }) }
				}
			};
		}

		[Preserve(AllMembers = true)]
		class MyViewCell : ViewCell
		{
			public MyViewCell()
			{
				var pan = new PanGestureRecognizer();
				pan.PanUpdated += Pan_PanUpdated;

				var label1 = new Label();
				label1.SetBinding(Label.TextProperty, "Text1");

				var label2 = new Label();
				label2.SetBinding(Label.TextProperty, "Text2");
				label2.GestureRecognizers.Add(pan);

				StackLayout stackLayout = new StackLayout { Orientation = StackOrientation.Horizontal, Children = { label1, label2 } };
				View = stackLayout;
			}

			void Pan_PanUpdated(object sender, PanUpdatedEventArgs e)
			{
				_label.Text = $"panned x:{e.TotalX} y:{e.TotalY}";
			}
		}

		[Preserve(AllMembers = true)]
		class TextContainer
		{
			public string Text1 { get; set; }
			public string Text2 { get; set; }
		}
	}
}