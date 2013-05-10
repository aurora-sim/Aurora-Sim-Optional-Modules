//-----------------------------------------------------------------------
// <copyright file="RequestUtility.cs" company="iron9light">
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
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading;

    using Newtonsoft.Json;

    internal static class RequestUtility
    {
        private static readonly Encoding Encoding = Encoding.UTF8;

////        internal static T GetResponseData2<T>(IRequestInfo requestInfo)
////        {
////            if (requestInfo == null)
////            {
////                throw new ArgumentNullException("requestInfo");
////            }

////            var webRequest = (HttpWebRequest)WebRequest.Create(requestInfo.Url);

////#if !SILVERLIGHT
////            webRequest.Referer = requestInfo.Referrer;
////#endif

////            if (!string.IsNullOrEmpty(requestInfo.PostContent))
////            {
////#if PocketPC || SILVERLIGHT
////                webRequest.Method = "POST";
////#else
////                webRequest.Method = WebRequestMethods.Http.Post;
////#endif

////                var postBytes = Encoding.GetBytes(requestInfo.PostContent);

////#if SILVERLIGHT
////                webRequest.Headers[HttpRequestHeader.ContentLength] = postBytes.Length.ToString();
////#else
////                webRequest.ContentLength = postBytes.Length;
////#endif

////                var requestStream = GetRequestStream(webRequest, requestInfo.OpenTimeout);

////                WriteRequestStream(requestStream, postBytes, requestInfo.SendTimeout);
////            }

////            string resultString = GetResultString(webRequest, requestInfo);

////            return Deserialize<T>(resultString);
////        }

////        private static Stream GetRequestStream(WebRequest webRequest, TimeSpan timeout)
////        {
////            return Invoke<Stream>(webRequest.BeginGetRequestStream, webRequest.EndGetRequestStream, timeout);
////        }

////        private static void WriteRequestStream(Stream stream, byte[] postBytes, TimeSpan timeout)
////        {
////            using (stream)
////            {
////                stream.WriteTimeout = (int)timeout.TotalMilliseconds;
////                stream.Write(postBytes, 0, postBytes.Length);
////            }
////        }

////        private static string GetResultString(WebRequest webRequest, IRequestInfo requestInfo)
////        {
////            try
////            {
////                // HACK: Not sure it should be OpenTimeout, CloseTimeout or ReceiveTimeout.
////                using (var webResponse = GetResponse(webRequest, requestInfo.OpenTimeout))
////                {
////                    using (var reader = new StreamReader(webResponse.GetResponseStream(), Encoding))
////                    {
////                        reader.BaseStream.ReadTimeout = (int)requestInfo.ReceiveTimeout.TotalMilliseconds;
////                        return reader.ReadToEnd();
////                    }
////                }
////            }
////            catch (WebException ex)
////            {
////                throw new GoogleAPIException("Failed to get response.", ex);
////            }
////            catch (IOException ex)
////            {
////                throw new GoogleAPIException("Cannot read the response stream.", ex);
////            }
////        }

////        private static WebResponse GetResponse(WebRequest webRequest, TimeSpan timeout)
////        {
////            return Invoke<WebResponse>(webRequest.BeginGetResponse, webRequest.EndGetResponse, timeout);
////        }

////        private static T Invoke<T>(Func<AsyncCallback, object, IAsyncResult> beginInvoke, Func<IAsyncResult, T> endInvoke, TimeSpan timeout)
////        {
////            Thread threadToKill = Thread.CurrentThread;

////            var asyncResult = beginInvoke(null, null);
////#if PocketPC
////            if (!asyncResult.AsyncWaitHandle.WaitOne((int)timeout.TotalMilliseconds, false))
////#else
////            if (!asyncResult.AsyncWaitHandle.WaitOne(timeout))
////#endif
////            {
////                threadToKill.Abort();
////                throw new TimeoutException();
////            }

////            return endInvoke(asyncResult);
////        }

        public static IAsyncResult BeginGetResponseData(IRequestInfo requestInfo, AsyncCallback callback, object state)
        {
            if (requestInfo == null)
            {
                throw new ArgumentNullException("requestInfo");
            }

            var webRequest = (HttpWebRequest)WebRequest.Create(requestInfo.Url);

#if !SILVERLIGHT
            webRequest.Referer = requestInfo.Referrer;
#endif

            var getResponseAsyncResult = new GetResponseAsyncResult(webRequest, state);

            var innerAsyncResult = webRequest.BeginGetResponse(
                asyncResult =>
                    {
                        getResponseAsyncResult.InnerAsyncResult = asyncResult;

                        if (callback != null)
                        {
                            callback(getResponseAsyncResult);
                        }
                    },
                null);

            getResponseAsyncResult.InnerAsyncResult = innerAsyncResult;

            return getResponseAsyncResult;
        }

        public static T EndGetResponseData<T>(IAsyncResult asyncResult)
        {
            var resultString = ((GetResponseAsyncResult)asyncResult).Value;

            return Deserialize<T>(resultString);
        }

#if !SILVERLIGHT
        public static T GetResponseData<T>(IRequestInfo requestInfo)
        {
            if (requestInfo == null)
            {
                throw new ArgumentNullException("requestInfo");
            }

            var webRequest = (HttpWebRequest)WebRequest.Create(requestInfo.Url);

#if !SILVERLIGHT
            webRequest.Referer = requestInfo.Referrer;
#endif
            string resultString;
            using (var webResponse = webRequest.GetResponse())
            {
                using (var reader = new StreamReader(webResponse.GetResponseStream(), Encoding))
                {
                    resultString = reader.ReadToEnd();
                }
            }

            return Deserialize<T>(resultString);
        }
#endif

        private static T Deserialize<T>(string text)
        {
            ResultObject<T> resultObject;
            try
            {
                resultObject = JsonConvert.DeserializeObject<ResultObject<T>>(text);
            }
            catch (Exception ex)
            {
                throw new DeserializeException(typeof(ResultObject<T>), text, ex);
            }

            if (resultObject.ResponseStatus != ResponseStatusConstant.DefaultStatus)
            {
                throw new GoogleServiceException(resultObject.ResponseStatus, resultObject.ResponseDetails);
            }

            return resultObject.ResponseData;
        }

        private class GetResponseAsyncResult : IAsyncResult
        {
            private readonly WebRequest webRequest;

            public GetResponseAsyncResult(WebRequest webRequest, object state)
            {
                this.webRequest = webRequest;
                this.AsyncState = state;
            }

            public string Value
            {
                get
                {
                    using (var webResponse = this.webRequest.EndGetResponse(this.InnerAsyncResult))
                    {
                        using (var reader = new StreamReader(webResponse.GetResponseStream(), Encoding))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }

            public IAsyncResult InnerAsyncResult { private get; set; }

            #region IAsyncResult Members

            public object AsyncState { get; private set; }

            public WaitHandle AsyncWaitHandle
            {
                get 
                {
                    return this.InnerAsyncResult.AsyncWaitHandle;
                }
            }

            public bool CompletedSynchronously
            {
                get
                {
                    return this.InnerAsyncResult.CompletedSynchronously;
                }
            }

            public bool IsCompleted
            {
                get
                {
                    return this.InnerAsyncResult.IsCompleted;
                }
            }

            #endregion
        }
    }
}
