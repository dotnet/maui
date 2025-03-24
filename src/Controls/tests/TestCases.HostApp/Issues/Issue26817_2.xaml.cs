namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 26817_2, "CollectionViewHandler assigns Accessibility Traits with SelectionMode correctly", PlatformAffected.macOS)]
	public partial class Issue26817_2 : ContentPage
	{
		public Issue26817_2()
		{
			InitializeComponent();
		}

		int toggleCount;

		private void Button_Clicked(object sender, EventArgs e)
		{
			var selectionMode = toggleCount switch
			{
				0 => SelectionMode.None,
				1 => SelectionMode.Single,
				2 => SelectionMode.Multiple,
				_ => throw new NotImplementedException(),
			};

			border.SelectionMode = selectionMode;
			vsl.SelectionMode = selectionMode;
			grid.SelectionMode = selectionMode;
			CheckAccessibilityTraits(border, Label1, "Border:");
			CheckAccessibilityTraits(vsl, Label2, "VSL:");
			CheckAccessibilityTraits(grid, Label3, "Grid:");

			selectionLabel.Text = "selectionMode: " + selectionMode;

			toggleCount = (toggleCount + 1) % 3;
		}

		public void CheckAccessibilityTraits(CollectionView collectionView, Label label, string prefix)
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
						label.Text = $"{prefix} Item is Button - {isButton}";
						break;
					}
				}
			}
#endif
		}
	}
}
