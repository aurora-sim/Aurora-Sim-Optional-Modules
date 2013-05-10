/*
 * 
 * Written by Darren Wurf for Glynn Tucker Consulting Engineers
 * Copyright (C) 2007 Glynn Tucker Consulting Engineers
 * See the file COPYING for licensing information.
 *  
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace GlynnTucker.Cache
{
    /// <summary>
    /// Static methods for using caches. All methods are thread safe.
    /// </summary>
    public class Cache
    {
        #region Private region
        // The context pool, it holds our caches.
        private static Dictionary<string, ICache> caches = new Dictionary<string, ICache>();
        // A default expiry time of 0 means no automatic expiration
        private static uint _defaultExpiryTime = 0;

        // Initialisation of static cache
        static Cache()
        {
            // Add a default context
            AddContext("default");
            Contexts = caches.Keys;
        }

        private static System.Threading.ReaderWriterLock readWriteLock = new System.Threading.ReaderWriterLock();
        private const int MAX_LOCK_WAIT = 1000; // milliseconds

        #endregion

        #region Static properties
        /// <summary>
        /// The exiry time for objects where no expiry was added.
        /// </summary>
        public static uint DefaultExpiryTime
        {
            get { return _defaultExpiryTime; }
            set { _defaultExpiryTime = value; }
        }
        #endregion

        #region Static methods for managing contexts

        /// <summary>
        /// Create a new SimpleMemoryCache in the context pool.
        /// </summary>
        /// <param name="context">The name of the context to create.</param>
        /// <returns>False if the context already exists, true otherwise.</returns>
        public static bool AddContext(string context)
        {
            return AddContext(context, typeof(SimpleMemoryCache));
        }

        /// <summary>
        /// Creates a new ICache of the specified type.
        /// </summary>
        /// <param name="context">The name of the context to create.</param>
        /// <param name="cacheType">The type of ICache to create.</param>
        /// <returns>False if the context already exists, true otherwise.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when type 'cacheType' does not implement GlynnTucker.Cache.ICache</exception>
        public static bool AddContext(string context, Type cacheType)
        {
            ICache newCache = (ICache)Activator.CreateInstance(cacheType);
            readWriteLock.AcquireWriterLock(MAX_LOCK_WAIT);
            try
            {
                if (caches.ContainsKey(context))
                {
                    return false;
                }
                else
                {
                    caches.Add(context, newCache);
                    return true;
                }
            }
            finally { readWriteLock.ReleaseWriterLock(); }
        }

        /// <summary>
        /// Removes all objects in the specified cache context.
        /// </summary>
        /// <param name="context">The cache context to clear.</param>
        /// <returns>False if the context does not exist, true otherwise.</returns>
        public static bool ClearContext(string context)
        {
            bool retval = true;
            readWriteLock.AcquireReaderLock(MAX_LOCK_WAIT);
            try
            {
                if (caches.ContainsKey(context))
                {
                    caches[context].Clear();
                }
                else
                {
                    retval = false;
                }
            }
            finally { readWriteLock.ReleaseReaderLock(); }
            return retval;
        }

        /// <summary>
        /// Checks for the existence of a specified cache context.
        /// </summary>
        /// <param name="context">The context to look for.</param>
        /// <returns>True if the context exists, false otherwise.</returns>
        public static bool ContainsContext(string context)
        {
            bool exists;

            readWriteLock.AcquireReaderLock(MAX_LOCK_WAIT);
            try
            {
                exists = caches.ContainsKey(context);
            }
            finally { readWriteLock.ReleaseReaderLock(); }

            return exists;
        }

        /// <summary>
        /// Removes a context from the context pool.
        /// </summary>
        /// <param name="context">The context to remove.</param>
        /// <returns>False if the context does not exist, true otherwise.</returns>
        public static bool RemoveContext(string context)
        {
            bool retval = true;
            readWriteLock.AcquireReaderLock(MAX_LOCK_WAIT);
            try
            {
                if (caches.ContainsKey(context))
                {
                    caches[context].Clear();

                    LockCookie lc = readWriteLock.UpgradeToWriterLock(MAX_LOCK_WAIT);
                    try
                    {
                        caches.Remove(context);
                    }
                    finally { readWriteLock.DowngradeFromWriterLock(ref lc); }
                }
                else
                {
                    retval = false;
                }
            }
            finally { readWriteLock.ReleaseReaderLock(); }
            return retval;
        }
        #endregion

        #region Static methods for adding/removing data
        /// <summary>
        /// Adds an object to the specified cache context.
        /// </summary>
        /// <param name="context">The context to add the object to.</param>
        /// <param name="key">The key used to retrieve the object.</param>
        /// <param name="value">The object to be stored.</param>
        /// <returns>False when the key already exists, true otherwise.</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the specified context does not exist.</exception>
        public static bool Add(string context, object key, object value)
        {
            readWriteLock.AcquireReaderLock(MAX_LOCK_WAIT);
            try
            {
                if (caches.ContainsKey(context))
                {
                    if (DefaultExpiryTime == 0)
                    {
                        return caches[context].Add(key, value);
                    }
                    else
                    {
                        return caches[context].Add(key, value, TimeSpan.FromSeconds(DefaultExpiryTime));
                    }
                }
                else
                {
                    throw new KeyNotFoundException("The context '" + context + "' does not exist.");
                }
            }
            finally { readWriteLock.ReleaseReaderLock(); }
        }

        /// <summary>
        /// Adds an object to the specified cache context.
        /// </summary>
        /// <param name="context">The context to add the object to.</param>
        /// <param name="key">The key used to retrieve the object.</param>
        /// <param name="value">The object to be stored.</param>
        /// <param name="expiration">The time the object will be removed from the cache.</param>
        /// <returns>False when the key already exists, true otherwise.</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the specified context does not exist.</exception>
        public static bool Add(string context, object key, object value, DateTime expiration)
        {
            readWriteLock.AcquireReaderLock(MAX_LOCK_WAIT);
            try
            {
                if (caches.ContainsKey(context))
                {
                    return caches[context].Add(key, value, expiration);
                }
                else
                {
                    throw new KeyNotFoundException("The context '" + context + "' does not exist.");
                }
            }
            finally { readWriteLock.ReleaseReaderLock(); }
        }

        /// <summary>
        /// Adds an object to the specified cache context.
        /// </summary>
        /// <param name="context">The context to add the object to.</param>
        /// <param name="key">The key used to retrieve the object.</param>
        /// <param name="value">The object to be stored.</param>
        /// <param name="slidingExpiration">The maximum length of time between accesses before the object is removed from the cache context.</param>
        /// <returns>False when the key already exists, true otherwise.</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the specified context does not exist.</exception>
        public static bool Add(string context, object key, object value, TimeSpan slidingExpiration)
        {
            readWriteLock.AcquireReaderLock(MAX_LOCK_WAIT);
            try
            {
                if (caches.ContainsKey(context))
                {
                    return caches[context].Add(key, value, slidingExpiration);
                }
                else
                {
                    throw new KeyNotFoundException("The context '" + context + "' does not exist.");
                }
            }
            finally { readWriteLock.ReleaseReaderLock(); }
        }

        /// <summary>
        /// Adds (or replaces) an object to the specified cache context.
        /// </summary>
        /// <param name="context">The context to add the object to.</param>
        /// <param name="key">The key used to retrieve the object.</param>
        /// <param name="value">The object to be stored.</param>
        /// <returns>false if the object was updated, true if the key was added.</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the specified context does not exist.</exception>
        public static bool AddOrUpdate(string context, object key, object value)
        {
            readWriteLock.AcquireReaderLock(MAX_LOCK_WAIT);
            try
            {
                if (caches.ContainsKey(context))
                {
                    ICache tmpCache = caches[context];

                    if (DefaultExpiryTime == 0)
                    {
                        return tmpCache.AddOrUpdate(key, value);
                    }
                    else
                    {
                        if (tmpCache.Contains(key))
                        {
                            tmpCache.Remove(key);
                        }
                        return tmpCache.AddOrUpdate(key, value, TimeSpan.FromSeconds(DefaultExpiryTime));
                    }
                }
                else
                {
                    throw new KeyNotFoundException("The context '" + context + "' does not exist.");
                }
            }
            finally { readWriteLock.ReleaseReaderLock(); }
        }

        /// <summary>
        /// Adds an object to the specified cache context.
        /// </summary>
        /// <param name="context">The context to add the object to.</param>
        /// <param name="key">The key used to retrieve the object.</param>
        /// <param name="value">The object to be stored.</param>
        /// <param name="expiration">The time the object will be removed from the cache.</param>
        /// <returns>false if the object was updated, true if the key was added.</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the specified context does not exist.</exception>
        public static bool AddOrUpdate(string context, object key, object value, DateTime expiration)
        {
            readWriteLock.AcquireReaderLock(MAX_LOCK_WAIT);
            try
            {
                if (caches.ContainsKey(context))
                {
                    return caches[context].AddOrUpdate(key, value, expiration);
                }
                else
                {
                    throw new KeyNotFoundException("The context '" + context + "' does not exist.");
                }
            }
            finally { readWriteLock.ReleaseReaderLock(); }
        }

        /// <summary>
        /// Adds an object to the specified cache context.
        /// </summary>
        /// <param name="context">The context to add the object to.</param>
        /// <param name="key">The key used to retrieve the object.</param>
        /// <param name="value">The object to be stored.</param>
        /// <param name="slidingExpiration">The maximum length of time between accesses before the object is removed from the cache context.</param>
        /// <returns>false if the object was updated, true if the key was added.</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the specified context does not exist.</exception>
        public static bool AddOrUpdate(string context, object key, object value, TimeSpan slidingExpiration)
        {
            readWriteLock.AcquireReaderLock(MAX_LOCK_WAIT);
            try
            {
                if (caches.ContainsKey(context))
                {
                    return caches[context].AddOrUpdate(key, value, slidingExpiration);
                }
                else
                {
                    throw new KeyNotFoundException("The context '" + context + "' does not exist.");
                }
            }
            finally { readWriteLock.ReleaseReaderLock(); }
        }

        // TODO: Support for dependencies and callbacks.

        /// <summary>
        /// Checks for the existence of an object in a context.
        /// </summary>
        /// <param name="context">The cache context to look in.</param>
        /// <param name="key">The object to look for.</returns>
        /// <returns>True if the object exists in the specified context, false otherwise.</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the specified context does not exist.</exception>
        public static bool Contains(string context, object key)
        {
            bool exists = false;
            readWriteLock.AcquireReaderLock(MAX_LOCK_WAIT);
            try
            {
                if (caches.ContainsKey(context))
                {
                    exists = caches[context].Contains(key);
                }
                else
                {
                    throw new KeyNotFoundException("The context '" + context + "' does not exist.");
                }
            }
            finally { readWriteLock.ReleaseReaderLock(); }
            return exists;
        }

        /// <summary>
        /// Returns the number of objects stored in the specified cache context.
        /// </summary>
        /// <param name="context">The cache context to count.</param>
        /// <returns>The number of objects in the specified cache context.</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the specified context does not exist.</exception>
        public static int Count(string context)
        {
            readWriteLock.AcquireReaderLock(MAX_LOCK_WAIT);
            try
            {
                if (caches.ContainsKey(context))
                {
                    return caches[context].Count;
                }
                else
                {
                    throw new KeyNotFoundException("The context '" + context + "' does not exist.");
                }
            }
            finally { readWriteLock.ReleaseReaderLock(); }
        }

        /// <summary>
        /// Fetches the specified object from the specified cache context.
        /// </summary>
        /// <param name="context">The cache context to retrieve an object from.</param>
        /// <param name="key">The key of the object to retrieve.</param>
        /// <returns>The requested object.</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the specified context does not exist.</exception>
        public static object Get(string context, object key)
        {
            readWriteLock.AcquireReaderLock(MAX_LOCK_WAIT);
            try
            {
                if (caches.ContainsKey(context))
                {
                    return caches[context].Get(key);
                }
                else
                {
                    throw new KeyNotFoundException("The context '" + context + "' does not exist.");
                }
            }
            finally { readWriteLock.ReleaseReaderLock(); }
        }

        /// <summary>
        /// Gets an IDictionaryEnumerator for stepping through a single cache context.
        /// </summary>
        /// <param name="context">The cache context to step through.</param>
        /// <returns>The requested IDictionaryEnumerator.</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the specified context does not exist.</exception>
        public static IDictionaryEnumerator GetEnumerator(string context)
        {
            readWriteLock.AcquireReaderLock(MAX_LOCK_WAIT);
            try
            {
                if (caches.ContainsKey(context))
                {
                    return caches[context].GetEnumerator();
                }
                else
                {
                    throw new KeyNotFoundException("The context '" + context + "' does not exist.");
                }
            }
            finally { readWriteLock.ReleaseReaderLock(); }
        }

        /// <summary>
        /// Removes the specified object from the specified cache context.
        /// </summary>
        /// <param name="context">The cache context to remove an object from.</param>
        /// <param name="key">The key of the object to remove.</param>
        /// <returns>false if the object does not exist, true otherwise</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the specified context does not exist.</exception>
        public static bool Remove(string context, object key)
        {
            readWriteLock.AcquireReaderLock(MAX_LOCK_WAIT);
            try
            {
                if (caches.ContainsKey(context))
                {
                    return caches[context].Remove(key);
                }
                else
                {
                    throw new KeyNotFoundException("The context '" + context + "' does not exist.");
                }
            }
            finally { readWriteLock.ReleaseReaderLock(); }
        }

        /// <summary>
        /// Attempts to get an object from the cache. Returns false on failure.
        /// </summary>
        /// <param name="context">The cache context to remove an object from.</param>
        /// <param name="key">The key of the object to retrieve.</param>
        /// <param name="value">After this method returns: contains the requested value
        /// on success, or null on failure.</param>
        /// <returns>True if the object exists in the cache, false otherwise.</returns>
        public static bool TryGet(string context, object key, out object value)
        {
            readWriteLock.AcquireReaderLock(MAX_LOCK_WAIT);
            try
            {
                if (caches.ContainsKey(context))
                {
                    return caches[context].TryGet(key, out value);
                }
                else
                {
                    throw new KeyNotFoundException("The context '" + context + "' does not exist.");
                }
            }
            finally { readWriteLock.ReleaseReaderLock(); }
        }

        /// <summary>
        /// Update an object in the specified context. The object to be updated must exist.
        /// </summary>
        /// <param name="context">The context containing the object to update.</param>
        /// <param name="key">The key of the object to update.</param>
        /// <param name="value">The new value of the object.</param>
        /// <returns>false if the object does not exist, true otherwise</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the specified context does not exist.</exception>
        public static bool Update(string context, object key, object value)
        {
            readWriteLock.AcquireReaderLock(MAX_LOCK_WAIT);
            try
            {
                if (caches.ContainsKey(context))
                {
                    return caches[context].Update(key, value);
                }
                else
                {
                    throw new KeyNotFoundException("The context '" + context + "' does not exist.");
                }
            }
            finally { readWriteLock.ReleaseReaderLock(); }
        }

        /// <summary>
        /// Update an object in the specified context. The object to be updated must exist.
        /// </summary>
        /// <param name="context">The context containing the object to update.</param>
        /// <param name="key">The key of the object to update.</param>
        /// <param name="value">The new value of the object.</param>
        /// <returns>false if the object does not exist, true otherwise</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the specified context does not exist.</exception>
        public static bool Update(string context, object key, object value, DateTime expiration)
        {
            readWriteLock.AcquireReaderLock(MAX_LOCK_WAIT);
            try
            {
                if (caches.ContainsKey(context))
                {
                    return caches[context].Update(key, value, expiration);
                }
                else
                {
                    throw new KeyNotFoundException("The context '" + context + "' does not exist.");
                }
            }
            finally { readWriteLock.ReleaseReaderLock(); }
        }

        /// <summary>
        /// Update an object in the specified context. The object to be updated must exist.
        /// </summary>
        /// <param name="context">The context containing the object to update.</param>
        /// <param name="key">The key of the object to update.</param>
        /// <param name="value">The new value of the object.</param>
        /// <returns>false if the object does not exist, true otherwise</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the specified context does not exist.</exception>
        public static bool Update(string context, string key, object value, TimeSpan slidingExpiration)
        {
            readWriteLock.AcquireReaderLock(MAX_LOCK_WAIT);
            try
            {
                if (caches.ContainsKey(context))
                {
                    return caches[context].Update(key, value, slidingExpiration);
                }
                else
                {
                    throw new KeyNotFoundException("The context '" + context + "' does not exist.");
                }
            }
            finally { readWriteLock.ReleaseReaderLock(); }
        }

        #endregion

        /// <summary>
        /// An enumerable object representing the cache contexts in the global cache.
        /// </summary>
        public static IEnumerable<string> Contexts;
        //public static GlynnTucker.Cache.Contexts Contexts = new Contexts(ref Cache.caches.Keys);
    }

}
