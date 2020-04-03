using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ClickAndClick.DataAccess
{
	public class APIHelper
	{
		private static Uri baseAddress;
		private static string apiName;
		private static CookieContainer cookies;

		public static void Open(string baseUrl, string apiName)
		{
			cookies = new CookieContainer();
			baseAddress = new Uri(baseUrl);
			APIHelper.apiName = apiName;

			cookies.Add(baseAddress, new Cookie("UserName", NCR.MyNcr.User.UserName));
			cookies.Add(baseAddress, new Cookie("ProxyId", NCR.MyNcr.User.Identity.ProxyId));
		}

		/// <summary>
		/// Makes a HTTP Get Request to API in order to get info
		/// </summary>
		/// <param name="url">Url after base. Do not add forward or back slashes.  e.g: tracapi/entities/27</param>
		/// <param name="querystring">the querystring of request if one is needed.  e.g: showAll&pie=Berry</param>
		/// <returns></returns>
		public static T GetRequest<T>(string url, string querystring = null)
		{
			T result = default(T);

			using (HttpClientHandler handler = new HttpClientHandler() { CookieContainer = cookies })
			{
				using (HttpClient client = new HttpClient(handler) { BaseAddress = baseAddress })
				{
					string finalUrl = baseAddress.AbsoluteUri + "/" + apiName + "/" + url;
					if (!string.IsNullOrWhiteSpace(querystring))
					{
						finalUrl += "?" + querystring;
					}
					string response = Task.Run(async () => { return await client.GetStringAsync(finalUrl); }).Result;
					result = JsonConvert.DeserializeObject<T>(response);
				}
			}

			return result;
		}

		/// <summary>
		/// Makes a HTTP Get Request to API in order to get info
		/// </summary>
		/// <param name="url">Url after base. Do not add forward or back slashes.  e.g: tracapi/entities/rename</param>
		/// <param name="data">The payload to the post request</param>
		/// <returns></returns>
		public static T PostRequest<T>(string url, object data)
		{
			T result = default(T);

			using (HttpClientHandler handler = new HttpClientHandler() { CookieContainer = cookies })
			{
				using (HttpClient client = new HttpClient(handler) { BaseAddress = baseAddress })
				{
					string finalUrl = baseAddress.AbsoluteUri + "/" + apiName + "/" + url;
					HttpContent content = null;
					if (data != null)
					{
						string json = JsonConvert.SerializeObject(data);
						content = new StringContent(json, Encoding.UTF8, "application/json");
					}

					HttpResponseMessage response = Task.Run(async () => { return await client.PostAsync(finalUrl, content); }).Result;
					string responseContent = Task.Run(async () => { return await response.Content.ReadAsStringAsync(); }).Result;

					result = JsonConvert.DeserializeObject<T>(responseContent);
				}
			}

			return result;
		}

		/// <summary>
		/// Makes a HTTP Get Request to API, and gets whether or not successful
		/// </summary>
		/// <param name="url">Url after base. Do not add forward or back slashes.  e.g: tracapi/entities/rename</param>
		/// <param name="data">The payload to the post request</param>
		public static bool PostRequest(string url, object data)
		{
			bool result = false;
			using (HttpClientHandler handler = new HttpClientHandler() { CookieContainer = cookies })
			{
				using (HttpClient client = new HttpClient(handler) { BaseAddress = baseAddress })
				{
					string finalUrl = baseAddress.AbsoluteUri + "/" + apiName + "/" + url;
					HttpContent content = null;
					if (data != null)
					{
						string json = JsonConvert.SerializeObject(data);
						content = new StringContent(json, Encoding.UTF8, "application/json");
					}

					HttpResponseMessage response = Task.Run(async () => { return await client.PostAsync(finalUrl, content); }).Result;
					result = response.IsSuccessStatusCode;
				}
			}
			return result;
		}

		public static T PostFile<T>(string url, string filepath, params KeyValuePair<string, string>[] formFields)
		{
			T result = default(T);
			using (HttpClientHandler handler = new HttpClientHandler() { CookieContainer = cookies })
			{
				using (HttpClient client = new HttpClient(handler) { BaseAddress = baseAddress })
				{
					using (MultipartFormDataContent content = new MultipartFormDataContent())
					{
						string finalUrl = baseAddress.AbsoluteUri + "/" + apiName + "/" + url;
						foreach (KeyValuePair<string, string> formField in formFields)
						{
							HttpContent objectContent = new StringContent(formField.Value, Encoding.UTF8, "application/text");
							content.Add(objectContent, formField.Key);
						}

						Stream fileStream = File.OpenRead(filepath);
						HttpContent fileStreamContent = new StreamContent(fileStream);
						content.Add(fileStreamContent, "file", "file");

						HttpResponseMessage response = Task.Run(async () => { return await client.PostAsync(finalUrl, content); }).Result;
						string responseContent = Task.Run(async () => { return await response.Content.ReadAsStringAsync(); }).Result;

						result = JsonConvert.DeserializeObject<T>(responseContent);
					}
				}
			}
			return result;
		}
	}
}
