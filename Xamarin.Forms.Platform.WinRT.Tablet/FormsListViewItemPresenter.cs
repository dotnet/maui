using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;

namespace Xamarin.Forms.Platform.WinRT
{
	// This exists to work around the fact that when you set these bindings in XAML and compile on Win10
	// the resulting xbf file does not load correctly on Win8.1. MS has acknowledged the issue but has no fixed it.
	// This hacks around the problem.
	public class FormsListViewItemPresenter : ListViewItemPresenter
	{
		public FormsListViewItemPresenter()
		{
			var verticalContentAlignBinding = new Windows.UI.Xaml.Data.Binding
			{
				Path = new PropertyPath("VerticalContentAlignment"),
				RelativeSource = new RelativeSource { Mode = RelativeSourceMode.TemplatedParent }
			};
			BindingOperations.SetBinding(this, VerticalContentAlignmentProperty, verticalContentAlignBinding);

			var paddingBinding = new Windows.UI.Xaml.Data.Binding
			{
				Path = new PropertyPath("Padding"),
				RelativeSource = new RelativeSource { Mode = RelativeSourceMode.TemplatedParent }
			};
			BindingOperations.SetBinding(this, PaddingProperty, paddingBinding);

			var contentTransitionBinding = new Windows.UI.Xaml.Data.Binding
			{
				Path = new PropertyPath("ContentTransitions"),
				RelativeSource = new RelativeSource { Mode = RelativeSourceMode.TemplatedParent }
			};
			BindingOperations.SetBinding(this, ContentTransitionsProperty, contentTransitionBinding);
		}
	}
}