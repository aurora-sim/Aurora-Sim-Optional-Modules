//-----------------------------------------------------------------------
// <copyright file="DetectData.cs" company="iron9light">
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

namespace Google.API.Translate
{
    using Newtonsoft.Json;

    [JsonObject]
    internal class DetectData
    {
        /// <summary>
        /// Gets the detected language.
        /// </summary>
        [JsonProperty("language")]
        public string LanguageCode { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the reliability of the detect.
        /// </summary>
        [JsonProperty("isReliable")]
        public bool IsReliable { get; internal set; }

        /// <summary>
        /// Gets the confidence level of the detect.
        /// </summary>
        [JsonProperty("confidence")]
        public double Confidence { get; internal set; }
    }
}