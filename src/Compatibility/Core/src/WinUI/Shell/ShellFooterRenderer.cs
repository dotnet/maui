using System;
using System.ComponentModel;
using Microsoft.UI.Xaml;
using Windows.Foundation;
using Windows.UI.Xaml;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public class ShellFooterRenderer : Microsoft.UI.Xaml.Controls.ContentControl
	{
		Shell _shell;

		public ShellFooterRenderer(Shell element)
		{
			Shell.VerifyShellUWPFlagEnabled(nameof(ShellFooterRenderer));

			SetElement(element);
			SizeChanged += OnShellFooterRendererSizeChanged;
			HorizontalContentAlignment = HorizontalAlignment.Stretch;
			VerticalContentAlignment = VerticalAlignment.Stretch;
		}

		void OnShellFooterRendererSizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (Element is Layout layout)
				layout.ForceLayout();
		}

		internal VisualElement Element { get; set; }

		public void SetElement(Shell shell)
		{
			if(_shell != null)
				_shell.PropertyChanged += OnShellPropertyChanged;

			if(shell != null)
			{
				_shell = shell;
				_shell.PropertyChanged += OnShellPropertyChanged;
				UpdateFooter();
			}
		}

		void OnShellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.IsOneOf(Shell.FlyoutFooterProperty, Shell.FlyoutFooterTemplateProperty))
				UpdateFooter();
		}

		void UpdateFooter()
		{
			if (Element != null)
			{
				if(Content is ViewToRendererConverter.WrapperControl wrapperControl)
				{
					wrapperControl.CleanUp();
					Content = null;
				}

				Element = null;
			}

			object Footer = null;

			if (_shell is IShellController controller)
				Footer = controller.FlyoutFooter;

			if (Footer is View visualElement)
			{
				Element = visualElement;
				Content = new ViewToRendererConverter.WrapperControl(visualElement);
			}
			else
			{
				Content = null;
			}
		}
	}
}
