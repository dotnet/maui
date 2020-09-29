using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms.CustomAttributes;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Issue(IssueTracker.Github, 10477, "[Bug] CollectionView Header Controls with Commands Don't work when EmptyView is Visible", PlatformAffected.Android)]
	public partial class Issue10477 : TestContentPage
	{
		public Issue10477()
		{
#if APP
			InitializeComponent();
#endif
			ClickCommand = new Command(Click);
			BindingContext = this;
		}

		public ObservableCollection<string> Items { get; } = new ObservableCollection<string>();
		public ICommand ClickCommand { get; }

		protected override void Init()
		{

		}

		void Click()
		{
			if (BackgroundColor == Color.DarkOrange)
				BackgroundColor = Color.Red;
			else
				BackgroundColor = Color.DarkOrange;
		}
	}
}