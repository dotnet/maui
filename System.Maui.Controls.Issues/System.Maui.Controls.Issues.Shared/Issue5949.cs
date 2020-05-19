using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Maui;
using System.Maui.CustomAttributes;
using System.Maui.Internals;

#if UITEST
using System.Maui.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace System.Maui.Controls.Issues
{

#if UITEST
	[Category(UITestCategories.CollectionView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5949, "CollectionView cannot access a disposed object.",
		PlatformAffected.iOS)]
	public class Issue5949 : TestContentPage
	{
		protected override void Init()
		{
			Appearing += Issue5949Appearing;
		}

		private void Issue5949Appearing(object sender, EventArgs e)
		{
			Application.Current.MainPage = new Issue5949_1();
		}

#if UITEST
		[Test]
		public void DoNotAccessDisposedCollectionView()
		{
			RunningApp.WaitForElement("Login");
			RunningApp.Tap("Login");	
			
			RunningApp.WaitForElement(Issue5949_2.BackButton);
			RunningApp.Tap(Issue5949_2.BackButton);
		
			RunningApp.WaitForElement("Login");
		}
#endif
	}
}