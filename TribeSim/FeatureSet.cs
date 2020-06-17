using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TribeSim
{
    class FeatureSet : DynamicObject
    {
        private Dictionary<AvailableFeatures, double> featureData = new Dictionary<AvailableFeatures, double>();
        public FeatureSet()
        {
            foreach (AvailableFeatures feature in Enum.GetValues(typeof(AvailableFeatures)))
            {
                featureData.Add(feature, 0);                
            }
        }

        public static FeatureSet GenerateInitialGeneStrand()
        {
            FeatureSet retval = new FeatureSet();
            double mean = 0;
            double stddev = 0;

            foreach (AvailableFeatures feature in Enum.GetValues(typeof(AvailableFeatures)))
            {
                switch (feature)
                {
                    case AvailableFeatures.CooperationEfficiency:
                        mean = WorldProperties.InitialStateGenesCooperationEfficiencyMean;
                        stddev = WorldProperties.InitialStateGenesCooperationEfficiencyStdDev;
                        break;
                    case AvailableFeatures.Creativity:
                        mean = WorldProperties.InitialStateGenesCreativityMean;
                        stddev = WorldProperties.InitialStateGenesCreativityStdDev;
                        break;
                    case AvailableFeatures.FreeRiderDeterminationEfficiency:
                        mean = WorldProperties.InitialStateGenesFreeRiderDeterminationEfficiencyMean;
                        stddev = WorldProperties.InitialStateGenesFreeRiderDeterminationEfficiencyStdDev;
                        break;
                    case AvailableFeatures.FreeRiderPunishmentLikelyhood:
                        mean = WorldProperties.InitialStateGenesFreeRiderPunishmentLikelyhoodMean;
                        stddev = WorldProperties.InitialStateGenesFreeRiderPunishmentLikelyhoodStdDev;
                        break;
                    case AvailableFeatures.HuntingEfficiency:
                        mean = WorldProperties.InitialStateGenesHuntingEfficiencyMean;
                        stddev = WorldProperties.InitialStateGenesHuntingEfficiencyStdDev;
                        break;
                    case AvailableFeatures.LikelyhoodOfNotBeingAFreeRider:
                        mean = WorldProperties.InitialStateGenesLikelyhoodOfNotBeingAFreeRiderMean;
                        stddev = WorldProperties.InitialStateGenesLikelyhoodOfNotBeingAFreeRiderStdDev;
                        break;
                    case AvailableFeatures.MemoryLimit:
                        mean = WorldProperties.InitialStateGenesMemorySizeMean;
                        stddev = WorldProperties.InitialStateGenesMemorySizeStdDev;
                        break;
                    case AvailableFeatures.StudyEfficiency:
                        mean = WorldProperties.InitialStateGenesStudyEfficiencyMean;
                        stddev = WorldProperties.InitialStateGenesStudyEfficiencyStdDev;
                        break;
                    case AvailableFeatures.StudyLikelyhood:
                        mean = WorldProperties.InitialStateGenesStudyLikelyhoodMean;
                        stddev = WorldProperties.InitialStateGenesStudyLikelyhoodStdDev;
                        break;
                    case AvailableFeatures.TeachingEfficiency:
                        mean = WorldProperties.InitialStateGenesTeachingEfficiencyMean;
                        stddev = WorldProperties.InitialStateGenesTeachingEfficiencyStdDev;
                        break;
                    case AvailableFeatures.TeachingLikelyhood:
                        mean = WorldProperties.InitialStateGenesTeachingLikelyhoodMean;
                        stddev = WorldProperties.InitialStateGenesTeachingLikelyhoodStdDev;
                        break;
                    case AvailableFeatures.TrickEfficiency:
                        mean = WorldProperties.InitialStateGenesTrickEfficiencyMean;
                        stddev = WorldProperties.InitialStateGenesTrickEfficiencyStdDev;
                        break;
                    case AvailableFeatures.TrickLikelyhood:
                        mean = WorldProperties.InitialStateGenesTrickLikelyhoodMean;
                        stddev = WorldProperties.InitialStateGenesTrickLikelyhoodStdDev;
                        break;
                    default:
                        throw new NotImplementedException("Unknown feature detected while trying to generate new creature.");
                }
                double value = SupportFunctions.NormalRandom(mean, stddev);
                if (!retval.featureData.ContainsKey(feature))
                {
                    retval.featureData.Add(feature, value);
                }
                else
                {
                    retval[feature] = value;
                }
            }

            return retval;
        }

        public FeatureSet(FeatureSet copyFrom)
        {
            foreach (AvailableFeatures feature in Enum.GetValues(typeof(AvailableFeatures)))
            {
                featureData.Add(feature, copyFrom[feature]);
            }
        }

        public double this[AvailableFeatures feature]
        {
            get { return featureData[feature]; }
            set { featureData[feature] = value; }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            AvailableFeatures f = (AvailableFeatures)Enum.Parse(typeof(AvailableFeatures), binder.Name);
            result = featureData[f];
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            AvailableFeatures f = (AvailableFeatures)Enum.Parse(typeof(AvailableFeatures), binder.Name);
            featureData[f] = Convert.ToDouble(value);
            return true;
        }

        
    }

    enum AvailableFeatures
    {
        [Description("Trick likelyhood|TrL")]        
        TrickLikelyhood,
        [Description("Trick efficiency|TrE")]
        TrickEfficiency,
        [Description("Teaching likelyhood|TeL")]
        TeachingLikelyhood,
        [Description("Teaching efficiency|TeE")]
        TeachingEfficiency,
        [Description("Study likelyhood|SL")]
        StudyLikelyhood,
        [Description("Study efficiency|SE")]
        StudyEfficiency,
        [Description("F.r. punishment likelyhood|FRPL")]
        FreeRiderPunishmentLikelyhood,
        [Description("F.r. determintaion efficiency|FRDE")]
        FreeRiderDeterminationEfficiency,
        [Description("Going hunting likelyhood|HL")]
        LikelyhoodOfNotBeingAFreeRider,
        [Description("Hunting efficiency|HE")]
        HuntingEfficiency,
        [Description("Cooperation efficiency|CE")]
        CooperationEfficiency,
        [Description("Memory limit|ML")]
        MemoryLimit,
        [Description("Creativity|Cre")]
        Creativity
    }

    static class EnumExtender
    {
        public static string GetDescription(this Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    DescriptionAttribute attr =
                           Attribute.GetCustomAttribute(field,
                             typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attr != null)
                    {
                        return attr.Description.Split('|')[0];
                    }
                }
            }
            return null;
        }

        public static string GetAcronym(this Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    DescriptionAttribute attr =
                           Attribute.GetCustomAttribute(field,
                             typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attr != null)
                    {
                        return attr.Description.Split('|')[1];
                    }
                }
            }
            return null;
        }
    }
}
