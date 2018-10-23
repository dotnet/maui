using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4136, "[Android] Missing call to UpdateLineHeight in LabelRenderer",
		PlatformAffected.Android)]
	public class Issue4136 : TestContentPage
	{
		protected override void Init()
		{
			var layout = new StackLayout { Spacing = 15 };

			var instructions = new Label
			{
				FontAttributes = FontAttributes.Bold,
				Text =
					"The second Label below should have more space between its lines than the first. If the two labels have the same spacing, this test has failed."
			};

			var normal = new Label
			{
				LineBreakMode = LineBreakMode.WordWrap,
				Text =
					"Lorem ipsum dolor sit amet, cu mei malis petentium, dolor tempor delicata no qui, eos ex vitae utinam vituperata. Utroque habemus philosophia ut mei, doctus placerat eam cu. An inermis scaevola pro, quo legimus deleniti ei, equidem docendi urbanitas ea eum. Saepe doctus ut pri. Nec ex wisi dolorem. Duo dolor vituperatoribus ea. Id purto instructior per. Nec partem accusamus ne. Qui ad saepe accumsan appellantur, duis omnesque has et, vim nihil nemore scaevola ne. Ei populo appetere recteque cum, meliore splendide appellantur vix id."
			};

			var spaced = new Label
			{
				LineHeight = 1.5,
				LineBreakMode = LineBreakMode.WordWrap,
				Text =
					"Lorem ipsum dolor sit amet, cu mei malis petentium, dolor tempor delicata no qui, eos ex vitae utinam vituperata. Utroque habemus philosophia ut mei, doctus placerat eam cu. An inermis scaevola pro, quo legimus deleniti ei, equidem docendi urbanitas ea eum. Saepe doctus ut pri. Nec ex wisi dolorem. Duo dolor vituperatoribus ea. Id purto instructior per. Nec partem accusamus ne. Qui ad saepe accumsan appellantur, duis omnesque has et, vim nihil nemore scaevola ne. Ei populo appetere recteque cum, meliore splendide appellantur vix id."
			};

			layout.Children.Add(instructions);
			layout.Children.Add(normal);
			layout.Children.Add(spaced);

			Content = layout;
		}
	}
}