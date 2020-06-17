using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TribeSim
{
    class GeneCode
    {
        private FeatureSet strandA;
        private FeatureSet strandB;
        private FeatureSet resultingSet;

        public static GeneCode GenerateInitial()
        {
            return new GeneCode(FeatureSet.GenerateInitialGeneStrand(), FeatureSet.GenerateInitialGeneStrand());            
        }

        public GeneCode(FeatureSet parentStrand1, FeatureSet parentStrand2)
        {
            strandA = new FeatureSet(parentStrand1);
            strandB = new FeatureSet(parentStrand2);
            resultingSet = new FeatureSet();
            foreach (AvailableFeatures feature in Enum.GetValues(typeof(AvailableFeatures)))
            {
                resultingSet[feature] = (strandA[feature] + strandB[feature]) / 2d;
            }
        }

        public static GeneCode GenerateFrom(GeneCode motherGenes, GeneCode fatherGenes)
        {
            FeatureSet genesInheritedFromMother = new FeatureSet();
            FeatureSet genesInheritedFromFather = new FeatureSet();
            foreach (AvailableFeatures feature in Enum.GetValues(typeof(AvailableFeatures)))
            {
                if (SupportFunctions.Flip())
                {
                    InheritTheFeatureWithAMutationChance(motherGenes, genesInheritedFromMother, feature, true);
                }
                else
                {
                    InheritTheFeatureWithAMutationChance(motherGenes, genesInheritedFromMother, feature, false);
                }

                if (SupportFunctions.Flip())
                {                    
                    InheritTheFeatureWithAMutationChance(fatherGenes, genesInheritedFromFather, feature, true);
                }
                else
                {                    
                    InheritTheFeatureWithAMutationChance(fatherGenes, genesInheritedFromFather, feature, false);
                }
            }
            return new GeneCode(genesInheritedFromMother, genesInheritedFromFather);
        }

        private static void InheritTheFeatureWithAMutationChance(GeneCode parentGenes, FeatureSet genesInheritedFromParent, AvailableFeatures feature, bool useStrandA)
        {
            FeatureSet parentStrand;
            if (useStrandA)
            {
                parentStrand = parentGenes.strandA;
            }
            else
            {
                parentStrand = parentGenes.strandB;
            }
            double chancceOfMutation = 0;
            double mutationStrengthMean = 0;
            double mutationStrengthStdDev = 0;
            switch (feature)
            {
                case AvailableFeatures.CooperationEfficiency:
                    chancceOfMutation = WorldProperties.MutationChanceCooperationEfficiency;
                    mutationStrengthMean = WorldProperties.MutationStrengthMeanCooperationEfficiency;
                    mutationStrengthStdDev = WorldProperties.MutationStrengthStdDevCooperationEfficiency;
                    break;
                case AvailableFeatures.Creativity:
                    chancceOfMutation = WorldProperties.MutationChanceCreativity;
                    mutationStrengthMean = WorldProperties.MutationStrengthMeanCreativity;
                    mutationStrengthStdDev = WorldProperties.MutationStrengthStdDevCreativity;
                    break;
                case AvailableFeatures.FreeRiderDeterminationEfficiency:
                    chancceOfMutation = WorldProperties.MutationChanceFreeRiderDeterminationEfficiency;
                    mutationStrengthMean = WorldProperties.MutationStrengthMeanFreeRiderDeterminationEfficiency;
                    mutationStrengthStdDev = WorldProperties.MutationStrengthStdDevFreeRiderDeterminationEfficiency;
                    break;
                case AvailableFeatures.FreeRiderPunishmentLikelyhood:
                    chancceOfMutation = WorldProperties.MutationChanceFreeRiderPunishmentLikelyhood;
                    mutationStrengthMean = WorldProperties.MutationStrengthMeanFreeRiderPunishmentLikelyhood;
                    mutationStrengthStdDev = WorldProperties.MutationStrengthStdDevFreeRiderPunishmentLikelyhood;
                    break;
                case AvailableFeatures.HuntingEfficiency:
                    chancceOfMutation = WorldProperties.MutationChanceHuntingEfficiency;
                    mutationStrengthMean = WorldProperties.MutationStrengthMeanHuntingEfficiency;
                    mutationStrengthStdDev = WorldProperties.MutationStrengthStdDevHuntingEfficiency;
                    break;
                case AvailableFeatures.LikelyhoodOfNotBeingAFreeRider:
                    chancceOfMutation = WorldProperties.MutationChanceLikelyhoodOfNotBeingAFreeRider;
                    mutationStrengthMean = WorldProperties.MutationStrengthMeanLikelyhoodOfNotBeingAFreeRider;
                    mutationStrengthStdDev = WorldProperties.MutationStrengthStdDevLikelyhoodOfNotBeingAFreeRider;
                    break;
                case AvailableFeatures.MemoryLimit:
                    chancceOfMutation = WorldProperties.MutationChanceMemoryLimit;
                    mutationStrengthMean = WorldProperties.MutationStrengthMeanMemoryLimit;
                    mutationStrengthStdDev = WorldProperties.MutationStrengthStdDevMemoryLimit;
                    break;
                case AvailableFeatures.StudyEfficiency:
                    chancceOfMutation = WorldProperties.MutationChanceStudyEfficiency;
                    mutationStrengthMean = WorldProperties.MutationStrengthMeanStudyEfficiency;
                    mutationStrengthStdDev = WorldProperties.MutationStrengthStdDevStudyEfficiency;
                    break;
                case AvailableFeatures.StudyLikelyhood:
                    chancceOfMutation = WorldProperties.MutationChanceStudyLikelyhood;
                    mutationStrengthMean = WorldProperties.MutationStrengthMeanStudyLikelyhood;
                    mutationStrengthStdDev = WorldProperties.MutationStrengthStdDevStudyLikelyhood;
                    break;
                case AvailableFeatures.TeachingEfficiency:
                    chancceOfMutation = WorldProperties.MutationChanceTeachingEfficiency;
                    mutationStrengthMean = WorldProperties.MutationStrengthMeanTeachingEfficiency;
                    mutationStrengthStdDev = WorldProperties.MutationStrengthStdDevTeachingEfficiency;
                    break;
                case AvailableFeatures.TeachingLikelyhood:
                    chancceOfMutation = WorldProperties.MutationChanceTeachingLikelyhood;
                    mutationStrengthMean = WorldProperties.MutationStrengthMeanTeachingLikelyhood;
                    mutationStrengthStdDev = WorldProperties.MutationStrengthStdDevTeachingLikelyhood;
                    break;
                case AvailableFeatures.TrickEfficiency:
                    chancceOfMutation = WorldProperties.MutationChanceTrickEfficiency;
                    mutationStrengthMean = WorldProperties.MutationStrengthMeanTrickEfficiency;
                    mutationStrengthStdDev = WorldProperties.MutationStrengthStdDevTrickEfficiency;
                    break;
                case AvailableFeatures.TrickLikelyhood:
                    chancceOfMutation = WorldProperties.MutationChanceTrickLikelyhood;
                    mutationStrengthMean = WorldProperties.MutationStrengthMeanTrickLikelyhood;
                    mutationStrengthStdDev = WorldProperties.MutationStrengthStdDevTrickLikelyhood;
                    break;                   
            }
            if (SupportFunctions.Chance(chancceOfMutation))
            {
                double newFeatureValue = -1;
                while (newFeatureValue < 0)
                {
                    newFeatureValue = parentStrand[feature] + SupportFunctions.NormalRandom(mutationStrengthMean, mutationStrengthStdDev);                    
                    if (newFeatureValue > 1 && feature != AvailableFeatures.CooperationEfficiency && feature != AvailableFeatures.HuntingEfficiency && feature != AvailableFeatures.MemoryLimit && feature != AvailableFeatures.TrickEfficiency)
                    {
                        newFeatureValue = 1;
                    }
                }
                genesInheritedFromParent[feature] = newFeatureValue;
            }
            else
            {
                genesInheritedFromParent[feature] = parentStrand[feature];
            }

        }

        public double this[AvailableFeatures feature] { get { return resultingSet[feature]; } }
    }
}
