using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Core.UITests
{
	internal abstract partial class BaseViewContainerRemote
	{
		bool TryConvertFont<T>(object prop, out T result)
		{
			result = default(T);

			if (prop.GetType() == typeof(string) && typeof(T) == typeof(Font))
			{
				Font font = ParsingUtils.ParseUIFont((string)prop);
				result = (T)((object)font);

				return true;
			}

			return false;
		}
	}
}
