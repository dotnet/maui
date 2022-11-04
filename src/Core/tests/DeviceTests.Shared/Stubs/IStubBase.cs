using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public interface IStubBase : IView, IVisualTreeElement, IToolTipElement
	{
		new string AutomationId { get; set; }

		new double Width { get; set; }

		new double Height { get; set; }

		new double MaximumWidth { get; set; }

		new double MaximumHeight { get; set; }

		new double MinimumWidth { get; set; }

		new double MinimumHeight { get; set; }

		new double TranslationX { get; set; }

		new double TranslationY { get; set; }

		new double Scale { get; set; }

		new double ScaleX { get; set; }

		new double ScaleY { get; set; }

		new double Rotation { get; set; }

		new double RotationX { get; set; }

		new double RotationY { get; set; }

		new double AnchorX { get; set; }

		new double AnchorY { get; set; }

		new Thickness Margin { get; set; }

		new FlowDirection FlowDirection { get; set; }

		new double Opacity { get; set; }

		new Visibility Visibility { get; set; }

		new Semantics Semantics { get; set; }

		new Paint Background { get; set; }

		new IShape Clip { get; set; }

		new bool InputTransparent { get; set; }
		new IElement Parent { get; set; }
	}

}
