/*
 * 
 * Written by Darren Wurf for Glynn Tucker Consulting Engineers
 * Copyright (C) 2007 Glynn Tucker Consulting Engineers
 * See the file COPYING for licensing information.
 *  
 */

using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace GlynnTucker.Cache
{
    public sealed class SimpleMemoryCache : ICache
    {
        #region Enumerator classes for the SimpleMemoryCache
        internal class EnumeratorJoiner : EnumeratorJoinerBase, IEnumerator
        {
            internal EnumeratorJoiner(SimpleMemoryCache parent, params IEnumerator[] enumerators) : base (parent.readWriteLock, enumerators)
            {
            }
        }

        internal class DictionaryEnumeratorJoiner : EnumeratorJoinerBase, IDictionaryEnumerator
        {
            private IList<IDictionaryEnumerator> enumerators;

            internal DictionaryEnumeratorJoiner(SimpleMemoryCache parent, params IDictionaryEnumerator[] enumerators)
                : base(parent.readWriteLock, enumerators)
            {
                this.enumerators = new List<IDictionaryEnumerator>(enumerators);
            }

            public override bool MoveNext()
            {
                rwLock.AcquireReaderLock(MAX_LOCK_WAIT);
                try
                {
                    if (currentEnumerator == null)
                    {
                        // First run of MoveNext()
                        currentEnumerator = 0;
                        enumerators[0].Reset();
                    }
                    else if (currentEnumerator == -1)
                    {
                        // MoveNext() has gone past the end of the item list.
                        // The state will remain invalid until Reset() is called.
                        return false;
                    }

                    if (enumerators[(int)currentEnumerator].MoveNext())
                    {
                        currentObject = enumerators[(int)currentEnumerator].Entry;
                        return true;
                    }
                    else
                    {
                        // We've hit the last item of the current enumerator;
                        if (currentEnumerator == enumerators.Count - 1)
                        {
                            // We're also on the last enumerator. State is now invalid.
                            currentEnumerator = -1;
                            currentObject = null;
                            return false;
                        }
                        else
                        {
                            currentEnumerator++;
                            return MoveNext();
                        }
                    }
                }
                finally { rwLock.ReleaseReaderLock(); }
            }
            public DictionaryEntry Entry
            {
                get
                {
                    return (DictionaryEntry)currentObject;
                }
            }
            public object Key
            {
                get
                {
                    return ((DictionaryEntry)currentObject).Key;
                }
            }
            public object Value
            {
                get
                {
                    return ((DictionaryEntry)currentObject).Value;
                }
            }
        }

        internal class CombinedDictionaryEnumerator : IDictionaryEnumerator
        {
            IDictionaryEnumerator keyEnumerator;
            IDictionaryEnumerator valueEnumerator;

            public CombinedDictionaryEnumerator(IDictionary keySource, IDictionary valueSource)
            {
                lock (this)
                {
                    keyEnumerator = keySource.GetEnumerator();
                    valueEnumerator = valueSource.GetEnumerator();
                }
            }

            public bool MoveNext()
            {
                lock (this)
                {
                    return keyEnumerator.MoveNext() && valueEnumerator.MoveNext();
                }
            }

            public void Reset()
            {
                lock (this)
                {
                    keyEnumerator.Reset();
                    valueEnumerator.Reset();
                }
            }

            public object Key
            {
                get { return keyEnumerator.Key; }
            }
            public object Value
            {
                get { return valueEnumerator.Value; }
            }

            public DictionaryEntry Entry
            {
                get { lock (this) { return new DictionaryEntry(keyEnumerator.Key, valueEnumerator.Value); } }
            }

            public object Current
            {
                get { return Entry; }
            }
        }
        #endregion
    
        #region Private and internal fields
        /// <summary>
        /// For thread safety.
        /// </summary>
        internal ReaderWriterLock readWriteLock = new ReaderWriterLock();
        internal const double CACHE_PURGE_HZ = 1.0;
        internal const int MAX_LOCK_WAIT = 5000; // milliseconds

        internal Hashtable untimedStorage = new Hashtable();
        internal SortedDictionary<TimedCacheKey, object> timedStorage = new SortedDictionary<TimedCacheKey, object>();
        internal Dictionary<object, TimedCacheKey> timedStorageIndex = new Dictionary<object, TimedCacheKey>();
        private System.Timers.Timer timer = new System.Timers.Timer(TimeSpan.FromSeconds(CACHE_PURGE_HZ).TotalMilliseconds);
        object isPurging = new object();
        #endregion

        #region Constructor
        public SimpleMemoryCache()
        {
            timer.Elapsed += new System.Timers.ElapsedEventHandler(PurgeCache);
            timer.Start();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Purges expired objects from the cache. Called automatically by the purge timer.
        /// </summary>
        private void PurgeCache(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Note: This implementation runs with low priority. If the cache lock
            // is heavily contended (many threads) the purge will take a long time
            // to obtain the lock it needs and may never be run.
            System.Threading.Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

            // Only let one thread purge at once - a buildup could cause a crash
            // This could cause the purge to be delayed while there are lots of read/write ops 
            // happening on the cache
            if (!Monitor.TryEnter(isPurging))
            {
                return;
            }

            try
            {
                readWriteLock.AcquireWriterLock(MAX_LOCK_WAIT);
                try
                {
                    List<object> expiredItems = new List<object>();
                    /*
                    DateTime startTime = DateTime.Now;
                    System.Console.WriteLine("Purge " + " started at " + startTime.ToLongTimeString());
                     */
                    foreach (TimedCacheKey timedKey in timedStorage.Keys)
                    {
                        if (timedKey.ExpirationDate < e.SignalTime)
                        {
                            // Mark the object for purge
                            expiredItems.Add(timedKey.Key);
                        }
                        else
                        {
                            break;
                        }
                    }

                    foreach (object key in expiredItems)
                    {
                        TimedCacheKey timedKey = timedStorageIndex[key];
                        timedStorageIndex.Remove(timedKey.Key);
                        timedStorage.Remove(timedKey);
                    }

                    /*
                    DateTime endTime = DateTime.Now;
                    System.Console.WriteLine("Purge completed at " + endTime.ToLongTimeString());
                    System.Console.WriteLine("Time taken to complete purge was " + TimeSpan.FromTicks(endTime.Ticks - startTime.Ticks));
                     */
                }
                catch (ApplicationException ae)
                {
                    // Unable to obtain write lock to the timed cache storage object
                    System.Console.WriteLine("Unable to complete cache purge, could not get writer lock.");
                }
                finally
                {
                    readWriteLock.ReleaseWriterLock();
                }
            }
            finally { Monitor.Exit(isPurging); }
        }
        #endregion

        #region ICache implementation
        public bool Add(object key, object value)
        {
            // Synchronise access to storage structures. A read lock may
            // already be acquired before this method is called.
            bool LockUpgraded = readWriteLock.IsReaderLockHeld;
            LockCookie lc = new LockCookie();
            if (LockUpgraded)
            {
                lc = readWriteLock.UpgradeToWriterLock(MAX_LOCK_WAIT);
            }
            else
            {
                readWriteLock.AcquireWriterLock(MAX_LOCK_WAIT);
            }
            try
            {
                // This is the actual adding of the key
                if (untimedStorage.ContainsKey(key))
                {
                    return false;
                }
                else
                {
                    untimedStorage.Add(key, value);
                    return true;
                }
            }
            finally
            {
                // Restore lock state
                if (LockUpgraded)
                {
                    readWriteLock.DowngradeFromWriterLock(ref lc);
                }
                else
                {
                    readWriteLock.ReleaseWriterLock();
                }
            }
        }

        public bool Add(object key, object value, DateTime expiration)
        {
            // Synchronise access to storage structures. A read lock may
            // already be acquired before this method is called.
            bool LockUpgraded = readWriteLock.IsReaderLockHeld;
            LockCookie lc = new LockCookie();
            if (LockUpgraded)
            {
                lc = readWriteLock.UpgradeToWriterLock(MAX_LOCK_WAIT);
            }
            else
            {
                readWriteLock.AcquireWriterLock(MAX_LOCK_WAIT);
            }
            try
            {
                // This is the actual adding of the key
                if (timedStorageIndex.ContainsKey(key))
                {
                    return false;
                }
                else
                {
                    TimedCacheKey internalKey = new TimedCacheKey(key, expiration);
                    timedStorage.Add(internalKey, value);
                    timedStorageIndex.Add(key, internalKey);
                    return true;
                }
            }
            finally
            {
                // Restore lock state
                if (LockUpgraded)
                {
                    readWriteLock.DowngradeFromWriterLock(ref lc);
                }
                else
                {
                    readWriteLock.ReleaseWriterLock();
                }
            }
        }

        public bool Add(object key, object value, TimeSpan slidingExpiration)
        {
            // Synchronise access to storage structures. A read lock may
            // already be acquired before this method is called.
            bool LockUpgraded = readWriteLock.IsReaderLockHeld;
            LockCookie lc = new LockCookie();
            if (LockUpgraded)
            {
                lc = readWriteLock.UpgradeToWriterLock(MAX_LOCK_WAIT);
            }
            else
            {
                readWriteLock.AcquireWriterLock(MAX_LOCK_WAIT);
            }
            try
            {
                // This is the actual adding of the key
                if (timedStorageIndex.ContainsKey(key))
                {
                    return false;
                }
                else
                {
                    TimedCacheKey internalKey = new TimedCacheKey(key, slidingExpiration);
                    timedStorage.Add(internalKey, value);
                    timedStorageIndex.Add(key, internalKey);
                    return true;
                }
            }
            finally
            {
                // Restore lock state
                if (LockUpgraded)
                {
                    readWriteLock.DowngradeFromWriterLock(ref lc);
                }
                else
                {
                    readWriteLock.ReleaseWriterLock();
                }
            }
        }

        public bool AddOrUpdate(object key, object value)
        {
            readWriteLock.AcquireReaderLock(MAX_LOCK_WAIT);
            try
            {
                if (Contains(key))
                {
                    Update(key, value);
                    return false;
                }
                else
                {
                    Add(key, value);
                    return true;
                }
            }
            finally { readWriteLock.ReleaseReaderLock(); }
        }

        public bool AddOrUpdate(object key, object value, DateTime expiration)
        {
            readWriteLock.AcquireReaderLock(MAX_LOCK_WAIT);
            try
            {
                if (Contains(key))
                {
                    Update(key, value, expiration);
                    return false;
                }
                else
                {
                    Add(key, value, expiration);
                    return true;
                }
            }
            finally { readWriteLock.ReleaseReaderLock(); }
        }

        public bool AddOrUpdate(object key, object value, TimeSpan slidingExpiration)
        {
            readWriteLock.AcquireReaderLock(MAX_LOCK_WAIT);
            try
            {
                if (Contains(key))
                {
                    Update(key, value, slidingExpiration);
                    return false;
                }
                else
                {
                    Add(key, value, slidingExpiration);
                    return true;
                }
            }
            finally { readWriteLock.ReleaseReaderLock(); }
        }

        public void Clear()
        {
            readWriteLock.AcquireWriterLock(MAX_LOCK_WAIT);
            try
            {
                timedStorage.Clear();
                timedStorageIndex.Clear();
                untimedStorage.Clear();
            }
            finally { readWriteLock.ReleaseWriterLock(); }
        }

        public bool Contains(object key)
        {
            readWriteLock.AcquireReaderLock(MAX_LOCK_WAIT);
            try
            {
                return untimedStorage.ContainsKey(key) || timedStorageIndex.ContainsKey(key);
            }
            finally { readWriteLock.ReleaseReaderLock(); }
        }

        public int Count
        {
            get
            {
                int theCount = 0;

                readWriteLock.AcquireReaderLock(MAX_LOCK_WAIT);
                try
                {
                    theCount = untimedStorage.Count + timedStorageIndex.Count;
                }
                finally { readWriteLock.ReleaseReaderLock(); }
         
                return theCount;
            }
        }

        public object Get(object key)
        {
            object o;
            readWriteLock.AcquireReaderLock(MAX_LOCK_WAIT);
            try
            {
                if (untimedStorage.ContainsKey(key))
                {
                    return untimedStorage[key];
                }

                LockCookie lc = readWriteLock.UpgradeToWriterLock(MAX_LOCK_WAIT);
                try
                {
                    if (timedStorageIndex.ContainsKey(key))
                    {
                        TimedCacheKey tkey = timedStorageIndex[key];
                        o = timedStorage[tkey];
                        timedStorage.Remove(tkey);
                        tkey.Accessed();
                        timedStorage.Add(tkey, o);
                        return o;
                    }
                    else
                    {
                        throw new ArgumentException("Key not found in the cache");
                    }
                }
                finally { readWriteLock.DowngradeFromWriterLock(ref lc); }
            }
            finally { readWriteLock.ReleaseReaderLock(); }
        }

        public object this[object key]
        {
            get
            {
                return Get(key);
            }
            set
            {
                AddOrUpdate(key, value);
            }
        }

        public object this[object key, DateTime expiration]
        {
            set
            {
                AddOrUpdate(key, value, expiration);
            }
        }

        public object this[object key, TimeSpan slidingExpiration]
        {
            set
            {
                AddOrUpdate(key, value, slidingExpiration);
            }
        }

        public bool Remove(object key)
        {
            readWriteLock.AcquireWriterLock(MAX_LOCK_WAIT);
            try
            {
                if (untimedStorage.ContainsKey(key))
                {
                    untimedStorage.Remove(key);
                    return true;
                }
                else if (timedStorageIndex.ContainsKey(key))
                {
                    timedStorage.Remove(timedStorageIndex[key]);
                    timedStorageIndex.Remove(key);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            finally { readWriteLock.ReleaseWriterLock(); }
        }

        public bool TryGet(object key, out object value)
        {
            object o;

            readWriteLock.AcquireReaderLock(MAX_LOCK_WAIT);
            try
            {
                if (untimedStorage.ContainsKey(key))
                {
                    value = untimedStorage[key];
                    return true;
                }

                LockCookie lc = readWriteLock.UpgradeToWriterLock(MAX_LOCK_WAIT);
                try
                {
                    if (timedStorageIndex.ContainsKey(key))
                    {
                        TimedCacheKey tkey = timedStorageIndex[key];
                        o = timedStorage[tkey];
                        timedStorage.Remove(tkey);
                        tkey.Accessed();
                        timedStorage.Add(tkey, o);
                        value = o;
                        return true;
                    }
                    else
                    {
                        value = null;
                        return false;
                    }
                }
                finally { readWriteLock.DowngradeFromWriterLock(ref lc); }
            }
            finally { readWriteLock.ReleaseReaderLock(); }
        }

        public bool Update(object key, object value)
        {
            // Synchronise access to storage structures. A read lock may
            // already be acquired before this method is called.
            bool LockUpgraded = readWriteLock.IsReaderLockHeld;
            LockCookie lc = new LockCookie();
            if (LockUpgraded)
            {
                lc = readWriteLock.UpgradeToWriterLock(MAX_LOCK_WAIT);
            }
            else
            {
                readWriteLock.AcquireWriterLock(MAX_LOCK_WAIT);
            }
            try
            {
                if (untimedStorage.ContainsKey(key))
                {
                    untimedStorage.Remove(key);
                    untimedStorage.Add(key, value);
                    return true;
                }
                else if (timedStorageIndex.ContainsKey(key))
                {
                    timedStorage.Remove(timedStorageIndex[key]);
                    timedStorageIndex[key].Accessed();
                    timedStorage.Add(timedStorageIndex[key], value);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                // Restore lock state
                if (LockUpgraded)
                {
                    readWriteLock.DowngradeFromWriterLock(ref lc);
                }
                else
                {
                    readWriteLock.ReleaseWriterLock();
                }
            }
        }

        public bool Update(object key, object value, DateTime expiration)
        {
            // Synchronise access to storage structures. A read lock may
            // already be acquired before this method is called.
            bool LockUpgraded = readWriteLock.IsReaderLockHeld;
            LockCookie lc = new LockCookie();
            if (LockUpgraded)
            {
                lc = readWriteLock.UpgradeToWriterLock(MAX_LOCK_WAIT);
            }
            else
            {
                readWriteLock.AcquireWriterLock(MAX_LOCK_WAIT);
            }
            try
            {
                if (untimedStorage.ContainsKey(key))
                {
                    untimedStorage.Remove(key);
                }
                else if (timedStorageIndex.ContainsKey(key))
                {
                    timedStorage.Remove(timedStorageIndex[key]);
                    timedStorageIndex.Remove(key);
                }
                else
                {
                    return false;
                }

                TimedCacheKey internalKey = new TimedCacheKey(key, expiration);
                timedStorage.Add(internalKey, value);
                timedStorageIndex.Add(key, internalKey);
                return true;
            }
            finally
            {
                // Restore lock state
                if (LockUpgraded)
                {
                    readWriteLock.DowngradeFromWriterLock(ref lc);
                }
                else
                {
                    readWriteLock.ReleaseWriterLock();
                }
            }
        }

        public bool Update(object key, object value, TimeSpan slidingExpiration)
        {
            // Synchronise access to storage structures. A read lock may
            // already be acquired before this method is called.
            bool LockUpgraded = readWriteLock.IsReaderLockHeld;
            LockCookie lc = new LockCookie();
            if (LockUpgraded)
            {
                lc = readWriteLock.UpgradeToWriterLock(MAX_LOCK_WAIT);
            }
            else
            {
                readWriteLock.AcquireWriterLock(MAX_LOCK_WAIT);
            }
            try
            {
                if (untimedStorage.ContainsKey(key))
                {
                    untimedStorage.Remove(key);
                }
                else if (timedStorageIndex.ContainsKey(key))
                {
                    timedStorage.Remove(timedStorageIndex[key]);
                    timedStorageIndex.Remove(key);
                }
                else
                {
                    return false;
                }
                TimedCacheKey internalKey = new TimedCacheKey(key, slidingExpiration);
                timedStorage.Add(internalKey, value);
                timedStorageIndex.Add(key, internalKey);
                return true;
            }
            finally
            {
                // Restore lock state
                if (LockUpgraded)
                {
                    readWriteLock.DowngradeFromWriterLock(ref lc);
                }
                else
                {
                    readWriteLock.ReleaseWriterLock();
                }
            }
        }

        #endregion

        #region IDictionary
        public ICollection Keys
        {
            get
            {
                // TODO: should we be passing a lock?
                return new CollectionJoinerBase(timedStorageIndex.Keys, untimedStorage.Keys);
            }
        }

        public ICollection Values
        {
            get
            {
                // TODO: should we be passing a lock?
                return new CollectionJoinerBase(timedStorage.Values, untimedStorage.Values);
            }
        }

        // A cache is not read only
        public bool IsReadOnly { get { return false; } }

        // A cache is not fixed-size
        public bool IsFixedSize { get { return false; } }

        public void CopyTo(Array array, int startIndex)
        {
            // Error checking
            if (array == null) { throw new ArgumentNullException("array"); }
            
            if (startIndex < 0) { throw new ArgumentOutOfRangeException("startIndex", "startIndex must be >= 0."); }

            if (array.Rank > 1) { throw new ArgumentException("array must be of Rank 1 (one-dimensional)", "array"); }
            if (startIndex >= array.Length) { throw new ArgumentException("startIndex must be less than the length of the array.", "startIndex"); }
            if (Count > array.Length - startIndex) { throw new ArgumentException("There is not enough space from startIndex to the end of the array to accomodate all items in the cache."); }

            // Copy the data to the array (in a thread-safe manner)
            readWriteLock.AcquireReaderLock(MAX_LOCK_WAIT);
            try
            {
                foreach (object o in timedStorage)
                {
                    array.SetValue(o, startIndex);
                    startIndex++;
                }
                foreach (object o in untimedStorage)
                {
                    array.SetValue(o, startIndex);
                    startIndex++;
                }
            }
            finally { readWriteLock.ReleaseReaderLock(); }
        }

        public object SyncRoot { get { return this;/*prototype implementation. TODO: think on this some more.*/ } }

        // This implementation is thread-safe
        public bool IsSynchronized { get { return true; } }

        public IDictionaryEnumerator GetEnumerator() { return new DictionaryEnumeratorJoiner(this, new CombinedDictionaryEnumerator(timedStorageIndex, timedStorage), untimedStorage.GetEnumerator()); }

        // TODO stuff
        public void foo()
        {
            IDictionary d = null; object o;
            o = d.SyncRoot; // This one will require some research - read up on Monitor locking
            o = d.Keys; // time-consuming
            o = d.Values; // time consuming
            o = d.GetEnumerator(); // a tad complex!
        }

        #endregion

        #region Suppress redundant IDictionary implementations
        void IDictionary.Add(object key, object value) { }
        void IDictionary.Remove(object key) { }

        IEnumerator IEnumerable.GetEnumerator() { return null; }
        #endregion
    }
}
