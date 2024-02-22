using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Controls.Handlers.Compatibility;

#if IOS
using UIKit;
#endif

namespace Maui.Controls.Sample
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
		}
	}

    internal class CustomShellNavBarAppearanceTracker : IShellNavBarAppearanceTracker
	{
		readonly IShellContext _context;
        readonly IShellNavBarAppearanceTracker _baseTracker;

        public CustomShellNavBarAppearanceTracker(IShellContext context, IShellNavBarAppearanceTracker baseTracker)
        {
            _context = context;
            _baseTracker = baseTracker;
        }

        public void Dispose() => _baseTracker.Dispose();

        public void ResetAppearance(UINavigationController controller) => _baseTracker.ResetAppearance(controller);

        public void SetAppearance(UINavigationController controller, ShellAppearance appearance) => _baseTracker.SetAppearance(controller, appearance);

        public void SetHasShadow(UINavigationController controller, bool hasShadow) => _baseTracker.SetHasShadow(controller, hasShadow);

        public void UpdateLayout(UINavigationController controller)
        {
            UIView? titleView = Shell.GetTitleView(_context.Shell.CurrentPage)?.Handler?.PlatformView as UIView;

            UIView? parentView = GetParentByType(titleView, typeof(UIKit.UIControl));

            if (parentView != null)
            {
                // height constraint
                NSLayoutConstraint.Create(parentView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, parentView.Superview, NSLayoutAttribute.Bottom, 1.0f, 0.0f).Active = true;
                NSLayoutConstraint.Create(parentView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, parentView.Superview, NSLayoutAttribute.Top, 1.0f, 0.0f).Active = true;

                // width constraint
                NSLayoutConstraint.Create(parentView, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, parentView.Superview, NSLayoutAttribute.Leading, 1.0f, 0.0f).Active = true;
                NSLayoutConstraint.Create(parentView, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, parentView.Superview, NSLayoutAttribute.Trailing, 1.0f, 0.0f).Active = true;
            }
            _baseTracker.UpdateLayout(controller);
        }

        static UIView? GetParentByType(UIView? view, Type type)
        {
            UIView? currentView = view;

            while (currentView != null)
            {
                if (currentView.GetType().UnderlyingSystemType == type)
                    break;

                currentView = currentView.Superview;
            }

            return currentView;
        }
	}

	public class CustomShellHandler : ShellRenderer
	{
		protected override IShellNavBarAppearanceTracker CreateNavBarAppearanceTracker(){
			return new CustomShellNavBarAppearanceTracker(this, base.CreateNavBarAppearanceTracker());
		}
	}
}