#pragma warning disable CS0618 // Type or member is obsolete
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class FrameStub : Frame, IStubBase
	{
		double IStubBase.MaximumWidth
		{
			get => base.MaximumWidthRequest;
			set => base.MaximumWidthRequest = value;
		}

		double IStubBase.MaximumHeight
		{
			get => base.MaximumHeightRequest;
			set => base.MaximumHeightRequest = value;
		}

		double IStubBase.MinimumWidth
		{
			get => base.MinimumWidthRequest;
			set => base.MinimumWidthRequest = value;
		}

		double IStubBase.MinimumHeight
		{
			get => base.MinimumHeightRequest;
			set => base.MinimumHeightRequest = value;
		}

		public Visibility Visibility
		{
			get => base.IsVisible ? Visibility.Visible : Visibility.Hidden;
			set
			{
				if (value == Visibility.Visible)
					base.IsVisible = true;
				else
					base.IsVisible = false;
			}
		}

		public Semantics Semantics { get; set; } = new Semantics();

		double IStubBase.Width
		{
			get => base.WidthRequest;
			set => base.WidthRequest = value;
		}

		double IStubBase.Height
		{
			get => base.HeightRequest;
			set => base.HeightRequest = value;
		}

		Paint IStubBase.Background
		{
			get => base.Background;
			set => base.Background = value;
		}

		IShape IStubBase.Clip
		{
			get;
			set;
		}

		IShape IView.Clip
		{
			get => (this as IStubBase).Clip;
		}


		IElement _parent;
		IElement IStubBase.Parent
		{
			get
			{
				return _parent ?? (IElement)base.Parent;
			}

			set
			{
				if (value is Element e)
					base.Parent = e;

				_parent = value;
			}
		}

		IElement IElement.Parent
		{
			get
			{
				return _parent ?? (IElement)base.Parent;
			}
		}

		PropertyMapper IPropertyMapperView.GetPropertyMapperOverrides() =>
			PropertyMapperOverrides;

		public PropertyMapper PropertyMapperOverrides
		{
			get;
			set;
		}
	}
}
#pragma warning restore CS0618 // Type or member is obsolete