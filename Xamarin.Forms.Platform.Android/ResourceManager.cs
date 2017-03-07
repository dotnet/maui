using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;
using Path = System.IO.Path;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.Android
{
	public static class ResourceManager
	{
		public static Type DrawableClass { get; set; }

		public static Type ResourceClass { get; set; }

		public static Bitmap GetBitmap(this Resources resource, FileImageSource fileImageSource)
		{
			var file = fileImageSource.File;

			var bitmap = GetBitmap(resource, file);
			if (bitmap != null)
				return bitmap;

			return BitmapFactory.DecodeFile(file);
		}

		public static Bitmap GetBitmap(this Resources resource, string name)
		{
			return BitmapFactory.DecodeResource(resource, IdFromTitle(name, DrawableClass));
		}

		public static Task<Bitmap> GetBitmapAsync(this Resources resource, string name)
		{
			return BitmapFactory.DecodeResourceAsync(resource, IdFromTitle(name, DrawableClass));
		}

		public static Drawable GetDrawable(this Resources resource, string name)
		{
			int id = IdFromTitle(name, DrawableClass);
			if (id == 0)
			{
				Log.Warning("Could not load image named: {0}", name);
				return null;
			}
			return ContextCompat.GetDrawable(Forms.Context, id);
		}

		public static int GetDrawableByName(string name)
		{
			return IdFromTitle(name, DrawableClass);
		}

		public static int GetResourceByName(string name)
		{
			return IdFromTitle(name, ResourceClass);
		}

		public static void Init(Assembly masterAssembly)
		{
			DrawableClass = masterAssembly.GetTypes().FirstOrDefault(x => x.Name == "Drawable");
			ResourceClass = masterAssembly.GetTypes().FirstOrDefault(x => x.Name == "Id");
		}

		internal static int IdFromTitle(string title, Type type)
		{
			string name = Path.GetFileNameWithoutExtension(title);
			int id = GetId(type, name);
			return id; // Resources.System.GetDrawable (Resource.Drawable.dashboard);
		}

		static int GetId(Type type, string propertyName)
		{
			FieldInfo[] props = type.GetFields();
			FieldInfo prop = props.Select(p => p).FirstOrDefault(p => p.Name == propertyName);
			if (prop != null)
				return (int)prop.GetValue(type);
			return 0;
		}
	}
}