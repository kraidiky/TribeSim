using System;
using System.ComponentModel;
using System.Reflection;

namespace TribeSim
{
    /// <summary> Этот класс теперь используем только для статической проверки типов и место хранения статических функций, больше он ни для чего не нужен, со всеми функциями массив отлично справляется. </summary>
    public static class FeatureSet
    {

        /// <summary> Для индексации используются целые числа, к которм приводятся enum-ы: (int)AvailableFeatures.value</summary>
        public static Features GenerateInitialGeneStrand(Random randomizer)
        {
            Features features = default;
            var descriptions = WorldProperties.FeatureDescriptions;
            for (int i = 0; i < descriptions.Length; i++)
                if (descriptions[i].InitialStateGenesMean != 0 || descriptions[i].InitialStateGenesStdDev != 0)
                    do {
                        features[i] = randomizer.NormalRandom(descriptions[i].InitialStateGenesMean, descriptions[i].InitialStateGenesStdDev);
                    } while (features[i] < 0);
            return features;
        }
    }

    public enum AvailableFeatures
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
        Creativity,
        [Description("Useless actions likelihood|Usl")]
        UselessActionsLikelihood,
        [Description("Ageing rate|AR")]
        AgeingRate,
        [Description("Sociability|Soc")]
        Sociability,
        [Description("Foraging Efficiency|Frg")]
        ForagingEfficiency,
        [Description("OrganizationAbility|Org")]
        OrganizationAbility
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
