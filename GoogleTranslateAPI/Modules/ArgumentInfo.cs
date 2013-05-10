//-----------------------------------------------------------------------
// <copyright file="ArgumentInfo.cs" company="iron9light">
// Copyright (c) 2010 iron9light
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// </copyright>
// <author>iron9light@gmail.com</author>
//-----------------------------------------------------------------------

namespace Google.API
{
    using System;
    using System.Reflection;

    internal sealed class ArgumentInfo
    {
        private readonly RuntimeMethodHandle getMethod;

        private readonly string name;

        private readonly bool optional;

        private readonly object defaultValue;

        public ArgumentInfo(RuntimeMethodHandle getMethod, string name, bool optional, object defaultValue)
        {
            this.getMethod = getMethod;
            this.name = name;
            this.optional = optional;
            this.defaultValue = defaultValue;
        }

        public string GetString(RequestBase request, bool encodeNeeded)
        {
            var value = MethodBase.GetMethodFromHandle(this.getMethod).Invoke(request, null);

            if (value == null)
            {
                if (this.optional)
                {
                    return null;
                }
                else
                {
                    value = this.defaultValue;
                }
            }

            var valueString = GetValueString(value);

            if (string.IsNullOrEmpty(valueString) && this.optional)
            {
                return null;
            }

            if (encodeNeeded)
            {
                valueString = HttpUtility.UrlEncode(valueString);
            }

            return this.name + "=" + valueString;
        }

        private static string GetValueString(object value)
        {
            if (value == null)
            {
                return null;
            }

            if (value.Equals(true))
            {
                return "1";
            }
            else if (value.Equals(false))
            {
                return null;
            }
            else
            {
                return value.ToString();
            }
        }
    }
}