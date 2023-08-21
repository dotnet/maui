//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System.Collections.Generic;
using System.Windows.Input;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.UITest.iOS;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
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