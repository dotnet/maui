using System;

using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Xamarin.Forms.Core.UITests
{

// Potential cleanup of Raw Queries
//		public void MyTest(IApp app)
//		{
//			var viewName = "myView";
//
//			app.Query (x => x.Marked (viewName + " View").Parent ().Index (0).Sibling ().Index (1).Child (0).Child (0));
//
//			app.Query (x => x.Marked (viewName + " View").Parent (0).Sibling (1).Child (0).Child (0));
//
//			app.Query (x => x.LayeredHiddenButton ("mine").LayeredHiddenButton ("yours"));
//
//			app.Query (x => x.LayeredHiddenButton (viewName).Parentx(4));
//
//			app.ForAndroid (x => {
//				x.Back();
//			});
//		}


// Potential cleanup of Raw Queries
// make public or reflection will not pick up in REPL
//	internal static class Exts
//	{
//		public static void ForAndroid(this IApp app, Action<AndroidApp> action)
//		{
//			if (app is AndroidApp)
//			{
//				action (app as AndroidApp);
//			}
//		}
//
//		public static AppQuery LayeredHiddenButton(this AppQuery query, string viewName)
//		{
//			if(query.QueryPlatform == QueryPlatform.Android)
//			{
//				return query.Marked (viewName + " Android View").Parent (0).Sibling (1).Child (0).Child (0);
//
//			}
//			return query.Marked (viewName + " iOS View").Parent (0).Sibling (1).Child (0).Child (0);
//		}
//		 
//		public static AppQuery Parentx(this AppQuery query, int index)
//		{
//			return query.Parent ().Index (index);
//		}
//	}

}
