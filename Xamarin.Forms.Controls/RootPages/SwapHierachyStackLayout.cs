using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	internal sealed class SwapRootButton : Button
	{

		public string PageStyleId { get; private set; }

		public SwapRootButton (string hierarchyDescription, Command command)
		{
			AutomationId = hierarchyDescription + "ButtonId";
			Text = hierarchyDescription;

			Command = command;
		}
	}

	internal class SwapHierachyStackLayout : ScrollView
	{
		public SwapHierachyStackLayout (string heirarchy)
		{
			AutomationId = "ChoosePageScrollView";

			BackgroundColor = Color.Blue;

			var buttons = new [] {
				new SwapRootButton ("Content", new Command (() => Application.Current.MainPage = new RootContentPage ("Content"))),
				new SwapRootButton ("Nav->Content", new Command (() => Application.Current.MainPage = new RootNavigationContentPage ("Nav->Content"))),
				new SwapRootButton ("MDP->Nav->Content", new Command (() => Application.Current.MainPage = new RootMDPNavigationContentPage ("MDP->Nav->Content"))),
				new SwapRootButton ("Tab->Content", new Command (() => Application.Current.MainPage = new RootTabbedContentPage ("Tab->Content"))),
				new SwapRootButton ("Tab->MDP->Nav->Content", new Command (() => Application.Current.MainPage = new RootTabbedMDPNavigationContentPage ("Tab->MDP->Nav->Content"))),
				new SwapRootButton ("Tab->Nav->Content", new Command (() => Application.Current.MainPage = new RootTabbedNavigationContentPage ("Tab->Nav->Content"))),
				new SwapRootButton ("Tab(Many)->Nav->Content", new Command (() => Application.Current.MainPage = new RootTabbedManyNavigationContentPage ("Tab(Many)->Nav->Content"))),
				// tsk tsk
				new SwapRootButton ("Nav->Tab->Content(BAD IDEA)", new Command (() => Application.Current.MainPage = new RootNavigationTabbedContentPage ("Nav->Tab->Content(BAD IDEA)"))),
				new SwapRootButton ("Nav->Tab(Many)->Content(BAD IDEA)", new Command (() => Application.Current.MainPage = new RootNavigationManyTabbedPage ("Nav->Tab(Many)->Content(BAD IDEA)"))),
				new SwapRootButton ("MDP->Nav->Tab->Content(BAD IDEA)", new Command (() => Application.Current.MainPage = new RootMDPNavigationTabbedContentPage ("MDP->Nav->Tab->Content(BAD IDEA)"))),
				// modals
				new SwapRootButton ("(Modal)Content", new Command (async () => await Application.Current.MainPage.Navigation.PushModalAsync(new RootContentPage ("(Modal)Content")))),
				new SwapRootButton ("(Modal)Nav->Content", new Command (async () => await Application.Current.MainPage.Navigation.PushModalAsync(new RootNavigationContentPage ("(Modal)Nav->Content")))),
				new SwapRootButton ("(Modal)MDP->Nav->Content", new Command (async () => await Application.Current.MainPage.Navigation.PushModalAsync(new RootMDPNavigationContentPage ("(Modal)MDP->Nav->Content")))),
				new SwapRootButton ("(Modal)Tab->Content", new Command (async () => await Application.Current.MainPage.Navigation.PushModalAsync(new RootTabbedContentPage ("(Modal)Tab->Content")))),
				new SwapRootButton ("(Modal)Tab->MDP->Nav->Content", new Command (async () => await Application.Current.MainPage.Navigation.PushModalAsync(new RootTabbedMDPNavigationContentPage ("(Modal)Tab->MDP->Nav->Content")))),
				new SwapRootButton ("(Modal)Tab->Nav->Content", new Command (async () => await Application.Current.MainPage.Navigation.PushModalAsync(new RootTabbedNavigationContentPage ("(Modal)Tab->Nav->Content")))),
				new SwapRootButton ("(Modal)Tab(Many)->Nav->Content", new Command (async () => await Application.Current.MainPage.Navigation.PushModalAsync(new RootTabbedManyNavigationContentPage ("(Modal)Tab(Many)->Nav->Content")))),
				// tsk tsk
				new SwapRootButton ("(Modal)Nav->Tab->Content(BAD IDEA)", new Command (async () => await Application.Current.MainPage.Navigation.PushModalAsync(new RootNavigationTabbedContentPage ("(Modal)Nav->Tab->Content(BAD IDEA)")))),
				new SwapRootButton ("(Modal)Nav->Tab(Many)->Content(BAD IDEA)", new Command (async () => await Application.Current.MainPage.Navigation.PushModalAsync(new RootNavigationManyTabbedPage ("(Modal)Nav->Tab(Many)->Content(BAD IDEA)")))),
				new SwapRootButton ("(Modal)MDP->Nav->Tab->Content(BAD IDEA)", new Command (async () => await Application.Current.MainPage.Navigation.PushModalAsync(new RootMDPNavigationTabbedContentPage ("(Modal)MDP->Nav->Tab->Content(BAD IDEA)")))),
				new SwapRootButton ("(Modal)CoreGallery", new Command (() => Application.Current.MainPage = CoreGallery.GetMainPage ()))
			};

			var layout = new StackLayout ();

			layout.Children.Add (new Label { Text = heirarchy });

			foreach (var button in buttons) {
				layout.Children.Add (button);
			}

			Content = layout;
		}
	}
}
