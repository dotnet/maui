namespace Maui.Controls.Sample.HybridWebApp.Playground;

sealed class SampleCounter
{
	int _value;
	public int Value => Volatile.Read(ref _value);
	public int Increment() => Interlocked.Increment(ref _value);
}
