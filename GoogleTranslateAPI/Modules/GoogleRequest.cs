//-----------------------------------------------------------------------
// <copyright file="GoogleRequest.cs" company="iron9light">
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
    using System.Net;

    /// <summary>
    /// The google request.
    /// </summary>
    internal abstract class GoogleRequest : RequestBase
    {
        /// <summary>
        /// This argument supplies the query term passed to the method.
        /// </summary>
        [Argument("q", Optional = false)]
        public string Query { get; set; }

        /// <summary>
        /// This argument supplies protocol version number. The only valid value at this point in time is 1.0.
        /// </summary>
        [Argument("v", Optional = false)]
        public string Version
        {
            get
            {
                return "1.0";
            }
        }

        /// <summary>
        /// This argument supplies the IP address of the end-user on whose behalf the request is being made. Requests that include it are less likely to be mistaken for abuse. In choosing to utilize this parameter, please be sure that you're in compliance with any local laws, including any laws relating to disclosure of personal information being sent.
        /// </summary>
        [Argument("userip")]
        public IPAddress UserIP { get; set; }

        /// <summary>
        /// This optional argument supplies the host language of the application making the request. If this argument is not present, the system will choose a value based on the value of the Accept-Language  http header. If this header is not present, a value of en is assumed.
        /// </summary>
        [Argument("hl")]
        public string HostLanguage { get; set; }

        /// <summary>
        /// This optional argument supplies the application's key. If specified, it must be a valid key associated with your site which is validated against the passed referer header. The advantage of supplying a key is so that we can identify and contact you should something go wrong with your application. Without a key, we will still take the same appropriate measures on our side, but we will not be able to contact you. It is definitely best for you to pass a key.
        /// </summary>
        [Argument("key")]
        public string APIKey { get; set; }
    }
}