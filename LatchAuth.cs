using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace LatchSDK
{
    public abstract class LatchAuth
    {
        protected const string QUERYSTRING_DELMITER = "?";
        protected const string UTC_STRING_FORMAT = "yyyy-MM-dd HH:mm:ss";
        public const string BODY_HASH_HEADER_NAME = X_11PATHS_HEADER_PREFIX + "Body-Hash";
        public const string DATE_NAME = "Date";

        private ConfigurationProxy proxy;

        public ConfigurationProxy Proxy
        {
            get
            {
                return proxy;
            }
            private set
            {
                proxy = value;
            }
        }

        private enum HeaderPart { Signature = 0, AppId = 1, AuthMethod = 2 };

        protected string appId { get; private set; }
        protected string secretKey { get; private set; }

        /// <summary>
        /// Creates an instance of the class with the <code>Application ID</code> and <code>Secret</code> obtained from Eleven Paths
        /// </summary>
        protected LatchAuth(string appId, string secretKey)
        {
            this.appId = appId;
            this.secretKey = secretKey;
        }

        public void SetProxyConfiguration(string host, int port, string user, string password, string domain)
        {
            if (Proxy == null)
            {
                Proxy = new ConfigurationProxy();
            }

            try
            {
                Proxy.SetHost(host)
                .SetPort(port)
                .SetUser(user)
                .SetPassword(password)
                .SetDomain(domain);
            }
            catch (ArgumentException e)
            {
            }
        }

        protected const string API_VERSION = "1.0";
        public static string API_HOST = "https://latch.elevenpaths.com";

        //App API
        public const string API_CHECK_STATUS_URL = "/api/" + API_VERSION + "/status";

        public const string API_PAIR_URL = "/api/" + API_VERSION + "/pair";
        public const string API_PAIR_WITH_ID_URL = "/api/" + API_VERSION + "/pairWithId";
        public const string API_UNPAIR_URL = "/api/" + API_VERSION + "/unpair";
        public const string API_LOCK_URL = "/api/" + API_VERSION + "/lock";
        public const string API_UNLOCK_URL = "/api/" + API_VERSION + "/unlock";
        public const string API_HISTORY_URL = "/api/" + API_VERSION + "/history";
        public const string API_OPERATION_URL = "/api/" + API_VERSION + "/operation";

        //User API
        public const string API_APPLICATION_URL = "/api/" + API_VERSION + "/application";

        public const string API_SUBSCRIPTION_URL = "/api/" + API_VERSION + "/subscription";

        public const string X_11PATHS_HEADER_PREFIX = "X-11paths-";
        private const string X_11PATHS_HEADER_SEPARATOR = ":";

        public const string AUTHORIZATION_HEADER_NAME = "Authorization";
        public const string DATE_HEADER_NAME = X_11PATHS_HEADER_PREFIX + "Date";
        public const string AUTHORIZATION_METHOD = "11PATHS";
        private const char AUTHORIZATION_HEADER_FIELD_SEPARATOR = ' ';

        public const string UTC_string_FORMAT = "yyyy-MM-dd HH:mm:ss";

        private const string HMAC_ALGORITHM = "HmacSHA1";

        private const string CHARSET_ASCII = "US-ASCII";
        private const string CHARSET_ISO_8859_1 = "ISO-8859-1";
        private const string CHARSET_UTF_8 = "UTF-8";
        private const string HTTP_METHOD_GET = "GET";
        private const string HTTP_METHOD_DELETE = "DELETE";
        private const string HTTP_HEADER_CONTENT_LENGTH = "Content-Length";
        private const string HTTP_HEADER_CONTENT_TYPE = "Content-Type";
        private const string HTTP_HEADER_CONTENT_TYPE_FORM_URLENCODED = "application/x-www-form-urlencoded";
        private const char PARAM_SEPARATOR = '&';
        private const string PARAM_VALUE_SEPARATOR = "=";

        public static void setHost(string host)
        {
            API_HOST = host;
        }

        /// <summary>
        /// The custom header consists of three parts: the method, the application ID and the signature.
        /// This method returns the specified part if it exists.
        /// </summary>
        /// <param name="part">The zero indexed part to be returned</param>
        /// <param name="header">The HTTP header value from which to extract the part</param>
        /// <returns>The specified part from the header or an empty string if not existent</returns>
        private static string GetPartFromHeader(HeaderPart headerPart, string header)
        {
            int part = (int)headerPart;
            if (part < 0)
            {
                return String.Empty;
            }
            if (!String.IsNullOrEmpty(header))
            {
                string[] parts = header.Split(AUTHORIZATION_HEADER_FIELD_SEPARATOR);
                if (parts.Length > part)
                {
                    return parts[part];
                }
            }
            return String.Empty;
        }

        /// <summary>
        /// Extracts the authorization method from the authorization header (the first parameter)
        /// </summary>
        /// <param name="authorizationHeader">Authorization HTTP Header</param>
        /// <returns>The Authorization method. Typical values are "Basic", "Digest" or "11PATHS"</returns>
        private static string GetAuthMethodFromHeader(string authorizationHeader)
        {
            return GetPartFromHeader(HeaderPart.Signature, authorizationHeader);
        }

        /// <summary>
        /// Extracts the application ID from the authorization header (the second parameter)
        /// </summary>
        /// <param name="authorizationHeader">Authorization HTTP Header</param>
        /// <returns>The requesting application Id. Identifies the application using the API</returns>
        private static string GetAppIdFromHeader(string authorizationHeader)
        {
            return GetPartFromHeader(HeaderPart.AppId, authorizationHeader);
        }

        /// <summary>
        /// Extracts the signature from the authorization header (the third parameter)
        /// </summary>
        /// <param name="authorizationHeader">Authorization HTTP Header</param>
        /// <returns>The signature of the current request. Verifies the identity of the application using the API</returns>
        private static string GetSignatureFromHeader(string authorizationHeader)
        {
            return GetPartFromHeader(HeaderPart.AuthMethod, authorizationHeader);
        }

        public LatchResponse HTTP_GET(string URL, IDictionary<string, string> headers)
        {
            return HTTP(new Uri(URL), HttpVerbs.Get, headers, null);
        }

        public LatchResponse HTTP_POST(string URL, IDictionary<string, string> headers, IDictionary<string, string> data)
        {
            return HTTP(new Uri(URL), HttpVerbs.Post, headers, data);
        }

        public LatchResponse HTTP_DELETE(string URL, IDictionary<string, string> headers)
        {
            return HTTP(new Uri(URL), HttpVerbs.Delete, headers, null);
        }

        public LatchResponse HTTP_PUT(string URL, IDictionary<string, string> headers, IDictionary<string, string> data)
        {
            return HTTP(new Uri(URL), HttpVerbs.Put, headers, data);
        }

        protected LatchResponse HTTP_POST_proxy(string url)
        {
            return HTTP_POST_proxy(url, new Dictionary<string, string>());
        }

        protected LatchResponse HTTP_GET_proxy(string URL, IDictionary<string, string> queryParams)
        {
            try
            {
                URL = String.Concat(URL, ParseQueryParams(queryParams));
                return HTTP_GET(String.Concat(GetApiHost(), URL), AuthenticationHeaders(HttpVerbs.Get, URL, null, null));
            }
            catch (Exception e)
            {
                return null;
            }
        }

        protected async Task<LatchResponse> ASYNC_HTTP_GET_proxy(string URL, IDictionary<string, string> queryParams)
        {
            return await Task.Factory.StartNew<LatchResponse>(() => HTTP_GET_proxy(URL, queryParams));
        }

        protected LatchResponse HTTP_DELETE_proxy(string URL)
        {
            try
            {
                return HTTP_DELETE(String.Concat(GetApiHost(), URL), AuthenticationHeaders(HttpVerbs.Delete, URL, null, null));
            }
            catch (Exception e)
            {
                return null;
            }
        }

        protected async Task<LatchResponse> ASYNC_HTTP_DELETE_proxy(string URL)
        {
            return await Task.Factory.StartNew<LatchResponse>(() => HTTP_DELETE_proxy(URL));
        }

        protected LatchResponse HTTP_POST_proxy(string URL, IDictionary<string, string> data)
        {
            try
            {
                return HTTP_POST(String.Concat(GetApiHost(), URL), AuthenticationHeaders(HttpVerbs.Post, URL, null, data), data);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        protected async Task<LatchResponse> ASYNC_HTTP_POST_proxy(string URL, IDictionary<string, string> data)
        {
            return await Task.Factory.StartNew<LatchResponse>(() => HTTP_POST_proxy(URL, data));
        }

        protected LatchResponse HTTP_PUT_proxy(string URL, IDictionary<string, string> data)
        {
            return HTTP_PUT(String.Concat(GetApiHost(), URL), AuthenticationHeaders(HttpVerbs.Put, URL, null, data), data);
        }

        protected async Task<LatchResponse> ASYNC_HTTP_PUT_proxy(string URL, IDictionary<string, string> data)
        {
            return await Task.Factory.StartNew<LatchResponse>(() => HTTP_PUT_proxy(URL, data));
        }

        /// <summary>
        /// Calculates the headers to be sent with a request to the API so the server can verify the signature
        /// </summary>
        /// <param name="httpMethod">The HTTP request method.</param>
        /// <param name="querystring">The urlencoded string including the path (from the first forward slash) and the parameters.</param>
        /// <param name="xHeaders">The HTTP request headers specific to the API, excluding X-11Paths-Date. null if not needed.</param>
        /// <param name="params">The HTTP request params. Must be only those to be sent in the body of the request and must be urldecoded. null if not needed.</param>
        /// <returns>A map with the {@value #AUTHORIZATION_HEADER_NAME} and {@value #DATE_HEADER_NAME} headers needed to be sent with a request to the API.</returns>
        private IDictionary<string, string> AuthenticationHeaders(HttpVerbs httpMethod, string querystring, IDictionary<string, string> xHeaders, IDictionary<string, string> param)
        {
            return AuthenticationHeaders(httpMethod, querystring, xHeaders, param, GetCurrentUTC());
        }

        /// <summary>
        /// Calculate the authentication headers to be sent with a request to the API
        /// </summary>
        /// <param name="httpMethod">The HTTP Method, currently only GET is supported</param>
        /// <param name="querystring">The urlencoded string including the path (from the first forward slash) and the parameters.</param>
        /// <param name="xHeaders">The HTTP request headers specific to the API, excluding X-11Paths-Date. null if not needed.</param>
        /// <param name="params">The HTTP request params. Must be only those to be sent in the body of the request and must be urldecoded. Null if not needed.</param>
        /// <param name="UTC">The Universal Coordinated Time for the X-11Paths-Date HTTP header</param>
        /// <returns>A map with the Authorization and X-11Paths-Date headers needed to sign a Latch API request</returns>
        private IDictionary<string, string> AuthenticationHeaders(HttpVerbs httpMethod, string queryString, IDictionary<string, string> xHeaders, IDictionary<string, string> param, string UTC)
        {
            if (String.IsNullOrEmpty(queryString) || String.IsNullOrEmpty(UTC))
            {
                return null;
            }
            return StringToSign(httpMethod, queryString, xHeaders, UTC, param);
        }

        private IDictionary<string, string> StringToSign(HttpVerbs httpMethod, string queryString, IDictionary<string, string> xHeaders, string UTC, IDictionary<string, string> param)
        {
            string stringToSign = String.Concat(httpMethod.ToString().ToUpper(), "\n", UTC, "\n", GetSerializedHeaders(xHeaders), "\n", queryString.Trim());
            {
                string serializedParams = GetSerializedParams(param);
                if (!String.IsNullOrEmpty(serializedParams))
                {
                    stringToSign = String.Concat(stringToSign, "\n", serializedParams);
                }
            }
            string signedData = String.Empty;
            try
            {
                signedData = SignData(stringToSign.ToString());
            }
            catch (Exception e)
            {
                return null;
            }

            string authorizationHeader = String.Concat(AUTHORIZATION_METHOD, AUTHORIZATION_HEADER_FIELD_SEPARATOR, this.appId, AUTHORIZATION_HEADER_FIELD_SEPARATOR, signedData);

            IDictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add(AUTHORIZATION_HEADER_NAME, authorizationHeader);
            headers.Add(DATE_HEADER_NAME, UTC);
            return headers;
        }

        /// <summary>
        /// Performs an HTTP request to an URL using the specified method and data, returning the response as a string
        /// </summary>
        protected virtual LatchResponse HTTP(Uri URL, HttpVerbs method, IDictionary<string, string> headers, IDictionary<string, string> data)
        {
            HttpWebRequest request = BuildHttpUrlConnection(URL, headers);
            if (request == null)
            {
                throw new HttpException("Request could not be created correctly");
            }
            request.Method = method.ToString();

            try
            {
                if (method.Equals(HttpVerbs.Post) || method.Equals(HttpVerbs.Put))
                {
                    request.ContentType = HTTP_HEADER_CONTENT_TYPE_FORM_URLENCODED;
                    using (StreamWriter sw = new StreamWriter(request.GetRequestStream()))
                    {
                        sw.Write(GetSerializedParams(data));
                        sw.Flush();
                    }
                }

                using (StreamReader sr = new StreamReader(request.GetResponse().GetResponseStream()))
                {
                    string json = sr.ReadToEnd();
                    return new LatchResponse(json);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private HttpWebRequest BuildHttpUrlConnection(Uri URL, IDictionary<string, string> headers)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);

            if (Proxy != null)
            {
                request = BuildHttpUrlConnectionProxy(request);
            }

            foreach (string key in headers.Keys)
            {
                if (key.Equals(AUTHORIZATION_HEADER_NAME, StringComparison.InvariantCultureIgnoreCase))
                {
                    request.Headers[AUTHORIZATION_HEADER_NAME] = headers[key];
                }
                else if (key.Equals(DATE_NAME, StringComparison.InvariantCultureIgnoreCase))
                {
                    try
                    {
                        request.Date = DateTime.Parse(headers[key], null, System.Globalization.DateTimeStyles.AssumeUniversal);
                    }
                    catch (Exception e)
                    {
                        return null;
                    }
                }
                else
                {
                    request.Headers.Add(key, headers[key]);
                }
            }
            return request;
        }

        private HttpWebRequest BuildHttpUrlConnectionProxy(HttpWebRequest request)
        {
            if (!String.IsNullOrEmpty(Proxy.Host))
            {
                request.Proxy = new WebProxy(Proxy.Host, Proxy.Port);
                if (!String.IsNullOrEmpty(Proxy.User) && !String.IsNullOrEmpty(Proxy.Password))
                {
                    if (!String.IsNullOrEmpty(Proxy.Domain))
                    {
                        request.Proxy.Credentials = new NetworkCredential(
                            Proxy.User,
                            Proxy.Password,
                            Proxy.Domain);
                    }
                    else
                    {
                        request.Proxy.Credentials = new NetworkCredential(
                            Proxy.User,
                            Proxy.Password);
                    }
                }
            }
            return request;
        }

        /// <summary>
        /// Signs the data provided in order to prevent tampering
        /// </summary>
        /// <param name="data">The string to sign</param>
        /// <returns>Base64 encoding of the HMAC-SHA1 hash of the data parameter using <code>secretKey</code> as cipher key.</returns>
        private string SignData(string data)
        {
            if (String.IsNullOrEmpty(data))
            {
                throw new ArgumentException("String to sign can not be null or empty.");
            }
            if (String.IsNullOrEmpty(secretKey))
            {
                throw new NullReferenceException("String used to sign can not be null or empty.");
            }
            using (HMACSHA1 hmacSha1 = new HMACSHA1(Encoding.ASCII.GetBytes(secretKey)))
            {
                return Convert.ToBase64String(hmacSha1.ComputeHash(Encoding.ASCII.GetBytes(data)));
            }
        }

        private static string ParseQueryParams(IDictionary<string, string> queryParams)
        {
            if (queryParams == null || queryParams.Count == 0)
            {
                return String.Empty;
            }

            string query = QUERYSTRING_DELMITER;
            foreach (string key in queryParams.Keys)
            {
                string value = queryParams[key];
                if (!String.IsNullOrEmpty(value))
                {
                    query += key + PARAM_VALUE_SEPARATOR + UrlEncode(value) + PARAM_SEPARATOR;
                }
            }
            return (query.EndsWith(PARAM_SEPARATOR.ToString())) ? query.Substring(0, query.Length - 1) : query;
        }

        /// <summary>
        /// Prepares and returns a string ready to be signed from the 11-paths specific HTTP headers received
        /// </summary>
        /// <param name="xHeaders">A non necessarily sorted IDictionary of the HTTP headers</param>
        /// <returns>A string with the serialized headers, an empty string if no headers are passed, or a ApplicationException if there's a problem
        ///  such as non specific 11paths headers</returns>
        private static string GetSerializedHeaders(IDictionary<string, string> xHeaders)
        {
            if (xHeaders != null)
            {
                SortedDictionary<string, string> sorted = new SortedDictionary<string, string>();

                foreach (string key in xHeaders.Keys)
                {
                    if (!key.StartsWith(X_11PATHS_HEADER_PREFIX, StringComparison.InvariantCultureIgnoreCase))
                    {
                        throw new ApplicationException("Error serializing headers. Only specific " + X_11PATHS_HEADER_PREFIX + " headers need to be signed");
                    }

                    sorted.Add(key.ToLowerInvariant(), xHeaders[key].Replace('\n', ' '));
                }

                string serializedHeaders = String.Empty;
                foreach (string key in sorted.Keys)
                {
                    serializedHeaders = String.Concat(serializedHeaders, key, X_11PATHS_HEADER_SEPARATOR, sorted[key], AUTHORIZATION_HEADER_FIELD_SEPARATOR);
                }

                return serializedHeaders.Trim(AUTHORIZATION_HEADER_FIELD_SEPARATOR);
            }
            else
            {
                return String.Empty;
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
            if (parameters != null)
            {
                SortedDictionary<string, string> sorted = new SortedDictionary<string, string>(parameters);

                string serializedParams = String.Empty;
                foreach (string key in sorted.Keys)
                {
                    serializedParams = String.Concat(serializedParams, UrlEncode(key), PARAM_VALUE_SEPARATOR);
                    serializedParams = String.Concat(serializedParams, UrlEncode(sorted[key]), PARAM_SEPARATOR);
                }

                return serializedParams.Trim(PARAM_SEPARATOR);
            }
            else
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Returns a string representation of the current time in UTC to be used in a Date HTTP Header
        /// </summary>
        protected virtual string GetCurrentUTC()
        {
            return DateTime.UtcNow.ToString(UTC_STRING_FORMAT);
        }

        private static long GetMillisecondsFromEpoch(DateTime date)
        {
            return (long)(date.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        /// <summary>
        /// Encodes a string to be passed as an URL parameter in UTF-8
        /// </summary>
        private static string UrlEncode(string value)
        {
            return HttpUtility.UrlEncode(value, Encoding.UTF8);
        }

        protected abstract string GetApiHost();
    }
}