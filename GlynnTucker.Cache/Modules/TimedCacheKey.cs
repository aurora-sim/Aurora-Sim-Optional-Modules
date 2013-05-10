/*
 * 
 * Written by Darren Wurf for Glynn Tucker Consulting Engineers
 * Copyright (C) 2007 Glynn Tucker Consulting Engineers
 * See the file COPYING for licensing information.
 *  
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace GlynnTucker.Cache
{
    /// <summary>
    /// Used by the SimpleMemoryCache for expiring old values.
    /// </summary>
    internal class TimedCacheKey : IComparable
    {
        private DateTime _expirationDate;
        private bool _slidingExpiration;
        private TimeSpan _slidingExpirationWindowSize;
        private object _key;

        public DateTime ExpirationDate
        {
            get { return _expirationDate; }
        }

        public object Key
        {
            get { return _key; }
        }

        public bool SlidingExpiration
        {
            get { return _slidingExpiration; }
        }

        public TimeSpan SlidingExpirationWindowSize
        {
            get { return _slidingExpirationWindowSize; }
        }

        public TimedCacheKey(object key, DateTime expirationDate)
        {
            _key = key;
            _slidingExpiration = false;
            _expirationDate = expirationDate;
        }

        public TimedCacheKey(object key, TimeSpan slidingExpirationWindowSize)
        {
            _key = key;
            _slidingExpiration = true;
            _slidingExpirationWindowSize = slidingExpirationWindowSize;
            Accessed();
        }

        public void Accessed()
        {
            if (_slidingExpiration)
            {
                _expirationDate = DateTime.Now.Add(_slidingExpirationWindowSize);
            }
        }

        #region IComparable
        public int CompareTo(object other)
        {
                if (this == null && other == null) { return 0; }
                if (this == null) { return -1; }
                if (other == null) { return 1; }

                if (!(other is TimedCacheKey))
                {
                    throw new ArgumentException("Item to compare is not a CacheItemKey");
                }

            int dateComparer = this._expirationDate.CompareTo((other as TimedCacheKey)._expirationDate);
                if (dateComparer != 0)
                {
                    return -dateComparer;
                }

                return this._key.GetHashCode().CompareTo((other as TimedCacheKey)._key.GetHashCode());
        }
        #endregion
    }
}
