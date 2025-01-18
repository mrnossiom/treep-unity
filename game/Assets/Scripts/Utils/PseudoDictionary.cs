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
        [SerializeField] private List<PseudoKeyValuePair<TKey, TValue>> entries;
        private Dictionary<TKey, TValue> _actualDictionary = new();

        public int Count {
            get {
                _actualDictionary = ToActualDictionary();
                return _actualDictionary.Count;
            }
        }

        public TValue this[TKey index] {
            get {
                _actualDictionary = ToActualDictionary();
                return _actualDictionary[index];
            }
        }

        public List<PseudoKeyValuePair<TKey, TValue>>
            FromActualDictionary(Dictionary<TKey, TValue> actualDictionary) {
            List<PseudoKeyValuePair<TKey, TValue>> pseudoDictionary = new();

            foreach (var pair in actualDictionary)
                pseudoDictionary.Add(new(pair.Key, pair.Value));

            return pseudoDictionary;
        }

        public List<PseudoKeyValuePair<TKey, TValue>> FromActualDictionary()
            => FromActualDictionary(_actualDictionary);

        // FROM PSEUDO TO DICTIONARY

        public Dictionary<TKey, TValue>
            ToActualDictionary() {
            Dictionary<TKey, TValue> dictionary = new();

            foreach (var entry in entries)
                dictionary.Add(entry.Key, entry.Value);

            return dictionary;
        }

        // OPERATIONS

        public void Add(TKey key, TValue value) {
            _actualDictionary = ToActualDictionary();
            _actualDictionary.Add(key, value);
            entries = FromActualDictionary();
        }

        public void Remove(TKey key) {
            _actualDictionary = ToActualDictionary();
            _actualDictionary.Remove(key);
            entries = FromActualDictionary();
        }

        public void Clear() {
            _actualDictionary.Clear();
            entries = new();
        }

        public TValue TryGetValue(TKey key) {
            _actualDictionary = ToActualDictionary();
            _actualDictionary.TryGetValue(key, out var value);
            return value;
        }
    }

    [System.Serializable]
    public struct PseudoKeyValuePair<TKey, TValue> {
        [SerializeField] public TKey key;
        [SerializeField] private TValue value;

        public TKey Key => key;
        public TValue Value => value;

        public PseudoKeyValuePair(TKey key, TValue value) {
            this.key = key;
            this.value = value;
        }
    }
}
