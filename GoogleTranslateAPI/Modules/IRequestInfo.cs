//-----------------------------------------------------------------------
// <copyright file="IRequestInfo.cs" company="iron9light">
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

    internal interface IRequestInfo
    {
        /// <summary>
        /// Gets the url string.
        /// </summary>
        string Url { get; }

        /// <summary>
        /// Gets the post content.
        /// </summary>
        string PostContent { get; }

#if !SILVERLIGHT
        string Referrer { get; }
#endif

        /// <summary>
        /// Gets the interval of time after which the open method, invoked by a communication object, times out.
        /// </summary>
        /// <returns>The <see cref="System.TimeSpan"/> that specifies the interval of time to wait for the open method to time out.</returns>
        TimeSpan OpenTimeout { get; }

        /// <summary>
        /// Gets the interval of time after which the close method, invoked by a communication object, times out.
        /// </summary>
        /// <returns>The <see cref="System.TimeSpan"/> that specifies the interval of time to wait for the close method to time out.</returns>
        TimeSpan CloseTimeout { get; }

        /// <summary>
        /// Gets the interval of time after which the send method, invoked by a communication object, times out.
        /// </summary>
        /// <returns>The <see cref="System.TimeSpan"/> that specifies the interval of time to wait for the send method to time out.</returns>
        TimeSpan SendTimeout { get; }

        /// <summary>
        /// Gets the interval of time after which the receive method, invoked by a communication object, times out.
        /// </summary>
        /// <returns>The <see cref="System.TimeSpan"/> that specifies the interval of time to wait for the receive method to time out.</returns>
        TimeSpan ReceiveTimeout { get; }
    }
}