//-----------------------------------------------------------------------
// <copyright file="HtmlEntities.cs" company="iron9light">
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

    /// <summary>
    /// helper class for lookup of HTML encoding entities
    /// </summary>
    internal class HtmlEntities
    {
        private static readonly object LookupLockObject = new object();

        /// <summary>
        /// The list is from http://www.w3.org/TR/REC-html40/sgml/entities.html
        /// </summary>
        private static readonly string[] EntitiesList = new[] 
                                                              {
                                                                 "\x0022-quot", 
                                                                 "\x0026-amp",
                                                                 "\x003c-lt", 
                                                                 "\x003e-gt", 
                                                                 "\x00a0-nbsp",
                                                                 "\x00a1-iexcl", 
                                                                 "\x00a2-cent",
                                                                 "\x00a3-pound",
                                                                 "\x00a4-curren",
                                                                 "\x00a5-yen", 
                                                                 "\x00a6-brvbar",
                                                                 "\x00a7-sect", 
                                                                 "\x00a8-uml", 
                                                                 "\x00a9-copy",
                                                                 "\x00aa-ordf", 
                                                                 "\x00ab-laquo",
                                                                 "\x00ac-not",
                                                                 "\x00ad-shy",
                                                                 "\x00ae-reg", 
                                                                 "\x00af-macr",
                                                                 "\x00b0-deg", 
                                                                 "\x00b1-plusmn", 
                                                                 "\x00b2-sup2",
                                                                 "\x00b3-sup3", 
                                                                 "\x00b4-acute",
                                                                 "\x00b5-micro",
                                                                 "\x00b6-para",
                                                                 "\x00b7-middot", 
                                                                 "\x00b8-cedil",
                                                                 "\x00b9-sup1", 
                                                                 "\x00ba-ordm", 
                                                                 "\x00bb-raquo",
                                                                 "\x00bc-frac14", 
                                                                 "\x00bd-frac12",
                                                                 "\x00be-frac34",
                                                                 "\x00bf-iquest",
                                                                 "\x00c0-Agrave", 
                                                                 "\x00c1-Aacute",
                                                                 "\x00c2-Acirc", 
                                                                 "\x00c3-Atilde", 
                                                                 "\x00c4-Auml",
                                                                 "\x00c5-Aring", 
                                                                 "\x00c6-AElig",
                                                                 "\x00c7-Ccedil",
                                                                 "\x00c8-Egrave",
                                                                 "\x00c9-Eacute", 
                                                                 "\x00ca-Ecirc",
                                                                 "\x00cb-Euml", 
                                                                 "\x00cc-Igrave", 
                                                                 "\x00cd-Iacute",
                                                                 "\x00ce-Icirc", 
                                                                 "\x00cf-Iuml",
                                                                 "\x00d0-ETH",
                                                                 "\x00d1-Ntilde",
                                                                 "\x00d2-Ograve", 
                                                                 "\x00d3-Oacute",
                                                                 "\x00d4-Ocirc", 
                                                                 "\x00d5-Otilde", 
                                                                 "\x00d6-Ouml",
                                                                 "\x00d7-times", 
                                                                 "\x00d8-Oslash",
                                                                 "\x00d9-Ugrave",
                                                                 "\x00da-Uacute",
                                                                 "\x00db-Ucirc", 
                                                                 "\x00dc-Uuml",
                                                                 "\x00dd-Yacute", 
                                                                 "\x00de-THORN", 
                                                                 "\x00df-szlig",
                                                                 "\x00e0-agrave", 
                                                                 "\x00e1-aacute",
                                                                 "\x00e2-acirc",
                                                                 "\x00e3-atilde",
                                                                 "\x00e4-auml", 
                                                                 "\x00e5-aring",
                                                                 "\x00e6-aelig", 
                                                                 "\x00e7-ccedil", 
                                                                 "\x00e8-egrave",
                                                                 "\x00e9-eacute", 
                                                                 "\x00ea-ecirc",
                                                                 "\x00eb-euml",
                                                                 "\x00ec-igrave",
                                                                 "\x00ed-iacute", 
                                                                 "\x00ee-icirc",
                                                                 "\x00ef-iuml", 
                                                                 "\x00f0-eth", 
                                                                 "\x00f1-ntilde",
                                                                 "\x00f2-ograve", 
                                                                 "\x00f3-oacute",
                                                                 "\x00f4-ocirc",
                                                                 "\x00f5-otilde",
                                                                 "\x00f6-ouml", 
                                                                 "\x00f7-divide",
                                                                 "\x00f8-oslash", 
                                                                 "\x00f9-ugrave", 
                                                                 "\x00fa-uacute",
                                                                 "\x00fb-ucirc", 
                                                                 "\x00fc-uuml",
                                                                 "\x00fd-yacute",
                                                                 "\x00fe-thorn",
                                                                 "\x00ff-yuml", 
                                                                 "\x0152-OElig",
                                                                 "\x0153-oelig", 
                                                                 "\x0160-Scaron", 
                                                                 "\x0161-scaron",
                                                                 "\x0178-Yuml", 
                                                                 "\x0192-fnof",
                                                                 "\x02c6-circ",
                                                                 "\x02dc-tilde",
                                                                 "\x0391-Alpha", 
                                                                 "\x0392-Beta",
                                                                 "\x0393-Gamma", 
                                                                 "\x0394-Delta", 
                                                                 "\x0395-Epsilon",
                                                                 "\x0396-Zeta", 
                                                                 "\x0397-Eta",
                                                                 "\x0398-Theta",
                                                                 "\x0399-Iota",
                                                                 "\x039a-Kappa", 
                                                                 "\x039b-Lambda",
                                                                 "\x039c-Mu", 
                                                                 "\x039d-Nu", 
                                                                 "\x039e-Xi",
                                                                 "\x039f-Omicron", 
                                                                 "\x03a0-Pi",
                                                                 "\x03a1-Rho",
                                                                 "\x03a3-Sigma",
                                                                 "\x03a4-Tau", 
                                                                 "\x03a5-Upsilon",
                                                                 "\x03a6-Phi", 
                                                                 "\x03a7-Chi", 
                                                                 "\x03a8-Psi",
                                                                 "\x03a9-Omega", 
                                                                 "\x03b1-alpha",
                                                                 "\x03b2-beta",
                                                                 "\x03b3-gamma",
                                                                 "\x03b4-delta", 
                                                                 "\x03b5-epsilon",
                                                                 "\x03b6-zeta", 
                                                                 "\x03b7-eta", 
                                                                 "\x03b8-theta",
                                                                 "\x03b9-iota", 
                                                                 "\x03ba-kappa",
                                                                 "\x03bb-lambda",
                                                                 "\x03bc-mu",
                                                                 "\x03bd-nu", 
                                                                 "\x03be-xi",
                                                                 "\x03bf-omicron", 
                                                                 "\x03c0-pi", 
                                                                 "\x03c1-rho",
                                                                 "\x03c2-sigmaf", 
                                                                 "\x03c3-sigma",
                                                                 "\x03c4-tau",
                                                                 "\x03c5-upsilon",
                                                                 "\x03c6-phi", 
                                                                 "\x03c7-chi",
                                                                 "\x03c8-psi", 
                                                                 "\x03c9-omega", 
                                                                 "\x03d1-thetasym",
                                                                 "\x03d2-upsih", 
                                                                 "\x03d6-piv",
                                                                 "\x2002-ensp",
                                                                 "\x2003-emsp",
                                                                 "\x2009-thinsp", 
                                                                 "\x200c-zwnj",
                                                                 "\x200d-zwj", 
                                                                 "\x200e-lrm", 
                                                                 "\x200f-rlm",
                                                                 "\x2013-ndash", 
                                                                 "\x2014-mdash",
                                                                 "\x2018-lsquo",
                                                                 "\x2019-rsquo",
                                                                 "\x201a-sbquo", 
                                                                 "\x201c-ldquo",
                                                                 "\x201d-rdquo", 
                                                                 "\x201e-bdquo", 
                                                                 "\x2020-dagger",
                                                                 "\x2021-Dagger", 
                                                                 "\x2022-bull",
                                                                 "\x2026-----ip",
                                                                 "\x2030-permil",
                                                                 "\x2032-prime", 
                                                                 "\x2033-Prime",
                                                                 "\x2039-lsaquo", 
                                                                 "\x203a-rsaquo", 
                                                                 "\x203e-oline",
                                                                 "\x2044-frasl", 
                                                                 "\x20ac-euro",
                                                                 "\x2111-image",
                                                                 "\x2118-weierp",
                                                                 "\x211c-real", 
                                                                 "\x2122-trade",
                                                                 "\x2135-alefsym", 
                                                                 "\x2190-larr", 
                                                                 "\x2191-uarr",
                                                                 "\x2192-rarr", 
                                                                 "\x2193-darr",
                                                                 "\x2194-harr",
                                                                 "\x21b5-crarr",
                                                                 "\x21d0-lArr", 
                                                                 "\x21d1-uArr",
                                                                 "\x21d2-rArr", 
                                                                 "\x21d3-dArr", 
                                                                 "\x21d4-hArr",
                                                                 "\x2200-f----l", 
                                                                 "\x2202-part",
                                                                 "\x2203-exist",
                                                                 "\x2205-empty",
                                                                 "\x2207-nabla", 
                                                                 "\x2208-isin",
                                                                 "\x2209-notin", 
                                                                 "\x220b-ni", 
                                                                 "\x220f-prod",
                                                                 "\x2211-sum", 
                                                                 "\x2212-minus",
                                                                 "\x2217-lowast",
                                                                 "\x221a-radic",
                                                                 "\x221d-prop", 
                                                                 "\x221e-infin",
                                                                 "\x2220-ang", 
                                                                 "\x2227-and", 
                                                                 "\x2228-or",
                                                                 "\x2229-cap", 
                                                                 "\x222a-cup",
                                                                 "\x222b-int",
                                                                 "\x2234-there4",
                                                                 "\x223c-sim", 
                                                                 "\x2245-cong",
                                                                 "\x2248-asymp", 
                                                                 "\x2260-ne", 
                                                                 "\x2261-equiv",
                                                                 "\x2264-le", 
                                                                 "\x2265-ge",
                                                                 "\x2282-sub",
                                                                 "\x2283-sup",
                                                                 "\x2284-nsub", 
                                                                 "\x2286-sube",
                                                                 "\x2287-supe", 
                                                                 "\x2295-oplus", 
                                                                 "\x2297-otimes",
                                                                 "\x22a5-perp", 
                                                                 "\x22c5-sdot",
                                                                 "\x2308-lceil",
                                                                 "\x2309-rceil",
                                                                 "\x230a-lfloor", 
                                                                 "\x230b-rfloor",
                                                                 "\x2329-lang", 
                                                                 "\x232a-rang", 
                                                                 "\x25ca-loz",
                                                                 "\x2660-spades", 
                                                                 "\x2663-clubs",
                                                                 "\x2665-hearts",
                                                                 "\x2666-diams",
                                                             };

        private static Dictionary<string, char> entitiesLookupTable;

        private HtmlEntities()
        {
        }

        internal static char Lookup(string entity)
        {
            if (entitiesLookupTable == null)
            {
                // populate hashtable on demand 
                lock (LookupLockObject)
                {
                    if (entitiesLookupTable == null)
                    {
                        var t = new Dictionary<string, char>();

                        foreach (var s in EntitiesList)
                        {
                            t[s.Substring(2)] = s[0]; // 1st char is the code, 2nd '-'
                        }

                        entitiesLookupTable = t;
                    }
                }
            }

            char c;

            if (entitiesLookupTable.TryGetValue(entity, out c))
            {
                return c;
            }

            return (char)0;
        }
    }
}