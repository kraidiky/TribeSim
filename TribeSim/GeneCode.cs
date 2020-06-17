using DocumentFormat.OpenXml.Drawing.Diagrams;
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

        public static GeneCode GenerateInitial(Random randomizer)
        {
            return new GeneCode(FeatureSet.GenerateInitialGeneStrand(randomizer), FeatureSet.GenerateInitialGeneStrand(randomizer));            
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

        public static GeneCode GenerateFrom(Random randomizer, GeneCode motherGenes, GeneCode fatherGenes)
        {
            double[] genesInheritedFromMother = FeatureSet.Blank();
            double[] genesInheritedFromFather = FeatureSet.Blank();

            int random = randomizer.Next(1 << WorldProperties.FEATURES_COUNT << WorldProperties.FEATURES_COUNT); // Избавимся от Flip который пустая трата ресурсов в данном месте.

            for (int feature = 0; feature < genesInheritedFromMother.Length; feature++)
            {
                var description = WorldProperties.FeatureDescriptions[feature];
                
                var parentGene = (random & 1) == 0 ? motherGenes.strandA[feature] : motherGenes.strandB[feature];
                genesInheritedFromMother[feature] = InheritTheFeatureWithAMutationChance(randomizer, parentGene, description);
                random = random >> 1;

                parentGene = (random & 1) == 0 ? fatherGenes.strandA[feature] : fatherGenes.strandB[feature];
                genesInheritedFromFather[feature] = InheritTheFeatureWithAMutationChance(randomizer, parentGene, description);
                random = random >> 1;
            }
            return new GeneCode(genesInheritedFromMother, genesInheritedFromFather);
        }

        private static double InheritTheFeatureWithAMutationChance(Random randomizer, double parentGenes, FeatureDescription feature)
        {
            if (randomizer.Chance(feature.ChancceOfMutation)) {
                double newFeatureValue = -1;
                while (newFeatureValue < 0) // При кривых начальных данных, например mean = -2 std = 1 может замедлиться в сотни раз.
                {
                    newFeatureValue = parentGenes + randomizer.NormalRandom(feature.MutationStrengthMean, feature.MutationStrengthStdDev);
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
