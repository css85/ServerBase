using System;
using System.Collections.Generic;

namespace SampleGame.Shared.Common
{
    public static class WeightedRandomizer
    {
        public static WeightedRandomizer<R> From<R>(Dictionary<R, long> spawnRate)
        {
            return new WeightedRandomizer<R>(spawnRate);
        }
    }

    public class WeightedRandomizer<T>
    {
        private static System.Random _random = new System.Random();
        private Dictionary<T, long> _weights;

        public WeightedRandomizer(Dictionary<T, long> weights)
        {
            _weights = new Dictionary<T, long>(weights);
//            _weights = weights;
        }

        public T TakeOne()
        {
            var sortedSpawnRate = Sort(_weights);
            long sum = 0;
            foreach (var spawn in _weights)
            {
                sum += spawn.Value;
            }

            long roll = _random.NextInt64(0, sum);

            T selected = sortedSpawnRate[sortedSpawnRate.Count - 1].Key;
            foreach (var spawn in sortedSpawnRate)
            {
                if (roll < spawn.Value)
                {
                    selected = spawn.Key;
//                    _weights[selected] = 0;
                    break;
                }
                roll -= spawn.Value;
            }

            return selected;
        }
        public List<T> TakeMulti(int num)
        {
            var list = new List<T>();
            for( int i = 0; i < num; i++)   
                list.Add(TakeOne());
            return list;
        }

        private List<KeyValuePair<T, long>> Sort(Dictionary<T, long> weights)
        {
            var list = new List<KeyValuePair<T, long>>(weights);

            list.Sort(
                delegate (KeyValuePair<T, long> firstPair,
                            KeyValuePair<T, long> nextPair)
                {
                    return firstPair.Value.CompareTo(nextPair.Value);
                }
                );

            return list;
        }
    }
}
