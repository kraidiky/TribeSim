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
        private AvailableFeatures? affectedFeature;
        private int knownBy = 0;

        private int maxKnowBy = 0;
        private int maxKnownByYear = 0;
        private int yearInvented = 0;
        private double efficiency;
        private double price;
        private double complexityCoefficient;
        private bool keepDiary = false;

        private string actionDescription = "";

        private List<Meme> prequisiteMemes = new List<Meme>();

        public bool PrequisitesAreMet(List<Meme> knownMemes)
        {
            foreach (Meme requiredMeme in prequisiteMemes)
            {
                if (!knownMemes.Contains(requiredMeme)) return false;
            }
            return true;
        }

        public List<Meme> WhichPrequisitesAreNotMet(List<Meme> knownMemes)
        {
            List<Meme> retVal = new List<Meme>();
            foreach (Meme requiredMeme in prequisiteMemes)
            {
                if (!knownMemes.Contains(requiredMeme))
                {
                    retVal.Add(requiredMeme);
                }
            }
            return retVal;
        }

        public double ComplexityCoefficient
        {            
            get { return complexityCoefficient; }
        }


        private long memeId = -1;


        public long MemeId
        {
            get { if (memeId == -1) memeId = maxMemeId++; return memeId; }
        }

        public AvailableFeatures? AffectedFeature
        {
            get { return affectedFeature; }
        }

        public double Price
        {
            get { return price; }
        }

        public double Efficiency
        {
            get { return efficiency; }
        }

        public string ActionDescription {
            get
            {
                if (string.IsNullOrWhiteSpace(actionDescription))
                {
                    if (prequisiteMemes.Count != 1)
                    {
                        if (this.AffectedFeature.HasValue && this.efficiency < 0)
                        {
                            actionDescription = NamesGenerator.GenerateActionDescription(this.AffectedFeature, true);
                        }
                        else
                        {
                            actionDescription = NamesGenerator.GenerateActionDescription(this.AffectedFeature, false);
                        }
                    }
                    else if (prequisiteMemes.Count == 1)
                    {
                        string oldDescription = prequisiteMemes[0].ActionDescription;
                        if (oldDescription.Contains("even better")) { actionDescription = oldDescription.Replace("even better", "much better"); }
                        else if (oldDescription.Contains("significantly better")) { actionDescription = oldDescription.Replace("significantly better", "even better"); }
                        else if (oldDescription.Contains("slightly better")) { actionDescription = oldDescription.Replace("slightly better", "significantly better"); }
                        else { actionDescription = oldDescription + " slightly better"; }
                    }
                }
                return actionDescription;
            }
        }

        private Meme() { }

        public static Meme InventNewMeme(Random randomizer, AvailableFeatures? memeAffectedFeature, List<Meme> memesAlreadyKnown = null)
        {
            Meme meme = new Meme();
            meme.yearInvented = World.Year;
            double thisMemeMean = 0;
            double thisMemeStdDev = 0;
            double thisMemePricePedestal = 0;
            double thisMemePriceEfficiencyRatio = 0;
            double thisMemePriceRandomMean = 0;
            double thisMemePriceRandomStdDev = 0;
            meme.keepDiary = randomizer.Chance(WorldProperties.ChancesThatMemeWillWriteALog);
            meme.affectedFeature = memeAffectedFeature;
            if (memeAffectedFeature.HasValue)
            {
                switch (memeAffectedFeature.Value)
                {
                    case AvailableFeatures.TrickLikelyhood:
                        thisMemeMean = WorldProperties.NewMemeTrickLikelyhoodMean;
                        thisMemeStdDev = WorldProperties.NewMemeTrickLikelyhoodStdDev;
                        thisMemePricePedestal = WorldProperties.MemeCostPedestalTrickLikelyhood;
                        thisMemePriceEfficiencyRatio = WorldProperties.MemeCostEfficiencyRatioTrickLikelyhood;
                        thisMemePriceRandomMean = WorldProperties.MemeCostRandomAverageTrickLikelyhood;
                        thisMemePriceRandomStdDev = WorldProperties.MemeCostRandomStdDevTrickLikelyhood;
                        break;
                    case AvailableFeatures.TrickEfficiency:
                        thisMemeMean = WorldProperties.NewMemeTrickEfficiencyMean;
                        thisMemeStdDev = WorldProperties.NewMemeTrickEfficiencyStdDev;
                        thisMemePricePedestal = WorldProperties.MemeCostPedestalTrickEfficiency;
                        thisMemePriceEfficiencyRatio = WorldProperties.MemeCostEfficiencyRatioTrickEfficiency;
                        thisMemePriceRandomMean = WorldProperties.MemeCostRandomAverageTrickEfficiency;
                        thisMemePriceRandomStdDev = WorldProperties.MemeCostRandomStdDevTrickEfficiency;
                        break;
                    case AvailableFeatures.TeachingLikelyhood:
                        thisMemeMean = WorldProperties.NewMemeTeachingLikelyhoodMean;
                        thisMemeStdDev = WorldProperties.NewMemeTeachingLikelyhoodStdDev;
                        thisMemePricePedestal = WorldProperties.MemeCostPedestalTeachingLikelyhood;
                        thisMemePriceEfficiencyRatio = WorldProperties.MemeCostEfficiencyRatioTeachingLikelyhood;
                        thisMemePriceRandomMean = WorldProperties.MemeCostRandomAverageTeachingLikelyhood;
                        thisMemePriceRandomStdDev = WorldProperties.MemeCostRandomStdDevTeachingLikelyhood;
                        break;
                    case AvailableFeatures.TeachingEfficiency:
                        thisMemeMean = WorldProperties.NewMemeTeachingEfficiencyMean;
                        thisMemeStdDev = WorldProperties.NewMemeTeachingEfficiencyStdDev;
                        thisMemePricePedestal = WorldProperties.MemeCostPedestalTeachingEfficiency;
                        thisMemePriceEfficiencyRatio = WorldProperties.MemeCostEfficiencyRatioTeachingEfficiency;
                        thisMemePriceRandomMean = WorldProperties.MemeCostRandomAverageTeachingEfficiency;
                        thisMemePriceRandomStdDev = WorldProperties.MemeCostRandomStdDevTeachingEfficiency;
                        break;
                    case AvailableFeatures.StudyLikelyhood:
                        thisMemeMean = WorldProperties.NewMemeStudyLikelyhoodMean;
                        thisMemeStdDev = WorldProperties.NewMemeStudyLikelyhoodStdDev;
                        thisMemePricePedestal = WorldProperties.MemeCostPedestalStudyLikelyhood;
                        thisMemePriceEfficiencyRatio = WorldProperties.MemeCostEfficiencyRatioStudyLikelyhood;
                        thisMemePriceRandomMean = WorldProperties.MemeCostRandomAverageStudyLikelyhood;
                        thisMemePriceRandomStdDev = WorldProperties.MemeCostRandomStdDevStudyLikelyhood;
                        break;
                    case AvailableFeatures.StudyEfficiency:
                        thisMemeMean = WorldProperties.NewMemeStudyEfficiencyMean;
                        thisMemeStdDev = WorldProperties.NewMemeStudyEfficiencyStdDev;
                        thisMemePricePedestal = WorldProperties.MemeCostPedestalStudyEfficiency;
                        thisMemePriceEfficiencyRatio = WorldProperties.MemeCostEfficiencyRatioStudyEfficiency;
                        thisMemePriceRandomMean = WorldProperties.MemeCostRandomAverageStudyEfficiency;
                        thisMemePriceRandomStdDev = WorldProperties.MemeCostRandomStdDevStudyEfficiency;
                        break;
                    case AvailableFeatures.FreeRiderPunishmentLikelyhood:
                        thisMemeMean = WorldProperties.NewMemeFreeRiderPunishmentLikelyhoodMean;
                        thisMemeStdDev = WorldProperties.NewMemeFreeRiderPunishmentLikelyhoodStdDev;
                        thisMemePricePedestal = WorldProperties.MemeCostPedestalFreeRiderPunishmentLikelyhood;
                        thisMemePriceEfficiencyRatio = WorldProperties.MemeCostEfficiencyRatioFreeRiderPunishmentLikelyhood;
                        thisMemePriceRandomMean = WorldProperties.MemeCostRandomAverageFreeRiderPunishmentLikelyhood;
                        thisMemePriceRandomStdDev = WorldProperties.MemeCostRandomStdDevFreeRiderPunishmentLikelyhood;
                        break;
                    case AvailableFeatures.FreeRiderDeterminationEfficiency:
                        thisMemeMean = WorldProperties.NewMemeFreeRiderDeterminationEfficiencyMean;
                        thisMemeStdDev = WorldProperties.NewMemeFreeRiderDeterminationEfficiencyStdDev;
                        thisMemePricePedestal = WorldProperties.MemeCostPedestalFreeRiderDeterminationEfficiency;
                        thisMemePriceEfficiencyRatio = WorldProperties.MemeCostEfficiencyRatioFreeRiderDeterminationEfficiency;
                        thisMemePriceRandomMean = WorldProperties.MemeCostRandomAverageFreeRiderDeterminationEfficiency;
                        thisMemePriceRandomStdDev = WorldProperties.MemeCostRandomStdDevFreeRiderDeterminationEfficiency;
                        break;
                    case AvailableFeatures.CooperationEfficiency:
                        thisMemeMean = WorldProperties.NewMemeCooperationEfficiencyMean;
                        thisMemeStdDev = WorldProperties.NewMemeCooperationEfficiencyStdDev;
                        thisMemePricePedestal = WorldProperties.MemeCostPedestalCooperationEfficiency;
                        thisMemePriceEfficiencyRatio = WorldProperties.MemeCostEfficiencyRatioCooperationEfficiency;
                        thisMemePriceRandomMean = WorldProperties.MemeCostRandomAverageCooperationEfficiency;
                        thisMemePriceRandomStdDev = WorldProperties.MemeCostRandomStdDevCooperationEfficiency;
                        break;
                    case AvailableFeatures.HuntingEfficiency:
                        thisMemeMean = WorldProperties.NewMemeHuntingEfficiencyMean;
                        thisMemeStdDev = WorldProperties.NewMemeHuntingEfficiencyStdDev;
                        thisMemePricePedestal = WorldProperties.MemeCostPedestalHuntingEfficiency;
                        thisMemePriceEfficiencyRatio = WorldProperties.MemeCostEfficiencyRatioHuntingEfficiency;
                        thisMemePriceRandomMean = WorldProperties.MemeCostRandomAverageHuntingEfficiency;
                        thisMemePriceRandomStdDev = WorldProperties.MemeCostRandomStdDevHuntingEfficiency;
                        break;
                    case AvailableFeatures.LikelyhoodOfNotBeingAFreeRider:
                        thisMemeMean = WorldProperties.NewMemeLikelyhoodOfNotBeingAFreeRiderMean;
                        thisMemeStdDev = WorldProperties.NewMemeLikelyhoodOfNotBeingAFreeRiderStdDev;
                        thisMemePricePedestal = WorldProperties.MemeCostPedestalLikelyhoodOfNotBeingAFreeRider;
                        thisMemePriceEfficiencyRatio = WorldProperties.MemeCostEfficiencyRatioLikelyhoodOfNotBeingAFreeRider;
                        thisMemePriceRandomMean = WorldProperties.MemeCostRandomAverageLikelyhoodOfNotBeingAFreeRider;
                        thisMemePriceRandomStdDev = WorldProperties.MemeCostRandomStdDevLikelyhoodOfNotBeingAFreeRider;
                        break;
                }
            }
            else
            {
                thisMemeMean = WorldProperties.NewMemeUselessEfficiencyMean;
                thisMemeStdDev = WorldProperties.NewMemeUselessEfficiencyStdDev;
                thisMemePricePedestal = WorldProperties.MemeCostPedestalUseless;
                thisMemePriceEfficiencyRatio = WorldProperties.MemeCostEfficiencyRatioUseless;
                thisMemePriceRandomMean = WorldProperties.MemeCostRandomAverageUseless;
                thisMemePriceRandomStdDev = WorldProperties.MemeCostRandomStdDevUseless;                
            }
            meme.efficiency = -1;
            meme.price = -1;
            bool is0to1meme = true;
            if (memeAffectedFeature == AvailableFeatures.CooperationEfficiency || memeAffectedFeature == AvailableFeatures.HuntingEfficiency || memeAffectedFeature == AvailableFeatures.TrickEfficiency)
            {
                is0to1meme = false;
            }
            do
            {
                meme.efficiency = randomizer.NormalRandom(thisMemeMean, thisMemeStdDev);
                if (thisMemeMean > 1 && is0to1meme)
                {
                    meme.efficiency = 1;
                }
                meme.complexityCoefficient = Math.Pow(2, (meme.efficiency - thisMemeMean) / thisMemeStdDev);
            }
            while ((meme.efficiency < 0 || (is0to1meme && meme.efficiency > 1)) && memeAffectedFeature != AvailableFeatures.LikelyhoodOfNotBeingAFreeRider);


            while (meme.price < 0)
            {
                meme.price = (thisMemePricePedestal
                    + Math.Abs(meme.efficiency) * thisMemePriceEfficiencyRatio
                    + randomizer.NormalRandom(thisMemePriceRandomMean, thisMemePriceRandomStdDev))
                    * Math.Pow(meme.ComplexityCoefficient, WorldProperties.MemeCostComplexityPriceCoefficient);
            }

            #region Complex culture
            if (WorldProperties.MemePrequisitesChance>0.000001 && memesAlreadyKnown!=null && memesAlreadyKnown.Count() > 0)
            {
                if (randomizer.Chance(WorldProperties.MemePrequisitesChance))
                {
                    do
                    {
                        int memeNo = randomizer.Next(memesAlreadyKnown.Count());
                        meme.prequisiteMemes.Add(memesAlreadyKnown[memeNo]);
                        memesAlreadyKnown.RemoveAt(memeNo);
                    } while (memesAlreadyKnown.Count > 0 && randomizer.Chance(WorldProperties.MemeSubsequentPrequisitesChance));
                    meme.price *= WorldProperties.MemePrequisiteExtraPricePedestal;
                    meme.efficiency *= WorldProperties.MemePrequisiteBonusGainPedestal;
                    double totalPrequisitePrice = 0;
                    double totalPrequisiteEfficiency = 0;
                    foreach (Meme prequisite in meme.prequisiteMemes)
                    {
                        totalPrequisiteEfficiency += prequisite.Efficiency;
                        totalPrequisitePrice += prequisite.Price;
                    }
                    meme.price += totalPrequisitePrice * WorldProperties.MemePrequisiteExtraPriceMultiplier;
                    meme.efficiency += totalPrequisiteEfficiency * WorldProperties.MemePrequisiteBonusGainMultiplier;
                }
            }
            #endregion

            if (meme.AffectedFeature.HasValue)
            {
                meme.Report(string.Format("Meme teaches how {3}. It affects: {0}; Efficiency: {1:f5}; It takes {2:f2} memory.", memeAffectedFeature.GetDescription(), meme.Efficiency, meme.Price, meme.ActionDescription));
            }
            else
            {
                meme.Report(string.Format("Meme is useless. It's about {2}; Efficiency: {0:f5}; It takes {1:f2} memory.", meme.Efficiency, meme.Price, meme.ActionDescription));
            }
            if (meme.prequisiteMemes.Count > 0)
            {
                if (meme.prequisiteMemes.Count == 1)
                {
                    meme.Report(string.Format("To learn it one must know how to {0} (#{1})", meme.prequisiteMemes[0].ActionDescription, meme.prequisiteMemes[0].MemeId));
                } else
                {
                    meme.Report("To learn it ione must know how to:");
                    foreach(Meme prequisite in meme.prequisiteMemes)
                    {
                        meme.Report(string.Format("  - {0} (#{1})", prequisite.ActionDescription, prequisite.MemeId));
                    }
                }
            }
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
                if (AffectedFeature.HasValue)
                {                    
                    return $"#{MemeId} {affectedFeature.Value.GetAcronym()}:{Efficiency:f2}";
                }
                else
                {
                    return $"#{MemeId} Useless";
                }
            }
        }
    }
}
