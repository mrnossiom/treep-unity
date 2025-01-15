using System.Collections.Generic;
using UnityEngine;

// See https://discussions.unity.com/t/exposing-dictionaries-in-the-inspector/887570

namespace Treep.Utils {
    /// <summary>
    /// A dictionary that can be serialized in Unity's Inspector
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    [System.Serializable]
    public class PseudoDictionary<TKey, TValue> {
        // PSEUDODICTIONARY ENTRIES
        // & DICTIONARY CONVERSION

        [SerializeField] List<PseudoKeyValuePair<TKey, TValue>> entries;
        private Dictionary<TKey, TValue> _actualDictionary = new();

        // COUNT

        public int Count {
            get {
                _actualDictionary = FromPseudoDictionaryToActualDictionary();
                return _actualDictionary.Count;
            }
        }

        // INDEXER

        public TValue this[TKey index] {
            get {
                _actualDictionary = FromPseudoDictionaryToActualDictionary();
                return _actualDictionary[index];
            }
        }

        // FROM DICTIONARY TO PSEUDO

        public List<PseudoKeyValuePair<TKey, TValue>>
            FromActualDictionaryToPseudoDictionary(Dictionary<TKey, TValue> actualDictionary) {
            List<PseudoKeyValuePair<TKey, TValue>> pseudoDictionary = new();

            foreach (KeyValuePair<TKey, TValue> pair in actualDictionary)
                pseudoDictionary.Add(new(pair.Key, pair.Value));

            return pseudoDictionary;
        }

        public List<PseudoKeyValuePair<TKey, TValue>> FromActualDictionaryToPseudoDictionary()
            => FromActualDictionaryToPseudoDictionary(_actualDictionary);

        // FROM PSEUDO TO DICTIONARY

        public Dictionary<TKey, TValue>
            FromPseudoDictionaryToActualDictionary(List<PseudoKeyValuePair<TKey, TValue>> pseudoDictionary) {
            Dictionary<TKey, TValue> dictionary = new();

            foreach (PseudoKeyValuePair<TKey, TValue> entry in pseudoDictionary)
                dictionary.Add(entry.Key, entry.Value);

            return dictionary;
        }

        public Dictionary<TKey, TValue> FromPseudoDictionaryToActualDictionary()
            => FromPseudoDictionaryToActualDictionary(entries);

        // OPERATIONS

        public void Add(TKey key, TValue value) {
            _actualDictionary = FromPseudoDictionaryToActualDictionary();
            _actualDictionary.Add(key, value);
            entries = FromActualDictionaryToPseudoDictionary();
        }

        public void Remove(TKey key) {
            _actualDictionary = FromPseudoDictionaryToActualDictionary();
            _actualDictionary.Remove(key);
            entries = FromActualDictionaryToPseudoDictionary();
        }

        public void Clear() {
            _actualDictionary.Clear();
            entries = new();
        }

        public TValue TryGetValue(TKey key) {
            _actualDictionary = FromPseudoDictionaryToActualDictionary();
            _actualDictionary.TryGetValue(key, out var value);
            return value;
        }
    }

    [System.Serializable]
    public struct PseudoKeyValuePair<T, U> {
        [SerializeField] public T key;
        [SerializeField] private U value;

        public T Key => key;
        public U Value => value;

        public PseudoKeyValuePair(T key, U value) {
            this.key = key;
            this.value = value;
        }
    }
}