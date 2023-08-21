// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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