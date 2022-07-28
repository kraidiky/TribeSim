using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace TribeSim
{
    class Meme
    {
        // Static members
        private volatile static int numInstances = 0;


        private StringBuilder storyline = new StringBuilder();
        private volatile int knownBy = 0;

        private volatile int totalKnowBy = 0;
        private int maxKnowBy = 0;
        private int maxKnownByYear = 0;
        private int yearInvented = 0;
        private double efficiency;
        private double price;
        private double complexityCoefficient;
        private bool keepDiary = false;

        private string actionDescription = null;

        public double ComplexityCoefficient
        {            
            get { return complexityCoefficient; }
        }

        public readonly int MemeId;

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

        // Заложимся что у нас будет не больше 64 фич. Если их когда-нибудь станет больше, значит мы уже получили нобелевскую премию. :))
        public const int MEMES_TYPES_MAX_COUNT = int.MaxValue >> 6;

        public static int CreateMemeId(AvailableFeatures affectedFeature, int seed) {
            if (seed <= 0)
                throw new Exception($"Incorrect seed = {seed} is to low.");
            if (seed << 6 >> 6 != seed)
                throw new Exception($"Incorrect seed = {seed} is to big.");
            return (seed << 6) + (int) affectedFeature;
        }

        public static void ParseMemeId(int memeId, out AvailableFeatures affectedFeature, out int seed) {
            affectedFeature = (AvailableFeatures) (memeId & ((1 << 7) - 1));
            seed = memeId >> 6;
        } 


        private Meme(AvailableFeatures affectedFeature, int memeId) {
            this.AffectedFeature = affectedFeature;
            MemeId = memeId;
        }

        public static Meme InventNewMeme(Random randomizer, AvailableFeatures memeAffectedFeature, List<Meme> memesAlreadyKnown = null) {
            var memesTimesMaxCount = WorldProperties.MemesTimesMaxCount > 0 ? (int) WorldProperties.MemesTimesMaxCount : MEMES_TYPES_MAX_COUNT;
            return InventNewMeme(randomizer.Next(Features.Length, memesTimesMaxCount), memeAffectedFeature, memesAlreadyKnown);
        }


        public static Meme InventNewMeme(int seed, AvailableFeatures memeAffectedFeature, List<Meme> memesAlreadyKnown = null) {
            Meme meme = new Meme(memeAffectedFeature, CreateMemeId(memeAffectedFeature, seed));
            var randomizer = new Random(meme.MemeId);
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
                    if (meme.efficiency == 0)
                        Console.WriteLine($"meme.efficiency == 0");
                }
                while (meme.efficiency < featureDescription.LowerRange || (meme.efficiency > featureDescription.UpperRange)); // А что, все мымы теперь 0-1? Может у нас специальный мем снижающий какое-то качество, почему бы и нет, собственно? Пусть отбор решает. Заодно посмотрим что он там нарешать сможет.

            meme.complexityCoefficient = Math.Pow(2, (meme.efficiency - featureDescription.MemeEfficiencyMean) / featureDescription.MemeEfficiencyStdDev); // Зачем вообще complexityCoefficient считать внутри цикла?

            while (meme.price < 0)
            {
                var efficiencyPrice = Math.Abs(meme.efficiency) * featureDescription.MemePriceEfficiencyRatio;
                var randomPricePart = randomizer.NormalRandom(featureDescription.MemePriceRandomMean, featureDescription.MemePriceRandomStdDev);
                var complexityMultiplier = Math.Pow(meme.ComplexityCoefficient, WorldProperties.MemeCostComplexityPriceCoefficient);
                meme.price = (efficiencyPrice + randomPricePart) * complexityMultiplier;
            }
            meme.price += featureDescription.MemePricePedestal;

            meme.Report(string.Format("Meme teaches how {3}. It affects: {0}; Efficiency: {1:f5}; It takes {2:f2} memory.", memeAffectedFeature.GetDescription(), meme.Efficiency, meme.Price, meme.ActionDescription)); // Логер, конечно, можно не переделывать, потому что мемы создаются шибко редко.

            return meme;
        }

        public static void ClearMemePool()
        {
            numInstances = 0;
        }

        public static int CountLiveMemes()
        {
            return numInstances;
        }

        public void ReportForgotten(Tribesman tribesman)
        {
            if (keepDiary)
                Report("Was forgotten by " + tribesman.Name);
           var stillThoseWhoKnow = Interlocked.Decrement(ref knownBy);
            if (stillThoseWhoKnow == 0)
            {
                Interlocked.Decrement(ref numInstances);
                if (WorldProperties.CollectMemesSuccess > .5f && tribesman.randomizer.Chance(WorldProperties.ChanceToCollectMemesSuccess))
                    ReportDetaliedStatistic();
            }
            if (stillThoseWhoKnow < 0)
            {
                throw new IndexOutOfRangeException("The meme is known by a negative amount of tribesmen.");
            }
        }

        public void ReportInvented(Tribesman tribesman)
        {
            if (keepDiary)
                Report("Was invented by " + tribesman.Name);
            Interlocked.Increment(ref numInstances);
            // Всё равно до выхода отсюда мем никому передаваться не будет.
            knownBy = 1;
            maxKnowBy = 1;
            totalKnowBy = 1;
            maxKnownByYear = World.Year;
        }

        public void ReportDetaliedStatistic() {
            StatisticsCollector.ReportEvent(new SucessStatistic(this, World.Year));
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

                storyline.AppendLine(string.Format("Invented at {0}y. Maximum was known by {1} at {2}y. Totaly:{3}" + Environment.NewLine, yearInvented, maxKnowBy, maxKnownByYear, totalKnowBy));
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
                if (keepDiary)
                    Report(string.Format("Was transferred from {0} to {1}.", teacher.Name, student.Name));
            }
            else
            {
                if (keepDiary)
                    Report(string.Format("Was learnt by {0} by watching others use it.", student.Name));
            }
            var stillThoseWhoKnow = Interlocked.Increment(ref knownBy);
            Interlocked.Increment(ref totalKnowBy);
            if (stillThoseWhoKnow == 1) // Это на тот экзотический случай если последний носитель мема умер, но позже этому мему кто-то научился взяв его из списка использованных.
                Interlocked.Increment(ref numInstances);
            if (stillThoseWhoKnow > maxKnowBy)
            {
                maxKnowBy = stillThoseWhoKnow;
                maxKnownByYear = World.Year;
            }
        }

        public string SignatureString => $"{AffectedFeature.GetAcronym()}:{Efficiency.ToSignificantNumbers(2)}#{MemeId}/{price.ToSignificantNumbers(2)}";
        public override string ToString() => SignatureString;

        public struct SucessStatistic : IDetaliedEvent {
            public int memeId;
            public AvailableFeatures affectedFeature;
            public double efficiency;
            public double price;
            public double complexityCoefficient;
            public int yearInvented;
            public int currentPopulation;
            public int maxPopulation;
            public int maxPopulationYear;
            public int totalPopulation;
            public int goesAwayYear;

            public SucessStatistic(Meme meme, int goesAwayYear) {
                memeId = meme.MemeId;
                affectedFeature = meme.AffectedFeature;
                efficiency = meme.efficiency;
                price = meme.price;
                complexityCoefficient = meme.complexityCoefficient;
                yearInvented = meme.yearInvented;
                currentPopulation = meme.knownBy;
                maxPopulation = meme.maxKnowBy;
                maxPopulationYear = meme.maxKnownByYear;
                totalPopulation = meme.totalKnowBy;
                this.goesAwayYear = goesAwayYear;
            }

            public string Header() {
                var sb = StringBuilderPool.Get();
                sb.Append(nameof(memeId));
                sb.Append(',').Append(nameof(affectedFeature));
                sb.Append(',').Append(nameof(efficiency));
                sb.Append(',').Append(nameof(price));
                sb.Append(',').Append(nameof(complexityCoefficient));
                sb.Append(',').Append(nameof(yearInvented));
                sb.Append(',').Append(nameof(currentPopulation));
                sb.Append(',').Append(nameof(maxPopulation));
                sb.Append(',').Append(nameof(maxPopulationYear));
                sb.Append(',').Append(nameof(totalPopulation));
                sb.Append(',').Append(nameof(goesAwayYear));
                sb.Append('\n');
                return sb.Release();
            }

            public string Data() {
                var sb = StringBuilderPool.Get();
                sb.Append(memeId);
                sb.Append(',').Append(affectedFeature.GetAcronym());
                sb.Append(',').Append(efficiency.ToString(CultureInfo.InvariantCulture));
                sb.Append(',').Append(price.ToString(CultureInfo.InvariantCulture));
                sb.Append(',').Append(complexityCoefficient.ToString(CultureInfo.InvariantCulture));
                sb.Append(',').Append(yearInvented);
                sb.Append(',').Append(currentPopulation);
                sb.Append(',').Append(maxPopulation);
                sb.Append(',').Append(maxPopulationYear);
                sb.Append(',').Append(totalPopulation);
                sb.Append(',').Append(goesAwayYear);
                sb.Append('\n');
                return sb.Release();
            }

            public void Clear() { }
        }

        public class EqualityComparer : IEqualityComparer<Meme>
        {
            public static readonly EqualityComparer Singleton;
            static EqualityComparer()
            {
                Singleton = new EqualityComparer();
            }
            
            public bool Equals(Meme x, Meme y) => (x == null && y == null) || (x != null && y != null && x.MemeId == y.MemeId);
            public int GetHashCode(Meme obj) => obj.MemeId;
        }
    }
}
