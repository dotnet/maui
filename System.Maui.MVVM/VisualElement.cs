using System;
using System.Collections.Generic;
using System.Maui;
using System.Runtime.CompilerServices;
using System.Text;

namespace Xamarin.Forms
{
	public partial class VisualElement : IFrameworkElement
	{
		#region IContentContainer
		Rectangle IFrameworkElement.Frame => this.Bounds;

		protected IViewRenderer Renderer { get; set; }
		IViewRenderer IFrameworkElement.Renderer
		{
			get
			{
				return Renderer;
			}

			set
			{
				Renderer = value;
			}
		}

		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);
			(Renderer)?.UpdateValue(propertyName);
		}

		IFrameworkElement IFrameworkElement.Parent => this.Parent as IFrameworkElement;

		SizeRequest desiredSize;
		SizeRequest IFrameworkElement.DesiredSize => desiredSize;
		bool isMeasureValid;
		bool isArrangeValid;
		bool IFrameworkElement.IsMeasureValid => isMeasureValid;

		bool IFrameworkElement.IsArrangeValid => isArrangeValid;


		void IFrameworkElement.Arrange(Rectangle bounds)
		{
			if (isArrangeValid)
				return;
			isArrangeValid = true;
			this.Layout(bounds);
		}

		SizeRequest IFrameworkElement.Measure(double widthConstraint, double heightConstraint)
		{
			if (!isMeasureValid)
				desiredSize = this.Renderer.GetDesiredSize(widthConstraint, heightConstraint);// this.OnMeasure(widthConstraint, heightConstraint);
			isMeasureValid = true;
			return desiredSize;
		}

		void IFrameworkElement.InvalidateMeasure()
		{
			isMeasureValid = false;
			isArrangeValid = false;
			this.InvalidateMeasure();
		}

		void IFrameworkElement.InvalidateArrange()
		{
			isArrangeValid = false;
		}
		#endregion
	}
}
