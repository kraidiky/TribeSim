using DocumentFormat.OpenXml.Bibliography;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace TribeSim
{
    class Tribesman
    {
        private Random randomizer;
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
        public string GetJSONString ()
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

        private static Meme[] EmptyMemes = new Meme[0];
        private List<Meme> memes = new List<Meme>();
        private List<int> lastYearMemeWasUsed = new List<int>();
        private Meme[][] memesByFeature = new Meme[WorldProperties.FEATURES_COUNT][] { EmptyMemes, EmptyMemes, EmptyMemes, EmptyMemes, EmptyMemes, EmptyMemes, EmptyMemes, EmptyMemes, EmptyMemes, EmptyMemes, EmptyMemes, EmptyMemes, EmptyMemes, EmptyMemes, EmptyMemes, EmptyMemes };
        private double?[] memesEffect = new double?[WorldProperties.FEATURES_COUNT];

        public static double[] reproductionCostIncrease;

        private double BasicPriceToGetThisChild;
        private GeneCode genes = null;
        private double resource;
        private double totalResourcesCollected;

        private void AddMeme(Meme newMeme) {
            // memes у нас теперь будет отсортирован по возрастанию прайса мемов
            int index = memes.AddToSortedList(newMeme);
            lastYearMemeWasUsed.Insert(index, World.Year);

            int feature = (int)newMeme.AffectedFeature;
            memesEffect[feature] = null;
            MemorySizeRemaining -= newMeme.Price;
            var oldByFeatures = memesByFeature[feature];
            var newByFeatures = new Meme[oldByFeatures.Length + 1];
            memesByFeature[feature] = newByFeatures;
            newByFeatures[0] = newMeme;
            oldByFeatures.CopyTo(newByFeatures, 1);

        }
        private void RemoveMeme(Meme meme) {
            int feature = (int)meme.AffectedFeature;
            int index = memes.IndexOf(meme);
            memes.RemoveAt(index);
            lastYearMemeWasUsed.RemoveAt(index);
            memesEffect[feature] = null;
            MemorySizeRemaining += meme.Price;
            var oldByFeatures = memesByFeature[feature];
            if (oldByFeatures.Length == 1) {
                memesByFeature[feature] = EmptyMemes;
            } else {
                var newByFeatures = new Meme[oldByFeatures.Length - 1];
                memesByFeature[feature] = newByFeatures;
                for (int i = 0, j = 0; i < oldByFeatures.Length; i++)
                    if (oldByFeatures[i] != meme) {
                        newByFeatures[j] = oldByFeatures[i];
                        j++;
                    }
            }
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
            retval.genes = GeneCode.GenerateInitial(randomizer);
            retval.ReportPhenotypeChange();
            retval.storyOfLife?.Append(retval.Name).Append(" was created as a member of a statring set.").AppendLine();
            retval.yearBorn = World.Year;
            retval.resource = WorldProperties.StatringAmountOfResources;
            retval.BasicPriceToGetThisChild = retval.BrainSize * WorldProperties.BrainSizeBirthPriceCoefficient;
            retval.MemorySizeRemaining = retval.MemorySizeTotal = retval.GetFeature(AvailableFeatures.MemoryLimit) + retval.GetMemorySizeBoost();            
            return retval;
        }

        private double GetMemorySizeBoost()
        {
            double boost = 0;
            boost += this.GetFeature(AvailableFeatures.CooperationEfficiency) * WorldProperties.GeneticCooperationEfficiancyToMemoryRatio;
            boost += this.GetFeature(AvailableFeatures.Creativity) * WorldProperties.GeneticCreativityToMemoryRatio;
            boost += this.GetFeature(AvailableFeatures.FreeRiderDeterminationEfficiency) * WorldProperties.GeneticFreeRiderDeterminationEfficiencytoMemoryRatio;
            boost += this.GetFeature(AvailableFeatures.FreeRiderPunishmentLikelyhood) * WorldProperties.GeneticFreeRiderPunishmentLikelyhoodToMemoryRatio;
            boost += this.GetFeature(AvailableFeatures.HuntingEfficiency) * WorldProperties.GeneticHuntingEfficiencyToMemoryRatio;
            boost += this.GetFeature(AvailableFeatures.HuntingBEfficiency) * WorldProperties.GeneticHuntingBEfficiencyToMemoryRatio;
            boost += this.GetFeature(AvailableFeatures.LikelyhoodOfNotBeingAFreeRider) * WorldProperties.GeneticLikelyhoodOfNotBeingAFreeRiderToMemoryRatio;
            boost += this.GetFeature(AvailableFeatures.StudyEfficiency) * WorldProperties.GeneticStudyEfficiencyToMemoryRatio;
            boost += this.GetFeature(AvailableFeatures.StudyLikelyhood) * WorldProperties.GeneticStudyLikelyhoodToMemoryRatio;
            boost += this.GetFeature(AvailableFeatures.TeachingEfficiency) * WorldProperties.GeneticTeachingEfficiencyToMemoryRatio;
            boost += this.GetFeature(AvailableFeatures.TeachingLikelyhood) * WorldProperties.GeneticTeachingLikelyhoodToMemoryRatio;
            boost += this.GetFeature(AvailableFeatures.TrickEfficiency) * WorldProperties.GeneticTrickEfficiencyToMemoryRatio;
            boost += this.GetFeature(AvailableFeatures.TrickLikelyhood) * WorldProperties.GeneticTrickLikelyhoodToMemoryRatio;
            return boost;
        }

        public string GetPhenotypeString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (AvailableFeatures af in Enum.GetValues(typeof(AvailableFeatures)))
            {
                sb.AppendFormat("{0:f5}; ", GetFeature(af));
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

        public double GetFeature(AvailableFeatures af)
        {
            var effect = memesEffect[(int)af];
            if (effect.HasValue) {
                return effect.Value;
            } else {
                var relevantMemes = memesByFeature[(int)af];// .Where(assoc => assoc.Meme.AffectedFeature == af);
                var retval = genes[af];
                var description = WorldProperties.FeatureDescriptions[(int)af];        
                if (af == AvailableFeatures.AgeingRate) { 
                    // AR надо считать вообще по-другому! тут вероятностная алгебра не работает, потому что этот показатель в принципе не вероятность!
                    // Надо следить за коммутативностью, сохраняя при этом значение выше нуля.
                    // Может ответ в логарифмической шкале? Надо думать. Пока не придумалось ничего лучше, чем обрезать после вычислений. 
                    // Это должно углубить потенциальную яму нулевого AR, но придумать что-то более элегантное мозгов пока не хватило.
                    foreach (Meme meme in relevantMemes) {
                        retval += meme.Efficiency - meme.Efficiency * retval;                        
                    }
                    if (retval<0) {
                        retval = 0;
                    }
                } else {
                    if (description.range == FeatureRange.ZeroToOne) { // 0..1 features.
                        foreach (Meme meme in relevantMemes)
                                retval += meme.Efficiency - meme.Efficiency * retval;                        
                    } else { //0..infinity features.
                        foreach (Meme meme in relevantMemes)
                            retval += meme.Efficiency;
                    }
                }
                memesEffect[(int)af] = retval;
                return retval;
            }
        }

        public void ConsumeLifeSupport() {
            double lifeSupportCosts = WorldProperties.LifeSupportCosts;
            if (UseGompertzAgeing) {
                double ARdifference = GetFeature(AvailableFeatures.AgeingRate) - WorldProperties.BaseGompertzAgeingRate;
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

            double creativity = this.GetFeature(AvailableFeatures.Creativity);
            if (randomizer.Chance(creativity))
            {
                AvailableFeatures af = (AvailableFeatures)(WorldProperties.MemesWhichCanBeInvented[randomizer.Next(WorldProperties.MemesWhichCanBeInvented.Length)]);

                if (InventNewMemeForAFeature(af, out Meme inventedMeme))
                {
                    storyOfLife?.AppendFormat("Invented how {5}. ({0} meme with {1} effect). Complexity is {2:f2} and {3} now has {4:f2} memory left. (Meme#{5})", af.GetDescription(), inventedMeme.Efficiency, inventedMeme.Price, Name, MemorySizeRemaining, inventedMeme.MemeId, inventedMeme.ActionDescription).AppendLine();
                    ReportPhenotypeChange();
                }
                else
                {
                    storyOfLife?.AppendFormat("Invented the new meme and forgot it immediately because {0} is too stupid to remember anything else. Memory size is {1:f2} and meme complexity is {2:f2}", Name, MemorySizeRemaining, inventedMeme.Price).AppendLine();
                }
            }
        }

        private bool InventNewMemeForAFeature(AvailableFeatures featureTheMemeWillAffect, out Meme createdMeme)
        {
            Meme newMeme = Meme.InventNewMeme(randomizer, featureTheMemeWillAffect);
            StatisticsCollector.ReportCountEvent(this.MyTribeName, "Meme Invented");
            createdMeme = newMeme;
            if (newMeme.Price < MemorySizeRemaining)
            {
                AddMeme(newMeme);
                newMeme.ReportInvented(this);                
                return true;
            }
            else
            {                
                return false;
            }
        }

        public void ForgetUnusedMemes()
        {
            int year = World.Year;
            for (int i = memes.Count - 1; i >= 0; i--) {
                Meme meme = memes[i];
                if (year - lastYearMemeWasUsed[i] > WorldProperties.DontForgetMemesThatWereUsedDuringThisPeriod)
                {
                    if (randomizer.Chance(WorldProperties.ChanceToForgetTheUnusedMeme))
                    {
                        meme.ReportForgotten(this);
                        storyOfLife?.AppendFormat("Forgotten how {1} ({0})", meme.SignatureString, meme.ActionDescription).AppendLine();
                        RemoveMeme(meme);
                        ReportPhenotypeChange();                                                
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

        public IReadOnlyList<Meme> knownMemes => memes;

        public bool TryToTeach(Tribesman student, bool isCulturalExchange=false)
        {
            if (memes.Count > 0 && randomizer.Chance(this.GetFeature(AvailableFeatures.TeachingLikelyhood)))
            {
                if (resource >= WorldProperties.TeachingCosts || randomizer.Chance(WorldProperties.ChanceToTeachIfUnsufficienResources))
                {
                    resource -= WorldProperties.TeachingCosts;
                    List<Meme> memeAssoc = memes.Where(meme => !student.knownMemes.Contains(meme)).ToList();
                    if (memeAssoc.Count == 0)
                    {
                        storyOfLife?.AppendFormat("{3}Tried to teach {0} something, but he already knows everything {1} can teach him. {2} resources wasted.", student.Name, Name, WorldProperties.TeachingCosts,isCulturalExchange?"Cultural Exchange! ":"").AppendLine();
                        return false;
                    }
                    Meme memeToTeach = memeAssoc[randomizer.Next(memeAssoc.Count)];
                    double teachingSuccessChance = SupportFunctions.MultilpyProbabilities(
                        SupportFunctions.SumProbabilities(
                            this.GetFeature(AvailableFeatures.TeachingEfficiency),
                            student.GetFeature(AvailableFeatures.StudyEfficiency)),
                        Math.Pow(1d / memeToTeach.ComplexityCoefficient, WorldProperties.MemeComplexityToLearningChanceCoefficient));
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

        private void UseMemeGroup(AvailableFeatures usedFeature, string activity = "engaged in unknown activity")
        {
            FeatureDescription description = WorldProperties.FeatureDescriptions[(int)usedFeature];
            if (!description.MemCanBeInvented)
                return;

            double C = GetFeature(AvailableFeatures.Creativity);
            double M = WorldProperties.ChanceToInventNewMemeWhileUsingItModifier;
            double T = WorldProperties.ChanceToInventNewMemeWhileUsingItThreshold;
            double chanceToInventNewMeme = T + M * C - T * M * C;
            
            if (randomizer.Chance(chanceToInventNewMeme))
            {
                Meme inventedMeme;
                
                if (InventNewMemeForAFeature(usedFeature, out inventedMeme))
                {
                    storyOfLife?.AppendFormat("While {6} invented how {7}. ({0} meme with {1} effect) The meme complexity is {2:f2} and {3} now has {4:f2} memory left. ({5})", usedFeature.GetDescription(), inventedMeme.Efficiency, inventedMeme.Price, Name, MemorySizeRemaining, inventedMeme.SignatureString, activity, inventedMeme.ActionDescription).AppendLine();
                    ReportPhenotypeChange();
                }
                else
                {
                    storyOfLife?.AppendFormat("While {3} invented the new meme but forgot it immediately. Too stupid to remember anything else. Memory size is {1:f2} and meme complexity is {2:f2}", Name, MemorySizeRemaining, inventedMeme.Price, activity).AppendLine();
                }
            }
            for (int i = 0; i < memes.Count; i++)
                if (memes[i].AffectedFeature == usedFeature) { // Использую пока такую менее эффективную схему чтобы rnd-шники при рефакторинге не поплыли. В дальнейшем lastYearMemeWasUsed можно будет хранить сразу по группам.
                    lastYearMemeWasUsed[i] = World.Year;
                    myTribe.MemeUsed(this, memes[i]); // Тот, кто не в племени не может пользовать мемы.
                }
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
            ReportPhenotypeChange();
        }

        public void DetermineAndPunishAFreeRider(List<Tribesman> members, List<Tribesman> freeRidersList)
        {
            if (randomizer.Chance(GetFeature(AvailableFeatures.FreeRiderPunishmentLikelyhood)))
            {
                resource -= WorldProperties.FreeRiderPunishmentCosts;
                if (randomizer.Chance(GetFeature(AvailableFeatures.FreeRiderDeterminationEfficiency)))
                {
                    // Punish a free rider

                    int rand = randomizer.Next(freeRidersList.Count);
                    Tribesman FreeRiderToBePunished = freeRidersList[rand];
                    while (FreeRiderToBePunished == this)
                    {
                        if (freeRidersList.Count == 1)
                        {
                            storyOfLife?.Append("Successfully determined himself to be the only free rider in a tribe.");
                            Meme inventedMeme2;
                            if ((WorldProperties.NewMemeLikelyhoodOfNotBeingAFreeRiderMean > 0.0001 || WorldProperties.NewMemeLikelyhoodOfNotBeingAFreeRiderMean < -0.0001) && WorldProperties.NewMemeLikelyhoodOfNotBeingAFreeRiderStdDev > 0.0001)
                            {
                                if (InventNewMemeForAFeature(AvailableFeatures.LikelyhoodOfNotBeingAFreeRider, out inventedMeme2))
                                {
                                    storyOfLife?.AppendFormat("Learned {1} ({0}) as a result of a guilty conscience.", inventedMeme2.SignatureString, inventedMeme2.ActionDescription).AppendLine();
                                    ReportPhenotypeChange();
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
                        FreeRiderToBePunished.ReportPhenotypeChange();
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

        public double GoHunting(AvailableFeatures huntingEfficiencyFeature)
        {
            var huntingCosts = huntingEfficiencyFeature == AvailableFeatures.HuntingEfficiency ? WorldProperties.HuntingCosts : WorldProperties.HuntingBCosts;
            if (resource <= huntingCosts)
            {
                storyOfLife?.AppendFormat("Wanted to go hunting, but could not. Too hungry. Resources remaining - {0:f2}, needed for the hunt - {1}.", resource, huntingEfficiencyFeature).AppendLine();
                return 0;
            }
            resource -= huntingCosts;
            storyOfLife?.AppendFormat("Went hunting with his friends. He hunted with {0:f1} efficiency and cooperated at {1:f2}. Spent {2} resource for the hunt, {3:f2} remaining.", GetFeature(huntingEfficiencyFeature), GetFeature(AvailableFeatures.CooperationEfficiency), huntingCosts, resource).AppendLine();
            UseMemeGroup(huntingEfficiencyFeature, "hunting");
            UseMemeGroup(AvailableFeatures.LikelyhoodOfNotBeingAFreeRider, "hunting");
            UseMemeGroup(AvailableFeatures.CooperationEfficiency, "hunting");            
            return GetFeature(huntingEfficiencyFeature);            
        }

        public void SkipHunting()
        {
            storyOfLife?.AppendFormat("Did not go hunting with the tribe. He told the others that {0}.", NamesGenerator.GenerateLameExcuse()).AppendLine();
        }

        public double TellHowMuchDoYouWant(double resourcesReceivedPerGroup)
        {
            double requestedShare = 1;
            if (randomizer.Chance(GetFeature(AvailableFeatures.TrickLikelyhood)))
            {
                requestedShare += GetFeature(AvailableFeatures.TrickEfficiency);
                storyOfLife?.AppendFormat("Group returned from the hunt with {0:f0} resources. He decided to play a trick and asked to get {1:f1}% more then the rest.", resourcesReceivedPerGroup, (requestedShare-1)*100).AppendLine();
                UseMemeGroup(AvailableFeatures.TrickEfficiency, "sharing resources");
                UseMemeGroup(AvailableFeatures.TrickLikelyhood, "sharing resources");
            }
            else
            {
                storyOfLife?.AppendFormat("Group returned from the hunt with {0:f0} resources. He asked for a fare share.", resourcesReceivedPerGroup).AppendLine();
            }
            return requestedShare;
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

        public void PerformUselessActions()
        {
            int uselessActionsMade = 0;
            double resourcesWasted = 0;
            var chance = GetFeature(AvailableFeatures.UselessActionsLikelihood);
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

                Meme[] relevantMemes = memesByFeature[(int)AvailableFeatures.UselessActionsLikelihood];
                if (relevantMemes.Length > 0) {
                    Meme randomKnownUselessMeme = relevantMemes[randomizer.Next(relevantMemes.Length)];

                    storyOfLife?.AppendFormat("Decided {0}. Didn't gain anything from it, but wasted {1:f2} resources on it. {2:f2} remaining.", randomKnownUselessMeme.ActionDescription, resourcesWasted, resource).AppendLine();
                    UseMemeGroup(AvailableFeatures.UselessActionsLikelihood, "performing useless actions");
                } else {
                    storyOfLife?.AppendFormat($"Geneticaly based useless actions. Chance: {chance}, but wasted {resourcesWasted:f2} resources on it. {resource:f2} remaining.").AppendLine();
                }
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
            memesUsedThisYear.ExcludeFromSortedList(memes, memesAvailableForStudy);

            /// Всё что может быть выучено отсортировали, можно начинать.
            while (memesAvailableForStudy.Count > 0 && randomizer.Chance(GetFeature(AvailableFeatures.StudyLikelyhood))) // Первый шаг оптимизации: Простое изменение порядка следования проверок сокращает время с 24 до 17
            {
                if (resource <= WorldProperties.StudyCosts)
                {
                    storyOfLife?.AppendLine("Wanted to learn something new but couldn't. Too hungry.");
                    break;
                }
                // Тут есть три возможные причины перехода к следующему элементу цикла - пререквизиты, шанс выучить и недостаток памяти. Наерняка можно сильно съэкономить если расположить их в правильном порядке.
                Meme memeToStudy = memesAvailableForStudy[randomizer.Next(memesAvailableForStudy.Count)];
                resource -= WorldProperties.StudyCosts;
                if (randomizer.Chance(
                    SupportFunctions.MultilpyProbabilities(
                        GetFeature(AvailableFeatures.StudyEfficiency),
                        Math.Pow(1d / memeToStudy.ComplexityCoefficient, WorldProperties.MemeComplexityToLearningChanceCoefficient))))
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
                    chanceOfDeathOfOldAge = WorldProperties.GompertzAgeingBasicMortalityRate * Math.Exp(GetFeature(AvailableFeatures.AgeingRate)*(age - WorldProperties.GompertzAgeingAgeAtWhichAgeingStarts));
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
                StatisticsCollector.ReportCountEvent(MyTribeName, "Deaths of old age");
                return true;
            }

            if (resource <= 0)
            {
                ReportOnDeathStatistics(age,"hunger");
                ForgetAllMemes();
                storyOfLife?.AppendFormat("Died of hunger at the age of {0}.", age).AppendLine();
                StatisticsCollector.ReportCountEvent(MyTribeName, "Deaths of hunger");                
                return true;
            }
            return false;
        }

        private void ReportOnDeathStatistics(int age, string cause) {
            StatisticsCollector.ReportAverageEvent(MyTribeName, "Longevity", age);
            double totalBrainUsage = 0;
            double totalBrainSize = 0;
            for (int i = 0; i < memes.Count; i++) {
                totalBrainUsage += memes[i].Price;
            }
            totalBrainSize = totalBrainUsage + MemorySizeRemaining;
            if (totalBrainSize > 0)
            {
                StatisticsCollector.ReportAverageEvent(MyTribeName, "% memory: unused when died", MemorySizeRemaining / totalBrainSize);
            }
            // Подбиваем подробную статистику по репродуктивному успеху.
            if (WorldProperties.CollectIndividualSuccess > 0.5) {
                if (randomizer.Chance(WorldProperties.ChanceToCollectIndividualSuccess)) {
                    var individualSucess = new IndividualSucess()
                    {
                        birthYear = yearBorn,
                        age = World.Year - yearBorn,
                        children = childrenCount,
                        totalResourcesCollected = (float)totalResourcesCollected,
                        tribeName = $"{myTribe.TribeName}-{myTribe.id}",
                        deathCause = cause
                    };
                    if (WorldProperties.CollectIndividualGenotypeValues > 0.5)
                    {
                        individualSucess.genotype = FeaturesFloatArray.Get();
                        for (int i = 0; i < WorldProperties.FEATURES_COUNT; i++)
                            individualSucess.genotype[i] = (float)genes[i];
                    }
                    if (WorldProperties.CollectIndividualPhenotypeValues > 0.5)
                    {
                        individualSucess.phenotype = FeaturesFloatArray.Get();
                        for (int i = 0; i < WorldProperties.FEATURES_COUNT; i++)
                            individualSucess.phenotype[i] = (float)GetFeature((AvailableFeatures)i);
                    }
                    StatisticsCollector.ReportEvent(individualSucess);
                }
            }
        }

        public struct IndividualSucess : IDetaliedEvent
        {
            public static string[] genotypeHeader;
            public static string[] phenotypeHeader;

            public int birthYear;
            public int age;
            public int children;
            public float totalResourcesCollected;
            public string deathCause;
            public string tribeName;
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
                if (genotype != null)
                    for (int i = 0; i < WorldProperties.FEATURES_COUNT; i++)
                        sb.Append(",gen.").Append(((AvailableFeatures)i).GetAcronym());
                if (phenotype != null)
                    for (int i = 0; i < WorldProperties.FEATURES_COUNT; i++)
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
                if (genotype != null) {
                    for (int i = 0; i < WorldProperties.FEATURES_COUNT; i++)
                        sb.Append(',').Append(genotype[i].ToString("F3", CultureInfo.InvariantCulture));
                }
                if (phenotype != null) {
                    for (int i = 0; i < WorldProperties.FEATURES_COUNT; i++)
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

        private void ForgetAllMemes()
        {
            for (int i = memes.Count - 1; i >= 0; i--) {
                memes[i].ReportForgotten(this);                
                RemoveMeme(memes[i]);
            }
        }

        public void DieOfLonliness()
        {
            int age = World.Year - yearBorn;
            ReportOnDeathStatistics(age, "lonliness");
            ForgetAllMemes();            
            storyOfLife?.AppendFormat("Died of lonliness at the age of {0}. Remained the only one in the tribe.", age).AppendLine();            
        }

        public void Release()
        {
            genes.Release();
        }


        public bool IsOfReproductionAge { get { return Age >= WorldProperties.DontBreedIfYoungerThan && (WorldProperties.MaximumBreedingAge < 0.5 || Age <= WorldProperties.MaximumBreedingAge); } }
        public int Age { get {return World.Year-yearBorn;} }

        public static Tribesman Breed(Random randomizer, Tribesman PartnerA, Tribesman PartnerB, List<Meme> cachedList)
        {
            double totalParentsResource = PartnerA.resource + PartnerB.resource;

            if (totalParentsResource * 5 < 2*(PartnerA.BasicPriceToGetThisChild + PartnerB.BasicPriceToGetThisChild)) // Проверка на вшивость. Если у них вдвоём не набирается даже 2/3 от того, что они сами стоили незачем и начинать.
                return null;

            Tribesman child = new Tribesman(randomizer);
            child.genes = GeneCode.GenerateFrom(randomizer, PartnerA.genes, PartnerB.genes);

            double priceToGetThisChildBrainSizePart = child.BrainSize * WorldProperties.BrainSizeBirthPriceCoefficient;
            double priceToGetThisChildGiftPart = WorldProperties.ChildStartingResourcePedestal + WorldProperties.ChildStartingResourceParentsCoefficient * (totalParentsResource - priceToGetThisChildBrainSizePart);
            double priceDueToAge = 0;
            if (WorldProperties.BreedingCostsIncreaseCoefficient > 0) {
                priceDueToAge = reproductionCostIncrease[PartnerA.Age] + reproductionCostIncrease[PartnerB.Age];
            }
            child.BasicPriceToGetThisChild = priceToGetThisChildBrainSizePart + WorldProperties.ChildStartingResourcePedestal; // Записываем только минимально необходимую часть ресурса, пошедшую на мозг и родительский бонус. Наследство может быть большим, маленьким или вообще нулевым.

            if (totalParentsResource > priceToGetThisChildBrainSizePart + priceToGetThisChildGiftPart + priceDueToAge)
            {
                StatisticsCollector.ReportCountEvent(PartnerA.MyTribeName, "Child births");
                StatisticsCollector.ReportAverageEvent(PartnerA.MyTribeName, "Child average brain size", child.BrainSize);
                child.yearBorn = World.Year;
                if (randomizer.Chance(WorldProperties.ChancesToWriteALog))
                    child.KeepsDiary();
                child.ReportPhenotypeChange();
                child.myTribe = PartnerA.myTribe;
                child.storyOfLife?.AppendFormat("Was born from {0} and {1}. His brain size is {2:f1}. His parents spent {3:f1} resources to raise him.", PartnerA.Name, PartnerB.Name, child.BrainSize, priceToGetThisChildBrainSizePart).AppendLine();
                child.MemorySizeRemaining = child.MemorySizeTotal = child.GetFeature(AvailableFeatures.MemoryLimit);
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
            child.genes.Release();
            return null;
        }

        private void TeachChild(Tribesman child, List<Meme> cachedListForTeaching)
        {
            if (memes.Count > 0)
            {
                memes.ExcludeFromSortedList(child.memes, cachedListForTeaching);
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
                        SupportFunctions.SumProbabilities(
                            this.GetFeature(AvailableFeatures.TeachingEfficiency),
                            child.GetFeature(AvailableFeatures.StudyEfficiency)),
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
                return WorldProperties.BrainSizePedestal +
                    genes[AvailableFeatures.TrickEfficiency] * WorldProperties.BrainSizeToTrickEfficiencyCoefficient +
                    genes[AvailableFeatures.TeachingEfficiency] * WorldProperties.BrainSizeToTeachingEfficiencyCoefficient +
                    genes[AvailableFeatures.StudyEfficiency] * WorldProperties.BrainSizeToStudyEfficiencyCoefficient +
                    genes[AvailableFeatures.FreeRiderDeterminationEfficiency] * WorldProperties.BrainSizeToFreeRiderDeterminationEfficiencyCoefficient +
                    genes[AvailableFeatures.HuntingEfficiency] * WorldProperties.BrainSizeToHuntingEfficiencyCoefficient +
                    genes[AvailableFeatures.HuntingBEfficiency] * WorldProperties.BrainSizeToHuntingBEfficiencyCoefficient +
                    genes[AvailableFeatures.CooperationEfficiency] * WorldProperties.BrainSizeToCooperationEfficiencyCoefficient +
                    genes[AvailableFeatures.MemoryLimit] * WorldProperties.BrainSizeToMemorySizeCoefficient +
                    genes[AvailableFeatures.Creativity] * WorldProperties.BrainSizeToCreativityCoefficient;
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
            StatisticsCollector.ReportAverageEvent(MyTribeName, "Average memes known", memes.Count);
            StatisticsCollector.ReportAverageEvent(MyTribeName, "Average resources posessed", resource);
            if (WorldProperties.CollectPhenotypeValues > 0.5)
            {
                foreach (AvailableFeatures af in Enum.GetValues(typeof(AvailableFeatures)))
                {
                    StatisticsCollector.ReportAverageEvent(MyTribeName, string.Concat("Avg. phenotype value (", af.GetDescription(), ")"), GetFeature(af));
                }
            }
            if (WorldProperties.CollectGenotypeValues > 0.5)
            {
                foreach (AvailableFeatures af in Enum.GetValues(typeof(AvailableFeatures)))
                {
                    StatisticsCollector.ReportAverageEvent(MyTribeName, string.Concat("Avg. genotype value (", af.GetDescription(), ")"), genes[af]);
                }
            }
            if (WorldProperties.CollectBrainUsagePercentages >0.5)
            {
                double totalBrainUsage = 0;
                double totalBrainSize = 0;
                var brainUsages = FeatureSet.Blank();
                for (int i = 0; i < memes.Count; i++) {
                    var meme = memes[i];
                    totalBrainUsage += meme.Price;
                    brainUsages[(int)meme.AffectedFeature] += meme.Price;
                }
                totalBrainSize = totalBrainUsage + MemorySizeRemaining;
                if (totalBrainSize > 0)
                {
                    StatisticsCollector.ReportAverageEvent(MyTribeName, "% memory: unused", MemorySizeRemaining / totalBrainSize);
                    for (int i = 0; i < brainUsages.Length; i++)
                        // if (brainUsages[i] > 0)
                        // нехорошо, конечно, отчитываться по заведомо ненужным величинам, но и убирать из статистики тех, кто не знает ни одного мема из данной категории тоже не годится.
                        // видимо нужен статический фильтр
                        StatisticsCollector.ReportAverageEvent(MyTribeName, string.Format("% memory: {0}", ((AvailableFeatures)i).ToString()), brainUsages[i] / totalBrainSize);
                }
            }
        }
    }
}
