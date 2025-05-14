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
                this._actualDictionary = this.ToActualDictionary();
                return this._actualDictionary.Count;
            }
        }

        public TValue this[TKey index] {
            get {
                this._actualDictionary = this.ToActualDictionary();
                return this._actualDictionary[index];
            }
        }

        public List<PseudoKeyValuePair<TKey, TValue>>
            FromActualDictionary(Dictionary<TKey, TValue> actualDictionary) {
            List<PseudoKeyValuePair<TKey, TValue>> pseudoDictionary = new();

            foreach (var pair in actualDictionary) {
                pseudoDictionary.Add(new PseudoKeyValuePair<TKey, TValue>(pair.Key, pair.Value));
            }

            return pseudoDictionary;
        }

        public List<PseudoKeyValuePair<TKey, TValue>> FromActualDictionary() {
            return this.FromActualDictionary(this._actualDictionary);
        }

        // FROM PSEUDO TO DICTIONARY

        public Dictionary<TKey, TValue>
            ToActualDictionary() {
            Dictionary<TKey, TValue> dictionary = new();

            foreach (var entry in this.entries) {
                dictionary.Add(entry.Key, entry.Value);
            }

            return dictionary;
        }

        // OPERATIONS

        public void Add(TKey key, TValue value) {
            this._actualDictionary = this.ToActualDictionary();
            this._actualDictionary.Add(key, value);
            this.entries = this.FromActualDictionary();
        }

        public void Remove(TKey key) {
            this._actualDictionary = this.ToActualDictionary();
            this._actualDictionary.Remove(key);
            this.entries = this.FromActualDictionary();
        }

        public void Clear() {
            this._actualDictionary.Clear();
            this.entries = new List<PseudoKeyValuePair<TKey, TValue>>();
        }

        public TValue TryGetValue(TKey key) {
            this._actualDictionary = this.ToActualDictionary();
            this._actualDictionary.TryGetValue(key, out var value);
            return value;
        }
    }

    [System.Serializable]
    public struct PseudoKeyValuePair<TKey, TValue> {
        [SerializeField] public TKey key;
        [SerializeField] private TValue value;

        public TKey Key => this.key;
        public TValue Value => this.value;

        public PseudoKeyValuePair(TKey key, TValue value) {
            this.key = key;
            this.value = value;
        }
    }
}
