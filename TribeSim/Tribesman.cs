using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TribeSim
{
    class Tribesman
    {
        private bool keepsDiary = false;
        private StringBuilder storyOfLife = new StringBuilder();
        private StringBuilder storyOfPhenotypeChanges = new StringBuilder();
        private double MemorySizeRemaining = 0;
        private string name = null;
        private int yearBorn = 0;
        private string myTribeName = "Unknown";

        public event EventHandler<Meme> MemeUsed;

        public int YearBorn
        {
            get { return yearBorn; }            
        }

        /// <summary>
        /// Returns a JSON string like 
        /// {"name":"Tikh-Ro", "memes": [1, 3]}
        /// </summary>
        /// <returns>Ex: {"name":"Tikh-Ro", "memes": [1, 3]}</returns>
        public string GetJSONString ()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{\"name\":{0}, \"memes\": [", Name );
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
            return storyOfPhenotypeChanges.ToString() + Environment.NewLine + storyOfLife.ToString();
        }

        private void ReportEvent(string description)
        {
            if (keepsDiary)
            {
                storyOfLife.AppendFormat("Year {0}: {1}", World.Year, description);
                storyOfLife.AppendLine();
            }
        }
        private void ReportPhenotypeChange()
        {
            if (keepsDiary)
            {
                if (storyOfPhenotypeChanges.Length < 3)
                {
                    storyOfPhenotypeChanges.AppendLine(GetPhenotypeHeaders());
                }
                storyOfPhenotypeChanges.AppendFormat("Year {0}: {1}", World.Year, GetPhenotypeString());
                storyOfPhenotypeChanges.AppendLine();
            }
        }

        private HashSet<TribesmanToMemeAssociation> memes = new HashSet<TribesmanToMemeAssociation>();
        private GeneCode genes = null;
        private double resource;
        private double Resource
        {
            get { return resource; }
            set { resource = value; }
        }

        private Tribesman()
        {
            keepsDiary = SupportFunctions.Chance(WorldProperties.ChancesToWriteALog);
        }

        public static Tribesman GenerateTribesmanFromAStartingSet()
        {            
            Tribesman retval = new Tribesman();
            retval.keepsDiary = SupportFunctions.Chance(WorldProperties.ChancesToWriteALog);
            retval.genes = GeneCode.GenerateInitial();
            retval.ReportPhenotypeChange();
            retval.ReportEvent(retval.Name + " was created as a member of a statring set.");
            retval.yearBorn = World.Year;
            retval.resource = WorldProperties.StatringAmountOfResources;
            retval.MemorySizeRemaining = retval.GetFeature(AvailableFeatures.MemoryLimit) + retval.GetMemorySizeBoost();            
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

        public double GetFeature(AvailableFeatures? af)
        {
            double retval = double.NaN;
            if (af.HasValue)
            {                
                HashSet<TribesmanToMemeAssociation> relevantMemes = new HashSet<TribesmanToMemeAssociation>(memes.Where(assoc => assoc.Meme.AffectedFeature == af));
                retval = genes[af.Value];
                switch (af)
                {
                    case AvailableFeatures.TrickLikelyhood: // 0..1 features.                                         
                    case AvailableFeatures.TeachingLikelyhood:
                    case AvailableFeatures.TeachingEfficiency:
                    case AvailableFeatures.StudyLikelyhood:
                    case AvailableFeatures.StudyEfficiency:
                    case AvailableFeatures.FreeRiderPunishmentLikelyhood:
                    case AvailableFeatures.FreeRiderDeterminationEfficiency:
                    case AvailableFeatures.LikelyhoodOfNotBeingAFreeRider:                        
                        foreach (TribesmanToMemeAssociation assoc in relevantMemes)
                        {
                            retval += assoc.Meme.Efficiency  - assoc.Meme.Efficiency * retval;
                        }
                        break;
                    case AvailableFeatures.HuntingEfficiency:   //0..infinity features.                    
                    case AvailableFeatures.TrickEfficiency:
                    case AvailableFeatures.CooperationEfficiency:
                        foreach (TribesmanToMemeAssociation assoc in relevantMemes)
                        {
                            retval += assoc.Meme.Efficiency;
                        }
                        break;
                }                
            }
            else
            {
                retval = 0;
                HashSet<TribesmanToMemeAssociation> relevantMemes = new HashSet<TribesmanToMemeAssociation>(memes.Where(assoc => assoc.Meme.AffectedFeature.HasValue == false));
                foreach (TribesmanToMemeAssociation assoc in relevantMemes)
                {
                    retval += assoc.Meme.Efficiency - assoc.Meme.Efficiency * retval;
                }
            }
            return retval;
        }

        public void ConsumeLifeSupport()
        {
            resource -= WorldProperties.LifeSupportCosts;
            ReportEvent("Eaten " + WorldProperties.LifeSupportCosts + " resources. " + resource.ToString("f2") + " left.");
        }

        public void TryInventMemeSpontaneously()
        {
            double creativity = this.GetFeature(AvailableFeatures.Creativity);
            if (SupportFunctions.Chance(creativity))
            {
                // Exit if no memes can be created
                if (WorldProperties.NewMemeCooperationEfficiencyMean==0 && WorldProperties.NewMemeCooperationEfficiencyStdDev==0 && WorldProperties.NewMemeFreeRiderDeterminationEfficiencyMean==0 && WorldProperties.NewMemeFreeRiderDeterminationEfficiencyStdDev==0 && WorldProperties.NewMemeFreeRiderPunishmentLikelyhoodMean==0 && WorldProperties.NewMemeFreeRiderPunishmentLikelyhoodStdDev==0 && WorldProperties.NewMemeHuntingEfficiencyMean ==0 && WorldProperties.NewMemeHuntingEfficiencyStdDev==0 && WorldProperties.NewMemeLikelyhoodOfNotBeingAFreeRiderMean==0 && WorldProperties.NewMemeLikelyhoodOfNotBeingAFreeRiderStdDev==0 && WorldProperties.NewMemeStudyEfficiencyMean==0 && WorldProperties.NewMemeStudyEfficiencyStdDev==0 && WorldProperties.NewMemeStudyLikelyhoodMean==0 && WorldProperties.NewMemeStudyLikelyhoodStdDev==0 && WorldProperties.NewMemeTeachingEfficiencyMean==0 && WorldProperties.NewMemeTeachingEfficiencyStdDev==0 && WorldProperties.NewMemeTeachingLikelyhoodMean==0 && WorldProperties.NewMemeTeachingLikelyhoodStdDev==0 && WorldProperties.NewMemeTrickEfficiencyMean==0 && WorldProperties.NewMemeTrickEfficiencyStdDev==0 && WorldProperties.NewMemeTrickLikelyhoodMean==0 && WorldProperties.NewMemeTrickLikelyhoodStdDev ==0 && WorldProperties.NewMemeUselessEfficiencyMean==0 && WorldProperties.NewMemeUselessEfficiencyStdDev==0)
                {
                    return;
                }
                int memeType = -1;
                AvailableFeatures? af = null;

                while (memeType == -1)
                {
                    memeType = SupportFunctions.UniformRandomInt(1, 13);                    
                    switch (memeType)
                    {
                        case 1:
                            af = AvailableFeatures.TrickLikelyhood;
                            if (WorldProperties.NewMemeTrickLikelyhoodMean == 0 && WorldProperties.NewMemeTrickLikelyhoodStdDev == 0) memeType = -1;
                            break;
                        case 2:
                            af = AvailableFeatures.TrickEfficiency;
                            if (WorldProperties.NewMemeTrickEfficiencyMean == 0 && WorldProperties.NewMemeTrickEfficiencyStdDev == 0) memeType = -1;
                            break;
                        case 3:
                            af = AvailableFeatures.TeachingLikelyhood;
                            if (WorldProperties.NewMemeTeachingLikelyhoodMean == 0 && WorldProperties.NewMemeTeachingLikelyhoodStdDev == 0) memeType = -1;
                            break;
                        case 4:
                            af = AvailableFeatures.TeachingEfficiency;
                            if (WorldProperties.NewMemeTeachingEfficiencyMean == 0 && WorldProperties.NewMemeTeachingEfficiencyStdDev == 0) memeType = -1;
                            break;
                        case 5:
                            af = AvailableFeatures.StudyLikelyhood;
                            if (WorldProperties.NewMemeStudyLikelyhoodMean == 0 && WorldProperties.NewMemeStudyEfficiencyStdDev == 0) memeType = -1;
                            break;
                        case 6:
                            af = AvailableFeatures.StudyEfficiency;
                            if (WorldProperties.NewMemeStudyEfficiencyMean == 0 && WorldProperties.NewMemeStudyEfficiencyStdDev == 0) memeType = -1;
                            break;
                        case 7:
                            af = AvailableFeatures.FreeRiderPunishmentLikelyhood;
                            if (WorldProperties.NewMemeFreeRiderPunishmentLikelyhoodMean == 0 && WorldProperties.NewMemeFreeRiderPunishmentLikelyhoodStdDev == 0) memeType = -1;
                            break;
                        case 8:
                            af = AvailableFeatures.FreeRiderDeterminationEfficiency;
                            if (WorldProperties.NewMemeFreeRiderDeterminationEfficiencyMean == 0 && WorldProperties.NewMemeFreeRiderDeterminationEfficiencyStdDev == 0) memeType = -1;
                            break;
                        case 9:
                            af = AvailableFeatures.LikelyhoodOfNotBeingAFreeRider;
                            if (WorldProperties.NewMemeLikelyhoodOfNotBeingAFreeRiderMean == 0 && WorldProperties.NewMemeLikelyhoodOfNotBeingAFreeRiderStdDev == 0) memeType = -1;
                            break;
                        case 10:
                            af = AvailableFeatures.HuntingEfficiency;
                            if (WorldProperties.NewMemeHuntingEfficiencyMean == 0 && WorldProperties.NewMemeHuntingEfficiencyStdDev == 0) memeType = -1;
                            break;
                        case 11:
                            af = AvailableFeatures.CooperationEfficiency;
                            if (WorldProperties.NewMemeCooperationEfficiencyMean == 0 && WorldProperties.NewMemeCooperationEfficiencyStdDev == 0) memeType = -1;
                            break;
                        case 12:
                            af = null;
                            if (WorldProperties.NewMemeUselessEfficiencyMean == 0 && WorldProperties.NewMemeUselessEfficiencyStdDev == 0) memeType = -1;
                            break;
                    }
                }
                Meme inventedMeme;
                
                if (InventNewMemeForAFeature(af, out inventedMeme))
                {
                    ReportEvent(string.Format("Invented how {5}. ({0} meme with {1} effect). Complexity is {2:f2} and {3} now has {4:f2} memory left. (Meme#{5})", af.HasValue ? af.Value.GetDescription() : "Nothing", inventedMeme.Efficiency, inventedMeme.Price, Name, MemorySizeRemaining, inventedMeme.MemeId, inventedMeme.ActionDescription));
                    ReportPhenotypeChange();
                }
                else
                {
                    ReportEvent(string.Format("Invented the new meme and forgot it immediately because {0} is too stupid to remember anything else. Memory size is {1:f2} and meme complexity is {2:f2}", Name, MemorySizeRemaining, inventedMeme.Price));
                }
            }
        }

        private bool InventNewMemeForAFeature(AvailableFeatures? featureTheMemeWillAffect, out Meme createdMeme)
        {
            Meme newMeme = Meme.InventNewMeme(featureTheMemeWillAffect);
            StatisticsCollector.ReportCountEvent(this.MyTribeName, "Meme Invented");
            createdMeme = newMeme;
            if (newMeme.Price < MemorySizeRemaining)
            {
                TribesmanToMemeAssociation tma = TribesmanToMemeAssociation.Create(this, newMeme);
                memes.Add(tma);
                MemorySizeRemaining -= newMeme.Price;
                tma.Meme.ReportInvented(this);                
                return true;
            }
            else
            {                
                return false;
            }
        }

        public void ForgetUnusedMemes()
        {
            foreach (TribesmanToMemeAssociation assoc in memes.ToList())
            {
                if (assoc.TurnsSinceLastUsed > WorldProperties.DontForgetMemesThatWereUsedDuringThisPeriod)
                {
                    if (SupportFunctions.Chance(WorldProperties.ChanceToForgetTheUnusedMeme))
                    {
                        assoc.Meme.ReportForgotten(this);
                        ReportEvent(string.Format("Forgotten how {1} ({0})",assoc.Meme.SignatureString, assoc.Meme.ActionDescription));
                        MemorySizeRemaining += assoc.Meme.Price;
                        memes.Remove(assoc);                        
                        ReportPhenotypeChange();                                                
                    }
                }
            }
        }

        ~Tribesman()
        {
            if (keepsDiary)
            {
                string lifeStory = GetLifeStory();
                if (lifeStory.Length < 10) return;
                string filename = Path.Combine(World.TribesmanLogFolder, this.Name + " born " + YearBorn + ".txt");
                File.WriteAllText(filename, lifeStory);
            }
        }

        public IEnumerable<Meme> knownMemes
        {
            get
            {
                return memes.Select(tma => (Meme)tma);
            }
        }

        public bool TryToTeach(Tribesman student, bool isCulturalExchange=false)
        {
            if (memes.Count > 0 && SupportFunctions.Chance(this.GetFeature(AvailableFeatures.TeachingLikelyhood)))
            {
                if (resource >= WorldProperties.TeachingCosts || SupportFunctions.Chance(WorldProperties.ChanceToTeachIfUnsufficienResources))
                {
                    resource -= WorldProperties.TeachingCosts;
                    List<TribesmanToMemeAssociation> memeAssoc = memes.Where(associ => !student.knownMemes.Contains(associ.Meme)).ToList();
                    if (memeAssoc.Count == 0)
                    {
                        ReportEvent(string.Format("{3}Tried to teach {0} something, but he already knows everything {1} can teach him. {2} resources wasted.", student.Name, Name, WorldProperties.TeachingCosts,isCulturalExchange?"Cultural Exchange! ":""));
                        return false;
                    }
                    Meme memeToTeach = memeAssoc[SupportFunctions.UniformRandomInt(0, memeAssoc.Count)];
                    double teachingSuccessChance = SupportFunctions.MultilpyProbabilities(
                        SupportFunctions.SumProbabilities(
                            this.GetFeature(AvailableFeatures.TeachingEfficiency),
                            student.GetFeature(AvailableFeatures.StudyEfficiency)),
                        Math.Pow(1d / memeToTeach.ComplexityCoefficient, WorldProperties.MemeComplexityToLearningChanceCoefficient));
                    if (memeToTeach.PrequisitesAreMet(student.knownMemes.ToList()))
                    {
                        if (SupportFunctions.Chance(teachingSuccessChance))
                        {
                            if (student.MemorySizeRemaining < memeToTeach.Price)
                            {
                                ReportEvent(string.Format("{3}Tried to teach {0} how {4} ({1}) and failed. {0} was too stupid to remember it. {2} resources wasted.", student.Name, memeToTeach.SignatureString, WorldProperties.TeachingCosts, isCulturalExchange ? "Cultural Exchange! " : "", memeToTeach.ActionDescription));
                                return false;
                            }
                            else
                            {
                                student.LearnNewMemeFrom(memeToTeach, this);
                                if (isCulturalExchange) memeToTeach.Report("Cultural Exchange!");
                                ReportEvent(string.Format("{3}Successfully taught {0} how {4} ({1}). {2} resources used for teaching.", student.Name, memeToTeach.SignatureString, WorldProperties.TeachingCosts, isCulturalExchange ? "Cultural Exchange! " : "", memeToTeach.ActionDescription));
                                UseMemeGroup(AvailableFeatures.TeachingEfficiency, "teaching");
                                UseMemeGroup(AvailableFeatures.TeachingLikelyhood, "teaching");
                                student.UseMemeGroup(AvailableFeatures.StudyEfficiency, "studying");
                                return true;
                            }
                        }
                        else
                        {
                            ReportEvent(string.Format("Tried to teach {0} how {4} ({1}) and failed. Chances were {2:2}%. {3} resources wasted.", student.Name, memeToTeach.SignatureString, teachingSuccessChance * 100d, WorldProperties.TeachingCosts, memeToTeach.ActionDescription));
                            return false;
                        }
                    }
                    else
                    {
                        List<Meme> unmetPrequisites = memeToTeach.WhichPrequisitesAreNotMet(student.knownMemes.ToList());
                        if (unmetPrequisites.Count == 1)
                        {
                            ReportEvent(string.Format("Tried to teach {0} {1} (#{2}), but found out that {0} is not ready to learn it. Must learn {3} (#{4}) first.", student.Name, memeToTeach.ActionDescription, memeToTeach.MemeId, unmetPrequisites[0].ActionDescription, unmetPrequisites[0].MemeId));
                            return false;
                        }
                        else
                        {
                            ReportEvent(string.Format("Tried to teach {0} {1} (#{2}), but found out that {0} is not ready to learn it. Must learn {3} other things first.", student.Name, memeToTeach.ActionDescription, memeToTeach.MemeId, unmetPrequisites.Count));
                            return false;
                        }
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

        private void UseMemeGroup(AvailableFeatures? usedFeature, string activity = "engaged in unknown activity")
        {
            double C = GetFeature(AvailableFeatures.Creativity);;
            double M = WorldProperties.ChanceToInventNewMemeWhileUsingItModifier;
            double T = WorldProperties.ChanceToInventNewMemeWhileUsingItThreshold;
            double chanceToInventNewMeme = T + M * C - T * M * C;
            if (usedFeature == null && WorldProperties.NewMemeUselessEfficiencyMean == 0 && WorldProperties.NewMemeUselessEfficiencyStdDev == 0) { return; }
            if (usedFeature == AvailableFeatures.CooperationEfficiency && WorldProperties.NewMemeCooperationEfficiencyMean == 0 && WorldProperties.NewMemeCooperationEfficiencyStdDev == 0) { return; }
            if (usedFeature == AvailableFeatures.FreeRiderDeterminationEfficiency && WorldProperties.NewMemeFreeRiderDeterminationEfficiencyMean == 0 && WorldProperties.NewMemeFreeRiderDeterminationEfficiencyStdDev == 0) { return; }
            if (usedFeature == AvailableFeatures.FreeRiderPunishmentLikelyhood && WorldProperties.NewMemeFreeRiderPunishmentLikelyhoodMean == 0 && WorldProperties.NewMemeFreeRiderPunishmentLikelyhoodStdDev == 0) { return; }
            if (usedFeature == AvailableFeatures.HuntingEfficiency && WorldProperties.NewMemeHuntingEfficiencyMean == 0 && WorldProperties.NewMemeHuntingEfficiencyStdDev == 0) { return; }
            if (usedFeature == AvailableFeatures.LikelyhoodOfNotBeingAFreeRider && WorldProperties.NewMemeLikelyhoodOfNotBeingAFreeRiderMean == 0 && WorldProperties.NewMemeLikelyhoodOfNotBeingAFreeRiderStdDev == 0) { return; }
            if (usedFeature == AvailableFeatures.StudyEfficiency && WorldProperties.NewMemeStudyEfficiencyMean == 0 && WorldProperties.NewMemeStudyEfficiencyStdDev == 0) { return; }
            if (usedFeature == AvailableFeatures.StudyLikelyhood && WorldProperties.NewMemeStudyLikelyhoodMean == 0 && WorldProperties.NewMemeStudyLikelyhoodStdDev == 0) { return; }
            if (usedFeature == AvailableFeatures.TeachingEfficiency && WorldProperties.NewMemeTeachingEfficiencyMean == 0 && WorldProperties.NewMemeTeachingEfficiencyStdDev == 0) { return; }
            if (usedFeature == AvailableFeatures.TeachingLikelyhood && WorldProperties.NewMemeTeachingLikelyhoodMean == 0 && WorldProperties.NewMemeTeachingLikelyhoodStdDev == 0) { return; }
            if (usedFeature == AvailableFeatures.TrickEfficiency && WorldProperties.NewMemeTrickEfficiencyMean == 0 && WorldProperties.NewMemeTrickEfficiencyStdDev == 0) { return; }
            if (usedFeature == AvailableFeatures.TrickLikelyhood && WorldProperties.NewMemeTrickLikelyhoodMean == 0 && WorldProperties.NewMemeTrickLikelyhoodStdDev == 0) { return; }
            
            if (SupportFunctions.Chance(chanceToInventNewMeme))
            {
                Meme inventedMeme;
                
                if (InventNewMemeForAFeature(usedFeature, out inventedMeme))
                {
                    ReportEvent(string.Format("While {6} invented how {7}. ({0} meme with {1} effect) The meme complexity is {2:f2} and {3} now has {4:f2} memory left. ({5})", usedFeature.HasValue ? usedFeature.Value.GetDescription() : "Nothing", inventedMeme.Efficiency, inventedMeme.Price, Name, MemorySizeRemaining, inventedMeme.SignatureString, activity, inventedMeme.ActionDescription));
                    ReportPhenotypeChange();
                }
                else
                {
                    ReportEvent(string.Format("While {3} invented the new meme but forgot it immediately. Too stupid to remember anything else. Memory size is {1:f2} and meme complexity is {2:f2}", Name, MemorySizeRemaining, inventedMeme.Price, activity));
                }
            }
            IEnumerable<TribesmanToMemeAssociation> relevantMemes = memes.Where(associ => associ.Meme.AffectedFeature == usedFeature);
            foreach (TribesmanToMemeAssociation tma in relevantMemes)
            {
                tma.Use();
                if (MemeUsed != null)
                {
                    MemeUsed(this, tma.Meme);
                }
            }
        }

        private void LearnNewMemeFrom(Meme meme, Tribesman teacher)
        {
            MemorySizeRemaining -= meme.Price;
            memes.Add(TribesmanToMemeAssociation.Create(this, meme));
            if (teacher != null)
            {
                ReportEvent(string.Format("{0} taught how {4} ({2}). {3:f2} memory remaining.", teacher.Name, Name, meme.SignatureString, MemorySizeRemaining, meme.ActionDescription));
            }
            else
            {
                ReportEvent(string.Format("Learned how {2} ({0}) from a tribesmate. {1:f2} memory remaining.", meme.SignatureString, MemorySizeRemaining, meme.ActionDescription));
            }
            meme.ReportTeaching(this, teacher);
            ReportPhenotypeChange();
        }

        public void TryToDetermineAndPunishAFreeRider(List<Tribesman> members, List<Tribesman> freeRidersList)
        {
            if (SupportFunctions.Chance(GetFeature(AvailableFeatures.FreeRiderPunishmentLikelyhood)))
            {
                resource -= WorldProperties.FreeRiderPunishmentCosts;
                if (SupportFunctions.Chance(GetFeature(AvailableFeatures.FreeRiderDeterminationEfficiency)))
                {
                    // Punish a free rider

                    int rand = SupportFunctions.UniformRandomInt(0, freeRidersList.Count);
                    Tribesman FreeRiderToBePunished = freeRidersList[rand];
                    while (FreeRiderToBePunished == this)
                    {
                        if (freeRidersList.Count == 1)
                        {
                            ReportEvent("Successfully determined himself to be the only free rider in a tribe.");
                            Meme inventedMeme2;
                            if ((WorldProperties.NewMemeLikelyhoodOfNotBeingAFreeRiderMean > 0.0001 || WorldProperties.NewMemeLikelyhoodOfNotBeingAFreeRiderMean < -0.0001) && WorldProperties.NewMemeLikelyhoodOfNotBeingAFreeRiderStdDev > 0.0001)
                            {
                                if (InventNewMemeForAFeature(AvailableFeatures.LikelyhoodOfNotBeingAFreeRider, out inventedMeme2))
                                {
                                    ReportEvent(string.Format("Learned {1} ({0}) as a result of a guilty conscience.", inventedMeme2.SignatureString, inventedMeme2.ActionDescription));
                                    ReportPhenotypeChange();
                                }
                                else
                                {
                                    ReportEvent("Tried to invent new meme as a result of a guilty conscience, but was too stupid to remember it.");
                                }
                            }
                            return;
                        }
                        else
                        {
                            rand = SupportFunctions.UniformRandomInt(0, freeRidersList.Count);
                            FreeRiderToBePunished = freeRidersList[rand];
                        }
                    }
                    FreeRiderToBePunished.resource -= WorldProperties.FreeRiderPunishmentAmount;
                    FreeRiderToBePunished.ReportEvent(string.Format("Was punished by {0} for being a free rider. {1} resources destroyed, {2:f2} remaining.", Name, WorldProperties.FreeRiderPunishmentAmount, FreeRiderToBePunished.resource));
                    ReportEvent(string.Format("Decided to punish a free rider. Successfully determined that {0} is one and punished him for {1} resources.", FreeRiderToBePunished.Name, WorldProperties.FreeRiderPunishmentAmount));
                    UseMemeGroup(AvailableFeatures.FreeRiderDeterminationEfficiency, "punishing a free rider");
                    UseMemeGroup(AvailableFeatures.FreeRiderPunishmentLikelyhood, "punishing a free rider");
                    Meme inventedMeme;
                    if (FreeRiderToBePunished.InventNewMemeForAFeature(AvailableFeatures.LikelyhoodOfNotBeingAFreeRider, out inventedMeme))
                    {
                        FreeRiderToBePunished.ReportEvent(string.Format("Learned {1} ({0}) as a result of punishment.", inventedMeme.SignatureString, inventedMeme.ActionDescription));
                        FreeRiderToBePunished.ReportPhenotypeChange();
                    }
                    else
                    {
                        FreeRiderToBePunished.ReportEvent("Tried to invent new meme as a result of punishment, but was too stupid to remember it.");
                    }
                }
                else
                {
                    // Punish random person                    
                    int rand = SupportFunctions.UniformRandomInt(0, members.Count);
                    Tribesman FreeRiderToBePunished = members[rand];
                    while (FreeRiderToBePunished == this)
                    {
                        if (members.Count == 1)
                        {
                            ReportEvent("Successfully determined himself to be the only man in a tribe. Punishing free riders makes no sense.");
                            return;
                        }
                        else
                        {
                            rand = SupportFunctions.UniformRandomInt(0, members.Count);
                            FreeRiderToBePunished = members[rand];
                        }
                    }
                    FreeRiderToBePunished.resource -= WorldProperties.FreeRiderPunishmentAmount;
                    FreeRiderToBePunished.ReportEvent(string.Format("Was punished by {0} on false suspiction.", Name));
                    ReportEvent(string.Format("Tried to punish a free rider, but failed to identify one. Punished {0} for {1} resources instead.", FreeRiderToBePunished.Name, WorldProperties.FreeRiderPunishmentAmount));
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
                ReportEvent(string.Format("Wanted to go hunting, but could not. Too hungry. Resources remaining - {0:f2}, needed for the hunt - {1}.", resource, WorldProperties.HuntingCosts));
                return 0;
            }
            resource -= WorldProperties.HuntingCosts;
            ReportEvent(string.Format("Went hunting with his friends. He hunted with {0:f1} efficiency and cooperated at {1:f2}. Spent {2} resource for the hunt, {3:f2} remaining.", GetFeature(AvailableFeatures.HuntingEfficiency), GetFeature(AvailableFeatures.CooperationEfficiency), WorldProperties.HuntingCosts, resource));
            UseMemeGroup(AvailableFeatures.HuntingEfficiency, "hunting");
            UseMemeGroup(AvailableFeatures.LikelyhoodOfNotBeingAFreeRider, "hunting");
            UseMemeGroup(AvailableFeatures.CooperationEfficiency, "hunting");            
            return GetFeature(AvailableFeatures.HuntingEfficiency);            
        }

        public void SkipHunting()
        {
            ReportEvent(string.Format("Did not go hunting with the tribe. He told the others that {0}.", NamesGenerator.GenerateLameExcuse()));
        }

        public double TellHowMuchDoYouWant(double resourcesReceivedPerGroup)
        {
            double requestedShare = 1;
            if (SupportFunctions.Chance(GetFeature(AvailableFeatures.TrickLikelyhood)))
            {
                requestedShare += GetFeature(AvailableFeatures.TrickEfficiency);
                ReportEvent(string.Format("Group returned from the hunt with {0:f0} resources. He decided to play a trick and asked to get {1:f1}% more then the rest.", resourcesReceivedPerGroup, (requestedShare-1)*100));
                UseMemeGroup(AvailableFeatures.TrickEfficiency, "sharing resources");
                UseMemeGroup(AvailableFeatures.TrickLikelyhood, "sharing resources");
            }
            else
            {
                ReportEvent(string.Format("Group returned from the hunt with {0:f0} resources. He asked for a fare share.", resourcesReceivedPerGroup));
            }
            return requestedShare;
        }

        public void RecieveResourcesShare(double recievedShare)
        {
            resource += recievedShare;
            if (SupportFunctions.Chance(0.99))
            {
                ReportEvent(string.Format("After {2} debate received {0:f0} resources from the common loot and now has {1:f0}.",recievedShare, resource, SupportFunctions.Flip()?"continuous":"short"));
            }
            else
            {
                ReportEvent(string.Format("After {3} debate and a broken {0} received {1:f0} resources from the common loot and now has {2:f0}", NamesGenerator.GenerateBodypart(), recievedShare, resource, SupportFunctions.Flip() ? "continuous" : "short"));
            }
        }

        public void PerformUselessActions()
        {
            int uselessActionsMade = 0;
            double resourcesWasted = 0;
            if (GetFeature(null)>=0.999 && WorldProperties.AllowRepetitiveUselessActions>1)
            {
                resource = 0;
                ReportEvent("Wasted all his resources on useless actions.");
                return;
            }
            while (SupportFunctions.Chance(GetFeature(null)))
            {
                uselessActionsMade++;
                resourcesWasted += WorldProperties.UselessActionCost;
                if (WorldProperties.AllowRepetitiveUselessActions < 0.5) break;
            }
            if (uselessActionsMade > 0)
            {
                resource -= resourcesWasted;

                List<TribesmanToMemeAssociation> relevantMemes = memes.Where(associ => associ.Meme.AffectedFeature == null).ToList();
                Meme randomKnownUselessMeme = relevantMemes[SupportFunctions.UniformRandomInt(0, relevantMemes.Count)].Meme;

                ReportEvent(string.Format("Decided {0}. Didn't gain anything from it, but wasted {1:f2} resources on it. {2:f2} remaining.", randomKnownUselessMeme.ActionDescription, resourcesWasted, resource));                
                UseMemeGroup(null, "performing useless actions");
            }
        }

        public bool HasMemesUsedThisTurn()
        {
            return memes.Any(meme => meme.TurnsSinceLastUsed == 0);            
        }

        public void PrepareForANewYear()
        {
            ReportEvent("    --------------    ");
        }

        public void StudyOneOrManyOfTheMemesUsedThisTurn(List<Meme> memesUsedThisYear)
        {
            List<Meme> memesAvailableForStudy = new List<Meme>(memesUsedThisYear);
            foreach (TribesmanToMemeAssociation assoc in memes)
            {
                if (memesAvailableForStudy.Contains(assoc))
                    memesAvailableForStudy.Remove(assoc);
            }
            while (SupportFunctions.Chance(GetFeature(AvailableFeatures.StudyLikelyhood)) && memesAvailableForStudy.Count > 0 )
            {
                if (resource <= WorldProperties.StudyCosts)
                {
                    ReportEvent("Wanted to learn something new but couldn't. Too hungry.");
                    break;
                }
                Meme memeToStudy = memesAvailableForStudy[SupportFunctions.UniformRandomInt(0, memesAvailableForStudy.Count)];
                if (memeToStudy.PrequisitesAreMet(knownMemes.ToList()))
                {
                    resource -= WorldProperties.StudyCosts;
                    if (SupportFunctions.Chance(
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
                            ReportEvent(string.Format("Tried to teach himself {3} ({0}), but was too stupid to remember it. Meme complexity is {1:f2} and memory size remianing is {2:f2}", memeToStudy.SignatureString, memeToStudy.Price, MemorySizeRemaining, memeToStudy.ActionDescription));
                            break;
                        }
                    }
                }
                else
                {
                    List<Meme> unmetPrequisites = memeToStudy.WhichPrequisitesAreNotMet(knownMemes.ToList());
                    if (unmetPrequisites.Count == 1)
                    {
                        ReportEvent(string.Format("Tried to teach himself {0} (#{1}), but was not ready to learn it. Must learn {2} (#{3}) first.", memeToStudy.ActionDescription, memeToStudy.MemeId, unmetPrequisites[0].ActionDescription, unmetPrequisites[0].MemeId));
                    }
                    else
                    {
                        ReportEvent(string.Format("Tried to teach himself {0} (#{1}), but was not ready to learn it. Must learn {2} other things first.", memeToStudy.ActionDescription, memeToStudy.MemeId, unmetPrequisites.Count));
                    }
                }
                memesAvailableForStudy.Remove(memeToStudy);
                if (WorldProperties.AllowRepetitiveStudying < 0.5) break;
            }
        }

        public bool WantsToDie()
        {
            int age = World.Year - yearBorn;
            if (age >= WorldProperties.DontDieIfYoungerThan)            
            {
                double chanceOfDeathOfOldAge = WorldProperties.DeathLinearChance + age * WorldProperties.DeathAgeDependantChance;
                if (SupportFunctions.Chance(chanceOfDeathOfOldAge))
                {
                    ReportOnDeathStatistics(age);
                    ForgetAllMemes();
                    ReportEvent(string.Format("Died of natural causes at the age of {0}.", age));
                    StatisticsCollector.ReportCountEvent(MyTribeName, "Deaths of old age");                    
                    return true;
                }
            }
            if (resource <= 0)
            {
                ReportOnDeathStatistics(age);
                ForgetAllMemes();
                ReportEvent(string.Format("Died of hunger at the age of {0}.", age));
                StatisticsCollector.ReportCountEvent(MyTribeName, "Deaths of hunger");                
                return true;
            }
            return false;
        }

        private void ReportOnDeathStatistics(int age)
        {
            StatisticsCollector.ReportAverageEvent(MyTribeName, "Longevity", age);
            double totalBrainUsage = 0;
            double totalBrainSize = 0;
            foreach (TribesmanToMemeAssociation tma in memes)
            {
                totalBrainUsage += tma.Meme.Price;
            }
            totalBrainSize = totalBrainUsage + MemorySizeRemaining;
            if (totalBrainSize > 0)
            {
                StatisticsCollector.ReportAverageEvent(MyTribeName, "% memory: unused when died", MemorySizeRemaining / totalBrainSize);
            }
        }

        private void ForgetAllMemes()
        {
            foreach (TribesmanToMemeAssociation assoc in memes.ToArray())
            {
                assoc.Meme.ReportForgotten(this);                
                MemorySizeRemaining += assoc.Meme.Price;
                memes.Remove(assoc);                       
            }
        }

        public void DieOfLonliness()
        {
            int age = World.Year - yearBorn;
            ReportOnDeathStatistics(age);
            ForgetAllMemes();            
            ReportEvent(string.Format("Died of lonliness at the age of {0}. Remained the only one in the tribe.", age));            
        }

        public bool IsOldEnoughToBreed { get { return Age >= WorldProperties.DontBreedIfYoungerThan; } }
        public int Age { get {return World.Year-yearBorn;} }

        public static Tribesman Breed(Tribesman PartnerA, Tribesman PartnerB)
        {
            double totalParentsResource = PartnerA.resource + PartnerB.resource;
            Tribesman child = new Tribesman();
            child.genes = GeneCode.GenerateFrom(PartnerA.genes, PartnerB.genes);

            double priceToGetThisChildBrainSizePart = child.BrainSize * WorldProperties.BrainSizeBirthPriceCoefficient;
            double priceToGetThisChildGiftPart = WorldProperties.ChildStartingResourcePedestal + WorldProperties.ChildStartingResourceParentsCoefficient * (totalParentsResource - priceToGetThisChildBrainSizePart);

            if (totalParentsResource > priceToGetThisChildBrainSizePart + priceToGetThisChildGiftPart)
            {
                StatisticsCollector.ReportCountEvent(PartnerA.MyTribeName, "Child births");
                StatisticsCollector.ReportAverageEvent(PartnerA.MyTribeName, "Child average brain size", child.BrainSize);
                child.yearBorn = World.Year;
                child.keepsDiary = SupportFunctions.Chance(WorldProperties.ChancesToWriteALog);
                child.ReportPhenotypeChange();
                child.myTribeName = PartnerA.MyTribeName;
                child.ReportEvent(string.Format("Was born from {0} and {1}. His brain size is {2:f1}. His parents spent {3:f1} resources to raise him.", PartnerA.Name, PartnerB.Name, child.BrainSize, priceToGetThisChildBrainSizePart));
                child.MemorySizeRemaining = child.GetFeature(AvailableFeatures.MemoryLimit);
                totalParentsResource -= priceToGetThisChildBrainSizePart + priceToGetThisChildGiftPart;
                child.resource =  WorldProperties.ChildStartingResourceSpendingsReceivedCoefficient * priceToGetThisChildBrainSizePart + priceToGetThisChildGiftPart;
                child.ReportEvent(string.Format("Parents have given {0:f1} resource as a birthday gift.", child.resource));                
                PartnerB.resource = PartnerA.resource = totalParentsResource / 2;
                PartnerA.ReportEvent(string.Format("Together with {0} have given birth to {1}. His brain size is {2:f1}. {3:f1} resources taken from each of the parents for birth. {4:f1} extra resources were taken to give to the child.", PartnerB.Name, child.Name, child.BrainSize, priceToGetThisChildBrainSizePart, WorldProperties.ChildStartingResourcePedestal + WorldProperties.ChildStartingResourceParentsCoefficient * totalParentsResource));
                PartnerB.ReportEvent(string.Format("Together with {0} have given birth to {1}. His brain size is {2:f1}. {3:f1} resources taken from each of the parents for birth. {4:f1} extra resources were taken to give to the child.", PartnerA.Name, child.Name, child.BrainSize, priceToGetThisChildBrainSizePart, WorldProperties.ChildStartingResourcePedestal + WorldProperties.ChildStartingResourceParentsCoefficient * totalParentsResource));
                PartnerA.TeachChild(child);
                PartnerB.TeachChild(child);
                return child;
            }
            return null;
        }

        private void TeachChild(Tribesman child)
        {
            if (memes.Count > 0)
            {
                for (int i = 0; i < WorldProperties.FreeTeachingRoundsForParents; i++)
                {
                    List<TribesmanToMemeAssociation> memeAssoc = memes.Where(associ => !child.knownMemes.Contains(associ.Meme)).ToList();
                    if (memeAssoc.Count == 0)
                    {
                        ReportEvent(string.Format("Tried to teach a child {0} something{2}, but he already knows everything his parent {1} can teach him.", child.Name, Name, i > 0 ? " else" : ""));
                        return;
                    }
                    Meme memeToTeach = memeAssoc[SupportFunctions.UniformRandomInt(0, memeAssoc.Count)];
                    double teachingSuccessChance = SupportFunctions.MultilpyProbabilities(
                        SupportFunctions.SumProbabilities(
                            this.GetFeature(AvailableFeatures.TeachingEfficiency),
                            child.GetFeature(AvailableFeatures.StudyEfficiency)),
                        Math.Pow(1d / memeToTeach.ComplexityCoefficient, WorldProperties.MemeComplexityToLearningChanceCoefficient));
                    if (memeToTeach.PrequisitesAreMet(child.knownMemes.ToList()))
                    {
                        if (SupportFunctions.Chance(teachingSuccessChance))
                        {
                            if (child.MemorySizeRemaining < memeToTeach.Price)
                            {
                                ReportEvent(string.Format("Tried to teach child {0} {2} ({1}) and failed. {0} was too stupid to remember it.", child.Name, memeToTeach.SignatureString, memeToTeach.ActionDescription));
                            }
                            else
                            {
                                child.LearnNewMemeFrom(memeToTeach, this);
                                ReportEvent(string.Format("Successfully taught child {0} {2} ({1}).", child.Name, memeToTeach.SignatureString, memeToTeach.ActionDescription));
                                UseMemeGroup(AvailableFeatures.TeachingEfficiency, "teaching child");
                                UseMemeGroup(AvailableFeatures.TeachingLikelyhood, "teaching child");
                                child.UseMemeGroup(AvailableFeatures.StudyEfficiency, "learning from parents");
                            }
                        }
                        else
                        {
                            ReportEvent(string.Format("Tried to teach child {0} {3} ({1}) and failed. Chances were {2:2}%.", child.Name, memeToTeach.SignatureString, teachingSuccessChance * 100d, memeToTeach.ActionDescription));
                        }
                    }
                    else
                    {
                        List<Meme> unmetPrequisites = memeToTeach.WhichPrequisitesAreNotMet(child.knownMemes.ToList());
                        if (unmetPrequisites.Count == 1)
                        {
                            ReportEvent(string.Format("Tried to teach child {0} {1} (#{2}), but found out that {0} is not ready to learn it. Must learn {3} (#{4}) first.", child.Name, memeToTeach.ActionDescription, memeToTeach.MemeId, unmetPrequisites[0].ActionDescription, unmetPrequisites[0].MemeId));
                        } 
                        else
                        {
                            ReportEvent(string.Format("Tried to teach child {0} {1} (#{2}), but found out that {0} is not ready to learn it. Must learn {3} other things first.", child.Name, memeToTeach.ActionDescription, memeToTeach.MemeId, unmetPrequisites.Count));
                        }
                    }
                }
            }

        }

        public void ReportJoiningTribe(Tribe tribe)
        {
            ReportEvent(string.Format("Joined the tribe {0}", tribe.TribeName));
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
                    genes[AvailableFeatures.CooperationEfficiency] * WorldProperties.BrainSizeToCooperationEfficiencyCoefficient +
                    genes[AvailableFeatures.MemoryLimit] * WorldProperties.BrainSizeToMemorySizeCoefficient +
                    genes[AvailableFeatures.Creativity] * WorldProperties.BrainSizeToCreativityCoefficient;
            }
        }

        public string MyTribeName { get => myTribeName; set => myTribeName = value; }

        public bool WantsToLeaveTribe()
        {
            if (SupportFunctions.Chance(WorldProperties.MigrationChance))
            {
                ReportEvent("Decided to leave the tribe and search for a better life somewhere else.");
                return true;
            }
            return false;
        }

        public void PrepareForMigrationOutsideOfTheScope()
        {
            ReportEvent("Decided to migrate to South America. Goodbye.");
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
                NullableDictionary<AvailableFeatures?, double> brainUsages = new NullableDictionary<AvailableFeatures?, double>();
                double totalBrainUsage = 0;
                double totalBrainSize = 0;
                foreach (AvailableFeatures af in Enum.GetValues(typeof(AvailableFeatures)))
                {
                    brainUsages.Add(af, 0);
                }
                brainUsages.Add((AvailableFeatures?)null, 0);
                foreach (TribesmanToMemeAssociation tma in memes)
                {
                    totalBrainUsage += tma.Meme.Price;
                    if (!brainUsages.ContainsKey(tma.Meme.AffectedFeature))
                    {
                        brainUsages.Add(tma.Meme.AffectedFeature, tma.Meme.Price);
                    }
                    else
                    {
                        brainUsages[tma.Meme.AffectedFeature] += tma.Meme.Price;
                    }
                }
                totalBrainSize = totalBrainUsage + MemorySizeRemaining;
                if (totalBrainSize > 0)
                {
                    StatisticsCollector.ReportAverageEvent(MyTribeName, "% memory: unused", MemorySizeRemaining / totalBrainSize);
                    foreach (KeyValuePair<AvailableFeatures?, double> kvp in brainUsages)
                    {
                        StatisticsCollector.ReportAverageEvent(MyTribeName, string.Format("% memory: {0}", kvp.Key == null ? "useless" : kvp.Key.ToString()), kvp.Value / totalBrainSize);
                    }
                }
            }
        }

        
    }

}
