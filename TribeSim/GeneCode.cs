using System;
using System.Windows.Forms;

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

            int randomMother = randomizer.Next(1 << WorldProperties.FEATURES_COUNT); // Избавимся от Flip который пустая трата ресурсов в данном месте.
            int randomFather = randomizer.Next(1 << WorldProperties.FEATURES_COUNT); // Рандомы по отдельности, потому что, внезапно, фич стало 16 и их количество перестало в int помещаться.

            for (int feature = 0; feature < genesInheritedFromMother.Length; feature++)
            {
                var description = WorldProperties.FeatureDescriptions[feature];
                
                var parentGene = (randomMother & 1) == 0 ? motherGenes.strandA[feature] : motherGenes.strandB[feature];
                genesInheritedFromMother[feature] = description.ChancceOfMutation > 0 && randomizer.NextDouble() < description.ChancceOfMutation ? MutateFeature(randomizer, parentGene, description) : parentGene; // Мутация выпадает редко, нефиг делать системкол, который в 99% лишний.
                randomMother = randomMother >> 1;

                parentGene = (randomFather & 1) == 0 ? fatherGenes.strandA[feature] : fatherGenes.strandB[feature];
                genesInheritedFromFather[feature] = description.ChancceOfMutation > 0 && randomizer.NextDouble() < description.ChancceOfMutation ? MutateFeature(randomizer, parentGene, description) : parentGene;
                randomFather = randomFather >> 1;
            }
            return new GeneCode(genesInheritedFromMother, genesInheritedFromFather);
        }

        private static double MutateFeature(Random randomizer, double parentGenes, FeatureDescription feature)
        {
            double newFeatureValue = -1;
            Double lowerBound = 0;
            Double upperBound = Double.PositiveInfinity;
            if (feature.range == FeatureRange.ZeroToOne) {                
                upperBound = 1;                    
            }
            short attempts = 0;
            do { // Обычно правильные данные выпадают с первого раза. и получается, что у нас ровно в два раза больше проверок, чем надо.
                if (attempts == 500) {
                    MessageBox.Show("Randomizer could not produce a value for a feature to fit between " + lowerBound + " and " + upperBound + " after 500 consecutive throws. The last attempt was " + newFeatureValue + ". Please fix the genetic values.", "Bad genetic initial data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                newFeatureValue = parentGenes + randomizer.NormalRandom(feature.MutationStrengthMean, feature.MutationStrengthStdDev);
                attempts++;                
            }
            while (newFeatureValue < lowerBound || newFeatureValue > upperBound); // При кривых начальных данных, например mean = -2 std = 1 может замедлиться в сотни раз.
            return newFeatureValue;
        }

        public double this[int feature] { get { return resultingSet[feature]; } }
        public double this[AvailableFeatures feature] { get { return resultingSet[(int)feature]; } }
    }
}
