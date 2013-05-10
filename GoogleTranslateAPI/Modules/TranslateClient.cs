//-----------------------------------------------------------------------
// <copyright file="TranslateClient.cs" company="iron9light">
// Copyright (c) 2009 iron9light
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

namespace Google.API.Translate
{
    using System;

    /// <summary>
    /// The client for translate and detect.
    /// </summary>
    /// <remarks>
    /// You can use public static fields of <see cref="Language"/> and <see cref="TranslateFormat"/> as your parameters.
    /// </remarks>
    /// <seealso cref="Language"/>
    /// <seealso cref="TranslateFormat"/>
    public class TranslateClient : GoogleClient
    {
#if !SILVERLIGHT
        /// <summary>
        /// Initializes a new instance of the <see cref="TranslateClient"/> class.
        /// </summary>
        /// <param name="referrer">The http referrer header.</param>
        /// <remarks>Applications MUST always include a valid and accurate http referer header in their requests.</remarks>
        public TranslateClient(string referrer)
            : base(referrer)
        {
        }

        /// <summary>
        /// Translate the text from <paramref name="from"/> to <paramref name="to"/>.
        /// </summary>
        /// <param name="text">The content to translate.</param>
        /// <param name="from">The language of the original text. You can set it as <c>Language.Unknown</c> to the auto detect it.</param>
        /// <param name="to">The target language you want to translate to.</param>
        /// <returns>The translate result.</returns>
        /// <exception cref="GoogleAPIException">Translate failed.</exception>
        /// <example>
        /// This is the c# code example.
        /// <code>
        /// string text = "Œ“œ≤ª∂≈‹≤Ω°£";
        /// TranslateClient client = new TranslateClient(/* Enter the URL of your site here */);
        /// string translated = client.Translate(text, Language.ChineseSimplified, Language.English);
        /// Console.WriteLine(translated);
        /// // I like running.
        /// </code>
        /// </example>
        public string Translate(string text, string from, string to)
        {
            return this.Translate(text, from, to, TranslateFormat.GetDefault());
        }

        /// <summary>
        /// Translate the text from <paramref name="from"/> to <paramref name="to"/>.
        /// </summary>
        /// <param name="text">The content to translate.</param>
        /// <param name="from">The language of the original text. You can set it as <c>Language.Unknown</c> to the auto detect it.</param>
        /// <param name="to">The target language you want to translate to.</param>
        /// <param name="format">The format of the text.</param>
        /// <returns>The translate result.</returns>
        /// <exception cref="GoogleAPIException">Translate failed.</exception>
        /// <example>
        /// This is the c# code example.
        /// <code>
        /// string text = GetYourHtmlString();
        /// TranslateClient client = new TranslateClient(/* Enter the URL of your site here */);
        /// string translated = client.Translate(text, Language.English, Language.French, TranslateFormat.Html);
        /// </code>
        /// </example>
        public string Translate(string text, string from, string to, string format)
        {
            var result = this.NativeTranslate(text, from, to, format);

            if (TranslateFormat.Text.Equals(format))
            {
                return HttpUtility.HtmlDecode(result.TranslatedText);
            }

            return result.TranslatedText;
        }

        /// <summary>
        /// Translate the text to <paramref name="to"/> and auto detect which language the text is from.
        /// </summary>
        /// <param name="text">The content to translate.</param>
        /// <param name="to">The target language you want to translate to.</param>
        /// <param name="from">The detected language of the original text.</param>
        /// <returns>The translate result.</returns>
        /// <exception cref="GoogleAPIException">Translate failed.</exception>
        /// <example>
        /// This is the c# code example.
        /// <code>
        /// string text = "Je t'aime.";
        /// string from;
        /// TranslateClient client = new TranslateClient(/* Enter the URL of your site here */);
        /// string translated = client.TranslateAndDetect(text, Language.English, out from);
        /// Language fromLanguage = from;
        /// Console.WriteLine("\"{0}\" is \"{1}\" in {2}", text, translated, fromLanguage);
        /// // "Je t'aime." is "I love you." in French.
        /// </code>
        /// </example>
        public string TranslateAndDetect(string text, string to, out string from)
        {
            return this.TranslateAndDetect(text, to, TranslateFormat.GetDefault(), out from);
        }

        /// <summary>
        /// Translate the text to <paramref name="to"/> and auto detect which language the text is from.
        /// </summary>
        /// <param name="text">The content to translate.</param>
        /// <param name="to">The target language you want to translate to.</param>
        /// <param name="format">The format of the text.</param>
        /// <param name="from">The detected language of the original text.</param>
        /// <returns>The translate result.</returns>
        /// <exception cref="GoogleAPIException">Translate failed.</exception>
        public string TranslateAndDetect(string text, string to, string format, out string from)
        {
            var result = this.NativeTranslate(
                text, Language.Unknown, to, format);

            from = result.DetectedSourceLanguage;

            if (TranslateFormat.Text.Equals(format))
            {
                return HttpUtility.HtmlDecode(result.TranslatedText);
            }

            return result.TranslatedText;
        }

        /// <summary>
        /// Detect the language for this text.
        /// </summary>
        /// <param name="text">The text you want to test.</param>
        /// <param name="isReliable">Whether the result is reliable</param>
        /// <param name="confidence">The confidence percent of the result.</param>
        /// <returns>The detected language.</returns>
        /// <exception cref="GoogleAPIException">Detect failed.</exception>
        public string Detect(string text, out bool isReliable, out double confidence)
        {
            var result = this.NativeDetect(text);

            var language = result.LanguageCode;
            isReliable = result.IsReliable;
            confidence = result.Confidence;
            return language;
        }

        internal TranslateData NativeTranslate(string text, string from, string to, string format)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            if (to == null)
            {
                throw new ArgumentNullException("to");
            }

            var request = new TranslateRequest { Query = text, From = from, To = to, Format = format };

            var responseData = this.GetResponseData<TranslateData>(request);

            return responseData;
        }

        internal DetectData NativeDetect(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            var request = new DetectRequest { Query = text };

            var responseData = this.GetResponseData<DetectData>(request);

            return responseData;
        }
#endif

        /// <summary>
        /// Begins an asynchronous request for translating the text from <paramref name="from"/> to <paramref name="to"/>.
        /// </summary>
        /// <param name="text">The content to translate.</param>
        /// <param name="from">The language of the original text. You can set it as <c>Language.Unknown</c> to the auto detect it.</param>
        /// <param name="to">The target language you want to translate to.</param>
        /// <param name="callback">The <see cref="AsyncCallback"/> delegate.</param>
        /// <param name="state">An object containing state information for this asynchronous request.</param>
        /// <returns>An <see cref="IAsyncResult"/> that references the asynchronous request.</returns>
        public IAsyncResult BeginTranslate(string text, string from, string to, AsyncCallback callback, object state)
        {
            return this.BeginTranslate(text, from, to, TranslateFormat.GetDefault(), callback, state);
        }

        /// <summary>
        /// Begins an asynchronous request for translating the text from <paramref name="from"/> to <paramref name="to"/>.
        /// </summary>
        /// <param name="text">The content to translate.</param>
        /// <param name="from">The language of the original text. You can set it as <c>Language.Unknown</c> to the auto detect it.</param>
        /// <param name="to">The target language you want to translate to.</param>
        /// <param name="format">The format of the text.</param>
        /// <param name="callback">The <see cref="AsyncCallback"/> delegate.</param>
        /// <param name="state">An object containing state information for this asynchronous request.</param>
        /// <returns>An <see cref="IAsyncResult"/> that references the asynchronous request.</returns>
        public IAsyncResult BeginTranslate(string text, string from, string to, string format, AsyncCallback callback, object state)
        {
            var translateAsyncResult = new TranslateAsyncResult(format);
            var innerAsyncResult = this.BeginNativeTranslate(
                text,
                from,
                to,
                format,
                asyncResult =>
                    {
                        translateAsyncResult.InnerAsyncResult = asyncResult;
                        if (callback != null)
                        {
                            callback(translateAsyncResult);
                        }
                    },
                state);

            translateAsyncResult.InnerAsyncResult = innerAsyncResult;
            return translateAsyncResult;
        }

        /// <summary>
        /// returns a translate result.
        /// </summary>
        /// <param name="asyncResult">An <see cref="IAsyncResult"/> that references a pending request for a response.</param>
        /// <returns>The translate result.</returns>
        public string EndTranslate(IAsyncResult asyncResult)
        {
            var translateAsyncResult = (TranslateAsyncResult)asyncResult;

            var result = this.EndNativeTranslate(translateAsyncResult.InnerAsyncResult);

            var format = translateAsyncResult.Format;

            if (TranslateFormat.Text.Equals(format))
            {
                return HttpUtility.HtmlDecode(result.TranslatedText);
            }

            return result.TranslatedText;
        }

        /// <summary>
        /// Begins an asynchronous request for translating the text to <paramref name="to"/> and auto detect which language the text is from.
        /// </summary>
        /// <param name="text">The content to translate.</param>
        /// <param name="to">The target language you want to translate to.</param>
        /// <param name="callback">The <see cref="AsyncCallback"/> delegate.</param>
        /// <param name="state">An object containing state information for this asynchronous request.</param>
        /// <returns>An <see cref="IAsyncResult"/> that references the asynchronous request.</returns>
        public IAsyncResult BeginTranslateAndDetect(string text, string to, AsyncCallback callback, object state)
        {
            return this.BeginTranslateAndDetect(text, to, TranslateFormat.GetDefault(), callback, state);
        }

        /// <summary>
        /// Begins an asynchronous request for translating the text to <paramref name="to"/> and auto detect which language the text is from.
        /// </summary>
        /// <param name="text">The content to translate.</param>
        /// <param name="to">The target language you want to translate to.</param>
        /// <param name="format">The format of the text.</param>
        /// <param name="callback">The <see cref="AsyncCallback"/> delegate.</param>
        /// <param name="state">An object containing state information for this asynchronous request.</param>
        /// <returns>An <see cref="IAsyncResult"/> that references the asynchronous request.</returns>
        public IAsyncResult BeginTranslateAndDetect(string text, string to, string format, AsyncCallback callback, object state)
        {
            var translateAsyncResult = new TranslateAsyncResult(format);
            var innerAsyncResult = this.BeginNativeTranslate(
                text,
                Language.Unknown,
                to,
                format,
                asyncResult =>
                {
                    translateAsyncResult.InnerAsyncResult = asyncResult;
                    if (callback != null)
                    {
                        callback(translateAsyncResult);
                    }
                },
                state);

            translateAsyncResult.InnerAsyncResult = innerAsyncResult;
            return translateAsyncResult;
        }

        /// <summary>
        /// returns a translate result.
        /// </summary>
        /// <param name="asyncResult">An <see cref="IAsyncResult"/> that references a pending request for a response.</param>
        /// <param name="from">The detected language of the original text.</param>
        /// <returns>The translate result.</returns>
        public string EndTranslateAndDetect(IAsyncResult asyncResult, out string from)
        {
            var translateAsyncResult = (TranslateAsyncResult)asyncResult;

            var result = this.EndNativeTranslate(translateAsyncResult.InnerAsyncResult);

            from = result.DetectedSourceLanguage;

            var format = translateAsyncResult.Format;

            if (TranslateFormat.Text.Equals(format))
            {
                return HttpUtility.HtmlDecode(result.TranslatedText);
            }

            return result.TranslatedText;
        }

        /// <summary>
        /// Begins an asynchronous request for detect the language for this text.
        /// </summary>
        /// <param name="text">The text you want to test.</param>
        /// <param name="callback">The <see cref="AsyncCallback"/> delegate.</param>
        /// <param name="state">An object containing state information for this asynchronous request.</param>
        /// <returns>An <see cref="IAsyncResult"/> that references the asynchronous request.</returns>
        public IAsyncResult BeginDetect(string text, AsyncCallback callback, object state)
        {
            return this.BeginNativeDetect(text, callback, state);
        }

        /// <summary>
        /// returns the detected language.
        /// </summary>
        /// <param name="asyncResult">An <see cref="IAsyncResult"/> that references a pending request for a response.</param>
        /// <param name="isReliable">Whether the result is reliable</param>
        /// <param name="confidence">The confidence percent of the result.</param>
        /// <returns>The detected language.</returns>
        public string EndDetect(IAsyncResult asyncResult, out bool isReliable, out double confidence)
        {
            var result = this.EndNativeDetect(asyncResult);

            var language = result.LanguageCode;
            isReliable = result.IsReliable;
            confidence = result.Confidence;
            return language;
        }

        internal IAsyncResult BeginNativeTranslate(string text, string from, string to, string format, AsyncCallback callback, object state)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            if (to == null)
            {
                throw new ArgumentNullException("to");
            }

            var request = new TranslateRequest { Query = text, From = from, To = to, Format = format };

            return this.BeginGetResponseData(request, callback, state);
        }

        internal TranslateData EndNativeTranslate(IAsyncResult asyncResult)
        {
            return this.EndGetResponseData<TranslateData>(asyncResult);
        }

        internal IAsyncResult BeginNativeDetect(string text, AsyncCallback callback, object state)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            var request = new DetectRequest { Query = text };

            return this.BeginGetResponseData(request, callback, state);
        }

        internal DetectData EndNativeDetect(IAsyncResult asyncResult)
        {
            return this.EndGetResponseData<DetectData>(asyncResult);
        }

        private class TranslateAsyncResult : IAsyncResult
        {
            private readonly string format;

            public TranslateAsyncResult(string format)
            {
                this.format = format;
            }

            public IAsyncResult InnerAsyncResult { get; set; }

            public string Format
            {
                get
                {
                    return this.format;
                }
            }

            #region IAsyncResult Members

            public object AsyncState
            {
                get
                {
                    return this.InnerAsyncResult.AsyncState;
                }
            }

            public System.Threading.WaitHandle AsyncWaitHandle
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