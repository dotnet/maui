using System;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public class ShellHeaderRenderer : Microsoft.UI.Xaml.Controls.ContentControl
	{
		Shell _shell;

		public ShellHeaderRenderer(Shell element)
		{
			Shell.VerifyShellUWPFlagEnabled(nameof(ShellHeaderRenderer));

			SetElement(element);
			SizeChanged += OnShellHeaderRendererSizeChanged;
			HorizontalContentAlignment = HorizontalAlignment.Stretch;
			VerticalContentAlignment = VerticalAlignment.Stretch;
		}

		void OnShellHeaderRendererSizeChanged(object sender, SizeChangedEventArgs e)
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
				UpdateHeader();
			}
		}

		void OnShellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.IsOneOf(Shell.FlyoutHeaderProperty, Shell.FlyoutHeaderTemplateProperty))
				UpdateHeader();
		}

		void UpdateHeader()
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

			object header = null;

			if (_shell is IShellController controller)
				header = controller.FlyoutHeader;

			if (header is View visualElement)
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
