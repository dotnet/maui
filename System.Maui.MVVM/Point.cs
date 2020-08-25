using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Forms
{
	[TypeConverter(typeof(PointTypeConverter))]
	public partial struct Point
    {
    }
}
