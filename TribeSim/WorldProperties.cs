using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace TribeSim
{
    static class WorldProperties
    {
        #region Constructor

        static WorldProperties()
        {
            LoadPersistance();
        }

        #endregion

        #region Private fields
        private static double initialStateRandomSeed;
        private static double ignoreMultithreading;

        private static bool inStableState = true;
        private static string filename = null;
        private static PropertyTree propertyTree = null;

        private static double startingNumberOfTribes;
        private static double startingTribePopulationMean;
        private static double startingTribePopulationStdDev;

        private static double initialStateGenesTrickLikelyhoodMean;
        private static double initialStateGenesTrickLikelyhoodStdDev;
        private static double initialStateGenesTrickEfficiencyMean;
        private static double initialStateGenesTrickEfficiencyStdDev;
        private static double initialStateGenesTeachingLikelyhoodMean;
        private static double initialStateGenesTeachingLikelyhoodStdDev;
        private static double initialStateGenesTeachingEfficiencyMean;
        private static double initialStateGenesTeachingEfficiencyStdDev;
        private static double initialStateGenesStudyLikelyhoodMean;
        private static double initialStateGenesStudyLikelyhoodStdDev;
        private static double initialStateGenesStudyEfficiencyMean;
        private static double initialStateGenesStudyEfficiencyStdDev;
        private static double initialStateGenesFreeRiderPunishmentLikelyhoodMean;
        private static double initialStateGenesFreeRiderPunishmentLikelyhoodStdDev;
        private static double initialStateGenesFreeRiderDeterminationEfficiencyMean;
        private static double initialStateGenesFreeRiderDeterminationEfficiencyStdDev;
        private static double initialStateGenesLikelyhoodOfNotBeingAFreeRiderMean;
        private static double initialStateGenesLikelyhoodOfNotBeingAFreeRiderStdDev;
        private static double initialStateGenesHuntingEfficiencyMean;
        private static double initialStateGenesHuntingEfficiencyStdDev;
        private static double initialStateGenesHuntingBEfficiencyMean;
        private static double initialStateGenesHuntingBEfficiencyStdDev;
        private static double initialStateGenesCooperationEfficiencyMean;
        private static double initialStateGenesCooperationEfficiencyStdDev;
        private static double initialStateGenesMemorySizeMean;
        private static double initialStateGenesMemorySizeStdDev;
        private static double initialStateGenesCreativityMean;
        private static double initialStateGenesCreativityStdDev;
        private static double initialStateGenesUselessActionsLikelihoodMean;
        private static double initialStateGenesUselessActionsLikelihoodStdDev;
        private static double initialStateGenesAgeingRateMean;
        private static double initialStateGenesAgeingRateStdDev;            

        private static double newMemeTrickLikelyhoodMean;
        private static double newMemeTrickLikelyhoodStdDev;
        private static double newMemeTrickEfficiencyMean;
        private static double newMemeTrickEfficiencyStdDev;
        private static double newMemeTeachingLikelyhoodMean;
        private static double newMemeTeachingLikelyhoodStdDev;
        private static double newMemeTeachingEfficiencyMean;
        private static double newMemeTeachingEfficiencyStdDev;
        private static double newMemeStudyLikelyhoodMean;
        private static double newMemeStudyLikelyhoodStdDev;
        private static double newMemeStudyEfficiencyMean;
        private static double newMemeStudyEfficiencyStdDev;
        private static double newMemeFreeRiderPunishmentLikelyhoodMean;
        private static double newMemeFreeRiderPunishmentLikelyhoodStdDev;
        private static double newMemeFreeRiderDeterminationEfficiencyMean;
        private static double newMemeFreeRiderDeterminationEfficiencyStdDev;
        private static double newMemeLikelyhoodOfNotBeingAFreeRiderMean;
        private static double newMemeLikelyhoodOfNotBeingAFreeRiderStdDev;
        private static double newMemeHuntingEfficiencyMean;
        private static double newMemeHuntingEfficiencyStdDev;
        private static double newMemeHuntingBEfficiencyMean;
        private static double newMemeHuntingBEfficiencyStdDev;
        private static double newMemeCooperationEfficiencyMean;
        private static double newMemeCooperationEfficiencyStdDev;
        private static double newMemeUselessCostMean;
        private static double newMemeUselessCostStdDev;
        private static double newMemeAgeingRateMean;
        private static double newMemeAgeingRateStdDev;
        private static double chanceToInventNewMemeWhileUsingItModifier;
        private static double chanceToInventNewMemeWhileUsingItThreshold;

        private static double dontForgetMemesThatWereUsedDuringThisPeriod;
        private static double chanceToForgetTheUnusedMeme;

        private static double lifeSupportCosts;
        private static double statringAmountOfResources;

        private static double allowRepetitiveTeaching;
        private static double allowRepetitiveStudying;
        private static double teachingCosts;
        private static double studyCosts;
        private static double chanceToTeachIfUnsufficienResources;

        private static double freeRiderPunishmentCosts;
        private static double freeRiderPunishmentAmount;

        private static double resourcesAvailableFromEnvironmentOnEveryStep;
        private static double resourcesAvailableFromEnvironmentOnEveryStepStdDev;
        private static double resourcesAvailableFromEnvironmentOnEveryStepDeviationLimit;

        private static double resourcesABReplacementPeriod;
        private static double resourcesAAvailableFromEnvironmentMinimum;

        private static double resourcesBAvailableFromEnvironmentOnEveryStep;
        private static double resourcesBAvailableFromEnvironmentOnEveryStepStdDev;
        private static double resourcesBAvailableFromEnvironmentOnEveryStepDeviationLimit;
        private static double resourcesBAvailableFromEnvironmentMinimum;

        private static double huntingCosts;
        private static double huntingBCosts;
        private static double uselessActionCost;
        private static double allowRepetitiveUselessActions;

        private static double dontDieIfYoungerThan;
        private static double deathLinearChance;
        private static double deathAgeDependantChance;

        private static double dontBreedIfYoungerThan;
        private static double brainSizeBirthPriceCoefficient;
        private static double freeTeachingRoundsForParents;
        private static double childStartingResourcePedestal;
        private static double childStartingResourceSpendingsReceivedCoefficient;
        private static double childStartingResourceParentsCoefficient;

        private static double brainSizePedestal;
        private static double brainSizeToTrickEfficiencyCoefficient;
        private static double brainSizeToTeachingEfficiencyCoefficient;
        private static double brainSizeToStudyEfficiencyCoefficient;
        private static double brainSizeToFreeRiderDeterminationEfficiencyCoefficient;
        private static double brainSizeToHuntingEfficiencyCoefficient;
        private static double brainSizeToHuntingBEfficiencyCoefficient;
        private static double brainSizeToCooperationEfficiencyCoefficient;
        private static double brainSizeToMemorySizeCoefficient;
        private static double brainSizeToCreativityCoefficient;

        private static double splitTribeIfBiggerThen;
        private static double splitTribeRatio;
        private static double migrateFromGroupsLargerThan;
        private static double migrationChance;
        private static double culturalExchangeTeachingChance;
        private static double allowRepetitiveCulturalExchange;

        private static double maximumTribesAllowedInTheWorld;

        private static double chancesToWriteALog;
        private static double chancesThatMemeWillWriteALog;
        private static double chancesThatTribeWillWriteALog;
        private static double collectPhenotypeValues;
        private static double collectGenotypeValues;
        private static double collectIndividualPhenotypeValues;
        private static double collectIndividualGenotypeValues;

        private static double skipLifeSupportStep;
        private static double skipSpontaneousMemeInventionStep;
        private static double skipForgettingMemesStep;
        private static double skipTeachingStep;
        private static double skipFreeRiderPunishmentStep;
        private static double skipHuntingStep;
        private static double skipHuntingBStep;
        private static double skipUselessActionsStep;
        private static double skipStudyingStep;
        private static double skipDeathStep;
        private static double skipBreedingStep;
        private static double skipGroupSplittingStep;
        private static double skipMigrationStep;
        private static double skipCulturalExchangeStep;

        private static double geneticTrickEfficiencyToMemoryRatio;
        private static double geneticTrickLikelyhoodToMemoryRatio;
        private static double geneticStudyEfficiencyToMemoryRatio;
        private static double geneticStudyLikelyhoodToMemoryRatio;
        private static double geneticLikelyhoodOfNotBeingAFreeRiderToMemoryRatio;
        private static double geneticTeachingEfficiencyToMemoryRatio;
        private static double geneticTeachingLikelyhoodToMemoryRatio;
        private static double geneticHuntingEfficiencyToMemoryRatio;
        private static double geneticHuntingBEfficiencyToMemoryRatio;
        private static double geneticCooperationEfficiancyToMemoryRatio;
        private static double geneticCreativityToMemoryRatio;
        private static double geneticFreeRiderDeterminationEfficiencytoMemoryRatio;
        private static double geneticFreeRiderPunishmentLikelyhoodToMemoryRatio;

        private static double _memesTimesTypesMaxCount;

        private static double memeCostPedestalTrickEfficiency;
        private static double memeCostEfficiencyRatioTrickEfficiency;
        private static double memeCostRandomAverageTrickEfficiency;
        private static double memeCostRandomStdDevTrickEfficiency;
        private static double memeCostPedestalTrickLikelyhood;
        private static double memeCostEfficiencyRatioTrickLikelyhood;
        private static double memeCostRandomAverageTrickLikelyhood;
        private static double memeCostRandomStdDevTrickLikelyhood;
        private static double memeCostPedestalStudyEfficiency;
        private static double memeCostEfficiencyRatioStudyEfficiency;
        private static double memeCostRandomAverageStudyEfficiency;
        private static double memeCostRandomStdDevStudyEfficiency;
        private static double memeCostPedestalStudyLikelyhood;
        private static double memeCostEfficiencyRatioStudyLikelyhood;
        private static double memeCostRandomAverageStudyLikelyhood;
        private static double memeCostRandomStdDevStudyLikelyhood;
        private static double memeCostPedestalTeachingEfficiency;
        private static double memeCostEfficiencyRatioTeachingEfficiency;
        private static double memeCostRandomAverageTeachingEfficiency;
        private static double memeCostRandomStdDevTeachingEfficiency;
        private static double memeCostPedestalTeachingLikelyhood;
        private static double memeCostEfficiencyRatioTeachingLikelyhood;
        private static double memeCostRandomAverageTeachingLikelyhood;
        private static double memeCostRandomStdDevTeachingLikelyhood;
        private static double memeCostPedestalHuntingEfficiency;
        private static double memeCostEfficiencyRatioHuntingEfficiency;
        private static double memeCostRandomAverageHuntingEfficiency;
        private static double memeCostRandomStdDevHuntingEfficiency;
        private static double memeCostPedestalHuntingBEfficiency;
        private static double memeCostEfficiencyRatioHuntingBEfficiency;
        private static double memeCostRandomAverageHuntingBEfficiency;
        private static double memeCostRandomStdDevHuntingBEfficiency;
        private static double memeCostPedestalCooperationEfficiency;
        private static double memeCostEfficiencyRatioCooperationEfficiency;
        private static double memeCostRandomAverageCooperationEfficiency;
        private static double memeCostRandomStdDevCooperationEfficiency;
        private static double memeCostPedestalLikelyhoodOfNotBeingAFreeRider;
        private static double memeCostEfficiencyRatioLikelyhoodOfNotBeingAFreeRider;
        private static double memeCostRandomAverageLikelyhoodOfNotBeingAFreeRider;
        private static double memeCostRandomStdDevLikelyhoodOfNotBeingAFreeRider;
        private static double memeCostPedestalFreeRiderDeterminationEfficiency;
        private static double memeCostEfficiencyRatioFreeRiderDeterminationEfficiency;
        private static double memeCostRandomAverageFreeRiderDeterminationEfficiency;
        private static double memeCostRandomStdDevFreeRiderDeterminationEfficiency;
        private static double memeCostPedestalFreeRiderPunishmentLikelyhood;
        private static double memeCostEfficiencyRatioFreeRiderPunishmentLikelyhood;
        private static double memeCostRandomAverageFreeRiderPunishmentLikelyhood;
        private static double memeCostRandomStdDevFreeRiderPunishmentLikelyhood;
        private static double memeCostPedestalUseless;
        private static double memeCostEfficiencyRatioUseless;
        private static double memeCostRandomAverageUseless;
        private static double memeCostRandomStdDevUseless;
        private static double memeCostPedestalAgeingRate;
        private static double memeCostEfficiencyRatioAgeingRate;
        private static double memeCostRandomAverageAgeingRate;
        private static double memeCostRandomStdDevAgeingRate;

        private static double memeCostComplexityPriceCoefficient;
        private static double memeComplexityToLearningChanceCoefficient;

        private static double mutationChanceTrickLikelyhood;
        private static double mutationChanceTrickEfficiency;
        private static double mutationChanceTeachingLikelyhood;
        private static double mutationChanceTeachingEfficiency;
        private static double mutationChanceStudyLikelyhood;
        private static double mutationChanceStudyEfficiency;
        private static double mutationChanceFreeRiderPunishmentLikelyhood;
        private static double mutationChanceFreeRiderDeterminationEfficiency;
        private static double mutationChanceLikelyhoodOfNotBeingAFreeRider;
        private static double mutationChanceHuntingEfficiency;
        private static double mutationChanceHuntingBEfficiency;
        private static double mutationChanceCooperationEfficiency;
        private static double mutationChanceMemoryLimit;
        private static double mutationChanceCreativity;
        private static double mutationChanceUselessActionsLikelihood;
        private static double mutationChanceAgeingRate;
        private static double mutationStrengthMeanTrickLikelyhood;
        private static double mutationStrengthMeanTrickEfficiency;
        private static double mutationStrengthMeanTeachingLikelyhood;
        private static double mutationStrengthMeanTeachingEfficiency;
        private static double mutationStrengthMeanStudyLikelyhood;
        private static double mutationStrengthMeanStudyEfficiency;
        private static double mutationStrengthMeanFreeRiderPunishmentLikelyhood;
        private static double mutationStrengthMeanFreeRiderDeterminationEfficiency;
        private static double mutationStrengthMeanLikelyhoodOfNotBeingAFreeRider;
        private static double mutationStrengthMeanHuntingEfficiency;
        private static double mutationStrengthMeanHuntingBEfficiency;
        private static double mutationStrengthMeanCooperationEfficiency;
        private static double mutationStrengthMeanMemoryLimit;
        private static double mutationStrengthMeanCreativity;
        private static double mutationStrengthMeanUselessActionsLikelihood;
        private static double mutationStrengthMeanAgeingRate;
        private static double mutationStrengthStdDevTrickLikelyhood;
        private static double mutationStrengthStdDevTrickEfficiency;
        private static double mutationStrengthStdDevTeachingLikelyhood;
        private static double mutationStrengthStdDevTeachingEfficiency;
        private static double mutationStrengthStdDevStudyLikelyhood;
        private static double mutationStrengthStdDevStudyEfficiency;
        private static double mutationStrengthStdDevFreeRiderPunishmentLikelyhood;
        private static double mutationStrengthStdDevFreeRiderDeterminationEfficiency;
        private static double mutationStrengthStdDevLikelyhoodOfNotBeingAFreeRider;
        private static double mutationStrengthStdDevHuntingEfficiency;
        private static double mutationStrengthStdDevHuntingBEfficiency;
        private static double mutationStrengthStdDevCooperationEfficiency;
        private static double mutationStrengthStdDevMemoryLimit;
        private static double mutationStrengthStdDevCreativity;
        private static double mutationStrengthStdDevUselessActionsLikelihood;
        private static double mutationStrengthStdDevAgeingRate;

        private static double collectGraphData;
        private static double collectFilesData;
        private static double collectIndividualSuccess;
        private static double chanceToCollectIndividualSuccess;
        private static double collectBrainUsagePercentages;
        private static double collectLiveMemes;
        private static double collectDetaliedMemesData;
        private static double collectMemesSuccess;
        private static double chanceToCollectMemesSuccess;

        private static double useGompertzAgeing;
        private static double gompertzAgeingAgeAtWhichAgeingStarts;
        private static double gompertzAgeingBasicMortalityRate;        
        private static double gompertzAgeingMortalityPlateau;

        private static double baseGompertzAgeingRate;
        private static double baseGompertzAgeingRateLifeCostIncrease;
        private static double baseGompertzAgeingRateLifeCostDecrease;
        private static double baseGompertzAgeingRateLifeCostQuadraticIncrease;
        private static double baseGompertzAgeingRateLifeCostQuadraticDecrease;
        private static double baseGompertzAgeingRateLifeCostExponentialMultiplierIncrease;
        private static double baseGompertzAgeingRateLifeCostExponentialMultiplierDecrease;
        private static double baseGompertzAgeingRateLifeCostExponentialCoefficientIncrease;
        private static double baseGompertzAgeingRateLifeCostExponentialCoefficientDecrease;

        private static double maximumBreedingAge;
        private static double breedingCostsIncreaseAge;
        private static double breedingCostsIncreaseCoefficient;        


        #endregion

        #region Modifiable propereties

        [DisplayableProperty("Initial State Random Seed.", group = "Initial state", description = "Initial state random seed.  Any values lesser or equal 0 - means full random simulations. Value greater then 0 - means every simulations will have random generator initialized by the same seed. It means all simulations with the same seed will be completely identical, except names in log files.")]
        public static double InitialStateRandomSeed { get => initialStateRandomSeed; set { initialStateRandomSeed = value; PersistChanges(); } }

        [DisplayableProperty("Mutation strength StdDev", group = "Genetics\\Mutation\\Useless Actions Likelihood")]
        public static double MutationStrengthStdDevUselessActionsLikelihood { get => mutationStrengthStdDevUselessActionsLikelihood; set { mutationStrengthStdDevUselessActionsLikelihood = value; PersistChanges(); } }
        [DisplayableProperty("Mutation strength StdDev", group = "Genetics\\Mutation\\Creativity")]
        public static double MutationStrengthStdDevCreativity { get => mutationStrengthStdDevCreativity; set {  mutationStrengthStdDevCreativity = value; PersistChanges(); } }
        [DisplayableProperty("Mutation strength StdDev", group = "Genetics\\Mutation\\Memory limit")]
        public static double MutationStrengthStdDevMemoryLimit { get => mutationStrengthStdDevMemoryLimit; set {  mutationStrengthStdDevMemoryLimit = value; PersistChanges(); } }
        [DisplayableProperty("Mutation strength StdDev", group = "Genetics\\Mutation\\Cooperation efficiency")]
        public static double MutationStrengthStdDevCooperationEfficiency { get => mutationStrengthStdDevCooperationEfficiency; set {  mutationStrengthStdDevCooperationEfficiency = value; PersistChanges(); } }
        [DisplayableProperty("Mutation strength StdDev", group = "Genetics\\Mutation\\Hunting efficiency")]
        public static double MutationStrengthStdDevHuntingEfficiency { get => mutationStrengthStdDevHuntingEfficiency; set {  mutationStrengthStdDevHuntingEfficiency = value; PersistChanges(); } }
        [DisplayableProperty("Mutation strength StdDev", group = "Genetics\\Mutation\\Hunting B efficiency")]
        public static double MutationStrengthStdDevHuntingBEfficiency { get => mutationStrengthStdDevHuntingBEfficiency; set { mutationStrengthStdDevHuntingBEfficiency = value; PersistChanges(); } }
        [DisplayableProperty("Mutation strength StdDev", group = "Genetics\\Mutation\\Likelyhood of not being a f-r")]
        public static double MutationStrengthStdDevLikelyhoodOfNotBeingAFreeRider { get => mutationStrengthStdDevLikelyhoodOfNotBeingAFreeRider; set {  mutationStrengthStdDevLikelyhoodOfNotBeingAFreeRider = value; PersistChanges(); } }
        [DisplayableProperty("Mutation strength StdDev", group = "Genetics\\Mutation\\F-r determination efficiency")]
        public static double MutationStrengthStdDevFreeRiderDeterminationEfficiency { get => mutationStrengthStdDevFreeRiderDeterminationEfficiency; set {  mutationStrengthStdDevFreeRiderDeterminationEfficiency = value; PersistChanges(); } }
        [DisplayableProperty("Mutation strength StdDev", group = "Genetics\\Mutation\\F-r punishment likelyhood")]
        public static double MutationStrengthStdDevFreeRiderPunishmentLikelyhood { get => mutationStrengthStdDevFreeRiderPunishmentLikelyhood; set {  mutationStrengthStdDevFreeRiderPunishmentLikelyhood = value; PersistChanges(); } }
        [DisplayableProperty("Mutation strength StdDev", group = "Genetics\\Mutation\\Study efficiency")]
        public static double MutationStrengthStdDevStudyEfficiency { get => mutationStrengthStdDevStudyEfficiency; set {  mutationStrengthStdDevStudyEfficiency = value; PersistChanges(); } }
        [DisplayableProperty("Mutation strength StdDev", group = "Genetics\\Mutation\\Study likelyhood")]
        public static double MutationStrengthStdDevStudyLikelyhood { get => mutationStrengthStdDevStudyLikelyhood; set {  mutationStrengthStdDevStudyLikelyhood = value; PersistChanges(); } }
        [DisplayableProperty("Mutation strength StdDev", group = "Genetics\\Mutation\\Teaching efficiency")]
        public static double MutationStrengthStdDevTeachingEfficiency { get => mutationStrengthStdDevTeachingEfficiency; set {  mutationStrengthStdDevTeachingEfficiency = value; PersistChanges(); } }
        [DisplayableProperty("Mutation strength StdDev", group = "Genetics\\Mutation\\Teaching likelyhood")]
        public static double MutationStrengthStdDevTeachingLikelyhood { get => mutationStrengthStdDevTeachingLikelyhood; set {  mutationStrengthStdDevTeachingLikelyhood = value; PersistChanges(); } }
        [DisplayableProperty("Mutation strength StdDev", group = "Genetics\\Mutation\\Trick efficiency")]
        public static double MutationStrengthStdDevTrickEfficiency { get => mutationStrengthStdDevTrickEfficiency; set {  mutationStrengthStdDevTrickEfficiency = value; PersistChanges(); } }
        [DisplayableProperty("Mutation strength StdDev", group = "Genetics\\Mutation\\Trick likelyhood")]
        public static double MutationStrengthStdDevTrickLikelyhood { get => mutationStrengthStdDevTrickLikelyhood; set {  mutationStrengthStdDevTrickLikelyhood = value; PersistChanges(); } }
        [DisplayableProperty("Mutation strength StdDev", group = "Genetics\\Mutation\\Gompertz Ageing")]
        public static double MutationStrengthStdDevAgeingRate{ get => mutationStrengthStdDevAgeingRate; set {  mutationStrengthStdDevAgeingRate = value; PersistChanges(); } }

        [DisplayableProperty("Mutation strength Mean", group = "Genetics\\Mutation\\Useless Actions Likelihood")]
        public static double MutationStrengthMeanUselessActionsLikelihood { get => mutationStrengthMeanUselessActionsLikelihood; set { mutationStrengthMeanUselessActionsLikelihood = value; PersistChanges(); } }
        [DisplayableProperty("Mutation strength Mean", group = "Genetics\\Mutation\\Creativity")]
        public static double MutationStrengthMeanCreativity { get => mutationStrengthMeanCreativity; set { mutationStrengthMeanCreativity = value; PersistChanges(); } }
        [DisplayableProperty("Mutation strength Mean", group = "Genetics\\Mutation\\Memory limit")]
        public static double MutationStrengthMeanMemoryLimit { get => mutationStrengthMeanMemoryLimit; set { mutationStrengthMeanMemoryLimit = value; PersistChanges(); } }
        [DisplayableProperty("Mutation strength Mean", group = "Genetics\\Mutation\\Cooperation efficiency")]
        public static double MutationStrengthMeanCooperationEfficiency { get => mutationStrengthMeanCooperationEfficiency; set { mutationStrengthMeanCooperationEfficiency = value; PersistChanges(); } }
        [DisplayableProperty("Mutation strength Mean", group = "Genetics\\Mutation\\Hunting efficiency")]
        public static double MutationStrengthMeanHuntingEfficiency { get => mutationStrengthMeanHuntingEfficiency; set { mutationStrengthMeanHuntingEfficiency = value; PersistChanges(); } }
        [DisplayableProperty("Mutation strength Mean", group = "Genetics\\Mutation\\Hunting B efficiency")]
        public static double MutationStrengthMeanHuntingBEfficiency { get => mutationStrengthMeanHuntingBEfficiency; set { mutationStrengthMeanHuntingBEfficiency = value; PersistChanges(); } }
        [DisplayableProperty("Mutation strength Mean", group = "Genetics\\Mutation\\Likelyhood of not being a f-r")]
        public static double MutationStrengthMeanLikelyhoodOfNotBeingAFreeRider { get => mutationStrengthMeanLikelyhoodOfNotBeingAFreeRider; set { mutationStrengthMeanLikelyhoodOfNotBeingAFreeRider = value; PersistChanges(); } }
        [DisplayableProperty("Mutation strength Mean", group = "Genetics\\Mutation\\F-r determination efficiency")]
        public static double MutationStrengthMeanFreeRiderDeterminationEfficiency { get => mutationStrengthMeanFreeRiderDeterminationEfficiency; set { mutationStrengthMeanFreeRiderDeterminationEfficiency = value; PersistChanges(); } }
        [DisplayableProperty("Mutation strength Mean", group = "Genetics\\Mutation\\F-r punishment likelyhood")]
        public static double MutationStrengthMeanFreeRiderPunishmentLikelyhood { get => mutationStrengthMeanFreeRiderPunishmentLikelyhood; set { mutationStrengthMeanFreeRiderPunishmentLikelyhood = value; PersistChanges(); } }
        [DisplayableProperty("Mutation strength Mean", group = "Genetics\\Mutation\\Study efficiency")]
        public static double MutationStrengthMeanStudyEfficiency { get => mutationStrengthMeanStudyEfficiency; set { mutationStrengthMeanStudyEfficiency = value; PersistChanges(); } }
        [DisplayableProperty("Mutation strength Mean", group = "Genetics\\Mutation\\Study likelyhood")]
        public static double MutationStrengthMeanStudyLikelyhood { get => mutationStrengthMeanStudyLikelyhood; set { mutationStrengthMeanStudyLikelyhood = value; PersistChanges(); } }
        [DisplayableProperty("Mutation strength Mean", group = "Genetics\\Mutation\\Teaching efficiency")]
        public static double MutationStrengthMeanTeachingEfficiency { get => mutationStrengthMeanTeachingEfficiency; set { mutationStrengthMeanTeachingEfficiency = value; PersistChanges(); } }
        [DisplayableProperty("Mutation strength Mean", group = "Genetics\\Mutation\\Teaching likelyhood")]
        public static double MutationStrengthMeanTeachingLikelyhood { get => mutationStrengthMeanTeachingLikelyhood; set { mutationStrengthMeanTeachingLikelyhood = value; PersistChanges(); } }
        [DisplayableProperty("Mutation strength Mean", group = "Genetics\\Mutation\\Trick efficiency")]
        public static double MutationStrengthMeanTrickEfficiency { get => mutationStrengthMeanTrickEfficiency; set { mutationStrengthMeanTrickEfficiency = value; PersistChanges(); } }
        [DisplayableProperty("Mutation strength Mean", group = "Genetics\\Mutation\\Trick likelyhood")]
        public static double MutationStrengthMeanTrickLikelyhood { get => mutationStrengthMeanTrickLikelyhood; set { mutationStrengthMeanTrickLikelyhood = value; PersistChanges(); } }
        [DisplayableProperty("Mutation strength Mean", group = "Genetics\\Mutation\\Gompertz Ageing")]
        public static double MutationStrengthMeanAgeingRate{ get => mutationStrengthMeanAgeingRate; set {  mutationStrengthMeanAgeingRate = value; PersistChanges(); } }


        [DisplayableProperty("Mutation Chance", group = "Genetics\\Mutation\\Useless Actions Likelihood")]
        public static double MutationChanceUselessActionsLikelihood { get => mutationChanceUselessActionsLikelihood; set { mutationChanceUselessActionsLikelihood = value; PersistChanges(); } }
        [DisplayableProperty("Mutation Chance", group = "Genetics\\Mutation\\Creativity")]
        public static double MutationChanceCreativity { get => mutationChanceCreativity; set { mutationChanceCreativity = value; PersistChanges(); } }
        [DisplayableProperty("Mutation Chance", group = "Genetics\\Mutation\\Memory limit")]
        public static double MutationChanceMemoryLimit { get => mutationChanceMemoryLimit; set { mutationChanceMemoryLimit = value; PersistChanges(); } }
        [DisplayableProperty("Mutation Chance", group = "Genetics\\Mutation\\Cooperation efficiency")]
        public static double MutationChanceCooperationEfficiency { get => mutationChanceCooperationEfficiency; set { mutationChanceCooperationEfficiency = value; PersistChanges(); } }
        [DisplayableProperty("Mutation Chance", group = "Genetics\\Mutation\\Hunting efficiency")]
        public static double MutationChanceHuntingEfficiency { get => mutationChanceHuntingEfficiency; set { mutationChanceHuntingEfficiency = value; PersistChanges(); } }
        [DisplayableProperty("Mutation Chance", group = "Genetics\\Mutation\\Hunting B efficiency")]
        public static double MutationChanceHuntingBEfficiency { get => mutationChanceHuntingBEfficiency; set { mutationChanceHuntingBEfficiency = value; PersistChanges(); } }
        [DisplayableProperty("Mutation Chance", group = "Genetics\\Mutation\\Likelyhood of not being a f-r")]
        public static double MutationChanceLikelyhoodOfNotBeingAFreeRider { get => mutationChanceLikelyhoodOfNotBeingAFreeRider; set { mutationChanceLikelyhoodOfNotBeingAFreeRider = value; PersistChanges(); } }
        [DisplayableProperty("Mutation Chance", group = "Genetics\\Mutation\\F-r determination efficiency")]
        public static double MutationChanceFreeRiderDeterminationEfficiency { get => mutationChanceFreeRiderDeterminationEfficiency; set { mutationChanceFreeRiderDeterminationEfficiency = value; PersistChanges(); } }
        [DisplayableProperty("Mutation Chance", group = "Genetics\\Mutation\\F-r punishment likelyhood")]
        public static double MutationChanceFreeRiderPunishmentLikelyhood { get => mutationChanceFreeRiderPunishmentLikelyhood; set { mutationChanceFreeRiderPunishmentLikelyhood = value; PersistChanges(); } }
        [DisplayableProperty("Mutation Chance", group = "Genetics\\Mutation\\Study efficiency")]
        public static double MutationChanceStudyEfficiency { get => mutationChanceStudyEfficiency; set { mutationChanceStudyEfficiency = value; PersistChanges(); } }
        [DisplayableProperty("Mutation Chance", group = "Genetics\\Mutation\\Study likelyhood")]
        public static double MutationChanceStudyLikelyhood { get => mutationChanceStudyLikelyhood; set { mutationChanceStudyLikelyhood = value; PersistChanges(); } }
        [DisplayableProperty("Mutation Chance", group = "Genetics\\Mutation\\Teaching efficiency")]
        public static double MutationChanceTeachingEfficiency { get => mutationChanceTeachingEfficiency; set { mutationChanceTeachingEfficiency = value; PersistChanges(); } }
        [DisplayableProperty("Mutation Chance", group = "Genetics\\Mutation\\Teaching likelyhood")]
        public static double MutationChanceTeachingLikelyhood { get => mutationChanceTeachingLikelyhood; set { mutationChanceTeachingLikelyhood = value; PersistChanges(); } }
        [DisplayableProperty("Mutation Chance", group = "Genetics\\Mutation\\Trick efficiency")]
        public static double MutationChanceTrickEfficiency { get => mutationChanceTrickEfficiency; set { mutationChanceTrickEfficiency = value; PersistChanges(); } }
        [DisplayableProperty("Mutation Chance", group = "Genetics\\Mutation\\Trick likelyhood")]
        public static double MutationChanceTrickLikelyhood { get => mutationChanceTrickLikelyhood; set { mutationChanceTrickLikelyhood = value; PersistChanges(); } }
        [DisplayableProperty("Mutation Chance", group = "Genetics\\Mutation\\Gompertz Ageing")]
        public static double MutationChanceAgeingRate { get => mutationChanceAgeingRate; set { mutationChanceAgeingRate = value; PersistChanges(); } }

        [DisplayableProperty("Memes Types Max Count", group = "Memes", description = "Totaly Max count of memes types is limited. 0 - default value it is 33 millions per feature.")]
        public static double MemesTimesMaxCount { get => _memesTimesTypesMaxCount; set { _memesTimesTypesMaxCount = value; PersistChanges(); } }

        [DisplayableProperty("Random part StdDev", group = "Memes\\Invention\\Costs\\Free rider punishment likelyhood")]
        public static double MemeCostRandomStdDevFreeRiderPunishmentLikelyhood { get => memeCostRandomStdDevFreeRiderPunishmentLikelyhood; set { memeCostRandomStdDevFreeRiderPunishmentLikelyhood = value; PersistChanges(); } }
        [DisplayableProperty("Random part mean", group = "Memes\\Invention\\Costs\\Free rider punishment likelyhood")]
        public static double MemeCostRandomAverageFreeRiderPunishmentLikelyhood { get => memeCostRandomAverageFreeRiderPunishmentLikelyhood; set { memeCostRandomAverageFreeRiderPunishmentLikelyhood = value; PersistChanges(); } }
        [DisplayableProperty("Correlation part coefficient (C)", group = "Memes\\Invention\\Costs\\Free rider punishment likelyhood")]
        public static double MemeCostEfficiencyRatioFreeRiderPunishmentLikelyhood { get => memeCostEfficiencyRatioFreeRiderPunishmentLikelyhood; set { memeCostEfficiencyRatioFreeRiderPunishmentLikelyhood = value; PersistChanges(); } }
        [DisplayableProperty("Pedestal value (P)", group = "Memes\\Invention\\Costs\\Free rider punishment likelyhood", description = "The cost of the meme will be calculated using the following formula: Cost = P + C*Efficiency + Random, where random part is determined by its mean and standard deviation")]
        public static double MemeCostPedestalFreeRiderPunishmentLikelyhood { get => memeCostPedestalFreeRiderPunishmentLikelyhood; set { memeCostPedestalFreeRiderPunishmentLikelyhood = value; PersistChanges(); } }

        [DisplayableProperty("Random part StdDev", group = "Memes\\Invention\\Costs\\Free rider determination efficiency")]
        public static double MemeCostRandomStdDevFreeRiderDeterminationEfficiency { get => memeCostRandomStdDevFreeRiderDeterminationEfficiency; set { memeCostRandomStdDevFreeRiderDeterminationEfficiency = value; PersistChanges(); } }
        [DisplayableProperty("Random part mean", group = "Memes\\Invention\\Costs\\Free rider determination efficiency")]
        public static double MemeCostRandomAverageFreeRiderDeterminationEfficiency { get => memeCostRandomAverageFreeRiderDeterminationEfficiency; set { memeCostRandomAverageFreeRiderDeterminationEfficiency = value; PersistChanges(); } }
        [DisplayableProperty("Correlation part coefficient (C)", group = "Memes\\Invention\\Costs\\Free rider determination efficiency")]
        public static double MemeCostEfficiencyRatioFreeRiderDeterminationEfficiency { get => memeCostEfficiencyRatioFreeRiderDeterminationEfficiency; set { memeCostEfficiencyRatioFreeRiderDeterminationEfficiency = value; PersistChanges(); } }
        [DisplayableProperty("Pedestal value (P)", group = "Memes\\Invention\\Costs\\Free rider determination efficiency", description = "The cost of the meme will be calculated using the following formula: Cost = P + C*Efficiency + Random, where random part is determined by its mean and standard deviation")]
        public static double MemeCostPedestalFreeRiderDeterminationEfficiency { get => memeCostPedestalFreeRiderDeterminationEfficiency; set { memeCostPedestalFreeRiderDeterminationEfficiency = value; PersistChanges(); } }

        [DisplayableProperty("Random part StdDev", group = "Memes\\Invention\\Costs\\Altuism")]
        public static double MemeCostRandomStdDevLikelyhoodOfNotBeingAFreeRider { get => memeCostRandomStdDevLikelyhoodOfNotBeingAFreeRider; set { memeCostRandomStdDevLikelyhoodOfNotBeingAFreeRider = value; PersistChanges(); } }
        [DisplayableProperty("Random part mean", group = "Memes\\Invention\\Costs\\Altuism")]
        public static double MemeCostRandomAverageLikelyhoodOfNotBeingAFreeRider { get => memeCostRandomAverageLikelyhoodOfNotBeingAFreeRider; set { memeCostRandomAverageLikelyhoodOfNotBeingAFreeRider = value; PersistChanges(); } }
        [DisplayableProperty("Correlation part coefficient (C)", group = "Memes\\Invention\\Costs\\Altuism")]
        public static double MemeCostEfficiencyRatioLikelyhoodOfNotBeingAFreeRider { get => memeCostEfficiencyRatioLikelyhoodOfNotBeingAFreeRider; set { memeCostEfficiencyRatioLikelyhoodOfNotBeingAFreeRider =value; PersistChanges(); }}
        [DisplayableProperty("Pedestal value (P)", group = "Memes\\Invention\\Costs\\Altuism", description = "The cost of the meme will be calculated using the following formula: Cost = P + C*Efficiency + Random, where random part is determined by its mean and standard deviation")]
        public static double MemeCostPedestalLikelyhoodOfNotBeingAFreeRider { get => memeCostPedestalLikelyhoodOfNotBeingAFreeRider; set { memeCostPedestalLikelyhoodOfNotBeingAFreeRider =value; PersistChanges(); }}

        [DisplayableProperty("Random part StdDev", group = "Memes\\Invention\\Costs\\Cooperation efficiency")]
        public static double MemeCostRandomStdDevCooperationEfficiency { get => memeCostRandomStdDevCooperationEfficiency; set { memeCostRandomStdDevCooperationEfficiency =value; PersistChanges(); }}
        [DisplayableProperty("Random part mean", group = "Memes\\Invention\\Costs\\Cooperation efficiency")]
        public static double MemeCostRandomAverageCooperationEfficiency { get => memeCostRandomAverageCooperationEfficiency; set { memeCostRandomAverageCooperationEfficiency =value; PersistChanges(); }}
        [DisplayableProperty("Correlation part coefficient (C)", group = "Memes\\Invention\\Costs\\Cooperation efficiency")]
        public static double MemeCostEfficiencyRatioCooperationEfficiency { get => memeCostEfficiencyRatioCooperationEfficiency; set { memeCostEfficiencyRatioCooperationEfficiency = value; PersistChanges(); } }
        [DisplayableProperty("Pedestal value (P)", group = "Memes\\Invention\\Costs\\Cooperation efficiency", description = "The cost of the meme will be calculated using the following formula: Cost = P + C*Efficiency + Random, where random part is determined by its mean and standard deviation")]
        public static double MemeCostPedestalCooperationEfficiency { get => memeCostPedestalCooperationEfficiency; set { memeCostPedestalCooperationEfficiency =value; PersistChanges(); }}

        [DisplayableProperty("Random part StdDev", group = "Memes\\Invention\\Costs\\Hunting efficiency")]
        public static double MemeCostRandomStdDevHuntingEfficiency { get => memeCostRandomStdDevHuntingEfficiency; set { memeCostRandomStdDevHuntingEfficiency =value; PersistChanges(); }}
        [DisplayableProperty("Random part mean", group = "Memes\\Invention\\Costs\\Hunting efficiency")]
        public static double MemeCostRandomAverageHuntingEfficiency { get => memeCostRandomAverageHuntingEfficiency; set { memeCostRandomAverageHuntingEfficiency =value; PersistChanges(); }}
        [DisplayableProperty("Correlation part coefficient (C)", group = "Memes\\Invention\\Costs\\Hunting efficiency")]
        public static double MemeCostEfficiencyRatioHuntingEfficiency { get => memeCostEfficiencyRatioHuntingEfficiency; set { memeCostEfficiencyRatioHuntingEfficiency =value; PersistChanges(); }}
        [DisplayableProperty("Pedestal value (P)", group = "Memes\\Invention\\Costs\\Hunting efficiency", description = "The cost of the meme will be calculated using the following formula: Cost = P + C*Efficiency + Random, where random part is determined by its mean and standard deviation")]
        public static double MemeCostPedestalHuntingEfficiency { get => memeCostPedestalHuntingEfficiency; set { memeCostPedestalHuntingEfficiency =value; PersistChanges(); }}

        [DisplayableProperty("Random part StdDev", group = "Memes\\Invention\\Costs\\Hunting B efficiency")]
        public static double MemeCostRandomStdDevHuntingBEfficiency { get => memeCostRandomStdDevHuntingBEfficiency; set { memeCostRandomStdDevHuntingBEfficiency = value; PersistChanges(); } }
        [DisplayableProperty("Random part mean", group = "Memes\\Invention\\Costs\\Hunting B efficiency")]
        public static double MemeCostRandomAverageHuntingBEfficiency { get => memeCostRandomAverageHuntingBEfficiency; set { memeCostRandomAverageHuntingBEfficiency = value; PersistChanges(); } }
        [DisplayableProperty("Correlation part coefficient (C)", group = "Memes\\Invention\\Costs\\Hunting B efficiency")]
        public static double MemeCostEfficiencyRatioHuntingBEfficiency { get => memeCostEfficiencyRatioHuntingBEfficiency; set { memeCostEfficiencyRatioHuntingBEfficiency = value; PersistChanges(); } }
        [DisplayableProperty("Pedestal value (P)", group = "Memes\\Invention\\Costs\\Hunting B efficiency", description = "The cost of the meme will be calculated using the following formula: Cost = P + C*Efficiency + Random, where random part is determined by its mean and standard deviation")]
        public static double MemeCostPedestalHuntingBEfficiency { get => memeCostPedestalHuntingBEfficiency; set { memeCostPedestalHuntingBEfficiency = value; PersistChanges(); } }

        [DisplayableProperty("Random part StdDev", group = "Memes\\Invention\\Costs\\Teaching likelyhood")]
        public static double MemeCostRandomStdDevTeachingLikelyhood { get => memeCostRandomStdDevTeachingLikelyhood; set { memeCostRandomStdDevTeachingLikelyhood =value; PersistChanges(); }}
        [DisplayableProperty("Random part mean", group = "Memes\\Invention\\Costs\\Teaching likelyhood")]
        public static double MemeCostRandomAverageTeachingLikelyhood { get => memeCostRandomAverageTeachingLikelyhood; set { memeCostRandomAverageTeachingLikelyhood =value; PersistChanges(); }}
        [DisplayableProperty("Correlation part coefficient (C)", group = "Memes\\Invention\\Costs\\Teaching likelyhood")]
        public static double MemeCostEfficiencyRatioTeachingLikelyhood { get => memeCostEfficiencyRatioTeachingLikelyhood; set { memeCostEfficiencyRatioTeachingLikelyhood =value; PersistChanges(); }}
        [DisplayableProperty("Pedestal value (P)", group = "Memes\\Invention\\Costs\\Teaching likelyhood", description = "The cost of the meme will be calculated using the following formula: Cost = P + C*Efficiency + Random, where random part is determined by its mean and standard deviation")]
        public static double MemeCostPedestalTeachingLikelyhood { get => memeCostPedestalTeachingLikelyhood; set { memeCostPedestalTeachingLikelyhood =value; PersistChanges(); }}

        [DisplayableProperty("Random part StdDev", group = "Memes\\Invention\\Costs\\Teaching efficiency")]
        public static double MemeCostRandomStdDevTeachingEfficiency { get => memeCostRandomStdDevTeachingEfficiency; set { memeCostRandomStdDevTeachingEfficiency =value; PersistChanges(); }}
        [DisplayableProperty("Random part mean", group = "Memes\\Invention\\Costs\\Teaching efficiency")]
        public static double MemeCostRandomAverageTeachingEfficiency { get => memeCostRandomAverageTeachingEfficiency; set { memeCostRandomAverageTeachingEfficiency =value; PersistChanges(); }}
        [DisplayableProperty("Correlation part coefficient (C)", group = "Memes\\Invention\\Costs\\Teaching efficiency")]
        public static double MemeCostEfficiencyRatioTeachingEfficiency { get => memeCostEfficiencyRatioTeachingEfficiency; set { memeCostEfficiencyRatioTeachingEfficiency =value; PersistChanges(); }}
        [DisplayableProperty("Pedestal value (P)", group = "Memes\\Invention\\Costs\\Teaching efficiency", description = "The cost of the meme will be calculated using the following formula: Cost = P + C*Efficiency + Random, where random part is determined by its mean and standard deviation")]
        public static double MemeCostPedestalTeachingEfficiency { get => memeCostPedestalTeachingEfficiency; set { memeCostPedestalTeachingEfficiency =value; PersistChanges(); }}

        [DisplayableProperty("Random part StdDev", group = "Memes\\Invention\\Costs\\Study likelyhood")]
        public static double MemeCostRandomStdDevStudyLikelyhood { get => memeCostRandomStdDevStudyLikelyhood; set { memeCostRandomStdDevStudyLikelyhood =value; PersistChanges(); }}
        [DisplayableProperty("Random part mean", group = "Memes\\Invention\\Costs\\Study likelyhood")]
        public static double MemeCostRandomAverageStudyLikelyhood { get => memeCostRandomAverageStudyLikelyhood; set { memeCostRandomAverageStudyLikelyhood =value; PersistChanges(); }}
        [DisplayableProperty("Correlation part coefficient (C)", group = "Memes\\Invention\\Costs\\Study likelyhood")]
        public static double MemeCostEfficiencyRatioStudyLikelyhood { get => memeCostEfficiencyRatioStudyLikelyhood; set { memeCostEfficiencyRatioStudyLikelyhood =value; PersistChanges(); }}
        [DisplayableProperty("Pedestal value (P)", group = "Memes\\Invention\\Costs\\Study likelyhood", description = "The cost of the meme will be calculated using the following formula: Cost = P + C*Efficiency + Random, where random part is determined by its mean and standard deviation")]
        public static double MemeCostPedestalStudyLikelyhood { get => memeCostPedestalStudyLikelyhood; set { memeCostPedestalStudyLikelyhood =value; PersistChanges(); }}

        [DisplayableProperty("Random part StdDev", group = "Memes\\Invention\\Costs\\Study efficiency")]
        public static double MemeCostRandomStdDevStudyEfficiency { get => memeCostRandomStdDevStudyEfficiency; set { memeCostRandomStdDevStudyEfficiency =value; PersistChanges(); }}
        [DisplayableProperty("Random part mean", group = "Memes\\Invention\\Costs\\Study efficiency")]
        public static double MemeCostRandomAverageStudyEfficiency { get => memeCostRandomAverageStudyEfficiency; set { memeCostRandomAverageStudyEfficiency =value; PersistChanges(); }}
        [DisplayableProperty("Correlation part coefficient (C)", group = "Memes\\Invention\\Costs\\Study efficiency")]
        public static double MemeCostEfficiencyRatioStudyEfficiency { get => memeCostEfficiencyRatioStudyEfficiency; set { memeCostEfficiencyRatioStudyEfficiency =value; PersistChanges(); }}
        [DisplayableProperty("Pedestal value (P)", group = "Memes\\Invention\\Costs\\Study efficiency", description = "The cost of the meme will be calculated using the following formula: Cost = P + C*Efficiency + Random, where random part is determined by its mean and standard deviation")]
        public static double MemeCostPedestalStudyEfficiency { get => memeCostPedestalStudyEfficiency; set { memeCostPedestalStudyEfficiency =value; PersistChanges(); }}

        [DisplayableProperty("Random part StdDev", group = "Memes\\Invention\\Costs\\Trick likelyhood")]
        public static double MemeCostRandomStdDevTrickLikelyhood { get => memeCostRandomStdDevTrickLikelyhood; set { memeCostRandomStdDevTrickLikelyhood =value; PersistChanges(); }}
        [DisplayableProperty("Random part mean", group = "Memes\\Invention\\Costs\\Trick likelyhood")]
        public static double MemeCostRandomAverageTrickLikelyhood { get => memeCostRandomAverageTrickLikelyhood; set { memeCostRandomAverageTrickLikelyhood =value; PersistChanges(); }}
        [DisplayableProperty("Correlation part coefficient (C)", group = "Memes\\Invention\\Costs\\Trick likelyhood")]
        public static double MemeCostEfficiencyRatioTrickLikelyhood { get => memeCostEfficiencyRatioTrickLikelyhood; set { memeCostEfficiencyRatioTrickLikelyhood =value; PersistChanges(); }}
        [DisplayableProperty("Pedestal value (P)", group = "Memes\\Invention\\Costs\\Trick likelyhood", description = "The cost of the meme will be calculated using the following formula: Cost = P + C*Efficiency + Random, where random part is determined by its mean and standard deviation")]
        public static double MemeCostPedestalTrickLikelyhood { get => memeCostPedestalTrickLikelyhood; set { memeCostPedestalTrickLikelyhood =value; PersistChanges(); }}

        [DisplayableProperty("Random part StdDev", group = "Memes\\Invention\\Costs\\Trick efficiency")]
        public static double MemeCostRandomStdDevTrickEfficiency { get => memeCostRandomStdDevTrickEfficiency; set { memeCostRandomStdDevTrickEfficiency =value; PersistChanges(); }}
        [DisplayableProperty("Random part mean", group = "Memes\\Invention\\Costs\\Trick efficiency")]
        public static double MemeCostRandomAverageTrickEfficiency { get => memeCostRandomAverageTrickEfficiency; set { memeCostRandomAverageTrickEfficiency =value; PersistChanges(); }}
        [DisplayableProperty("Correlation part coefficient (C)", group = "Memes\\Invention\\Costs\\Trick efficiency")]
        public static double MemeCostEfficiencyRatioTrickEfficiency { get => memeCostEfficiencyRatioTrickEfficiency; set { memeCostEfficiencyRatioTrickEfficiency =value; PersistChanges(); }}
        [DisplayableProperty("Pedestal value (P)", group = "Memes\\Invention\\Costs\\Trick efficiency", description = "The cost of the meme will be calculated using the following formula: Cost = P + C*Efficiency + Random, where random part is determined by its mean and standard deviation")]
        public static double MemeCostPedestalTrickEfficiency { get => memeCostPedestalTrickEfficiency; set { memeCostPedestalTrickEfficiency =value; PersistChanges(); }}
        
        [DisplayableProperty("Random part StdDev", group = "Memes\\Invention\\Costs\\Ageing rate")]
        public static double MemeCostRandomStdDevAgeingRate { get => memeCostRandomStdDevAgeingRate; set { memeCostRandomStdDevAgeingRate =value; PersistChanges(); }}
        [DisplayableProperty("Random part mean", group = "Memes\\Invention\\Costs\\Ageing rate")]
        public static double MemeCostRandomAverageAgeingRate { get => memeCostRandomAverageAgeingRate; set { memeCostRandomAverageAgeingRate =value; PersistChanges(); }}
        [DisplayableProperty("Correlation part coefficient (C)", group = "Memes\\Invention\\Costs\\Ageing rate")]
        public static double MemeCostEfficiencyRatioAgeingRate { get => memeCostEfficiencyRatioAgeingRate; set { memeCostEfficiencyRatioAgeingRate=value; PersistChanges(); }}
        [DisplayableProperty("Pedestal value (P)", group = "Memes\\Invention\\Costs\\Ageing rate", description = "The cost of the meme will be calculated using the following formula: Cost = P + C*Efficiency + Random, where random part is determined by its mean and standard deviation")]
        public static double MemeCostPedestalAgeingRate { get => memeCostPedestalAgeingRate; set { memeCostPedestalAgeingRate=value; PersistChanges(); }}
       

        [DisplayableProperty("Meme cost complexity price coef.", group = "Memes\\Invention\\Costs", description = "Meme complexity (CMPLX) is a coefficient equal to CMPLX = 2^(MemeEfficiency - MemeEfficiencyMean) / StdDev. It equals 1 if efficiency equals mean, 2 if efficiency is StdDev more than mean, 4 if efficiency is 2StdDevs more than mean, 0.5 of efficiency is StdDev less than mean. This parameter determines how does CMPLX affect meme cost. If set to 0 CMPLX will not affect the cost at all. If set to 1 price will be multiplied by the CMPLX. General equation is Price = UmaffectedPrice * (CMPLX ^ x) where x is this parameter.")]
        public static double MemeCostComplexityPriceCoefficient { get => memeCostComplexityPriceCoefficient; set { memeCostComplexityPriceCoefficient = value; PersistChanges(); } }
        [DisplayableProperty("Complexity to learning chance coef.", group = "Memes\\Transition", description = "If set to 0 meme complexity won't affect the learning success chance. If set to 1 chance will be multiplied by CMPLX coefficient. General equation is LearningChance = UnaffectedLearningChance*(CMPLX ^ x) where is the current parameter.")]
        public static double MemeComplexityToLearningChanceCoefficient { get => memeComplexityToLearningChanceCoefficient; set { memeComplexityToLearningChanceCoefficient = value; PersistChanges(); } }


        [DisplayableProperty("Random part StdDev", group = "Memes\\Invention\\Costs\\Useless memes")]
        public static double MemeCostRandomStdDevUseless { get => memeCostRandomStdDevUseless; set { memeCostRandomStdDevUseless = value; PersistChanges(); } }
        [DisplayableProperty("Random part mean", group = "Memes\\Invention\\Costs\\Useless memes")]
        public static double MemeCostRandomAverageUseless { get => memeCostRandomAverageUseless; set { memeCostRandomAverageUseless = value; PersistChanges(); } }
        [DisplayableProperty("Correlation part coefficient (C)", group = "Memes\\Invention\\Costs\\Useless memes")]
        public static double MemeCostEfficiencyRatioUseless { get => memeCostEfficiencyRatioUseless; set { memeCostEfficiencyRatioUseless = value; PersistChanges(); } }
        [DisplayableProperty("Pedestal value (P)", group = "Memes\\Invention\\Costs\\Useless memes", description = "The cost of the meme will be calculated using the following formula: Cost = P + C*Efficiency + Random, where random part is determined by its mean and standard deviation")]
        public static double MemeCostPedestalUseless { get => memeCostPedestalUseless; set { memeCostPedestalUseless = value; PersistChanges(); } }


        [DisplayableProperty("F-R likelyhod", group = "Genetics\\Brain size\\Memory size boost", description = "Memory will get a boost equal to this parameter multiplied by respective GENETIC value.  [0, infinity)")]
        public static double GeneticFreeRiderPunishmentLikelyhoodToMemoryRatio
        {
            get => geneticFreeRiderPunishmentLikelyhoodToMemoryRatio;
            set { geneticFreeRiderPunishmentLikelyhoodToMemoryRatio = value; PersistChanges(); }
        }

        [DisplayableProperty("F-R determination eff.", group = "Genetics\\Brain size\\Memory size boost")]
        public static double GeneticFreeRiderDeterminationEfficiencytoMemoryRatio
        {
            get => geneticFreeRiderDeterminationEfficiencytoMemoryRatio;
            set { geneticFreeRiderDeterminationEfficiencytoMemoryRatio = value; PersistChanges(); }
        }

        [DisplayableProperty("Creativity", group = "Genetics\\Brain size\\Memory size boost")]
        public static double GeneticCreativityToMemoryRatio
        {
            get => geneticCreativityToMemoryRatio;
            set { geneticCreativityToMemoryRatio = value; PersistChanges(); }
        }

        [DisplayableProperty("Cooperation efficiency", group = "Genetics\\Brain size\\Memory size boost")]
        public static double GeneticCooperationEfficiancyToMemoryRatio
        {
            get => geneticCooperationEfficiancyToMemoryRatio;
            set { geneticCooperationEfficiancyToMemoryRatio = value; PersistChanges(); }
        }

        [DisplayableProperty("Hunting efiiciency", group = "Genetics\\Brain size\\Memory size boost")]
        public static double GeneticHuntingEfficiencyToMemoryRatio
        {
            get => geneticHuntingEfficiencyToMemoryRatio;
            set { geneticHuntingEfficiencyToMemoryRatio = value; PersistChanges(); }
        }

        [DisplayableProperty("Hunting B efiiciency", group = "Genetics\\Brain size\\Memory size boost")]
        public static double GeneticHuntingBEfficiencyToMemoryRatio
        {
            get => geneticHuntingBEfficiencyToMemoryRatio;
            set { geneticHuntingBEfficiencyToMemoryRatio = value; PersistChanges(); }
        }

        [DisplayableProperty("Teachiung likelyhood", group = "Genetics\\Brain size\\Memory size boost")]
        public static double GeneticTeachingLikelyhoodToMemoryRatio
        {
            get => geneticTeachingLikelyhoodToMemoryRatio;
            set { geneticTeachingLikelyhoodToMemoryRatio = value; PersistChanges(); }
        }

        [DisplayableProperty("Teaching efficiency", group = "Genetics\\Brain size\\Memory size boost")]
        public static double GeneticTeachingEfficiencyToMemoryRatio
        {
            get => geneticTeachingEfficiencyToMemoryRatio;
            set { geneticTeachingEfficiencyToMemoryRatio = value; PersistChanges(); }
        }

        [DisplayableProperty("Altruism", group = "Genetics\\Brain size\\Memory size boost")]
        public static double GeneticLikelyhoodOfNotBeingAFreeRiderToMemoryRatio
        {
            get => geneticLikelyhoodOfNotBeingAFreeRiderToMemoryRatio;
            set { geneticLikelyhoodOfNotBeingAFreeRiderToMemoryRatio = value; PersistChanges(); }
        }

        [DisplayableProperty("Study likelyhood", group = "Genetics\\Brain size\\Memory size boost")]
        public static double GeneticStudyLikelyhoodToMemoryRatio
        {
            get => geneticStudyLikelyhoodToMemoryRatio;
            set { geneticStudyLikelyhoodToMemoryRatio = value; PersistChanges(); }
        }

        [DisplayableProperty("Study efficiency", group = "Genetics\\Brain size\\Memory size boost")]
        public static double GeneticStudyEfficiencyToMemoryRatio
        {
            get => geneticStudyEfficiencyToMemoryRatio;
            set { geneticStudyEfficiencyToMemoryRatio = value; PersistChanges(); }
        }

        [DisplayableProperty("Trick likelyhood", group = "Genetics\\Brain size\\Memory size boost")]
        public static double GeneticTrickLikelyhoodToMemoryRatio
        {
            get => geneticTrickLikelyhoodToMemoryRatio;
            set { geneticTrickLikelyhoodToMemoryRatio = value; PersistChanges(); }
        }

        [DisplayableProperty("Trick efficiency", group = "Genetics\\Brain size\\Memory size boost")]
        public static double GeneticTrickEfficiencyToMemoryRatio
        {
            get => geneticTrickEfficiencyToMemoryRatio;
            set { geneticTrickEfficiencyToMemoryRatio = value; PersistChanges(); }
        }


        [DisplayableProperty("Skip life support", group = "Lifestyle\\Steps", description = "Skip the step where life support cost is deducted. 1 - Skip; 0 - Don't skip.")]
        public static double SkipLifeSupportStep
        {
            get { return WorldProperties.skipLifeSupportStep; }
            set { WorldProperties.skipLifeSupportStep = value; PersistChanges(); }
        }

        [DisplayableProperty("Skip meme invention", group = "Lifestyle\\Steps", description = "Skip the step where new random memes are invented. 1 - Skip; 0 - Don't skip.")]
        public static double SkipSpontaneousMemeInventionStep
        {
            get { return WorldProperties.skipSpontaneousMemeInventionStep; }
            set { WorldProperties.skipSpontaneousMemeInventionStep = value; PersistChanges(); }
        }

        [DisplayableProperty("Skip forgetting memes", group = "Lifestyle\\Steps", description = "Skip the step where unused memes are forgotten. 1 - Skip; 0 - Don't skip.")]
        public static double SkipForgettingMemesStep
        {
            get { return WorldProperties.skipForgettingMemesStep; }
            set { WorldProperties.skipForgettingMemesStep = value; PersistChanges(); }
        }

        [DisplayableProperty("Skip teaching", group = "Lifestyle\\Steps", description = "Skip the step where teacher is passing memes to random fellow tribesmen. 1 - Skip; 0 - Don't skip.")]
        public static double SkipTeachingStep
        {
            get { return WorldProperties.skipTeachingStep; }
            set { WorldProperties.skipTeachingStep = value; PersistChanges(); }
        }

        [DisplayableProperty("Skip free rider punishment", group = "Lifestyle\\Steps", description = "Skip the step where free riders are punished. 1 - Skip; 0 - Don't skip.")]
        public static double SkipFreeRiderPunishmentStep
        {
            get { return WorldProperties.skipFreeRiderPunishmentStep; }
            set { WorldProperties.skipFreeRiderPunishmentStep = value; PersistChanges(); }
        }

        [DisplayableProperty("Skip hunting", group = "Lifestyle\\Steps", description = "Skip the step where tribes are hunting and sharing the loot. Disabling will likely cause extinction. 1 - Skip; 0 - Don't skip.")]
        public static double SkipHuntingStep
        {
            get { return WorldProperties.skipHuntingStep; }
            set { WorldProperties.skipHuntingStep = value; PersistChanges(); }
        }

        [DisplayableProperty("Skip hunting B", group = "Lifestyle\\Steps", description = "Skip the step where tribes are hunting of B resource and sharing the loot. Disabling will likely cause extinction. 1 - Skip; 0 - Don't skip.")]
        public static double SkipHuntingBStep
        {
            get { return WorldProperties.skipHuntingBStep; }
            set { WorldProperties.skipHuntingBStep = value; PersistChanges(); }
        }

        [DisplayableProperty("Skip useless actions", group = "Lifestyle\\Steps", description = "Skip the step where useless actions are performed. 1 - Skip; 0 - Don't skip.")]
        public static double SkipUselessActionsStep
        {
            get { return WorldProperties.skipUselessActionsStep; }
            set { WorldProperties.skipUselessActionsStep = value; PersistChanges(); }
        }

        [DisplayableProperty("Skip study", group = "Lifestyle\\Steps", description = "Skip the step where memes are passed with the students' initiative. 1 - Skip; 0 - Don't skip.")]
        public static double SkipStudyingStep
        {
            get { return WorldProperties.skipStudyingStep; }
            set { WorldProperties.skipStudyingStep = value; PersistChanges(); }
        }

        [DisplayableProperty("Skip death", group = "Lifestyle\\Steps", description = "Skip the step where tribesmen die of hunger and natural causes. Disabling will lead to outside of the model migration being the only population limiter. 1 - Skip; 0 - Don't skip.")]
        public static double SkipDeathStep
        {
            get { return WorldProperties.skipDeathStep; }
            set { WorldProperties.skipDeathStep = value; PersistChanges(); }
        }

        [DisplayableProperty("Skip breeding", group = "Lifestyle\\Steps", description = "Skip the step where new tribesmen are born. Disabling will likely lead to extinction. 1 - Skip; 0 - Don't skip.")]
        public static double SkipBreedingStep
        {
            get { return WorldProperties.skipBreedingStep; }
            set { WorldProperties.skipBreedingStep = value; PersistChanges(); }
        }

        [DisplayableProperty("Skip group splitting", group = "Lifestyle\\Steps", description = "Skip the step where  groups which had become too big splitting. 1 - Skip; 0 - Don't skip.")]
        public static double SkipGroupSplittingStep
        {
            get { return WorldProperties.skipGroupSplittingStep; }
            set { WorldProperties.skipGroupSplittingStep = value; PersistChanges(); }
        }

        [DisplayableProperty("Skip migration", group = "Lifestyle\\Steps", description = "Skip the step where tribesmen migrate to other tribes. 1 - Skip; 0 - Don't skip.")]
        public static double SkipMigrationStep
        {
            get { return WorldProperties.skipMigrationStep; }
            set { WorldProperties.skipMigrationStep = value; PersistChanges(); }
        }

        [DisplayableProperty("Skip cultural exchange", group = "Lifestyle\\Steps", description = "Skip the step where memes are exchanged between tribes. 1 - Skip; 0 - Don't skip.")]
        public static double SkipCulturalExchangeStep
        {
            get { return WorldProperties.skipCulturalExchangeStep; }
            set { WorldProperties.skipCulturalExchangeStep = value; PersistChanges(); }
        }

        [DisplayableProperty("Ignore Multithreading (debug only)", group = "Program settings", description = "1 - ignore multithreading. This feature is good for debuging - step by step and profiling.")]
        public static double IgnoreMultithreading { get => WorldProperties.ignoreMultithreading; set => WorldProperties.ignoreMultithreading = value; }

        [DisplayableProperty("Collect phenotype averages", group = "Program settings", description = "1 - a graph with average phenotype values will be available 0 - the model will run faster")]
        public static double CollectPhenotypeValues
        {
            get { return WorldProperties.collectPhenotypeValues; }
            set { WorldProperties.collectPhenotypeValues = value; PersistChanges(); }
        }

        [DisplayableProperty("Collect genotype averages", group = "Program settings", description = "1 - a graph with average genetic values will be available 0 - the model will run faster")]
        public static double CollectGenotypeValues
        {
            get { return WorldProperties.collectGenotypeValues; }
            set { WorldProperties.collectGenotypeValues = value; PersistChanges(); }
        }

        [DisplayableProperty("Meme diary chance", group = "Program settings", description = "Chances that a meme will keep the diary. 0 - no one will, 1 - everyone will. A tradeoff between speed and having a full picture. [0, 1]")]
        public static double ChancesThatMemeWillWriteALog
        {
            get { return WorldProperties.chancesThatMemeWillWriteALog; }
            set { WorldProperties.chancesThatMemeWillWriteALog = value; PersistChanges(); }
        }

        [DisplayableProperty("Tribe diary chance", group = "Program settings", description = "Chances that a tribe will keep the diary. 0 - no one will, 1 - everyone will. A tradeoff between speed and having a full picture. [0, 1]")]
        public static double ChancesThatTribeWillWriteALog
        {
            get { return WorldProperties.chancesThatTribeWillWriteALog; }
            set { WorldProperties.chancesThatTribeWillWriteALog = value; PersistChanges(); }
        }

        [DisplayableProperty("Tribesman diary chance", group = "Program settings", description = "Chances that a tribesman will keep the diary. 0 - no one will, 1 - everyone will. A tradeoff between speed and having a full picture. [0, 1]")]
        public static double ChancesToWriteALog
        {
            get { return WorldProperties.chancesToWriteALog; }
            set { WorldProperties.chancesToWriteALog = value; }
        }

        [DisplayableProperty("Max Tribes", group = "Environment", description = "If the number of tribes will grow above this point random tribes will migrate outside of the model. [1, infinity)")]
        public static double MaximumTribesAllowedInTheWorld
        {
            get { return WorldProperties.maximumTribesAllowedInTheWorld; }
            set { WorldProperties.maximumTribesAllowedInTheWorld = value; PersistChanges(); }
        }


        [DisplayableProperty("Group migration threshold", group = "Lifestyle\\Migartion and cultural exchange", description = "No migration will occur from tribes of this size and below [0, infinity)")]
        public static double MigrateFromGroupsLargerThan
        {
            get { return WorldProperties.migrateFromGroupsLargerThan; }
            set { WorldProperties.migrateFromGroupsLargerThan = value; PersistChanges(); }
        }

        [DisplayableProperty("Migration chance", group = "Lifestyle\\Migartion and cultural exchange", description = "The chance that a tribesman will migrate from one group that is too big to a random one. [0, 1]")]
        public static double MigrationChance
        {
            get { return WorldProperties.migrationChance; }
            set { WorldProperties.migrationChance = value; PersistChanges(); }
        }

        [DisplayableProperty("Cultural exchange chance", group = "Lifestyle\\Migartion and cultural exchange", description = "The chance that two tribesmen from different tribes will meet and attempt a teaching round. [0, 1]")]
        public static double CulturalExchangeTeachingChance
        {
            get { return WorldProperties.culturalExchangeTeachingChance; }
            set { WorldProperties.culturalExchangeTeachingChance = value; PersistChanges(); }
        }

        [DisplayableProperty("Repeat cultural exchange", group = "Lifestyle\\Migartion and cultural exchange", description = "If set to 1 consecutive teaching rounds can take place. New random tribes and tribesmen would be selected. 1 - allow, 0 - disallow")]
        public static double AllowRepetitiveCulturalExchange
        {
            get { return WorldProperties.allowRepetitiveCulturalExchange; }
            set { WorldProperties.allowRepetitiveCulturalExchange = value; }
        }

        [DisplayableProperty("Tribe Split Threshold", group = "Lifestyle\\Groups", description = "Tribe will split in two if contains more members than this amount.")]
        public static double SplitTribeIfBiggerThen
        {
            get { return WorldProperties.splitTribeIfBiggerThen; }
            set { WorldProperties.splitTribeIfBiggerThen = value; PersistChanges(); }
        }

        [DisplayableProperty("Tribe Split Ratio", group = "Lifestyle\\Groups", description = "The ratio at which a tribe would be split. 0 - no members will join new tribe. 0.5 - half of the members will join new tribe. 1 - all members will join new tribe. (0, 1)")]
        public static double SplitTribeRatio
        {
            get { return WorldProperties.splitTribeRatio; }
            set { WorldProperties.splitTribeRatio = value; PersistChanges(); }
        }

        [DisplayableProperty("A", group = "Lifestyle\\Breeding\\Starting Resource")]
        public static double ChildStartingResourcePedestal
        {
            get { return WorldProperties.childStartingResourcePedestal; }
            set { WorldProperties.childStartingResourcePedestal = value; PersistChanges(); }
        }

        [DisplayableProperty("B", group = "Lifestyle\\Breeding\\Starting Resource")]
        public static double ChildStartingResourceSpendingsReceivedCoefficient
        {
            get { return WorldProperties.childStartingResourceSpendingsReceivedCoefficient; }
            set { WorldProperties.childStartingResourceSpendingsReceivedCoefficient = value; PersistChanges(); }
        }

        [DisplayableProperty("C", group = "Lifestyle\\Breeding\\Starting Resource", description = "The child will recieve the following amount of resources: A + B*ResourcesSpentByParents + C*ParentsWealth. A+C*ParentsWealth will be deducted in equal shares from parents resources. ParentsWealth is calculates after initial birth costs are deducted.")]
        public static double ChildStartingResourceParentsCoefficient
        {
            get { return WorldProperties.childStartingResourceParentsCoefficient; }
            set { WorldProperties.childStartingResourceParentsCoefficient = value; PersistChanges(); }
        }


        [DisplayableProperty("Free teaching rounds", group = "Lifestyle\\Breeding", description = "The number of free teaching rounds each parent gets for free. [0, infinity]")]
        public static double FreeTeachingRoundsForParents
        {
            get { return WorldProperties.freeTeachingRoundsForParents; }
            set { WorldProperties.freeTeachingRoundsForParents = value; PersistChanges(); }
        }

        [DisplayableProperty("Brain size pedestal P=", group = "Genetics\\Brain size", description = "The brain size is determined by the following formula. BrainSize = P + C1*TrickEfficiency + C2*TeachingEfficiency + C3*StudyEfficiency + C4*FreeRiderDeterminationEfficiency + C5*HuntingEfficiency + C6*CooperationEfficieny + C7*MemorySize + C8*Creativity. All coefficients [-infinity, infinity]")]
        public static double BrainSizePedestal
        {
            get { return WorldProperties.brainSizePedestal; }
            set { WorldProperties.brainSizePedestal = value; PersistChanges(); }
        }

        [DisplayableProperty("Maximum breeding age", group = "Lifestyle\\Breeding", description = "Tribesmen older than this value can not reproduce. If set to 0 the breding will not be limited by the age.")]
        public static double MaximumBreedingAge {
            get { return maximumBreedingAge; }
            set { maximumBreedingAge = value; PersistChanges(); }
        }

        [DisplayableProperty("Increase after", group = "Lifestyle\\Breeding\\Costs", description = "Reproduction costs start to increase at this age. [0, infinity]")]
        public static double BreedingCostsIncreaseAge {
            get { return breedingCostsIncreaseAge; }
            set { breedingCostsIncreaseAge = value; PersistChanges(); }
        }

        [DisplayableProperty("Increase coefficient", group = "Lifestyle\\Breeding\\Costs", description = "Reproduction costs will increase by A*x, where A is this parameter and x is the difference between the age and 'Increase after' parameter.")]
        public static double BreedingCostsIncreaseCoefficient {
            get { return breedingCostsIncreaseCoefficient; }
            set { breedingCostsIncreaseCoefficient = value; PersistChanges(); }
        }


        [DisplayableProperty("C1*TrickEfficiency C1=", group = "Genetics\\Brain size")]
        public static double BrainSizeToTrickEfficiencyCoefficient
        {
            get { return WorldProperties.brainSizeToTrickEfficiencyCoefficient; }
            set { WorldProperties.brainSizeToTrickEfficiencyCoefficient = value; PersistChanges(); }
        }

        [DisplayableProperty("C2*TeachingEfficiency C2=", group = "Genetics\\Brain size")]
        public static double BrainSizeToTeachingEfficiencyCoefficient
        {
            get { return WorldProperties.brainSizeToTeachingEfficiencyCoefficient; }
            set { WorldProperties.brainSizeToTeachingEfficiencyCoefficient = value; PersistChanges(); }
        }

        [DisplayableProperty("C3*StudyEfficiency C3=", group = "Genetics\\Brain size")]
        public static double BrainSizeToStudyEfficiencyCoefficient
        {
            get { return WorldProperties.brainSizeToStudyEfficiencyCoefficient; }
            set { WorldProperties.brainSizeToStudyEfficiencyCoefficient = value; PersistChanges(); }
        }

        [DisplayableProperty("C4*FreeRiderDeterminationEfficiency C4=", group = "Genetics\\Brain size")]
        public static double BrainSizeToFreeRiderDeterminationEfficiencyCoefficient
        {
            get { return WorldProperties.brainSizeToFreeRiderDeterminationEfficiencyCoefficient; }
            set { WorldProperties.brainSizeToFreeRiderDeterminationEfficiencyCoefficient = value; PersistChanges(); }
        }

        [DisplayableProperty("C5*HuntingEfficiency C5=", group = "Genetics\\Brain size")]
        public static double BrainSizeToHuntingEfficiencyCoefficient
        {
            get { return WorldProperties.brainSizeToHuntingEfficiencyCoefficient; }
            set { WorldProperties.brainSizeToHuntingEfficiencyCoefficient = value; PersistChanges(); }
        }

        [DisplayableProperty("C6*CooperationEfficieny C6=", group = "Genetics\\Brain size")]
        public static double BrainSizeToCooperationEfficiencyCoefficient
        {
            get { return WorldProperties.brainSizeToCooperationEfficiencyCoefficient; }
            set { WorldProperties.brainSizeToCooperationEfficiencyCoefficient = value; PersistChanges(); }
        }

        [DisplayableProperty("C7*MemorySize C7=", group = "Genetics\\Brain size")]
        public static double BrainSizeToMemorySizeCoefficient
        {
            get { return WorldProperties.brainSizeToMemorySizeCoefficient; }
            set { WorldProperties.brainSizeToMemorySizeCoefficient = value; PersistChanges(); }
        }

        [DisplayableProperty("C8*Creativity C8=", group = "Genetics\\Brain size")]
        public static double BrainSizeToCreativityCoefficient
        {
            get { return WorldProperties.brainSizeToCreativityCoefficient; }
            set { WorldProperties.brainSizeToCreativityCoefficient = value; PersistChanges(); }
        }

        [DisplayableProperty("C9*HuntingBEfficiency C9=", group = "Genetics\\Brain size")]
        public static double BrainSizeToHuntingBEfficiencyCoefficient
        {
            get { return WorldProperties.brainSizeToHuntingBEfficiencyCoefficient; }
            set { WorldProperties.brainSizeToHuntingBEfficiencyCoefficient = value; PersistChanges(); }
        }

        [DisplayableProperty("Brain size birth price", group = "Lifestyle\\Breeding", description = "How does a brain size relate to the resources required to get a child. Resources = BrainSize*This Coefficient [0, infinity]")]
        public static double BrainSizeBirthPriceCoefficient
        {
            get { return WorldProperties.brainSizeBirthPriceCoefficient; }
            set { WorldProperties.brainSizeBirthPriceCoefficient = value; PersistChanges(); }
        }

        [DisplayableProperty("Youth age", group = "Lifestyle\\Breeding", description = "A tribesman doesn't participate in breeding if younger than this value. [0, infinity]")]
        public static double DontBreedIfYoungerThan
        {
            get { return WorldProperties.dontBreedIfYoungerThan; }
            set { WorldProperties.dontBreedIfYoungerThan = value; PersistChanges(); }
        }

        [DisplayableProperty("Youth age", group = "Lifestyle\\Death", description = "A tribesman doesn't die if younger than this value.")]
        public static double DontDieIfYoungerThan
        {
            get { return WorldProperties.dontDieIfYoungerThan; }
            set { WorldProperties.dontDieIfYoungerThan = value; PersistChanges(); }
        }

        [DisplayableProperty("A", group = "Lifestyle\\Death")]
        public static double DeathLinearChance
        {
            get { return WorldProperties.deathLinearChance; }
            set { WorldProperties.deathLinearChance = value; PersistChanges(); }
        }

        [DisplayableProperty("B", group = "Lifestyle\\Death", description = "When a tribesman is older than the youth age his chances of dying of age equal A + B*Age")]
        public static double DeathAgeDependantChance
        {
            get { return WorldProperties.deathAgeDependantChance; }
            set { WorldProperties.deathAgeDependantChance = value; PersistChanges(); }
        }


        [DisplayableProperty("Allow repetitive useless actions", group = "Lifestyle", description = "If set to 1 then after performing a useless action a tribesman will have the same chance of performing the next one ans so on. If set to 0 only one useless action can be performed.")]
        public static double AllowRepetitiveUselessActions
        {
            get { return WorldProperties.allowRepetitiveUselessActions; }
            set { WorldProperties.allowRepetitiveUselessActions = value; PersistChanges(); }
        }

        [DisplayableProperty("Useless action costs", group = "Lifestyle", description = "The amount of resources that will be wasted for a useless action. [0, infinity)")]
        public static double UselessActionCost
        {
            get { return WorldProperties.uselessActionCost; }
            set { WorldProperties.uselessActionCost = value; PersistChanges(); }
        }

        [DisplayableProperty("Hunting costs", group = "Lifestyle", description = "The amount of resources that will be deducted for hunting. [0, infinity) First resource")]
        public static double HuntingCosts
        {
            get { return WorldProperties.huntingCosts; }
            set { WorldProperties.huntingCosts = value; PersistChanges(); }
        }

        [DisplayableProperty("Hunting B costs", group = "Lifestyle", description = "The amount of resources that will be deducted for hunting. [0, infinity) B resource")]
        public static double HuntingBCosts
        {
            get { return WorldProperties.huntingBCosts; }
            set { WorldProperties.huntingBCosts = value; PersistChanges(); }
        }

        [DisplayableProperty("Environment support", group = "Environment", description = "The amount of resources that environment can provide for the hunters. If resources is not enough to match total hunting efforts of all tribes the tribes will receive resources respective to their share in total hunting effort. [0, infinity) or -1 for unlimited")]
        public static double ResourcesAvailableFromEnvironmentOnEveryStep
        {
            get { return WorldProperties.resourcesAvailableFromEnvironmentOnEveryStep; }
            set { WorldProperties.resourcesAvailableFromEnvironmentOnEveryStep = value; PersistChanges(); }
        }

        [DisplayableProperty("Environment support standard deviation", group = "Environment", description = "Стандартное отклонение количества ресурсов на каждый ход. Отсекаются варианты меньше 0")]
        public static double ResourcesAvailableFromEnvironmentOnEveryStepStdDev
        {
            get { return WorldProperties.resourcesAvailableFromEnvironmentOnEveryStepStdDev; }
            set { WorldProperties.resourcesAvailableFromEnvironmentOnEveryStepStdDev = value; PersistChanges(); }
        }

        [DisplayableProperty("Environment support deviation limit", group = "Environment", description = "Ограничение выбросов в количестве ресурсов, измеряется в сигмах. Значение 3 значит отсекаться будут попытки случайно сгенерировать количество ресурсов отличающееся от среднего больше чем 3 сигмы стандартного отклонения")]
        public static double ResourcesAvailableFromEnvironmentOnEveryStepDeviationLimit
        {
            get { return WorldProperties.resourcesAvailableFromEnvironmentOnEveryStepDeviationLimit; }
            set { WorldProperties.resourcesAvailableFromEnvironmentOnEveryStepDeviationLimit = value; PersistChanges(); }
        }

        [DisplayableProperty("Resources A to B Replacement Period", group = "Environment", description = "Период полного переключения с основного ресурса на ресурс B и обратно по функции cos. 0 - означает отсутствие переключения.")]
        public static double ResourcesABReplacementPeriod
        {
            get { return WorldProperties.resourcesABReplacementPeriod; }
            set { WorldProperties.resourcesABReplacementPeriod = value; PersistChanges(); }
        }

        [DisplayableProperty("Resources A Minimum", group = "Environment", description = "Минимальное значение до которого количество основного ресурса снижается в нижней точке цикла.")]
        public static double ResourcesAAvailableFromEnvironmentMinimum
        {
            get { return WorldProperties.resourcesAAvailableFromEnvironmentMinimum; }
            set { WorldProperties.resourcesAAvailableFromEnvironmentMinimum = value; PersistChanges(); }
        }

        [DisplayableProperty("Resources B Available From Environment On Every Step", group = "Environment", description = "Максимальное количество ресурса B, которое буедет получаться когда периодическое изменение уменьшит ресурс A до минимума. Обычно такого же размера как и Environment support, чтобы перекладывание из одного в другое не меняло суммарный возможный ресурс. 0 - Означает, что фаза GoHuntingB вообще игнорируется.")]
        public static double ResourcesBAvailableFromEnvironmentOnEveryStep
        {
            get { return WorldProperties.resourcesBAvailableFromEnvironmentOnEveryStep; }
            set { WorldProperties.resourcesBAvailableFromEnvironmentOnEveryStep = value; PersistChanges(); }
        }

        [DisplayableProperty("Environment B support standard deviation", group = "Environment", description = "Стандартное отклонение случайного изменения количества ресурсов B на каждый ход. Отсекаются варианты меньше 0")]
        public static double ResourcesBAvailableFromEnvironmentOnEveryStepStdDev
        {
            get { return WorldProperties.resourcesBAvailableFromEnvironmentOnEveryStepStdDev; }
            set { WorldProperties.resourcesBAvailableFromEnvironmentOnEveryStepStdDev = value; PersistChanges(); }
        }

        [DisplayableProperty("Environment B support deviation limit", group = "Environment", description = "Ограничение выбросов в количестве ресурсов B, измеряется в сигмах. Значение 3 значит отсекаться будут попытки случайно сгенерировать количество ресурсов отличающееся от среднего больше чем 3 сигмы стандартного отклонения")]
        public static double ResourcesBAvailableFromEnvironmentOnEveryStepDeviationLimit
        {
            get { return WorldProperties.resourcesBAvailableFromEnvironmentOnEveryStepDeviationLimit; }
            set { WorldProperties.resourcesBAvailableFromEnvironmentOnEveryStepDeviationLimit = value; PersistChanges(); }
        }

        [DisplayableProperty("Resources B Minimum", group = "Environment", description = "Минимальное значение до которого количество ресурса B снижается в нижней точке цикла.")]
        public static double ResourcesBAvailableFromEnvironmentMinimum
        {
            get { return WorldProperties.resourcesBAvailableFromEnvironmentMinimum; }
            set { WorldProperties.resourcesBAvailableFromEnvironmentMinimum = value; PersistChanges(); }
        }

        [DisplayableProperty("Cost to punish a free rider", group = "Lifestyle\\Free riders", description = "The amount of resources that will be deducted from the punisher for the attempt to punish a free rider. The amount will be taken disregarding whether the punished one is a free rider or not. [0, infinity)")]
        public static double FreeRiderPunishmentCosts
        {
            get { return WorldProperties.freeRiderPunishmentCosts; }
            set { WorldProperties.freeRiderPunishmentCosts = value; PersistChanges(); }
        }

        [DisplayableProperty("Free rider punishment", group = "Lifestyle\\Free riders", description = "The amount of resources that will be taken and destroyed as a punishment for being a free rider [0, infinity)")]
        public static double FreeRiderPunishmentAmount
        {
            get { return WorldProperties.freeRiderPunishmentAmount; }
            set { WorldProperties.freeRiderPunishmentAmount = value; PersistChanges(); }
        }

        [DisplayableProperty("Usage creativity modifier", group = "Memes\\Invention", description = "This modifier will be applied to basic creativity to increase or decrease the chance to invent new meme while performing the relevant action. The overall formula is C*M + T - C*M*T (the sum of probabilities C*M and T) where C is creativity, M is this modifier and T is the additional threshold. [0, infinity), Ex: 1 - apply basic crativity, 2 - basic creativity is doubled, 0.5 basic creativity is halved")]
        public static double ChanceToInventNewMemeWhileUsingItModifier
        {
            get { return WorldProperties.chanceToInventNewMemeWhileUsingItModifier; }
            set { WorldProperties.chanceToInventNewMemeWhileUsingItModifier = value; PersistChanges(); }
        }

        [DisplayableProperty("Usage creativity threshold", group = "Memes\\Invention", description = "Additional probability of inventing a new meme while performing the relevant action. The T from the formual above. [0, 1]")]
        public static double ChanceToInventNewMemeWhileUsingItThreshold
        {
            get { return WorldProperties.chanceToInventNewMemeWhileUsingItThreshold; }
            set { WorldProperties.chanceToInventNewMemeWhileUsingItThreshold = value; PersistChanges(); }
        }

        [DisplayableProperty("Allow repetitive teaching", group = "Lifestyle\\Teaching", description = "If set to 1 then after teaching the teacher will have the same chance to teach again and so on. If set to 0 only one teaching action can be performed.")]
        public static double AllowRepetitiveTeaching
        {
            get { return WorldProperties.allowRepetitiveTeaching; }
            set { WorldProperties.allowRepetitiveTeaching = value; PersistChanges(); }
        }

        [DisplayableProperty("Teaching costs", group = "Lifestyle\\Teaching", description = "The amount of resources that will be deducted from the teacher when he attempts to pass a meme to a student [0, infinity)")]
        public static double TeachingCosts
        {
            get { return WorldProperties.teachingCosts; }
            set { WorldProperties.teachingCosts = value; PersistChanges(); }
        }

        [DisplayableProperty("Chance to hungry-teach", group = "Lifestyle\\Teaching", description = "The chance that a teacher will not ignore his desire to teach when he doesn't have enough resourses putting himself at risk. [0, 1]")]
        public static double ChanceToTeachIfUnsufficienResources
        {
            get { return WorldProperties.chanceToTeachIfUnsufficienResources; }
            set { WorldProperties.chanceToTeachIfUnsufficienResources = value; PersistChanges(); }
        }

        [DisplayableProperty("Study costs", group = "Lifestyle\\Teaching", description = "The amount of resources that will be deducted from the student when he attempts to copy a meme from someone else [0, infinity)")]
        public static double StudyCosts
        {
            get { return WorldProperties.studyCosts; }
            set { WorldProperties.studyCosts = value; PersistChanges(); }
        }

        [DisplayableProperty("Allow repetitive studying", group = "Lifestyle\\Teaching", description = "If set to 1 then after studying a meme from a tribesmate the student will have the same chance to learn someone else's and so on until a study fails. If set to 0 only one study action can be performed.")]
        public static double AllowRepetitiveStudying
        {
            get { return WorldProperties.allowRepetitiveStudying; }
            set { WorldProperties.allowRepetitiveStudying = value; PersistChanges(); }
        }

        [DisplayableProperty("Recent memes protection window", group = "Memes", description = "Don't forget the memes that were used for this number of previous turns. [0, infinity)")]
        public static double DontForgetMemesThatWereUsedDuringThisPeriod
        {
            get { return WorldProperties.dontForgetMemesThatWereUsedDuringThisPeriod; }
            set { WorldProperties.dontForgetMemesThatWereUsedDuringThisPeriod = value; PersistChanges(); }
        }

        [DisplayableProperty("Chance to forget unused meme", group = "Memes", description = "When the meme is not used for a certain period of time (defined below) it will have this chance to be forgotten on every turn. [0, 1]")]
        public static double ChanceToForgetTheUnusedMeme
        {
            get { return WorldProperties.chanceToForgetTheUnusedMeme; }
            set { WorldProperties.chanceToForgetTheUnusedMeme = value; PersistChanges(); }
        }


        [DisplayableProperty("Starting amount of resources", group = "Initial state", description = "Amount fo resources a tribesman has at the onset.")]
        public static double StatringAmountOfResources
        {
            get { return WorldProperties.statringAmountOfResources; }
            set { WorldProperties.statringAmountOfResources = value; PersistChanges(); }
        }

        [DisplayableProperty("Mean", group = "Initial state\\Genes\\Trick likelyhood")]
        public static double InitialStateGenesTrickLikelyhoodMean
        {
            get { return WorldProperties.initialStateGenesTrickLikelyhoodMean; }
            set { WorldProperties.initialStateGenesTrickLikelyhoodMean = value; PersistChanges(); }
        }

        [DisplayableProperty("Standard deviation", group = "Initial state\\Genes\\Trick likelyhood", description = "Genetically defined chances of playing a trick to get more food. [0, 1]")]
        public static double InitialStateGenesTrickLikelyhoodStdDev
        {
            get { return WorldProperties.initialStateGenesTrickLikelyhoodStdDev; }
            set { WorldProperties.initialStateGenesTrickLikelyhoodStdDev = value; PersistChanges(); }
        }

        [DisplayableProperty("Mean", group = "Initial state\\Genes\\Trick efficiency")]
        public static double InitialStateGenesTrickEfficiencyMean
        {
            get { return WorldProperties.initialStateGenesTrickEfficiencyMean; }
            set { WorldProperties.initialStateGenesTrickEfficiencyMean = value; PersistChanges(); }
        }

        [DisplayableProperty("Standard deviation", group = "Initial state\\Genes\\Trick efficiency", description = "Genetically defined efficiency of a played trick. [0, infinity)")]
        public static double InitialStateGenesTrickEfficiencyStdDev
        {
            get { return WorldProperties.initialStateGenesTrickEfficiencyStdDev; }
            set { WorldProperties.initialStateGenesTrickEfficiencyStdDev = value; PersistChanges(); }
        }

        [DisplayableProperty("Mean", group = "Initial state\\Genes\\Teaching likelyhood")]
        public static double InitialStateGenesTeachingLikelyhoodMean
        {
            get { return WorldProperties.initialStateGenesTeachingLikelyhoodMean; }
            set { WorldProperties.initialStateGenesTeachingLikelyhoodMean = value; PersistChanges(); }
        }

        [DisplayableProperty("Standard deviation", group = "Initial state\\Genes\\Teaching likelyhood", description = "Genetically defined chance to initiate teaching. [0, 1)")]
        public static double InitialStateGenesTeachingLikelyhoodStdDev
        {
            get { return WorldProperties.initialStateGenesTeachingLikelyhoodStdDev; }
            set { WorldProperties.initialStateGenesTeachingLikelyhoodStdDev = value; PersistChanges(); }
        }

        [DisplayableProperty("Mean", group = "Initial state\\Genes\\Teaching efficiency")]
        public static double InitialStateGenesTeachingEfficiencyMean
        {
            get { return WorldProperties.initialStateGenesTeachingEfficiencyMean; }
            set { WorldProperties.initialStateGenesTeachingEfficiencyMean = value; PersistChanges(); }
        }

        [DisplayableProperty("Standard deviation", group = "Initial state\\Genes\\Teaching efficiency", description = "Genetically defined chance of successfull teaching. (Also affected by study efficiency). [0, 1)")]
        public static double InitialStateGenesTeachingEfficiencyStdDev
        {
            get { return WorldProperties.initialStateGenesTeachingEfficiencyStdDev; }
            set { WorldProperties.initialStateGenesTeachingEfficiencyStdDev = value; PersistChanges(); }
        }

        [DisplayableProperty("Mean", group = "Initial state\\Genes\\Study likelyhood")]
        public static double InitialStateGenesStudyLikelyhoodMean
        {
            get { return WorldProperties.initialStateGenesStudyLikelyhoodMean; }
            set { WorldProperties.initialStateGenesStudyLikelyhoodMean = value; PersistChanges(); }
        }

        [DisplayableProperty("Standard deviation", group = "Initial state\\Genes\\Study likelyhood", description = "Genetically defined chance to initiate study. [0, 1)")]
        public static double InitialStateGenesStudyLikelyhoodStdDev
        {
            get { return WorldProperties.initialStateGenesStudyLikelyhoodStdDev; }
            set { WorldProperties.initialStateGenesStudyLikelyhoodStdDev = value; PersistChanges(); }
        }

        [DisplayableProperty("Mean", group = "Initial state\\Genes\\Study efficiency")]
        public static double InitialStateGenesStudyEfficiencyMean
        {
            get { return WorldProperties.initialStateGenesStudyEfficiencyMean; }
            set { WorldProperties.initialStateGenesStudyEfficiencyMean = value; PersistChanges(); }
        }

        [DisplayableProperty("Standard deviation", group = "Initial state\\Genes\\Study efficiency", description = "Genetically defined chance of successfull study. [0, 1)")]
        public static double InitialStateGenesStudyEfficiencyStdDev
        {
            get { return WorldProperties.initialStateGenesStudyEfficiencyStdDev; }
            set { WorldProperties.initialStateGenesStudyEfficiencyStdDev = value; PersistChanges(); }
        }

        [DisplayableProperty("Mean", group = "Initial state\\Genes\\Free rider punishment likelyhood")]
        public static double InitialStateGenesFreeRiderPunishmentLikelyhoodMean
        {
            get { return WorldProperties.initialStateGenesFreeRiderPunishmentLikelyhoodMean; }
            set { WorldProperties.initialStateGenesFreeRiderPunishmentLikelyhoodMean = value; PersistChanges(); }
        }

        [DisplayableProperty("Standard deviation", group = "Initial state\\Genes\\Free rider punishment likelyhood", description = "Genetically defined desire (chance) to punish free riders. [0, 1)")]
        public static double InitialStateGenesFreeRiderPunishmentLikelyhoodStdDev
        {
            get { return WorldProperties.initialStateGenesFreeRiderPunishmentLikelyhoodStdDev; }
            set { WorldProperties.initialStateGenesFreeRiderPunishmentLikelyhoodStdDev = value; PersistChanges(); }
        }

        [DisplayableProperty("Mean", group = "Initial state\\Genes\\Free rider determination efficiency")]
        public static double InitialStateGenesFreeRiderDeterminationEfficiencyMean
        {
            get { return WorldProperties.initialStateGenesFreeRiderDeterminationEfficiencyMean; }
            set { WorldProperties.initialStateGenesFreeRiderDeterminationEfficiencyMean = value; PersistChanges(); }
        }

        [DisplayableProperty("Standard deviation", group = "Initial state\\Genes\\Free rider determination efficiency", description = "Genetically defined chance to successfully spot a free rider. [0, 1)")]
        public static double InitialStateGenesFreeRiderDeterminationEfficiencyStdDev
        {
            get { return WorldProperties.initialStateGenesFreeRiderDeterminationEfficiencyStdDev; }
            set { WorldProperties.initialStateGenesFreeRiderDeterminationEfficiencyStdDev = value; PersistChanges(); }
        }

        [DisplayableProperty("Mean", group = "Initial state\\Genes\\Cooperation desire")]
        public static double InitialStateGenesLikelyhoodOfNotBeingAFreeRiderMean
        {
            get { return WorldProperties.initialStateGenesLikelyhoodOfNotBeingAFreeRiderMean; }
            set { WorldProperties.initialStateGenesLikelyhoodOfNotBeingAFreeRiderMean = value; PersistChanges(); }
        }

        [DisplayableProperty("Standard deviation", group = "Initial state\\Genes\\Cooperation desire", description = "Genetically defined altruism. Chance to participate in a group hunt. [0, 1)")]
        public static double InitialStateGenesLikelyhoodOfNotBeingAFreeRiderStdDev
        {
            get { return WorldProperties.initialStateGenesLikelyhoodOfNotBeingAFreeRiderStdDev; }
            set { WorldProperties.initialStateGenesLikelyhoodOfNotBeingAFreeRiderStdDev = value; PersistChanges(); }
        }

        [DisplayableProperty("Mean", group = "Initial state\\Genes\\Hunting efficiency")]
        public static double InitialStateGenesHuntingEfficiencyMean
        {
            get { return WorldProperties.initialStateGenesHuntingEfficiencyMean; }
            set { WorldProperties.initialStateGenesHuntingEfficiencyMean = value; PersistChanges(); }
        }

        [DisplayableProperty("Standard deviation", group = "Initial state\\Genes\\Hunting efficiency", description = "Genetically defined hunting skill. [0, infinity)")]
        public static double InitialStateGenesHuntingEfficiencyStdDev
        {
            get { return WorldProperties.initialStateGenesHuntingEfficiencyStdDev; }
            set { WorldProperties.initialStateGenesHuntingEfficiencyStdDev = value; PersistChanges(); }
        }

        [DisplayableProperty("Mean", group = "Initial state\\Genes\\Hunting B efficiency")]
        public static double InitialStateGenesHuntingBEfficiencyMean
        {
            get { return WorldProperties.initialStateGenesHuntingBEfficiencyMean; }
            set { WorldProperties.initialStateGenesHuntingBEfficiencyMean = value; PersistChanges(); }
        }

        [DisplayableProperty("Standard deviation", group = "Initial state\\Genes\\Hunting B efficiency", description = "Genetically defined hunting B skill. [0, infinity)")]
        public static double InitialStateGenesHuntingBEfficiencyStdDev
        {
            get { return WorldProperties.initialStateGenesHuntingBEfficiencyStdDev; }
            set { WorldProperties.initialStateGenesHuntingBEfficiencyStdDev = value; PersistChanges(); }
        }

        [DisplayableProperty("Mean", group = "Initial state\\Genes\\Cooperation efficiency")]
        public static double InitialStateGenesCooperationEfficiencyMean
        {
            get { return WorldProperties.initialStateGenesCooperationEfficiencyMean; }
            set { WorldProperties.initialStateGenesCooperationEfficiencyMean = value; PersistChanges(); }
        }

        [DisplayableProperty("Standard deviation", group = "Initial state\\Genes\\Cooperation efficiency", description = "Genetically defined ability to cooperate with others. [0, infinity) Below 1 - the tribsman is hintering the hunt. Above 1 - the tribesman is adding more to the group then his own efforts.")]
        public static double InitialStateGenesCooperationEfficiencyStdDev
        {
            get { return WorldProperties.initialStateGenesCooperationEfficiencyStdDev; }
            set { WorldProperties.initialStateGenesCooperationEfficiencyStdDev = value; PersistChanges(); }
        }

        [DisplayableProperty("Mean", group = "Initial state\\Genes\\Memory size")]
        public static double InitialStateGenesMemorySizeMean
        {
            get { return WorldProperties.initialStateGenesMemorySizeMean; }
            set { WorldProperties.initialStateGenesMemorySizeMean = value; PersistChanges(); }
        }

        [DisplayableProperty("Standard deviation", group = "Initial state\\Genes\\Memory size", description = "Genetically defined memory capacity. [0, infinity)")]
        public static double InitialStateGenesMemorySizeStdDev
        {
            get { return WorldProperties.initialStateGenesMemorySizeStdDev; }
            set { WorldProperties.initialStateGenesMemorySizeStdDev = value; PersistChanges(); }
        }

        [DisplayableProperty("Mean", group = "Initial state\\Genes\\Creativity")]
        public static double InitialStateGenesCreativityMean
        {
            get { return WorldProperties.initialStateGenesCreativityMean; }
            set { WorldProperties.initialStateGenesCreativityMean = value; PersistChanges(); }
        }

        [DisplayableProperty("Standard deviation", group = "Initial state\\Genes\\Creativity", description = "Genetically defined chance to invent new meme. [0, 1]")]
        public static double InitialStateGenesCreativityStdDev
        {
            get { return WorldProperties.initialStateGenesCreativityStdDev; }
            set { WorldProperties.initialStateGenesCreativityStdDev = value; PersistChanges(); }
        }

        [DisplayableProperty("Mean", group = "Initial state\\Genes\\Ageing rate")]
        public static double InitialStateGenesAgeingRateMean
        {
            get { return WorldProperties.initialStateGenesAgeingRateMean; }
            set { WorldProperties.initialStateGenesAgeingRateMean= value; PersistChanges(); }
        }

        [DisplayableProperty("Standard deviation", group = "Initial state\\Genes\\Ageing rate", description = "Ageing rate for the Gompertz ageing law. [0, 1]")]
        public static double InitialStateGenesAgeingRateStdDev
        {
            get { return WorldProperties.initialStateGenesAgeingRateStdDev; }
            set { WorldProperties.initialStateGenesAgeingRateStdDev = value; PersistChanges(); }
        }

        [DisplayableProperty("Mean", group = "Initial state\\Genes\\UselessActionsLikelihood")]
        public static double InitialStateGenesUselessActionsLikelihoodMean
        {
            get { return WorldProperties.initialStateGenesUselessActionsLikelihoodMean; }
            set { WorldProperties.initialStateGenesUselessActionsLikelihoodMean = value; PersistChanges(); }
        }

        [DisplayableProperty("Standard deviation", group = "Initial state\\Genes\\UselessActionsLikelihood", description = "Genetically defined chance to do useless actions. [0, 1]")]
        public static double InitialStateGenesUselessActionsLikelihoodStdDev
        {
            get { return WorldProperties.initialStateGenesUselessActionsLikelihoodStdDev; }
            set { WorldProperties.initialStateGenesUselessActionsLikelihoodStdDev = value; PersistChanges(); }
        }

        [DisplayableProperty("Resource consumed per year", group = "Environment", description = "The amount of resource consumed every year by a single being. [0, infinity)")]
        public static double LifeSupportCosts
        {
            get { return WorldProperties.lifeSupportCosts; }
            set { WorldProperties.lifeSupportCosts = value; PersistChanges(); }
        }

        [DisplayableProperty("Likelyhood change mean", group = "Memes\\Invention\\Trick likelyhood", description = "The mean of the efficiency of the newly invented meme that increases the likelyhood of playing a trick on tribesmates to get more food. [0, 1)")]
        public static double NewMemeTrickLikelyhoodMean
        {
            get { return WorldProperties.newMemeTrickLikelyhoodMean; }
            set { WorldProperties.newMemeTrickLikelyhoodMean = value; PersistChanges(); }
        }

        [DisplayableProperty("Likelyhood change StdDev", group = "Memes\\Invention\\Trick likelyhood")]
        public static double NewMemeTrickLikelyhoodStdDev
        {
            get { return WorldProperties.newMemeTrickLikelyhoodStdDev; }
            set { WorldProperties.newMemeTrickLikelyhoodStdDev = value; PersistChanges(); }
        }

        [DisplayableProperty("Efficiency change mean", group = "Memes\\Invention\\Trick efficiency", description = "The mean of the efficiency of the newly invented meme that increases the power a trick on tribesmates to get more food. [0, infinity) The trick with the power of 1 will make the trickster receive double the resources compared with the honest members")]
        public static double NewMemeTrickEfficiencyMean
        {
            get { return WorldProperties.newMemeTrickEfficiencyMean; }
            set { WorldProperties.newMemeTrickEfficiencyMean = value; PersistChanges(); }
        }

        [DisplayableProperty("Efficiency change StdDev", group = "Memes\\Invention\\Trick efficiency")]
        public static double NewMemeTrickEfficiencyStdDev
        {
            get { return WorldProperties.newMemeTrickEfficiencyStdDev; }
            set { WorldProperties.newMemeTrickEfficiencyStdDev = value; PersistChanges(); }
        }

        [DisplayableProperty("Likelyhood change mean", group = "Memes\\Invention\\Teaching likelyhood", description = "The mean of the efficiency of the newly invented meme that increases the likelyhood that it's bearer will decide to teach someone one of his own memes. [0, 1)")]
        public static double NewMemeTeachingLikelyhoodMean
        {
            get { return WorldProperties.newMemeTeachingLikelyhoodMean; }
            set { WorldProperties.newMemeTeachingLikelyhoodMean = value; PersistChanges(); }
        }

        [DisplayableProperty("Likelyhood change StdDev", group = "Memes\\Invention\\Teaching likelyhood")]
        public static double NewMemeTeachingLikelyhoodStdDev
        {
            get { return WorldProperties.newMemeTeachingLikelyhoodStdDev; }
            set { WorldProperties.newMemeTeachingLikelyhoodStdDev = value; PersistChanges(); }
        }

        [DisplayableProperty("Efficiency change mean", group = "Memes\\Invention\\Teaching efficiency", description = "The mean of the efficiency of the newly invented meme that increases the chances for successfully teaching a new meme when a teacher initiates the teaching. [0, 1)")]
        public static double NewMemeTeachingEfficiencyMean
        {
            get { return WorldProperties.newMemeTeachingEfficiencyMean; }
            set { WorldProperties.newMemeTeachingEfficiencyMean = value; PersistChanges(); }
        }

        [DisplayableProperty("Efficiency change StdDev", group = "Memes\\Invention\\Teaching efficiency")]
        public static double NewMemeTeachingEfficiencyStdDev
        {
            get { return WorldProperties.newMemeTeachingEfficiencyStdDev; }
            set { WorldProperties.newMemeTeachingEfficiencyStdDev = value; PersistChanges(); }
        }

        [DisplayableProperty("Likelyhood change mean", group = "Memes\\Invention\\Study likelyhood", description = "The mean of the efficiency of the newly invented meme that increases the chances of student-initiated learning. [0, 1)")]
        public static double NewMemeStudyLikelyhoodMean
        {
            get { return WorldProperties.newMemeStudyLikelyhoodMean; }
            set { WorldProperties.newMemeStudyLikelyhoodMean = value; PersistChanges(); }
        }

        [DisplayableProperty("Likelyhood change StdDev", group = "Memes\\Invention\\Study likelyhood")]
        public static double NewMemeStudyLikelyhoodStdDev
        {
            get { return WorldProperties.newMemeStudyLikelyhoodStdDev; }
            set { WorldProperties.newMemeStudyLikelyhoodStdDev = value; PersistChanges(); }
        }

        [DisplayableProperty("Efficiency change mean", group = "Memes\\Invention\\Study efficiency", description = "The mean of the efficiency of the newly invented meme that increases the chances of successfull learning. [0, 1)")]
        public static double NewMemeStudyEfficiencyMean
        {
            get { return WorldProperties.newMemeStudyEfficiencyMean; }
            set { WorldProperties.newMemeStudyEfficiencyMean = value; PersistChanges(); }
        }

        [DisplayableProperty("Efficiency change StdDev", group = "Memes\\Invention\\Study efficiency")]
        public static double NewMemeStudyEfficiencyStdDev
        {
            get { return WorldProperties.newMemeStudyEfficiencyStdDev; }
            set { WorldProperties.newMemeStudyEfficiencyStdDev = value; PersistChanges(); }
        }

        [DisplayableProperty("Likelyhood change mean", group = "Memes\\Invention\\Free rider punishment likelyhood", description = "The mean of the efficiency of the newly invented meme that increases the bearer's chances for attempting to punish a free-rider. [0, 1)")]
        public static double NewMemeFreeRiderPunishmentLikelyhoodMean
        {
            get { return WorldProperties.newMemeFreeRiderPunishmentLikelyhoodMean; }
            set { WorldProperties.newMemeFreeRiderPunishmentLikelyhoodMean = value; PersistChanges(); }
        }

        [DisplayableProperty("Likelyhood change StdDev", group = "Memes\\Invention\\Free rider punishment likelyhood")]
        public static double NewMemeFreeRiderPunishmentLikelyhoodStdDev
        {
            get { return WorldProperties.newMemeFreeRiderPunishmentLikelyhoodStdDev; }
            set { WorldProperties.newMemeFreeRiderPunishmentLikelyhoodStdDev = value; PersistChanges(); }
        }

        [DisplayableProperty("Efficiency change mean", group = "Memes\\Invention\\Free rider determination efficiency", description = "The mean of the efficiency of the newly invented meme that increases the bearer's chances for successfully identifying the free rider. [0, 1)")]
        public static double NewMemeFreeRiderDeterminationEfficiencyMean
        {
            get { return WorldProperties.newMemeFreeRiderDeterminationEfficiencyMean; }
            set { WorldProperties.newMemeFreeRiderDeterminationEfficiencyMean = value; PersistChanges(); }
        }

        [DisplayableProperty("Efficiency change StdDev", group = "Memes\\Invention\\Free rider determination efficiency")]
        public static double NewMemeFreeRiderDeterminationEfficiencyStdDev
        {
            get { return WorldProperties.newMemeFreeRiderDeterminationEfficiencyStdDev; }
            set { WorldProperties.newMemeFreeRiderDeterminationEfficiencyStdDev = value; PersistChanges(); }
        }

        [DisplayableProperty("Likelyhood change mean", group = "Memes\\Invention\\Not being a free rider likelyhood", description = "The mean of the efficiency of the newly invented meme that increases the bearer's chances for not being a free-rider. [0, 1)")]
        public static double NewMemeLikelyhoodOfNotBeingAFreeRiderMean
        {
            get { return WorldProperties.newMemeLikelyhoodOfNotBeingAFreeRiderMean; }
            set { WorldProperties.newMemeLikelyhoodOfNotBeingAFreeRiderMean = value; PersistChanges(); }
        }

        [DisplayableProperty("Efficiency change StdDev", group = "Memes\\Invention\\Not being a free rider likelyhood")]
        public static double NewMemeLikelyhoodOfNotBeingAFreeRiderStdDev
        {
            get { return WorldProperties.newMemeLikelyhoodOfNotBeingAFreeRiderStdDev; }
            set { WorldProperties.newMemeLikelyhoodOfNotBeingAFreeRiderStdDev = value; PersistChanges(); }
        }

        [DisplayableProperty("Efficiency increase mean", group = "Memes\\Invention\\Hunting efficiency", description = "The mean of the efficiency of the newly invented meme that increases the bearer's hunting efficiency. [0, infinity)")]
        public static double NewMemeHuntingEfficiencyMean
        {
            get { return WorldProperties.newMemeHuntingEfficiencyMean; }
            set { WorldProperties.newMemeHuntingEfficiencyMean = value; PersistChanges(); }
        }

        [DisplayableProperty("Efficiency increase StdDev", group = "Memes\\Invention\\Hunting efficiency")]
        public static double NewMemeHuntingEfficiencyStdDev
        {
            get { return WorldProperties.newMemeHuntingEfficiencyStdDev; }
            set { WorldProperties.newMemeHuntingEfficiencyStdDev = value; PersistChanges(); }
        }

        [DisplayableProperty("Efficiency increase mean", group = "Memes\\Invention\\Hunting B efficiency", description = "The mean of the efficiency of the newly invented meme that increases the bearer's hunting B efficiency. [0, infinity)")]
        public static double NewMemeHuntingBEfficiencyMean
        {
            get { return WorldProperties.newMemeHuntingBEfficiencyMean; }
            set { WorldProperties.newMemeHuntingBEfficiencyMean = value; PersistChanges(); }
        }

        [DisplayableProperty("Efficiency increase StdDev", group = "Memes\\Invention\\Hunting B efficiency")]
        public static double NewMemeHuntingBEfficiencyStdDev
        {
            get { return WorldProperties.newMemeHuntingBEfficiencyStdDev; }
            set { WorldProperties.newMemeHuntingBEfficiencyStdDev = value; PersistChanges(); }
        }

        [DisplayableProperty("Efficiency increase mean", group = "Memes\\Invention\\Coopertaion efficiency", description = "The mean of the efficiency of the newly invented meme that increases the bearer's ability to cooperate with others while hunting. [0, infinity)")]
        public static double NewMemeCooperationEfficiencyMean
        {
            get { return WorldProperties.newMemeCooperationEfficiencyMean; }
            set { WorldProperties.newMemeCooperationEfficiencyMean = value; PersistChanges(); }
        }

        [DisplayableProperty("Efficiency increase StdDev", group = "Memes\\Invention\\Coopertaion efficiency")]
        public static double NewMemeCooperationEfficiencyStdDev
        {
            get { return WorldProperties.newMemeCooperationEfficiencyStdDev; }
            set { WorldProperties.newMemeCooperationEfficiencyStdDev = value; PersistChanges(); }
        }

        [DisplayableProperty("Efficiency increase mean", group = "Memes\\Invention\\Ageing rate", description = "The mean of the efficiency of the newly invented meme that increases the bearer's chances to live longer. [0, 1)")]
        public static double NewMemeAgeingRateMean
        {
            get { return WorldProperties.newMemeAgeingRateMean; }
            set { WorldProperties.newMemeAgeingRateMean = value; PersistChanges(); }
        }

        [DisplayableProperty("Efficiency increase StdDev", group = "Memes\\Invention\\Ageing rate")]
        public static double NewMemeAgeingRateStdDev
        {
            get { return WorldProperties.newMemeAgeingRateStdDev; }
            set { WorldProperties.newMemeAgeingRateStdDev = value; PersistChanges(); }
        }

        [DisplayableProperty("Average cost", group = "Memes\\Invention\\Useless memes", description = "The average efficiency of a useless meme. [0, 1)")]
        public static double NewMemeUselessEfficiencyMean
        {
            get { return WorldProperties.newMemeUselessCostMean; }
            set { WorldProperties.newMemeUselessCostMean = value; PersistChanges(); }
        }

        [DisplayableProperty("Cost StdDev", group = "Memes\\Invention\\Useless memes")]
        public static double NewMemeUselessEfficiencyStdDev
        {
            get { return WorldProperties.newMemeUselessCostStdDev; }
            set { WorldProperties.newMemeUselessCostStdDev = value; PersistChanges(); }
        }


        [DisplayableProperty("Standard Deviation", group = "Initial state\\Tribes\\Tribe Initial Population")]
        public static double StartingTribePopulationStdDev
        {
            get { return WorldProperties.startingTribePopulationStdDev; }
            set { WorldProperties.startingTribePopulationStdDev = value; PersistChanges(); }
        }

        [DisplayableProperty("Starting Number Of Tribes", group = "Initial state\\Tribes")]
        public static double StartingNumberOfTribes
        {
            get { return WorldProperties.startingNumberOfTribes; }
            set { WorldProperties.startingNumberOfTribes = value; PersistChanges(); }
        }

        [DisplayableProperty("Mean", group = "Initial state\\Tribes\\Tribe Initial Population")]
        public static double StartingTribePopulationMean
        {
            get { return WorldProperties.startingTribePopulationMean; }
            set { WorldProperties.startingTribePopulationMean = value; PersistChanges(); }
        }

        [DisplayableProperty("Collect data for graphs", group = "Program settings\\Output", description = "If set to 0, live graphs will be disabled. Otherwise it will specify how often a graph point will be created. 1 will create a really fine graph, but seriously affect the performance, while 1000 will create very coarse graphs fast.")]
        public static double CollectGraphData { get => collectGraphData; set { collectGraphData = value; PersistChanges(); } }
        [DisplayableProperty("Collect data in files", group = "Program settings\\Output", description = "If set to 0, files with the data will not be generated. Otherwise the files will be generated and will contain the data for every nth year where N is the number you insert. (Ex: 1 - every year will be recorded, 2 - every second year, 10 - every tenth year etc). Warning! The program will run slower the more tribes exist in the world and the more often you record the file.")]
        public static double CollectFilesData { get => collectFilesData; set { collectFilesData = value; PersistChanges(); } }
        [DisplayableProperty("Collect brain usage percentages", group = "Program settings", description = "If set to 1, brain usage percentages will be recorded.")]
        public static double CollectBrainUsagePercentages { get => collectBrainUsagePercentages; set { collectBrainUsagePercentages = value; PersistChanges(); } }
        [DisplayableProperty("Collect number of live memes", group = "Program settings", description = "If set to 1, the number of live memes will be recorded.")]
        public static double CollectLiveMemes { get => collectLiveMemes; set { collectLiveMemes = value; PersistChanges(); } }

        [DisplayableProperty("Collect Individual Success", group = "Program settings\\Collect Individual Success", description = "[ATTENTION!!! Очень ресурсоёмко] If set to 1, мы записываем в момент смерти каждого tribesman, сколько детей он успел породить и (выжиывание не проверяем) и размеры всех его признаков, генетические и фенотипические, а также возраст и аккумулированный за жизнь ресурс. Очень подробная информация для анализа. Очень много места занимает.")]
        public static double CollectIndividualSuccess { get => collectIndividualSuccess; set { collectIndividualSuccess = value; PersistChanges(); } }
        [DisplayableProperty("Chance to Collect Individual Success", group = "Program settings\\Collect Individual Success", description = "Вероятность, с которой записывать умерших, чтобы иметь не миллионы записей, а меньше.")]
        public static double ChanceToCollectIndividualSuccess { get => chanceToCollectIndividualSuccess; set { chanceToCollectIndividualSuccess = value; PersistChanges(); } }
        [DisplayableProperty("Collect Individual phenotype", group = "Program settings\\Collect Individual Success", description = "1 - collect individual phenotype values will collected 0 - the model will run faster")]
        public static double CollectIndividualPhenotypeValues { get { return WorldProperties.collectIndividualPhenotypeValues; } set { WorldProperties.collectIndividualPhenotypeValues = value; PersistChanges(); }}
        [DisplayableProperty("Collect Individual genotype", group = "Program settings\\Collect Individual Success", description = "1 - collect individual genetic values will be collected 0 - the model will run faster")]
        public static double CollectIndividualGenotypeValues { get { return WorldProperties.collectIndividualGenotypeValues; } set { WorldProperties.collectIndividualGenotypeValues = value; PersistChanges(); } }

        [DisplayableProperty("Collect Memes Success", group = "Program settings\\Collect Memes Success", description = "If set to 1, we collect memes sucess on the end of his lives moment.")]
        public static double CollectMemesSuccess { get => collectMemesSuccess; set { collectMemesSuccess = value; PersistChanges(); } }
        [DisplayableProperty("Chance to Collect Memes Success", group = "Program settings\\Collect Memes Success", description = "Chance to collect memes sucess, If you want to collect the part of mems history only.")]
        public static double ChanceToCollectMemesSuccess { get => chanceToCollectMemesSuccess; set { chanceToCollectMemesSuccess = value; PersistChanges(); } }

        [DisplayableProperty("Use Gompertz Ageing", group = "Lifestyle\\Death\\Gompertz Ageing", description = "If set to 0, the tribesmen will age according to the old method. Otherwise Gompertz law will be used. Until the age of (AR) the chances of dying will be equal to (BMR). After the age of (AR) chances of dying of natural causes will equal (BMR) *e^( (AR) * (Age - (AS) )). AR is determined from genes and memes. Chances of dying cannot go higher than plateau value if set.")]
        public static double UseGompertzAgeing { get => useGompertzAgeing; set {useGompertzAgeing = value; PersistChanges();}}

        [DisplayableProperty("Basic mortality rate", group = "Lifestyle\\Death\\Gompertz Ageing", description = "(BMR) The chance of dying before the ageing begins.")]
        public static double GompertzAgeingBasicMortalityRate  { get => gompertzAgeingBasicMortalityRate; set {gompertzAgeingBasicMortalityRate = value; PersistChanges();}}

        [DisplayableProperty("Age at which ageing starts", group = "Lifestyle\\Death\\Gompertz Ageing", description = "(AS) Before that age the chances of dying are equal to basic mortality rate, and increase exponentially afterwards.")]
        public static double GompertzAgeingAgeAtWhichAgeingStarts { get => gompertzAgeingAgeAtWhichAgeingStarts; set {gompertzAgeingAgeAtWhichAgeingStarts = value; PersistChanges();}}

        [DisplayableProperty("Mortality plateau", group = "Lifestyle\\Death\\Gompertz Ageing", description = "Chance of death cannot go higher than this value. This should not be lower than BMR. Values outside the range [BMR, 1] will be treated as 1.")]
        public static double GompertzAgeingMortalityPlateau { get => gompertzAgeingMortalityPlateau; set {gompertzAgeingMortalityPlateau = value; PersistChanges();}}

        [DisplayableProperty("Basic ageing rate", group = "Environment\\Ageing costs", description = "Basic ageing rate. Any genetic or cultural difference from it may affect the living costs. \n If Gompertz ageing is enabled the living costs will be calculated using the formula LC = A + BΔ + CΔ² + De^EΔ, where Δ is a difference between actual AR and the base AR, A is basic costs of living, and B, C D, and E are parameters which are set in this section. \n The lc decrease uses the same formula, but a separate set of parameters, marked with ₂ (IE A₂). All parameters should be positive for default behaviour. If a parameter is set to a negative value this will revert the living costs change. I.E. make life less expensive for long livers, or more expensive for short livers.")]
        public static double BaseGompertzAgeingRate { get => baseGompertzAgeingRate; set {
                baseGompertzAgeingRate = value;
                PersistChanges();
            } }

        [DisplayableProperty("(B)", group = "Environment\\Ageing costs", description = "")]
        public static double BaseGompertzAgeingRateLifeCostIncrease { get => baseGompertzAgeingRateLifeCostIncrease; set { 
                baseGompertzAgeingRateLifeCostIncrease = value;                
                PersistChanges(); 
            } }

        [DisplayableProperty("(C)", group = "Environment\\Ageing costs", description = "")]
        public static double BaseGompertzAgeingRateLifeCostQuadraticIncrease { get => baseGompertzAgeingRateLifeCostQuadraticIncrease; set { 
                baseGompertzAgeingRateLifeCostQuadraticIncrease = value;                
                PersistChanges(); 
            } }

        [DisplayableProperty("(D)", group = "Environment\\Ageing costs", description = "")]
        public static double BaseGompertzAgeingRateLifeCostExponentialCoefficientIncrease{ get => baseGompertzAgeingRateLifeCostExponentialCoefficientIncrease; set { 
                baseGompertzAgeingRateLifeCostExponentialCoefficientIncrease = value;                
                PersistChanges(); 
            } }

        [DisplayableProperty("(E)", group = "Environment\\Ageing costs", description = "")]
        public static double BaseGompertzAgeingRateLifeCostExponentialMultiplierIncrease { get => baseGompertzAgeingRateLifeCostExponentialMultiplierIncrease; set { 
                baseGompertzAgeingRateLifeCostExponentialMultiplierIncrease = value;                
                PersistChanges(); 
            } }

        [DisplayableProperty("(B₂)", group = "Environment\\Ageing costs", description = "")]
        public static double BaseGompertzAgeingRateLifeCostDecrease { get => baseGompertzAgeingRateLifeCostDecrease; set {
                baseGompertzAgeingRateLifeCostDecrease = value;                
                PersistChanges();
            } }
        
        [DisplayableProperty("(C₂)", group = "Environment\\Ageing costs", description = "")]
        public static double BaseGompertzAgeingRateLifeCostQuadraticDecrease { get => baseGompertzAgeingRateLifeCostQuadraticDecrease; set {
                baseGompertzAgeingRateLifeCostQuadraticDecrease = value;                
                PersistChanges();
            } }
        
        [DisplayableProperty("(D₂)", group = "Environment\\Ageing costs", description = "")]
        public static double BaseGompertzAgeingRateLifeCostExponentialCoefficientDecrease { get => baseGompertzAgeingRateLifeCostExponentialCoefficientDecrease; set {
                baseGompertzAgeingRateLifeCostExponentialCoefficientDecrease = value;                
                PersistChanges();
            } }

        [DisplayableProperty("(E₂)", group = "Environment\\Ageing costs", description = "")]
        public static double BaseGompertzAgeingRateLifeCostExponentialMultiplierDecrease { get => baseGompertzAgeingRateLifeCostExponentialMultiplierDecrease; set {
                baseGompertzAgeingRateLifeCostExponentialMultiplierDecrease = value;                
                PersistChanges();
            } }


        #endregion

        #region Persistance
        public static void LoadPersistance(string loadFilename = null)
        {
            Console.WriteLine($"WorldProperties.LoadPersistance({loadFilename})");

            inStableState = false;
            if (loadFilename != null)
            {
                filename = loadFilename;
            }
            else
            {
                loadFilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Tribe Sim", "Default Settings.trsim");
                if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Tribe Sim")))
                {
                    Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Tribe Sim"));
                }
            }
            Dictionary<string, PropertyInfo> propertiesDictionary = new Dictionary<string, PropertyInfo>();
            List<PropertyInfo> customProperties = new List<PropertyInfo>(typeof(WorldProperties).GetProperties());

            foreach (PropertyInfo customProperty in customProperties)
            {
                DisplayableProperty displayInfo = customProperty.GetCustomAttribute<DisplayableProperty>();
                if (displayInfo == null) continue;
                if (propertiesDictionary.ContainsKey(displayInfo.XMLName))
                    Console.WriteLine($"{displayInfo.XMLName} >> {customProperty}");
                propertiesDictionary.Add(displayInfo.XMLName, customProperty);
            }
            if (File.Exists(loadFilename))
            {
                XmlReader reader = XmlReader.Create(loadFilename);
                while (reader.Read())
                {
                    if (propertiesDictionary.ContainsKey(reader.Name))
                    {
                        string namememory = reader.Name;
                        reader.Read();
                        propertiesDictionary[namememory].SetValue(null, reader.Value.ConvertToDouble());
                        reader.Read();
                    }
                    // The following lines are only needed ones to migrate from older *.trsim files
                    else if (reader.Name== "Memes_Invention_Averagecost")
                    {
                        double val = 0;
                        reader.Read();
                        double.TryParse(reader.Value.Trim(), out val);
                        reader.Read();
                        WorldProperties.MemeCostRandomAverageCooperationEfficiency = val;
                        WorldProperties.MemeCostRandomAverageFreeRiderDeterminationEfficiency = val;
                        WorldProperties.MemeCostRandomAverageFreeRiderPunishmentLikelyhood = val;
                        WorldProperties.MemeCostRandomAverageHuntingEfficiency = val;
                        WorldProperties.MemeCostRandomAverageLikelyhoodOfNotBeingAFreeRider = val;
                        WorldProperties.MemeCostRandomAverageStudyEfficiency = val;
                        WorldProperties.MemeCostRandomAverageStudyLikelyhood = val;
                        WorldProperties.MemeCostRandomAverageTeachingEfficiency = val;
                        WorldProperties.MemeCostRandomAverageTeachingLikelyhood = val;
                        WorldProperties.MemeCostRandomAverageTrickEfficiency = val;
                        WorldProperties.MemeCostRandomAverageTrickLikelyhood = val;
                        WorldProperties.MemeCostRandomAverageUseless = val;
                    }
                    else if (reader.Name == "Memes_Invention_CoststdDev")
                    {
                        double val = 0;
                        reader.Read();
                        double.TryParse(reader.Value.Trim(), out val);
                        reader.Read();
                        WorldProperties.MemeCostRandomStdDevCooperationEfficiency = val;
                        WorldProperties.MemeCostRandomStdDevFreeRiderDeterminationEfficiency = val;
                        WorldProperties.MemeCostRandomStdDevFreeRiderPunishmentLikelyhood = val;
                        WorldProperties.MemeCostRandomStdDevHuntingEfficiency = val;
                        WorldProperties.MemeCostRandomStdDevLikelyhoodOfNotBeingAFreeRider = val;
                        WorldProperties.MemeCostRandomStdDevStudyEfficiency = val;
                        WorldProperties.MemeCostRandomStdDevStudyLikelyhood = val;
                        WorldProperties.MemeCostRandomStdDevTeachingEfficiency = val;
                        WorldProperties.MemeCostRandomStdDevTeachingLikelyhood = val;
                        WorldProperties.MemeCostRandomStdDevTrickEfficiency = val;
                        WorldProperties.MemeCostRandomStdDevTrickLikelyhood = val;
                        WorldProperties.MemeCostRandomStdDevUseless = val;
                    }
                }
                reader.Close();
            }
            inStableState = true;
        }

        public static void PersistChanges(string newFilename = null)
        {
            if (!inStableState) return;
            if (newFilename != null)
            {
                filename = newFilename;
            }
            else
            {
                newFilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Tribe Sim", "Default Settings.trsim");
                if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Tribe Sim")))
                {
                    Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Tribe Sim"));
                }
            }
            XmlDocument xmlDocument = new XmlDocument();
            XmlNode rootNode = xmlDocument.AppendChild(xmlDocument.CreateElement("Settings"));
            List<PropertyInfo> customProperties = new List<PropertyInfo>(typeof(WorldProperties).GetProperties());

            foreach (PropertyInfo customProperty in customProperties)
            {
                DisplayableProperty displayInfo = customProperty.GetCustomAttribute<DisplayableProperty>();
                if (displayInfo == null) continue;
                rootNode.AppendChild(xmlDocument.CreateElement(displayInfo.XMLName)).InnerText = customProperty.GetValue(null).ToString();
            }

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineOnAttributes = true;
            settings.NewLineHandling = NewLineHandling.Entitize;
            XmlWriter outputWriter = XmlWriter.Create(newFilename, settings);
            xmlDocument.WriteTo(outputWriter);
            outputWriter.Close();
        }
        #endregion

        #region Property tree
        public static PropertyTree PropertyTree
        {
            get
            {
                if (propertyTree == null)
                {
                    propertyTree = BuildPropertyTree();
                }
                return WorldProperties.propertyTree;
            }
        }      

        private static TribeSim.PropertyTree BuildPropertyTree()
        {
            PropertyTree retval = new PropertyTree();

            List<PropertyInfo> customProperties = new List<PropertyInfo>(typeof(WorldProperties).GetProperties());

            foreach (PropertyInfo customProperty in customProperties)
            {
                DisplayableProperty displayInfo = customProperty.GetCustomAttribute<DisplayableProperty>();
                if (displayInfo == null) continue;
                string groupName = displayInfo.group;
                string[] path = groupName.Split('\\');
                PropertyTree curnode = retval;
                foreach (string node in path)
                {
                    if (!curnode.treeBranches.ContainsKey(node))
                    {
                        curnode.treeBranches.Add(node, new PropertyTree());
                    }
                    curnode = curnode.treeBranches[node];
                }
                if (curnode.treeLeafs.ContainsKey(displayInfo.name))
                {
                    throw new Exception("Duplicate property " + displayInfo.name + " at path " + displayInfo.group);
                }
                curnode.treeLeafs.Add(displayInfo.name, customProperty);
            }

            return retval;
        }
        #endregion

        #region Structured property description
        /// <summary> Количество фич. Эту цифру можно рассчитывать, и это будет надёжнее, но чуть-чуть медленнее. Поэтому вместо того чтобы её вычислять мы будем её проверять. </summary>
        public const int FEATURES_COUNT = 16;
        public static FeatureDescription[] FeatureDescriptions;
        public static int[] MemesWhichCanBeInvented;
        public static void ResetFeatureDescriptions() {
            // Проверяем, что константа количества фич выставлена правильно.
            int maxFeatureIndex = -1;
            foreach (AvailableFeatures feature in Enum.GetValues(typeof(AvailableFeatures)))
                maxFeatureIndex = Math.Max(maxFeatureIndex, (int)feature);
            if (maxFeatureIndex + 1 != WorldProperties.FEATURES_COUNT)
                throw new Exception("Please fix WorldProperties.FEATURES_COUNT");

            FeatureDescriptions = new FeatureDescription[WorldProperties.FEATURES_COUNT];
            FeatureDescriptions[(int)AvailableFeatures.TrickLikelyhood] = new FeatureDescription() {
                range = FeatureRange.ZeroToOne,
                InitialStateGenesMean = WorldProperties.InitialStateGenesTrickLikelyhoodMean,
                InitialStateGenesStdDev = WorldProperties.InitialStateGenesTrickLikelyhoodStdDev,
                ChancceOfMutation = WorldProperties.MutationChanceTrickLikelyhood,
                MutationStrengthMean = WorldProperties.MutationStrengthMeanTrickLikelyhood,
                MutationStrengthStdDev = WorldProperties.MutationStrengthStdDevTrickLikelyhood,

                MemeEfficiencyMean = WorldProperties.NewMemeTrickLikelyhoodMean,
                MemeEfficiencyStdDev = WorldProperties.NewMemeTrickLikelyhoodStdDev,
                MemePricePedestal = WorldProperties.MemeCostPedestalTrickLikelyhood,
                MemePriceEfficiencyRatio = WorldProperties.MemeCostEfficiencyRatioTrickLikelyhood,
                MemePriceRandomMean = WorldProperties.MemeCostRandomAverageTrickLikelyhood,
                MemePriceRandomStdDev = WorldProperties.MemeCostRandomStdDevTrickLikelyhood,
                MemCanBeInvented = WorldProperties.NewMemeTrickLikelyhoodMean != 0 || WorldProperties.NewMemeTrickLikelyhoodStdDev != 0,
            };
            FeatureDescriptions[(int)AvailableFeatures.TrickEfficiency] = new FeatureDescription() {
                range = FeatureRange.Positive,
                InitialStateGenesMean = WorldProperties.InitialStateGenesTrickEfficiencyMean,
                InitialStateGenesStdDev = WorldProperties.InitialStateGenesTrickEfficiencyStdDev,
                ChancceOfMutation = WorldProperties.MutationChanceTrickEfficiency,
                MutationStrengthMean = WorldProperties.MutationStrengthMeanTrickEfficiency,
                MutationStrengthStdDev = WorldProperties.MutationStrengthStdDevTrickEfficiency,

                MemeEfficiencyMean = WorldProperties.NewMemeTrickEfficiencyMean,
                MemeEfficiencyStdDev = WorldProperties.NewMemeTrickEfficiencyStdDev,
                MemePricePedestal = WorldProperties.MemeCostPedestalTrickEfficiency,
                MemePriceEfficiencyRatio = WorldProperties.MemeCostEfficiencyRatioTrickEfficiency,
                MemePriceRandomMean = WorldProperties.MemeCostRandomAverageTrickEfficiency,
                MemePriceRandomStdDev = WorldProperties.MemeCostRandomStdDevTrickEfficiency,
                MemCanBeInvented = WorldProperties.NewMemeTrickEfficiencyMean != 0 || WorldProperties.NewMemeTrickEfficiencyStdDev != 0,
            };
            FeatureDescriptions[(int)AvailableFeatures.TeachingLikelyhood] = new FeatureDescription() {
                range = FeatureRange.ZeroToOne,
                InitialStateGenesMean = WorldProperties.InitialStateGenesTeachingLikelyhoodMean,
                InitialStateGenesStdDev = WorldProperties.InitialStateGenesTeachingLikelyhoodStdDev,
                ChancceOfMutation = WorldProperties.MutationChanceTeachingLikelyhood,
                MutationStrengthMean = WorldProperties.MutationStrengthMeanTeachingLikelyhood,
                MutationStrengthStdDev = WorldProperties.MutationStrengthStdDevTeachingLikelyhood,

                MemeEfficiencyMean = WorldProperties.NewMemeTeachingLikelyhoodMean,
                MemeEfficiencyStdDev = WorldProperties.NewMemeTeachingLikelyhoodStdDev,
                MemePricePedestal = WorldProperties.MemeCostPedestalTeachingLikelyhood,
                MemePriceEfficiencyRatio = WorldProperties.MemeCostEfficiencyRatioTeachingLikelyhood,
                MemePriceRandomMean = WorldProperties.MemeCostRandomAverageTeachingLikelyhood,
                MemePriceRandomStdDev = WorldProperties.MemeCostRandomStdDevTeachingLikelyhood,
                MemCanBeInvented = WorldProperties.NewMemeTeachingLikelyhoodMean != 0 || WorldProperties.NewMemeTeachingLikelyhoodStdDev != 0,
            };
            FeatureDescriptions[(int)AvailableFeatures.TeachingEfficiency] = new FeatureDescription() {
                range = FeatureRange.ZeroToOne,
                InitialStateGenesMean = WorldProperties.InitialStateGenesTeachingEfficiencyMean,
                InitialStateGenesStdDev = WorldProperties.InitialStateGenesTeachingEfficiencyStdDev,
                ChancceOfMutation = WorldProperties.MutationChanceTeachingEfficiency,
                MutationStrengthMean = WorldProperties.MutationStrengthMeanTeachingEfficiency,
                MutationStrengthStdDev = WorldProperties.MutationStrengthStdDevTeachingEfficiency,

                MemeEfficiencyMean = WorldProperties.NewMemeTeachingEfficiencyMean,
                MemeEfficiencyStdDev = WorldProperties.NewMemeTeachingEfficiencyStdDev,
                MemePricePedestal = WorldProperties.MemeCostPedestalTeachingEfficiency,
                MemePriceEfficiencyRatio = WorldProperties.MemeCostEfficiencyRatioTeachingEfficiency,
                MemePriceRandomMean = WorldProperties.MemeCostRandomAverageTeachingEfficiency,
                MemePriceRandomStdDev = WorldProperties.MemeCostRandomStdDevTeachingEfficiency,
                MemCanBeInvented = WorldProperties.NewMemeTeachingEfficiencyMean != 0 || WorldProperties.NewMemeTeachingEfficiencyStdDev != 0,
            };
            FeatureDescriptions[(int)AvailableFeatures.StudyLikelyhood] = new FeatureDescription() {
                range = FeatureRange.ZeroToOne,
                InitialStateGenesMean = WorldProperties.InitialStateGenesStudyLikelyhoodMean,
                InitialStateGenesStdDev = WorldProperties.InitialStateGenesStudyLikelyhoodStdDev,
                ChancceOfMutation = WorldProperties.MutationChanceStudyLikelyhood,
                MutationStrengthMean = WorldProperties.MutationStrengthMeanStudyLikelyhood,
                MutationStrengthStdDev = WorldProperties.MutationStrengthStdDevStudyLikelyhood,

                MemeEfficiencyMean = WorldProperties.NewMemeStudyLikelyhoodMean,
                MemeEfficiencyStdDev = WorldProperties.NewMemeStudyLikelyhoodStdDev,
                MemePricePedestal = WorldProperties.MemeCostPedestalStudyLikelyhood,
                MemePriceEfficiencyRatio = WorldProperties.MemeCostEfficiencyRatioStudyLikelyhood,
                MemePriceRandomMean = WorldProperties.MemeCostRandomAverageStudyLikelyhood,
                MemePriceRandomStdDev = WorldProperties.MemeCostRandomStdDevStudyLikelyhood,
                MemCanBeInvented = WorldProperties.NewMemeStudyLikelyhoodMean != 0 || WorldProperties.NewMemeStudyLikelyhoodStdDev != 0,
            };
            FeatureDescriptions[(int)AvailableFeatures.StudyEfficiency] = new FeatureDescription() {
                range = FeatureRange.ZeroToOne,
                InitialStateGenesMean = WorldProperties.InitialStateGenesStudyEfficiencyMean,
                InitialStateGenesStdDev = WorldProperties.InitialStateGenesStudyEfficiencyStdDev,
                ChancceOfMutation = WorldProperties.MutationChanceStudyEfficiency,
                MutationStrengthMean = WorldProperties.MutationStrengthMeanStudyEfficiency,
                MutationStrengthStdDev = WorldProperties.MutationStrengthStdDevStudyEfficiency,

                MemeEfficiencyMean = WorldProperties.NewMemeStudyEfficiencyMean,
                MemeEfficiencyStdDev = WorldProperties.NewMemeStudyEfficiencyStdDev,
                MemePricePedestal = WorldProperties.MemeCostPedestalStudyEfficiency,
                MemePriceEfficiencyRatio = WorldProperties.MemeCostEfficiencyRatioStudyEfficiency,
                MemePriceRandomMean = WorldProperties.MemeCostRandomAverageStudyEfficiency,
                MemePriceRandomStdDev = WorldProperties.MemeCostRandomStdDevStudyEfficiency,
                MemCanBeInvented = WorldProperties.NewMemeStudyEfficiencyMean != 0 || WorldProperties.NewMemeStudyEfficiencyStdDev != 0,
            };
            FeatureDescriptions[(int)AvailableFeatures.FreeRiderPunishmentLikelyhood] = new FeatureDescription() {
                range = FeatureRange.ZeroToOne,
                InitialStateGenesMean = WorldProperties.InitialStateGenesFreeRiderPunishmentLikelyhoodMean,
                InitialStateGenesStdDev = WorldProperties.InitialStateGenesFreeRiderPunishmentLikelyhoodStdDev,
                ChancceOfMutation = WorldProperties.MutationChanceFreeRiderPunishmentLikelyhood,
                MutationStrengthMean = WorldProperties.MutationStrengthMeanFreeRiderPunishmentLikelyhood,
                MutationStrengthStdDev = WorldProperties.MutationStrengthStdDevFreeRiderPunishmentLikelyhood,

                MemeEfficiencyMean = WorldProperties.NewMemeFreeRiderPunishmentLikelyhoodMean,
                MemeEfficiencyStdDev = WorldProperties.NewMemeFreeRiderPunishmentLikelyhoodStdDev,
                MemePricePedestal = WorldProperties.MemeCostPedestalFreeRiderPunishmentLikelyhood,
                MemePriceEfficiencyRatio = WorldProperties.MemeCostEfficiencyRatioFreeRiderPunishmentLikelyhood,
                MemePriceRandomMean = WorldProperties.MemeCostRandomAverageFreeRiderPunishmentLikelyhood,
                MemePriceRandomStdDev = WorldProperties.MemeCostRandomStdDevFreeRiderPunishmentLikelyhood,
                MemCanBeInvented = WorldProperties.NewMemeFreeRiderPunishmentLikelyhoodMean != 0 || WorldProperties.NewMemeFreeRiderPunishmentLikelyhoodStdDev != 0,
            };
            FeatureDescriptions[(int)AvailableFeatures.FreeRiderDeterminationEfficiency] = new FeatureDescription() {
                range = FeatureRange.ZeroToOne,
                InitialStateGenesMean = WorldProperties.InitialStateGenesFreeRiderDeterminationEfficiencyMean,
                InitialStateGenesStdDev = WorldProperties.InitialStateGenesFreeRiderDeterminationEfficiencyStdDev,
                ChancceOfMutation = WorldProperties.MutationChanceFreeRiderDeterminationEfficiency,
                MutationStrengthMean = WorldProperties.MutationStrengthMeanFreeRiderDeterminationEfficiency,
                MutationStrengthStdDev = WorldProperties.MutationStrengthStdDevFreeRiderDeterminationEfficiency,
                
                MemeEfficiencyMean = WorldProperties.NewMemeFreeRiderDeterminationEfficiencyMean,
                MemeEfficiencyStdDev = WorldProperties.NewMemeFreeRiderDeterminationEfficiencyStdDev,
                MemePricePedestal = WorldProperties.MemeCostPedestalFreeRiderDeterminationEfficiency,
                MemePriceEfficiencyRatio = WorldProperties.MemeCostEfficiencyRatioFreeRiderDeterminationEfficiency,
                MemePriceRandomMean = WorldProperties.MemeCostRandomAverageFreeRiderDeterminationEfficiency,
                MemePriceRandomStdDev = WorldProperties.MemeCostRandomStdDevFreeRiderDeterminationEfficiency,
                MemCanBeInvented = WorldProperties.NewMemeFreeRiderDeterminationEfficiencyMean != 0 || WorldProperties.NewMemeFreeRiderDeterminationEfficiencyStdDev != 0,
            };
            FeatureDescriptions[(int)AvailableFeatures.LikelyhoodOfNotBeingAFreeRider] = new FeatureDescription() {
                range = FeatureRange.ZeroToOne,
                InitialStateGenesMean = WorldProperties.InitialStateGenesLikelyhoodOfNotBeingAFreeRiderMean,
                InitialStateGenesStdDev = WorldProperties.InitialStateGenesLikelyhoodOfNotBeingAFreeRiderStdDev,
                ChancceOfMutation = WorldProperties.MutationChanceLikelyhoodOfNotBeingAFreeRider,
                MutationStrengthMean = WorldProperties.MutationStrengthMeanLikelyhoodOfNotBeingAFreeRider,
                MutationStrengthStdDev = WorldProperties.MutationStrengthStdDevLikelyhoodOfNotBeingAFreeRider,

                MemeEfficiencyMean = WorldProperties.NewMemeLikelyhoodOfNotBeingAFreeRiderMean,
                MemeEfficiencyStdDev = WorldProperties.NewMemeLikelyhoodOfNotBeingAFreeRiderStdDev,
                MemePricePedestal = WorldProperties.MemeCostPedestalLikelyhoodOfNotBeingAFreeRider,
                MemePriceEfficiencyRatio = WorldProperties.MemeCostEfficiencyRatioLikelyhoodOfNotBeingAFreeRider,
                MemePriceRandomMean = WorldProperties.MemeCostRandomAverageLikelyhoodOfNotBeingAFreeRider,
                MemePriceRandomStdDev = WorldProperties.MemeCostRandomStdDevLikelyhoodOfNotBeingAFreeRider,
                MemCanBeInvented = WorldProperties.NewMemeLikelyhoodOfNotBeingAFreeRiderMean != 0 || WorldProperties.NewMemeLikelyhoodOfNotBeingAFreeRiderStdDev != 0,
            };
            FeatureDescriptions[(int)AvailableFeatures.HuntingEfficiency] = new FeatureDescription() {
                range = FeatureRange.Positive,
                InitialStateGenesMean = WorldProperties.InitialStateGenesHuntingEfficiencyMean,
                InitialStateGenesStdDev = WorldProperties.InitialStateGenesHuntingEfficiencyStdDev,
                ChancceOfMutation = WorldProperties.MutationChanceHuntingEfficiency,
                MutationStrengthMean = WorldProperties.MutationStrengthMeanHuntingEfficiency,
                MutationStrengthStdDev = WorldProperties.MutationStrengthStdDevHuntingEfficiency,

                MemeEfficiencyMean = WorldProperties.NewMemeHuntingEfficiencyMean,
                MemeEfficiencyStdDev = WorldProperties.NewMemeHuntingEfficiencyStdDev,
                MemePricePedestal = WorldProperties.MemeCostPedestalHuntingEfficiency,
                MemePriceEfficiencyRatio = WorldProperties.MemeCostEfficiencyRatioHuntingEfficiency,
                MemePriceRandomMean = WorldProperties.MemeCostRandomAverageHuntingEfficiency,
                MemePriceRandomStdDev = WorldProperties.MemeCostRandomStdDevHuntingEfficiency,
                MemCanBeInvented = WorldProperties.NewMemeHuntingEfficiencyMean != 0 || WorldProperties.NewMemeHuntingEfficiencyStdDev != 0,
            };
            FeatureDescriptions[(int)AvailableFeatures.HuntingBEfficiency] = new FeatureDescription()
            {
                range = FeatureRange.Positive,
                InitialStateGenesMean = WorldProperties.InitialStateGenesHuntingBEfficiencyMean,
                InitialStateGenesStdDev = WorldProperties.InitialStateGenesHuntingBEfficiencyStdDev,
                ChancceOfMutation = WorldProperties.MutationChanceHuntingBEfficiency,
                MutationStrengthMean = WorldProperties.MutationStrengthMeanHuntingBEfficiency,
                MutationStrengthStdDev = WorldProperties.MutationStrengthStdDevHuntingBEfficiency,

                MemeEfficiencyMean = WorldProperties.NewMemeHuntingBEfficiencyMean,
                MemeEfficiencyStdDev = WorldProperties.NewMemeHuntingBEfficiencyStdDev,
                MemePricePedestal = WorldProperties.MemeCostPedestalHuntingBEfficiency,
                MemePriceEfficiencyRatio = WorldProperties.MemeCostEfficiencyRatioHuntingBEfficiency,
                MemePriceRandomMean = WorldProperties.MemeCostRandomAverageHuntingBEfficiency,
                MemePriceRandomStdDev = WorldProperties.MemeCostRandomStdDevHuntingBEfficiency,
                MemCanBeInvented = WorldProperties.NewMemeHuntingBEfficiencyMean != 0 || WorldProperties.NewMemeHuntingBEfficiencyStdDev != 0,
            };
            FeatureDescriptions[(int)AvailableFeatures.CooperationEfficiency] = new FeatureDescription() {
                range = FeatureRange.Positive,
                InitialStateGenesMean = WorldProperties.InitialStateGenesCooperationEfficiencyMean,
                InitialStateGenesStdDev = WorldProperties.InitialStateGenesCooperationEfficiencyStdDev,
                ChancceOfMutation = WorldProperties.MutationChanceCooperationEfficiency,
                MutationStrengthMean = WorldProperties.MutationStrengthMeanCooperationEfficiency,
                MutationStrengthStdDev = WorldProperties.MutationStrengthStdDevCooperationEfficiency,

                MemeEfficiencyMean = WorldProperties.NewMemeCooperationEfficiencyMean,
                MemeEfficiencyStdDev = WorldProperties.NewMemeCooperationEfficiencyStdDev,
                MemePricePedestal = WorldProperties.MemeCostPedestalCooperationEfficiency,
                MemePriceEfficiencyRatio = WorldProperties.MemeCostEfficiencyRatioCooperationEfficiency,
                MemePriceRandomMean = WorldProperties.MemeCostRandomAverageCooperationEfficiency,
                MemePriceRandomStdDev = WorldProperties.MemeCostRandomStdDevCooperationEfficiency,
                MemCanBeInvented = WorldProperties.NewMemeCooperationEfficiencyMean != 0 || WorldProperties.NewMemeCooperationEfficiencyStdDev != 0,
            };
            FeatureDescriptions[(int)AvailableFeatures.MemoryLimit] = new FeatureDescription() {
                range = FeatureRange.Positive,
                InitialStateGenesMean = WorldProperties.InitialStateGenesMemorySizeMean,
                InitialStateGenesStdDev = WorldProperties.InitialStateGenesMemorySizeStdDev,
                ChancceOfMutation = WorldProperties.MutationChanceMemoryLimit,
                MutationStrengthMean = WorldProperties.MutationStrengthMeanMemoryLimit,
                MutationStrengthStdDev = WorldProperties.MutationStrengthStdDevMemoryLimit,

                MemeEfficiencyMean = 0,
                MemeEfficiencyStdDev = 0,
                MemePricePedestal = 0,
                MemePriceEfficiencyRatio = 0,
                MemePriceRandomMean = 0,
                MemePriceRandomStdDev = 0,
                MemCanBeInvented = false,
            };
            FeatureDescriptions[(int)AvailableFeatures.Creativity] = new FeatureDescription() {
                range = FeatureRange.ZeroToOne,
                InitialStateGenesMean = WorldProperties.InitialStateGenesCreativityMean,
                InitialStateGenesStdDev = WorldProperties.InitialStateGenesCreativityStdDev,
                ChancceOfMutation = WorldProperties.MutationChanceCreativity,
                MutationStrengthMean = WorldProperties.MutationStrengthMeanCreativity,
                MutationStrengthStdDev = WorldProperties.MutationStrengthStdDevCreativity,

                MemeEfficiencyMean = 0,
                MemeEfficiencyStdDev = 0,
                MemePricePedestal = 0,
                MemePriceEfficiencyRatio = 0,
                MemePriceRandomMean = 0,
                MemePriceRandomStdDev = 0,
                MemCanBeInvented = false,
            };
            FeatureDescriptions[(int)AvailableFeatures.UselessActionsLikelihood] = new FeatureDescription() {
                range = FeatureRange.ZeroToOne,
                InitialStateGenesMean = WorldProperties.InitialStateGenesUselessActionsLikelihoodMean,
                InitialStateGenesStdDev = WorldProperties.InitialStateGenesUselessActionsLikelihoodStdDev,
                ChancceOfMutation = WorldProperties.MutationChanceUselessActionsLikelihood,
                MutationStrengthMean = WorldProperties.MutationStrengthMeanUselessActionsLikelihood,
                MutationStrengthStdDev = WorldProperties.MutationStrengthStdDevUselessActionsLikelihood,

                MemeEfficiencyMean = WorldProperties.NewMemeUselessEfficiencyMean,
                MemeEfficiencyStdDev = WorldProperties.NewMemeUselessEfficiencyStdDev,
                MemePricePedestal = WorldProperties.MemeCostPedestalUseless,
                MemePriceEfficiencyRatio = WorldProperties.MemeCostEfficiencyRatioUseless,
                MemePriceRandomMean = WorldProperties.MemeCostRandomAverageUseless,
                MemePriceRandomStdDev = WorldProperties.MemeCostRandomStdDevUseless,
                MemCanBeInvented = WorldProperties.NewMemeUselessEfficiencyMean != 0 || WorldProperties.NewMemeUselessEfficiencyStdDev != 0,
            };
            FeatureDescriptions[(int)AvailableFeatures.AgeingRate] = new FeatureDescription() {
                range = FeatureRange.MinusOneToPlusOne,
                InitialStateGenesMean = WorldProperties.InitialStateGenesAgeingRateMean,
                InitialStateGenesStdDev = WorldProperties.InitialStateGenesAgeingRateStdDev,
                ChancceOfMutation = WorldProperties.MutationChanceAgeingRate,
                MutationStrengthMean = WorldProperties.MutationStrengthMeanAgeingRate,
                MutationStrengthStdDev = WorldProperties.MutationStrengthStdDevAgeingRate,

                MemeEfficiencyMean = WorldProperties.NewMemeAgeingRateMean,
                MemeEfficiencyStdDev = WorldProperties.NewMemeAgeingRateStdDev,
                MemePricePedestal = WorldProperties.MemeCostPedestalAgeingRate,
                MemePriceEfficiencyRatio = WorldProperties.MemeCostEfficiencyRatioAgeingRate,
                MemePriceRandomMean = WorldProperties.MemeCostRandomAverageAgeingRate,
                MemePriceRandomStdDev = WorldProperties.MemeCostRandomStdDevAgeingRate,
                MemCanBeInvented = WorldProperties.NewMemeAgeingRateMean != 0 || WorldProperties.NewMemeAgeingRateStdDev != 0,
            };
            // Заранее отсортировать те мемы, которые могут быть изобретены
            MemesWhichCanBeInvented = FeatureDescriptions
                .Select((description, i) => description.MemCanBeInvented ? i : -1)
                .Where(index => index >= 0)
                .ToArray();
        }
        #endregion


        private static double? baseGompertzAgeingRateLifeCostIncreaseInverse = null;
        private static double? baseGompertzAgeingRateLifeCostDecreaseInverse = null;
        public static double BaseGompertzAgeingRateLifeCostIncreaseInverse {
            get {
                if (!baseGompertzAgeingRateLifeCostIncreaseInverse.HasValue) {
                    baseGompertzAgeingRateLifeCostIncreaseInverse = 1 / baseGompertzAgeingRateLifeCostIncrease;
                }
                return baseGompertzAgeingRateLifeCostIncreaseInverse.Value;
            }
        }

        public static double BaseGompertzAgeingRateLifeCostDecreaseInverse {
            get {
                if (!baseGompertzAgeingRateLifeCostDecreaseInverse.HasValue) {
                    baseGompertzAgeingRateLifeCostDecreaseInverse = 1 / baseGompertzAgeingRateLifeCostDecrease;
                }
                return baseGompertzAgeingRateLifeCostDecreaseInverse.Value;
            }
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class DisplayableProperty : System.Attribute
    {
        public string name;
        public string group;
        public string description;
        private string xmlNameCache=null;

        public DisplayableProperty(string name)
        {
            this.name = name;
            group = "Other";
        }

        public string XMLName
        {
            get {
                if (xmlNameCache == null)
                {
                    string retval = group + "\\" + name;
                    retval = retval.Replace(" ", "").Replace('\\', '_').Replace('(','_').Replace(')','_').Replace('*','-').Replace('=','-').Replace('\'','_').Replace('₂','2').Replace('Δ','D');
                    xmlNameCache = retval;
                }
                return xmlNameCache;
            }
        }
    }

    public class PropertyTree
    {
        public Dictionary<string, PropertyInfo> treeLeafs = new Dictionary<string, PropertyInfo>();
        public Dictionary<string, PropertyTree> treeBranches = new Dictionary<string, PropertyTree>();
    }

    public struct FeatureDescription {
        public FeatureRange range;
        public double InitialStateGenesMean;
        public double InitialStateGenesStdDev;
        public double ChancceOfMutation;
        public double MutationStrengthMean;
        public double MutationStrengthStdDev;

        public double MemeEfficiencyMean;
        public double MemeEfficiencyStdDev;
        public double MemePricePedestal;
        public double MemePriceEfficiencyRatio;
        public double MemePriceRandomMean;
        public double MemePriceRandomStdDev;
        public bool MemCanBeInvented;

        public Double UpperRange {
            get {
                switch (range) {
                    case FeatureRange.MinusOneToPlusOne:
                    case FeatureRange.ZeroToOne:
                        return 1;
                    case FeatureRange.Positive:
                        return Double.PositiveInfinity;                        
                }
                throw new NotImplementedException("The range " + range.ToString() + " is not implemented.");
            }
        }

        public Double LowerRange {
            get {
                switch (range) {
                    case FeatureRange.MinusOneToPlusOne:
                        return -1;
                    case FeatureRange.ZeroToOne:                        
                    case FeatureRange.Positive:
                        return 0;
                }
                throw new NotImplementedException("The range " + range.ToString() + " is not implemented.");
            }
        }
    }

    public enum FeatureRange {
        Positive,
        ZeroToOne,
        MinusOneToPlusOne
    }
}
