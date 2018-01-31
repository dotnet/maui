using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.V4.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Xamarin.Forms.Platform.Android.AppCompat;
using FragmentManager = Android.Support.V4.App.FragmentManager;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.Android
{
	public class Platform : BindableObject, IPlatform, INavigation, IDisposable, IPlatformLayout
	{
		internal const string CloseContextActionsSignalName = "Xamarin.CloseContextActions";

		internal static readonly BindableProperty RendererProperty = BindableProperty.CreateAttached("Renderer", typeof(IVisualElementRenderer), typeof(Platform), default(IVisualElementRenderer),
			propertyChanged: (bindable, oldvalue, newvalue) =>
			{
				var view = bindable as VisualElement;
				if (view != null)
					view.IsPlatformEnabled = newvalue != null;
			});

		IMasterDetailPageController MasterDetailPageController => CurrentMasterDetailPage as IMasterDetailPageController;

		readonly Context _context;
		readonly Activity _activity;

		readonly PlatformRenderer _renderer;
		readonly ToolbarTracker _toolbarTracker = new ToolbarTracker();

		NavigationPage _currentNavigationPage;

		TabbedPage _currentTabbedPage;

		Color _defaultActionBarTitleTextColor;

		bool _disposed;

		bool _ignoreAndroidSelection;

		Page _navigationPageCurrentPage;
		NavigationModel _navModel = new NavigationModel();

		readonly bool _embedded;

		internal Platform(Context context, bool embedded)
		{
			_embedded = embedded;
			_context = context ?? throw new ArgumentNullException(nameof(context), "Somehow we're getting a null context passed in");
			_activity = context as Activity;

			if (!embedded)
			{
				_defaultActionBarTitleTextColor = SetDefaultActionBarTitleTextColor();
			}
			
			_renderer = new PlatformRenderer(context, this);

			if (embedded)
			{
				// Set up handling of DisplayAlert/DisplayActionSheet/UpdateProgressBarVisibility
				if (_activity == null)
				{
					// Can't show dialogs if it's not an activity
					return;
				}

				PopupManager.Subscribe(_activity);
				return;
			}

			FormsApplicationActivity.BackPressed += HandleBackPressed;

			_toolbarTracker.CollectionChanged += ToolbarTrackerOnCollectionChanged;
		}

		#region IPlatform implementation

		internal Page Page { get; private set; }

		#endregion

		IPageController CurrentPageController => _navModel.CurrentPage as IPageController;

		ActionBar ActionBar => _activity?.ActionBar;

		MasterDetailPage CurrentMasterDetailPage { get; set; }

		NavigationPage CurrentNavigationPage
		{
			get { return _currentNavigationPage; }
			set
			{
				if (_currentNavigationPage == value)
					return;

				if (_currentNavigationPage != null)
				{
					_currentNavigationPage.Pushed -= CurrentNavigationPageOnPushed;
					_currentNavigationPage.Popped -= CurrentNavigationPageOnPopped;
					_currentNavigationPage.PoppedToRoot -= CurrentNavigationPageOnPoppedToRoot;
					_currentNavigationPage.PropertyChanged -= CurrentNavigationPageOnPropertyChanged;
				}

				RegisterNavPageCurrent(null);

				_currentNavigationPage = value;

				if (_currentNavigationPage != null)
				{
					_currentNavigationPage.Pushed += CurrentNavigationPageOnPushed;
					_currentNavigationPage.Popped += CurrentNavigationPageOnPopped;
					_currentNavigationPage.PoppedToRoot += CurrentNavigationPageOnPoppedToRoot;
					_currentNavigationPage.PropertyChanged += CurrentNavigationPageOnPropertyChanged;
					RegisterNavPageCurrent(_currentNavigationPage.CurrentPage);
				}
			}
		}

		TabbedPage CurrentTabbedPage
		{
			get { return _currentTabbedPage; }
			set
			{
				if (_currentTabbedPage == value)
					return;

				if (_currentTabbedPage != null)
				{
					_currentTabbedPage.PagesChanged -= CurrentTabbedPageChildrenChanged;
					_currentTabbedPage.PropertyChanged -= CurrentTabbedPageOnPropertyChanged;

					if (value == null)
						ActionBar.RemoveAllTabs();
				}

				_currentTabbedPage = value;

				if (_currentTabbedPage != null)
				{
					_currentTabbedPage.PagesChanged += CurrentTabbedPageChildrenChanged;
					_currentTabbedPage.PropertyChanged += CurrentTabbedPageOnPropertyChanged;
				}

				UpdateActionBarTitle();

				ActionBar.NavigationMode = value == null ? ActionBarNavigationMode.Standard : ActionBarNavigationMode.Tabs;
				CurrentTabbedPageChildrenChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}
		}

#pragma warning disable 618 // Eventually we will need to determine how to handle the v7 ActionBarDrawerToggle for AppCompat
		ActionBarDrawerToggle MasterDetailPageToggle { get; set; }
#pragma warning restore 618

		void IDisposable.Dispose()
		{
			if (_disposed)
				return;
			_disposed = true;

			SetPage(null);

			if (_embedded)
			{
				PopupManager.Unsubscribe(_context);
			}

			FormsApplicationActivity.BackPressed -= HandleBackPressed;
			_toolbarTracker.CollectionChanged -= ToolbarTrackerOnCollectionChanged;
			_toolbarTracker.Target = null;

			CurrentNavigationPage = null;
			CurrentMasterDetailPage = null;
			CurrentTabbedPage = null;
		}

		void INavigation.InsertPageBefore(Page page, Page before)
		{
			throw new InvalidOperationException("InsertPageBefore is not supported globally on Android, please use a NavigationPage.");
		}

		IReadOnlyList<Page> INavigation.ModalStack => _navModel.Modals.ToList();

		IReadOnlyList<Page> INavigation.NavigationStack => new List<Page>();

		Task<Page> INavigation.PopAsync()
		{
			return ((INavigation)this).PopAsync(true);
		}

		Task<Page> INavigation.PopAsync(bool animated)
		{
			throw new InvalidOperationException("PopAsync is not supported globally on Android, please use a NavigationPage.");
		}

		Task<Page> INavigation.PopModalAsync()
		{
			return ((INavigation)this).PopModalAsync(true);
		}

		Task<Page> INavigation.PopModalAsync(bool animated)
		{
			Page modal = _navModel.PopModal();

			((IPageController)modal).SendDisappearing();
			var source = new TaskCompletionSource<Page>();

			IVisualElementRenderer modalRenderer = GetRenderer(modal);
			if (modalRenderer != null)
			{
				if (animated)
				{
					modalRenderer.View.Animate().Alpha(0).ScaleX(0.8f).ScaleY(0.8f).SetDuration(250).SetListener(new GenericAnimatorListener
					{
						OnEnd = a =>
						{
							modalRenderer.View.RemoveFromParent();
							modalRenderer.Dispose();
							source.TrySetResult(modal);
							CurrentPageController?.SendAppearing();
						}
					});
				}
				else
				{
					modalRenderer.View.RemoveFromParent();
					modalRenderer.Dispose();
					source.TrySetResult(modal);
					CurrentPageController?.SendAppearing();
				}
			}

			_toolbarTracker.Target = _navModel.Roots.Last();
			UpdateActionBar();

			return source.Task;
		}

		Task INavigation.PopToRootAsync()
		{
			return ((INavigation)this).PopToRootAsync(true);
		}

		Task INavigation.PopToRootAsync(bool animated)
		{
			throw new InvalidOperationException("PopToRootAsync is not supported globally on Android, please use a NavigationPage.");
		}

		Task INavigation.PushAsync(Page root)
		{
			return ((INavigation)this).PushAsync(root, true);
		}

		Task INavigation.PushAsync(Page root, bool animated)
		{
			throw new InvalidOperationException("PushAsync is not supported globally on Android, please use a NavigationPage.");
		}

		Task INavigation.PushModalAsync(Page modal)
		{
			return ((INavigation)this).PushModalAsync(modal, true);
		}

		async Task INavigation.PushModalAsync(Page modal, bool animated)
		{
			CurrentPageController?.SendDisappearing();

			_navModel.PushModal(modal);

			modal.Platform = this;

			await PresentModal(modal, animated);

			// Verify that the modal is still on the stack
			if (_navModel.CurrentPage == modal)
				((IPageController)modal).SendAppearing();

			_toolbarTracker.Target = _navModel.Roots.Last();

			UpdateActionBar();
		}

		void INavigation.RemovePage(Page page)
		{
			throw new InvalidOperationException("RemovePage is not supported globally on Android, please use a NavigationPage.");
		}

		[Obsolete("CreateRenderer(VisualElement) is obsolete as of version 2.5. Please use CreateRendererWithContext(VisualElement, Context) instead.")]
		public static IVisualElementRenderer CreateRenderer(VisualElement element)
		{
			// If there's a previewer context set, use that when created 
			return CreateRenderer(element, GetPreviewerContext(element) ?? Forms.Context);
		}

		internal static IVisualElementRenderer CreateRenderer(VisualElement element, Context context)
		{
			IVisualElementRenderer renderer = Registrar.Registered.GetHandlerForObject<IVisualElementRenderer>(element, context)
				?? new DefaultRenderer(context);
			renderer.SetElement(element);

			return renderer;
		}

		public static IVisualElementRenderer CreateRendererWithContext(VisualElement element, Context context)
		{
			// This is an interim method to allow public access to CreateRenderer(element, context), which we 
			// can't make public yet because it will break the previewer
			return CreateRenderer(element, context);
		}

		public static IVisualElementRenderer GetRenderer(VisualElement bindable)
		{
			return (IVisualElementRenderer)bindable.GetValue(RendererProperty);
		}

		public static void SetRenderer(VisualElement bindable, IVisualElementRenderer value)
		{
			bindable.SetValue(RendererProperty, value);
		}

		public void UpdateActionBarTextColor()
		{
			if (_embedded)
			{
				return;
			}

			SetActionBarTextColor();
			UpdateActionBarUpImageColor();
		}

		protected override void OnBindingContextChanged()
		{
			SetInheritedBindingContext(Page, BindingContext);

			base.OnBindingContextChanged();
		}

		internal static IVisualElementRenderer CreateRenderer(VisualElement element, FragmentManager fragmentManager, Context context)
		{
			IVisualElementRenderer renderer = Registrar.Registered.GetHandlerForObject<IVisualElementRenderer>(element, context) ?? new DefaultRenderer(context);

			var managesFragments = renderer as IManageFragments;
			managesFragments?.SetFragmentManager(fragmentManager);

			renderer.SetElement(element);

			return renderer;
		}

		internal ViewGroup GetViewGroup()
		{
			return _renderer;
		}

		internal void PrepareMenu(IMenu menu)
		{
			if (_embedded)
			{
				return;
			}

			foreach (ToolbarItem item in _toolbarTracker.ToolbarItems)
				item.PropertyChanged -= HandleToolbarItemPropertyChanged;
			menu.Clear();

			if (!ShouldShowActionBarTitleArea())
				return;

			foreach (ToolbarItem item in _toolbarTracker.ToolbarItems)
			{
				IMenuItemController controller = item;
				item.PropertyChanged += HandleToolbarItemPropertyChanged;
				if (item.Order == ToolbarItemOrder.Secondary)
				{
					IMenuItem menuItem = menu.Add(item.Text);
					menuItem.SetEnabled(controller.IsEnabled);
					menuItem.SetOnMenuItemClickListener(new GenericMenuClickListener(controller.Activate));
				}
				else
				{
					IMenuItem menuItem = menu.Add(item.Text);
					var icon = item.Icon;
					if (!string.IsNullOrEmpty(icon))
					{
						Drawable iconDrawable = _context.GetFormsDrawable(icon);
						if (iconDrawable != null)
							menuItem.SetIcon(iconDrawable);
					}
					menuItem.SetEnabled(controller.IsEnabled);
					menuItem.SetShowAsAction(ShowAsAction.Always);
					menuItem.SetOnMenuItemClickListener(new GenericMenuClickListener(controller.Activate));
				}
			}
		}

		internal async void SendHomeClicked()
		{
			if (UpButtonShouldNavigate())
			{
				if (NavAnimationInProgress)
					return;
				NavAnimationInProgress = true;
				await CurrentNavigationPage.PopAsync();
				NavAnimationInProgress = false;
			}
			else if (CurrentMasterDetailPage != null)
			{
				if (MasterDetailPageController.ShouldShowSplitMode && CurrentMasterDetailPage.IsPresented)
					return;
				CurrentMasterDetailPage.IsPresented = !CurrentMasterDetailPage.IsPresented;
			}
		}

		internal void SetPage(Page newRoot)
		{
			var layout = false;
			List<IVisualElementRenderer> toDispose = null;

			if (Page != null)
			{
				_renderer.RemoveAllViews();

				toDispose = _navModel.Roots.Select(Android.Platform.GetRenderer).ToList();

				_navModel = new NavigationModel();

				layout = true;
			}

			if (newRoot == null)
				return;

			_navModel.Push(newRoot, null);

			Page = newRoot;
			Page.Platform = this;
			AddChild(Page, layout);

			Application.Current.NavigationProxy.Inner = this;

			_toolbarTracker.Target = newRoot;

			UpdateActionBar();

			if (toDispose?.Count > 0)
			{
				// Queue up disposal of the previous renderers after the current layout updates have finished
				new Handler(Looper.MainLooper).Post(() =>
				{
					foreach (IVisualElementRenderer rootRenderer in toDispose)
					{
						rootRenderer.Dispose();
					}
				});
			}
		}

		internal void UpdateActionBar()
		{
			if (ActionBar == null || _embedded) //Fullscreen theme doesn't have action bar
				return;

			List<Page> relevantAncestors = AncestorPagesOfPage(_navModel.CurrentPage);

			IEnumerable<NavigationPage> navPages = relevantAncestors.OfType<NavigationPage>();
			if (navPages.Count() > 1)
				throw new Exception("Android only allows one navigation page on screen at a time");
			NavigationPage navPage = navPages.FirstOrDefault();

			IEnumerable<TabbedPage> tabbedPages = relevantAncestors.OfType<TabbedPage>();
			if (tabbedPages.Count() > 1)
				throw new Exception("Android only allows one tabbed page on screen at a time");
			TabbedPage tabbedPage = tabbedPages.FirstOrDefault();

			CurrentMasterDetailPage = relevantAncestors.OfType<MasterDetailPage>().FirstOrDefault();
			CurrentNavigationPage = navPage;
			CurrentTabbedPage = tabbedPage;

			if (navPage != null && navPage.CurrentPage == null)
			{
				throw new InvalidOperationException("NavigationPage must have a root Page before being used. Either call PushAsync with a valid Page, or pass a Page to the constructor before usage.");
			}

			if (ShouldShowActionBarTitleArea() || CurrentTabbedPage != null)
				ShowActionBar();
			else
				HideActionBar();

			UpdateMasterDetailToggle();
		}

		internal void UpdateActionBarBackgroundColor()
		{
			if (ActionBar == null)
			{
				return;
			}

			if (!ShouldShowActionBarTitleArea())
				return;

			Color colorToUse = Color.Default;
			if (CurrentNavigationPage != null)
			{
#pragma warning disable 618 // Make sure Tint still works 
				if (CurrentNavigationPage.Tint != Color.Default)
					colorToUse = CurrentNavigationPage.Tint;
#pragma warning restore 618
				else if (CurrentNavigationPage.BarBackgroundColor != Color.Default)
					colorToUse = CurrentNavigationPage.BarBackgroundColor;
			}
			using (Drawable drawable = colorToUse == Color.Default ? GetActionBarBackgroundDrawable() : new ColorDrawable(colorToUse.ToAndroid()))
				ActionBar.SetBackgroundDrawable(drawable);
		}

		internal void UpdateMasterDetailToggle(bool update = false)
		{
			if (CurrentMasterDetailPage == null)
			{
				if (MasterDetailPageToggle == null)
					return;
				// clear out the icon
				ClearMasterDetailToggle();
				return;
			}
			if (!CurrentMasterDetailPage.ShouldShowToolbarButton() || string.IsNullOrEmpty(CurrentMasterDetailPage.Master.Icon) ||
				(MasterDetailPageController.ShouldShowSplitMode && CurrentMasterDetailPage.IsPresented))
			{
				//clear out existing icon;
				ClearMasterDetailToggle();
				return;
			}

			if (MasterDetailPageToggle == null || update)
			{
				ClearMasterDetailToggle();
				GetNewMasterDetailToggle();
			}

			bool state;
			if (CurrentNavigationPage == null)
				state = true;
			else
				state = !UpButtonShouldNavigate();
			if (state == MasterDetailPageToggle.DrawerIndicatorEnabled)
				return;
			MasterDetailPageToggle.DrawerIndicatorEnabled = state;
			MasterDetailPageToggle.SyncState();
		}

		void AddChild(VisualElement view, bool layout = false)
		{
			if (GetRenderer(view) != null)
				return;

			IVisualElementRenderer renderView = CreateRenderer(view, _context);
			SetRenderer(view, renderView);

			if (layout)
				view.Layout(new Rectangle(0, 0, _context.FromPixels(_renderer.Width), _context.FromPixels(_renderer.Height)));

			_renderer.AddView(renderView.View);
		}

#pragma warning disable 618 // This may need to be updated to work with TabLayout/AppCompat
		ActionBar.Tab AddTab(Page page, int index)
#pragma warning restore 618
		{
			ActionBar actionBar = ActionBar;

			if (actionBar == null)
			{
				return null;
			}

			TabbedPage currentTabs = CurrentTabbedPage;

			var atab = actionBar.NewTab();
			atab.SetText(page.Title);
			atab.TabSelected += (sender, e) =>
			{
				if (!_ignoreAndroidSelection)
					currentTabs.CurrentPage = page;
			};
			actionBar.AddTab(atab, index);

			page.PropertyChanged += PagePropertyChanged;
			return atab;
		}

		List<Page> AncestorPagesOfPage(Page root)
		{
			var result = new List<Page>();
			if (root == null)
				return result;

			if (root is IPageContainer<Page>)
			{
				var navPage = (IPageContainer<Page>)root;
				result.AddRange(AncestorPagesOfPage(navPage.CurrentPage));
			}
			else if (root is MasterDetailPage)
				result.AddRange(AncestorPagesOfPage(((MasterDetailPage)root).Detail));
			else
			{
				foreach (Page page in ((IPageController)root).InternalChildren.OfType<Page>())
					result.AddRange(AncestorPagesOfPage(page));
			}

			result.Add(root);
			return result;
		}

		void ClearMasterDetailToggle()
		{
			if (MasterDetailPageToggle == null)
				return;

			MasterDetailPageToggle.DrawerIndicatorEnabled = false;
			MasterDetailPageToggle.SyncState();
			MasterDetailPageToggle.Dispose();
			MasterDetailPageToggle = null;
		}

		void CurrentNavigationPageOnPopped(object sender, NavigationEventArgs eventArg)
		{
			UpdateActionBar();
		}

		void CurrentNavigationPageOnPoppedToRoot(object sender, EventArgs eventArgs)
		{
			UpdateActionBar();
		}

		void CurrentNavigationPageOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
#pragma warning disable 618 // Make sure Tint still works
			if (e.PropertyName == NavigationPage.TintProperty.PropertyName)
#pragma warning restore 618
				UpdateActionBarBackgroundColor();
			else if (e.PropertyName == NavigationPage.BarBackgroundColorProperty.PropertyName)
				UpdateActionBarBackgroundColor();
			else if (e.PropertyName == NavigationPage.BarTextColorProperty.PropertyName)
				UpdateActionBarTextColor();
			else if (e.PropertyName == NavigationPage.CurrentPageProperty.PropertyName)
				RegisterNavPageCurrent(CurrentNavigationPage.CurrentPage);
		}

		void CurrentNavigationPageOnPushed(object sender, NavigationEventArgs eventArg)
		{
			UpdateActionBar();
		}

		void CurrentTabbedPageChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (CurrentTabbedPage == null)
				return;

			_ignoreAndroidSelection = true;

			e.Apply((o, index, create) => AddTab((Page)o, index), (o, index) => RemoveTab((Page)o, index), Reset);

			if (CurrentTabbedPage.CurrentPage != null)
			{
				Page page = CurrentTabbedPage.CurrentPage;
				int index = TabbedPage.GetIndex(page);
				if (index >= 0 && index < CurrentTabbedPage.Children.Count)
					ActionBar.GetTabAt(index).Select();
			}

			_ignoreAndroidSelection = false;
		}

		void CurrentTabbedPageOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName != "CurrentPage")
				return;

			UpdateActionBar();

			// If we switch tabs while pushing a new page, UpdateActionBar() can set currentTabbedPage to null
			if (_currentTabbedPage == null)
				return;

			NavAnimationInProgress = true;

			Page page = _currentTabbedPage.CurrentPage;
			if (page == null)
			{
				ActionBar.SelectTab(null);
				NavAnimationInProgress = false;
				return;
			}

			int index = TabbedPage.GetIndex(page);
			if (ActionBar.SelectedNavigationIndex == index || index >= ActionBar.NavigationItemCount)
			{
				NavAnimationInProgress = false;
				return;
			}

			ActionBar.SelectTab(ActionBar.GetTabAt(index));

			NavAnimationInProgress = false;
		}

		Drawable GetActionBarBackgroundDrawable()
		{
			int[] backgroundDataArray = { global::Android.Resource.Attribute.Background };

			using (var outVal = new TypedValue())
			{
				_context.Theme.ResolveAttribute(global::Android.Resource.Attribute.ActionBarStyle, outVal, true);
				TypedArray actionBarStyle = _context.Theme.ObtainStyledAttributes(outVal.ResourceId, backgroundDataArray);

				Drawable result = actionBarStyle.GetDrawable(0);
				actionBarStyle.Recycle();
				return result;
			}
		}

		void GetNewMasterDetailToggle()
		{
			int icon = ResourceManager.GetDrawableByName(CurrentMasterDetailPage.Master.Icon);
			var drawer = GetRenderer(CurrentMasterDetailPage) as MasterDetailRenderer;
			if (drawer == null)
				return;

			if (_activity == null)
			{
				return;
			}

#pragma warning disable 618 // Eventually we will need to determine how to handle the v7 ActionBarDrawerToggle for AppCompat
			MasterDetailPageToggle = new ActionBarDrawerToggle(_activity, drawer, icon, 0, 0);
#pragma warning restore 618

			MasterDetailPageToggle.SyncState();
		}

		bool HandleBackPressed(object sender, EventArgs e)
		{
			if (NavAnimationInProgress)
				return true;

			Page root = _navModel.Roots.Last();
			bool handled = root.SendBackButtonPressed();

			return handled;
		}

		void HandleToolbarItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (_activity == null)
			{
				return;
			}

			if (e.PropertyName == MenuItem.IsEnabledProperty.PropertyName)
				_activity.InvalidateOptionsMenu();
			else if (e.PropertyName == MenuItem.TextProperty.PropertyName)
				_activity.InvalidateOptionsMenu();
			else if (e.PropertyName == MenuItem.IconProperty.PropertyName)
				_activity.InvalidateOptionsMenu();
		}

		void HideActionBar()
		{
			ReloadToolbarItems();
			UpdateActionBarHomeAsUp(ActionBar);
			ActionBar.Hide();
		}

		void NavigationPageCurrentPageOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == NavigationPage.HasNavigationBarProperty.PropertyName)
				UpdateActionBar();
			else if (e.PropertyName == Page.TitleProperty.PropertyName)
				UpdateActionBarTitle();
		}

		void PagePropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			if (args.PropertyName == Page.TitleProperty.PropertyName)
			{
				ActionBar actionBar = ActionBar;
				TabbedPage currentTabs = CurrentTabbedPage;

				if (actionBar == null || currentTabs == null || actionBar.TabCount == 0)
					return;

				var page = sender as Page;
				var atab = actionBar.GetTabAt(currentTabs.Children.IndexOf(page));
				atab.SetText(page.Title);
			}
		}

		Task PresentModal(Page modal, bool animated)
		{
			IVisualElementRenderer modalRenderer = GetRenderer(modal);
			if (modalRenderer == null)
			{
				modalRenderer = CreateRenderer(modal, _context);
				SetRenderer(modal, modalRenderer);

				if (modal.BackgroundColor == Color.Default && modal.BackgroundImage == null)
					modalRenderer.View.SetWindowBackground();
			}
			modalRenderer.Element.Layout(new Rectangle(0, 0, _context.FromPixels(_renderer.Width), _context.FromPixels(_renderer.Height)));
			_renderer.AddView(modalRenderer.View);

			var source = new TaskCompletionSource<bool>();
			NavAnimationInProgress = true;
			if (animated)
			{
				modalRenderer.View.Alpha = 0;
				modalRenderer.View.ScaleX = 0.8f;
				modalRenderer.View.ScaleY = 0.8f;
				modalRenderer.View.Animate().Alpha(1).ScaleX(1).ScaleY(1).SetDuration(250).SetListener(new GenericAnimatorListener
				{
					OnEnd = a =>
					{
						source.TrySetResult(false);
						NavAnimationInProgress = false;
					},
					OnCancel = a =>
					{
						source.TrySetResult(true);
						NavAnimationInProgress = false;
					}
				});
			}
			else
			{
				NavAnimationInProgress = false;
				source.TrySetResult(true);
			}

			return source.Task;
		}

		void RegisterNavPageCurrent(Page page)
		{
			if (_navigationPageCurrentPage != null)
				_navigationPageCurrentPage.PropertyChanged -= NavigationPageCurrentPageOnPropertyChanged;

			_navigationPageCurrentPage = page;

			if (_navigationPageCurrentPage != null)
				_navigationPageCurrentPage.PropertyChanged += NavigationPageCurrentPageOnPropertyChanged;
		}

		void ReloadToolbarItems()
		{
			_activity?.InvalidateOptionsMenu();
		}

		void RemoveTab(Page page, int index)
		{
			page.PropertyChanged -= PagePropertyChanged;
			ActionBar?.RemoveTabAt(index);
		}

		void Reset()
		{
			ActionBar.RemoveAllTabs();

			if (CurrentTabbedPage == null)
				return;

			var i = 0;
			foreach (Page tab in CurrentTabbedPage.Children.OfType<Page>())
			{
				var realTab = AddTab(tab, i++);
				if (tab == CurrentTabbedPage.CurrentPage)
					realTab.Select();
			}
		}

		void SetActionBarTextColor()
		{
			Color navigationBarTextColor = CurrentNavigationPage == null ? Color.Default : CurrentNavigationPage.BarTextColor;
			TextView actionBarTitleTextView = null;

			if (Forms.IsLollipopOrNewer && _activity != null)
			{
				int actionbarId = _activity.Resources.GetIdentifier("action_bar", "id", "android");
				if (actionbarId > 0)
				{
					var toolbar = _activity.FindViewById(actionbarId) as ViewGroup;
					if (toolbar != null)
					{
						for (int i = 0; i < toolbar.ChildCount; i++)
						{
							var textView = toolbar.GetChildAt(i) as TextView;
							if (textView != null)
							{
								actionBarTitleTextView = textView;
								break;
							}
						}
					}
				}
			}

			if (actionBarTitleTextView == null && _activity != null)
			{
				int actionBarTitleId = _activity.Resources.GetIdentifier("action_bar_title", "id", "android");
				if (actionBarTitleId > 0)
					actionBarTitleTextView = _activity.FindViewById<TextView>(actionBarTitleId);
			}

			if (actionBarTitleTextView != null && navigationBarTextColor != Color.Default)
				actionBarTitleTextView.SetTextColor(navigationBarTextColor.ToAndroid());
			else if (actionBarTitleTextView != null && navigationBarTextColor == Color.Default)
				actionBarTitleTextView.SetTextColor(_defaultActionBarTitleTextColor.ToAndroid());
		}

		Color SetDefaultActionBarTitleTextColor()
		{
			var defaultTitleTextColor = new Color();

			if (_activity == null)
			{
				return defaultTitleTextColor;
			}

			TextView actionBarTitleTextView = null;

			int actionBarTitleId = _activity.Resources.GetIdentifier("action_bar_title", "id", "android");
			if (actionBarTitleId > 0)
				actionBarTitleTextView = _activity.FindViewById<TextView>(actionBarTitleId);

			if (actionBarTitleTextView != null)
			{
				ColorStateList defaultTitleColorList = actionBarTitleTextView.TextColors;
				string defaultColorHex = defaultTitleColorList.DefaultColor.ToString("X");
				defaultTitleTextColor = Color.FromHex(defaultColorHex);
			}

			return defaultTitleTextColor;
		}

		bool ShouldShowActionBarTitleArea()
		{
			if (_activity == null)
				return false;

			if (_activity.Window.Attributes.Flags.HasFlag(WindowManagerFlags.Fullscreen))
				return false;

			bool hasMasterDetailPage = CurrentMasterDetailPage != null;
			bool navigated = CurrentNavigationPage != null && ((INavigationPageController)CurrentNavigationPage).StackDepth > 1;
			bool navigationPageHasNavigationBar = CurrentNavigationPage != null && NavigationPage.GetHasNavigationBar(CurrentNavigationPage.CurrentPage);
			//if we have MDP and Navigation , we let navigation choose
			if (CurrentNavigationPage != null && hasMasterDetailPage)
			{
				return NavigationPage.GetHasNavigationBar(CurrentNavigationPage.CurrentPage);
			}
			return navigationPageHasNavigationBar || (hasMasterDetailPage && !navigated);
		}

		bool ShouldUpdateActionBarUpColor()
		{
			bool hasMasterDetailPage = CurrentMasterDetailPage != null;
			bool navigated = CurrentNavigationPage != null && ((INavigationPageController)CurrentNavigationPage).StackDepth > 1;
			return (hasMasterDetailPage && navigated) || !hasMasterDetailPage;
		}

		void ShowActionBar()
		{
			ReloadToolbarItems();
			UpdateActionBarTitle();
			UpdateActionBarBackgroundColor();
			UpdateActionBarTextColor();
			ActionBar.Show();
		}

		void ToolbarTrackerOnCollectionChanged(object sender, EventArgs eventArgs)
		{
			ReloadToolbarItems();
		}

		bool UpButtonShouldNavigate()
		{
			if (CurrentNavigationPage == null)
				return false;

			bool pagePushed = ((INavigationPageController)CurrentNavigationPage).StackDepth > 1;
			bool pushedPageHasBackButton = NavigationPage.GetHasBackButton(CurrentNavigationPage.CurrentPage);

			return pagePushed && pushedPageHasBackButton;
		}

		void UpdateActionBarHomeAsUp(ActionBar actionBar)
		{
			bool showHomeAsUp = ShouldShowActionBarTitleArea() && (CurrentMasterDetailPage != null || UpButtonShouldNavigate());
			actionBar.SetDisplayHomeAsUpEnabled(showHomeAsUp);
		}

		void UpdateActionBarTitle()
		{
			Page view = null;
			if (CurrentNavigationPage != null)
				view = CurrentNavigationPage.CurrentPage;
			else if (CurrentTabbedPage != null)
				view = CurrentTabbedPage.CurrentPage;

			if (view == null || _activity == null)
				return;

			ActionBar actionBar = _activity.ActionBar;

			var useLogo = false;
			var showHome = false;
			var showTitle = false;

			if (ShouldShowActionBarTitleArea())
			{
				actionBar.Title = view.Title;
				FileImageSource titleIcon = NavigationPage.GetTitleIcon(view);
				if (!string.IsNullOrWhiteSpace(titleIcon))
				{
					var iconBitmap = new BitmapDrawable(_context.Resources, ResourceManager.GetBitmap(_context.Resources, titleIcon));
					if (iconBitmap != null && iconBitmap.Bitmap != null)
						actionBar.SetLogo(iconBitmap);

					useLogo = true;
					showHome = true;
					showTitle = true;
				}
				else
				{
					showHome = true;
					showTitle = true;
				}
			}

			ActionBarDisplayOptions options = 0;
			if (useLogo)
				options = options | ActionBarDisplayOptions.UseLogo;
			if (showHome)
				options = options | ActionBarDisplayOptions.ShowHome;
			if (showTitle)
				options = options | ActionBarDisplayOptions.ShowTitle;
			actionBar.SetDisplayOptions(options, ActionBarDisplayOptions.UseLogo | ActionBarDisplayOptions.ShowTitle | ActionBarDisplayOptions.ShowHome);

			UpdateActionBarHomeAsUp(actionBar);
		}

		void UpdateActionBarUpImageColor()
		{
			if (_activity == null)
			{
				return;
			}

			Color navigationBarTextColor = CurrentNavigationPage == null ? Color.Default : CurrentNavigationPage.BarTextColor;
			ImageView actionBarUpImageView = null;

			int actionBarUpId = _activity.Resources.GetIdentifier("up", "id", "android");
			if (actionBarUpId > 0)
				actionBarUpImageView = _activity.FindViewById<ImageView>(actionBarUpId);

			if (actionBarUpImageView != null && navigationBarTextColor != Color.Default)
			{
				if (ShouldUpdateActionBarUpColor())
					actionBarUpImageView.SetColorFilter(navigationBarTextColor.ToAndroid(), PorterDuff.Mode.SrcIn);
				else
					actionBarUpImageView.SetColorFilter(null);
			}
			else if (actionBarUpImageView != null && navigationBarTextColor == Color.Default)
				actionBarUpImageView.SetColorFilter(null);
		}

		internal static int GenerateViewId()
		{
			// getting unique Id's is an art, and I consider myself the Jackson Pollock of the field
			if ((int)Build.VERSION.SdkInt >= 17)
				return global::Android.Views.View.GenerateViewId();

			// Numbers higher than this range reserved for xml
			// If we roll over, it can be exceptionally problematic for the user if they are still retaining things, android's internal implementation is
			// basically identical to this except they do a lot of locking we don't have to because we know we only do this
			// from the UI thread
			if (s_id >= 0x00ffffff)
				s_id = 0x00000400;
			return s_id++;
		}

		static int s_id = 0x00000400;

		#region Previewer Stuff
		
		internal static readonly BindableProperty PageContextProperty = 
			BindableProperty.CreateAttached("PageContext", typeof(Context), typeof(Platform), null);

		internal Platform(Context context) : this(context, false)
		{
			// we have this overload instead of using a default value for 
			// the 'embedded' bool parameter so the previewer can find it via reflection
		}

		internal static void SetPageContext(BindableObject bindable, Context context)		
 		{
			// Set a context for this page and its child controls
			bindable.SetValue(PageContextProperty, context);
		}
		
		static Context GetPreviewerContext(Element element)
		{
			// Walk up the tree and find the Page this element is hosted in
			Element parent = element;
			while (!Application.IsApplicationOrNull(parent.RealParent))
			{
				parent = parent.RealParent;
			}

			// If a page is found, return the PageContext set by the previewer for that page (if any)
			return (parent as Page)?.GetValue(PageContextProperty) as Context;
		}

		#endregion

		internal class DefaultRenderer : VisualElementRenderer<View>
		{
			bool _notReallyHandled;

			[Obsolete("This constructor is obsolete as of version 2.5. Please use DefaultRenderer(Context) instead.")]
			public DefaultRenderer()
			{
			}

			readonly MotionEventHelper _motionEventHelper = new MotionEventHelper();

			public DefaultRenderer(Context context) : base(context)
			{
				ChildrenDrawingOrderEnabled = true;
			}

			internal void NotifyFakeHandling()
			{
				_notReallyHandled = true;
			}

			public override bool OnTouchEvent(MotionEvent e)
			{
				if (base.OnTouchEvent(e))
					return true;

				return _motionEventHelper.HandleMotionEvent(Parent, e);
			}

			protected override void OnElementChanged(ElementChangedEventArgs<View> e)
			{
				base.OnElementChanged(e);

				_motionEventHelper.UpdateElement(e.NewElement);
			}

			public override bool DispatchTouchEvent(MotionEvent e)
			{
				#region Excessive explanation
				// Normally dispatchTouchEvent feeds the touch events to its children one at a time, top child first,
				// (and only to the children in the hit-test area of the event) stopping as soon as one of them has handled
				// the event. 

				// But to be consistent across the platforms, we don't want this behavior; if an element is not input transparent
				// we don't want an event to "pass through it" and be handled by an element "behind/under" it. We just want the processing
				// to end after the first non-transparent child, regardless of whether the event has been handled.

				// This is only an issue for a couple of controls; the interactive controls (switch, button, slider, etc) already "handle" their touches 
				// and the events don't propagate to other child controls. But for image, label, and box that doesn't happen. We can't have those controls 
				// lie about their events being handled because then the events won't propagate to *parent* controls (e.g., a frame with a label in it would
				// never get a tap gesture from the label). In other words, we *want* parent propagation, but *do not want* sibling propagation. So we need to short-circuit 
				// base.DispatchTouchEvent here, but still return "false".

				// Duplicating the logic of ViewGroup.dispatchTouchEvent and modifying it slightly for our purposes is a non-starter; the method is too
				// complex and does a lot of micro-optimization. Instead, we provide a signalling mechanism for the controls which don't already "handle" touch
				// events to tell us that they will be lying about handling their event; they then return "true" to short-circuit base.DispatchTouchEvent.

				// The container gets this message and after it gets the "handled" result from dispatchTouchEvent, 
				// it then knows to ignore that result and return false/unhandled. This allows the event to propagate up the tree.
				#endregion

				_notReallyHandled = false;

				var result = base.DispatchTouchEvent(e);

				if (result && _notReallyHandled)
				{
					// If the child control returned true from its touch event handler but signalled that it was a fake "true", then we
					// don't consider the event truly "handled" yet. 
					// Since a child control short-circuited the normal dispatchTouchEvent stuff, this layout never got the chance for
					// IOnTouchListener.OnTouch and the OnTouchEvent override to try handling the touches; we'll do that now
					return OnTouchEvent(e);
				}

				return result;
			}
		}

		#region IPlatformEngine implementation

		void IPlatformLayout.OnLayout(bool changed, int l, int t, int r, int b)
		{
			if (changed)
			{
				// ActionBar title text color resets on rotation, make sure to update
				UpdateActionBarTextColor();
				foreach (Page modal in _navModel.Roots.ToList())
					modal.Layout(new Rectangle(0, 0, _context.FromPixels(r - l), _context.FromPixels(b - t)));
			}

			foreach (IVisualElementRenderer view in _navModel.Roots.Select(GetRenderer))
				view?.UpdateLayout();
		}

		SizeRequest IPlatform.GetNativeSize(VisualElement view, double widthConstraint, double heightConstraint)
		{
			var reference = Guid.NewGuid().ToString();
			Performance.Start(reference);

			// FIXME: potential crash
			IVisualElementRenderer viewRenderer = GetRenderer(view);

			// negative numbers have special meanings to android they don't to us
			widthConstraint = widthConstraint <= -1 ? double.PositiveInfinity : _context.ToPixels(widthConstraint);
			heightConstraint = heightConstraint <= -1 ? double.PositiveInfinity : _context.ToPixels(heightConstraint);

			int width = !double.IsPositiveInfinity(widthConstraint)
							? MeasureSpecFactory.MakeMeasureSpec((int)widthConstraint, MeasureSpecMode.AtMost)
							: MeasureSpecFactory.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);

			int height = !double.IsPositiveInfinity(heightConstraint)
							 ? MeasureSpecFactory.MakeMeasureSpec((int)heightConstraint, MeasureSpecMode.AtMost)
							 : MeasureSpecFactory.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);

			SizeRequest rawResult = viewRenderer.GetDesiredSize(width, height);
			if (rawResult.Minimum == Size.Zero)
				rawResult.Minimum = rawResult.Request;
			var result = new SizeRequest(new Size(_context.FromPixels(rawResult.Request.Width), _context.FromPixels(rawResult.Request.Height)),
				new Size(_context.FromPixels(rawResult.Minimum.Width), _context.FromPixels(rawResult.Minimum.Height)));

			Performance.Stop(reference);
			return result;
		}

		bool _navAnimationInProgress;

		internal bool NavAnimationInProgress
		{
			get { return _navAnimationInProgress; }
			set
			{
				if (_navAnimationInProgress == value)
					return;
				_navAnimationInProgress = value;
				if (value)
					MessagingCenter.Send(this, CloseContextActionsSignalName);
			}
		}

		#endregion
	}
}
