using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	internal static class PerformanceDataManager
	{
		const string GetScenarioResultsRoute = "/api/ScenarioResults/device/";
		const string PostScenarioResultDetailsRoute = "/api/ScenarioResultDetails/";
		const string PostScenarioResultRoute = "/api/ScenarioResults/";
		static readonly HttpClient _client;

		static PerformanceDataManager()
		{
			_client = new HttpClient
			{
				BaseAddress = new Uri("http://xf-perf.azurewebsites.net/"),
				Timeout = TimeSpan.FromSeconds(5)
			};
			_client.DefaultRequestHeaders.Accept.Clear();
			_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}

		[Preserve(AllMembers = true)]
		public enum Result
		{
			Inconclusive,
			Pass,
			Fail
		}

		public static async Task<Dictionary<string, double>> GetScenarioResults(string deviceId)
		{
			var response = await _client.GetAsync(GetScenarioResultsRoute + deviceId);

			if (!response.IsSuccessStatusCode)
				return new Dictionary<string, double>();

			var content = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<Dictionary<string, double>>(content);
		}

		public static async Task PostScenarioResults(string scenarioName, Result result, Guid testRunReferenceId, string deviceIdentifier, string platform, string version, string idiom, string buildInfo, double totalMilliseconds, Dictionary<string, PerformanceProvider.Statistic> details)
		{
			var data = new ScenarioResult
			{
				Result = result,
				RunCompleted = DateTime.UtcNow,
				TestRunReferenceId = testRunReferenceId,
				DeviceIdentifier = deviceIdentifier,
				DevicePlatform = platform,
				DeviceVersion = version,
				DeviceIdiom = idiom,
				TotalMilliseconds = totalMilliseconds,
				BuildInfo = buildInfo
			};
			var json = JsonConvert.SerializeObject(data);
			var content = new StringContent(json, Encoding.UTF8, "application/json");
			var response = await _client.PostAsync(PostScenarioResultRoute + scenarioName, content);
			var responseContent = await response.Content.ReadAsStringAsync();

			var scenarioResult = JsonConvert.DeserializeObject<ScenarioResult>(responseContent);

			await PostScenarioResultDetails(scenarioResult.Id, details);
		}

		static async Task PostScenarioResultDetails(Guid scenarioResultId, Dictionary<string, PerformanceProvider.Statistic> details)
		{
			foreach (var detail in details)
			{
				var data = new ScenarioResultDetail
				{
					CallCount = detail.Value.CallCount,
					IsDetail = detail.Value.IsDetail,
					TotalMilliseconds = TimeSpan.FromTicks(detail.Value.TotalTime).TotalMilliseconds,
					ScenarioResultId = scenarioResultId,
					Name = detail.Key
				};
				var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
				await _client.PostAsync(PostScenarioResultDetailsRoute + scenarioResultId, content);
			}
		}

		[Preserve(AllMembers = true)]
		public class ScenarioResult
		{
			public string DeviceIdentifier { get; set; }
			public string DeviceIdiom { get; set; }
			public string DevicePlatform { get; set; }
			public string DeviceVersion { get; set; }
			public Guid Id { get; set; }
			public Result Result { get; set; }
			public DateTime RunCompleted { get; set; }
			public Guid ScenarioId { get; set; }
			public Guid TestRunReferenceId { get; set; }
			public double TotalMilliseconds { get; set; }
			public string BuildInfo { get; set; }
		}

		[Preserve(AllMembers = true)]
		public class ScenarioResultDetail
		{
			public int CallCount { get; set; }
			public Guid Id { get; set; }
			public bool IsDetail { get; set; }
			public string Name { get; set; }
			public Guid ScenarioResultId { get; set; }
			public double TotalMilliseconds { get; set; }
		}
	}
}