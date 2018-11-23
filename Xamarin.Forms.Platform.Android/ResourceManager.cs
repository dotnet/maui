using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;
using Path = System.IO.Path;
using Xamarin.Forms.Internals;
using AndroidAppCompat = Android.Support.V7.Content.Res.AppCompatResources;

namespace Xamarin.Forms.Platform.Android
{
	public static class ResourceManager
	{
		public static Type DrawableClass { get; set; }

		public static Type ResourceClass { get; set; }

		public static Type StyleClass { get; set; }

		internal static Drawable GetFormsDrawable(this Context context, FileImageSource fileImageSource)
		{
			var file = fileImageSource.File;
			Drawable drawable = context.GetDrawable(fileImageSource);
			if (drawable == null)
			{
				var bitmap = GetBitmap(context.Resources, file) ?? BitmapFactory.DecodeFile(file);
				if (bitmap == null)
				{
					var source = Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(fileImageSource);
					bitmap = source.LoadImageAsync(fileImageSource, context).GetAwaiter().GetResult();
				}
				if (bitmap != null)
					drawable = new BitmapDrawable(context.Resources, bitmap);
			}
			return drawable;
		}

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

		[Obsolete("GetDrawable(this Resources, string) is obsolete as of version 2.5. "
			+ "Please use GetDrawable(this Context, string) instead.")]
		public static Drawable GetDrawable(this Resources resource, string name)
		{
			int id = IdFromTitle(name, DrawableClass);
			if (id == 0)
			{
				Log.Warning("Could not load image named: {0}", name);
				return null;
			}

			return AndroidAppCompat.GetDrawable(Forms.Context, id);
		}

		public static Drawable GetDrawable(this Context context, string name)
		{
			int id = IdFromTitle(name, DrawableClass);
			if (id == 0)
			{
				Log.Warning("Could not load image named: {0}", name);
				return null;
			}

			return AndroidAppCompat.GetDrawable(context, id);
		}

		public static int GetDrawableByName(string name)
		{
			return IdFromTitle(name, DrawableClass);
		}

		public static int GetResourceByName(string name)
		{
			return IdFromTitle(name, ResourceClass);
		}

		public static int GetStyleByName(string name)
		{
			return IdFromTitle(name, StyleClass);
		}

		public static void Init(Assembly masterAssembly)
		{
			DrawableClass = masterAssembly.GetTypes().FirstOrDefault(x => x.Name == "Drawable" || x.Name == "Resource_Drawable");
			ResourceClass = masterAssembly.GetTypes().FirstOrDefault(x => x.Name == "Id" || x.Name == "Resource_Id");
			StyleClass = masterAssembly.GetTypes().FirstOrDefault(x => x.Name == "Style" || x.Name == "Resource_Style");
			
		}

		internal static int IdFromTitle(string title, Type type)
		{
			string name = Path.GetFileNameWithoutExtension(title);
			int id = GetId(type, name);
			return id;
		}

		static int GetId(Type type, string memberName)
		{
			object value = type.GetFields().FirstOrDefault(p => p.Name == memberName)?.GetValue(type)
				?? type.GetProperties().FirstOrDefault(p => p.Name == memberName)?.GetValue(type);
			if (value is int)
				return (int)value;
			return 0;
		}
	}
}