//-----------------------------------------------------------------------
// <copyright file="ArgumentAttribute.cs" company="iron9light">
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

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    internal sealed class ArgumentAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentAttribute"/> class.
        /// </summary>
        /// <param name="name">
        /// The argument name.
        /// </param>
        public ArgumentAttribute(string name)
        {
            this.Name = name;
            this.Optional = true;
            this.DefaultValue = null;
            this.IsPostContent = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentAttribute"/> class.
        /// </summary>
        /// <param name="name">
        /// The argument name.
        /// </param>
        /// <param name="defaultValue">
        /// Default value.
        /// </param>
        public ArgumentAttribute(string name, object defaultValue)
        {
            this.Name = name;
            if (defaultValue == null)
            {
                this.Optional = true;
            }
            else
            {
                this.Optional = false;
            }

            this.DefaultValue = defaultValue;
            this.IsPostContent = false;
        }

        /// <summary>
        /// Gets the argument name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this argument is optional.
        /// The default value is true.
        /// </summary>
        public bool Optional { get; set; }

        /// <summary>
        /// Gets the default value. Or return null is no default value.
        /// </summary>
        public object DefaultValue { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this argument is post content.
        /// The default value is false.
        /// </summary>
        public bool IsPostContent { get; set; }
    }
}
