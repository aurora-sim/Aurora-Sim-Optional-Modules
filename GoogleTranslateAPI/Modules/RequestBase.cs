//-----------------------------------------------------------------------
// <copyright file="RequestBase.cs" company="iron9light">
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
    using System.Collections.Generic;
    using System.Linq;

    internal abstract class RequestBase : IRequestInfo
    {
        #region Fields

        private ICollection<ArgumentInfo> urlArgInfos;

        private ICollection<ArgumentInfo> postArgInfos;

        private string urlString;

        private string postContent;

        #endregion

        #region Properties

#if !SILVERLIGHT
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

        /// <summary>
        /// Gets the url string.
        /// </summary>
        public string Url
        {
            get
            {
                if (this.urlString == null)
                {
                    this.urlString = this.GetUrlString();
                }

                return this.urlString;
            }
        }

        /// <summary>
        /// Gets the post content.
        /// </summary>
        public string PostContent
        {
            get
            {
                if (this.postContent == null)
                {
                    this.postContent = this.GetPostContent();
                }

                return this.postContent;
            }
        }

        protected abstract string BaseAddress { get; }

        private ICollection<ArgumentInfo> UrlArgInfos
        {
            get
            {
                if (this.urlArgInfos == null)
                {
                    this.urlArgInfos = typeof(ArgumentInfoProvider<>).MakeGenericType(this.GetType()).GetProperty("UrlArgInfos").GetValue(null, null) as ICollection<ArgumentInfo>;
                }

                return this.urlArgInfos;
            }
        }

        private ICollection<ArgumentInfo> PostArgInfos
        {
            get
            {
                if (this.postArgInfos == null)
                {
                    this.postArgInfos = typeof(ArgumentInfoProvider<>).MakeGenericType(this.GetType()).GetProperty("PostArgInfos").GetValue(null, null) as ICollection<ArgumentInfo>;
                }

                return this.postArgInfos;
            }
        }

        #endregion

        #region Methods

        public void Reset()
        {
            this.urlString = null;
            this.postContent = null;
        }

        public override string ToString()
        {
            return this.Url;
        }

        private string GetUrlString()
        {
            var argString = this.GetArgString(this.UrlArgInfos, true);

            if (string.IsNullOrEmpty(argString))
            {
                return this.BaseAddress;
            }

            return this.BaseAddress + "?" + argString;
        }

        private string GetPostContent()
        {
            return this.GetArgString(this.PostArgInfos, false);
        }

        private string GetArgString(IEnumerable<ArgumentInfo> argInfos, bool encodeNeeded)
        {
            var valueStrings = from s in
                                   (from argInfo in argInfos
                                    select argInfo.GetString(this, encodeNeeded))
                               where !string.IsNullOrEmpty(s)
                               select s;
            var argString = string.Join("&", valueStrings.ToArray());
            return argString;
        }

        #endregion
    }
}
