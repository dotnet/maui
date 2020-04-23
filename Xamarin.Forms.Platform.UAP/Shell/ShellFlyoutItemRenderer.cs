using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Xamarin.Forms.Platform.UWP
{
	public class ShellFlyoutItemRenderer : ContentControl
	{
		public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
			nameof(IsSelected), typeof(bool), typeof(ShellFlyoutItemRenderer),
			new PropertyMetadata(default(bool), IsSelectedChanged));

		View _content;
		public ShellFlyoutItemRenderer()
		{
			this.DataContextChanged += OnDataContextChanged;
		}

		public bool IsSelected
		{
			get => (bool)GetValue(IsSelectedProperty);
			set => SetValue(IsSelectedProperty, value);
		}

		void OnDataContextChanged(Windows.UI.Xaml.FrameworkElement sender, Windows.UI.Xaml.DataContextChangedEventArgs args)
		{
			if(Content is ViewToRendererConverter.WrapperControl oldControl)
			{				
				oldControl.CleanUp();
				if (_content != null)
				{
					_content.BindingContext = null;
					_content.Parent = null;
					_content = null;
				}
			}

			var bo = (BindableObject)DataContext;
			var shell = (bo as Element)?.FindParent<Shell>();
			DataTemplate dataTemplate = (shell as IShellController)?.GetFlyoutItemDataTemplate(bo);

			if(dataTemplate != null)
			{
				_content = (View)dataTemplate.CreateContent();
				_content.BindingContext = bo;
				_content.Parent = shell;
				Content = new ViewToRendererConverter.WrapperControl(_content);
			}
		}

		static void IsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var content = ((ShellFlyoutItemRenderer)d)._content;
			if(content != null)
			{
				if((bool)e.NewValue)
					VisualStateManager.GoToState(content, "Selected");
				else
					VisualStateManager.GoToState(content, "Normal");
			}
		}
	}
}
