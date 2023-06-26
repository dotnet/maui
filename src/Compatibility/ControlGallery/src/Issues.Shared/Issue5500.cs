using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5500, "[iOS] Editor with material visuals value binding not working on physical device",
		PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Editor)]
#endif
	public class Issue5500 : TestContentPage
	{
		Editor editor;
		Entry entry;

		protected override void Init()
		{
			Visual = VisualMarker.Material;

			editor = new Editor();
			entry = new Entry();

			editor.SetBinding(Editor.TextProperty, "Text");
			editor.BindingContext = entry;
			editor.Placeholder = "Editor";
			editor.AutoSize = EditorAutoSizeOption.TextChanges;
			editor.AutomationId = "EditorAutomationId";

			entry.SetBinding(Entry.TextProperty, "Text");
			entry.BindingContext = editor;
			entry.Placeholder = "Entry";
			entry.AutomationId = "EntryAutomationId";

			Content = new StackLayout()
			{
				Children =
				{
					new Label(){ Text = "Typing into either text field should change the other field to match" },
					entry,
					editor
				}
			};
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			Device.BeginInvokeOnMainThread(GarbageCollectionHelper.Collect);
		}


#if UITEST && __IOS__
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void VerifyEditorTextChangeEventsAreFiring()
		{
			RunningApp.WaitForElement("EditorAutomationId");
			RunningApp.EnterText("EditorAutomationId", "Test 1");

			Assert.AreEqual("Test 1", RunningApp.WaitForElement("EditorAutomationId")[0].ReadText());
			Assert.AreEqual("Test 1", RunningApp.WaitForElement("EntryAutomationId")[0].ReadText());

			RunningApp.ClearText("EntryAutomationId");
			RunningApp.EnterText("EntryAutomationId", "Test 2");

			Assert.AreEqual("Test 2", RunningApp.WaitForElement("EditorAutomationId")[0].ReadText());
			Assert.AreEqual("Test 2", RunningApp.WaitForElement("EntryAutomationId")[0].ReadText());
		}
#endif
	}
}
