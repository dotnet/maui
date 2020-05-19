using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WButton = System.Windows.Controls.Button;

namespace System.Maui.Platform.WPF.Controls
{
	public class FormsButton : WButton
	{
		public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(nameof(CornerRadius), typeof(int), typeof(FormsButton),
			new PropertyMetadata(default(int), OnCornerRadiusChanged));

		Border _contentPresenter;


		public int CornerRadius
		{
			get
			{
				return (int)GetValue(CornerRadiusProperty);
			}
			set
			{
				SetValue(CornerRadiusProperty, value);
			}
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_contentPresenter = this.GetChildren<Border>().FirstOrDefault();
			UpdateCornerRadius();
		}

		static void OnCornerRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((FormsButton)d).UpdateCornerRadius();
		}

		void UpdateCornerRadius()
		{
			if (_contentPresenter != null)
				_contentPresenter.CornerRadius = new System.Windows.CornerRadius(CornerRadius);
		}
	}
}
