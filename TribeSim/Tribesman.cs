﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace TribeSim
{
    class Tribesman
    {
        public Random randomizer;
        public void SetRandomizer(Random randomizer) {
            this.randomizer = randomizer;
        }

        private StringBuilder storyOfLife;
        private StringBuilder storyOfPhenotypeChanges;
        private double MemorySizeTotal = 0;
        private double MemorySizeRemaining = 0;
        private string name = null;
        private int yearBorn = 0;
        private Tribe myTribe;
        public int childrenCount;

        public int YearBorn
        {
            get { return yearBorn; }
        }

        public void KeepsDiary() {
            storyOfLife = new StringBuilder();
            storyOfPhenotypeChanges = new StringBuilder();
        }

        /// <summary>
        /// Returns a JSON string like 
        /// {"name":"Tikh-Ro", "memes": [1, 3]}
        /// </summary>
        /// <returns>Ex: {"name":"Tikh-Ro", "memes": [1, 3]}</returns>
        public string GetJSONString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{\"name\":\"").Append(Name).Append("\", \"memes\": [");
            bool isFirst = true;
            foreach (Meme m in knownMemes)
            {
                if (!isFirst) sb.Append(", ");
                sb.Append(m.MemeId);
                isFirst = false;
            }
            sb.Append("]}");
            return sb.ToString();
        }

        public string Name
        {
            get
            {
                if (name == null) { name = NamesGenerator.GenerateName(); }
                return name;
            }
        }

        public string GetLifeStory()
        {
            return storyOfPhenotypeChanges?.ToString() + Environment.NewLine + storyOfLife?.ToString();
        }

        private void ReportPhenotypeChange()
        {
            if (storyOfPhenotypeChanges != null)
            {
                if (storyOfPhenotypeChanges.Length < 3)
                {
                    storyOfPhenotypeChanges.AppendLine(GetPhenotypeHeaders());
                }
                storyOfPhenotypeChanges.AppendFormat("Year {0}: {1}", World.Year, GetPhenotypeString());
                storyOfPhenotypeChanges.AppendLine();
            }
        }

        private MemesSet memesSet = new MemesSet();
        public Features Phenotype = default;
        private int[] lastYearMemeWasUsed = new int[Features.Length];

        public static double[] reproductionCostIncrease;

        private double BasicPriceToGetThisChild;
        private GeneCode genocode = null;
        public Features Genotype;
        private double resource;
        private double totalResourcesCollected;

        private void AddMeme(Meme newMeme) {
            if (!memesSet.Add(newMeme, out _))
                throw new Exception("Something wrong with memes set");
            myTribe?.MemeAdded(this, newMeme);
            MemorySizeRemaining = MemorySizeTotal - memesSet.MemoryPrice;
            memesSet.CalculateEffect((int)newMeme.AffectedFeature);
            CalculatePhenotype((int)newMeme.AffectedFeature);
            ReportPhenotypeChange();
        }
        private void RemoveMeme(Meme meme) {
            if (!memesSet.Remove(meme, out _))
                throw new Exception("Something wrong with memes set");
            myTribe?.MemeRemoved(this, meme);
            MemorySizeRemaining = MemorySizeTotal - memesSet.MemoryPrice;
            memesSet.CalculateEffect((int)meme.AffectedFeature);
            CalculatePhenotype((int)meme.AffectedFeature);
            ReportPhenotypeChange();
        }

        private Tribesman(Random randomizer)
        {
            this.randomizer = randomizer;
            if (randomizer.Chance(WorldProperties.ChancesToWriteALog))
                KeepsDiary();
        }

        public static Tribesman GenerateTribesmanFromAStartingSet(Random randomizer)
        {
            Tribesman retval = new Tribesman(randomizer);
            if (randomizer.Chance(WorldProperties.ChancesToWriteALog))
                retval.KeepsDiary();
            retval.genocode = GeneCode.GenerateInitial(randomizer);
            retval.Genotype = retval.genocode.Genotype();
            retval.storyOfLife?.Append(retval.Name).Append(" was created as a member of a statring set.").AppendLine();
            retval.yearBorn = World.Year;
            retval.resource = WorldProperties.StatringAmountOfResources;
            retval.BasicPriceToGetThisChild = retval.BrainSize * WorldProperties.BrainSizeBirthPriceCoefficient;
            retval.CalculateMemorySizeTotal();
            retval.MemorySizeRemaining = retval.MemorySizeTotal;
            retval.CalculatePhenotype();
            retval.ReportPhenotypeChange();
            return retval;
        }

        private void CalculateMemorySizeTotal()
        {
            this.MemorySizeTotal = Genotype.MemoryLimit;
            MemorySizeTotal += Genotype.CooperationEfficiency * WorldProperties.GeneticCooperationEfficiancyToMemoryRatio;
            MemorySizeTotal += Genotype.Creativity * WorldProperties.GeneticCreativityToMemoryRatio;
            MemorySizeTotal += Genotype.FreeRiderDeterminationEfficiency * WorldProperties.GeneticFreeRiderDeterminationEfficiencytoMemoryRatio;
            MemorySizeTotal += Genotype.FreeRiderPunishmentLikelyhood * WorldProperties.GeneticFreeRiderPunishmentLikelyhoodToMemoryRatio;
            MemorySizeTotal += Genotype.HuntingEfficiency * WorldProperties.GeneticHuntingEfficiencyToMemoryRatio;
            MemorySizeTotal += Genotype.LikelyhoodOfNotBeingAFreeRider * WorldProperties.GeneticLikelyhoodOfNotBeingAFreeRiderToMemoryRatio;
            MemorySizeTotal += Genotype.StudyEfficiency * WorldProperties.GeneticStudyEfficiencyToMemoryRatio;
            MemorySizeTotal += Genotype.StudyLikelyhood * WorldProperties.GeneticStudyLikelyhoodToMemoryRatio;
            MemorySizeTotal += Genotype.TeachingEfficiency * WorldProperties.GeneticTeachingEfficiencyToMemoryRatio;
            MemorySizeTotal += Genotype.TeachingLikelyhood * WorldProperties.GeneticTeachingLikelyhoodToMemoryRatio;
            MemorySizeTotal += Genotype.TrickEfficiency * WorldProperties.GeneticTrickEfficiencyToMemoryRatio;
            MemorySizeTotal += Genotype.TrickLikelyhood * WorldProperties.GeneticTrickLikelyhoodToMemoryRatio;
        }

        public string GetPhenotypeString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (AvailableFeatures af in Enum.GetValues(typeof(AvailableFeatures)))
            {
                sb.AppendFormat("{0:f5}; ", Phenotype[(int)af]);
            }
            sb.AppendFormat("{0:f5} ", MemorySizeRemaining);
            return sb.ToString();
        }

        public string GetPhenotypeHeaders()
        {
            StringBuilder sb = new StringBuilder();
            foreach (AvailableFeatures af in Enum.GetValues(typeof(AvailableFeatures)))
            {
                sb.AppendFormat("{0}; ", af.ToString());
            }
            sb.Append("Memory free");
            return sb.ToString();
        }


        private void CalculatePhenotype() {
            for (int i = Features.Length - 1; i >= 0; i--)
                CalculatePhenotype(i);
        }
        private void CalculatePhenotype(int feature) {
            Phenotype[feature] = WorldProperties.FeatureDescriptions[feature].Aggregator().Append(Genotype[feature]).Append(memesSet.MemesEffect[feature]).Value;
            if (feature == (int)AvailableFeatures.AgeingRate && Phenotype[feature] < 0)
                // AR надо считать вообще по-другому! тут вероятностная алгебра не работает, потому что этот показатель в принципе не вероятность!
                // Надо следить за коммутативностью, сохраняя при этом значение выше нуля.
                // Может ответ в логарифмической шкале? Надо думать. Пока не придумалось ничего лучше, чем обрезать после вычислений. 
                // Это должно углубить потенциальную яму нулевого AR, но придумать что-то более элегантное мозгов пока не хватило.
                Phenotype[feature] = 0;
        }

        public void ConsumeLifeSupport() {
            double lifeSupportCosts = WorldProperties.LifeSupportCosts;
            if (UseGompertzAgeing) {
                double ARdifference = Phenotype.AgeingRate - WorldProperties.BaseGompertzAgeingRate;
                if (ARdifference != 0) {
                    if (ARdifference < 0) {
                        if (WorldProperties.BaseGompertzAgeingRateLifeCostIncrease != 0)
                            lifeSupportCosts -= ARdifference * WorldProperties.BaseGompertzAgeingRateLifeCostIncrease; // ARDifference отрицательная, то есть цена жизнеобеспечения возрастёт.
                        if (WorldProperties.BaseGompertzAgeingRateLifeCostQuadraticIncrease != 0)
                            lifeSupportCosts += ARdifference * ARdifference * WorldProperties.BaseGompertzAgeingRateLifeCostQuadraticIncrease;
                        if (WorldProperties.BaseGompertzAgeingRateLifeCostExponentialCoefficientIncrease != 0 && WorldProperties.BaseGompertzAgeingRateLifeCostExponentialMultiplierIncrease != 0)
                            lifeSupportCosts += WorldProperties.BaseGompertzAgeingRateLifeCostExponentialCoefficientIncrease * Math.Exp(Math.Abs(ARdifference) * WorldProperties.BaseGompertzAgeingRateLifeCostExponentialMultiplierIncrease);
                    } else if (ARdifference > 0) {
                        if (WorldProperties.BaseGompertzAgeingRateLifeCostDecrease != 0)
                            lifeSupportCosts -= ARdifference * WorldProperties.BaseGompertzAgeingRateLifeCostDecrease; // ARDifference положительная, то есть цена жизнеобеспечения падает.
                        if (WorldProperties.BaseGompertzAgeingRateLifeCostQuadraticDecrease != 0)
                            lifeSupportCosts -= ARdifference * ARdifference * WorldProperties.BaseGompertzAgeingRateLifeCostQuadraticDecrease;
                        if (WorldProperties.BaseGompertzAgeingRateLifeCostExponentialCoefficientDecrease != 0 && WorldProperties.BaseGompertzAgeingRateLifeCostExponentialMultiplierDecrease != 0)
                            lifeSupportCosts -= WorldProperties.BaseGompertzAgeingRateLifeCostExponentialCoefficientDecrease * Math.Exp(ARdifference * WorldProperties.BaseGompertzAgeingRateLifeCostExponentialMultiplierDecrease);
                    }
                    if (lifeSupportCosts < 0) {
                        lifeSupportCosts = 0;
                    }
                }
            }


            resource -= lifeSupportCosts;
            storyOfLife?.Append("Eaten ").Append(lifeSupportCosts.ToString("f2", CultureInfo.InvariantCulture)).Append(" resources. ").Append(resource.ToString("f2", CultureInfo.InvariantCulture)).Append(" left.").AppendLine();
        }

        public void TryInventMemeSpontaneously()
        {
            // Exit if no memes can be created
            if (WorldProperties.MemesWhichCanBeInvented.Length == 0)
                return;

            double creativity = Phenotype.Creativity;
            if (randomizer.Chance(creativity))
            {
                AvailableFeatures af = (AvailableFeatures)(WorldProperties.MemesWhichCanBeInvented[randomizer.Next(WorldProperties.MemesWhichCanBeInvented.Length)]);

                if (InventNewMemeForAFeature(af, out Meme inventedMeme))
                {
                    storyOfLife?.AppendFormat("Invented how {5}. ({0} meme with {1} effect). Complexity is {2:f2} and {3} now has {4:f2} memory left. (Meme#{5})", af.GetDescription(), inventedMeme.Efficiency, inventedMeme.Price, Name, MemorySizeRemaining, inventedMeme.MemeId, inventedMeme.ActionDescription).AppendLine();
                    UseMemeGroup(AvailableFeatures.Creativity, "InventMemeSpontaneously");
                    UseMemeGroup(AvailableFeatures.MemoryLimit, "InventMemeSpontaneously");
                }
                else
                {
                    storyOfLife?.AppendFormat("Invented the new meme and forgot it immediately because {0} is too stupid to remember anything else. Memory size is {1:f2} and meme complexity is {2:f2}", Name, MemorySizeRemaining, inventedMeme.Price).AppendLine();
                }
            }
        }

        private bool InventNewMemeForAFeature(AvailableFeatures featureTheMemeWillAffect, out Meme createdMeme)
        {
            createdMeme = Meme.InventNewMeme(randomizer, featureTheMemeWillAffect);
            myTribe.statistic.CollectThisYear?.ReportCountEvent(TribeStatistic.EventName.MemeInvented);
            if (memesSet.memes.Contains(createdMeme, Meme.EqualityComparer.Singleton)) {
                // Tribesman already known meme with the same Id.
                if (WorldProperties.CollectMemesSuccess > .5f && randomizer.Chance(WorldProperties.ChanceToCollectMemesSuccess))
                    createdMeme.ReportDetaliedStatistic();
                return false;
            } else if (createdMeme.Price < MemorySizeRemaining) {
                AddMeme(createdMeme);
                createdMeme.ReportInvented(this);
                return true;
            } else {
                if (WorldProperties.CollectMemesSuccess > .5f && randomizer.Chance(WorldProperties.ChanceToCollectMemesSuccess))
                    createdMeme.ReportDetaliedStatistic();
                return false;
            }
        }

        public void ForgetUnusedMemes()
        {
            int year = World.Year;
            for (int feature = 0; feature < memesSet.memesByFeature.Length; feature++) // Из коробки оптимизатор прохода по массиву жмёт хорошо
            {
                int memesCount = memesSet.memesByFeature[feature];
                if (memesCount > 0 && year - lastYearMemeWasUsed[feature] > WorldProperties.DontForgetMemesThatWereUsedDuringThisPeriod)
                {
                    for (int memeIndex = memesCount - 1; memeIndex >= 0; memeIndex--)
                        if (randomizer.Chance(WorldProperties.ChanceToForgetTheUnusedMeme))
                        {
                            Meme meme = memesSet.GetMemeByFeature(feature, memeIndex);
                            meme.ReportForgotten(this);
                            storyOfLife?.AppendFormat("Forgotten how {1} ({0})", meme.SignatureString,
                                meme.ActionDescription).AppendLine();
                            RemoveMeme(meme);
                        }
                }
            }
        }

        ~Tribesman()
        {
            if (storyOfLife != null)
            {
                string lifeStory = GetLifeStory(); // Ну не знаю - не знаю по идее надо в стрим писать прямо из стрингбилдера, нафига память то тратить.
                if (lifeStory.Length < 10) return;
                string filename = Path.Combine(World.TribesmanLogFolder, this.Name + " born " + YearBorn + ".txt");
                File.WriteAllText(filename, lifeStory);
            }
        }

        public IReadOnlyList<Meme> knownMemes => memesSet.memes;

        public bool TryToTeach(Tribesman student, bool isCulturalExchange = false)
        {
            if (memesSet.memes.Count > 0 && randomizer.Chance(Phenotype.TeachingLikelyhood))
            {
                if (resource >= WorldProperties.TeachingCosts || randomizer.Chance(WorldProperties.ChanceToTeachIfUnsufficienResources))
                {
                    resource -= WorldProperties.TeachingCosts;
                    List<Meme> memeAssoc = memesSet.memes.Where(meme => !student.knownMemes.Contains(meme, Meme.EqualityComparer.Singleton)).ToList();
                    if (memeAssoc.Count == 0)
                    {
                        storyOfLife?.AppendFormat("{3}Tried to teach {0} something, but he already knows everything {1} can teach him. {2} resources wasted.", student.Name, Name, WorldProperties.TeachingCosts, isCulturalExchange ? "Cultural Exchange! " : "").AppendLine();
                        return false;
                    }
                    Meme memeToTeach = memeAssoc[randomizer.Next(memeAssoc.Count)];
                    double teachingSuccessChance = SupportFunctions.MultilpyProbabilities(
                        WorldProperties.FeatureDescriptions[(int)AvailableFeatures.TeachingEfficiency].Aggregator()
                            .Append(Phenotype.TeachingEfficiency)
                            .Append(student.Phenotype.StudyEfficiency)
                            .Value,
                        Math.Pow(1d / memeToTeach.ComplexityCoefficient, WorldProperties.MemeComplexityToLearningChanceCoefficient));;
                    if (randomizer.Chance(teachingSuccessChance))
                    {
                        if (student.MemorySizeRemaining < memeToTeach.Price)
                        {
                            storyOfLife?.AppendFormat("{3}Tried to teach {0} how {4} ({1}) and failed. {0} was too stupid to remember it. {2} resources wasted.", student.Name, memeToTeach.SignatureString, WorldProperties.TeachingCosts, isCulturalExchange ? "Cultural Exchange! " : "", memeToTeach.ActionDescription).AppendLine();
                            return false;
                        }
                        else
                        {
                            student.LearnNewMemeFrom(memeToTeach, this);
                            if (isCulturalExchange) memeToTeach.Report("Cultural Exchange!");
                            storyOfLife?.AppendFormat("{3}Successfully taught {0} how {4} ({1}). {2} resources used for teaching.", student.Name, memeToTeach.SignatureString, WorldProperties.TeachingCosts, isCulturalExchange ? "Cultural Exchange! " : "", memeToTeach.ActionDescription).AppendLine();
                            UseMemeGroup(AvailableFeatures.TeachingEfficiency, "teaching");
                            UseMemeGroup(AvailableFeatures.TeachingLikelyhood, "teaching");
                            student.UseMemeGroup(AvailableFeatures.StudyEfficiency, "studying");
                            return true;
                        }
                    }
                    else
                    {
                        storyOfLife?.AppendFormat("Tried to teach {0} how {4} ({1}) and failed. Chances were {2:2}%. {3} resources wasted.", student.Name, memeToTeach.SignatureString, teachingSuccessChance * 100d, WorldProperties.TeachingCosts, memeToTeach.ActionDescription).AppendLine();
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public void UseMemeGroup(AvailableFeatures usedFeature, string activity = "engaged in unknown activity")
        {
            FeatureDescription description = WorldProperties.FeatureDescriptions[(int)usedFeature];
            if (!description.MemCanBeInvented)
                return;

            double C = Phenotype.Creativity;
            double M = WorldProperties.ChanceToInventNewMemeWhileUsingItModifier;
            double T = WorldProperties.ChanceToInventNewMemeWhileUsingItThreshold;
            double chanceToInventNewMeme = T + M * C - T * M * C;

            if (randomizer.Chance(chanceToInventNewMeme))
            {
                Meme inventedMeme;

                if (InventNewMemeForAFeature(usedFeature, out inventedMeme))
                {
                    storyOfLife?.AppendFormat("While {6} invented how {7}. ({0} meme with {1} effect) The meme complexity is {2:f2} and {3} now has {4:f2} memory left. ({5})", usedFeature.GetDescription(), inventedMeme.Efficiency, inventedMeme.Price, Name, MemorySizeRemaining, inventedMeme.SignatureString, activity, inventedMeme.ActionDescription).AppendLine();
                    UseMemeGroup(AvailableFeatures.Creativity, "Invent Meme During ability use");
                    UseMemeGroup(AvailableFeatures.MemoryLimit, "Invent Meme During ability use");
                }
                else
                {
                    storyOfLife?.AppendFormat("While {3} invented the new meme but forgot it immediately. Too stupid to remember anything else. Memory size is {1:f2} and meme complexity is {2:f2}", Name, MemorySizeRemaining, inventedMeme.Price, activity).AppendLine();
                }
            }
            lastYearMemeWasUsed[(int)usedFeature] = World.Year; // Переделываем на группы чтобы 
            for (int i = 0; i < memesSet.memes.Count; i++)
                if (memesSet.memes[i].AffectedFeature == usedFeature)
                    myTribe.MemeUsed(this, memesSet.memes[i]); // Тот, кто не в племени не может пользовать мемы.
        }

        private void LearnNewMemeFrom(Meme meme, Tribesman teacher)
        {
            AddMeme(meme);
            if (teacher != null)
            {
                storyOfLife?.AppendFormat("{0} taught how {4} ({2}). {3:f2} memory remaining.", teacher.Name, Name, meme.SignatureString, MemorySizeRemaining, meme.ActionDescription).AppendLine();
            }
            else
            {
                storyOfLife?.AppendFormat("Learned how {2} ({0}) from a tribesmate. {1:f2} memory remaining.", meme.SignatureString, MemorySizeRemaining, meme.ActionDescription).AppendLine();
            }
            meme.ReportTeaching(this, teacher);
        }

        public void DetermineAndPunishAFreeRider(List<Tribesman> members, List<Tribesman> freeRidersList)
        {
            if (randomizer.Chance(Phenotype.FreeRiderPunishmentLikelyhood))
            {
                resource -= WorldProperties.FreeRiderPunishmentCosts;
                if (randomizer.Chance(Phenotype.FreeRiderDeterminationEfficiency))
                {
                    // Punish a free rider

                    int rand = randomizer.Next(freeRidersList.Count);
                    Tribesman FreeRiderToBePunished = freeRidersList[rand];
                    while (FreeRiderToBePunished == this)
                    {
                        if (freeRidersList.Count == 1)
                        {
                            storyOfLife?.Append("Successfully determined himself to be the only free rider in a tribe.");
                            
                            if (WorldProperties.InventNewPositiveMemesForANotBeAFreeRider > 0.5 && WorldProperties.FeatureDescriptions[(int)AvailableFeatures.LikelyhoodOfNotBeingAFreeRider].MemCanBeInvented)
                            {
                                if (InventNewMemeForAFeature(AvailableFeatures.LikelyhoodOfNotBeingAFreeRider, out var inventedMeme2))
                                {
                                    if (inventedMeme2.Efficiency > 0) {
                                        storyOfLife?.AppendFormat("Learned {1} ({0}) as a result of a guilty conscience.", inventedMeme2.SignatureString, inventedMeme2.ActionDescription).AppendLine();
                                    } else {
                                        inventedMeme2.ReportForgotten(this);
                                        storyOfLife?.AppendFormat("Forgot new meme{0}, cause it was a negative meme teaching how to {1}", inventedMeme2.SignatureString, inventedMeme2.ActionDescription).AppendLine();
                                        RemoveMeme(inventedMeme2);
                                    }
                                }
                                else
                                {
                                    storyOfLife?.Append("Tried to invent new meme as a result of a guilty conscience, but was too stupid to remember it.");
                                }
                            }
                            return;
                        }
                        else
                        {
                            rand = randomizer.Next(freeRidersList.Count);
                            FreeRiderToBePunished = freeRidersList[rand];
                        }
                    }
                    FreeRiderToBePunished.resource -= WorldProperties.FreeRiderPunishmentAmount;
                    FreeRiderToBePunished.storyOfLife?.AppendFormat("Was punished by {0} for being a free rider. {1} resources destroyed, {2:f2} remaining.", Name, WorldProperties.FreeRiderPunishmentAmount, FreeRiderToBePunished.resource).AppendLine();
                    storyOfLife?.AppendFormat("Decided to punish a free rider. Successfully determined that {0} is one and punished him for {1} resources.", FreeRiderToBePunished.Name, WorldProperties.FreeRiderPunishmentAmount).AppendLine();
                    UseMemeGroup(AvailableFeatures.FreeRiderDeterminationEfficiency, "punishing a free rider");
                    UseMemeGroup(AvailableFeatures.FreeRiderPunishmentLikelyhood, "punishing a free rider");
                    Meme inventedMeme;
                    if (FreeRiderToBePunished.InventNewMemeForAFeature(AvailableFeatures.LikelyhoodOfNotBeingAFreeRider, out inventedMeme))
                    {
                        FreeRiderToBePunished.storyOfLife?.AppendFormat("Learned {1} ({0}) as a result of punishment.", inventedMeme.SignatureString, inventedMeme.ActionDescription).AppendLine();
                    }
                    else
                    {
                        FreeRiderToBePunished.storyOfLife?.Append("Tried to invent new meme as a result of punishment, but was too stupid to remember it.");
                    }
                }
                else
                {
                    // Punish random person                    
                    int rand = randomizer.Next(members.Count);
                    Tribesman FreeRiderToBePunished = members[rand];
                    while (FreeRiderToBePunished == this)
                    {
                        if (members.Count == 1)
                        {
                            storyOfLife?.Append("Successfully determined himself to be the only man in a tribe. Punishing free riders makes no sense.");
                            return;
                        }
                        else
                        {
                            rand = randomizer.Next(members.Count);
                            FreeRiderToBePunished = members[rand];
                        }
                    }
                    FreeRiderToBePunished.resource -= WorldProperties.FreeRiderPunishmentAmount;
                    FreeRiderToBePunished.storyOfLife?.AppendFormat("Was punished by {0} on false suspiction.", Name).AppendLine();
                    storyOfLife?.AppendFormat("Tried to punish a free rider, but failed to identify one. Punished {0} for {1} resources instead.", FreeRiderToBePunished.Name, WorldProperties.FreeRiderPunishmentAmount).AppendLine();
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public double GoHunting()
        {
            if (resource <= WorldProperties.HuntingCosts)
            {
                storyOfLife?.AppendFormat("Wanted to go hunting, but could not. Too hungry. Resources remaining - {0:f2}, needed for the hunt - {1}.", resource, WorldProperties.HuntingCosts).AppendLine();
                return 0;
            }
            resource -= WorldProperties.HuntingCosts;
            storyOfLife?.AppendFormat("Went hunting with his friends. He hunted with {0:f1} efficiency and cooperated at {1:f2}. Spent {2} resource for the hunt, {3:f2} remaining.", Phenotype.HuntingEfficiency, Phenotype.CooperationEfficiency, WorldProperties.HuntingCosts, resource).AppendLine();
            UseMemeGroup(AvailableFeatures.HuntingEfficiency, "hunting");
            UseMemeGroup(AvailableFeatures.LikelyhoodOfNotBeingAFreeRider, "hunting");
            UseMemeGroup(AvailableFeatures.CooperationEfficiency, "hunting");
            return Phenotype.HuntingEfficiency;
        }

        public void SkipHunting()
        {
            storyOfLife?.AppendFormat("Did not go hunting with the tribe. He told the others that {0}.", NamesGenerator.GenerateLameExcuse()).AppendLine();
        }

        public double TellHowMuchDoYouWant(double resourcesReceivedPerGroup)
        {
            double requestedShare = 1;
            if (randomizer.Chance(Phenotype.TrickLikelyhood))
            {
                requestedShare += Phenotype.TrickEfficiency;
                storyOfLife?.AppendFormat("Group returned from the hunt with {0:f0} resources. He decided to play a trick and asked to get {1:f1}% more then the rest.", resourcesReceivedPerGroup, (requestedShare - 1) * 100).AppendLine();
                UseMemeGroup(AvailableFeatures.TrickEfficiency, "sharing resources");
                UseMemeGroup(AvailableFeatures.TrickLikelyhood, "sharing resources");
            }
            else
            {
                storyOfLife?.AppendFormat("Group returned from the hunt with {0:f0} resources. He asked for a fare share.", resourcesReceivedPerGroup).AppendLine();
            }
            return requestedShare;
        }
        public double GetForagingEffort() {
            if (Phenotype.ForagingEfficiency > 0) {
                UseMemeGroup(AvailableFeatures.ForagingEfficiency, "foraging");
                return Phenotype.ForagingEfficiency;
            }
            return 0;
        }

        public double CanBeOrganizator()  {
            if (Phenotype.OrganizationAbility > 0 && resource >= WorldProperties.OrganizationAbilityCosts) {
                if (WorldProperties.OrganizationAbilityMemesUsedByWinnerOnly < 0.5)
                    UseMemeGroup(AvailableFeatures.OrganizationAbility, "GoHunting");
                return Phenotype.OrganizationAbility;
            }
            return 0;
        }
        public void ToBeOrganizator()
        {
            resource -= WorldProperties.OrganizationAbilityCosts;
            if (WorldProperties.OrganizationAbilityMemesUsedByWinnerOnly > 0.5)
                UseMemeGroup(AvailableFeatures.OrganizationAbility, "GoHunting");
            storyOfLife?.AppendLine($"Has Organization Ability:{Phenotype.OrganizationAbility} and win Organizator role, with spending {WorldProperties.OrganizationAbilityCosts.ToSignificantNumbers(2)} resources, {resource.ToSignificantNumbers(2)} lefts.");
        }

        public void RecieveResourcesShare(double recievedShare)
        {
            resource += recievedShare;
            totalResourcesCollected += recievedShare;
            if (storyOfLife != null)
                if (randomizer.Chance(0.99))
                {
                    storyOfLife.AppendFormat("After {2} debate received {0:f0} resources from the common loot and now has {1:f0}.", recievedShare, resource, NamesGenerator.Flip()?"continuous":"short").AppendLine(); // Тут рандомизация только для логов, так что воспроизводить ничего не нужно.
                }
                else
                {
                    storyOfLife.AppendFormat("After {3} debate and a broken {0} received {1:f0} resources from the common loot and now has {2:f0}", NamesGenerator.GenerateBodypart(), recievedShare, resource, NamesGenerator.Flip() ? "continuous" : "short").AppendLine(); // Тут рандомизация только для логов, так что воспроизводить ничего не нужно.
                }
        }

        public void ReceiveForagedResources(double recievedShare)
        {
            resource += recievedShare;
            totalResourcesCollected += recievedShare;
            storyOfLife?.AppendFormat("Went foraging, found {0} resources and now has {1}.", recievedShare, resource).AppendLine();
        }

        public void PerformUselessActions()
        {
            int uselessActionsMade = 0;
            double resourcesWasted = 0;
            var chance = Phenotype.UselessActionsLikelihood;
            if (chance >= 0.999 && WorldProperties.AllowRepetitiveUselessActions>1)
            {
                resource = 0;
                storyOfLife?.Append("Wasted all his resources on useless actions.");
                return;
            }
            while (randomizer.Chance(chance))
            {
                uselessActionsMade++;
                resourcesWasted += WorldProperties.UselessActionCost;
                if (WorldProperties.AllowRepetitiveUselessActions < 0.5) break;
            }
            if (uselessActionsMade > 0)
            {
                resource -= resourcesWasted;

                if (storyOfLife != null) {
                    if (Phenotype.UselessActionsLikelihood > Genotype.UselessActionsLikelihood) {
                        var relevantMemes = memesSet.memes.Where(meme => (int)meme.AffectedFeature == (int)AvailableFeatures.UselessActionsLikelihood);
                        Meme randomKnownUselessMeme = relevantMemes.Skip(randomizer.Next(relevantMemes.Count())).FirstOrDefault(); // Всё это медленно и печально, но оно нужно только для логов, так что сойдёт. 
                        storyOfLife?.AppendFormat("Decided {0}. Didn't gain anything from it, but wasted {1:f2} resources on it. {2:f2} remaining.", randomKnownUselessMeme?.ActionDescription ?? "by genetic predisposition", resourcesWasted, resource).AppendLine();
                    } else {
                        storyOfLife?.AppendFormat($"Geneticaly based useless actions. Chance: {chance}, but wasted {resourcesWasted:f2} resources on it. {resource:f2} remaining.").AppendLine();
                    }
                }
                UseMemeGroup(AvailableFeatures.UselessActionsLikelihood, "performing useless actions");
            }
        }

        public void PrepareForANewYear()
        {
            storyOfLife?.Append("    --------------    ");
            if (UseGompertzAgeing) {
                if (Age >= WorldProperties.GompertzAgeingAgeAtWhichAgeingStarts) {
                    UseMemeGroup(AvailableFeatures.AgeingRate, "caring for himself.");
                }
            }
        }

        public void StudyOneOrManyOfTheMemesUsedThisTurn(List<Meme> memesUsedThisYear, List<Meme> memesAvailableForStudy)
        {
            memesUsedThisYear.ExcludeFromSortedList(memesSet.memes, memesAvailableForStudy);

            /// Всё что может быть выучено отсортировали, можно начинать.
            while (memesAvailableForStudy.Count > 0 && randomizer.Chance(Phenotype.StudyLikelyhood)) // Первый шаг оптимизации: Простое изменение порядка следования проверок сокращает время с 24 до 17
            {
                if (resource <= WorldProperties.StudyCosts)
                {
                    storyOfLife?.AppendLine("Wanted to learn something new but couldn't. Too hungry.");
                    break;
                }
                // Тут есть три возможные причины перехода к следующему элементу цикла - пререквизиты, шанс выучить и недостаток памяти. Наерняка можно сильно съэкономить если расположить их в правильном порядке.
                Meme memeToStudy = memesAvailableForStudy[randomizer.Next(memesAvailableForStudy.Count)];
                resource -= WorldProperties.StudyCosts;
                var chanceOfStudy = SupportFunctions.MultilpyProbabilities(
                        Phenotype.StudyEfficiency,
                        Math.Pow(1d / memeToStudy.ComplexityCoefficient, WorldProperties.MemeComplexityToLearningChanceCoefficient));
                if (randomizer.Chance(chanceOfStudy))
                {
                    if (MemorySizeRemaining > memeToStudy.Price)
                    {
                        LearnNewMemeFrom(memeToStudy, null);
                        UseMemeGroup(AvailableFeatures.StudyEfficiency, "learning");
                        UseMemeGroup(AvailableFeatures.StudyLikelyhood, "learning");
                    }
                    else
                    {
                        storyOfLife?.AppendFormat("Tried to teach himself {3} ({0}), but was too stupid to remember it. Meme complexity is {1:f2} and memory size remianing is {2:f2}", memeToStudy.SignatureString, memeToStudy.Price, MemorySizeRemaining, memeToStudy.ActionDescription).AppendLine();
                        break;
                    }
                }
                memesAvailableForStudy.Remove(memeToStudy);
                if (WorldProperties.AllowRepetitiveStudying < 0.5) break;
            }
        }

        public static Boolean? useGompertzAgeing = null;
        public static Boolean UseGompertzAgeing {
            get {
                if (!useGompertzAgeing.HasValue) {                    
                    useGompertzAgeing = WorldProperties.UseGompertzAgeing > 0.5;
                }
                return useGompertzAgeing.Value;
            }
            set {
                useGompertzAgeing = value;
            }
        }
        public bool WantsToDie()
        {
            int age = World.Year - yearBorn;
            double chanceOfDeathOfOldAge = 0;
            if (UseGompertzAgeing) {
                if (age <= WorldProperties.GompertzAgeingAgeAtWhichAgeingStarts) {
                    chanceOfDeathOfOldAge = WorldProperties.GompertzAgeingBasicMortalityRate;
                } else {
                    chanceOfDeathOfOldAge = WorldProperties.GompertzAgeingBasicMortalityRate * Math.Exp(Phenotype.AgeingRate*(age - WorldProperties.GompertzAgeingAgeAtWhichAgeingStarts));
                    double mortalityPlateau = 1;
                    if (WorldProperties.GompertzAgeingMortalityPlateau < WorldProperties.GompertzAgeingBasicMortalityRate || WorldProperties.GompertzAgeingMortalityPlateau > 1 ) {
                        mortalityPlateau = 1;
                    } else {
                        mortalityPlateau = WorldProperties.GompertzAgeingMortalityPlateau;
                    }
                    chanceOfDeathOfOldAge = Math.Max(WorldProperties.GompertzAgeingBasicMortalityRate, Math.Min(mortalityPlateau, chanceOfDeathOfOldAge));
                }
            } else {
                if (age >= WorldProperties.DontDieIfYoungerThan)            
                {
                    chanceOfDeathOfOldAge = WorldProperties.DeathLinearChance + age * WorldProperties.DeathAgeDependantChance;                    
                }
            }

            if (randomizer.Chance(chanceOfDeathOfOldAge))
            {
                ReportOnDeathStatistics(age, "age");
                ForgetAllMemes();
                storyOfLife?.AppendFormat("Died of natural causes at the age of {0}. Chances of dying were {1:f1}%.",
                    age, chanceOfDeathOfOldAge * 100).AppendLine();
                myTribe.statistic.CollectThisYear?.ReportCountEvent(TribeStatistic.EventName.DeathsOfOldAge);
                return true;
            }

            if (resource <= 0)
            {
                ReportOnDeathStatistics(age,"hunger");
                ForgetAllMemes();
                storyOfLife?.AppendFormat("Died of hunger at the age of {0}.", age).AppendLine();
                myTribe.statistic.CollectThisYear?.ReportCountEvent(TribeStatistic.EventName.DeathsOfHunger);
                return true;
            }
            return false;
        }

        private void ReportOnDeathStatistics(int age, string cause) {
            myTribe.statistic.CollectThisYear?.ReportAvgEvent(TribeStatistic.EventName.Longevity, age);
            double totalBrainUsage = 0;
            double totalBrainSize = 0;
            for (int i = 0; i < memesSet.memes.Count; i++) {
                totalBrainUsage += memesSet.memes[i].Price;
            }
            totalBrainSize = totalBrainUsage + MemorySizeRemaining;
            if (totalBrainSize > 0)
            {
                myTribe.statistic.CollectThisYear?.ReportAvgEvent(TribeStatistic.EventName.MemoryUnusedWhenDied, MemorySizeRemaining / totalBrainSize);
            }
            // Подбиваем подробную статистику по репродуктивному успеху.
            if (WorldProperties.CollectIndividualSuccess > 0.5) {
                if (randomizer.Chance(WorldProperties.ChanceToCollectIndividualSuccess)) {
                    var individualSucess = new IndividualSuccess()
                    {
                        birthYear = yearBorn,
                        age = World.Year - yearBorn,
                        children = childrenCount,
                        totalResourcesCollected = (float)totalResourcesCollected,
                        tribeName = $"{myTribe.TribeName}-{myTribe.id}",
                        priceOfThisChild = (float)BasicPriceToGetThisChild,
                        deathCause = cause
                    };
                    if (WorldProperties.CollectIndividualGenotypeValues > 0.5)
                    {
                        individualSucess.genotype = FeaturesFloatArray.Get();
                        for (int i = 0; i < Features.Length; i++)
                            individualSucess.genotype[i] = (float)Genotype[i];
                    }
                    if (WorldProperties.CollectIndividualPhenotypeValues > 0.5)
                    {
                        individualSucess.phenotype = FeaturesFloatArray.Get();
                        for (int i = 0; i < Features.Length; i++)
                            individualSucess.phenotype[i] = (float)Phenotype[i];
                    }
                    StatisticsCollector.ReportEvent(individualSucess);
                }
            }
        }

        public struct IndividualSuccess : IDetaliedEvent
        {
            public int birthYear;
            public int age;
            public int children;
            public float totalResourcesCollected;
            public string deathCause;
            public string tribeName;
            public float priceOfThisChild;
            public float[] genotype;
            public float[] phenotype;

            public string Header()
            {
                var sb = StringBuilderPool.Get();
                sb.Append("birthYear");
                sb.Append(',').Append("age");
                sb.Append(',').Append("children");
                sb.Append(',').Append("totalResourcesCollected");
                sb.Append(',').Append("deathCause");
                sb.Append(',').Append("tribeName");
                sb.Append(',').Append("priceOfThisChild");
                if (genotype != null)
                    for (int i = 0; i < Features.Length; i++)
                        sb.Append(",gen.").Append(((AvailableFeatures)i).GetAcronym());
                if (phenotype != null)
                    for (int i = 0; i < Features.Length; i++)
                        sb.Append(",phen.").Append(((AvailableFeatures)i).GetAcronym());
                sb.Append('\n');
                return sb.Release();
            }

            public string Data()
            {
                var sb = StringBuilderPool.Get();
                sb.Append(birthYear);
                sb.Append(',').Append(age);
                sb.Append(',').Append(children);
                sb.Append(',').Append(totalResourcesCollected.ToString("F3", CultureInfo.InvariantCulture));
                sb.Append(',').Append(deathCause);
                sb.Append(',').Append(tribeName);
                sb.Append(',').Append(priceOfThisChild.ToString("F3", CultureInfo.InvariantCulture));
                if (genotype != null) {
                    for (int i = 0; i < Features.Length; i++)
                        sb.Append(',').Append(genotype[i].ToString("F3", CultureInfo.InvariantCulture));
                }
                if (phenotype != null) {
                    for (int i = 0; i < Features.Length; i++)
                        sb.Append(',').Append(phenotype[i].ToString("F3", CultureInfo.InvariantCulture));
                }
                sb.Append('\n');
                return sb.Release();
            }

            public void Clear()
            {
                genotype?.ReleaseFeatures();
                phenotype?.ReleaseFeatures();
            }
        }

        private void ForgetAllMemes() {
            for (int i = memesSet.memes.Count - 1; i >= 0; i--)
                memesSet.memes[i].ReportForgotten(this);                
        }

        public void DieOfLonliness()
        {
            myTribe.statistic.CollectThisYear?.ReportCountEvent(TribeStatistic.EventName.DeathsOfLonliness);
            int age = World.Year - yearBorn;
            ReportOnDeathStatistics(age, "lonliness");
            ForgetAllMemes();            
            storyOfLife?.AppendFormat("Died of lonliness at the age of {0}. Remained the only one in the tribe.", age).AppendLine();            
        }

        public void Release()
        {
        }


        public bool IsOfReproductionAge { get { return Age >= WorldProperties.DontBreedIfYoungerThan && (WorldProperties.MaximumBreedingAge < 0.5 || Age <= WorldProperties.MaximumBreedingAge); } }
        public int Age { get {return World.Year-yearBorn;} }

        public static Tribesman Breed(Random randomizer, Tribesman PartnerA, Tribesman PartnerB, List<Meme> cachedList)
        {
            double totalParentsResource = PartnerA.resource + PartnerB.resource;

            if (totalParentsResource * 5 < 2*(PartnerA.BasicPriceToGetThisChild + PartnerB.BasicPriceToGetThisChild)) // Проверка на вшивость. Если у них вдвоём не набирается даже 2/3 от того, что они сами стоили незачем и начинать.
                return null;

            Tribesman child = new Tribesman(randomizer);
            child.genocode = GeneCode.GenerateFrom(randomizer, PartnerA.genocode, PartnerB.genocode);
            child.Genotype = child.genocode.Genotype();

            double priceToGetThisChildBrainSizePart = child.BrainSize * WorldProperties.BrainSizeBirthPriceCoefficient;
            double priceToGetThisChildGiftPart = WorldProperties.ChildStartingResourcePedestal + WorldProperties.ChildStartingResourceParentsCoefficient * (totalParentsResource - priceToGetThisChildBrainSizePart);
            double priceDueToAge = 0;
            if (WorldProperties.BreedingCostsIncreaseCoefficient > 0) {
                priceDueToAge = reproductionCostIncrease[PartnerA.Age] + reproductionCostIncrease[PartnerB.Age];
            }
            child.BasicPriceToGetThisChild = priceToGetThisChildBrainSizePart + WorldProperties.ChildStartingResourcePedestal; // Записываем только минимально необходимую часть ресурса, пошедшую на мозг и родительский бонус. Наследство может быть большим, маленьким или вообще нулевым.

            if (totalParentsResource > priceToGetThisChildBrainSizePart + priceToGetThisChildGiftPart + priceDueToAge)
            {
                PartnerA.myTribe.statistic.CollectThisYear?.ReportCountEvent(TribeStatistic.EventName.ChildBirths);
                PartnerA.myTribe.statistic.CollectThisYear?.ReportAvgEvent(TribeStatistic.EventName.ChildAverageBrainSize, child.BrainSize);
                child.yearBorn = World.Year;
                if (randomizer.Chance(WorldProperties.ChancesToWriteALog))
                    child.KeepsDiary();
                child.myTribe = PartnerA.myTribe;
                child.storyOfLife?.AppendFormat("Was born from {0} and {1}. His brain size is {2:f1}. His parents spent {3:f1} resources to raise him.", PartnerA.Name, PartnerB.Name, child.BrainSize, priceToGetThisChildBrainSizePart).AppendLine();
                child.CalculateMemorySizeTotal();
                child.MemorySizeRemaining = child.MemorySizeTotal;
                child.CalculatePhenotype();
                child.ReportPhenotypeChange();
                totalParentsResource -= priceToGetThisChildBrainSizePart + priceToGetThisChildGiftPart + priceDueToAge;
                child.resource =  WorldProperties.ChildStartingResourceSpendingsReceivedCoefficient * priceToGetThisChildBrainSizePart + priceToGetThisChildGiftPart;
                child.storyOfLife?.AppendFormat("Parents have given {0:f1} resource as a birthday gift.", child.resource).AppendLine();   
                if (priceDueToAge > 0) {
                    child.storyOfLife?.AppendFormat("Parents also wasted {0:f1} resources because they were old.", priceDueToAge).AppendLine();
                }
                PartnerB.resource = PartnerA.resource = totalParentsResource / 2;
                PartnerA.storyOfLife?.AppendFormat("Together with {0} have given birth to {1}. His brain size is {2:f1}. {3:f1} resources taken from each of the parents for birth. {4:f1} extra resources were taken to give to the child. {5:f1} extra resources were wasted due to age.", PartnerB.Name, child.Name, child.BrainSize, priceToGetThisChildBrainSizePart, WorldProperties.ChildStartingResourcePedestal + WorldProperties.ChildStartingResourceParentsCoefficient * totalParentsResource, reproductionCostIncrease[PartnerA.Age]).AppendLine();
                PartnerB.storyOfLife?.AppendFormat("Together with {0} have given birth to {1}. His brain size is {2:f1}. {3:f1} resources taken from each of the parents for birth. {4:f1} extra resources were taken to give to the child. {5:f1} extra resources were wasted due to age.", PartnerA.Name, child.Name, child.BrainSize, priceToGetThisChildBrainSizePart, WorldProperties.ChildStartingResourcePedestal + WorldProperties.ChildStartingResourceParentsCoefficient * totalParentsResource, reproductionCostIncrease[PartnerB.Age]).AppendLine();
                PartnerA.TeachChild(child, cachedList);
                PartnerB.TeachChild(child, cachedList);
                return child;
            }
            return null;
        }

        private void TeachChild(Tribesman child, List<Meme> cachedListForTeaching)
        {
            if (memesSet.memes.Count > 0)
            {
                memesSet.memes.ExcludeFromSortedList(child.memesSet.memes, cachedListForTeaching);
                for (int i = 0; i < WorldProperties.FreeTeachingRoundsForParents; i++)
                {
                    if (cachedListForTeaching.Count == 0)
                    {
                        storyOfLife?.AppendFormat("Tried to teach a child {0} something{2}, but he already knows everything his parent {1} can teach him.", child.Name, Name, i > 0 ? " else" : "").AppendLine();
                        return;
                    }
                    int memeIndexToTeach = randomizer.Next(cachedListForTeaching.Count);
                    Meme memeToTeach = cachedListForTeaching[memeIndexToTeach];
                    // А вот это вообще очень опасный копипейст, если я правильно понимаю.
                    double teachingSuccessChance = SupportFunctions.MultilpyProbabilities(
                        WorldProperties.FeatureDescriptions[(int)AvailableFeatures.TeachingEfficiency].Aggregator()
                            .Append(Phenotype.TeachingEfficiency)
                            .Append(child.Phenotype.StudyEfficiency)
                            .Value,
                        Math.Pow(1d / memeToTeach.ComplexityCoefficient, WorldProperties.MemeComplexityToLearningChanceCoefficient));
                    if (randomizer.Chance(teachingSuccessChance))
                    {
                        if (child.MemorySizeRemaining < memeToTeach.Price)
                        {
                            storyOfLife?.AppendFormat("Tried to teach child {0} {2} ({1}) and failed. {0} was too stupid to remember it.", child.Name, memeToTeach.SignatureString, memeToTeach.ActionDescription).AppendLine();
                        }
                        else
                        {
                            child.LearnNewMemeFrom(memeToTeach, this);
                            cachedListForTeaching.RemoveAt(memeIndexToTeach);
                            storyOfLife?.AppendFormat("Successfully taught child {0} {2} ({1}).", child.Name, memeToTeach.SignatureString, memeToTeach.ActionDescription).AppendLine();
                            UseMemeGroup(AvailableFeatures.TeachingEfficiency, "teaching child");
                            UseMemeGroup(AvailableFeatures.TeachingLikelyhood, "teaching child");
                            child.UseMemeGroup(AvailableFeatures.StudyEfficiency, "learning from parents");
                        }
                    }
                    else
                    {
                        storyOfLife?.AppendFormat("Tried to teach child {0} {3} ({1}) and failed. Chances were {2:2}%.", child.Name, memeToTeach.SignatureString, teachingSuccessChance * 100d, memeToTeach.ActionDescription).AppendLine();
                    }
                }
            }
        }

        public void ReportJoiningTribe(Tribe tribe)
        {
            storyOfLife?.AppendFormat("Joined the tribe {0}", tribe.TribeName).AppendLine();
        }

        public double BrainSize
        {
            get
            {
                double brainSize = WorldProperties.BrainSizePedestal;
                for (int i = 0; i < Features.Length; i++)
                    brainSize += WorldProperties.FeatureDescriptions[i].BrainSizeBoost * Genotype[i];
                return brainSize;
            }
        }

        /// <summary> Идентификатор уникальный только внутри одного племени. При вступлении в новое племя идентификатор тоже получается новый. </summary>
        public int TribeMemberId;
        public Tribe MyTribe { get => myTribe; set => myTribe = value; }
        public string MyTribeName => myTribe?.TribeName ?? "Unknow";

        public bool WantsToLeaveTribe()
        {
            if (randomizer.Chance(WorldProperties.MigrationChance))
            {
                storyOfLife?.Append("Decided to leave the tribe and search for a better life somewhere else.");
                return true;
            }
            return false;
        }

        public void PrepareForMigrationOutsideOfTheScope()
        {
            storyOfLife?.Append("Decided to migrate to South America. Goodbye.");
        }

        public void ReportEndOfYearStatistics()
        {
            if (myTribe.statistic.CollectThisYear != null)
            {
                myTribe.statistic.ReportAvgEvent(TribeStatistic.EventName.AverageMemesKnown, memesSet.memes.Count);
                myTribe.statistic.ReportAvgEvent(TribeStatistic.EventName.AverageResourcesPosessed, resource);
                if (WorldProperties.CollectPhenotypeValues > 0.5)
                    for (int i = 0; i < Features.Length; i++)
                        myTribe.statistic.ReportAvgEvent(TribeStatistic.EventName.AveragePhenotypeValue, (AvailableFeatures)i, Phenotype[i]);

                if (WorldProperties.CollectGenotypeValues > 0.5)
                    for (int i = 0; i < Features.Length; i++)
                        myTribe.statistic.ReportAvgEvent(TribeStatistic.EventName.AverageGenotypeValue, (AvailableFeatures)i, Genotype[i]);

                if (WorldProperties.CollectBrainUsagePercentages > 0.5) {
                    double totalBrainUsage = 0;
                    double totalBrainSize = 0;
                    Features brainUsages = default;
                    Features memesCount = default;
                    Features memesEffect = default;
                    for (int i = 0; i < memesSet.memes.Count; i++) {
                        var meme = memesSet.memes[i];
                        totalBrainUsage += meme.Price;
                        brainUsages[(int)meme.AffectedFeature] += meme.Price;
                        memesCount[(int)meme.AffectedFeature]++;
                        memesEffect[(int)meme.AffectedFeature] += meme.Efficiency;
                    }

                    totalBrainSize = totalBrainUsage + MemorySizeRemaining;
                    if (totalBrainSize > 0) {
                        myTribe.statistic.ReportAvgEvent(TribeStatistic.EventName.MemoryUnused,
                            MemorySizeRemaining / totalBrainSize);
                        for (int i = 0; i < Features.Length; i++)
                            if (WorldProperties.FeatureDescriptions[i].MemCanBeInvented) {
                                // if (brainUsages[i] > 0)
                                // нехорошо, конечно, отчитываться по заведомо ненужным величинам, но и убирать из статистики тех, кто не знает ни одного мема из данной категории тоже не годится.
                                // видимо нужен статический фильтр
                                myTribe.statistic.ReportAvgEvent(TribeStatistic.EventName.MemoryPercentageUsage, ((AvailableFeatures)i), brainUsages[i] / totalBrainSize);
                                myTribe.statistic.ReportAvgEvent(TribeStatistic.EventName.MemesSize, ((AvailableFeatures)i), memesCount[i] != 0 ? brainUsages[i] / memesCount[i] : 0);
                                myTribe.statistic.ReportAvgEvent(TribeStatistic.EventName.MemesRelativeEffectiveness, ((AvailableFeatures)i), memesCount[i] != 0 ? memesEffect[i] / brainUsages[i] : 0);
                            }
                    }
                }
            }
        }
    }
}

