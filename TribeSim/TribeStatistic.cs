
using System.Linq;
using System;

namespace TribeSim {
    class TribeStatistic {
        public enum EventName
        {
            // Это евенты диапазонов, занимают все индексы от 0 до Features.Length - 1
            [StatisticEventsGroupName("Avg. genotype value ({0})")]
            AverageGenotypeValue = Features.Length - 1,
            [StatisticEventsGroupName("Avg. phenotype value ({0})")]
            AveragePhenotypeValue = Features.Length * 2 - 1,
            [StatisticEventsGroupName("% memory: {1}")]
            MemoryPercentageUsage = Features.Length * 3 - 1,
            [StatisticEventsGroupName("avg. memes size: {1}")]
            MemesSize = Features.Length * 4 - 1,
            [StatisticEventsGroupName("avg. memes relative effectiveness: {1}")]
            MemesRelativeEffectiveness = Features.Length * 5 - 1,

            [StatisticEventName("Average memes known")]
            AverageMemesKnown,
            [StatisticEventName("Average resources posessed")]
            AverageResourcesPosessed,
            [StatisticEventName("Deaths of hunger")]
            DeathsOfHunger,
            [StatisticEventName("Deaths of old age")]
            DeathsOfOldAge,
            [StatisticEventName("Deaths of lonliness")]
            DeathsOfLonliness,
            [StatisticEventName("% memory: unused when died")]
            MemoryUnusedWhenDied,
            [StatisticEventName("Longevity")]
            Longevity,
            [StatisticEventName("Child births")]
            ChildBirths,
            [StatisticEventName("Child average brain size")]
            ChildAverageBrainSize,
            [StatisticEventName("Meme Invented")]
            MemeInvented,
            [StatisticEventName("% memory: unused")]
            MemoryUnused,
            [StatisticEventName("Live memes")]
            LiveMemes,
            [StatisticEventName("Tribes in the world.")]
            TribesInTheWorld,
            [StatisticEventName("Population")]
            Population,
            [StatisticEventName("Average hunting efforts")]
            AverageHuntingEfforts,
            [StatisticEventName("Total hunting efforts")]
            TotalHuntingEfforts,
            [StatisticEventName("Percentage of hunters")]
            PercentageOfHunters,
            [StatisticEventName("Total memes types")]
            TotalMemesTypes,
            [StatisticEventName("Memetical Equality")]
            MemeticalEquality,
        }
        public class StatisticEventName : Attribute
        {
            public readonly string Name;
            public StatisticEventName(string name)
            {
                Name = name;
            }
        }
        public class StatisticEventsGroupName : Attribute
        {
            public readonly string NameFormat;
            public StatisticEventsGroupName(string nameFormat)
            {
                NameFormat = nameFormat;
            }
        }
        public static string[] eventFullNames;
        public static int EventNamesCount => Enum.GetValues(typeof(EventName)).Cast<int>().Max() + 1;

        static TribeStatistic() {
            eventFullNames = new string[EventNamesCount];
            foreach (var key in Enum.GetValues(typeof(EventName))) {
                int index = (int)key;
                var enumItem = (EventName)key;
                var groupName = enumItem.GetType().GetField(enumItem.ToString()).GetCustomAttributes(typeof(StatisticEventsGroupName), true).Where(a => a is StatisticEventsGroupName).Cast<StatisticEventsGroupName>().FirstOrDefault();
                if (groupName != null) {
                    for (int i = 0; i < Features.Length; i++)
                        eventFullNames[index - i] = String.Format(groupName.NameFormat, ((AvailableFeatures)i).GetDescription(), ((AvailableFeatures)i).ToString());
                }
                var eventName = enumItem.GetType().GetField(enumItem.ToString()).GetCustomAttributes(typeof(StatisticEventName), true).Where(a => a is StatisticEventName).Cast<StatisticEventName>().FirstOrDefault();
                if (eventName != null)
                    eventFullNames[index] = eventName.Name;
            }
        }

        public int eventTypesCount;
        public StatisticElement[] elements;
        public TribeStatistic() {
            elements = new StatisticElement[EventNamesCount];
        }
        public TribeStatistic CollectThisYear;

        public void ReportCountEvent(EventName evnt) {
            var element = elements[(int)evnt] ??= new StatisticElement() { type = StatisticElement.EventType.count }; // Не проверяю тип, потому что время теряем, а ошибка такого типа маловероятна.
            element.count++;
        }
        public void ReportCountEvent(EventName evnt, AvailableFeatures feature)
        {
            var element = elements[(int)evnt - (int)feature] ??= new StatisticElement() {type = StatisticElement.EventType.count };
            element.count++;
        }

        public void ReportSumEvent(EventName evnt, double value)
        {
            var element = elements[(int)evnt] ??= new StatisticElement() { type = StatisticElement.EventType.sum };
            element.value += (float)value;
        }
        public void ReportSumEvent(EventName evnt, AvailableFeatures feature, double value)
        {
            var element = elements[(int)evnt - (int)feature] ??= new StatisticElement() { type = StatisticElement.EventType.sum };
            element.value += (float)value;
        }

        public void ReportAvgEvent(EventName evnt, double value) {
            var element = elements[(int)evnt] ??= new StatisticElement() { type = StatisticElement.EventType.avg };
            element.count++;
            element.value += (float)value;
        }
        public void ReportAvgEvent(EventName evnt, AvailableFeatures feature, double value)
        {
            var element = elements[(int)evnt - (int)feature] ??= new StatisticElement() { type = StatisticElement.EventType.avg };
            element.count++;
            element.value += (float)value;
        }

        public TribeStatistic Include(TribeStatistic other) {
            for (int i = 0; i < other.elements.Length; i++) {
                var element = other.elements[i];
                if (element != null) {
                    var globalElement = elements[i] ??= new StatisticElement() { type = element.type };
                    switch (element.type) {
                        case StatisticElement.EventType.count:
                            globalElement.count += element.count;
                            break;
                        case StatisticElement.EventType.sum:
                            globalElement.value += element.value;
                            break;
                        case StatisticElement.EventType.avg:
                            globalElement.value += element.value;
                            globalElement.count += element.count;
                            break;
                    }
                }
            }
            return this;
        }

        public TribeStatistic Clear()
        {
            for (int i = 0; i < elements.Length; i++)
                elements[i] = null;
            return this;
        }

        public TribeStatistic Reset() {
            for (int i = 0; i < elements.Length; i++)
                if (elements[i] != null)
                    elements[i].value = elements[i].count = 0;
            return this;
        }
        public class StatisticElement {
            public enum EventType { count, sum, avg }
            public EventType type;
            public float value;
            public int count;

            public float ValueFloat { get {
                    switch (type)
                    {
                        case EventType.count:
                            return count;
                        case EventType.sum:
                            return value;
                        case EventType.avg:
                            return count > 0 ? value / count : 0;
                        default:
                            return 0;
                    }
            }}

            public string ValueString { get {
                switch (type) {
                    case EventType.count:
                        return count.ToString();
                    case EventType.sum:
                        return value.SignificantSigns(3);
                    case EventType.avg:
                        return (count > 0 ? value / count : 0).SignificantSigns(3);
                    default:
                        return null;
                }
            }}
        }
    }
}
