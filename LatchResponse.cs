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
using System.Web.Script.Serialization;

namespace LatchSDK
{
    public class LatchResponse
    {
        public Dictionary<string, object> Data { get; private set; }
        public Error Error { get; private set; }

        private static JavaScriptSerializer js = new JavaScriptSerializer();

        public LatchResponse(String json)
        {
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
        }
    }
}