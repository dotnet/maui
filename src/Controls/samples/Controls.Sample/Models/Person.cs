namespace Maui.Controls.Sample.Models
{
	public class Person
	{
		public string Name { get; private set; }

		public int Age { get; private set; }

		public string Location { get; private set; }

		public Person(string name, int age, string location)
		{
			Name = name;
			Age = age;
			Location = location;
		}
	}
}