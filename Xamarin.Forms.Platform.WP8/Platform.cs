using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.WinPhone
{
	// from mono 
	public class Platform : BindableObject, IPlatform, INavigation
	{
		internal static readonly BindableProperty RendererProperty = BindableProperty.CreateAttached("Renderer", typeof(IVisualElementRenderer), typeof(Platform), default(IVisualElementRenderer));

		readonly TurnstileTransition _backwardInTransition = new TurnstileTransition { Mode = TurnstileTransitionMode.BackwardIn };

		readonly TurnstileTransition _backwardOutTransition = new TurnstileTransition { Mode = TurnstileTransitionMode.BackwardOut };

		readonly TurnstileTransition _forwardInTransition = new TurnstileTransition { Mode = TurnstileTransitionMode.ForwardIn };

		readonly TurnstileTransition _forwardOutTransition = new TurnstileTransition { Mode = TurnstileTransitionMode.ForwardOut };

		readonly NavigationModel _navModel = new NavigationModel();

		readonly PhoneApplicationPage _page;

		readonly Canvas _renderer;
		readonly ToolbarTracker _tracker = new ToolbarTracker();

		Page _currentDisplayedPage;
		CustomMessageBox _visibleMessageBox;

		internal Platform(PhoneApplicationPage page)
		{
			_tracker.SeparateMasterDetail = true;

			page.BackKeyPress += OnBackKeyPress;
			_page = page;

			_renderer = new Canvas();
			_renderer.SizeChanged += RendererSizeChanged;
			_renderer.Loaded += (sender, args) => UpdateSystemTray();

			_tracker.CollectionChanged += (sender, args) => UpdateToolbarItems();

			ProgressIndicator indicator;
			SystemTray.SetProgressIndicator(page, indicator = new ProgressIndicator { IsVisible = false, IsIndeterminate = true });

			var busyCount = 0;
			MessagingCenter.Subscribe(this, Page.BusySetSignalName, (Page sender, bool enabled) =>
			{
				busyCount = Math.Max(0, enabled ? busyCount + 1 : busyCount - 1);
				indicator.IsVisible = busyCount > 0;
			});

			MessagingCenter.Subscribe(this, Page.AlertSignalName, (Page sender, AlertArguments arguments) =>
			{
				var messageBox = new CustomMessageBox { Title = arguments.Title, Message = arguments.Message };
				if (arguments.Accept != null)
					messageBox.LeftButtonContent = arguments.Accept;
				messageBox.RightButtonContent = arguments.Cancel;
				messageBox.Show();
				_visibleMessageBox = messageBox;
				messageBox.Dismissed += (o, args) =>
				{
					arguments.SetResult(args.Result == CustomMessageBoxResult.LeftButton);
					_visibleMessageBox = null;
				};
			});

			MessagingCenter.Subscribe(this, Page.ActionSheetSignalName, (Page sender, ActionSheetArguments arguments) =>
			{
				var messageBox = new CustomMessageBox { Title = arguments.Title };

				var listBox = new ListBox { FontSize = 36, Margin = new System.Windows.Thickness(12) };
				var itemSource = new List<string>();

				if (!string.IsNullOrWhiteSpace(arguments.Destruction))
					itemSource.Add(arguments.Destruction);
				itemSource.AddRange(arguments.Buttons);
				if (!string.IsNullOrWhiteSpace(arguments.Cancel))
					itemSource.Add(arguments.Cancel);

				listBox.ItemsSource = itemSource.Select(s => new TextBlock { Text = s, Margin = new System.Windows.Thickness(0, 12, 0, 12) });
				messageBox.Content = listBox;

				listBox.SelectionChanged += (o, args) => messageBox.Dismiss();
				messageBox.Dismissed += (o, args) =>
				{
					string result = listBox.SelectedItem != null ? ((TextBlock)listBox.SelectedItem).Text : null;
					arguments.SetResult(result);
					_visibleMessageBox = null;
				};

				messageBox.Show();
				_visibleMessageBox = messageBox;
			});
		}

		internal Size Size
		{
			get { return new Size(_renderer.ActualWidth, _renderer.ActualHeight); }
		}

		Page Page { get; set; }

		void INavigation.InsertPageBefore(Page page, Page before)
		{
			_navModel.InsertPageBefore(page, before);
		}

		IReadOnlyList<Page> INavigation.ModalStack
		{
			get { return _navModel.Roots.ToList(); }
		}

		IReadOnlyList<Page> INavigation.NavigationStack
		{
			get { return _navModel.Tree.Last(); }
		}

		Task<Page> INavigation.PopAsync()
		{
			return ((INavigation)this).PopAsync(true);
		}

		Task<Page> INavigation.PopAsync(bool animated)
		{
			return Pop(Page, animated);
		}

		Task<Page> INavigation.PopModalAsync()
		{
			return ((INavigation)this).PopModalAsync(true);
		}

		Task<Page> INavigation.PopModalAsync(bool animated)
		{
			var tcs = new TaskCompletionSource<Page>();
			Page result = _navModel.PopModal();

			IReadOnlyList<Page> last = _navModel.Tree.Last();
			IEnumerable<Page> stack = last;
			if (last.Count > 1)
				stack = stack.Skip(1);

			Page navRoot = stack.First();
			Page current = _navModel.CurrentPage;
			if (current == navRoot)
				current = _navModel.Roots.Last(); // Navigation page itself, since nav root has a host

			SetCurrent(current, animated, true, () => tcs.SetResult(result));
			return tcs.Task;
		}

		Task INavigation.PopToRootAsync()
		{
			return ((INavigation)this).PopToRootAsync(true);
		}

		async Task INavigation.PopToRootAsync(bool animated)
		{
			await PopToRoot(Page, animated);
		}

		Task INavigation.PushAsync(Page root)
		{
			return ((INavigation)this).PushAsync(root, true);
		}

		Task INavigation.PushAsync(Page root, bool animated)
		{
			return Push(root, Page, animated);
		}

		Task INavigation.PushModalAsync(Page modal)
		{
			return ((INavigation)this).PushModalAsync(modal, true);
		}

		Task INavigation.PushModalAsync(Page modal, bool animated)
		{
			var tcs = new TaskCompletionSource<object>();
			_navModel.PushModal(modal);
			SetCurrent(_navModel.CurrentPage, animated, completedCallback: () => tcs.SetResult(null));
			return tcs.Task;
		}

		void INavigation.RemovePage(Page page)
		{
			RemovePage(page, true);
		}

		SizeRequest IPlatform.GetNativeSize(VisualElement view, double widthConstraint, double heightConstraint)
		{
			// Hack around the fact that Canvas ignores the child constraints.
			// It is entirely possible using Canvas as our base class is not wise.
			// FIXME: This should not be an if statement. Probably need to define an interface here.
			if (widthConstraint > 0 && heightConstraint > 0 && GetRenderer(view) != null)
			{
				IVisualElementRenderer element = GetRenderer(view);
				return element.GetDesiredSize(widthConstraint, heightConstraint);
			}

			return new SizeRequest();
		}

		public static IVisualElementRenderer CreateRenderer(VisualElement element)
		{
			IVisualElementRenderer result = Registrar.Registered.GetHandlerForObject<IVisualElementRenderer>(element) ?? new ViewRenderer();
			result.SetElement(element);
			return result;
		}

		public static IVisualElementRenderer GetRenderer(VisualElement self)
		{
			return (IVisualElementRenderer)self.GetValue(RendererProperty);
		}

		public static void SetRenderer(VisualElement self, IVisualElementRenderer renderer)
		{
			self.SetValue(RendererProperty, renderer);
			self.IsPlatformEnabled = renderer != null;
		}

		internal Canvas GetCanvas()
		{
			return _renderer;
		}

		internal async Task<Page> Pop(Page ancestor, bool animated)
		{
			Page result = _navModel.Pop(ancestor);

			Page navRoot = _navModel.Tree.Last().Skip(1).First();
			Page current = _navModel.CurrentPage;

			// The following code is a terrible horrible ugly hack that we are kind of stuck with for the time being
			// Effectively what can happen is a TabbedPage with many navigation page children needs to have all those children in the
			// nav stack. If you have multiple each of those roots needs to be skipped over.

			// In general the check for the NavigationPage will always hit if the check for the Skip(1) hits, but since that check
			// was always there it is left behind to ensure compatibility with previous behavior.
			bool replaceWithRoot = current == navRoot;
			var parent = current.Parent as NavigationPage;
			if (parent != null)
			{
				if (((IPageController)parent).InternalChildren[0] == current)
					replaceWithRoot = true;
			}

			if (replaceWithRoot)
				current = _navModel.Roots.Last(); // Navigation page itself, since nav root has a host

			await SetCurrent(current, animated, true);
			return result;
		}

		internal async Task PopToRoot(Page ancestor, bool animated)
		{
			_navModel.PopToRoot(ancestor);
			await SetCurrent(_navModel.CurrentPage, animated, true);
		}

		internal async Task PushCore(Page root, Page ancester, bool animated, bool realize = true)
		{
			_navModel.Push(root, ancester);
			if (realize)
				await SetCurrent(_navModel.CurrentPage, animated);

			if (root.NavigationProxy.Inner == null)
				root.NavigationProxy.Inner = this;
		}

		internal async void RemovePage(Page page, bool popCurrent)
		{
			if (popCurrent && _navModel.CurrentPage == page)
				await ((INavigation)this).PopAsync();
			else
				_navModel.RemovePage(page);
		}

		internal Task SetCurrent(Page page, bool animated, bool popping = false, Action completedCallback = null)
		{
			var tcs = new TaskCompletionSource<bool>();
			if (page == _currentDisplayedPage)
			{
				tcs.SetResult(true);
				return tcs.Task;
			}

			if (!animated)
				tcs.SetResult(true);

			page.Platform = this;

			if (GetRenderer(page) == null)
				SetRenderer(page, CreateRenderer(page));

			page.Layout(new Rectangle(0, 0, _renderer.ActualWidth, _renderer.ActualHeight));
			IVisualElementRenderer pageRenderer = GetRenderer(page);
			if (pageRenderer != null)
			{
				((FrameworkElement)pageRenderer.ContainerElement).Width = _renderer.ActualWidth;
				((FrameworkElement)pageRenderer.ContainerElement).Height = _renderer.ActualHeight;
			}

			Page current = _currentDisplayedPage;
			UIElement currentElement = null;
			if (current != null)
				currentElement = (UIElement)GetRenderer(current);

			if (popping)
			{
				ITransition transitionOut = null;
				if (current != null)
				{
					if (animated)
						transitionOut = _backwardOutTransition.GetTransition(currentElement);
					else
						_renderer.Children.Remove(currentElement);
				}

				var pageElement = (UIElement)GetRenderer(page);

				if (animated)
				{
					transitionOut.Completed += (s, e) =>
					{
						transitionOut.Stop();
						_renderer.Children.Remove(currentElement);
						UpdateToolbarTracker();

						_renderer.Children.Add(pageElement);

						ITransition transitionIn = _backwardInTransition.GetTransition(pageElement);
						transitionIn.Completed += (si, ei) =>
						{
							transitionIn.Stop();
							if (completedCallback != null)
								completedCallback();

							tcs.SetResult(true);
						};
						transitionIn.Begin();
					};

					transitionOut.Begin();
				}
				else
				{
					UpdateToolbarTracker();
					_renderer.Children.Add(pageElement);
					if (completedCallback != null)
						completedCallback();
				}
			}
			else
			{
				ITransition transitionOut = null;
				if (current != null)
				{
					if (animated)
						transitionOut = _forwardOutTransition.GetTransition(currentElement);
					else
						_renderer.Children.Remove(currentElement);
				}

				if (animated)
				{
					if (transitionOut != null)
					{
						transitionOut.Completed += (o, e) =>
						{
							_renderer.Children.Remove(currentElement);
							transitionOut.Stop();

							UpdateToolbarTracker();

							var element = (UIElement)GetRenderer(page);
							_renderer.Children.Add(element);
							ITransition transitionIn = _forwardInTransition.GetTransition(element);
							transitionIn.Completed += (s, ie) =>
							{
								transitionIn.Stop();
								if (completedCallback != null)
									completedCallback();
								tcs.SetResult(true);
							};
							transitionIn.Begin();
						};

						transitionOut.Begin();
					}
					else
					{
						UpdateToolbarTracker();

						_renderer.Children.Add((UIElement)GetRenderer(page));
						ITransition transitionIn = _forwardInTransition.GetTransition((UIElement)GetRenderer(page));
						transitionIn.Completed += (s, e) =>
						{
							transitionIn.Stop();
							if (completedCallback != null)
								completedCallback();

							tcs.SetResult(true);
						};
						transitionIn.Begin();
					}
				}
				else
				{
					_renderer.Children.Add((UIElement)GetRenderer(page));
					UpdateToolbarTracker();
					if (completedCallback != null)
						completedCallback();
				}
			}

			_currentDisplayedPage = page;

			return tcs.Task;
		}

		internal void SetPage(Page newRoot)
		{
			if (newRoot == null)
				return;

			Page = newRoot;
			_navModel.Clear();
			_navModel.PushModal(newRoot);
			SetCurrent(newRoot, false, true);

			Application.Current.NavigationProxy.Inner = this;
		}

		internal event EventHandler SizeChanged;

		void OnBackKeyPress(object sender, CancelEventArgs e)
		{
			if (_visibleMessageBox != null)
			{
				_visibleMessageBox.Dismiss();
				e.Cancel = true;
				return;
			}

			Page lastRoot = _navModel.Roots.Last();

			bool handled = lastRoot.SendBackButtonPressed();

			e.Cancel = handled;
		}

		Task Push(Page root, Page ancester, bool animated)
		{
			return PushCore(root, ancester, animated);
		}

		void RendererSizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateFormSizes();
			EventHandler handler = SizeChanged;
			if (handler != null)
				handler(this, EventArgs.Empty);
		}

		void UpdateFormSizes()
		{
			foreach (Page f in _navModel.Roots)
			{
				f.Layout(new Rectangle(0, 0, _renderer.ActualWidth, _renderer.ActualHeight));
#pragma warning disable 618
				IVisualElementRenderer pageRenderer = f.GetRenderer();
#pragma warning restore 618
				if (pageRenderer != null)
				{
					((FrameworkElement)pageRenderer.ContainerElement).Width = _renderer.ActualWidth;
					((FrameworkElement)pageRenderer.ContainerElement).Height = _renderer.ActualHeight;
				}
			}
		}

		void UpdateSystemTray()
		{
			var lightThemeVisibility = (Visibility)System.Windows.Application.Current.Resources["PhoneLightThemeVisibility"];
			if (lightThemeVisibility == Visibility.Visible && SystemTray.BackgroundColor == System.Windows.Media.Color.FromArgb(0, 0, 0, 0))
			{
				SystemTray.BackgroundColor = System.Windows.Media.Color.FromArgb(1, 255, 255, 255);
			}
		}

		void UpdateToolbarItems()
		{
			if (_page.ApplicationBar == null)
				_page.ApplicationBar = new ApplicationBar();

			ToolbarItem[] items = _tracker.ToolbarItems.ToArray();
			MasterDetailPage masterDetail = _tracker.Target.Descendants().Prepend(_tracker.Target).OfType<MasterDetailPage>().FirstOrDefault();

			TaggedAppBarButton oldMasterDetailButton = _page.ApplicationBar.Buttons.OfType<TaggedAppBarButton>().FirstOrDefault(b => b.Tag is MasterDetailPage && b.Tag != masterDetail);

			if (oldMasterDetailButton != null)
				_page.ApplicationBar.Buttons.Remove(oldMasterDetailButton);

			if (masterDetail != null)
			{
				if (masterDetail.ShouldShowToolbarButton())
				{
					if (_page.ApplicationBar.Buttons.OfType<TaggedAppBarButton>().All(b => b.Tag != masterDetail))
					{
						var button = new TaggedAppBarButton
						{
							IconUri = new Uri(masterDetail.Master.Icon ?? "ApplicationIcon.jpg", UriKind.Relative),
							Text = masterDetail.Master.Title,
							IsEnabled = true,
							Tag = masterDetail
						};
						button.Click += (sender, args) =>
						{
							var masterDetailRenderer = GetRenderer(masterDetail) as MasterDetailRenderer;

							if (masterDetailRenderer != null)
								masterDetailRenderer.Toggle();
						};
						_page.ApplicationBar.Buttons.Add(button);
					}
				}
			}

			var buttonsToAdd = new List<TaggedAppBarButton>();
			foreach (ToolbarItem item in items.Where(i => i.Order != ToolbarItemOrder.Secondary))
			{
				if (_page.ApplicationBar.Buttons.OfType<TaggedAppBarButton>().Any(b => b.Tag == item))
					continue;

				var button = new TaggedAppBarButton
				{
					IconUri = new Uri(item.Icon ?? "ApplicationIcon.jpg", UriKind.Relative),
					Text = !string.IsNullOrWhiteSpace(item.Text) ? item.Text : (string)item.Icon ?? "ApplicationIcon.jpg",
					IsEnabled = item.IsEnabled,
					Tag = item
				};
				button.Click += (sender, args) => item.Activate();
				buttonsToAdd.Add(button);
			}

			var menuItemsToAdd = new List<TaggedAppBarMenuItem>();
			foreach (ToolbarItem item in items.Where(i => i.Order == ToolbarItemOrder.Secondary))
			{
				if (_page.ApplicationBar.MenuItems.OfType<TaggedAppBarMenuItem>().Any(b => b.Tag == item))
					continue;

				var button = new TaggedAppBarMenuItem { Text = !string.IsNullOrWhiteSpace(item.Text) ? item.Text : (string)item.Icon ?? "MenuItem", IsEnabled = true, Tag = item };
				button.Click += (sender, args) => item.Activate();
				menuItemsToAdd.Add(button);
			}

			TaggedAppBarButton[] deadButtons = _page.ApplicationBar.Buttons.OfType<TaggedAppBarButton>().Where(b => b.Tag is ToolbarItem && !items.Contains(b.Tag)).ToArray();

			TaggedAppBarMenuItem[] deadMenuItems = _page.ApplicationBar.MenuItems.OfType<TaggedAppBarMenuItem>().Where(b => b.Tag is ToolbarItem && !items.Contains(b.Tag)).ToArray();

			// we must remove the dead buttons before adding the new ones so we don't accidentally go over the limit during the transition
			foreach (TaggedAppBarButton deadButton in deadButtons)
			{
				deadButton.Dispose();
				_page.ApplicationBar.Buttons.Remove(deadButton);
			}

			foreach (TaggedAppBarMenuItem deadMenuItem in deadMenuItems)
				_page.ApplicationBar.MenuItems.Remove(deadMenuItem);

			// fixme, insert in order
			foreach (TaggedAppBarButton newButton in buttonsToAdd)
				_page.ApplicationBar.Buttons.Add(newButton);

			foreach (TaggedAppBarMenuItem newMenuItem in menuItemsToAdd)
				_page.ApplicationBar.MenuItems.Add(newMenuItem);

			_page.ApplicationBar.IsVisible = _page.ApplicationBar.Buttons.Count > 0 || _page.ApplicationBar.MenuItems.Count > 0;
		}

		void UpdateToolbarTracker()
		{
			if (_navModel.Roots.Last() != null)
				_tracker.Target = _navModel.Roots.Last();
		}

		class TaggedAppBarButton : ApplicationBarIconButton, IDisposable
		{
			bool _disposed;
			object _tag;

			public object Tag
			{
				get { return _tag; }
				set
				{
					if (_tag == null && value is ToolbarItem)
						(value as ToolbarItem).PropertyChanged += TaggedAppBarButton_PropertyChanged;
					_tag = value;
				}
			}

			public void Dispose()
			{
				if (_disposed)
					return;
				_disposed = true;

				if (Tag != null && Tag is ToolbarItem)
					(Tag as ToolbarItem).PropertyChanged -= TaggedAppBarButton_PropertyChanged;
			}

			void TaggedAppBarButton_PropertyChanged(object sender, PropertyChangedEventArgs e)
			{
				var item = Tag as ToolbarItem;
				if (item == null)
					return;

				if (e.PropertyName == item.IsEnabledPropertyName)
					IsEnabled = item.IsEnabled;
				else if (e.PropertyName == MenuItem.TextProperty.PropertyName)
					Text = !string.IsNullOrWhiteSpace(item.Text) ? item.Text : (string)item.Icon ?? "ApplicationIcon.jpg";
				else if (e.PropertyName == MenuItem.IconProperty.PropertyName)
					IconUri = new Uri(item.Icon ?? "ApplicationIcon.jpg", UriKind.Relative);
			}
		}

		class TaggedAppBarMenuItem : ApplicationBarMenuItem
		{
			public object Tag { get; set; }
		}
	}
}