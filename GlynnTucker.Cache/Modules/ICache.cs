/*
 * 
 * Written by Darren Wurf for Glynn Tucker Consulting Engineers
 * Copyright (C) 2007 Glynn Tucker Consulting Engineers
 * See the file COPYING for licensing information.
 *  
 */

using System;
using System.Collections;

namespace GlynnTucker.Cache
{
    /// <summary>
    /// The interface that all GlynnTucker.Cache caches must implement.
    /// </summary>
    public interface ICache : 
//        IEnumerable, // included in IDictionary
//        ICollection, // Included in IDictionary
//        IList, // Bad idea, would need to maintain an additional index
//        IDictionaryEnumerator, // Needs to be a seperate object
        IDictionary

    {
        // TODO: Support for dependencies and callbacks.
        
        /// <summary>
        /// Adds an object to the cache.
        /// </summary>
        /// <param name="key">The key that will be used to retrieve the object.</param>
        /// <param name="value">The object to be stored.</param>
        /// <returns>false if the object already exists, true otherwise</returns>
        new bool Add(object key, object value);

        /// <summary>
        /// Adds an object to the cache.
        /// </summary>
        /// <param name="key">The key that will be used to retrieve the object.</param>
        /// <param name="value">The object to be stored.</param>
        /// <param name="expiration">The time when the object will be automatically removed from the cache.</param>
        /// <returns>false if the object already exists, true otherwise</returns>
        bool Add(object key, object value, DateTime expiration);
        /// <summary>
        /// Adds an object to the cache.
        /// </summary>
        /// <param name="key">The key that will be used to retrieve the object.</param>
        /// <param name="value">The object to be stored.</param>
        /// <param name="slidingExpiration">The maximum time between accesses before the objects is removed from the cache.</param>
        /// <returns>false if the object already exists, true otherwise</returns>
        bool Add(object key, object value, TimeSpan slidingExpiration);
        

        /// <summary>
        /// Adds (or replaces) an object in the cache.
        /// </summary>
        /// <param name="key">The key that will be used to retrieve the object.</param>
        /// <param name="value">The object to be stored.</param>
        /// <returns>false if the object was updated, true if the key was added.</returns>
        bool AddOrUpdate(object key, object value);

        /// <summary>
        /// Adds (or replaces) an object in the cache. If updated (replaced), the new expiry will be used.
        /// </summary>
        /// <param name="key">The key that will be used to retrieve the object.</param>
        /// <param name="value">The object to be stored.</param>
        /// <param name="expiration">The time when the object will be automatically removed from the cache.</param>
        /// <returns>false if the object was updated, true if the key was added.</returns>
        bool AddOrUpdate(object key, object value, DateTime expiration);

        /// <summary>
        /// Adds (or replaces) an object in the cache. If updated (relaced), the new sliding expiration will be used.
        /// </summary>
        /// <param name="key">The key that will be used to retrieve the object.</param>
        /// <param name="value">The object to be stored.</param>
        /// <param name="slidingExpiration">The maximum time between accesses before the objects is removed from the cache.</param>
        /// <returns>false if the object was updated, true if the key was added.</returns>
        bool AddOrUpdate(object key, object value, TimeSpan slidingExpiration);

        /// <summary>
        /// Gets an object from the cache.
        /// </summary>
        /// <param name="key">The key of the object to retrieve.</param>
        /// <returns>The specified object.</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when 
        /// the specified key does not exist in the cache.</exception>
        object Get(object key);

        /// <summary>
        /// Gets or sets an object in the cache, with an absolute expiry.
        /// </summary>
        /// <param name="key">The key of the object to insert/retrieve.</param>
        /// <param name="expiration">The time when the object will be automatically removed from the cache.</param>
        /// <returns>(When getting): The specified object.</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when 
        /// the specified key does not exist in the cache.</exception>
        object this[object key, DateTime expiration] { set; }

        /// <summary>
        /// Gets or sets an object in the cache, with an absolute expiry.
        /// </summary>
        /// <param name="key">The key of the object to insert/retrieve.</param>
        /// <param name="slidingExpiration">The maximum time between accesses before the objects is removed from the cache.</param>
        /// <returns>(When getting): The specified object.</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when 
        /// the specified key does not exist in the cache.</exception>
        object this[object key, TimeSpan slidingExpiration] { set; }

        // TODO: Support for enumeration

        /// <summary>
        /// Removes an object from the cache.
        /// </summary>
        /// <param name="key">The object to remove.</param>
        /// <returns>false if the object does not exist, true otherwise</returns>
        new bool Remove(object key);

        /// <summary>
        /// Attempts to get an object from the cache. Returns false on failure.
        /// </summary>
        /// <param name="key">The key of the object to retrieve.</param>
        /// <param name="value">After this method returns: contains the requested value
        /// on success, or null on failure.</param>
        /// <returns>True if the object exists in the cache, false otherwise.</returns>
        bool TryGet(object key, out object value);

        /// <summary>
        /// Updates an object in the cache with a new value.
        /// </summary>
        /// <param name="key">The key of the object to update.</param>
        /// <param name="value">The value to update to.</param>
        /// <returns>false if the object does not exist, true otherwise</returns>
        bool Update(object key, object value);

        /// <summary>
        /// Updates an object in the cache with a new value and/or expiration time.
        /// </summary>
        /// <param name="key">The key of the object to update.</param>
        /// <param name="value">The value to update to.</param>
        /// <param name="expiration">The new expiry date.</param>
        /// <returns>false if the object does not exist, true otherwise</returns>
        bool Update(object key, object value, DateTime expiration);

        /// <summary>
        /// Updates an object in the cache with a new value.
        /// </summary>
        /// <param name="key">The key of the object to update.</param>
        /// <param name="value">The value to update to.</param>
        /// <param name="expiration">The new expiry lifetime.</param>
        /// <returns>false if the object does not exist, true otherwise</returns>
        bool Update(object key, object value, TimeSpan slidingExpiration);
    }
}
