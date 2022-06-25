﻿namespace TribeSim
{
    public struct Features
    {
        public double TrickLikelyhood;
        public double TrickEfficiency;
        public double TeachingLikelyhood;
        public double TeachingEfficiency;
        public double StudyLikelyhood;
        public double StudyEfficiency;
        public double FreeRiderPunishmentLikelyhood;
        public double FreeRiderDeterminationEfficiency;
        public double LikelyhoodOfNotBeingAFreeRider;
        public double HuntingEfficiency;
        public double HuntingBEfficiency;
        public double CooperationEfficiency;
        public double MemoryLimit;
        public double Creativity;
        public double UselessActionsLikelihood;
        public double AgeingRate;
        public double Sociability;
        public double ForagingEfficiency;
        public double OrganizationAbility;

        public const int Length = 19;

        public double this[int index]
        {
            get
            {
                unsafe
                {
                    fixed (double* features = &TrickLikelyhood)
                    {
                        return *(features + index);
                    }
                }
            }
            set
            {
                unsafe
                {
                    fixed (double* features = &TrickLikelyhood)
                    {
                        *(features + index) = value;
                    }
                }
            }
        }
    }
}
