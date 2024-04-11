using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public static class TestCases
	{
		public class TestCaseScreen : TableView
		{
			public static Dictionary<string, Action> PageToAction = new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase);

			bool _filterBugzilla;
			bool _filterNone;
			bool _filterGitHub;
			bool _filterManual;
			string _filter;

			static TextCell MakeIssueCell(string text, string detail, Action tapped)
			{
				PageToAction[text] = tapped;
				if (detail != null)
					PageToAction[detail] = tapped;

				var cell = new TextCell { Text = text, Detail = detail };
				cell.Tapped += (s, e) => tapped();
				return cell;
			}

			Action ActivatePageAndNavigate(IssueAttribute issueAttribute, Type type)
			{
				Action navigationAction = null;

				if (issueAttribute.NavigationBehavior == NavigationBehavior.PushAsync)
				{
					return async () =>
					{
						var page = ActivatePage(type);
						TrackOnInsights(page);
						await Navigation.PushAsync(page);
					};
				}

				if (issueAttribute.NavigationBehavior == NavigationBehavior.PushModalAsync)
				{
					return async () =>
					{
						var page = ActivatePage(type);
						TrackOnInsights(page);
						await Navigation.PushModalAsync(page);
					};
				}

				if (issueAttribute.NavigationBehavior == NavigationBehavior.Default)
				{
					return async () =>
					{
						var page = ActivatePage(type);
						TrackOnInsights(page);
						if (page is ContentPage /*|| page is CarouselPage*/)
						{
							await Navigation.PushAsync(page);
						}
						else if (page is Shell)
						{
							Application.Current.MainPage = page;
						}
						else
						{
							await Navigation.PushModalAsync(page);
						}
					};
				}

				if (issueAttribute.NavigationBehavior == NavigationBehavior.SetApplicationRoot)
				{
					return () =>
					{
						var page = ActivatePage(type);
						TrackOnInsights(page);
						Application.Current.MainPage = page;
					};
				}

				return navigationAction;
			}

			static void TrackOnInsights(Page page)
			{

			}

			Page ActivatePage(Type type)
			{
				var page = Activator.CreateInstance(type) as Page;
				if (page == null)
				{
					throw new InvalidCastException("Issue must be of type Page");
				}
				return page;
			}

			class IssueModel
			{
				public IssueTracker IssueTracker { get; set; }
				public string IssueNumber { get; set; }
				public int IssueTestNumber { get; set; }
				public string Name { get; set; }
				public string Description { get; set; }
				public Action Action { get; set; }

				public bool Matches(string filter)
				{
					if (string.IsNullOrEmpty(filter))
					{
						return true;
					}

					// If the user has typed something which looks like part of a short issue name 
					// (e.g. 'B605' or 'G13'), make sure we match that
					if (string.Compare(Name, 0, filter, 0, filter.Length, StringComparison.OrdinalIgnoreCase) == 0)
					{
						return true;
					}

					if (Description.Contains(filter, StringComparison.OrdinalIgnoreCase))
					{
						return true;
					}

					if (IssueNumber.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase))
					{
						return true;
					}

					return false;
				}
			}

			readonly List<IssueModel> _issues;
			TableSection _section;

			void VerifyNoDuplicates()
			{
				var duplicates = new HashSet<string>();
				_issues.ForEach(im =>
				{
					if (im.IssueTracker != IssueTracker.None &&
					duplicates.Contains(im.Name, StringComparer.OrdinalIgnoreCase) && !IsExempt(im.Name))
					{
						throw new NotSupportedException("Please provide unique tracker + issue number combo: "
							+ im.IssueTracker.ToString() + im.IssueNumber.ToString() + im.IssueTestNumber.ToString());
					}

					duplicates.Add(im.Name);
				});
			}

			public TestCaseScreen()
			{
				AutomationId = "TestCasesIssueList";

				Intent = TableIntent.Settings;

				var assembly = typeof(TestCases).Assembly;

				_issues =
					(from type in assembly.GetTypes()
					 let attribute = type.GetCustomAttribute<IssueAttribute>()
					 where attribute != null
					 select new IssueModel
					 {
						 IssueTracker = attribute.IssueTracker,
						 IssueNumber = attribute.IssueNumber,
						 IssueTestNumber = attribute.IssueTestNumber,
						 Name = attribute.DisplayName,
						 Description = attribute.Description,
						 Action = ActivatePageAndNavigate(attribute, type)
					 }).ToList();

				VerifyNoDuplicates();

				FilterIssues();
			}

			public void FilterTracker(IssueTracker tracker)
			{
				switch (tracker)
				{
					case IssueTracker.Github:
						_filterGitHub = !_filterGitHub;
						break;
					case IssueTracker.Bugzilla:
						_filterBugzilla = !_filterBugzilla;
						break;
					case IssueTracker.ManualTest:
						_filterManual = !_filterManual;
						break;
					case IssueTracker.None:
						_filterNone = !_filterNone;
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(tracker), tracker, null);
				}

				FilterIssues(_filter);
			}

			public bool TryToNavigateTo(string name)
			{
				var issue = _issues.SingleOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
				if (issue == null)
					return false;

				issue.Action();
				return true;
			}

			public async void FilterIssues(string filter = null)
			{
				filter = filter?.Trim();
				_filter = filter;

				await Task.Delay(10);
				if (_filter != filter)
				{
					return;
				}

				PageToAction.Clear();

				var issueCells = Enumerable.Empty<TextCell>();

				if (!_filterBugzilla)
				{
					var bugzillaIssueCells =
						from issueModel in _issues
						where issueModel.IssueTracker == IssueTracker.Bugzilla && issueModel.Matches(filter)
						orderby issueModel.IssueNumber descending
						select MakeIssueCell(issueModel.Name, issueModel.Description, issueModel.Action);

					issueCells = issueCells.Concat(bugzillaIssueCells);
				}

				if (!_filterGitHub)
				{
					var githubIssueCells =
						from issueModel in _issues
						where issueModel.IssueTracker == IssueTracker.Github && issueModel.Matches(filter)
						orderby issueModel.IssueNumber descending
						select MakeIssueCell(issueModel.Name, issueModel.Description, issueModel.Action);

					issueCells = issueCells.Concat(githubIssueCells);
				}

				if (!_filterManual)
				{
					var manualIssueCells =
						from issueModel in _issues
						where issueModel.IssueTracker == IssueTracker.ManualTest && issueModel.Matches(filter)
						orderby issueModel.IssueNumber descending
						select MakeIssueCell(issueModel.Name, issueModel.Description, issueModel.Action);

					issueCells = issueCells.Concat(manualIssueCells);
				}

				if (!_filterNone)
				{
					var untrackedIssueCells =
						from issueModel in _issues
						where issueModel.IssueTracker == IssueTracker.None && issueModel.Matches(filter)
						orderby issueModel.IssueNumber descending, issueModel.Description
						select MakeIssueCell(issueModel.Name, issueModel.Description, issueModel.Action);

					issueCells = issueCells.Concat(untrackedIssueCells);
				}

				if (_section != null)
				{
					Root.Remove(_section);
				}

				_section = new TableSection("Bug Repro");

				foreach (var issueCell in issueCells)
				{
					_section.Add(issueCell);
				}

				Root.Add(_section);
			}

			HashSet<string> _exemptNames = new HashSet<string> { };

			// Legacy reasons, do not add to this list
			// Going forward, make sure only one Issue attribute exist for a Tracker + Issue number pair
			bool IsExempt(string name) => _exemptNames.Contains(name);
		}

		public static NavigationPage GetTestCases()
		{
			TestCaseScreen testCaseScreen = null;
			var rootLayout = new StackLayout();

			var testCasesRoot = new ContentPage
			{
				Title = "Bug Repro's",
				Content = rootLayout
			};

			var searchBar = new SearchBar()
			{
				MinimumHeightRequest = 42, // Need this for Android N, see https://bugzilla.xamarin.com/show_bug.cgi?id=43975
				AutomationId = "SearchBarGo"
			};

			var searchButton = new Button()
			{
				Text = "Search And Go To Issue",
				AutomationId = "SearchButton",
				Command = new Command(() =>
				{
					try
					{
						if (TestCaseScreen.PageToAction.ContainsKey(searchBar.Text?.Trim()))
						{
							TestCaseScreen.PageToAction[searchBar.Text?.Trim()]();
						}
						else if (!testCaseScreen.TryToNavigateTo(searchBar.Text?.Trim()))
						{
							throw new Exception($"Unable to Navigate to {searchBar.Text}");
						}
					}
					catch (Exception e)
					{
						System.Diagnostics.Debug.WriteLine(e.Message);
						Console.WriteLine(e.Message);
					}

				})
			};

			var leaveTestCasesButton = new Button
			{
				AutomationId = "GoBackToGalleriesButton",
				Text = "Go Back to Galleries",
				Command = new Command(() => testCasesRoot.Navigation.PopModalAsync())
			};

			rootLayout.Children.Add(leaveTestCasesButton);
			rootLayout.Children.Add(searchBar);
			rootLayout.Children.Add(searchButton);

			testCaseScreen = new TestCaseScreen();

			rootLayout.Children.Add(CreateTrackerFilter(testCaseScreen));

			rootLayout.Children.Add(testCaseScreen);

			searchBar.TextChanged += (sender, args) => SearchBarOnTextChanged(sender, args, testCaseScreen);

			var page = new NavigationPage(testCasesRoot);
			CoreNavigationPage.InitNavigationPageStyling(page);

			if (Microsoft.Maui.Devices.DeviceInfo.Platform == Microsoft.Maui.Devices.DevicePlatform.iOS ||
			   Microsoft.Maui.Devices.DeviceInfo.Platform == Microsoft.Maui.Devices.DevicePlatform.Android)
			{
				page.Title = "Test Cases";
			}
			else if (Microsoft.Maui.Devices.DeviceInfo.Platform == Microsoft.Maui.Devices.DevicePlatform.WinUI)
			{
				page.Title = "Tests";
			}

			return page;
		}

		static Layout CreateTrackerFilter(TestCaseScreen testCaseScreen)
		{
			var trackerFilterLayout = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.Fill
			};

			var bzSwitch = new Switch { IsToggled = true };
			trackerFilterLayout.Children.Add(new Label { Text = "Bugzilla", VerticalOptions = LayoutOptions.Center });
			trackerFilterLayout.Children.Add(bzSwitch);
			bzSwitch.Toggled += (sender, args) => testCaseScreen.FilterTracker(IssueTracker.Bugzilla);

			var ghSwitch = new Switch { IsToggled = true };
			trackerFilterLayout.Children.Add(new Label { Text = "GitHub", VerticalOptions = LayoutOptions.Center });
			trackerFilterLayout.Children.Add(ghSwitch);
			ghSwitch.Toggled += (sender, args) => testCaseScreen.FilterTracker(IssueTracker.Github);

			var manualSwitch = new Switch { IsToggled = true };
			trackerFilterLayout.Children.Add(new Label { Text = "Manual", VerticalOptions = LayoutOptions.Center });
			trackerFilterLayout.Children.Add(manualSwitch);
			manualSwitch.Toggled += (sender, args) => testCaseScreen.FilterTracker(IssueTracker.ManualTest);

			var noneSwitch = new Switch { IsToggled = true };
			trackerFilterLayout.Children.Add(new Label { Text = "None", VerticalOptions = LayoutOptions.Center });
			trackerFilterLayout.Children.Add(noneSwitch);
			noneSwitch.Toggled += (sender, args) => testCaseScreen.FilterTracker(IssueTracker.None);

			return trackerFilterLayout;
		}

		static void SearchBarOnTextChanged(object sender, TextChangedEventArgs textChangedEventArgs, TestCaseScreen cases)
		{
			var filter = textChangedEventArgs.NewTextValue;

			if (String.IsNullOrEmpty(filter))
			{
				return;
			}

			cases.FilterIssues(filter);
		}
	}

}