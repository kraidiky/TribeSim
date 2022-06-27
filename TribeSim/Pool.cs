using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TribeSim
{
    public class Pool<T> where T : class, new()
    {
        private static Pool<T> _instance;
        public static Pool<T> Instance => _instance ?? (_instance = new Pool<T>());

        private ConcurrentQueue<T> queue = new ConcurrentQueue<T>();
        private Action<T> _clearMethod;
        private int _maxCount;

        public void SetSettings(int maxCount, Action<T> clearMethod)
        {
            _maxCount = maxCount;
            _clearMethod = clearMethod;
        }

        public T Get()
        {
            if (queue.TryDequeue(out var t))
                return t;
            return new T();
        }

        public void Release(T t)
        {
            _clearMethod?.Invoke(t);
            if (_maxCount > 0 && queue.Count < _maxCount)
                queue.Enqueue(t);
        }
    }

    public class PoolNew<T> where T : class
    {
        private ConcurrentQueue<T> queue = new ConcurrentQueue<T>();
        private Action<T> _clearMethod;
        private Func<T> _createMethod;
        private int _maxCount;

        public void SetSettings(int maxCount, Action<T> clearMethod, Func<T> createMethod)
        {
            _maxCount = maxCount;
            _clearMethod = clearMethod;
            _createMethod = createMethod;
        }

        public T Get()
        {
            if (queue.TryDequeue(out var t))
                return t;
            return _createMethod();
        }

        public void Release(T t)
        {
            _clearMethod?.Invoke(t);
            if (_maxCount > 0 && queue.Count < _maxCount)
                queue.Enqueue(t);
        }
    }

    public static class StringBuilderPool
    {
        private readonly static Pool<StringBuilder> _pool = new Pool<StringBuilder>();

        static StringBuilderPool()
        {
            _pool.SetSettings(1024, sb => sb.Clear());
        }

        public static StringBuilder Get() => _pool.Get();

        public static string Release(this StringBuilder t)
        {
            string str = t.ToString();
            _pool.Release(t);
            return str;
        }
    }

    public static class FeaturesFloatArray
    {
        private readonly static PoolNew<float[]> _pool = new PoolNew<float[]>();

        static FeaturesFloatArray() {
            _pool.SetSettings(
                20*1024,
                arr => Array.Clear(arr, 0, Features.Length),
                () => new float[Features.Length]);
        }
        public static float[] Get() => _pool.Get();
        public static void ReleaseFeatures(this float[] arr)
        {
            if (arr.Length != Features.Length)
                throw new ArgumentException($"Wrong Size! Must be {Features.Length}");
            _pool.Release(arr);
        }
    }
    public static class FeaturesDoubleArray
    {
        private readonly static PoolNew<double[]> _pool = new PoolNew<double[]>();

        static FeaturesDoubleArray()
        {
            _pool.SetSettings(
                20*1024,
                arr => Array.Clear(arr, 0, Features.Length),
                () => new double[Features.Length]);
        }
        public static double[] Get() => _pool.Get();
        public static void ReleaseFeatures(this double[] arr)
        {
            if (arr.Length != Features.Length)
                throw new ArgumentException($"Wrong Size! Must be {Features.Length}");
            _pool.Release(arr);
        }
    }
}
