using System.Collections.Generic;

namespace VT0
{

    public class TwoWayDictionary<T1, T2>
    {
        private readonly Dictionary<T1, T2> _a = new Dictionary<T1, T2>();
        private readonly Dictionary<T2, T1> _b = new Dictionary<T2, T1>();

        public void Add(T1 a, T2 b)
        {
            _a.Add(a, b);
            _b.Add(b, a);
        }

        public bool Remove(T1 a)
        {
            T2 b;
            if (_a.TryGetValue(a, out b))
            {
                _a.Remove(a);
                _b.Remove(b);
                return true;
            }
            return false;
        }

        public bool TryGetValue1(T1 key, out T2 value)
        {
            return _a.TryGetValue(key, out value);
        }

        public bool TryGetValue2(T2 key, out T1 value)
        {
            return _b.TryGetValue(key, out value);
        }
    }
}
