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

namespace LatchSDK
{
    public sealed class Latch : LatchBase
    {
        public Latch(string appId, string secretKey) : base(appId, secretKey)
        {
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
        /// Pairs an account using its name
        /// </summary>
        /// <param name="id">Name of the account</param>
        /// <param name="commonName">Name attached to this pairing. Showed in admin panels</param>
        /// <returns>If everything goes well, a <code>LatchResponse</code> object containing the account ID</returns>
        /// <remarks>Only works in test backend</remarks>
        public LatchResponse PairWithId(string id, string commonName)
        {
            var data = new Dictionary<string, string>()
            {
                { "commonName", commonName }
            };
            return HttpPerformRequest(API_PAIR_WITH_ID_URL + "/" + UrlEncode(id), HttpMethod.POST, data);
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
        /// Pairs an account using a token
        /// </summary>
        /// <param name="token">Pairing token obtained by the user from the mobile application</param>
        /// <param name="commonName">Name attached to this pairing. Showed in admin panels</param>
        /// <returns>If everything goes well, a <code>LatchResponse</code> object containing the AccountID</returns>
        public LatchResponse Pair(string token, string commonName)
        {
            var data = new Dictionary<string, string>()
            {
                { "commonName", commonName }
            };
            return HttpPerformRequest(API_PAIR_URL + "/" + UrlEncode(token), HttpMethod.POST, data);
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
        /// Create a new TOTP for a given user
        /// </summary>
        /// <param name="userId">Identifier associated with the user (provider-dependent)</param>
        /// <param name="commonName">Name associated with the user (provider-dependent)</param>
        /// <returns>If everything goes well, a <code>LatchResponse</code> object containing the TOTP ID and all of its properties</returns>
        public LatchResponse CreateTOTP(string userId, string commonName)
        {
            var data = new Dictionary<string, string>()
            {
                { "userId", userId },
                { "commonName", commonName }
            };
            return HttpPerformRequest(API_TOTP_URL, HttpMethod.POST, data);
        }

        /// <summary>
        /// Delete a specific TOTP using its identifier totpId
        /// </summary>
        /// <param name="totpId">Identifier of the TOTP to be deleted</param>
        /// <returns>If everything goes well, an empty response</returns>
        public LatchResponse DeleteTOTP(string totpId)
        {
            return HttpPerformRequest(API_TOTP_URL + "/" + UrlEncode(totpId));
        }

        /// <summary>
        /// Returns the information of a specific TOTP based on its totpId
        /// </summary>
        /// <param name="totpId">Identifier of the TOTP to be retrieved</param>
        /// <returns>If everything goes well, a <code>LatchResponse</code> object containing the TOTP properties</returns>
        public LatchResponse GetTOTP(string totpId)
        {
            return HttpPerformRequest(API_TOTP_URL + "/" + UrlEncode(totpId));
        }

        /// <summary>
        /// Validates a specific TOTP code provided by the user. The totpId identifies the TOTP, and code is the generated code that needs to be validated
        /// </summary>
        /// <param name="totpId">Identifier of the TOTP that will be used to validate the algorithm against the provided code</param>
        /// <param name="code">Code provided by the end-user, to be validated using the algorithm specified by the totpId</param>
        /// <returns></returns>
        public LatchResponse ValidateTOTP(string totpId, string code)
        {
            var data = new Dictionary<string, string>()
            {
                { "code", code }
            };
            return HttpPerformRequest(API_TOTP_URL + "/" + UrlEncode(totpId) + "/validate", HttpMethod.POST, data);
        }

        public LatchResponse CreateQSecret(int length, string encoding)
        {
            var data = new Dictionary<string, string>()
            {
                { "length", length.ToString() },
                { "encoding", encoding },
            };
            return HttpPerformRequest(API_Q_SECRET_URL, HttpMethod.POST, data);
        }
    }
}
