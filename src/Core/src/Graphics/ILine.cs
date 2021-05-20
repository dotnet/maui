namespace Microsoft.Maui.Graphics
{
	public interface ILine : IShape
	{
		public double X1 { get; set; }

		public double Y1 { get; set; }

		public double X2 { get; set; }

		public double Y2 { get; set; }
	}
}