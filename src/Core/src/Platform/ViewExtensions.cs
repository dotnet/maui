using System;
using System.Numerics;
using Microsoft.Maui.Graphics;
#if NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using INativeViewHandler = Microsoft.Maui.IViewHandler;
#endif

namespace Microsoft.Maui
{
	/// <include file="../../docs/Microsoft.Maui/ViewExtensions.xml" path="Type[@FullName='Microsoft.Maui.ViewExtensions']/Docs" />
	public static partial class ViewExtensions
	{
		internal static Vector3 ExtractPosition(this Matrix4x4 matrix) => matrix.Translation;

		internal static Vector3 ExtractScale(this Matrix4x4 matrix) => new Vector3(matrix.M11, matrix.M22, matrix.M33);

		internal static double ExtractAngleInRadians(this Matrix4x4 matrix) => Math.Atan2(matrix.M21, matrix.M11);

		internal static double ExtractAngleInDegrees(this Matrix4x4 matrix) => ExtractAngleInRadians(matrix) * 180 / Math.PI;

		/// <include file="../../docs/Microsoft.Maui/ViewExtensions.xml" path="//Member[@MemberName='ToHandler']/Docs" />
		public static INativeViewHandler ToHandler(this IView view, IMauiContext context) =>
			(INativeViewHandler)ElementExtensions.ToHandler(view, context);
	}
}
