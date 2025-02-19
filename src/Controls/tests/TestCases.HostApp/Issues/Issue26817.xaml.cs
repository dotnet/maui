namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 26817, "CollectionViewHandler2 assigns Accessibility Traits with SelectionMode correctly", PlatformAffected.macOS)]
	public partial class Issue26817 : ContentPage
	{
		public Issue26817()
		{
			InitializeComponent();
		}

		bool toggle;

		private void Button_Clicked(object sender, EventArgs e)
		{
			toggle = !toggle;
			collectionView.SelectionMode = toggle ? SelectionMode.None : SelectionMode.Single;
			CheckAccessibilityTraits();
		}

		public void CheckAccessibilityTraits()
		{
#if IOS || MACCATALYST
			if (collectionView.Handler?.PlatformView is UIKit.UIView collectionViewWrapper)
			{
				if (collectionViewWrapper.Subviews.Length == 0 || collectionViewWrapper.Subviews[0] is not UIKit.UIView cv)
					return;

				foreach (var child in cv.Subviews)
				{
					if (child is UIKit.UICollectionViewCell cell && cell.ContentView.Subviews.FirstOrDefault() is UIKit.UIView firstChild)
					{
						bool isButton = (firstChild.AccessibilityTraits & UIKit.UIAccessibilityTrait.Button) == UIKit.UIAccessibilityTrait.Button;
						Label1.Text = $"Is Button - {isButton}";
						break;
					}
				}
			}
#endif
		}
	}
}
