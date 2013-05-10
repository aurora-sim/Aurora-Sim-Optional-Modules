//-----------------------------------------------------------------------
// <copyright file="ArgumentInfoProvider.cs" company="iron9light">
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
    using System.Collections.Generic;
    using System.Reflection;

    internal static class ArgumentInfoProvider<T> where T : RequestBase
    {
        static ArgumentInfoProvider()
        {
            UrlArgInfos = new List<ArgumentInfo>();
            PostArgInfos = new List<ArgumentInfo>();

            var propertyInfos = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var info in propertyInfos)
            {
                var attrs = info.GetCustomAttributes(typeof(ArgumentAttribute), true);
                if (attrs.Length == 0)
                {
                    continue;
                }

                var argAttr = attrs[0] as ArgumentAttribute;

                var argInfo = new ArgumentInfo(
                    info.GetGetMethod(true).MethodHandle,
                    argAttr.Name,
                    argAttr.Optional,
                    argAttr.DefaultValue);

                if (argAttr.IsPostContent)
                {
                    PostArgInfos.Add(argInfo);
                }
                else
                {
                    UrlArgInfos.Add(argInfo);
                }
            }
        }

        public static ICollection<ArgumentInfo> UrlArgInfos { get; private set; }

        public static ICollection<ArgumentInfo> PostArgInfos { get; private set; }
    }
}