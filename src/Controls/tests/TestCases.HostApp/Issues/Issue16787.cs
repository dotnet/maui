namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 16787, "CollectionView runtime binding errors when loading the ItemSource asynchronously", PlatformAffected.UWP)]
	public class Issue16787 : TestContentPage
	{
		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();
		}
		protected override void Init()
		{
			var cv = new CollectionView();
			cv.BindingContextChanged += Cv_BindingContextChanged;
			this.BindingContext = this;
			cv.ItemTemplate = new DataTemplate(() =>
			{
				int bindingContextChanges = 0;
				var label = new Label();
				label.BindingContextChanged += (_, _) =>
				{
					bindingContextChanges++;
					label.Text = bindingContextChanges.ToString();
				};
				label.AutomationId = "LabelBindingCount";
				return label;
			});

			cv.ItemsSource = new[] { "random" };


			var layout = new VerticalStackLayout()
			{
				new Label()
				{
					Text = "The value below this label should be a 1. That's how many times the BindingContext has changed on the Templated element"
				}
			};
			SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
			Content = cv;
		}

		private void Cv_BindingContextChanged(object sender, System.EventArgs e)
		{
		}
	}
}
