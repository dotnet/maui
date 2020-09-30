using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.UITest.iOS;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7792, "(Android) CarouselView string EmptyView not displayed", PlatformAffected.Android)]
	public partial class Issue7792 : TestContentPage
	{
		public Issue7792()
		{
#if APP
			InitializeComponent();
#endif
			BindingContext = new Issue7792ViewModel();
		}

		protected override void Init()
		{

		}
	}

	[Preserve(AllMembers = true)]
	public class Issue7792Model
	{
		public string Text1 { get; set; }
		public string Text2 { get; set; }
	}

	[Preserve(AllMembers = true)]
	public class Issue7792ViewModel : BindableObject
	{
		public IList<Issue7792Model> EmptyItems { get; private set; }
	}
}