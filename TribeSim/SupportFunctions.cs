using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public static double SumProbabilities(params double[] args) {
            if (args.Count() == 0) return 0;
            double retval = args[0];
            for (int i = 1; i < args.Count(); i++) {
                if (args[i] >= 1) return 1;
                retval = retval + args[i] - retval * args[i];
            }
            return retval;
        }

        public static double MultilpyProbabilities(double probability, double multiplier) {
            if (probability >= 1) return 1;
            if (probability <= 0) return 0;
            return 1 - ((1 - probability) / multiplier);
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

        // Возвращаем позицию в которую вставлен элемент. Этот функционал нужен не везде, где применяется эта функция.
        // Меняем принцип сортировки мемов, теперь они отсортированы по MemeId
        public static int AddToSortedList(this List<Meme> target, Meme meme) {
            int i = target.Count - 1;
            if (i < 0 || meme.MemeId >= target[i].MemeId) {
                target.Add(meme);
                return target.Count - 1;
            } else {
                for (--i; i >= 0; --i)
                    if (meme.MemeId > target[i].MemeId) {
                        int position = i + 1;
                        target.Insert(position, meme);
                        return position;
                    }
                target.Insert(0, meme);
                return 0;
            }
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
            // Выход из цикла мог означать, что used ещё может и остались, а вот my точно кончились. Ну ил инаоборот, собственно
            for (; sourceIndex < source.Count; sourceIndex++)
                target.Add(source[sourceIndex]);
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
