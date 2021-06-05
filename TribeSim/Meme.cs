using DocumentFormat.OpenXml.CustomProperties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TribeSim
{
    class Meme
    {
        // Static members
        private static int numInstances = 0;
        private static long maxMemeId = 0;
        private static Object staticLocker = new Object();


        private StringBuilder storyline = new StringBuilder();
        private int knownBy = 0;

        private int maxKnowBy = 0;
        private int maxKnownByYear = 0;
        private int yearInvented = 0;
        private double efficiency;
        private double price;
        private double complexityCoefficient;
        private bool keepDiary = false;

        private string actionDescription = "";

        public double ComplexityCoefficient
        {            
            get { return complexityCoefficient; }
        }


        private long memeId = -1;


        public long MemeId
        {
            get { if (memeId == -1) memeId = maxMemeId++; return memeId; }
        }

        public readonly AvailableFeatures AffectedFeature;

        public double Price
        {
            get { return price; }
        }

        public double Efficiency
        {
            get { return efficiency; }
        }

        public string ActionDescription => actionDescription ?? (actionDescription = NamesGenerator.GenerateActionDescription(AffectedFeature, AffectedFeature == AvailableFeatures.AgeingRate ? efficiency > 0 : efficiency < 0));

        private Meme(AvailableFeatures affectedFeature) {
            this.AffectedFeature = affectedFeature;
        }

        public static Meme InventNewMeme(Random randomizer, AvailableFeatures memeAffectedFeature, List<Meme> memesAlreadyKnown = null)
        {
            Meme meme = new Meme(memeAffectedFeature);
            meme.yearInvented = World.Year;
            FeatureDescription featureDescription = WorldProperties.FeatureDescriptions[(int)memeAffectedFeature];

            meme.keepDiary = randomizer.Chance(WorldProperties.ChancesThatMemeWillWriteALog);
            meme.efficiency = -1;
            meme.price = -1;
            if (featureDescription.MemeEfficiencyMean > 1 && (featureDescription.range == FeatureRange.ZeroToOne || featureDescription.range == FeatureRange.MinusOneToPlusOne)) {
                meme.efficiency = 1;
            } else if (featureDescription.MemeEfficiencyMean <= -1 && featureDescription.range == FeatureRange.MinusOneToPlusOne) {
                meme.efficiency = -1;
            } else
                do {
                    meme.efficiency = randomizer.NormalRandom(featureDescription.MemeEfficiencyMean, featureDescription.MemeEfficiencyStdDev);
                }
                while (meme.efficiency < featureDescription.LowerRange || (meme.efficiency > featureDescription.UpperRange)); // А что, все мымы теперь 0-1? Может у нас специальный мем снижающий какое-то качество, почему бы и нет, собственно? Пусть отбор решает. Заодно посмотрим что он там нарешать сможет.

            meme.complexityCoefficient = Math.Pow(2, (meme.efficiency - featureDescription.MemeEfficiencyMean) / featureDescription.MemeEfficiencyStdDev); // Зачем вообще complexityCoefficient считать внутри цикла?

            while (meme.price < 0)
            {
                var pedestalPrice = featureDescription.MemePricePedestal;
                var efficiencyPrice = Math.Abs(meme.efficiency) * featureDescription.MemePriceEfficiencyRatio;
                var randomPricePart = randomizer.NormalRandom(featureDescription.MemePriceRandomMean, featureDescription.MemePriceRandomStdDev);
                var complexityMultiplier = Math.Pow(meme.ComplexityCoefficient, WorldProperties.MemeCostComplexityPriceCoefficient);
                meme.price = (pedestalPrice + efficiencyPrice + randomPricePart) * complexityMultiplier;
            }

            meme.Report(string.Format("Meme teaches how {3}. It affects: {0}; Efficiency: {1:f5}; It takes {2:f2} memory.", memeAffectedFeature.GetDescription(), meme.Efficiency, meme.Price, meme.ActionDescription)); // Логер, конечно, можно не переделывать, потому что мемы создаются шибко редко.

            return meme;
        }

        public static void ClearMemePool()
        {
            numInstances = 0;
            maxMemeId = 0;
        }

        public static int CountLiveMemes()
        {
            return numInstances;
        }

        public void ReportForgotten(Tribesman tribesman)
        {
            Report("Was forgotten by " + tribesman.Name);
            lock (staticLocker)
            {
                knownBy--;
            }
            if (knownBy == 0)
            {
                lock (staticLocker)
                {
                    numInstances--;
                }
            }
            if (knownBy < 0)
            {
                //throw new IndexOutOfRangeException("The meme is known by a negative amount of tribesmen.");
            }
        }

        public void ReportInvented(Tribesman tribesman)
        {
            Report("Was invented by " + tribesman.Name);
            lock (staticLocker)
            {
                numInstances++;
                knownBy = 1;
                maxKnowBy = 1;
                maxKnownByYear = World.Year;
            }
        }

        public void Report(string message)
        {
            if (keepDiary)
            {
                lock (this)
                {
                    storyline.AppendFormat("Year {0}: {1}", World.Year, message);
                    storyline.AppendLine();

                    if (storyline.Length > 1024 * 512)
                    {
                        string filename = Path.Combine(World.MemesLogFolder, this.MemeId + " meme.txt");
                        if (storyline.Length > 170)
                        {
                            File.AppendAllText(filename, storyline.ToString());
                        }
                        storyline.Clear();
                    }
                }
            }
        }

        /// <summary>
        /// Generates a JSON string like
        /// {"id": 1, "AffectedFeature": "Hunting Efficiency", "Price": 20, "Effect": 0.01}
        /// </summary>
        /// <returns></returns>
        public string GetJSONString()
        {
            return $"{{\"id\": {MemeId}, \"AffectedFeature\": {AffectedFeature.ToString()}, \"Price\": {Price}, \"Effect\": {Efficiency}}}";
        }

        ~Meme()
        {
            if (keepDiary)
            {
                string filename = Path.Combine(World.MemesLogFolder, this.MemeId + " meme.txt");

                storyline.AppendLine(string.Format("Invented at {0}y. Maximum was known by {1} at {2}y." + Environment.NewLine, yearInvented, maxKnowBy, maxKnownByYear));
                if (storyline.Length > 170)
                {
                    File.AppendAllText(filename, storyline.ToString());
                }
            }
        }

        public void ReportTeaching(Tribesman student, Tribesman teacher)
        {
            if (teacher != null)
            {
                Report(string.Format("Was transferred from {0} to {1}.", teacher.Name, student.Name));
            }
            else
            {
                Report(string.Format("Was learnt by {0} by watching others use it.", student.Name));
            }
            lock (staticLocker)
            {
                knownBy++;
            }
            if (knownBy > maxKnowBy)
            {
                maxKnowBy = knownBy;
                maxKnownByYear = World.Year;
            }
        }

        public string SignatureString
        {
            get
            {
                if (AffectedFeature == AvailableFeatures.AgeingRate) {
                    return $"#{MemeId} {AffectedFeature.GetAcronym()}:{Efficiency:f6}";
                } else {
                    return $"#{MemeId} {AffectedFeature.GetAcronym()}:{Efficiency:f2}";
                }
            }
        }
    }
}
