using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Linq;

namespace InnoviApiProxy
{
    internal static class HttpUtils
    {
        internal static HttpClient BaseHttpClient()
        {
            HttpClient myClient = new HttpClient();
            myClient.BaseAddress = new Uri(Settings.InnoviApiEndpoint);
            myClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            myClient.DefaultRequestHeaders.TryAddWithoutValidation("X-API-KEY", Settings.ApiKey);
            myClient.Timeout = new TimeSpan(0, 0, 15);
            return myClient;
        }

        private static void checkForTimeout(HttpResponseMessage i_HttpResponse)
        {
            if (i_HttpResponse.StatusCode == System.Net.HttpStatusCode.GatewayTimeout)
            {
                throw new HttpRequestException("Could not get a response from the server");
            }
        }

        internal static JObject GetHttpResponseBody(HttpResponseMessage i_HttpResponseMessage)
        {

            checkForTimeout(i_HttpResponseMessage);
            Task<string> responseAsString = i_HttpResponseMessage.Content.ReadAsStringAsync();
            string str = responseAsString.Result;
            JObject responseJsonObject = null;

            try
            {
                responseJsonObject = JObject.Parse(str);
            }
            catch(JsonReaderException)
            {
                responseJsonObject = null;
            }

            return responseJsonObject;
        }

        internal static bool IsUrlFound(string i_Url)
        {
            bool isUrlFound = true;

            verifyLoggedInStatus();
            HttpClient client = BaseHttpClient();
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, i_Url);

            Task<HttpResponseMessage> result = client.SendAsync(httpRequest);
            HttpResponseMessage response = result.Result;
            var statusCode = response.StatusCode;
            
            if(statusCode == System.Net.HttpStatusCode.NotFound)
            {
                isUrlFound = false;
            }

         
            return isUrlFound;
        }

        private static void AddSensorNameToEvent(SensorEvent i_SensorEvent)
        {
            Cache sensorNameCache = Cache.Fetch();

            i_SensorEvent.SensorName = sensorNameCache.GetSensorName(i_SensorEvent.SensorId);
        }

        private static void AddSensorNamesToEventsList(List<SensorEvent> i_SensorEvents)
        {
            List<int> sensorIds = new List<int>();

            foreach (SensorEvent sensorEvent in i_SensorEvents)
            {
                sensorIds.Add(sensorEvent.SensorId);
            }

            UpdateSensorNamesCache(sensorIds);

            Cache sensorCache = Cache.Fetch();

            foreach (SensorEvent sensorEvent in i_SensorEvents)
            {
                sensorEvent.SensorName = sensorCache.GetSensorName(sensorEvent.SensorId);
            }
        }

        private static void verifyLoggedInStatus()
        {
            if (InnoviApiService.AccessToken == null)
            {
                throw new InvalidOperationException("Not logged in");
            }
        }

        private static void verifyCodeZero(JObject i_HttpResponseBody)
        {
            if (i_HttpResponseBody["code"].ToString() != "0")
            {
                throw new HttpRequestException("server error - " + i_HttpResponseBody["code"].ToString());
            }
        }

        internal static Sensor GetSensorByID(int i_SensorId)
        {
            verifyLoggedInStatus();

            HttpClient client = BaseHttpClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-ACCESS-TOKEN", InnoviApiService.AccessToken);
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, Settings.ApiVersionEndpoint +
                "sensors/" + i_SensorId.ToString());
            Task<HttpResponseMessage> result = client.SendAsync(httpRequest);
            HttpResponseMessage response = result.Result;
            JObject responseJsonObject = GetHttpResponseBody(response);

            verifyCodeZero(responseJsonObject);
            InnoviApiService.RefreshAccessToken(response);
            Sensor sensor = JsonConvert.DeserializeObject<Sensor>(responseJsonObject["entity"].ToString());

            return sensor;
        }

        internal static string GetSensorNameById(int i_SensorId)
        {
            Sensor sensor = GetSensorByID(i_SensorId);

            string sensorName = "Unavailable";
            if (sensor != null)
            {
                sensorName = sensor.Name;
            }

            return sensorName;
        }

        internal static void UpdateSensorNamesCache(List<int> i_SensorIds)
        {
            verifyLoggedInStatus();

            if (i_SensorIds.Count < 1)
            {
                throw new ArgumentException("List must contain at least one value");
            }

            HttpClient client = BaseHttpClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-ACCESS-TOKEN", InnoviApiService.AccessToken);
            string baseUri = Settings.ApiVersionEndpoint + "sensors/list?";
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, baseUri);
            StringBuilder requestUriBuilder = new StringBuilder();
            requestUriBuilder.Append(baseUri);
            int sensorCount = 0;
            Dictionary<int, int> addedSensors = new Dictionary<int, int>();

            Cache cache = Cache.Fetch();

            foreach (int SensorId in i_SensorIds)
            {
                if (!addedSensors.ContainsKey(SensorId))
                {
                    addedSensors.Add(SensorId, 0);

                    if (sensorCount++ > 0)
                    {
                        requestUriBuilder.Append("&");
                    }

                    requestUriBuilder.Append("id=" + SensorId.ToString());
                }
            }

            httpRequest.RequestUri = new Uri(requestUriBuilder.ToString(), UriKind.Relative);

            Task<HttpResponseMessage> result = client.SendAsync(httpRequest);
            HttpResponseMessage response = result.Result;
            JObject responseJsonObject = GetHttpResponseBody(response);

            verifyCodeZero(responseJsonObject);
            InnoviApiService.RefreshAccessToken(response);
            List<Sensor> sensors = JsonConvert.DeserializeObject<List<Sensor>>(responseJsonObject["list"].ToString());

          

            List<string> sensorNames = new List<string>();

            foreach (Sensor sensor in sensors)
            {
                cache.AddToSensorCache(sensor.sensorId, sensor.Name);
            }
        }

        internal static bool SwitchAccount(int i_AccountId)
        {
            bool isSuccessful = false;

            verifyLoggedInStatus();
            HttpClient client = BaseHttpClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-ACCESS-TOKEN", InnoviApiService.AccessToken); 
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, Settings.ApiVersionEndpoint +"user/switch-account");

            Dictionary<string, string> jsonBuilder = new Dictionary<string, string>();
            jsonBuilder.Add("accountId", i_AccountId.ToString());
            string requestBody = JsonConvert.SerializeObject(jsonBuilder);
            httpRequest.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            Task<HttpResponseMessage> result = client.SendAsync(httpRequest);
            HttpResponseMessage response = result.Result;
            JObject responseJsonObject = GetHttpResponseBody(response);

            verifyCodeZero(responseJsonObject);
            InnoviApiService.RefreshAccessToken(response);
            isSuccessful = responseJsonObject["code"].ToString() == "0";

            return isSuccessful;
        }

        internal static List<SensorEvent> GetFolderEvents(int i_FolderId, int i_PageId, out int i_PagesCount)
        {
            verifyLoggedInStatus();
            HttpClient client = BaseHttpClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-ACCESS-TOKEN", InnoviApiService.AccessToken); 

            return getFolderEventsHepler(client, 0,  i_PageId, out i_PagesCount);
        }

        private static List<SensorEvent> getFolderEventsHepler(HttpClient i_Client, int i_FolderId, int i_PageId, out int i_PagesCount)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, Settings.ApiVersionEndpoint +
               "events?folderId=" + i_FolderId.ToString() + "&page=" + i_PageId.ToString());
            Task<HttpResponseMessage> result = i_Client.SendAsync(httpRequest);
            HttpResponseMessage response = result.Result;

            JObject responseJsonObject = GetHttpResponseBody(response);
            i_PagesCount = int.Parse(responseJsonObject["pages"].ToString());

            List<SensorEvent> events = JsonConvert.DeserializeObject<List<SensorEvent>>(responseJsonObject["list"].ToString());

            verifyCodeZero(responseJsonObject);
            InnoviApiService.RefreshAccessToken(response);

            if (events.Count == 0)
            {
                events = null;
            }
            else
            {
                AddSensorNamesToEventsList(events);
            }

            return events;
        }

        internal static List<Sensor> GetFolderSensors(int i_FolderId, int i_PageId, out int i_PagesCount)
        {
            verifyLoggedInStatus();
            HttpClient client = BaseHttpClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-ACCESS-TOKEN", InnoviApiService.AccessToken);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, Settings.ApiVersionEndpoint +
                "sensors?page=" + i_PageId.ToString() + "&folder=" + i_FolderId.ToString() + "&sort=name+");

            return getFolderSensorsHelper(client, i_FolderId, i_PageId, out i_PagesCount, httpRequest);
        }

        internal static List<Sensor.Health> GetSensorHealthArray (int i_SensorId)
        {
            verifyLoggedInStatus();
            HttpClient client = BaseHttpClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-ACCESS-TOKEN", InnoviApiService.AccessToken);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, Settings.ApiVersionEndpoint +
                "sensors/" + i_SensorId.ToString() + "/status/overtime");

            Task <HttpResponseMessage> result = client.SendAsync(httpRequest);
            HttpResponseMessage response = result.Result;
            JObject responseJsonObject = GetHttpResponseBody(response);

            List<Sensor.Health> HealthArray = JsonConvert.DeserializeObject<List<Sensor.Health>>(responseJsonObject["entity"]["data"].ToString());

            verifyCodeZero(responseJsonObject);
            InnoviApiService.RefreshAccessToken(response);

            if (HealthArray.Count == 0)
            {
                HealthArray = null;
            }

            return HealthArray;

        }

        internal static List<Sensor> GetAllFolderSensors(int i_FolderId, int i_PageId, out int i_PagesCount)
        {
            verifyLoggedInStatus();
            HttpClient client = BaseHttpClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-ACCESS-TOKEN", InnoviApiService.AccessToken);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, Settings.ApiVersionEndpoint +
                "sensors?page=" + i_PageId.ToString() + "&folder=" + i_FolderId.ToString() + "&subFolders=true" + "&sort=name+");

            return getFolderSensorsHelper(client, i_FolderId, i_PageId, out i_PagesCount, httpRequest);
        }

        private static List<Sensor> getFolderSensorsHelper(HttpClient i_Client, int i_FolderId, int i_PageId, out int i_PagesCount, HttpRequestMessage i_HttpRequest)
        {
            Task<HttpResponseMessage> result = i_Client.SendAsync(i_HttpRequest);
            HttpResponseMessage response = result.Result;
            JObject responseJsonObject = GetHttpResponseBody(response);

            i_PagesCount = int.Parse(responseJsonObject["pages"].ToString());

            List<Sensor> sensors = JsonConvert.DeserializeObject<List<Sensor>>(responseJsonObject["list"].ToString());

            verifyCodeZero(responseJsonObject);
            InnoviApiService.RefreshAccessToken(response);

            if (sensors.Count == 0)
            {
                sensors = null;
            }

            return sensors;
        }

        internal static List<Folder> GetFolders(int i_FolderId, int i_PageId, out int i_PagesCount)
        {
            verifyLoggedInStatus();
            HttpClient client = BaseHttpClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-ACCESS-TOKEN", InnoviApiService.AccessToken); 
          

            return GetFoldersHelper(client, i_FolderId, i_PageId, out i_PagesCount);
        }

        private static List<Folder> GetFoldersHelper(HttpClient i_Client, int i_FolderId, int i_PageId, out int i_PagesCount)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, Settings.ApiVersionEndpoint + 
                "folders/" + i_FolderId.ToString() + "/folders" + "?page=" + i_PageId.ToString() + "&sort=name+");


            Task <HttpResponseMessage> result = i_Client.SendAsync(httpRequest);
            HttpResponseMessage response = result.Result;
            JObject responseJsonObject = GetHttpResponseBody(response);

            i_PagesCount = int.Parse(responseJsonObject["pages"].ToString());

            List<Folder> folders = JsonConvert.DeserializeObject<List<Folder>>(responseJsonObject["list"].ToString());

            verifyCodeZero(responseJsonObject);
            InnoviApiService.RefreshAccessToken(response);

            if (folders.Count == 0)
            {
                folders = null;
            }

            return folders;
        }

        internal static List<SensorEvent> GetSensorEvents(int i_SensorId, int i_PageId, out int i_PagesCount)
        {
            verifyLoggedInStatus();
            HttpClient client = BaseHttpClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-ACCESS-TOKEN", InnoviApiService.AccessToken); 
          
            return GetSensorEventsHelper(client, i_SensorId, i_PageId, out i_PagesCount);
        }

        private static List<SensorEvent> GetSensorEventsHelper(HttpClient i_Client, int i_SensorId, int i_PageId, out int i_PagesCount)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, Settings.ApiVersionEndpoint + 
                "events?sensorId=" + i_SensorId.ToString() + "&page=" + i_PageId.ToString());


            Task<HttpResponseMessage> result = i_Client.SendAsync(httpRequest);
            HttpResponseMessage response = result.Result;
            JObject responseJsonObject = GetHttpResponseBody(response);

            i_PagesCount = int.Parse(responseJsonObject["pages"].ToString());
            InnoviApiService.RefreshAccessToken(response);
            List<SensorEvent> events = JsonConvert.DeserializeObject<List<SensorEvent>>(responseJsonObject["list"].ToString());

            verifyCodeZero(responseJsonObject);
            InnoviApiService.RefreshAccessToken(response);

            if (events.Count == 0)
            {
                events = null;
            }
            else
            {
                AddSensorNamesToEventsList(events);
            }

            return events;
        }

        internal static byte[] GetSensorReferenceImage(int i_AccountId, int i_SensorId)
        {
            HttpClient myClient = new HttpClient();
            myClient.BaseAddress = new Uri(Settings.InnoviApiEndpoint);
            myClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/jpeg"));
            myClient.DefaultRequestHeaders.TryAddWithoutValidation("X-API-KEY", Settings.ApiKey);
            myClient.Timeout = new TimeSpan(0, 0, 15);

            myClient.DefaultRequestHeaders.TryAddWithoutValidation("X-ACCESS-TOKEN", InnoviApiService.AccessToken);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, "sensorImage?accountId=" + i_AccountId.ToString() +
                "&sensorId=" + i_SensorId.ToString());


            Task<HttpResponseMessage> result = myClient.SendAsync(httpRequest);
            HttpResponseMessage response = result.Result;
            Task<byte[]> responseAsByteArray = response.Content.ReadAsByteArrayAsync();

            return responseAsByteArray.Result;
        }
    }
}
