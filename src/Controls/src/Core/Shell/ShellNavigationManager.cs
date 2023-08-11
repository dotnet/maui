#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.Controls
{
	internal class ShellNavigationManager
	{
		readonly Shell _shell;
		ShellNavigatedEventArgs _accumulatedEvent;
		bool _accumulateNavigatedEvents;
		public bool AccumulateNavigatedEvents => _accumulateNavigatedEvents;
		public event EventHandler<ShellNavigatedEventArgs> Navigated;
		public event EventHandler<ShellNavigatingEventArgs> Navigating;

		public ShellNavigationManager(Shell shell)
		{
			_shell = shell;
		}

		public Task GoToAsync(
			ShellNavigationState state,
			bool? animate,
			bool enableRelativeShellRoutes,
			ShellNavigatingEventArgs deferredArgs = null,
			ShellRouteParameters parameters = null,
			bool? canCancel = null)
		{
			return GoToAsync(new ShellNavigationParameters
			{
				TargetState = state,
				Animated = animate,
				EnableRelativeShellRoutes = enableRelativeShellRoutes,
				DeferredArgs = deferredArgs,
				Parameters = parameters,
				CanCancel = canCancel
			});
		}

		public Task GoToAsync(ShellNavigationParameters shellNavigationParameters) =>
			GoToAsync(shellNavigationParameters, null);

		internal async Task GoToAsync(
			ShellNavigationParameters shellNavigationParameters,
			ShellNavigationRequest navigationRequest)
		{
			// check for any pending navigations that need to complete
			if (_shell?.CurrentItem?.CurrentItem?.PendingNavigationTask != null)
				await (_shell?.CurrentItem?.CurrentItem?.PendingNavigationTask ?? Task.CompletedTask);

			if (shellNavigationParameters.PagePushing != null && navigationRequest == null)
				Routing.RegisterImplicitPageRoute(shellNavigationParameters.PagePushing);

			var state = shellNavigationParameters.TargetState ?? new ShellNavigationState(Routing.GetRoute(shellNavigationParameters.PagePushing), false);
			bool? animate = shellNavigationParameters.Animated;
			bool enableRelativeShellRoutes = shellNavigationParameters.EnableRelativeShellRoutes;
			ShellNavigatingEventArgs deferredArgs = shellNavigationParameters.DeferredArgs;

			navigationRequest ??= ShellUriHandler.GetNavigationRequest(_shell, state.FullLocation, enableRelativeShellRoutes, shellNavigationParameters: shellNavigationParameters);

			bool isRelativePopping = ShellUriHandler.IsTargetRelativePop(shellNavigationParameters);
			var parameters = shellNavigationParameters.Parameters ?? new ShellRouteParameters();

			ShellNavigationSource source = CalculateNavigationSource(_shell, _shell.CurrentState, navigationRequest);

			// If the deferredArgs are non null that means we are processing a delayed navigation
			// so the user has indicated they want to go forward with navigation
			// This scenario only comes up from UI iniated navigation (i.e. switching tabs)
			if (deferredArgs == null)
			{
				bool canCancel = (shellNavigationParameters.CanCancel.HasValue) ? shellNavigationParameters.CanCancel.Value : _shell.CurrentState != null;
				var navigatingArgs = ProposeNavigation(source, state, canCancel, animate ?? true);

				if (navigatingArgs != null)
				{
					bool accept = !navigatingArgs.NavigationDelayedOrCancelled;
					if (navigatingArgs.DeferredTask != null)
						accept = await navigatingArgs.DeferredTask;

					if (!accept)
						return;
				}
			}

			Routing.RegisterImplicitPageRoutes(_shell);

			_accumulateNavigatedEvents = true;

			var uri = navigationRequest.Request.FullUri;
			var queryString = navigationRequest.Query;
			parameters.SetQueryStringParameters(queryString);
			ApplyQueryAttributes(_shell, parameters, false, false);

			var shellItem = navigationRequest.Request.Item;
			var shellSection = navigationRequest.Request.Section;
			var currentShellSection = _shell.CurrentItem?.CurrentItem;
			var nextActiveSection = shellSection ?? shellItem?.CurrentItem;

			ShellContent shellContent = navigationRequest.Request.Content;
			bool modalStackPreBuilt = false;

			// check for any pending navigations that need to complete
			if (currentShellSection?.PendingNavigationTask != null)
				await (currentShellSection?.PendingNavigationTask ?? Task.CompletedTask);

			// If we're replacing the whole stack and there are global routes then build the navigation stack before setting the shell section visible
			if (navigationRequest.Request.GlobalRoutes.Count > 0 &&
				nextActiveSection != null &&
				navigationRequest.StackRequest == ShellNavigationRequest.WhatToDoWithTheStack.ReplaceIt)
			{
				modalStackPreBuilt = true;

				bool? isAnimated = (nextActiveSection != currentShellSection) ? false : animate;
				await nextActiveSection.GoToAsync(navigationRequest, parameters, _shell.FindMauiContext()?.Services, isAnimated, isRelativePopping);
			}

			if (shellItem != null)
			{
				ApplyQueryAttributes(shellItem, parameters, navigationRequest.Request.Section == null, false);
				bool navigatedToNewShellElement = false;

				if (shellSection != null && shellContent != null)
				{
					ApplyQueryAttributes(shellContent, parameters, navigationRequest.Request.GlobalRoutes.Count == 0, isRelativePopping);
					if (shellSection.CurrentItem != shellContent)
					{
						shellSection.SetValueFromRenderer(ShellSection.CurrentItemProperty, shellContent);
						navigatedToNewShellElement = true;
					}
				}

				if (shellSection != null)
				{
					ApplyQueryAttributes(shellSection, parameters, navigationRequest.Request.Content == null, false);
					if (shellItem.CurrentItem != shellSection)
					{
						shellItem.SetValueFromRenderer(ShellItem.CurrentItemProperty, shellSection);
						navigatedToNewShellElement = true;
					}
				}

				if (_shell.CurrentItem != shellItem)
				{
					_shell.SetValueFromRenderer(Shell.CurrentItemProperty, shellItem);
					navigatedToNewShellElement = true;
				}

				// Setting the current item isn't an async operation but it triggers an async
				// navigation path. So this waits until that's finished before returning from GotoAsync
				if (_shell?.CurrentItem?.CurrentItem?.PendingNavigationTask != null)
					await (_shell?.CurrentItem?.CurrentItem?.PendingNavigationTask ?? Task.CompletedTask);

				if (!modalStackPreBuilt && currentShellSection?.Navigation.ModalStack.Count > 0)
				{
					// - navigating to new shell element so just pop everything
					// - or route contains no global route requests
					if (navigatedToNewShellElement || navigationRequest.Request.GlobalRoutes.Count == 0)
					{
						// remove all non visible pages first so the transition just smoothly goes from
						// currently visible modal to base page
						if (navigationRequest.Request.GlobalRoutes.Count == 0)
						{
							for (int i = currentShellSection.Stack.Count - 1; i >= 1; i--)
								currentShellSection.Navigation.RemovePage(currentShellSection.Stack[i]);
						}

						await currentShellSection.PopModalStackToPage(null, animate);
					}
				}

				if (navigationRequest.Request.GlobalRoutes.Count > 0 && navigationRequest.StackRequest != ShellNavigationRequest.WhatToDoWithTheStack.ReplaceIt)
				{
					// TODO get rid of this hack and fix so if there's a stack the current page doesn't display
					await _shell.Dispatcher.DispatchAsync(() =>
					{
						return _shell.CurrentItem.CurrentItem.GoToAsync(navigationRequest, parameters, _shell.FindMauiContext()?.Services, animate, isRelativePopping);
					});
				}
				else if (navigationRequest.Request.GlobalRoutes.Count == 0 &&
					navigationRequest.StackRequest == ShellNavigationRequest.WhatToDoWithTheStack.ReplaceIt &&
					nextActiveSection?.Navigation?.NavigationStack?.Count > 1)
				{
					// TODO get rid of this hack and fix so if there's a stack the current page doesn't display
					await _shell.Dispatcher.DispatchAsync(() =>
					{
						return _shell.CurrentItem.CurrentItem.GoToAsync(navigationRequest, parameters, _shell.FindMauiContext()?.Services, animate, isRelativePopping);
					});
				}
			}
			else
			{
				await _shell.CurrentItem.CurrentItem.GoToAsync(navigationRequest, parameters, _shell.FindMauiContext()?.Services, animate, isRelativePopping);
			}

			// Setting the current item isn't an async operation but it triggers an async
			// navigation path. So this waits until that's finished before returning from GotoAsync
			if (_shell?.CurrentItem?.CurrentItem?.PendingNavigationTask != null)
				await (_shell?.CurrentItem?.CurrentItem?.PendingNavigationTask ?? Task.CompletedTask);

			(_shell as IShellController).UpdateCurrentState(source);
			_accumulateNavigatedEvents = false;

			// this can be null in the event that no navigation actually took place!
			if (_accumulatedEvent != null)
				HandleNavigated(_accumulatedEvent);
		}

		ActionDisposable _waitingForWindow;
		public void HandleNavigated(ShellNavigatedEventArgs args)
		{
			_waitingForWindow?.Dispose();
			_waitingForWindow = null;

			// we don't want to fire Navigated until shell is attached to an actual window
			if (_shell.Window == null || _shell.CurrentPage == null)
			{
				_shell.PropertyChanged += WaitForWindowToSet;
				var shellContent = _shell?.CurrentItem?.CurrentItem?.CurrentItem;

				if (shellContent != null)
					shellContent.ChildAdded += WaitForWindowToSet;

				_waitingForWindow = new ActionDisposable(() =>
				{
					_shell.PropertyChanged -= WaitForWindowToSet;
					if (shellContent != null)
						shellContent.ChildAdded -= WaitForWindowToSet;
				});

				void WaitForWindowToSet(object sender, EventArgs e)
				{
					if (_shell.Window != null &&
						_shell.CurrentPage != null)
					{
						_waitingForWindow?.Dispose();
						_waitingForWindow = null;

						_shell.CurrentItem?.SendAppearing();
						HandleNavigated(args);
					}
				}

				return;
			}

			if (AccumulateNavigatedEvents)
			{
				if (_accumulatedEvent == null)
					_accumulatedEvent = args;
			}
			else
			{
				_accumulatedEvent = null;
				BaseShellItem baseShellItem = _shell.CurrentItem?.CurrentItem?.CurrentItem;

				if (baseShellItem != null)
				{
					baseShellItem.OnAppearing(() =>
					{
						FireNavigatedEvents(args, _shell);
					});
				}
				else
				{
					FireNavigatedEvents(args, _shell);
				}

				void FireNavigatedEvents(ShellNavigatedEventArgs a, Shell shell)
				{
					Navigated?.Invoke(this, args);
					// reset active page route tree
					Routing.ClearImplicitPageRoutes();
					Routing.RegisterImplicitPageRoutes(_shell);
				}
			}
		}

		public static void ApplyQueryAttributes(Element element, ShellRouteParameters query, bool isLastItem, bool isPopping)
		{
			string prefix = "";
			if (!isLastItem)
			{
				var route = Routing.GetRoute(element);
				if (string.IsNullOrEmpty(route) || Routing.IsImplicit(route))
					return;
				prefix = route + ".";
			}

			//if the lastItem is implicitly wrapped, get the actual ShellContent
			if (isLastItem)
			{
				if (element is IShellItemController shellitem && shellitem.GetItems().FirstOrDefault() is ShellSection section)
					element = section;
				if (element is IShellSectionController shellsection && shellsection.GetItems().FirstOrDefault() is ShellContent content)
					element = content;
				if (element is ShellContent shellcontent && shellcontent.Content is Element e)
					element = e;
			}

			if (!(element is BaseShellItem baseShellItem))
				baseShellItem = element?.Parent as BaseShellItem;

			//filter the query to only apply the keys with matching prefix
			var filteredQuery = new ShellRouteParameters(query, prefix);


			if (baseShellItem is ShellContent)
				baseShellItem.ApplyQueryAttributes(MergeData(element, filteredQuery, isPopping));
			else if (isLastItem)
				element.SetValue(ShellContent.QueryAttributesProperty, MergeData(element, query, isPopping));

			ShellRouteParameters MergeData(Element shellElement, ShellRouteParameters data, bool isPopping)
			{
				if (!isPopping)
					return data;

				var returnValue = new ShellRouteParameters(data);

				var existing = (ShellRouteParameters)shellElement.GetValue(ShellContent.QueryAttributesProperty);

				if (existing == null)
					return data;

				foreach (var datum in existing)
				{
					if (!returnValue.ContainsKey(datum.Key))
						returnValue[datum.Key] = datum.Value;
				}

				return returnValue;
			}
		}

		// This is used for cases where the user is navigating via native UI navigation i.e. clicking on Tabs
		// If the user defers this type of navigation we generate the equivalent GotoAsync call
		// so when the deferral is completed the same navigation can complete
		public bool ProposeNavigationOutsideGotoAsync(
			ShellNavigationSource source,
			ShellItem shellItem,
			ShellSection shellSection,
			ShellContent shellContent,
			IReadOnlyList<Page> stack,
			bool canCancel,
			bool isAnimated)
		{
			if (AccumulateNavigatedEvents)
				return true;

			var proposedState = GetNavigationState(shellItem, shellSection, shellContent, stack, shellSection.Navigation.ModalStack);
			var navArgs = ProposeNavigation(source, proposedState, canCancel, isAnimated);

			if (navArgs.DeferralCount > 0)
			{
				navArgs.RegisterDeferralCompletedCallBack(async () =>
				{
					if (navArgs.Cancelled)
					{
						return;
					}

					Func<Task> navigationTask = () => GoToAsync(navArgs.Target, navArgs.Animate, false, navArgs);

					await _shell
						.FindDispatcher()
						.DispatchIfRequiredAsync(navigationTask);
				});
			}

			return !navArgs.NavigationDelayedOrCancelled;
		}

		ShellNavigatingEventArgs ProposeNavigation(
			ShellNavigationSource source,
			ShellNavigationState proposedState,
			bool canCancel,
			bool isAnimated)
		{
			if (AccumulateNavigatedEvents)
				return null;

			var navArgs = new ShellNavigatingEventArgs(_shell.CurrentState, proposedState, source, canCancel)
			{
				Animate = isAnimated
			};

			HandleNavigating(navArgs);

			return navArgs;
		}

		public void HandleNavigating(ShellNavigatingEventArgs args)
		{
			if (!args.DeferredEventArgs)
			{
				Navigating?.Invoke(this, args);
			}
			else
			{
				return;
			}
		}

		public static ShellNavigationSource CalculateNavigationSource(Shell shell, ShellNavigationState current, ShellNavigationRequest request)
		{
			if (request.StackRequest == ShellNavigationRequest.WhatToDoWithTheStack.PushToIt)
				return ShellNavigationSource.Push;

			if (current == null)
				return ShellNavigationSource.ShellItemChanged;

			var targetUri = ShellUriHandler.ConvertToStandardFormat(shell, request.Request.FullUri);
			var currentUri = ShellUriHandler.ConvertToStandardFormat(shell, current.FullLocation);

			var targetPaths = ShellUriHandler.RetrievePaths(targetUri.PathAndQuery);
			var currentPaths = ShellUriHandler.RetrievePaths(currentUri.PathAndQuery);

			var targetPathsLength = targetPaths.Length;
			var currentPathsLength = currentPaths.Length;

			if (targetPathsLength < 4 || currentPathsLength < 4)
				return ShellNavigationSource.Unknown;

			if (targetPaths[1] != currentPaths[1])
				return ShellNavigationSource.ShellItemChanged;
			if (targetPaths[2] != currentPaths[2])
				return ShellNavigationSource.ShellSectionChanged;
			if (targetPaths[3] != currentPaths[3])
				return ShellNavigationSource.ShellContentChanged;

			if (targetPathsLength == currentPathsLength)
				return ShellNavigationSource.Unknown;

			if (targetPathsLength < currentPathsLength)
			{
				for (var i = 0; i < targetPathsLength; i++)
				{
					var targetPath = targetPaths[i];
					if (targetPath != currentPaths[i])
						break;

					if (i == targetPathsLength - 1)
					{
						if (targetPathsLength == 4)
							return ShellNavigationSource.PopToRoot;

						return ShellNavigationSource.Pop;
					}
				}

				if (targetPaths[targetPathsLength - 1] == currentPaths[currentPathsLength - 1])
					return ShellNavigationSource.Remove;

				if (targetPathsLength == 4)
					return ShellNavigationSource.PopToRoot;

				return ShellNavigationSource.Pop;
			}
			else if (targetPathsLength > currentPathsLength)
			{
				for (var i = 0; i < currentPathsLength; i++)
				{
					if (targetPaths[i] != currentPaths[i])
						break;

					if (i == targetPathsLength - 1)
						return ShellNavigationSource.Push;
				}
			}

			if (targetPaths[targetPathsLength - 1] == currentPaths[currentPathsLength - 1])
				return ShellNavigationSource.Insert;

			return ShellNavigationSource.Push;
		}

		public static ShellNavigationParameters GetNavigationParameters(
			ShellItem shellItem,
			ShellSection shellSection,
			ShellContent shellContent,
			IReadOnlyList<Page> sectionStack,
			IReadOnlyList<Page> modalStack)
		{
			var state = GetNavigationState(shellItem, shellSection, shellContent, sectionStack, modalStack);
			var navStack = shellSection.Navigation.NavigationStack;

			var topNavStackPage =
				(modalStack?.Count > 0 ? modalStack[modalStack.Count - 1] : null) ??
				(navStack?.Count > 0 ? navStack[navStack.Count - 1] : null);

			var queryParametersTarget =
				topNavStackPage as BindableObject ??
				(shellContent?.Content as BindableObject) ??
				shellContent;

			ShellRouteParameters routeParameters = null;

			if (queryParametersTarget?.GetValue(ShellContent.QueryAttributesProperty) is
				ShellRouteParameters shellRouteParameters)
			{
				routeParameters = shellRouteParameters;
			}

			return new ShellNavigationParameters()
			{
				TargetState = state,
				Animated = false,
				EnableRelativeShellRoutes = false,
				DeferredArgs = null,
				Parameters = routeParameters
			};
		}

		public static ShellNavigationState GetNavigationState(ShellItem shellItem, ShellSection shellSection, ShellContent shellContent, IReadOnlyList<Page> sectionStack, IReadOnlyList<Page> modalStack)
		{
			List<string> routeStack = new List<string>();

			bool stackAtRoot = sectionStack == null || sectionStack.Count <= 1;
			bool hasUserDefinedRoute =
				(Routing.IsUserDefined(shellItem)) ||
				(Routing.IsUserDefined(shellSection)) ||
				(Routing.IsUserDefined(shellContent));

			if (shellItem != null)
			{
				var shellItemRoute = shellItem.Route;
				routeStack.Add(shellItemRoute);

				if (shellSection != null)
				{
					var shellSectionRoute = shellSection.Route;
					routeStack.Add(shellSectionRoute);

					if (shellContent != null)
					{
						var shellContentRoute = shellContent.Route;
						routeStack.Add(shellContentRoute);
					}

					if (!stackAtRoot)
					{
						for (int i = 1; i < sectionStack.Count; i++)
						{
							var page = sectionStack[i];
							routeStack.AddRange(ShellUriHandler.CollapsePath(Routing.GetRoute(page), routeStack, hasUserDefinedRoute));
						}
					}

					if (modalStack != null && modalStack.Count > 0)
					{
						for (int i = 0; i < modalStack.Count; i++)
						{
							var topPage = modalStack[i];

							routeStack.AddRange(ShellUriHandler.CollapsePath(Routing.GetRoute(topPage), routeStack, hasUserDefinedRoute));

							for (int j = 1; j < topPage.Navigation.NavigationStack.Count; j++)
							{
								routeStack.AddRange(ShellUriHandler.CollapsePath(Routing.GetRoute(topPage.Navigation.NavigationStack[j]), routeStack, hasUserDefinedRoute));
							}
						}
					}
				}
			}

			if (routeStack.Count > 0)
				routeStack.Insert(0, "/");

			return new ShellNavigationState(String.Join("/", routeStack), true);
		}

		public static List<Page> BuildFlattenedNavigationStack(Shell shell)
		{
			var section = shell.CurrentItem.CurrentItem;
			return BuildFlattenedNavigationStack(section.Stack, section.Navigation.ModalStack);
		}

		public static List<Page> BuildFlattenedNavigationStack(IReadOnlyList<Page> startingList, IReadOnlyList<Page> modalStack)
		{
			var returnValue = startingList.ToList();
			if (modalStack == null)
				return returnValue;

			for (int i = 0; i < modalStack.Count; i++)
			{
				returnValue.Add(modalStack[i]);
				for (int j = 1; j < modalStack[i].Navigation.NavigationStack.Count; j++)
				{
					returnValue.Add(modalStack[i].Navigation.NavigationStack[j]);
				}
			}

			return returnValue;
		}
	}
}
