using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xamarin.Forms.Platform.WPF.Helpers;
using Xamarin.Forms.Platform.WPF.Interfaces;

namespace Xamarin.Forms.Platform.WPF.Controls
{
	public class FormsPage : UserControl
	{
		public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(FormsPage));
		public static readonly DependencyProperty BackButtonTitleProperty = DependencyProperty.Register("BackButtonTitle", typeof(string), typeof(FormsPage));
		public static readonly DependencyProperty HasNavigationBarProperty = DependencyProperty.Register("HasNavigationBar", typeof(bool), typeof(FormsPage), new PropertyMetadata(true));
		public static readonly DependencyProperty HasBackButtonProperty = DependencyProperty.Register("HasBackButton", typeof(bool), typeof(FormsPage), new PropertyMetadata(true));
		public static readonly DependencyProperty PrimaryTopBarCommandsProperty = DependencyProperty.Register("PrimaryTopBarCommands", typeof(ObservableCollection<FrameworkElement>), typeof(FormsPage));
		public static readonly DependencyProperty SecondaryTopBarCommandsProperty = DependencyProperty.Register("SecondaryTopBarCommands", typeof(ObservableCollection<FrameworkElement>), typeof(FormsPage));
		public static readonly DependencyProperty PrimaryBottomBarCommandsProperty = DependencyProperty.Register("PrimaryBottomBarCommands", typeof(ObservableCollection<FrameworkElement>), typeof(FormsPage));
		public static readonly DependencyProperty SecondaryBottomBarCommandsProperty = DependencyProperty.Register("SecondaryBottomBarCommands", typeof(ObservableCollection<FrameworkElement>), typeof(FormsPage));
		public static readonly DependencyProperty ContentBottomBarProperty = DependencyProperty.Register("ContentBottomBar", typeof(object), typeof(FormsPage));
		public static readonly DependencyProperty TitleBarBackgroundColorProperty = DependencyProperty.Register("TitleBarBackgroundColor", typeof(Brush), typeof(FormsPage));
		public static readonly DependencyProperty TitleBarTextColorProperty = DependencyProperty.Register("TitleBarTextColor", typeof(Brush), typeof(FormsPage));

		public Brush TitleBarBackgroundColor
		{
			get { return (Brush)GetValue(TitleBarBackgroundColorProperty); }
			set { SetValue(TitleBarBackgroundColorProperty, value); }
		}

		public Brush TitleBarTextColor
		{
			get { return (Brush)GetValue(TitleBarTextColorProperty); }
			set { SetValue(TitleBarTextColorProperty, value); }
		}

		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		public string BackButtonTitle
		{
			get { return (string)GetValue(BackButtonTitleProperty); }
			set { SetValue(BackButtonTitleProperty, value); }
		}

		public bool HasNavigationBar
		{
			get { return (bool)GetValue(HasNavigationBarProperty); }
			set { SetValue(HasNavigationBarProperty, value); }
		}

		public bool HasBackButton
		{
			get { return (bool)GetValue(HasBackButtonProperty); }
			set { SetValue(HasBackButtonProperty, value); }
		}

		public ObservableCollection<FrameworkElement> PrimaryTopBarCommands
		{
			get { return (ObservableCollection<FrameworkElement>)GetValue(PrimaryTopBarCommandsProperty); }
			set { SetValue(PrimaryTopBarCommandsProperty, value); }
		}

		public ObservableCollection<FrameworkElement> SecondaryTopBarCommands
		{
			get { return (ObservableCollection<FrameworkElement>)GetValue(SecondaryTopBarCommandsProperty); }
			set { SetValue(SecondaryTopBarCommandsProperty, value); }
		}

		public ObservableCollection<FrameworkElement> PrimaryBottomBarCommands
		{
			get { return (ObservableCollection<FrameworkElement>)GetValue(PrimaryBottomBarCommandsProperty); }
			set { SetValue(PrimaryBottomBarCommandsProperty, value); }
		}

		public ObservableCollection<FrameworkElement> SecondaryBottomBarCommands
		{
			get { return (ObservableCollection<FrameworkElement>)GetValue(SecondaryBottomBarCommandsProperty); }
			set { SetValue(SecondaryBottomBarCommandsProperty, value); }
		}

		public object ContentBottomBar
		{
			get { return (object)GetValue(ContentBottomBarProperty); }
			set { SetValue(ContentBottomBarProperty, value); }
		}

		public IFormsNavigation Navigation
		{
			get
			{
				IFormsNavigation nav = this.TryFindParent<FormsNavigationPage>();
				return nav ?? new DefaultNavigation();
			}
		}

		public FormsWindow ParentWindow
		{
			get
			{
				if (System.Windows.Application.Current.MainWindow is FormsWindow parentWindow)
					return parentWindow;
				return null;
			}
		}

		public FormsPage()
		{
			this.SetValue(FormsPage.PrimaryTopBarCommandsProperty, new ObservableCollection<FrameworkElement>());
			this.SetValue(FormsPage.SecondaryTopBarCommandsProperty, new ObservableCollection<FrameworkElement>());
			this.SetValue(FormsPage.PrimaryBottomBarCommandsProperty, new ObservableCollection<FrameworkElement>());
			this.SetValue(FormsPage.SecondaryBottomBarCommandsProperty, new ObservableCollection<FrameworkElement>());

			this.Loaded += (sender, e) => Appearing();
			this.Unloaded += (sender, e) => Disappearing();
		}

		private void OnPropertyChanged(object sender, EventArgs arg)
		{
			ParentWindow?.SynchronizeAppBar();
		}

		private void Commands_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			ParentWindow?.SynchronizeToolbarCommands();
		}

		protected virtual void Appearing()
		{
			this.PrimaryTopBarCommands.CollectionChanged += Commands_CollectionChanged;
			this.SecondaryTopBarCommands.CollectionChanged += Commands_CollectionChanged;
			this.PrimaryBottomBarCommands.CollectionChanged += Commands_CollectionChanged;
			this.SecondaryBottomBarCommands.CollectionChanged += Commands_CollectionChanged;
			DependencyPropertyDescriptor.FromProperty(FormsPage.TitleProperty, typeof(FormsPage)).AddValueChanged(this, OnPropertyChanged);
			DependencyPropertyDescriptor.FromProperty(FormsPage.HasBackButtonProperty, typeof(FormsPage)).AddValueChanged(this, OnPropertyChanged);
			DependencyPropertyDescriptor.FromProperty(FormsPage.HasNavigationBarProperty, typeof(FormsPage)).AddValueChanged(this, OnPropertyChanged);
			DependencyPropertyDescriptor.FromProperty(FormsPage.BackButtonTitleProperty, typeof(FormsPage)).AddValueChanged(this, OnPropertyChanged);
			DependencyPropertyDescriptor.FromProperty(FormsPage.TitleBarBackgroundColorProperty, typeof(FormsPage)).AddValueChanged(this, OnPropertyChanged);
			DependencyPropertyDescriptor.FromProperty(FormsPage.TitleBarTextColorProperty, typeof(FormsPage)).AddValueChanged(this, OnPropertyChanged);
			ParentWindow?.SynchronizeToolbarCommands();
			ParentWindow?.SynchronizeAppBar();
		}

		protected virtual void Disappearing()
		{
			this.PrimaryTopBarCommands.CollectionChanged -= Commands_CollectionChanged;
			this.SecondaryTopBarCommands.CollectionChanged -= Commands_CollectionChanged;
			this.PrimaryBottomBarCommands.CollectionChanged -= Commands_CollectionChanged;
			this.SecondaryBottomBarCommands.CollectionChanged -= Commands_CollectionChanged;
			DependencyPropertyDescriptor.FromProperty(FormsPage.TitleProperty, typeof(FormsPage)).RemoveValueChanged(this, OnPropertyChanged);
			DependencyPropertyDescriptor.FromProperty(FormsPage.HasBackButtonProperty, typeof(FormsPage)).RemoveValueChanged(this, OnPropertyChanged);
			DependencyPropertyDescriptor.FromProperty(FormsPage.HasNavigationBarProperty, typeof(FormsPage)).RemoveValueChanged(this, OnPropertyChanged);
			DependencyPropertyDescriptor.FromProperty(FormsPage.BackButtonTitleProperty, typeof(FormsPage)).RemoveValueChanged(this, OnPropertyChanged);
			DependencyPropertyDescriptor.FromProperty(FormsPage.TitleBarBackgroundColorProperty, typeof(FormsPage)).RemoveValueChanged(this, OnPropertyChanged);
			DependencyPropertyDescriptor.FromProperty(FormsPage.TitleBarTextColorProperty, typeof(FormsPage)).RemoveValueChanged(this, OnPropertyChanged);
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
		}

		public virtual string GetTitle()
		{
			return this.Title;
		}

		public virtual bool GetHasNavigationBar()
		{
			return this.HasNavigationBar;
		}

		public virtual Brush GetTitleBarBackgroundColor()
		{
			return this.TitleBarBackgroundColor;
		}

		public virtual Brush GetTitleBarTextColor()
		{
			return this.TitleBarTextColor;
		}

		public virtual IEnumerable<FrameworkElement> GetPrimaryTopBarCommands()
		{
			return this.PrimaryTopBarCommands;
		}

		public virtual IEnumerable<FrameworkElement> GetSecondaryTopBarCommands()
		{
			return this.SecondaryTopBarCommands;
		}

		public virtual IEnumerable<FrameworkElement> GetPrimaryBottomBarCommands()
		{
			return this.PrimaryBottomBarCommands;
		}

		public virtual IEnumerable<FrameworkElement> GetSecondaryBottomBarCommands()
		{
			return this.SecondaryBottomBarCommands;
		}
	}
}
