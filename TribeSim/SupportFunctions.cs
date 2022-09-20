using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace TribeSim
{
    static class SupportFunctions {
        public static double NormalRandom(this Random randomizer, double mean, double stdDev) {
            double u1 = 1.0 - randomizer.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0 - randomizer.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                         Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            double randNormal =
                         mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
            return randNormal;
        }

        public static double MultilpyProbabilities(double probability, double multiplier) {
            if (probability >= 1) return 1;
            if (probability <= 0) return 0;
            return 1 - Math.Pow(1 - probability, multiplier);
        }

        public static bool Flip(this Random randomizer) {
            return randomizer.NextDouble() < 0.5;
        }

        public static bool Chance(this Random randomizer, double chance) {
            if (chance > 0) // Потому что этот кейс очень частый, а операции NextDouble немножко тяжелая. ускоряет на пару процентов.
                return randomizer.NextDouble() < chance;
            else
                return false;
        }
        /// <summary> Функция нужна для удобства пошаговой отладки, чтобы можно было в одну строчку заменить все паралелизмы на последовательное выполнение. </summary>
        public static void Parallel<T>(this IList<T> items, Action<T> action) {
            if (WorldProperties.IgnoreMultithreading < .5)
                System.Threading.Tasks.Parallel.ForEach<T>(items, action);
            else
                foreach (var item in items) action(item);
        }

        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action) {
            foreach (var item in items)
                action(item);
        }

        public static bool FindIndexIntoSortedList(this List<Meme> target, Meme meme, out int index)
        {
            int memeId = meme.MemeId;
            if (target.Count == 0) { index = 0; return false; }
            int left = 0;
            int targetMemeId = target[left].MemeId;
            if (memeId < targetMemeId) { index = 0; return false; }
            if (memeId == targetMemeId) { index = 0; return true; }
            int right = target.Count - 1;
            targetMemeId = target[right].MemeId;
            if (memeId > targetMemeId) { index = right + 1; return false; }
            if (memeId == targetMemeId) { index = right; return true; }
            while (right - left > 1)
            {
                int center = (left + right) / 2;
                targetMemeId = target[center].MemeId;
                if (memeId == targetMemeId) { index = center; return true; }
                else if (memeId < targetMemeId) {
                    right = center;
                } else {
                    left = center;
                }
            }
            index = left + 1;
            return false;
        }

        public static void ExcludeFromSortedList(this List<Meme> source, List<Meme> excludeList, List<Meme> target) {
            target.Clear();
            int sourceIndex = 0;
            if (excludeList.Count > 0) {
                for (int excludedIndex = 0; excludedIndex < excludeList.Count && sourceIndex < source.Count;) {
                    Meme sourceMeme = source[sourceIndex], excludedMeme = excludeList[excludedIndex];
                    if (sourceMeme.MemeId == excludedMeme.MemeId) { // пропустить совпадающие элементы.
                        sourceIndex++;
                        excludedIndex++;
                        continue;
                    } else if (sourceMeme.MemeId > excludedMeme.MemeId) {
                        excludedIndex++;
                    } else if (sourceMeme.MemeId < excludedMeme.MemeId) {
                        sourceIndex++;
                        target.Add(sourceMeme);
                    } // Теперь, когда сортировка идёт по MemeId ситуация когда несколько мемов могут иметь одинаковый признак сортировки невозможна и особый сложный код для обработки этого случая уже не нужен
                }
            }
            // Выход из цикла мог означать, что used ещё может и остались, а вот my точно кончились. Ну или наоборот, собственно
            for (; sourceIndex < source.Count; sourceIndex++)
                target.Add(source[sourceIndex]);
        }
        
        public static void ExcludeFromSortedList(this IEnumerable<Meme> source, IEnumerable<Meme> excludeList, List<Meme> target)
        {
            target.Clear();
            var sourceMemes = source.GetEnumerator();
            var excludedMemes = excludeList.GetEnumerator();
            if (!sourceMemes.MoveNext())
                return;
            while(excludedMemes.MoveNext()) {
                do {
                    if (sourceMemes.Current.MemeId == excludedMemes.Current.MemeId) { // пропустить совпадающие элементы.
                        if (!sourceMemes.MoveNext())
                            return;
                        break;
                    } else if (sourceMemes.Current.MemeId > excludedMemes.Current.MemeId) {
                        break;
                    } else { // if (sourceMemes.Current.MemeId < excludedMemes.Current.MemeId)
                        target.Add(sourceMemes.Current);
                        if (!sourceMemes.MoveNext())
                            return;
                    }
                } while (true);

            }
            do {
                target.Add(sourceMemes.Current);
            } while (sourceMemes.MoveNext());
        }

        private static string[] formats = new[] { "F0", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9" };
        private static int[] multiplier = new[] { 1, 10, 100, 1000, 10000, 100000, 1000000, 10000000, 100000000, 1000000000 };
        public static string SignificantSigns(this float value, int signs) {
            if (float.IsNaN(value))
                return "NaN";
            if (value == (int)value)
                return ((int)value).ToString(CultureInfo.InvariantCulture);
            int significantSigns = value != 0 ? (int)Math.Ceiling(Math.Log10(Math.Abs(value))) : 3;
            significantSigns = (significantSigns > 0 ? 0 : -significantSigns) + signs;
            significantSigns = significantSigns < formats.Length ? significantSigns : formats.Length - 1;
            if (significantSigns < multiplier.Length && multiplier[significantSigns] * value == (int)Math.Ceiling(multiplier[significantSigns] * value))
                return value.ToString(CultureInfo.InvariantCulture);
            return value.ToString(formats[significantSigns], CultureInfo.InvariantCulture);
        }
    }

    class NullableDictionary<K, V> : IDictionary<K, V>
    {
        Dictionary<K, V> dict = new Dictionary<K, V>();
        V nullValue = default(V);
        bool hasNull = false;

        public NullableDictionary()
        {
        }

        public void Add(K key, V value)
        {
            if (key == null)
                if (hasNull)
                    throw new ArgumentException("Duplicate key");
                else
                {
                    nullValue = value;
                    hasNull = true;
                }
            else
                dict.Add(key, value);
        }

        public bool ContainsKey(K key)
        {
            if (key == null)
                return hasNull;
            return dict.ContainsKey(key);
        }

        public ICollection<K> Keys
        {
            get
            {
                if (!hasNull)
                    return dict.Keys;

                List<K> keys = dict.Keys.ToList();
                keys.Add(default(K));
                return new ReadOnlyCollection<K>(keys);
            }
        }

        public bool Remove(K key)
        {
            if (key != null)
                return dict.Remove(key);

            bool oldHasNull = hasNull;
            hasNull = false;
            return oldHasNull;
        }

        public bool TryGetValue(K key, out V value)
        {
            if (key != null)
                return dict.TryGetValue(key, out value);

            value = hasNull ? nullValue : default(V);
            return hasNull;
        }

        public ICollection<V> Values
        {
            get
            {
                if (!hasNull)
                    return dict.Values;

                List<V> values = dict.Values.ToList();
                values.Add(nullValue);
                return new ReadOnlyCollection<V>(values);
            }
        }

        public V this[K key]
        {
            get
            {
                if (key == null)
                    if (hasNull)
                        return nullValue;
                    else
                        throw new KeyNotFoundException();
                else
                    return dict[key];
            }
            set
            {
                if (key == null)
                {
                    nullValue = value;
                    hasNull = true;
                }
                else
                    dict[key] = value;
            }
        }

        public void Add(KeyValuePair<K, V> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            hasNull = false;
            dict.Clear();
        }

        public bool Contains(KeyValuePair<K, V> item)
        {
            if (item.Key != null)
                return ((ICollection<KeyValuePair<K, V>>)dict).Contains(item);
            if (hasNull)
                return EqualityComparer<V>.Default.Equals(nullValue, item.Value);
            return false;
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<K, V>>)dict).CopyTo(array, arrayIndex);
            if (hasNull)
                array[arrayIndex + dict.Count] = new KeyValuePair<K, V>(default(K), nullValue);
        }

        public int Count
        {
            get { return dict.Count + (hasNull ? 1 : 0); }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<K, V> item)
        {
            V value;
            if (TryGetValue(item.Key, out value) && EqualityComparer<V>.Default.Equals(item.Value, value))
                return Remove(item.Key);
            return false;
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            if (!hasNull)
                return dict.GetEnumerator();
            else
                return GetEnumeratorWithNull();
        }

        private IEnumerator<KeyValuePair<K, V>> GetEnumeratorWithNull()
        {
            yield return new KeyValuePair<K, V>(default(K), nullValue);
            foreach (var kv in dict)
                yield return kv;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
