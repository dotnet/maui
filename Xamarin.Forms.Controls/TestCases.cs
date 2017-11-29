using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Forms.Controls.TestCasesPages;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls
{
	public static class TestCases
	{
		public class TestCaseScreen : TableView
		{
			public static Dictionary<string, Action> PageToAction = new Dictionary<string, Action> ();

			static TextCell MakeIssueCell (string text, string detail, Action tapped)
			{
				PageToAction[text] = tapped;
				if (detail != null)
					PageToAction[detail] = tapped;

				var cell = new TextCell { Text = text, Detail = detail };
				cell.Tapped += (s, e) => tapped();
				return cell;
			}

			Action ActivatePageAndNavigate (IssueAttribute issueAttribute, Type type)
			{
				Action navigationAction = null;

				if (issueAttribute.NavigationBehavior == NavigationBehavior.PushAsync) {
					return async () => {
						var page = ActivatePage (type);
						TrackOnInsights (page);
						await Navigation.PushAsync (page);
					};
				}

				if (issueAttribute.NavigationBehavior == NavigationBehavior.PushModalAsync) {
					return async () => {
						var page = ActivatePage (type);
						TrackOnInsights (page);
						await Navigation.PushModalAsync (page);
					};
				}

				if (issueAttribute.NavigationBehavior == NavigationBehavior.Default) {
					return async () => {
						var page = ActivatePage (type);
						TrackOnInsights (page);
						if (page is ContentPage || page is CarouselPage) {
							
							await Navigation.PushAsync (page);

						} else {
							await Navigation.PushModalAsync (page);
						}
					}; 
				}

				if (issueAttribute.NavigationBehavior == NavigationBehavior.SetApplicationRoot) {
					return () => {
						var page = ActivatePage (type);
						TrackOnInsights (page);
						Application.Current.MainPage = page;
					};
				}

				return navigationAction;
			}

			static void TrackOnInsights (Page page)
			{
				
			}

			Page ActivatePage (Type type)
			{
				var page = Activator.CreateInstance (type) as Page;
				if (page == null) {
					throw new InvalidCastException ("Issue must be of type Page");
				}
				return page;
			}

			

			public TestCaseScreen ()
			{
				AutomationId = "TestCasesIssueList";

				Intent = TableIntent.Settings;

				var assembly = typeof (TestCases).GetTypeInfo ().Assembly;

				var issueModels = 
					(from typeInfo in assembly.DefinedTypes.Select (o => o.AsType ().GetTypeInfo ())
					where typeInfo.GetCustomAttribute<IssueAttribute> () != null
					let attribute = typeInfo.GetCustomAttribute<IssueAttribute> ()
					select new {
						IssueTracker = attribute.IssueTracker,
						IssueNumber = attribute.IssueNumber,
						IssueTestNumber = attribute.IssueTestNumber,
						Name = attribute.DisplayName,
						Description = attribute.Description,
						Action = ActivatePageAndNavigate (attribute, typeInfo.AsType ())
					}).ToList();

				var root = new TableRoot ();
				var section = new TableSection ("Bug Repro");
				root.Add (section);

				var duplicates = new HashSet<string> ();
				issueModels.ForEach (im =>
				{
					if (duplicates.Contains (im.Name) && !IsExempt (im.Name)) {
						throw new NotSupportedException ("Please provide unique tracker + issue number combo: " 
							+ im.IssueTracker.ToString () + im.IssueNumber.ToString () + im.IssueTestNumber.ToString());
					}

					duplicates.Add (im.Name);
				});

				var githubIssueCells = 
					from issueModel in issueModels
					where issueModel.IssueTracker == IssueTracker.Github
					orderby issueModel.IssueNumber descending
					select MakeIssueCell (issueModel.Name, issueModel.Description, issueModel.Action);

				var bugzillaIssueCells = 
					from issueModel in issueModels
					where issueModel.IssueTracker == IssueTracker.Bugzilla
					orderby issueModel.IssueNumber descending
					select MakeIssueCell (issueModel.Name, issueModel.Description, issueModel.Action);

				var untrackedIssueCells = 
					from issueModel in issueModels
					where issueModel.IssueTracker == IssueTracker.None
					orderby issueModel.IssueNumber descending, issueModel.Description 
					select MakeIssueCell (issueModel.Name, issueModel.Description, issueModel.Action);

				var issueCells = bugzillaIssueCells.Concat (githubIssueCells).Concat (untrackedIssueCells);

				foreach (var issueCell in issueCells) {
					section.Add (issueCell);
				} 

				Root = root;
			}

			// Legacy reasons, do not add to this list
			// Going forward, make sure only one Issue attribute exist for a Tracker + Issue number pair
			bool IsExempt (string name)
			{
				if (name == "G1461" || 
					name == "G342" || 
					name == "G1305" || 
					name == "G1653" || 
					name == "N0")
					return true;
				else
					return false;
			}
		}

		public static NavigationPage GetTestCases ()
		{
			var rootLayout = new StackLayout ();
			
			var testCasesRoot = new ContentPage {
				Title = "Bug Repro's",
				Content = rootLayout
			};

			var searchBar = new SearchBar() {
				AutomationId = "SearchBarGo"
			};

			var searchButton = new Button () {
				Text = "Search And Go To Issue",
				AutomationId = "SearchButton",
				Command = new Command (() => {
					try {
						TestCaseScreen.PageToAction[searchBar.Text] ();
					} catch (Exception e) {
						System.Diagnostics.Debug.WriteLine (e.Message);
					}
					 
				})
			};

			var leaveTestCasesButton = new Button {
				AutomationId = "GoBackToGalleriesButton",
				Text = "Go Back to Galleries",
				Command = new Command (() => testCasesRoot.Navigation.PopModalAsync ())
			};

			rootLayout.Children.Add (leaveTestCasesButton);
			rootLayout.Children.Add (searchBar);
			rootLayout.Children.Add (searchButton);
			rootLayout.Children.Add (new TestCaseScreen ());

			var page = new NavigationPage(testCasesRoot);
			switch (Device.RuntimePlatform) {
			case Device.iOS:
			case Device.Android:
			default:
				page.Title = "Test Cases";
				break;
			case Device.UWP:
				page.Title = "Tests";
				break;
			}
			return page;
		}
	}

}
