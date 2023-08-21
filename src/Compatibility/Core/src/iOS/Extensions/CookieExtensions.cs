//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;
using System.Linq;
using System.Net;
using Foundation;

#if __MOBILE__
namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
#else

namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
#endif
{
	internal static class CookieExtensions
	{
		public static Cookie ToCookie(this NSHttpCookie nscookie, bool setDomain = true)
		{
			Uri commentUri = null;
			if (nscookie.CommentUrl != null)
				commentUri = nscookie.CommentUrl;

			Cookie cookie = new Cookie()
			{
				Comment = nscookie.Comment,
				CommentUri = commentUri,
				Expires = nscookie.ExpiresDate.ToDateTime(),
				HttpOnly = nscookie.IsHttpOnly,
				Name = nscookie.Name,
				Path = nscookie.Path,
				Secure = nscookie.IsSecure,
				Value = nscookie.Value,
				Version = (int)nscookie.Version,
				Port = String.Join(",", nscookie.PortList.Select(x => x.ToString()))
			};

			if (setDomain)
				cookie.Domain = nscookie.Domain;

			return cookie;
		}
	}
}