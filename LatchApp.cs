/*
    Returns the Latch responses according to the configuration set.
    Copyright (C) 2013 Eleven Paths

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
using System.Web;

namespace LatchSDK
{
    public class LatchApp : LatchAuth
    {
        public enum FeatureMode { MANDATORY, OPT_IN, DISABLED }

        public LatchApp(string appId, string secretKey)
            : base(appId, secretKey)
        {
        }

        /// <summary>
        /// Sets the host of the Latch API backend
        /// </summary>
        /// <param name="host">The host of the Latch API backend in standard URI format (e.g: "https://latch.elevenpaths.com")</param>
        public static void SetHost(string host)
        {
            if (String.IsNullOrEmpty(host))
            {
                return;
            }

            API_HOST = host.TrimEnd('/');
        }

        /// <summary>
        /// Pairs an account using its name
        /// </summary>
        /// <param name="id">Name of the account</param>
        /// <returns>If everything goes well, a <code>LatchResponse</code> object containing the account ID</returns>
        /// <remarks>Only works in test backend</remarks>
        public LatchResponse PairWithId(string id)
        {
            return HTTP_GET_proxy(String.Concat(API_PAIR_WITH_ID_URL, "/", UrlEncode(id)), null);
        }

        /// <summary>
        /// Pairs an account using a token
        /// </summary>
        /// <param name="token">Pairing token obtained by the user from the mobile application</param>
        /// <returns>If everything goes well, a <code>LatchResponse</code> object containing the AccountID</returns>
        public LatchResponse Pair(string token)
        {
            return HTTP_GET_proxy(String.Concat(API_PAIR_URL, "/", UrlEncode(token)), null);
        }

        /// <summary>
        /// Requests the status of the specified account ID
        /// </summary>
        /// <param name="accountId">The account ID of the user</param>
        /// <returns>If everything goes well, a <code>LatchResponse</code> object containing the status of the account</returns>
        public LatchResponse Status(string accountId)
        {
            return HTTP_GET_proxy(API_CHECK_STATUS_URL + "/" + UrlEncode(accountId), null);
        }

        /// <summary>
        /// Return application status for a given accountId
        /// </summary>
        /// <param name="accountId">The accountId which status is going to be retrieved</param>
        /// <param name="silent">True for not sending lock/unlock push notifications to the mobile devices, false otherwise.</param>
        /// <param name="noOtp">True for not generating a OTP if needed.</param>
        /// <returns>LatchResponse containing the status</returns>
        public LatchResponse Status(string accountId, bool silent, bool noOtp)
        {
            return Status(accountId, null, silent, noOtp);
        }

        /// <summary>
        /// Return operation status for a given accountId and operationId
        /// </summary>
        /// <param name="accountId">The accountId which status is going to be retrieved</param>
        /// <param name="operationId">The operationId which status is going to be retrieved</param>
        /// <returns>LatchResponse containing the status</returns>
        public LatchResponse Status(string accountId, string operationId)
        {
            return Status(accountId, operationId, false, false);
        }

        /// <summary>
        /// Return operation status for a given accountId and operationId
        /// </summary>
        /// <param name="accountId">The accountId which status is going to be retrieved</param>
        /// <param name="operationId">The operationId which status is going to be retrieved</param>
        /// <param name="silent">True for not sending lock/unlock push notifications to the mobile devices, false otherwise.</param>
        /// <param name="noOtp">True for not generating a OTP if needed.</param>
        /// <returns>LatchResponse containing the status</returns>
        public LatchResponse Status(string accountId, string operationId, bool silent, bool noOtp)
        {
            string url = String.Concat(API_CHECK_STATUS_URL, "/", accountId);
            if (!String.IsNullOrEmpty(operationId))
            {
                url = String.Concat(url, "/op/", operationId);
            }

            if (noOtp)
            {
                url = String.Concat(url, "/nootp");
            }
            if (silent)
            {
                url = String.Concat(url, "/silent");
            }

            return HTTP_GET_proxy(url.ToString(), null);
        }

        /// <summary>
        /// Return application status for a given accountId while sending some custom data (Like OTP token or a message)
        /// </summary>
        /// <param name="accountId">The accountId which status is going to be retrieved</param>
        /// <param name="otpToken">This will be the OTP sent to the user instead of generating a new one</param>
        /// <param name="otpMessage">To attach a custom message with the OTP to the user</param>
        /// <returns>LatchResponse containing the status</returns>
        public LatchResponse Status(string accountId, string otpToken, string otpMessage)
        {
            return Status(accountId, null, false, otpToken, otpMessage);
        }

        /// <summary>
        /// Return application status for a given accountId while sending some custom data (Like OTP token or a message)
        /// </summary>
        /// <param name="accountId">The accountId which status is going to be retrieved</param>
        /// <param name="silent">True for not sending lock/unlock push notifications to the mobile devices, false otherwise.</param>
        /// <param name="otpToken">This will be the OTP sent to the user instead of generating a new one</param>
        /// <param name="otpMessage">To attach a custom message with the OTP to the user</param>
        /// <returns>LatchResponse containing the status</returns>
        public LatchResponse Status(string accountId, bool silent, string otpToken, string otpMessage)
        {
            return Status(accountId, null, silent, otpToken, otpMessage);
        }

        /// <summary>
        /// Return operation status for a given accountId and operation while sending some custom data (Like OTP token or a message)
        /// </summary>
        /// <param name="accountId">The accountId which status is going to be retrieved</param>
        /// <param name="operationId">The operationId which status is going to be retrieved</param>
        /// <param name="silent">True for not sending lock/unlock push notifications to the mobile devices, false otherwise.</param>
        /// <param name="otpToken">This will be the OTP sent to the user instead of generating a new one</param>
        /// <param name="otpMessage">To attach a custom message with the OTP to the user</param>
        /// <returns>LatchResponse containing the status</returns>
        public LatchResponse Status(string accountId, string operationId, bool silent, string otpToken, string otpMessage)
        {
            string url = String.Concat(API_CHECK_STATUS_URL, "/", accountId);

            if (!String.IsNullOrEmpty(operationId))
            {
                url = String.Concat(url, "/op/", operationId);
            }

            if (silent)
            {
                url = String.Concat(url, "/silent");
            }

            IDictionary<string, string> data = new Dictionary<string, string>();
            if (!String.IsNullOrEmpty(otpToken))
            {
                data.Add("otp", otpToken);
            }
            if (!String.IsNullOrEmpty(otpMessage))
            {
                data.Add("msg", otpMessage);
            }
            return HTTP_POST_proxy(url.ToString(), data);
        }

        /// <summary>
        /// Requests the status of the specified account ID and operation ID
        /// </summary>
        /// <param name="accountId">The account ID of the user</param>
        /// <param name="operationId">The operation ID to be checked</param>
        /// <returns>If everything goes well, a <code>LatchResponse</code> object containing the status of the operation</returns>
        [Obsolete]
        public LatchResponse OperationStatus(string accountId, string operationId)
        {
            return Status(accountId, operationId, false, false);
        }

        [Obsolete]
        public LatchResponse operationStatus(string accountId, string operationId, bool silent, bool noOtp)
        {
            return Status(accountId, operationId, silent, noOtp);
        }

        /// <summary>
        /// Unpairs the specified account ID
        /// </summary>
        /// <param name="accountId">The account ID of the user</param>
        /// <returns>If everything goes well, an empty response</returns>
        public LatchResponse Unpair(string accountId)
        {
            return HTTP_GET_proxy(API_UNPAIR_URL + "/" + UrlEncode(accountId), null);
        }

        /// <summary>
        /// Locks the specified account ID
        /// </summary>
        /// <param name="accountId">The account ID of the user to be locked</param>
        /// <returns>If everything goes well, an empty response</returns>
        /// <remarks>Only for premium accounts</remarks>
        public LatchResponse Lock(string accountId)
        {
            IDictionary<string, string> data = null;
            return HTTP_POST_proxy(API_LOCK_URL + "/" + UrlEncode(accountId), data);
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
            IDictionary<string, string> data = null;
            return HTTP_POST_proxy(API_LOCK_URL + "/" + UrlEncode(accountId) + "/op/" + UrlEncode(operationId), data);
        }

        /// <summary>
        /// Unlocks the specified account ID
        /// </summary>
        /// <param name="accountId">The account ID of the user to be unlocked</param>
        /// <returns>If everything goes well, an empty response</returns>
        /// <remarks>Only for premium accounts</remarks>
        public LatchResponse Unlock(string accountId)
        {
            IDictionary<string, string> data = null;
            return HTTP_POST_proxy(API_UNLOCK_URL + "/" + UrlEncode(accountId), data);
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
            IDictionary<string, string> data = null;
            return HTTP_POST_proxy(API_UNLOCK_URL + "/" + UrlEncode(accountId) + "/op/" + UrlEncode(operationId), data);
        }

        /// <summary>
        /// Requests the history of the specified account ID
        /// </summary>
        /// <param name="accountId">The account ID of the user</param>
        /// <returns>If everything goes well, a <code>LatchResponse</code> object containing the requested information</returns>
        /// <remarks>Only for premium accounts</remarks>
        public LatchResponse History(string accountId)
        {
            return HTTP_GET_proxy(API_HISTORY_URL + "/" + UrlEncode(accountId), null);
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
            return HTTP_GET_proxy(API_HISTORY_URL + "/" + UrlEncode(accountId) + "/" +
                fromMillisFromEpoch.GetValueOrDefault(0).ToString() + "/" +
                toMillisFromEpoch.GetValueOrDefault(GetMillisecondsFromEpoch(DateTime.Now)).ToString(), null);
        }

        /// <summary>
        /// Gets all operations of the application
        /// </summary>
        /// <returns>If everything goes well, a <code>LatchResponse</code> object containing all operations</returns>
        public LatchResponse GetOperations()
        {
            return HTTP_GET_proxy(API_OPERATION_URL, null);
        }

        /// <summary>
        /// Gets all suboperations under the specified parent operation
        /// </summary>
        /// <param name="parentOperationId">Parent operation ID</param>
        /// <returns>If everything goes well, a <code>LatchResponse</code> object containing all suboperations</returns>
        public LatchResponse GetOperations(string parentOperationId)
        {
            return HTTP_GET_proxy(API_OPERATION_URL + "/" + UrlEncode(parentOperationId), null);
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
            return HTTP_PUT_proxy(API_OPERATION_URL, data);
        }

        /// <summary>
        /// Removes an existing operation
        /// </summary>
        /// <param name="operationId">Operation ID to remove</param>
        /// <returns>If everything goes well, an empty response</returns>
        public LatchResponse RemoveOperation(string operationId)
        {
            return HTTP_DELETE_proxy(API_OPERATION_URL + "/" + UrlEncode(operationId));
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
            return HTTP_POST_proxy(API_OPERATION_URL + "/" + UrlEncode(operationId), data);
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

        protected override string GetApiHost()
        {
            return API_HOST;
        }
    }
}