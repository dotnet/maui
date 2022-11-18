using System.ComponentModel;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Controls.Platform
{
	public class ShellHeaderView : UI.Xaml.Controls.ContentControl
	{
		Shell _shell;

		public ShellHeaderView(Shell element)
		{
			SetElement(element);
			SizeChanged += OnShellHeaderViewSizeChanged;
			HorizontalContentAlignment = HorizontalAlignment.Stretch;
			VerticalContentAlignment = VerticalAlignment.Stretch;
			IsTabStop = false;
		}

		void OnShellHeaderViewSizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (Element is Controls.Compatibility.Layout layout)
				layout.ForceLayout();
		}

		internal VisualElement Element { get; set; }

		public void SetElement(Shell shell)
		{
			if (_shell != null)
				_shell.PropertyChanged -= OnShellPropertyChanged;

			if (shell != null)
			{
				_shell = shell;
				_shell.PropertyChanged += OnShellPropertyChanged;
				UpdateHeader();
			}
		}

		void OnShellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.IsOneOf(Shell.FlyoutHeaderProperty, Shell.FlyoutHeaderTemplateProperty))
				UpdateHeader();
		}

		void UpdateHeader()
		{
			if (Element != null)
			{
				if (Content is ViewToHandlerConverter.WrapperControl wrapperControl)
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
				Content = new ViewToHandlerConverter.WrapperControl(visualElement);
			}
			else
			{
				Content = null;
			}
		}
	}
}
