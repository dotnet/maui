using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[ContentProperty("Content")]
	[RenderWith(typeof(_SwipeViewRenderer))]
	public class SwipeView : ContentView, IElementConfiguration<SwipeView>, ISwipeViewController
	{
		readonly Lazy<PlatformConfigurationRegistry<SwipeView>> _platformConfigurationRegistry;

		public SwipeView()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<SwipeView>>(() => new PlatformConfigurationRegistry<SwipeView>(this));
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void VerifySwipeViewFlagEnabled(
			string constructorHint = null,
			[CallerMemberName] string memberName = "")
		{
			ExperimentalFlags.VerifyFlagEnabled(nameof(SwipeView), ExperimentalFlags.SwipeViewExperimental, memberName: memberName);
		}

		public static readonly BindableProperty LeftItemsProperty =
			BindableProperty.Create(nameof(LeftItems), typeof(SwipeItems), typeof(SwipeView), null, BindingMode.OneWay, null, defaultValueCreator: SwipeItemsDefaultValueCreator,
				propertyChanged: OnSwipeItemsChanged);

		public static readonly BindableProperty RightItemsProperty =
			BindableProperty.Create(nameof(RightItems), typeof(SwipeItems), typeof(SwipeView), null, BindingMode.OneWay, null, defaultValueCreator: SwipeItemsDefaultValueCreator);

		public static readonly BindableProperty TopItemsProperty =
			BindableProperty.Create(nameof(TopItems), typeof(SwipeItems), typeof(SwipeView), null, BindingMode.OneWay, null, defaultValueCreator: SwipeItemsDefaultValueCreator);

		public static readonly BindableProperty BottomItemsProperty =
			BindableProperty.Create(nameof(BottomItems), typeof(SwipeItems), typeof(SwipeView), null, BindingMode.OneWay, null, defaultValueCreator: SwipeItemsDefaultValueCreator);

		public SwipeItems LeftItems
		{
			get { return (SwipeItems)GetValue(LeftItemsProperty); }
			set { SetValue(LeftItemsProperty, value); }
		}

		public SwipeItems RightItems
		{
			get { return (SwipeItems)GetValue(RightItemsProperty); }
			set { SetValue(RightItemsProperty, value); }
		}

		public SwipeItems TopItems
		{
			get { return (SwipeItems)GetValue(TopItemsProperty); }
			set { SetValue(TopItemsProperty, value); }
		}

		public SwipeItems BottomItems
		{
			get { return (SwipeItems)GetValue(BottomItemsProperty); }
			set { SetValue(BottomItemsProperty, value); }
		}

		static void OnSwipeItemsChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((SwipeView)bindable).UpdateSwipeItemsParent((SwipeItems)newValue);
		}

		public event EventHandler<SwipeStartedEventArgs> SwipeStarted;
		public event EventHandler<SwipeChangingEventArgs> SwipeChanging;
		public event EventHandler<SwipeEndedEventArgs> SwipeEnded;
		public event EventHandler CloseRequested;

		public void Close()
		{
			CloseRequested?.Invoke(this, EventArgs.Empty);
		}

		void ISwipeViewController.SendSwipeStarted(SwipeStartedEventArgs args) => SwipeStarted?.Invoke(this, args);

		void ISwipeViewController.SendSwipeChanging(SwipeChangingEventArgs args) => SwipeChanging?.Invoke(this, args);

		void ISwipeViewController.SendSwipeEnded(SwipeEndedEventArgs args) => SwipeEnded?.Invoke(this, args);

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();

			object bc = BindingContext;

			if (LeftItems != null)
				SetInheritedBindingContext(LeftItems, bc);

			if (RightItems != null)
				SetInheritedBindingContext(RightItems, bc);

			if (TopItems != null)
				SetInheritedBindingContext(TopItems, bc);

			if (BottomItems != null)
				SetInheritedBindingContext(BottomItems, bc);
		}
  
		SwipeItems SwipeItemsDefaultValueCreator() => new SwipeItems();

		static object SwipeItemsDefaultValueCreator(BindableObject bindable)
		{
			return ((SwipeView)bindable).SwipeItemsDefaultValueCreator();
		}

		public IPlatformElementConfiguration<T, SwipeView> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		void UpdateSwipeItemsParent(SwipeItems swipeItems)
		{
			swipeItems.Parent = this;

			foreach (var swipeItem in swipeItems)
				((VisualElement)swipeItem).Parent = swipeItems;
		}
	}
}