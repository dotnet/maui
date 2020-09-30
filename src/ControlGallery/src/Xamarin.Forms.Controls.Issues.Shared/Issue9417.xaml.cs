using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

#if UITEST
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.SwipeView)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 9417, "[iOS] XF 4.5 pre2 crashes on Swipe GetSwipeItemSize", PlatformAffected.iOS)]
	public partial class Issue9417 : TestContentPage
	{
		public Issue9417()
		{
#if APP
			Title = "Issue 9417";
			InitializeComponent();
#endif
		}

		public ObservableCollection<Issue9417Model> Items { get; set; } = new ObservableCollection<Issue9417Model>(Enumerable.Range(0, 10).Select(i => new Issue9417Model()));
		public Command DeleteCommand { get; set; }
		public Command EditCommand { get; set; }

		protected override void Init()
		{

		}
	}

	[Preserve(AllMembers = true)]
	public class Issue9417Model
	{

	}
}