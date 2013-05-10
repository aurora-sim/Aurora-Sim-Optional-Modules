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
using System.Text;
using System.Threading;

namespace GlynnTucker.Cache
{
    class CollectionJoinerBase : ICollection
    {
        IList<ICollection> collections;
        public CollectionJoinerBase(params ICollection[] collections)
        {
            this.collections = collections;
        }

        public void CopyTo(Array array, int startIndex)
        {
            // Error checking
            if (array == null) { throw new ArgumentNullException("array"); }

            if (startIndex < 0) { throw new ArgumentOutOfRangeException("startIndex", "startIndex must be >= 0."); }

            if (array.Rank > 1) { throw new ArgumentException("array must be of Rank 1 (one-dimensional)", "array"); }
            if (startIndex >= array.Length) { throw new ArgumentException("startIndex must be less than the length of the array.", "startIndex"); }
            if (Count > array.Length - startIndex) { throw new ArgumentException("There is not enough space from startIndex to the end of the array to accomodate all items in the cache."); }

            // Copy the data to the array (in a thread-safe manner)
            foreach (ICollection c in collections)
            {
                c.CopyTo(array, startIndex);
                startIndex += c.Count;
            }
        }

        public int Count
        {
            get
            {
                int theCount = 0;
                foreach (ICollection c in collections)
                {
                    theCount += c.Count;
                }
                return theCount;
            }
        }

        public object SyncRoot { get { return this; } }

        // TODO: Is this correct?
        private bool? isSync = null;
        public bool IsSynchronized { 
            get {
                if (isSync == null)
                {
                    isSync = true;
                    foreach (ICollection c in collections)
                    {
                        isSync &= c.IsSynchronized;
                    }
                }
                return (bool)isSync;
            } 
        }

        protected ReaderWriterLock rwLock = new ReaderWriterLock();
        public IEnumerator GetEnumerator()
        {
            IEnumerator[] elist = new IEnumerator[collections.Count];
            for (int i = 0; i < collections.Count; i++)
            {
                elist[i] = collections[i].GetEnumerator();
            }
            return new EnumeratorJoinerBase(rwLock, elist);
        }
    }
}
