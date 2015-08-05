using BaseSDK;
using System.Collections.Generic;
using System.Text;

namespace LatchSDK
{
    public class LatchUser : ApiAuth
    {
        protected const string API_VERSION = "1.0";
        public const string API_HOST = "https://latch.elevenpaths.com";

        private const string API_APPLICATION_URL = "/api/" + API_VERSION + "/application";
        private const string API_SUBSCRIPTION_URL = "/api/" + API_VERSION + "/subscription";

        public LatchUser(string appId, string secretKey)
            : base(appId, secretKey)
        {
        }

        protected override string GetApiHost()
        {
            return API_HOST;
        }

        public ApiResponse GetSubscription()
        {
            return HTTP_GET_proxy(new StringBuilder(API_SUBSCRIPTION_URL).ToString(), null);
        }

        public ApiResponse CreateApplication(string name, string twoFactor, string lockOnRequest, string contactPhone, string contactEmail)
        {
            IDictionary<string, string> data = new Dictionary<string, string>();
            data.Add("name", name);
            data.Add("two_factor", twoFactor);
            data.Add("lock_on_request", lockOnRequest);
            data.Add("contactEmail", contactEmail);
            data.Add("contactPhone", contactPhone);
            return HTTP_PUT_proxy(new StringBuilder(API_APPLICATION_URL).ToString(), data);
        }

        public ApiResponse RemoveApplication(string applicationId)
        {
            return HTTP_DELETE_proxy(new StringBuilder(API_APPLICATION_URL).Append("/").Append(applicationId).ToString());
        }

        public ApiResponse GetApplications()
        {
            return HTTP_GET_proxy(new StringBuilder(API_APPLICATION_URL).ToString(), null);
        }

        public ApiResponse UpdateApplication(string applicationId, string name, string twoFactor, string lockOnRequest, string contactPhone, string contactEmail)
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