using System.Collections.Generic;

namespace TribeSim
{
    class MemesSet
    {
        public double MemoryPrice{ get; private set; }

        public List<Meme> memes = new List<Meme>();
        public int[] memesByFeature = new int[WorldProperties.FEATURES_COUNT];
        public double[] memesEffect = new double[WorldProperties.FEATURES_COUNT];

        public bool Add(Meme meme)
        {
            // memes у нас теперь будет отсортирован по возрастанию прайса мемов
            if (memes.FindIndexIntoSortedList(meme, out var index))
                return false;
            memes.Insert(index, meme);
            MemoryPrice += meme.Price;
            int feature = (int)meme.AffectedFeature;
            memesByFeature[feature]++;
            CalculateEffect(feature);
            return true;
        }
        public bool Remove(Meme meme)
        {
            if (!memes.FindIndexIntoSortedList(meme, out var index))
                return false;
            memes.RemoveAt(index);
            MemoryPrice -= meme.Price;
            int feature = (int)meme.AffectedFeature;
            memesByFeature[feature]--;
            CalculateEffect(feature);
            return true;
        }
        public void Clear() {
            memes.Clear();
            MemoryPrice = 0;
            for (int i = 0; i < memesByFeature.Length; i++)
                memesEffect[i] = memesByFeature[i] = 0;
        }

        private void CalculateEffect(int feature)
        {
            var description = WorldProperties.FeatureDescriptions[feature];
            double retval = 0;
            foreach (Meme meme in memes)
                if ((int)meme.AffectedFeature == feature)
                    retval = description.Aggregate(retval, meme.Efficiency);
            memesEffect[feature] = retval;
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