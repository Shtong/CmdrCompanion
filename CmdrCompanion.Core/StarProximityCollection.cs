using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdrCompanion.Core
{
    /// <summary>
    /// A specialized dictionnary that is used to store lists of distance between a star and a set of other stars.
    /// </summary>
    /// <seealso cref="Star.KnownStarProximities"/>
    public sealed class StarProximityCollection :
        IDictionary<AstronomicalObject, float>, 
        ICollection<KeyValuePair<AstronomicalObject, float>>, 
        IDictionary, 
        ICollection, 
        IReadOnlyDictionary<AstronomicalObject, float>,
        IReadOnlyCollection<KeyValuePair<AstronomicalObject, float>>,
        IEnumerable<KeyValuePair<AstronomicalObject, float>>,
        IEnumerable,
        INotifyCollectionChanged
    {
        private Dictionary<AstronomicalObject, float> _data;
        private SortedDictionary<float, List<AstronomicalObject>> _sortedIndex;

        internal StarProximityCollection()
        {
            _data = new Dictionary<AstronomicalObject, float>();
            _sortedIndex = new SortedDictionary<float, List<AstronomicalObject>>();
        }

        #region Structures
        private class DictionaryEnumeratorWrapper<TKey, TValue> : IDictionaryEnumerator
        {
            private IEnumerator<KeyValuePair<TKey, TValue>> _innerEnumerator;

            public DictionaryEnumeratorWrapper(IEnumerator<KeyValuePair<TKey, TValue>> enumerator)
            {
                _innerEnumerator = enumerator;
            }

            public DictionaryEntry Entry
            {
                get { return new DictionaryEntry(Key, Value); }
            }

            public object Key
            {
                get { return _innerEnumerator.Current.Key; }
            }

            public object Value
            {
                get { return _innerEnumerator.Current.Value; }
            }

            public object Current
            {
                get { return _innerEnumerator.Current; }
            }

            public bool MoveNext()
            {
                return _innerEnumerator.MoveNext();
            }

            public void Reset()
            {
                _innerEnumerator.Reset();
            }
        }
        #endregion

        #region Writing
        internal void Set(AstronomicalObject star, float distance)
        {
            bool isNew = false;
            float previousDistance = 0;
            if(_data.ContainsKey(star))
            {
                if (_data[star] == distance)
                    return;

                _sortedIndex[_data[star]].Remove(star);
                previousDistance = _data[star];
                _data[star] = distance;
            }
            else
            {
                _data.Add(star, distance);
                isNew = true;
            }

            if (!_sortedIndex.ContainsKey(distance))
                _sortedIndex.Add(distance, new List<AstronomicalObject>() { star });
            else
                _sortedIndex[distance].Add(star);

            if (isNew)
                OnCollectionChangedAdd(star);
            else
                OnCollectionChangedReplace(star, previousDistance);
        }
        #endregion

        #region Collection interfaces
        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</param>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the key; otherwise, false.
        /// </returns>
        public bool ContainsKey(AstronomicalObject key)
        {
            return _data.ContainsKey(key);
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        public IEnumerable<AstronomicalObject> Keys
        {
            get 
            {
                return _sortedIndex.Values.SelectMany(v => v);
            }
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
        /// <returns>
        /// true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key; otherwise, false.
        /// </returns>
        public bool TryGetValue(AstronomicalObject key, out float value)
        {
            return _data.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        public IEnumerable<float> Values
        {
            get 
            {
                return _sortedIndex.Keys;
            }
        }

        /// <summary>
        /// Gets or sets the element with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public float this[AstronomicalObject key]
        {
            get { return _data[key]; }
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public int Count
        {
            get { return _data.Count; }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<AstronomicalObject, float>> GetEnumerator()
        {
            foreach (KeyValuePair<float, List<AstronomicalObject>> pair in _sortedIndex)
                foreach (AstronomicalObject s in pair.Value)
                    yield return new KeyValuePair<AstronomicalObject, float>(s, pair.Key);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        void IDictionary<AstronomicalObject, float>.Add(AstronomicalObject key, float value)
        {
            throw new NotSupportedException();
        }

        bool IDictionary<AstronomicalObject, float>.Remove(AstronomicalObject key)
        {
            throw new NotSupportedException();
        }

        void ICollection<KeyValuePair<AstronomicalObject, float>>.Add(KeyValuePair<AstronomicalObject, float> item)
        {
            throw new NotSupportedException();
        }

        void ICollection<KeyValuePair<AstronomicalObject, float>>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<KeyValuePair<AstronomicalObject, float>>.Contains(KeyValuePair<AstronomicalObject, float> item)
        {
            return ((ICollection<KeyValuePair<AstronomicalObject, float>>)_data).Contains(item);
        }

        void ICollection<KeyValuePair<AstronomicalObject, float>>.CopyTo(KeyValuePair<AstronomicalObject, float>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<AstronomicalObject, float>>)_data).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<AstronomicalObject, float>>.IsReadOnly
        {
            get { return true; }
        }

        bool ICollection<KeyValuePair<AstronomicalObject, float>>.Remove(KeyValuePair<AstronomicalObject, float> item)
        {
            throw new NotSupportedException();
        }



        bool IDictionary<AstronomicalObject, float>.ContainsKey(AstronomicalObject key)
        {
            return _data.ContainsKey(key);
        }

        ICollection<AstronomicalObject> IDictionary<AstronomicalObject, float>.Keys
        {
            get { return _sortedIndex.Values.SelectMany(v => v).ToList(); }
        }

        bool IDictionary<AstronomicalObject, float>.TryGetValue(AstronomicalObject key, out float value)
        {
            return _data.TryGetValue(key, out value);
        }

        ICollection<float> IDictionary<AstronomicalObject, float>.Values
        {
            get { return _sortedIndex.Keys; }
        }

        float IDictionary<AstronomicalObject, float>.this[AstronomicalObject key]
        {
            get
            {
                return _data[key];
            }
            set
            {
                throw new NotSupportedException();
            }
        }


        int ICollection<KeyValuePair<AstronomicalObject, float>>.Count
        {
            get { return _data.Count; }
        }

        IEnumerator<KeyValuePair<AstronomicalObject, float>> IEnumerable<KeyValuePair<AstronomicalObject, float>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        void IDictionary.Add(object key, object value)
        {
            throw new NotSupportedException();
        }

        void IDictionary.Clear()
        {
            throw new NotSupportedException();
        }

        bool IDictionary.Contains(object key)
        {
            if (key is KeyValuePair<AstronomicalObject, float>)
                return _data.Contains((KeyValuePair<AstronomicalObject, float>)key);
            else
                return false;
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new DictionaryEnumeratorWrapper<AstronomicalObject, float>(GetEnumerator());
        }

        bool IDictionary.IsFixedSize
        {
            get { return false; }
        }

        bool IDictionary.IsReadOnly
        {
            get { return true; }
        }

        ICollection IDictionary.Keys
        {
            get { return _sortedIndex.Values; }
        }

        void IDictionary.Remove(object key)
        {
            throw new NotSupportedException();
        }

        ICollection IDictionary.Values
        {
            get { return _sortedIndex.Keys; }
        }

        object IDictionary.this[object key]
        {
            get
            {
                if (key is AstronomicalObject)
                    return _data[(AstronomicalObject)key];
                else
                    return null;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)_data).CopyTo(array, index);
        }

        int ICollection.Count
        {
            get { return _data.Count; }
        }

        bool ICollection.IsSynchronized
        {
            get { return ((ICollection)_data).IsSynchronized; }
        }

        object ICollection.SyncRoot
        {
            get { return ((ICollection)_data).SyncRoot; }
        }
        #endregion

        #region INotifyCollectionChanged

        private void OnCollectionChangedAdd(AstronomicalObject newStar)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _data[newStar]));
        }

        private void OnCollectionChangedReplace(AstronomicalObject changedStar, float previousDistance)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, (object)_data[changedStar], (object)new KeyValuePair<AstronomicalObject, float>(changedStar, previousDistance)));
        }

        /// <summary>
        /// Occurs when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        #endregion
    }
}
