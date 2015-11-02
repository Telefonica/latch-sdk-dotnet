/*
    Manages the error codes that Latch may return.
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

namespace LatchSDK
{
    public class Error
    {
        private int code;
        private string message;

        public int Code { get { return code; } }
        public string Message { get { return message; } }

        public Error(int code, string message)
        {
            this.code = code;
            this.message = message;
        }

        public override string ToString()
        {
            return "E" + this.code.ToString() + " - " + message;
        }
    }
}