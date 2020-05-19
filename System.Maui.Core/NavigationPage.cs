using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_NavigationPageRenderer))]
	public class NavigationPage : Page, IPageContainer<Page>, IBarElement, INavigationPageController, IElementConfiguration<NavigationPage> 
	{
		public static readonly BindableProperty BackButtonTitleProperty = BindableProperty.CreateAttached("BackButtonTitle", typeof(string), typeof(Page), null);

		public static readonly BindableProperty HasNavigationBarProperty = BindableProperty.CreateAttached("HasNavigationBar", typeof(bool), typeof(Page), true);

		public static readonly BindableProperty HasBackButtonProperty = BindableProperty.CreateAttached("HasBackButton", typeof(bool), typeof(NavigationPage), true);

		[Obsolete("TintProperty is obsolete as of version 1.2.0. Please use BarBackgroundColorProperty and BarTextColorProperty to change NavigationPage bar color properties.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static readonly BindableProperty TintProperty = BindableProperty.Create("Tint", typeof(Color), typeof(NavigationPage), Color.Default);

		public static readonly BindableProperty BarBackgroundColorProperty = BarElement.BarBackgroundColorProperty;

		public static readonly BindableProperty BarTextColorProperty = BarElement.BarTextColorProperty;

		public static readonly BindableProperty TitleIconImageSourceProperty = BindableProperty.CreateAttached("TitleIconImageSource", typeof(ImageSource), typeof(NavigationPage), default(ImageSource));

		[Obsolete("TitleIconProperty is obsolete as of 4.0.0. Please use TitleIconImageSourceProperty instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static readonly BindableProperty TitleIconProperty = TitleIconImageSourceProperty;

		public static readonly BindableProperty IconColorProperty = BindableProperty.CreateAttached("IconColor", typeof(Color), typeof(NavigationPage), Color.Default);

		public static readonly BindableProperty TitleViewProperty = BindableProperty.CreateAttached("TitleView", typeof(View), typeof(NavigationPage), null, propertyChanging: TitleViewPropertyChanging);

		static readonly BindablePropertyKey CurrentPagePropertyKey = BindableProperty.CreateReadOnly("CurrentPage", typeof(Page), typeof(NavigationPage), null);
		public static readonly BindableProperty CurrentPageProperty = CurrentPagePropertyKey.BindableProperty;

		static readonly BindablePropertyKey RootPagePropertyKey = BindableProperty.CreateReadOnly(nameof(RootPage), typeof(Page), typeof(NavigationPage), null);
		public static readonly BindableProperty RootPageProperty = RootPagePropertyKey.BindableProperty;

		public NavigationPage()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<NavigationPage>>(() => new PlatformConfigurationRegistry<NavigationPage>(this));

			Navigation = new NavigationImpl(this);
		}

		public NavigationPage(Page root) : this()
		{
			PushPage(root);
		}

		public Color BarBackgroundColor {
			get => (Color)GetValue(BarElement.BarBackgroundColorProperty);
			set => SetValue(BarElement.BarBackgroundColorProperty, value);
		}

		public Color BarTextColor {
			get => (Color)GetValue(BarElement.BarTextColorProperty);
			set => SetValue(BarElement.BarTextColorProperty, value);
		}

		[Obsolete("Tint is obsolete as of version 1.2.0. Please use BarBackgroundColor and BarTextColor to change NavigationPage bar color properties.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Color Tint
		{
			get { return (Color)GetValue(TintProperty); }
			set { SetValue(TintProperty, value); }
		}

		internal Task CurrentNavigationTask { get; set; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		public Page Peek(int depth)
		{
			if (depth < 0)
			{
				return null;
			}

			if (InternalChildren.Count <= depth)
			{
				return null;
			}

			return (Page)InternalChildren[InternalChildren.Count - depth - 1];
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public IEnumerable<Page> Pages => InternalChildren.Cast<Page>();

		[EditorBrowsable(EditorBrowsableState.Never)]
		public int StackDepth
		{
			get { return InternalChildren.Count; }
		}

		public Page CurrentPage
		{
			get { return (Page)GetValue(CurrentPageProperty); }
			private set { SetValue(CurrentPagePropertyKey, value); }
		}

		public Page RootPage
		{
			get { return (Page)GetValue(RootPageProperty); }
			private set { SetValue(RootPagePropertyKey, value); }
		}

		static void TitleViewPropertyChanging(BindableObject bindable, object oldValue, object newValue)
		{
			if (oldValue == newValue)
				return;

			if(bindable is Page page)
			{
				page.SetTitleView((View)oldValue, (View)newValue);
			}
			else if (oldValue != null)
			{
				var oldElem = (View)oldValue;
				oldElem.Parent = null;
			}
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

		[Obsolete("GetTitleIcon is obsolete as of 4.0.0. Please use GetTitleIconImageSource instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static FileImageSource GetTitleIcon(BindableObject bindable)
		{
			return bindable.GetValue(TitleIconImageSourceProperty) as FileImageSource;
		}

		public static ImageSource GetTitleIconImageSource(BindableObject bindable)
		{
			return (ImageSource)bindable.GetValue(TitleIconImageSourceProperty);
		}

		public static View GetTitleView(BindableObject bindable)
		{
			return (View)bindable.GetValue(TitleViewProperty);
		}

		public static Color GetIconColor(BindableObject bindable)
		{
			if (bindable == null)
			{
				return Color.Default;		
			}

			return (Color)bindable.GetValue(IconColorProperty);
		}

		public Task<Page> PopAsync()
		{
			return PopAsync(true);
		}

		public async Task<Page> PopAsync(bool animated)
		{
			var tcs = new TaskCompletionSource<bool>();
			try
			{
				if (CurrentNavigationTask != null && !CurrentNavigationTask.IsCompleted)
				{
					var oldTask = CurrentNavigationTask;
					CurrentNavigationTask = tcs.Task;
					await oldTask;
				}
				else
					CurrentNavigationTask = tcs.Task;

				var result = await PopAsyncInner(animated, false);
				tcs.SetResult(true);
				return result;
			}
			catch (Exception)
			{
				CurrentNavigationTask = null;
				tcs.SetCanceled();

				throw;
			}
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

		[Obsolete("SetTitleIcon is obsolete as of 4.0.0. Please use SetTitleIconImageSource instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void SetTitleIcon(BindableObject bindable, FileImageSource value)
		{
			bindable.SetValue(TitleIconImageSourceProperty, value);
		}

		public static void SetTitleIconImageSource(BindableObject bindable, ImageSource value)
		{
			bindable.SetValue(TitleIconImageSourceProperty, value);
		}

		public static void SetTitleView(BindableObject bindable, View value)
		{
			bindable.SetValue(TitleViewProperty, value);
		}

		public static void SetIconColor(BindableObject bindable, Color value)
		{
			bindable.SetValue(IconColorProperty, value);
		}

		protected override bool OnBackButtonPressed()
		{
			if (CurrentPage.SendBackButtonPressed())
				return true;

			if (StackDepth > 1)
			{
				SafePop();
				return true;
			}

			return base.OnBackButtonPressed();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler<NavigationRequestedEventArgs> InsertPageBeforeRequested;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public async Task<Page> PopAsyncInner(bool animated, bool fast)
		{
			if (StackDepth == 1)
			{
				return null;
			}

			var page = (Page)InternalChildren.Last();

			return await (this as INavigationPageController).RemoveAsyncInner(page, animated, fast);
		}

		async Task<Page> INavigationPageController.RemoveAsyncInner(Page page, bool animated, bool fast)
		{
			if (StackDepth == 1)
			{
				return null;
			}

			var args = new NavigationRequestedEventArgs(page, animated);

			var removed = true;

			EventHandler<NavigationRequestedEventArgs> requestPop = PopRequested;
			if (requestPop != null)
			{
				requestPop(this, args);

				if (args.Task != null && !fast)
					removed = await args.Task;
			}

			if (!removed && !fast)
				return CurrentPage;

			InternalChildren.Remove(page);

			CurrentPage = (Page)InternalChildren.Last();

			if (Popped != null)
				Popped(this, args);

			return page;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler<NavigationRequestedEventArgs> PopRequested;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler<NavigationRequestedEventArgs> PopToRootRequested;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler<NavigationRequestedEventArgs> PushRequested;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler<NavigationRequestedEventArgs> RemovePageRequested;

		void InsertPageBefore(Page page, Page before)
		{
			if (page == null)
				throw new ArgumentNullException($"{nameof(page)} cannot be null.");

			if (before == null)
				throw new ArgumentNullException($"{nameof(before)} cannot be null.");

			if (!InternalChildren.Contains(before))
				throw new ArgumentException($"{nameof(before)} must be a child of the NavigationPage", nameof(before));

			if (InternalChildren.Contains(page))
				throw new ArgumentException("Cannot insert page which is already in the navigation stack");

			EventHandler<NavigationRequestedEventArgs> handler = InsertPageBeforeRequested;
			handler?.Invoke(this, new NavigationRequestedEventArgs(page, before, false));

			int index = InternalChildren.IndexOf(before);
			InternalChildren.Insert(index, page);

			if (index == 0)
				RootPage = page;

			// Shouldn't be required?
			if (Width > 0 && Height > 0)
				ForceLayout();
		}

		async Task PopToRootAsyncInner(bool animated)
		{
			if (StackDepth == 1)
				return;

			Element[] childrenToRemove = InternalChildren.Skip(1).ToArray();
			foreach (Element child in childrenToRemove)
				InternalChildren.Remove(child);

			CurrentPage = RootPage;

			var args = new NavigationRequestedEventArgs(RootPage, animated);

			EventHandler<NavigationRequestedEventArgs> requestPopToRoot = PopToRootRequested;
			if (requestPopToRoot != null)
			{
				requestPopToRoot(this, args);

				if (args.Task != null)
					await args.Task;
			}

			PoppedToRoot?.Invoke(this, new PoppedToRootEventArgs(RootPage, childrenToRemove.OfType<Page>().ToList()));
		}

		async Task PushAsyncInner(Page page, bool animated)
		{
			if (InternalChildren.Contains(page))
				return;

			PushPage(page);

			var args = new NavigationRequestedEventArgs(page, animated);

			EventHandler<NavigationRequestedEventArgs> requestPush = PushRequested;
			if (requestPush != null)
			{
				requestPush(this, args);

				if (args.Task != null)
					await args.Task;
			}

			Pushed?.Invoke(this, args);
		}

		void PushPage(Page page)
		{
			InternalChildren.Add(page);

			if (InternalChildren.Count == 1)
				RootPage = page;

			CurrentPage = page;
		}

		void RemovePage(Page page)
		{
			if (page == null)
				throw new ArgumentNullException($"{nameof(page)} cannot be null.");

			if (page == CurrentPage && CurrentPage == RootPage)
				throw new InvalidOperationException("Cannot remove root page when it is also the currently displayed page.");
			if (page == CurrentPage)
			{
				Log.Warning("NavigationPage", "RemovePage called for CurrentPage object. This can result in undesired behavior, consider calling PopAsync instead.");
				PopAsync();
				return;
			}

			if (!InternalChildren.Contains(page))
				throw new ArgumentException("Page to remove must be contained on this Navigation Page");

			EventHandler<NavigationRequestedEventArgs> handler = RemovePageRequested;
			handler?.Invoke(this, new NavigationRequestedEventArgs(page, true));

			InternalChildren.Remove(page);
			if (RootPage == page)
				RootPage = (Page)InternalChildren.First();
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
				_castingList = new Lazy<ReadOnlyCastingList<Page, Element>>(() => new ReadOnlyCastingList<Page, Element>(Owner.InternalChildren));
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
