/*
   Returns the Latch responses according to the configuration set.
   Copyright (C) 2023 Telefonica Digital

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
#if NETSTANDARD
using System.Text.Json;
using System.Text.Json.Nodes;
#endif
#if NETFRAMEWORK
using System.Web.Script.Serialization;
#endif
namespace LatchSDK
{
    public class LatchResponse
    {
#if NETSTANDARD
        public JsonObject Data { get; private set; }
#endif

#if NETFRAMEWORK
        private static JavaScriptSerializer js = new JavaScriptSerializer();
        public Dictionary<string, object> Data { get; private set; }
#endif
        public Error Error { get; private set; }

        public LatchResponse(string json)
        {
#if NETSTANDARD
            var response = JsonSerializer.Deserialize<JsonObject>(json);
            if (response.TryGetPropertyValue("data", out var dataValue))
            {
                this.Data = dataValue.AsObject();
            }

            if (response.TryGetPropertyValue("error", out var err))
            {
                var error = err.AsObject();
                if (error.TryGetPropertyValue("code", out var codeNode))
                {
                    String message = error.TryGetPropertyValue("message", out var messNode) ? messNode.GetValue<string>() : String.Empty;
                    this.Error = new Error(codeNode.GetValue<int>(), message);
                }
            }
#endif
#if NETFRAMEWORK

            Dictionary<string, object> response = (Dictionary<string, object>)js.DeserializeObject(json);
            if (response.ContainsKey("data"))
            {
                this.Data = (Dictionary<string, object>)response["data"];
            }

            if (response.ContainsKey("error"))
            {
                Dictionary<string, object> err = (Dictionary<string, object>)response["error"];
                int code;
                if (err.ContainsKey("code") && int.TryParse(err["code"].ToString(), out code))
                {
                    String message = err.ContainsKey("message") ? err["message"].ToString() : string.Empty;
                    this.Error = new Error(code, message);
                }
            }
#endif
        }
    }
}