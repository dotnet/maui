using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Maui.Platform.WPF.Extensions;
using System.Maui.Platform.WPF.Helpers;
using System.Maui.Platform.WPF.Interfaces;

namespace System.Maui.Platform.WPF.Controls
{
	[TemplatePart(Name = "PART_TopAppBar", Type = typeof(FormsAppBar))]
	[TemplatePart(Name = "PART_BottomAppBar", Type = typeof(FormsAppBar))]
	public class FormsWindow : Window
	{
		FormsAppBar topAppBar;
		FormsAppBar bottomAppBar;
		System.Windows.Controls.Button previousButton;
		System.Windows.Controls.Button previousModalButton;
		System.Windows.Controls.Button hamburgerButton;

		public static readonly DependencyProperty StartupPageProperty = DependencyProperty.Register("StartupPage", typeof(object), typeof(FormsWindow));
		public static readonly DependencyProperty CurrentModalPageProperty = DependencyProperty.Register("CurrentModalPage", typeof(object), typeof(FormsWindow));
		public static readonly DependencyProperty ContentLoaderProperty = DependencyProperty.Register("ContentLoader", typeof(IContentLoader), typeof(FormsWindow), new PropertyMetadata(new DefaultContentLoader(), OnContentLoaderChanged));
		public static readonly DependencyProperty CurrentTitleProperty = DependencyProperty.Register("CurrentTitle", typeof(string), typeof(FormsWindow));
		public static readonly DependencyProperty HasBackButtonProperty = DependencyProperty.Register("HasBackButton", typeof(bool), typeof(FormsWindow));
		public static readonly DependencyProperty HasBackButtonModalProperty = DependencyProperty.Register("HasBackButtonModal", typeof(bool), typeof(FormsWindow));
		public static readonly DependencyProperty HasNavigationBarProperty = DependencyProperty.Register("HasNavigationBar", typeof(bool), typeof(FormsWindow));
		public static readonly DependencyProperty BackButtonTitleProperty = DependencyProperty.Register("BackButtonTitle", typeof(string), typeof(FormsWindow));
		public static readonly DependencyProperty CurrentNavigationPageProperty = DependencyProperty.Register("CurrentNavigationPage", typeof(FormsNavigationPage), typeof(FormsWindow));
		public static readonly DependencyProperty CurrentMasterDetailPageProperty = DependencyProperty.Register("CurrentMasterDetailPage", typeof(FormsMasterDetailPage), typeof(FormsWindow));
		public static readonly DependencyProperty CurrentContentDialogProperty = DependencyProperty.Register("CurrentContentDialog", typeof(FormsContentDialog), typeof(FormsWindow));
		public static readonly DependencyProperty TitleBarBackgroundColorProperty = DependencyProperty.Register("TitleBarBackgroundColor", typeof(Brush), typeof(FormsWindow));
		public static readonly DependencyProperty TitleBarTextColorProperty = DependencyProperty.Register("TitleBarTextColor", typeof(Brush), typeof(FormsWindow));

		public Brush TitleBarBackgroundColor
		{
			get { return (Brush)GetValue(TitleBarBackgroundColorProperty); }
			private set { SetValue(TitleBarBackgroundColorProperty, value); }
		}

		public Brush TitleBarTextColor
		{
			get { return (Brush)GetValue(TitleBarTextColorProperty); }
			private set { SetValue(TitleBarTextColorProperty, value); }
		}

		public FormsContentDialog CurrentContentDialog
		{
			get { return (FormsContentDialog)GetValue(CurrentContentDialogProperty); }
			set { SetValue(CurrentContentDialogProperty, value); }
		}

		public object StartupPage
		{
			get { return (object)GetValue(StartupPageProperty); }
			set { SetValue(StartupPageProperty, value); }
		}

		public string CurrentTitle
		{
			get { return (string)GetValue(CurrentTitleProperty); }
			private set { SetValue(CurrentTitleProperty, value); }
		}

		public FormsNavigationPage CurrentNavigationPage
		{
			get { return (FormsNavigationPage)GetValue(CurrentNavigationPageProperty); }
			private set { SetValue(CurrentNavigationPageProperty, value); }
		}

		public FormsMasterDetailPage CurrentMasterDetailPage
		{
			get { return (FormsMasterDetailPage)GetValue(CurrentMasterDetailPageProperty); }
			private set { SetValue(CurrentMasterDetailPageProperty, value); }
		}

		public bool HasBackButton
		{
			get { return (bool)GetValue(HasBackButtonProperty); }
			private set { SetValue(HasBackButtonProperty, value); }
		}

		public bool HasBackButtonModal
		{
			get { return (bool)GetValue(HasBackButtonModalProperty); }
			private set { SetValue(HasBackButtonModalProperty, value); }
		}

		public bool HasNavigationBar
		{
			get { return (bool)GetValue(HasNavigationBarProperty); }
			private set { SetValue(HasNavigationBarProperty, value); }
		}

		public string BackButtonTitle
		{
			get { return (string)GetValue(BackButtonTitleProperty); }
			private set { SetValue(BackButtonTitleProperty, value); }
		}

		public object CurrentModalPage
		{
			get { return (object)GetValue(CurrentModalPageProperty); }
			private set { SetValue(CurrentModalPageProperty, value); }
		}

		public IContentLoader ContentLoader
		{
			get { return (IContentLoader)GetValue(ContentLoaderProperty); }
			set { SetValue(ContentLoaderProperty, value); }
		}


		public FormsWindow()
		{
			this.DefaultStyleKey = typeof(FormsWindow);
			this.Loaded += (sender, e) => Appearing();
			this.Unloaded += (sender, e) => Disappearing();
		}

		protected virtual void Appearing()
		{

		}

		protected virtual void Disappearing()
		{

		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			topAppBar = Template.FindName("PART_TopAppBar", this) as FormsAppBar;
			bottomAppBar = Template.FindName("PART_BottomAppBar", this) as FormsAppBar;
			previousButton = Template.FindName("PART_Previous", this) as System.Windows.Controls.Button;
			if (previousButton != null)
			{
				previousButton.Click += PreviousButton_Click;
			}
			previousModalButton = Template.FindName("PART_Previous_Modal", this) as System.Windows.Controls.Button;
			if (previousButton != null)
			{
				previousModalButton.Click += PreviousModalButton_Click;
			}
			hamburgerButton = Template.FindName("PART_Hamburger", this) as System.Windows.Controls.Button;
			if (hamburgerButton != null)
			{
				hamburgerButton.Click += HamburgerButton_Click;
			}
		}

		private void PreviousModalButton_Click(object sender, RoutedEventArgs e)
		{
			OnBackSystemButtonPressed();
		}

		private void HamburgerButton_Click(object sender, RoutedEventArgs e)
		{
			if (CurrentMasterDetailPage != null)
			{
				CurrentMasterDetailPage.IsPresented = !CurrentMasterDetailPage.IsPresented;
			}
		}

		private void PreviousButton_Click(object sender, RoutedEventArgs e)
		{
			if (CurrentNavigationPage != null && CurrentNavigationPage.StackDepth > 1)
			{
				CurrentNavigationPage.OnBackButtonPressed();
			}
		}

		private static void OnContentLoaderChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue == null)
				throw new ArgumentNullException("ContentLoader");
		}

		public void SynchronizeAppBar()
		{
			IEnumerable<FormsPage> childrens = this.FindVisualChildren<FormsPage>();

			CurrentTitle = childrens.FirstOrDefault()?.GetTitle();
			HasNavigationBar = childrens.FirstOrDefault()?.GetHasNavigationBar() ?? false;
			CurrentNavigationPage = childrens.OfType<FormsNavigationPage>()?.FirstOrDefault();
			CurrentMasterDetailPage = childrens.OfType<FormsMasterDetailPage>()?.FirstOrDefault();
			var page = childrens.FirstOrDefault();
			if (page != null)
			{
				TitleBarBackgroundColor = page.GetTitleBarBackgroundColor();
				TitleBarTextColor = page.GetTitleBarTextColor();
			}
			else
			{
				ClearValue(TitleBarBackgroundColorProperty);
				ClearValue(TitleBarTextColorProperty);
			}

			hamburgerButton.Visibility = CurrentMasterDetailPage != null ? Visibility.Visible : Visibility.Collapsed;

			if (CurrentNavigationPage != null)
			{
				HasBackButton = CurrentNavigationPage.GetHasBackButton();
				BackButtonTitle = CurrentNavigationPage.GetBackButtonTitle();

			}
			else
			{
				HasBackButton = false;
				BackButtonTitle = "";
			}
		}

		public void SynchronizeToolbarCommands()
		{
			IEnumerable<FormsPage> childrens = this.FindVisualChildren<FormsPage>();

			var page = childrens.FirstOrDefault();
			if (page == null) return;

			topAppBar.PrimaryCommands = page.GetPrimaryTopBarCommands().OrderBy(ti => ti.GetValue(FrameworkElementAttached.PriorityProperty));
			topAppBar.SecondaryCommands = page.GetSecondaryTopBarCommands().OrderBy(ti => ti.GetValue(FrameworkElementAttached.PriorityProperty));
			bottomAppBar.PrimaryCommands = page.GetPrimaryBottomBarCommands().OrderBy(ti => ti.GetValue(FrameworkElementAttached.PriorityProperty));
			bottomAppBar.SecondaryCommands = page.GetSecondaryBottomBarCommands().OrderBy(ti => ti.GetValue(FrameworkElementAttached.PriorityProperty));
			bottomAppBar.Content = childrens.LastOrDefault(x => x.ContentBottomBar != null)?.ContentBottomBar;

			topAppBar.Reset();
			bottomAppBar.Reset();
		}

		public void ShowContentDialog(FormsContentDialog contentDialog)
		{
			this.CurrentContentDialog = contentDialog;
		}

		public void HideContentDialog()
		{
			this.CurrentContentDialog = null;
		}

		public ObservableCollection<object> InternalChildren { get; } = new ObservableCollection<object>();


		public void PushModal(object page)
		{
			PushModal(page, true);
		}

		public void PushModal(object page, bool animated)
		{
			InternalChildren.Add(page);
			this.CurrentModalPage = InternalChildren.Last();
			this.HasBackButtonModal = true;
		}

		public object PopModal()
		{
			return PopModal(true);
		}

		public object PopModal(bool animated)
		{
			if (InternalChildren.Count < 1)
				return null;

			var modal = InternalChildren.Last();

			if (InternalChildren.Remove(modal))
			{
				/*if (LightContentControl != null)
				{
					LightContentControl.Transition = animated ? TransitionType.Right : TransitionType.Normal;
				}*/
				CurrentModalPage = InternalChildren.LastOrDefault();
			}

			this.HasBackButtonModal = InternalChildren.Count >= 1;

			return modal;
		}

		public virtual void OnBackSystemButtonPressed()
		{
			PopModal();
		}
	}
}
