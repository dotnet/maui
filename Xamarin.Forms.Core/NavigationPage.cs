using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_NavigationPageRenderer))]
	public class NavigationPage : Page, IPageContainer<Page>, INavigationPageController, IElementConfiguration<NavigationPage> 
	{
		public static readonly BindableProperty BackButtonTitleProperty = BindableProperty.CreateAttached("BackButtonTitle", typeof(string), typeof(Page), null);

		public static readonly BindableProperty HasNavigationBarProperty = BindableProperty.CreateAttached("HasNavigationBar", typeof(bool), typeof(Page), true);

		public static readonly BindableProperty HasBackButtonProperty = BindableProperty.CreateAttached("HasBackButton", typeof(bool), typeof(NavigationPage), true);

		[Obsolete("Use BarBackgroundColorProperty and BarTextColorProperty to change NavigationPage bar color properties")] public static readonly BindableProperty TintProperty =
			BindableProperty.Create("Tint", typeof(Color), typeof(NavigationPage), Color.Default);

		public static readonly BindableProperty BarBackgroundColorProperty = BindableProperty.Create("BarBackgroundColor", typeof(Color), typeof(NavigationPage), Color.Default);

		public static readonly BindableProperty BarTextColorProperty = BindableProperty.Create("BarTextColor", typeof(Color), typeof(NavigationPage), Color.Default);

		public static readonly BindableProperty TitleIconProperty = BindableProperty.CreateAttached("TitleIcon", typeof(FileImageSource), typeof(NavigationPage), default(FileImageSource));

		static readonly BindablePropertyKey CurrentPagePropertyKey = BindableProperty.CreateReadOnly("CurrentPage", typeof(Page), typeof(NavigationPage), null);
		public static readonly BindableProperty CurrentPageProperty = CurrentPagePropertyKey.BindableProperty;
		
		public NavigationPage()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<NavigationPage>>(() => new PlatformConfigurationRegistry<NavigationPage>(this));

			Navigation = new NavigationImpl(this);
		}

		public NavigationPage(Page root) : this()
		{
			PushPage(root);
		}

		public Color BarBackgroundColor
		{
			get { return (Color)GetValue(BarBackgroundColorProperty); }
			set { SetValue(BarBackgroundColorProperty, value); }
		}

		public Color BarTextColor
		{
			get { return (Color)GetValue(BarTextColorProperty); }
			set { SetValue(BarTextColorProperty, value); }
		}

		[Obsolete("Use BarBackgroundColor and BarTextColor to change NavigationPage bar color properties")]
		public Color Tint
		{
			get { return (Color)GetValue(TintProperty); }
			set { SetValue(TintProperty, value); }
		}

		internal Task CurrentNavigationTask { get; set; }

		Stack<Page> INavigationPageController.StackCopy
		{
			get
			{
				var result = new Stack<Page>(PageController.InternalChildren.Count);
				foreach (Page page in PageController.InternalChildren)
					result.Push(page);
				return result;
			}
		}

		int INavigationPageController.StackDepth
		{
			get { return PageController.InternalChildren.Count; }
		}

		IPageController PageController => this as IPageController;

		public Page CurrentPage
		{
			get { return (Page)GetValue(CurrentPageProperty); }
			private set { SetValue(CurrentPagePropertyKey, value); }
		}

		public static string GetBackButtonTitle(BindableObject page)
		{
			return (string)page.GetValue(BackButtonTitleProperty);
		}

		public static bool GetHasBackButton(Page page)
		{
			if (page == null)
				throw new ArgumentNullException("page");
			return (bool)page.GetValue(HasBackButtonProperty);
		}

		public static bool GetHasNavigationBar(BindableObject page)
		{
			return (bool)page.GetValue(HasNavigationBarProperty);
		}

		public static FileImageSource GetTitleIcon(BindableObject bindable)
		{
			return (FileImageSource)bindable.GetValue(TitleIconProperty);
		}

		public Task<Page> PopAsync()
		{
			return PopAsync(true);
		}

		public async Task<Page> PopAsync(bool animated)
		{
			if (CurrentNavigationTask != null && !CurrentNavigationTask.IsCompleted)
			{
				var tcs = new TaskCompletionSource<bool>();
				Task oldTask = CurrentNavigationTask;
				CurrentNavigationTask = tcs.Task;
				await oldTask;

				Page page = await ((INavigationPageController)this).PopAsyncInner(animated);
				tcs.SetResult(true);
				return page;
			}

			Task<Page> result = ((INavigationPageController)this).PopAsyncInner(animated);
			CurrentNavigationTask = result;
			return await result;
		}

		public event EventHandler<NavigationEventArgs> Popped;

		public event EventHandler<NavigationEventArgs> PoppedToRoot;

		public Task PopToRootAsync()
		{
			return PopToRootAsync(true);
		}

		public async Task PopToRootAsync(bool animated)
		{
			if (CurrentNavigationTask != null && !CurrentNavigationTask.IsCompleted)
			{
				var tcs = new TaskCompletionSource<bool>();
				Task oldTask = CurrentNavigationTask;
				CurrentNavigationTask = tcs.Task;
				await oldTask;

				await PopToRootAsyncInner(animated);
				tcs.SetResult(true);
				return;
			}

			Task result = PopToRootAsyncInner(animated);
			CurrentNavigationTask = result;
			await result;
		}

		public Task PushAsync(Page page)
		{
			return PushAsync(page, true);
		}

		public async Task PushAsync(Page page, bool animated)
		{
			if (CurrentNavigationTask != null && !CurrentNavigationTask.IsCompleted)
			{
				var tcs = new TaskCompletionSource<bool>();
				Task oldTask = CurrentNavigationTask;
				CurrentNavigationTask = tcs.Task;
				await oldTask;

				await PushAsyncInner(page, animated);
				tcs.SetResult(true);
				return;
			}

			CurrentNavigationTask = PushAsyncInner(page, animated);
			await CurrentNavigationTask;
		}

		public event EventHandler<NavigationEventArgs> Pushed;

		public static void SetBackButtonTitle(BindableObject page, string value)
		{
			page.SetValue(BackButtonTitleProperty, value);
		}

		public static void SetHasBackButton(Page page, bool value)
		{
			if (page == null)
				throw new ArgumentNullException("page");
			page.SetValue(HasBackButtonProperty, value);
		}

		public static void SetHasNavigationBar(BindableObject page, bool value)
		{
			page.SetValue(HasNavigationBarProperty, value);
		}

		public static void SetTitleIcon(BindableObject bindable, FileImageSource value)
		{
			bindable.SetValue(TitleIconProperty, value);
		}

		protected override bool OnBackButtonPressed()
		{
			if (CurrentPage.SendBackButtonPressed())
				return true;

			if (((INavigationPageController)this).StackDepth > 1)
			{
				SafePop();
				return true;
			}

			return base.OnBackButtonPressed();
		}

		event EventHandler<NavigationRequestedEventArgs> InsertPageBeforeRequestedInternal;

		event EventHandler<NavigationRequestedEventArgs> INavigationPageController.InsertPageBeforeRequested
		{
			add { InsertPageBeforeRequestedInternal += value; }
			remove { InsertPageBeforeRequestedInternal -= value; }
		}

		async Task<Page> INavigationPageController.PopAsyncInner(bool animated, bool fast)
		{
			if (((INavigationPageController)this).StackDepth == 1)
			{
				return null;
			}

			var page = (Page)PageController.InternalChildren.Last();

			var args = new NavigationRequestedEventArgs(page, animated);

			var removed = true;

			EventHandler<NavigationRequestedEventArgs> requestPop = PopRequestedInternal;
			if (requestPop != null)
			{
				requestPop(this, args);

				if (args.Task != null && !fast)
					removed = await args.Task;
			}

			if (!removed && !fast)
				return CurrentPage;

			PageController.InternalChildren.Remove(page);

			CurrentPage = (Page)PageController.InternalChildren.Last();

			if (Popped != null)
				Popped(this, args);

			return page;
		}

		event EventHandler<NavigationRequestedEventArgs> PopRequestedInternal;

		event EventHandler<NavigationRequestedEventArgs> INavigationPageController.PopRequested
		{
			add { PopRequestedInternal += value; }
			remove { PopRequestedInternal -= value; }
		}

		event EventHandler<NavigationRequestedEventArgs> PopToRootRequestedInternal;

		event EventHandler<NavigationRequestedEventArgs> INavigationPageController.PopToRootRequested
		{
			add { PopToRootRequestedInternal += value; }
			remove { PopToRootRequestedInternal -= value; }
		}

		event EventHandler<NavigationRequestedEventArgs> PushRequestedInternal;

		event EventHandler<NavigationRequestedEventArgs> INavigationPageController.PushRequested
		{
			add { PushRequestedInternal += value; }
			remove { PushRequestedInternal -= value; }
		}

		event EventHandler<NavigationRequestedEventArgs> RemovePageRequestedInternal;

		event EventHandler<NavigationRequestedEventArgs> INavigationPageController.RemovePageRequested
		{
			add { RemovePageRequestedInternal += value; }
			remove { RemovePageRequestedInternal -= value; }
		}

		void InsertPageBefore(Page page, Page before)
		{
			if (!PageController.InternalChildren.Contains(before))
				throw new ArgumentException("before must be a child of the NavigationPage", "before");

			if (PageController.InternalChildren.Contains(page))
				throw new ArgumentException("Cannot insert page which is already in the navigation stack");

			EventHandler<NavigationRequestedEventArgs> handler = InsertPageBeforeRequestedInternal;
			handler?.Invoke(this, new NavigationRequestedEventArgs(page, before, false));

			int index = PageController.InternalChildren.IndexOf(before);
			PageController.InternalChildren.Insert(index, page);

			// Shouldn't be required?
			if (Width > 0 && Height > 0)
				ForceLayout();
		}

		async Task PopToRootAsyncInner(bool animated)
		{
			if (((INavigationPageController)this).StackDepth == 1)
				return;

			var root = (Page)PageController.InternalChildren.First();

			var childrenToRemove = PageController.InternalChildren.ToArray().Where(c => c != root);
			foreach (var child in childrenToRemove)
				PageController.InternalChildren.Remove(child);

			CurrentPage = root;

			var args = new NavigationRequestedEventArgs(root, animated);

			EventHandler<NavigationRequestedEventArgs> requestPopToRoot = PopToRootRequestedInternal;
			if (requestPopToRoot != null)
			{
				requestPopToRoot(this, args);

				if (args.Task != null)
					await args.Task;
			}

			if (PoppedToRoot != null)
				PoppedToRoot(this, new PoppedToRootEventArgs(root, childrenToRemove.OfType<Page>().ToList()));
		}

		async Task PushAsyncInner(Page page, bool animated)
		{
			if (PageController.InternalChildren.Contains(page))
				return;

			PushPage(page);

			var args = new NavigationRequestedEventArgs(page, animated);

			EventHandler<NavigationRequestedEventArgs> requestPush = PushRequestedInternal;
			if (requestPush != null)
			{
				requestPush(this, args);

				if (args.Task != null)
					await args.Task;
			}

			if (Pushed != null)
				Pushed(this, args);
		}

		void PushPage(Page page)
		{
			PageController.InternalChildren.Add(page);

			CurrentPage = page;
		}

		void RemovePage(Page page)
		{
			if (page == CurrentPage && ((INavigationPageController)this).StackDepth <= 1)
				throw new InvalidOperationException("Cannot remove root page when it is also the currently displayed page.");
			if (page == CurrentPage)
			{
				Log.Warning("NavigationPage", "RemovePage called for CurrentPage object. This can result in undesired behavior, consider called PopAsync instead.");
				PopAsync();
				return;
			}

			if (!PageController.InternalChildren.Contains(page))
				throw new ArgumentException("Page to remove must be contained on this Navigation Page");

			EventHandler<NavigationRequestedEventArgs> handler = RemovePageRequestedInternal;
			if (handler != null)
				handler(this, new NavigationRequestedEventArgs(page, true));

			PageController.InternalChildren.Remove(page);
		}

		void SafePop()
		{
			PopAsync(true).ContinueWith(t =>
			{
				if (t.IsFaulted)
					throw t.Exception;
			});
		}

		class NavigationImpl : NavigationProxy
		{
			readonly Lazy<ReadOnlyCastingList<Page, Element>> _castingList;

			public NavigationImpl(NavigationPage owner)
			{
				Owner = owner;
				_castingList = new Lazy<ReadOnlyCastingList<Page, Element>>(() => new ReadOnlyCastingList<Page, Element>(((IPageController)Owner).InternalChildren));
			}

			NavigationPage Owner { get; }

			protected override IReadOnlyList<Page> GetNavigationStack()
			{
				return _castingList.Value;
			}

			protected override void OnInsertPageBefore(Page page, Page before)
			{
				Owner.InsertPageBefore(page, before);
			}

			protected override Task<Page> OnPopAsync(bool animated)
			{
				return Owner.PopAsync(animated);
			}

			protected override Task OnPopToRootAsync(bool animated)
			{
				return Owner.PopToRootAsync(animated);
			}

			protected override Task OnPushAsync(Page root, bool animated)
			{
				return Owner.PushAsync(root, animated);
			}

			protected override void OnRemovePage(Page page)
			{
				Owner.RemovePage(page);
			}
		}

		readonly Lazy<PlatformConfigurationRegistry<NavigationPage>> _platformConfigurationRegistry;

		public new IPlatformElementConfiguration<T, NavigationPage> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}
	}
}