//-----------------------------------------------------------------------
// <copyright file="GoogleClient.cs" company="iron9light">
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
    using System.Net;

    /// <summary>
    /// The abstract base class for all google service client classes.
    /// </summary>
    public abstract class GoogleClient
    {
#if SILVERLIGHT
        protected GoogleClient()
        {
#else
        protected GoogleClient(string referrer)
        {
            this.Referrer = referrer;
#endif

            this.OpenTimeout = new TimeSpan(0, 1, 0);
            this.CloseTimeout = new TimeSpan(0, 1, 0);
            this.SendTimeout = new TimeSpan(0, 1, 0);
            this.ReceiveTimeout = new TimeSpan(0, 10, 0);
        }

        /// <summary>
        /// This argument supplies the IP address of the end-user on whose behalf the request is being made.
        /// Requests that include it are less likely to be mistaken for abuse.
        /// In choosing to utilize this parameter, please be sure that you're in compliance with any local laws, including any laws relating to disclosure of personal information being sent.
        /// </summary>
        public IPAddress UserIP { get; set; }

        /// <summary>
        /// This optional argument supplies the host language of the application making the request.
        /// If this argument is not present then the system will choose a value based on the value of the <b>Accept-Language</b> http header. If this header is not present, a value of <b>en</b> is assumed.
        /// </summary>
        /// <value>The accept language.</value>
        public string AcceptLanguage { get; set; }

        /// <summary>
        /// This optional argument supplies the application's key.
        /// If specified, it must be a valid key associated with your site which is validated against the passed referer header. The advantage of supplying a key is so that we can identify and contact you should something go wrong with your application. Without a key, we will still take the same appropriate measures on our side, but we will not be able to contact you. It is definitely best for you to pass a key.
        /// </summary>
        /// <value>The API key.</value>
        public string ApiKey { get; set; }

#if !SILVERLIGHT
        /// <summary>
        /// Gets or sets the http referrer header.
        /// </summary>
        /// <value>The referrer.</value>
        /// <remarks>Applications MUST always include a valid and accurate http referer header in their requests.</remarks>
        public string Referrer { get; set; }
#endif

        /// <summary>
        /// Gets or sets the interval of time after which the open method, invoked by a communication object, times out.
        /// </summary>
        /// <returns>The <see cref="System.TimeSpan"/> that specifies the interval of time to wait for the open method to time out.</returns>
        public TimeSpan OpenTimeout { get; set; }

        /// <summary>
        /// Gets or sets the interval of time after which the close method, invoked by a communication object, times out.
        /// </summary>
        /// <returns>The <see cref="System.TimeSpan"/> that specifies the interval of time to wait for the close method to time out.</returns>
        public TimeSpan CloseTimeout { get; set; }

        /// <summary>
        /// Gets or sets the interval of time after which the send method, invoked by a communication object, times out.
        /// </summary>
        /// <returns>The <see cref="System.TimeSpan"/> that specifies the interval of time to wait for the send method to time out.</returns>
        public TimeSpan SendTimeout { get; set; }

        /// <summary>
        /// Gets or sets the interval of time after which the receive method, invoked by a communication object, times out.
        /// </summary>
        /// <returns>The <see cref="System.TimeSpan"/> that specifies the interval of time to wait for the receive method to time out.</returns>
        public TimeSpan ReceiveTimeout { get; set; }

        internal IAsyncResult BeginGetResponseData(GoogleRequest request, AsyncCallback callback, object state)
        {
            this.SetValueTo(request);

            return RequestUtility.BeginGetResponseData(request, callback, state);
        }

        internal T EndGetResponseData<T>(IAsyncResult asyncResult)
        {
            return RequestUtility.EndGetResponseData<T>(asyncResult);
        }

#if !SILVERLIGHT
        internal T GetResponseData<T>(GoogleRequest request)
        {
            this.SetValueTo(request);

            return RequestUtility.GetResponseData<T>(request);
        }
#endif

        internal void SetValueTo(GoogleRequest request)
        {
#if !SILVERLIGHT
            request.Referrer = this.Referrer;
#endif

            request.OpenTimeout = this.OpenTimeout;
            request.CloseTimeout = this.CloseTimeout;
            request.SendTimeout = this.SendTimeout;
            request.ReceiveTimeout = this.ReceiveTimeout;

            request.UserIP = this.UserIP;
            request.HostLanguage = this.AcceptLanguage;
            request.APIKey = this.ApiKey;
        }
    }
}