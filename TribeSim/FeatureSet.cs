using System;
using System.ComponentModel;
using System.Reflection;

namespace TribeSim
{
    /// <summary> Этот класс теперь используем только для статической проверки типов и место хранения статических функций, больше он ни для чего не нужен, со всеми функциями массив отлично справляется. </summary>
    public static class FeatureSet
    {
        /// <summary> Генерит пустой массив нужного размера, чтобы не таскать по всему коду знгания о константе FEATURES_COUNT. Ну и вообще, можно тут под капот спрятать пулинг массивов, если будет желание. </summary>
        public static double[] Blank() {
            return new double[WorldProperties.FEATURES_COUNT];
        }
        /// <summary> Для индексации используются целые числа, к которм приводятся enum-ы: (int)AvailableFeatures.value</summary>
        public static double[] GenerateInitialGeneStrand()
        {
            var genes = Blank();
            var features = WorldProperties.FeatureDescriptions;
            for (int i = 0; i < features.Length; i++)
                genes[i] = SupportFunctions.NormalRandom(features[i].InitialStateGenesMean, features[i].InitialStateGenesStdDev);
            return genes;
        }

        /// <summary> Можно было бы обойтись и Clone но вдруг пулинг потребуется. </summary>
        public static double[] Copy(this double[] genes) {
            var copy = Blank();
            genes.CopyTo(copy, 0);
            return copy;
        }

        public static double GetFeature(this double[] genes, AvailableFeatures feature) {
            return genes[(int)feature];
        }
        public static double SetFeature(this double[] genes, AvailableFeatures feature, double value) {
            return genes[(int)feature] = value;
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
