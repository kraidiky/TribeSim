using System;
using System.Collections.Generic;

namespace TribeSim
{
    class MemesSetWithCounting
    {
        public MemesSet memesSet = new MemesSet();
        public List<int> counts = new List<int>();

        public void Add(Meme meme) {
            if (memesSet.Add(meme, out var index)) {
                counts.Insert(index, 1);
            } else {
                counts[index] += 1;
            }
        }

        public void Remove(Meme meme) {
            if (memesSet.FindIndexOf(meme, out var index)) {
                var count = counts[index] - 1;
                if (count > 0) {
                    counts[index] = count;
                } else {
                    counts.RemoveAt(index);
                    memesSet.Remove(meme, out _);
                }
            }
            else
                throw new Exception("Wrong MemesSetWithCounting Remove usage.");
        }

        public void Add(IEnumerable<Meme> memes) {
            foreach (var meme in memes)
                Add(meme);
        }
        public void Remove(IEnumerable<Meme> memes) {
            foreach (var meme in memes)
                Remove(meme);
        }
    }
}