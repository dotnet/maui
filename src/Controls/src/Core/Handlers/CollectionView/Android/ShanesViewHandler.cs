using System;
using Android.Content;
using AndroidX.RecyclerView.Widget;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers
{
	public class ShanesViewHandler : ViewHandler<ShanesView, ShanesViewNativeView>
	{

		public static PropertyMapper<ShanesView, ShanesViewHandler> ShanesViewMapper;
		static ShanesViewHandler()
		{
			ShanesViewMapper = new PropertyMapper<ShanesView, ShanesViewHandler>(ViewHandler.ViewMapper)
			{
				[nameof(ShanesView.DifferentProperty)] = MapDifferentProperty
			};


			var mapper = SomeMiddleViewHandler.CreateMapper<ShanesView, ShanesViewHandler>();

			foreach (var thing in mapper.UpdateKeys)
			{
				ShanesViewMapper.Add(thing, mapper[thing]);
			}
		}



		private static void MapDifferentProperty(ShanesViewHandler arg1, ShanesView arg2)
		{
			arg1.NativeView.DoDifferentThing();
		}

		public ShanesViewHandler(PropertyMapper mapper, CommandMapper commandMapper = null) : base(mapper, commandMapper)
		{
		}

		protected override ShanesViewNativeView CreateNativeView()
		{
			return new ShanesViewNativeView(Context);
		}
	}




	static class SomeMiddleViewHandler
	{
		public static PropertyMapper<TVirtualView, THandler> CreateMapper<TVirtualView, THandler>()
			where TVirtualView : SomeMiddleView
			where THandler : ViewHandler
		{
			return new PropertyMapper<TVirtualView, THandler>()
			{
				[nameof(SomeMiddleView.SomeProperty)] = MapSomeProperty
			};
		}

		private static void MapSomeProperty(ViewHandler arg1, SomeMiddleView arg2)
		{
			(arg1.NativeView as ShanesViewNativeView).DoSomething();
		}
	}



	public class SomeMiddleView : View
	{
		public string SomeProperty { get; set; }
	}

	public class ShanesView : SomeMiddleView
	{
		public string DifferentProperty { get;set;}
	}



	public class SomeMiddleNativeView : RecyclerView
	{
		public SomeMiddleNativeView(Context context) : base(context)
		{
		}

		public void DoSomething()
		{
		}
	}


	public class ShanesViewNativeView : SomeMiddleNativeView
	{
		public ShanesViewNativeView(Context context) : base(context)
		{
		}

		internal void DoDifferentThing()
		{
		}
	}


}
