using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TribeSim
{
    class GeneCode
    {
        private double[] strandA;
        private double[] strandB;
        private double[] resultingSet;

        public static GeneCode GenerateInitial()
        {
            return new GeneCode(FeatureSet.GenerateInitialGeneStrand(), FeatureSet.GenerateInitialGeneStrand());            
        }

        public GeneCode(double[] parentStrand1, double[] parentStrand2)
        {
            strandA = parentStrand1; // Клонирование тут не нужно, потому что сюдя всегда попадают или рекомбинированные масивы из других либо случайно наполненные специально для этого дела. 
            strandB = parentStrand2;
            resultingSet = FeatureSet.Blank();
            for (int i = 0; i < strandA.Length; i++)
            {
                resultingSet[i] = (strandA[i] + strandB[i]) * .5; // Умножение работает сильно быстрее деления
            }
        }

        public static GeneCode GenerateFrom(GeneCode motherGenes, GeneCode fatherGenes)
        {
            double[] genesInheritedFromMother = FeatureSet.Blank();
            double[] genesInheritedFromFather = FeatureSet.Blank();
            for (int feature = 0; feature < genesInheritedFromMother.Length; feature++)
            {
                genesInheritedFromMother[feature] = InheritTheFeatureWithAMutationChance(
                    SupportFunctions.Flip() ? motherGenes.strandA[feature] : motherGenes.strandB[feature],
                    WorldProperties.FeatureDescriptions[feature]);

                genesInheritedFromFather[feature] = InheritTheFeatureWithAMutationChance(
                    SupportFunctions.Flip() ? fatherGenes.strandA[feature] : fatherGenes.strandB[feature],
                    WorldProperties.FeatureDescriptions[feature]);
            }
            return new GeneCode(genesInheritedFromMother, genesInheritedFromFather);
        }

        private static double InheritTheFeatureWithAMutationChance(double parentGenes, FeatureDescription feature)
        {
            if (SupportFunctions.Chance(feature.ChancceOfMutation)) {
                double newFeatureValue = -1;
                while (newFeatureValue < 0) // При кривых начальных данных, например mean = -2 std = 1 может замедлиться в сотни раз.
                {
                    newFeatureValue = parentGenes + SupportFunctions.NormalRandom(feature.MutationStrengthMean, feature.MutationStrengthStdDev);
                    if (newFeatureValue > 1 && feature.Is0to1Feature) {
                        newFeatureValue = -1; // Раз уж мы числа меньше 0 отбрасываем, то и больше 1 надо отбрасывать, а то распределение получится однобокое
                    }
                }
                return newFeatureValue;
            } else
                return parentGenes;

        }

        public double this[int feature] { get { return resultingSet[feature]; } }
        public double this[AvailableFeatures feature] { get { return resultingSet[(int)feature]; } }
    }
}
