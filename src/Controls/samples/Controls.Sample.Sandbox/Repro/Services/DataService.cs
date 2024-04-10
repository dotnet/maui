using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CollectionViewPerformanceMaui.Models;

namespace CollectionViewPerformanceMaui.Services
{
	public sealed class DataService : IDataService
	{
		public async Task<List<Data>> GetData()
		{
			await Task.Delay(2000);

			return Enumerable.Range(0, 100)
				.Select(x => new Data())
				.ToList();
		}
	}
}
