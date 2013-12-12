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

namespace LatchSDK
{
    public class Latch
    {
        private const string API_VERSION = "0.6";
        public static string API_HOST = "https://latch.elevenpaths.com";
        public const string API_CHECK_STATUS_URL = "/api/" + API_VERSION + "/status";
        public const string API_PAIR_URL = "/api/" + API_VERSION + "/pair";
        public const string API_PAIR_WITH_ID_URL = "/api/" + API_VERSION + "/pairWithId";
        public const string API_UNPAIR_URL = "/api/" + API_VERSION + "/unpair";

        public const string AUTHORIZATION_HEADER_NAME = "Authorization";
        public const string DATE_HEADER_NAME = "X-11Paths-Date";
        public const string AUTHORIZATION_METHOD = "11PATHS";
        private const char AUTHORIZATION_HEADER_FIELD_SEPARATOR = ' ';

        public const string UTC_STRING_FORMAT = "yyyy-MM-dd HH:mm:ss";

        public const string X_11PATHS_HEADER_PREFIX = "X-11paths-";
        private const char X_11PATHS_HEADER_SEPARATOR = ':';

        public static void SetHost(string host)
        {
            if (string.IsNullOrEmpty(host))
                return;

            API_HOST = host.TrimEnd('/');
        }

        /// <summary>
        /// The custom header consists of three parts, the method, the appId and the signature. 
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

        /// <param name="authorizationHeader">Authorization HTTP Header</param>
        /// <returns>The Authorization method. Typical values are "Basic", "Digest" or "11PATHS"</returns>
        public static string GetAuthMethodFromHeader(string authorizationHeader)
        {
            return GetPartFromHeader(0, authorizationHeader);
        }

        /// <param name="authorizationHeader">Authorization HTTP Header</param>
        /// <returns>The requesting application Id. Identifies the application using the API</returns>
        public static string GetAppIdFromHeader(string authorizationHeader)
        {
            return GetPartFromHeader(1, authorizationHeader);
        }

        /// <param name="authorizationHeader">Authorization HTTP Header</param>
        /// <returns>The signature of the current request. Verifies the identity of the application using the API</returns>
        public static string GetSignatureFromHeader(string authorizationHeader)
        {
            return GetPartFromHeader(2, authorizationHeader);
        }

        private String appId;
        private String secretKey;

        /// <summary>
        /// Create an instance of the class with the Application ID and secret obtained from Eleven Paths 
        /// </summary>
        public Latch(string appId, string secretKey)
        {
            this.appId = appId;
            this.secretKey = secretKey;
        }


        public string HttpGet(string url, IDictionary<string, string> headers)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

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
                StreamReader sr = new StreamReader(request.GetResponse().GetResponseStream());
                return sr.ReadToEnd();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private LatchResponse HttpGetProxy(string url)
        {
            return new LatchResponse(HttpGet(API_HOST + url, AuthenticationHeaders("GET", url, null)));
        }

        public LatchResponse PairWithId(string id)
        {
            return HttpGetProxy(new StringBuilder(API_PAIR_WITH_ID_URL).Append("/").Append(id).ToString());
        }

        public LatchResponse Pair(string token)
        {
            return HttpGetProxy(new StringBuilder(API_PAIR_URL).Append("/").Append(token).ToString());
        }

        public LatchResponse Status(string accountId)
        {
            return HttpGetProxy(new StringBuilder(API_CHECK_STATUS_URL).Append("/").Append(accountId).ToString());
        }

        public LatchResponse OperationStatus(string accountId, string operationId)
        {
            return HttpGetProxy(new StringBuilder(API_CHECK_STATUS_URL).Append("/").Append(accountId).Append("/op/").Append(operationId).ToString());
        }

        public LatchResponse Unpair(string id)
        {
            return HttpGetProxy(new StringBuilder(API_UNPAIR_URL).Append("/").Append(id).ToString());
        }


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
        /// Calculate the authentication headers to be sent with a request to the API 
        /// </summary>
        /// <param name="httpMethod">The HTTP Method. Currently only GET is supported</param>
        /// <param name="queryString">The urlencoded string including the path (from the first forward slash) and the parameters</param>
        /// <param name="xHeaders">HTTP headers specific to the 11-paths API. Null if not needed.</param>
        /// <returns>An IDictionary with the Authorization and Date headers needed to sign a Latch API request</returns>
        public IDictionary<string, string> AuthenticationHeaders(string httpMethod, string queryString, IDictionary<string, string> xHeaders)
        {
            return AuthenticationHeaders(httpMethod, queryString, xHeaders, GetCurrentUTC());
        }

        /// <summary>
        /// Calculate the authentication headers to be sent with a request to the API 
        /// </summary>
        /// <param name="httpMethod">The HTTP Method. Currently only GET is supported</param>
        /// <param name="queryString">The urlencoded string including the path (from the first forward slash) and the parameters</param>
        /// <param name="xHeaders">HTTP headers specific to the 11-paths API. Null if not needed.</param>
        /// <param name="utc">The Universal Coordinated Time for the Date HTTP header</param>
        /// <returns>An IDictionary with the Authorization and Date headers needed to sign a Latch API request</returns>
        public IDictionary<string, string> AuthenticationHeaders(string httpMethod, string queryString, IDictionary<string, string> xHeaders, string utc)
        {
            StringBuilder stringToSign = new StringBuilder()
                .Append(httpMethod.ToUpper().Trim()).Append("\n")
                .Append(utc).Append("\n")
                .Append(GetSerializedHeaders(xHeaders)).Append("\n")
                .Append(queryString.Trim());

            String signedData = SignData(stringToSign.ToString());

            String authorizationHeader = new StringBuilder(AUTHORIZATION_METHOD)
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
        /// <param name="xHeaders">A non necessarily ordered IDictionary of the HTTP headers to be ordered without duplicates</param>
        /// <returns>A String with the serialized headers, an empty string if no headers are passed, or null if there's a problem
        ///  such as non specific 11paths headers</returns>
        private string GetSerializedHeaders(IDictionary<string, string> xHeaders)
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
                    sorted.Add(key.ToLower(), xHeaders[key].Replace('\n', ' '));
                }

                StringBuilder serializedHeaders = new StringBuilder();
                foreach (string key in sorted.Keys)
                {
                    serializedHeaders.Append(key).Append(X_11PATHS_HEADER_SEPARATOR).Append(sorted[key]).Append(" ");
                }

                return serializedHeaders.ToString().Trim();
            }
            else
            {
                return "";
            }
        }

        /// <returns>A string representation of the current time in UTC to be used in a Date HTTP Header</returns>
        private static string GetCurrentUTC()
        {
            return DateTime.UtcNow.ToString(UTC_STRING_FORMAT);
        }

    }
}
