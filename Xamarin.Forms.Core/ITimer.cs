using System;

namespace Xamarin.Forms
{
	//this will go once Timer is included in Pcl profiles
	internal interface ITimer
	{
		void Change(int dueTime, int period);
		void Change(long dueTime, long period);
		void Change(TimeSpan dueTime, TimeSpan period);
		void Change(uint dueTime, uint period);
	}
}