using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TribeSim
{
    class TribesmanToMemeAssociation
    {
        private static HashSet<WeakReference<TribesmanToMemeAssociation>> weakRefernceList = new HashSet<WeakReference<TribesmanToMemeAssociation>>();
        private static readonly object tribesmanToMemeAssociationLocker = new object();
        private TribesmanToMemeAssociation(){}
        public static TribesmanToMemeAssociation Create(Tribesman man, Meme meme)
        {
            TribesmanToMemeAssociation assoc = new TribesmanToMemeAssociation();
            assoc.meme = meme;
            assoc.tribesman = man;
            assoc.weakRefernenceToThis = new WeakReference<TribesmanToMemeAssociation>(assoc);
            lock (tribesmanToMemeAssociationLocker)
            {
                weakRefernceList.Add(assoc.weakRefernenceToThis);
            }
            return assoc;
        }

        public static void EndOfTurn()
        {
            foreach (WeakReference<TribesmanToMemeAssociation> assocWr in weakRefernceList.ToList())
            {
                TribesmanToMemeAssociation assoc = null;
                if (assocWr!=null && assocWr.TryGetTarget(out assoc))
                {
                    assoc.turnsSinceLastUsed++;
                }
            }
        }

        private WeakReference<TribesmanToMemeAssociation> weakRefernenceToThis;
        private Tribesman tribesman;
        private Meme meme;
        private int turnsSinceLastUsed = 0;

        public int TurnsSinceLastUsed { get { return turnsSinceLastUsed; } }
        public void Use() { turnsSinceLastUsed = 0; }
        public Meme Meme { get { return meme; } }
        public Tribesman Tribesman { get { return tribesman; } }

        public static implicit operator Meme(TribesmanToMemeAssociation a) // Фича, конечно, весёлая, но по читаемости кода шибает кувалдой. А ещё маскирует от программиста лишние действия, так что лучше не делать такого если не пишешь крутую библиотеку.
        {
            return a.meme;
        }

        ~TribesmanToMemeAssociation()
        {
            lock (tribesmanToMemeAssociationLocker)
            {
                weakRefernceList.Remove(weakRefernenceToThis);
            }
        }
    }
}
