/*
    Latch C# SDK - Set of  reusable classes to  allow developers integrate Latch on their applications.
    Copyright (C) 2013 Eleven Paths.
 
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

namespace LatchSDK
{
    public class Latch
    {
        private const string API_VERSION = "0.9";
        private static string apiHost = "https://latch.elevenpaths.com";
        public static string API_HOST { get { return apiHost; } }

        public const string API_CHECK_STATUS_URL = "/api/" + API_VERSION + "/status";
        public const string API_PAIR_URL = "/api/" + API_VERSION + "/pair";
        public const string API_PAIR_WITH_ID_URL = "/api/" + API_VERSION + "/pairWithId";
        public const string API_UNPAIR_URL = "/api/" + API_VERSION + "/unpair";
        public const string API_LOCK_URL = "/api/" + API_VERSION + "/lock";
        public const string API_UNLOCK_URL = "/api/" + API_VERSION + "/unlock";
        public const string API_HISTORY_URL = "/api/" + API_VERSION + "/history";
        public const string API_OPERATION_URL = "/api/" + API_VERSION + "/operation";

        public const string AUTHORIZATION_HEADER_NAME = "Authorization";
        public const string DATE_HEADER_NAME = "X-11Paths-Date";
        public const string AUTHORIZATION_METHOD = "11PATHS";
        private const char AUTHORIZATION_HEADER_FIELD_SEPARATOR = ' ';

        public const string UTC_STRING_FORMAT = "yyyy-MM-dd HH:mm:ss";

        public const string X_11PATHS_HEADER_PREFIX = "X-11paths-";
        private const char X_11PATHS_HEADER_SEPARATOR = ':';
        private const char PARAM_SEPARATOR = '&';
        private const char PARAM_VALUE_SEPARATOR = '=';

        private string appId;
        private string secretKey;

        protected enum HttpMethod { GET, POST, PUT, DELETE }
        public enum FeatureMode { MANDATORY, OPT_IN, DISABLED }

        /// <summary>
        /// Creates an instance of the class with the <code>Application ID</code> and <code>Secret</code> obtained from Eleven Paths 
        /// </summary>
        public Latch(string appId, string secretKey)
        {
            this.appId = appId;
            this.secretKey = secretKey;
        }

        /// <summary>
        /// Sets the host of the Latch API backend
        /// </summary>
        /// <param name="host">The host of the Latch API backend in standard URI format (e.g: "https://latch.elevenpaths.com")</param>
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
        protected static string HttpPerformRequest(string url, HttpMethod method, IDictionary<string, string> headers, IDictionary<string, string> data)
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
        private LatchResponse HttpPerformRequest(string url, HttpMethod method = HttpMethod.GET, IDictionary<string, string> data = null)
        {
            var authHeaders = AuthenticationHeaders(method.ToString(), url, null, data);
            return new LatchResponse(HttpPerformRequest(apiHost + url, method, authHeaders, data));
        }



        /// <summary>
        /// Pairs an account using its name
        /// </summary>
        /// <param name="id">Name of the account</param>
        /// <returns>If everything goes well, a <code>LatchResponse</code> object containing the account ID</returns>
        /// <remarks>Only works in test backend</remarks>
        public LatchResponse PairWithId(string id)
        {
            return HttpPerformRequest(API_PAIR_WITH_ID_URL + "/" + UrlEncode(id));
        }

        /// <summary>
        /// Pairs an account using a token
        /// </summary>
        /// <param name="token">Pairing token obtained by the user from the mobile application</param>
        /// <returns>If everything goes well, a <code>LatchResponse</code> object containing the AccountID</returns>
        public LatchResponse Pair(string token)
        {
            return HttpPerformRequest(API_PAIR_URL + "/" + UrlEncode(token));
        }

        /// <summary>
        /// Requests the status of the specified account ID
        /// </summary>
        /// <param name="accountId">The account ID of the user</param>
        /// <returns>If everything goes well, a <code>LatchResponse</code> object containing the status of the account</returns>
        public LatchResponse Status(string accountId)
        {
            return HttpPerformRequest(API_CHECK_STATUS_URL + "/" + UrlEncode(accountId));
        }

        /// <summary>
        /// Requests the status of the specified account ID and operation ID
        /// </summary>
        /// <param name="accountId">The account ID of the user</param>
        /// <param name="operationId">The operation ID to be checked</param>
        /// <returns>If everything goes well, a <code>LatchResponse</code> object containing the status of the operation</returns>
        public LatchResponse OperationStatus(string accountId, string operationId)
        {
            return HttpPerformRequest(API_CHECK_STATUS_URL + "/" + UrlEncode(accountId) + "/op/" + UrlEncode(operationId));
        }

        /// <summary>
        /// Unpairs the specified account ID
        /// </summary>
        /// <param name="accountId">The account ID of the user</param>
        /// <returns>If everything goes well, an empty response</returns>
        public LatchResponse Unpair(string accountId)
        {
            return HttpPerformRequest(API_UNPAIR_URL + "/" + UrlEncode(accountId));
        }

        /// <summary>
        /// Locks the specified account ID
        /// </summary>
        /// <param name="accountId">The account ID of the user to be locked</param>
        /// <returns>If everything goes well, an empty response</returns>
        /// <remarks>Only for premium accounts</remarks>
        public LatchResponse Lock(string accountId)
        {
            return HttpPerformRequest(API_LOCK_URL + "/" + UrlEncode(accountId), HttpMethod.POST);
        }

        /// <summary>
        /// Locks the specified operation ID of the specified account ID
        /// </summary>
        /// <param name="accountId">The account ID of the user</param>
        /// <param name="operationId">The operation ID to be locked</param>
        /// <returns>If everything goes well, an empty response</returns>
        /// <remarks>Only for premium accounts</remarks>
        public LatchResponse Lock(string accountId, string operationId)
        {
            return HttpPerformRequest(API_LOCK_URL + "/" + UrlEncode(accountId) + "/op/" + UrlEncode(operationId), HttpMethod.POST);
        }

        /// <summary>
        /// Unlocks the specified account ID
        /// </summary>
        /// <param name="accountId">The account ID of the user to be unlocked</param>
        /// <returns>If everything goes well, an empty response</returns>
        /// <remarks>Only for premium accounts</remarks>
        public LatchResponse Unlock(string accountId)
        {
            return HttpPerformRequest(API_UNLOCK_URL + "/" + UrlEncode(accountId), HttpMethod.POST);
        }

        /// <summary>
        /// Unlocks the specified operation ID of the specified account ID
        /// </summary>
        /// <param name="accountId">The account ID of the user</param>
        /// <param name="operationId">The operation ID to be unlocked</param>
        /// <returns>If everything goes well, an empty response</returns>
        /// <remarks>Only for premium accounts</remarks>
        public LatchResponse Unlock(string accountId, string operationId)
        {
            return HttpPerformRequest(API_UNLOCK_URL + "/" + UrlEncode(accountId) + "/op/" + UrlEncode(operationId), HttpMethod.POST);
        }

        /// <summary>
        /// Requests the history of the specified account ID
        /// </summary>
        /// <param name="accountId">The account ID of the user</param>
        /// <returns>If everything goes well, a <code>LatchResponse</code> object containing the requested information</returns>
        /// <remarks>Only for premium accounts</remarks>
        public LatchResponse History(string accountId)
        {
            return HttpPerformRequest(API_HISTORY_URL + "/" + UrlEncode(accountId));
        }
        
        /// <summary>
        /// Requests the history of the specified account ID between two instants
        /// </summary>
        /// <param name="accountId">The account ID of the user</param>
        /// <param name="from">Starting date and time</param>
        /// <param name="to">Ending date and time</param>
        /// <returns>If everything goes well, a <code>LatchResponse</code> object containing the requested information</returns>
        /// <remarks>Only for premium accounts</remarks>
        public LatchResponse History(string accountId, DateTime? from, DateTime? to)
        {
            long? fromMillisFromEpoch = from.HasValue ? GetMillisecondsFromEpoch(from.Value) : (long?)null;
            long? toMillisFromEpoch = to.HasValue ? GetMillisecondsFromEpoch(to.Value) : (long?)null;
            return History(accountId, fromMillisFromEpoch, toMillisFromEpoch);
        }

        /// <summary>
        /// Requests the history of the specified account ID between two instants
        /// </summary>
        /// <param name="accountId">The account ID of the user</param>
        /// <param name="fromMillisFromEpoch">Starting date and time in milliseconds ellapsed from Epoch time</param>
        /// <param name="toMillisFromEpoch">Ending date and time in milliseconds ellapsed from Epoch time</param>
        /// <returns>If everything goes well, a <code>LatchResponse</code> object containing the requested information</returns>
        /// <remarks>Only for premium accounts</remarks>
        public LatchResponse History(string accountId, long? fromMillisFromEpoch, long? toMillisFromEpoch)
        {
            return HttpPerformRequest(API_HISTORY_URL + "/" + UrlEncode(accountId) + "/" +
                fromMillisFromEpoch.GetValueOrDefault(0).ToString() + "/" +
                toMillisFromEpoch.GetValueOrDefault(GetMillisecondsFromEpoch(DateTime.Now)).ToString());
        }

        /// <summary>
        /// Gets all operations of the application
        /// </summary>
        /// <returns>If everything goes well, a <code>LatchResponse</code> object containing all operations</returns>
        public LatchResponse GetOperations()
        {
            return HttpPerformRequest(API_OPERATION_URL);
        }

        /// <summary>
        /// Gets all suboperations under the specified parent operation
        /// </summary>
        /// <param name="parentOperationId">Parent operation ID</param>
        /// <returns>If everything goes well, a <code>LatchResponse</code> object containing all suboperations</returns>
        public LatchResponse GetOperations(string parentOperationId)
        {
            return HttpPerformRequest(API_OPERATION_URL + "/" + UrlEncode(parentOperationId));
        }
        
        /// <summary>
        /// Creates a new operation with the specified parameters
        /// </summary>
        /// <param name="parentId">Parent operation ID</param>
        /// <param name="name">Name of the new operation</param>
        /// <param name="twoFactor">Two factor (OTP) mode (optional)</param>
        /// <param name="lockOnRequest">Lock on request (LOR) mode (optional)</param>
        /// <returns>If everything goes well, a <code>LatchResponse</code> object containing the new operation ID</returns>
        public LatchResponse CreateOperation(string parentId, string name, FeatureMode twoFactor = FeatureMode.DISABLED, FeatureMode lockOnRequest = FeatureMode.DISABLED)
        {
            var data = new Dictionary<string, string>();
            data.Add("parentId", parentId);
            data.Add("name", name);
            data.Add("two_factor", twoFactor.ToString());
            data.Add("lock_on_request", lockOnRequest.ToString());
            return HttpPerformRequest(API_OPERATION_URL, HttpMethod.PUT, data);
        }

        /// <summary>
        /// Removes an existing operation
        /// </summary>
        /// <param name="operationId">Operation ID to remove</param>
        /// <returns>If everything goes well, an empty response</returns>
        public LatchResponse RemoveOperation(string operationId)
        {
            return HttpPerformRequest(API_OPERATION_URL + "/" + UrlEncode(operationId), HttpMethod.DELETE);
        }

        /// <summary>
        /// Updates an existing operation with one or more of the specified parameters
        /// </summary>
        /// <param name="operationId">Operation ID to change</param>
        /// <param name="name">New name of the operation (optional)</param>
        /// <param name="twoFactor">New two factor (OTP) mode (optional)</param>
        /// <param name="lockOnRequest">New lock on request (LOR) mode (optional)</param>
        /// <returns>If everything goes well, an empty response</returns>
        public LatchResponse UpdateOperation(string operationId, string name, FeatureMode? twoFactor = null, FeatureMode? lockOnRequest = null)
        {
            var data = new Dictionary<string, string>();
            if (!String.IsNullOrEmpty(name))
                data.Add("name", name);
            if (twoFactor.HasValue)
                data.Add("two_factor", twoFactor.ToString());
            if (lockOnRequest.HasValue)
                data.Add("lock_on_request", lockOnRequest.ToString());
            return HttpPerformRequest(API_OPERATION_URL + "/" + UrlEncode(operationId), HttpMethod.POST, data);
        }




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
        public IDictionary<string, string> AuthenticationHeaders(string httpMethod, string queryString, IDictionary<string, string> xHeaders, IDictionary<string, string> parameters)
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
        public IDictionary<string, string> AuthenticationHeaders(string httpMethod, string queryString, IDictionary<string, string> xHeaders, IDictionary<string, string> parameters, string utc)
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
        private static long GetMillisecondsFromEpoch(DateTime date)
        {
            return (long)(date.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        /// <summary>
        /// Encodes a string to be passed as an URL parameter in UTF-8
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string UrlEncode(string value)
        {
            return HttpUtility.UrlEncode(value, Encoding.UTF8);
        }

    }
}
