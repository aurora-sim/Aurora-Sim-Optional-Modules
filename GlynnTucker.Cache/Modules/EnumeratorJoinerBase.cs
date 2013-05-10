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
    class EnumeratorJoinerBase : IEnumerator
    {
        private IList<IEnumerator> enumerators;

        internal const int MAX_LOCK_WAIT = 10000; // milliseconds

        // if null, the state of the enumeratorjoiner is invalid (ie it 
        // points to before the first item or after the last item).
        protected int? currentEnumerator = null;
        protected object currentObject;
        protected ReaderWriterLock rwLock;

        internal EnumeratorJoinerBase(ReaderWriterLock rwLock, params IEnumerator[] enumerators)
        {
            this.rwLock = rwLock;
            this.enumerators = new List<IEnumerator>(enumerators);
        }

        public virtual bool MoveNext()
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
                    currentObject = enumerators[(int)currentEnumerator].Current;
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

        public virtual void Reset()
        {
            currentEnumerator = null;
            currentObject = null;
        }

        public virtual object Current
        {
            get
            {
                // TODO: detect modification of the collection
                if (currentEnumerator == null)
                {
                    throw new InvalidOperationException("Current object accessed before MoveNext() was called.");
                }
                else if (currentEnumerator == -1)
                {
                    throw new InvalidOperationException("Enumerator is past the end of the collection.");
                }
                else
                {
                    return currentObject;
                }
            }
        }
    }
}
