using System;
using System.Collections.Generic;

namespace TribeSim
{
    class MemesSet
    {
        public double MemoryPrice{ get; private set; }

        public List<Meme> memes = new List<Meme>();
        public int[] memesByFeature = new int[Features.Length];
        public Features MemesEffect = default;

        public bool Add(Meme meme, out int index)
        {
            // memes у нас теперь будет отсортирован по возрастанию прайса мемов
            if (memes.FindIndexIntoSortedList(meme, out index))
                return false;
            memes.Insert(index, meme);
            MemoryPrice += meme.Price;
            int feature = (int)meme.AffectedFeature;
            memesByFeature[feature]++;
            return true;
        }
        public bool Remove(Meme meme, out int index)
        {
            if (!memes.FindIndexIntoSortedList(meme, out index))
                return false;
            memes.RemoveAt(index);
            MemoryPrice -= meme.Price;
            int feature = (int)meme.AffectedFeature;
            memesByFeature[feature]--;
            return true;
        }

        public bool FindIndexOf(Meme meme, out int index) => memes.FindIndexIntoSortedList(meme, out index);
        
        public void Clear() {
            memes.Clear();
            MemoryPrice = 0;
            for (int i = 0; i < memesByFeature.Length; i++)
                MemesEffect[i] = memesByFeature[i] = 0;
        }

        public void CalculateEffect(int feature)
        {
            var description = WorldProperties.FeatureDescriptions[feature];
            var aggregator = WorldProperties.FeatureDescriptions[feature].Aggregator();
            foreach (Meme meme in memes)
                if ((int)meme.AffectedFeature == feature)
                    aggregator.Append(meme.Efficiency);
            MemesEffect[feature] = aggregator.Value;
        }
        // Достать мем по указанному порядковому номеру с указанной фичей
        public Meme GetMemeByFeature(int feature, int index)
        {
            for (int i = 0; i < memes.Count; i++)
                if ((int)memes[i].AffectedFeature == feature)
                    if (0 == index--)
                        return memes[i];
            return null;
        }
    }
}