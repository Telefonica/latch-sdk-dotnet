/*
    Latch C# SDK - Set of  reusable classes to  allow developers integrate Latch on their applications.
    Copyright (C) 2023 Telefonica Digital.
 
    This library is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either
    version 2.1 of the License, or (at your option) any later version.
 
    This library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
    Lesser General Public License for more details.
 
    You should have received a copy of the GNU Lesser General Public
    License along with this library; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA

*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Net;
using System.Web;
using System.Threading.Tasks;

namespace LatchSDK
{
    public abstract class LatchBase
    {
        private const string API_VERSION = "2.0";
        private static string apiHost = "https://latch.tu.com";
        protected static string API_HOST { get { return apiHost; } }

        protected const string API_BASE_URL = "/api/" + API_VERSION;
        protected const string API_CHECK_STATUS_URL = API_BASE_URL + "/status";
        protected const string API_PAIR_URL = API_BASE_URL + "/pair";
        protected const string API_PAIR_WITH_ID_URL = API_BASE_URL + "/pairWithId";
        protected const string API_UNPAIR_URL = API_BASE_URL + "/unpair";
        protected const string API_LOCK_URL = API_BASE_URL + "/lock";
        protected const string API_UNLOCK_URL = API_BASE_URL + "/unlock";
        protected const string API_HISTORY_URL = API_BASE_URL + "/history";
        protected const string API_OPERATION_URL = API_BASE_URL + "/operation";
        protected const string API_TOTP_URL = API_BASE_URL + "/totps";
        protected const string API_Q_SECRET_URL = API_BASE_URL + "/qsecrets";

        protected const string AUTHORIZATION_HEADER_NAME = "Authorization";
        protected const string DATE_HEADER_NAME = "X-11Paths-Date";
        protected const string AUTHORIZATION_METHOD = "11PATHS";
        private const char AUTHORIZATION_HEADER_FIELD_SEPARATOR = ' ';

        protected const string UTC_STRING_FORMAT = "yyyy-MM-dd HH:mm:ss";

        protected const string X_11PATHS_HEADER_PREFIX = "X-11paths-";
        private const char X_11PATHS_HEADER_SEPARATOR = ':';
        private const char PARAM_SEPARATOR = '&';
        private const char PARAM_VALUE_SEPARATOR = '=';

        private string appId;
        private string secretKey;

        protected enum HttpMethod { GET, POST, PUT, DELETE }
        public enum FeatureMode { MANDATORY, OPT_IN, DISABLED }

        /// <summary>
        /// Creates an instance of the class with the <code>Application ID</code> and <code>Secret</code> obtained from Telefonica Digital 
        /// </summary>
        protected LatchBase(string appId, string secretKey)
        {
            this.appId = appId;
            this.secretKey = secretKey;
#if NETFRAMEWORK
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
#endif
        }

        /// <summary>
        /// Sets the host of the Latch API backend
        /// </summary>
        /// <param name="host">The host of the Latch API backend in standard URI format (e.g: "https://latch.tu.com")</param>
        public static void SetHost(string host)
        {
            if (string.IsNullOrEmpty(host))
                return;

            apiHost = host.TrimEnd('/');
        }

        /// <summary>
        /// The custom header consists of three parts: the method, the application ID and the signature. 
        /// This method returns the specified part if it exists.
        /// </summary>
        /// <param name="part">The zero indexed part to be returned</param>
        /// <param name="header">The HTTP header value from which to extract the part</param>
        /// <returns>The specified part from the header or an empty string if not existent</returns>
        private static string GetPartFromHeader(int part, string header)
        {
            if (header != null)
            {
                string[] parts = header.Split(AUTHORIZATION_HEADER_FIELD_SEPARATOR);
                if (parts.Length > part)
                {
                    return parts[part];
                }
            }
            return "";
        }

        /// <summary>
        /// Extracts the authorization method from the authorization header (the first parameter)
        /// </summary>
        /// <param name="authorizationHeader">Authorization HTTP Header</param>
        /// <returns>The Authorization method. Typical values are "Basic", "Digest" or "11PATHS"</returns>
        private static string GetAuthMethodFromHeader(string authorizationHeader)
        {
            return GetPartFromHeader(0, authorizationHeader);
        }

        /// <summary>
        /// Extracts the application ID from the authorization header (the second parameter)
        /// </summary>
        /// <param name="authorizationHeader">Authorization HTTP Header</param>
        /// <returns>The requesting application Id. Identifies the application using the API</returns>
        private static string GetAppIdFromHeader(string authorizationHeader)
        {
            return GetPartFromHeader(1, authorizationHeader);
        }

        /// <summary>
        /// Extracts the signature from the authorization header (the third parameter)
        /// </summary>
        /// <param name="authorizationHeader">Authorization HTTP Header</param>
        /// <returns>The signature of the current request. Verifies the identity of the application using the API</returns>
        private static string GetSignatureFromHeader(string authorizationHeader)
        {
            return GetPartFromHeader(2, authorizationHeader);
        }

        /// <summary>
        /// Performs an HTTP request to an URL using the specified method, headers and data, returning the response as a string
        /// </summary>
        private static string HttpPerformRequest(string url, HttpMethod method, IDictionary<string, string> headers, IDictionary<string, string> data)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method.ToString();
            foreach (string key in headers.Keys)
            {
                if (key.Equals("Authorization", StringComparison.InvariantCultureIgnoreCase))
                {
                    request.Headers.Add(HttpRequestHeader.Authorization, headers[key]);
                }
                else if (key.Equals("Date", StringComparison.InvariantCultureIgnoreCase))
                {
                    request.Date = DateTime.Parse(headers[key], null, System.Globalization.DateTimeStyles.AssumeUniversal);
                }
                else
                {
                    request.Headers.Add(key, headers[key]);
                }
            }

            try
            {
                if (method == HttpMethod.POST || method == HttpMethod.PUT)
                {
                    request.ContentType = "application/x-www-form-urlencoded";
                    using (StreamWriter sw = new StreamWriter(request.GetRequestStream()))
                    {
                        sw.Write(GetSerializedParams(data));
                        sw.Flush();
                    }
                }

                using (StreamReader sr = new StreamReader(request.GetResponse().GetResponseStream()))
                {
                    return sr.ReadToEnd();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Performs an HTTP request to an URL using the specified method and data, returning the response as a string
        /// </summary>
        protected LatchResponse HttpPerformRequest(string url, HttpMethod method = HttpMethod.GET, IDictionary<string, string> data = null)
        {
            var authHeaders = AuthenticationHeaders(method.ToString(), url, null, data);
            return new LatchResponse(HttpPerformRequest(apiHost + url, method, authHeaders, data));
        }

#if NETSTANDARD || NET45_OR_GREATER
        /// <summary>
        /// Performs an async HTTP request to an URL using the specified method, headers and data, returning the response as a string
        /// </summary>
        private static async Task<string> HttpPerformRequestAsync(string url, HttpMethod method, IDictionary<string, string> headers, IDictionary<string, string> data)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method.ToString();
            foreach (string key in headers.Keys)
            {
                if (key.Equals("Authorization", StringComparison.InvariantCultureIgnoreCase))
                {
                    request.Headers.Add(HttpRequestHeader.Authorization, headers[key]);
                }
                else if (key.Equals("Date", StringComparison.InvariantCultureIgnoreCase))
                {
                    request.Date = DateTime.Parse(headers[key], null, System.Globalization.DateTimeStyles.AssumeUniversal);
                }
                else
                {
                    request.Headers.Add(key, headers[key]);
                }
            }

            try
            {
                if (method == HttpMethod.POST || method == HttpMethod.PUT)
                {
                    request.ContentType = "application/x-www-form-urlencoded";
                    using (StreamWriter sw = new StreamWriter(await request.GetRequestStreamAsync()))
                    {
                        await sw.WriteAsync(GetSerializedParams(data));
                        sw.Flush();
                    }
                }

                using (StreamReader sr = new StreamReader((await request.GetResponseAsync()).GetResponseStream()))
                {
                    return await sr.ReadToEndAsync();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected async Task<LatchResponse> HttpPerformRequestAsync(string url, HttpMethod method = HttpMethod.GET, IDictionary<string, string> data = null)
        {
            var authHeaders = AuthenticationHeaders(method.ToString(), url, null, data);
            return new LatchResponse(await HttpPerformRequestAsync(apiHost + url, method, authHeaders, data));
        }
#endif

        /// <summary>
        /// Signs the data provided in order to prevent tampering
        /// </summary>
        /// <param name="data">The string to sign</param>
        /// <returns>Base64 encoding of the HMAC-SHA1 hash of the data parameter using <code>secretKey</code> as cipher key.</returns>              
        private string SignData(string data)
        {
            try
            {
                HMACSHA1 hmacSha1 = new HMACSHA1(Encoding.ASCII.GetBytes(secretKey));
                return Convert.ToBase64String(hmacSha1.ComputeHash(Encoding.ASCII.GetBytes(data)));
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Calculates the authentication headers to be sent with a request to the API 
        /// </summary>
        /// <param name="httpMethod">The HTTP Method. Currently only GET is supported</param>
        /// <param name="queryString">The urlencoded string including the path (from the first forward slash) and the parameters</param>
        /// <param name="xHeaders">HTTP headers specific to the 11-paths API. Null if not needed.</param>
        /// <returns>An IDictionary with the Authorization and Date headers needed to sign a Latch API request</returns>
        protected IDictionary<string, string> AuthenticationHeaders(string httpMethod, string queryString, IDictionary<string, string> xHeaders, IDictionary<string, string> parameters)
        {
            return AuthenticationHeaders(httpMethod, queryString, xHeaders, parameters, GetCurrentUTC());
        }

        /// <summary>
        /// Calculates the authentication headers to be sent with a request to the API 
        /// </summary>
        /// <param name="httpMethod">The HTTP Method. Currently only GET is supported</param>
        /// <param name="queryString">The urlencoded string including the path (from the first forward slash) and the parameters</param>
        /// <param name="xHeaders">HTTP headers specific to the 11-paths API. Null if not needed.</param>
        /// <param name="utc">The Universal Coordinated Time for the Date HTTP header</param>
        /// <returns>An IDictionary with the Authorization and Date headers needed to sign a Latch API request</returns>
        protected IDictionary<string, string> AuthenticationHeaders(string httpMethod, string queryString, IDictionary<string, string> xHeaders, IDictionary<string, string> parameters, string utc)
        {
            StringBuilder stringToSign = new StringBuilder()
                .Append(httpMethod.ToUpper().Trim()).Append("\n")
                .Append(utc).Append("\n")
                .Append(GetSerializedHeaders(xHeaders)).Append("\n")
                .Append(queryString.Trim());

            if (parameters != null && parameters.Count > 0)
            {
                string serializedParams = GetSerializedParams(parameters);
                if (!string.IsNullOrEmpty(serializedParams))
                {
                    stringToSign.Append("\n").Append(serializedParams);
                }
            }

            string signedData = SignData(stringToSign.ToString());

            string authorizationHeader = new StringBuilder(AUTHORIZATION_METHOD)
                .Append(AUTHORIZATION_HEADER_FIELD_SEPARATOR)
                .Append(this.appId)
                .Append(AUTHORIZATION_HEADER_FIELD_SEPARATOR)
                .Append(signedData)
                .ToString();

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add(AUTHORIZATION_HEADER_NAME, authorizationHeader);
            headers.Add(DATE_HEADER_NAME, utc);
            return headers;
        }


        /// <summary>
        /// Prepares and returns a string ready to be signed from the 11-paths specific HTTP headers received 
        /// </summary>
        /// <param name="xHeaders">A non necessarily sorted IDictionary of the HTTP headers</param>
        /// <returns>A string with the serialized headers, an empty string if no headers are passed, or null if there's a problem
        ///  such as non specific 11paths headers</returns>
        private static string GetSerializedHeaders(IDictionary<string, string> xHeaders)
        {
            if (xHeaders != null && xHeaders.Count > 0)
            {
                SortedDictionary<string, string> sorted = new SortedDictionary<string, string>();

                foreach (string key in xHeaders.Keys)
                {
                    if (!key.StartsWith(X_11PATHS_HEADER_PREFIX, StringComparison.InvariantCultureIgnoreCase))
                    {
                        throw new ApplicationException("Error serializing headers. Only specific " + X_11PATHS_HEADER_PREFIX + " headers need to be signed");
                    }
                    sorted.Add(key.ToLower(), xHeaders[key].Replace('\n', ' '));
                }

                StringBuilder serializedHeaders = new StringBuilder();
                foreach (string key in sorted.Keys)
                {
                    serializedHeaders.Append(key).Append(X_11PATHS_HEADER_SEPARATOR).Append(sorted[key]).Append(AUTHORIZATION_HEADER_FIELD_SEPARATOR);
                }

                return serializedHeaders.ToString().Trim(AUTHORIZATION_HEADER_FIELD_SEPARATOR);
            }
            else
            {
                return "";
            }
        }


        /// <summary>
        /// Prepares and returns a string ready to be signed from the parameters of an HTTP request
        /// </summary>
        /// <param name="parameters">A non necessarily sorted IDictionary of the parameters</param>
        /// <returns>A string with the serialized parameters, an empty string if no headers are passed</returns>
        /// <remarks> The params must be only those included in the body of the HTTP request when its content type
        ///     is application/x-www-urlencoded and must be urldecoded. </remarks>
        private static string GetSerializedParams(IDictionary<string, string> parameters)
        {
            if (parameters != null && parameters.Count > 0)
            {
                SortedDictionary<string, string> sorted = new SortedDictionary<string, string>(parameters);

                StringBuilder serializedParams = new StringBuilder();
                foreach (string key in sorted.Keys)
                {
                    serializedParams.Append(UrlEncode(key)).Append(PARAM_VALUE_SEPARATOR)
                                    .Append(UrlEncode(sorted[key])).Append(PARAM_SEPARATOR);
                }

                return serializedParams.ToString().Trim(PARAM_SEPARATOR);
            }
            else
            {
                return "";
            }
        }


        /// <summary>
        /// Returns a string representation of the current time in UTC to be used in a Date HTTP Header
        /// </summary>
        private static string GetCurrentUTC()
        {
            return DateTime.UtcNow.ToString(UTC_STRING_FORMAT);
        }

        /// <summary>
        /// Returns the number of milliseconds ellapsed from the <code>date</code> parameter to Epoch/POSIX time (1970-1-1)
        /// </summary>
        protected static long GetMillisecondsFromEpoch(DateTime date)
        {
            return (long)(date.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        /// <summary>
        /// Encodes a string to be passed as an URL parameter in UTF-8
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected static string UrlEncode(string value)
        {
            return HttpUtility.UrlEncode(value, Encoding.UTF8);
        }
    }
}
