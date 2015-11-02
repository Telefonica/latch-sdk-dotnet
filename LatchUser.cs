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

using System.Collections.Generic;
using System.Text;

namespace LatchSDK
{
    public class LatchUser : LatchAuth
    {
        public LatchUser(string appId, string secretKey)
            : base(appId, secretKey)
        {
        }

        protected override string GetApiHost()
        {
            return API_HOST;
        }

        public LatchResponse GetSubscription()
        {
            return HTTP_GET_proxy(new StringBuilder(API_SUBSCRIPTION_URL).ToString(), null);
        }

        public LatchResponse CreateApplication(string name, string twoFactor, string lockOnRequest, string contactPhone, string contactEmail)
        {
            IDictionary<string, string> data = new Dictionary<string, string>();
            data.Add("name", name);
            data.Add("two_factor", twoFactor);
            data.Add("lock_on_request", lockOnRequest);
            data.Add("contactEmail", contactEmail);
            data.Add("contactPhone", contactPhone);
            return HTTP_PUT_proxy(new StringBuilder(API_APPLICATION_URL).ToString(), data);
        }

        public LatchResponse RemoveApplication(string applicationId)
        {
            return HTTP_DELETE_proxy(new StringBuilder(API_APPLICATION_URL).Append("/").Append(applicationId).ToString());
        }

        public LatchResponse GetApplications()
        {
            return HTTP_GET_proxy(new StringBuilder(API_APPLICATION_URL).ToString(), null);
        }

        public LatchResponse UpdateApplication(string applicationId, string name, string twoFactor, string lockOnRequest, string contactPhone, string contactEmail)
        {
            IDictionary<string, string> data = new Dictionary<string, string>();
            data.Add("name", name);
            data.Add("two_factor", twoFactor);
            data.Add("lock_on_request", lockOnRequest);
            data.Add("contactPhone", contactPhone);
            data.Add("contactEmail", contactEmail);
            return HTTP_POST_proxy(new StringBuilder(API_APPLICATION_URL).Append("/").Append(applicationId).ToString(), data);
        }
    }
}