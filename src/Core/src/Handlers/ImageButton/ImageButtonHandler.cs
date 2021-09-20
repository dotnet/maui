//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Microsoft.Maui.Handlers
//{
//	public partial interface IImageButtonHandler : IViewHandler, IButtonHandler, IImageHandler
//	{
//		public new IImageButton VirtualView { get; }
//	}

//	public static partial class ImageButtonMapper
//	{
//		public static IPropertyMapper<IImageButton, IImageButtonHandler> Mapper =
//			new PropertyMapper<IImageButton, IImageButtonHandler>(ViewHandler.ViewMapper, ButtonMapper.Mapper, ImageMapper.Mapper)
//			{
//			};

//		public static CommandMapper<IButton, IButtonHandler> ImageButtonCommandMapper = new(ButtonMapper.CommandMapper)
//		{
//		};
//	}

//	public partial class ImageButtonHandler : IImageButtonHandler
//	{
//		IButton IButtonHandler.VirtualView => this.VirtualView;

//		IImage IImageHandler.VirtualView => this.VirtualView;

//		ImageSourceServiceResultManager IImageHandler.SourceManager { get; } = new ImageSourceServiceResultManager();

//		public ImageButtonHandler() : base(ImageMapper.Mapper)
//		{
//		}

//		public ImageButtonHandler(IPropertyMapper mapper) : base(mapper ?? ImageButtonMapper.Mapper)
//		{
//		}
//	}
//}
