using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TribeSim
{
    class Tribe
    {
        private static int nextFreeId = 0;

        public int id;
        public int seed;
        public Random randomizer;

        private List<Tribesman> members = new List<Tribesman>();
        private MemesSetWithCounting memesSet = new();
        private bool keepsLog = false;
        private string logFileName = "";
        private int yearBegun;
        private int lastYearActive;

        private List<Meme> logMemesList = new List<Meme>();
        private List<Tribesman> logTribesmenList = new List<Tribesman>();
        private StringBuilder logMembers = new StringBuilder();
        private StringBuilder logMemes = new StringBuilder();
        private StringBuilder logInventions = new StringBuilder();
        private StringBuilder logForgettings = new StringBuilder();
        private StringBuilder logTeachings = new StringBuilder();
        private StringBuilder logPunishments = new StringBuilder();
        private StringBuilder logHunt = new StringBuilder();
        private StringBuilder logUselessActions = new StringBuilder();
        private StringBuilder logStudies = new StringBuilder();
        private StringBuilder logDeaths = new StringBuilder();
        private StringBuilder logBirths = new StringBuilder();
        private StringBuilder logImmigration = new StringBuilder();
        private StringBuilder logCultuarlExchanges = new StringBuilder();

        public Tribe(int randomSeed)
        {
            id = nextFreeId++; // Племена создаются только из майнтрэда
            randomizer = new Random(seed = randomSeed);
            TribeName = NamesGenerator.GenerateTribeName();
            keepsLog = randomizer.Chance(WorldProperties.ChancesThatTribeWillWriteALog);
            yearBegun = World.Year;
            lastYearActive = yearBegun;
            if (keepsLog)
            {
                logFileName = Path.Combine(World.TribesLogFolder, TribeName + "(" + World.Year);
                File.WriteAllText(logFileName, Properties.Resources.TribeViewerTemplateBeginning);
            }
        }

        ~Tribe()
        {
            if (keepsLog)
            {
                File.AppendAllText(logFileName, Properties.Resources.TribeViewerTemplateEnding);
                File.Move(logFileName, logFileName + " - " + lastYearActive + ".html");
            }
        }

        public int Population
        {
            get { return members.Count(); }
        }
        public bool IsExtinct
        {
            get { return members.Count == 0; }
        }
        public readonly int TribeId;
        public string TribeName = null;
        private int yearBorn = 0;
        private HashSet<int> memesUsedThisYearHash = new HashSet<int>();
        private List<Meme> memesUsedThisYear = new List<Meme>();

        private int nextFreeMemberId = 0;
        public void AcceptMember(Tribesman member)
        {
            member.MyTribe = this;
            member.TribeMemberId = nextFreeMemberId++;
            member.SetRandomizer(randomizer);
            members.Add(member);
            memesSet.Add(member.knownMemes);
            member.ReportJoiningTribe(this);
            if (keepsLog && !logTribesmenList.Contains(member))
            {
                if (logTribesmenList.Count>0) logMembers.Append(", ");
                logMembers.Append(member.GetJSONString());                
                logTribesmenList.Add(member);
            }
        }

        public void MemberDie(Tribesman member)
        {
            MemberLeaves(member);
        }

        public void MemberLeaves(Tribesman member)
        {
            memesSet.Remove(member.knownMemes);
            member.MyTribe  = null;
            members.Remove(member);
        }

        public void MemeAdded(Tribesman member, Meme meme) {
            memesSet.Add(meme);
        }
        public void MemeRemoved(Tribesman member, Meme meme) {
            memesSet.Remove(meme);
        }

        public void MemeUsed(Tribesman member, Meme e)
        {
            if (memesUsedThisYearHash.Add(e.MemeId)) {
                memesUsedThisYear.FindIndexIntoSortedList(e, out var index);
                memesUsedThisYear.Insert(index, e);
            }
        }

        public void ConsumeLifeSupport()
        {
            bool isFirst = true;
            foreach (Tribesman man in members)
            {
                man.ConsumeLifeSupport();
                if (keepsLog)
                {
                    if (!isFirst) logMembers.Append(", ");
                    logMembers.Append(man.GetJSONString());
                    isFirst = false;
                    logTribesmenList.Add(man);
                }
            }
        }

        public void SpontaneousMemeInvention()
        {
            foreach (Tribesman man in members)
            {
                man.TryInventMemeSpontaneously();
            }
        }

        public void ForgetUnusedMeme()
        {
            foreach (Tribesman man in members)
            {
                man.ForgetUnusedMemes();
            }
        }

        public void Teach()
        {
            foreach (Tribesman man in members)
            {
                Tribesman student;
                if (members.Count==1) return;
                do
                {
                    student = members[randomizer.Next(members.Count)];
                    while (student == man) // this cycle is just to make sure he's not trying to tech himself
                    {
                        student = members[randomizer.Next(members.Count)];
                    }
                } while (man.TryToTeach(student) && WorldProperties.AllowRepetitiveTeaching > 0.5);
            }
            // Collect tribe memes info
            StatisticsCollector.ReportAverageEvent(TribeName, "Total memes types", memesSet.memesSet.memes.Count);
            if (memesSet.counts.Count > 0)
            {
                int maxMemesCount = memesSet.counts.Max();
                double memeticalEquality = ((double)memesSet.counts.Sum()) / maxMemesCount / memesSet.memesSet.memes.Count;
                StatisticsCollector.ReportAverageEvent(TribeName, "Memetical Equality", memeticalEquality);
            }
        }

        private List<Tribesman> _punishers = new List<Tribesman>();
        private List<double> _freeraidersFeatures = new List<double>();
        private List<Tribesman> _freeRaiders = new List<Tribesman>();
        public void PunishFreeRider()
        {
            _punishers.Clear();
            for (int i = 0; i < members.Count; i++) {
                var man = members[i];
                var chance =  man.Phenotype.FreeRiderPunishmentLikelyhood;
                if (randomizer.Chance(chance))
                    _punishers.Add(man);
            }
            if (_punishers.Count > 0) {
                _freeraidersFeatures.Clear();
                double min = double.MaxValue, max = double.MinValue;
                for (int i = 0; i < members.Count; i++) {
                    double feature = members[i].Phenotype.LikelyhoodOfNotBeingAFreeRider;
                    min = Math.Min(min, feature);
                    max = Math.Max(max, feature);
                    _freeraidersFeatures.Add(feature);
                }
                double threshold = min + (max - min) * 0.25;
                _freeRaiders.Clear();
                for (int i = 0; i < _freeraidersFeatures.Count; i++) {
                    if (_freeraidersFeatures[i] > threshold)
                        _freeRaiders.Add(members[i]);
                }
                if (_freeRaiders.Count > 0) // Такая шизовая ситуация возможна если у всех в племени строго одинаковый геном по данному признаку.
                    for (int i = 0; i < _punishers.Count; i++)
                        _punishers[i].DetermineAndPunishAFreeRider(members, _freeRaiders);
            }
        }

        public double GoHunting(AvailableFeatures huntingEfficiencyFeature)
        {
            double maxOrganizationAbility = 0;
            Tribesman organizator = null;
            foreach (Tribesman man in members)
            {
                var organizationAbility = man.Phenotype.OrganizationAbility;
                if (maxOrganizationAbility < organizationAbility) {
                    organizator = man;
                    maxOrganizationAbility = organizationAbility;
                }
            }
                

            double sumHuntingPowers = 0;
            double cooperationCoefficient = 0;
            double sumGenotypeHuntingPowers = 0;
            int numHunters = 0;
            foreach (Tribesman man in members)
            {
                var chance = man.Phenotype.LikelyhoodOfNotBeingAFreeRider;
                if (randomizer.Chance(chance))
                {
                    double huntingEfforts = man.GoHunting(huntingEfficiencyFeature);
                    if (huntingEfforts > 0)
                    {
                        sumHuntingPowers += huntingEfforts;
                        cooperationCoefficient += man.Phenotype.CooperationEfficiency;
                        numHunters++;
                        if (maxOrganizationAbility != 0)
                            sumGenotypeHuntingPowers += man.Genotype[(int)huntingEfficiencyFeature];
                    }
                }
                else
                {
                    man.SkipHunting();
                }
            }
            if (members.Count > 0)
            {
                StatisticsCollector.ReportAverageEvent(TribeName, "Percentage of hunters", (double)numHunters / members.Count);
            }
            if (numHunters == 0)
            {
                return 0;
            }
            if (maxOrganizationAbility != 0)
            {
                memesSet.memesSet.CalculateEffect((int)huntingEfficiencyFeature);
                var maxHuntingEffort = sumGenotypeHuntingPowers + memesSet.memesSet.MemesEffect[(int)huntingEfficiencyFeature] * numHunters;
                sumHuntingPowers = sumHuntingPowers + (maxHuntingEffort - sumHuntingPowers) * maxOrganizationAbility;
                organizator.UseMemeGroup(AvailableFeatures.OrganizationAbility, "GoHunting");
            }

            cooperationCoefficient /= numHunters;
            StatisticsCollector.ReportAverageEvent(TribeName, "Average hunting efforts", sumHuntingPowers * cooperationCoefficient);
            StatisticsCollector.ReportSumEvent(TribeName, "Total hunting efforts", sumHuntingPowers * cooperationCoefficient);

            return sumHuntingPowers * cooperationCoefficient;
            
        }

        public double foragingEffort = 0;
        public void CalculateForagingEffort()
        {
            foragingEffort = 0;
            foreach (var member in members)
                foragingEffort += member.GetForagingEffort();
        }

        public void ShareForagingResources(double resourcesPerForagingEfficiency)
        {
            foreach (var member in members)
                member.RecieveResourcesShare(resourcesPerForagingEfficiency * member.Phenotype.ForagingEfficiency);
        }

        private double[] takenShares = new double[0];
        public void ReceiveAndShareResource(double resourcesReceivedPerGroup)
        {
            if (double.IsNaN(resourcesReceivedPerGroup))
            {
                throw new ArgumentException("Received NaN resources");
            }
            if (takenShares.Length != members.Count)
                takenShares = new double[members.Count]; // Количество мемберов меняется довольно редко, создание массива тут оправдано.

            double totalShare = 0;
            for (int i = 0; i < takenShares.Length; i++) {
                double requestedShare = members[i].TellHowMuchDoYouWant(resourcesReceivedPerGroup);
                takenShares[i] = requestedShare;
                totalShare += requestedShare;

            }

            double resourcesPerRequest = resourcesReceivedPerGroup / totalShare; // Деление вообще довольно медленная операция, не надо ей злоупотреблять
            for (int i = 0; i < takenShares.Length; i++)
            {
                members[i].RecieveResourcesShare(resourcesPerRequest * takenShares[i]);
            }
        }

        public void PerformUselessActions()
        {
            foreach (Tribesman man in members)
            {
                man.PerformUselessActions();
            }
        }

        private List<Meme> _memesCache = new List<Meme>(); // Достаточно одного кэшированного листа, потому что массивы никогда не обрабатываются одновременно. 
        public void Study()
        {
            foreach (Tribesman man in members)
            {
                man.StudyOneOrManyOfTheMemesUsedThisTurn(memesUsedThisYear, _memesCache);
            }
        }

        public void PrepareForANewYear()
        {
            memesUsedThisYearHash.Clear();
            memesUsedThisYear.Clear();
            foreach (Tribesman man in members)
            {
                man.PrepareForANewYear();
            }
        }

        public void Die()
        {
            for (var i = members.Count - 1; i >= 0; --i)
            {
                Tribesman man = members[i];
                if (man.WantsToDie())
                {
                    MemberDie(man);
                }
            }

            if (members.Count == 1)
            {
                members[0].DieOfLonliness();
                MemberDie(members[0]);
            }
        }

        private List<Tribesman> breedingPartners = new List<Tribesman>();
        public void Breed()
        {
            breedingPartners.Clear();
            members
                .Where(member => member.IsOfReproductionAge)
                .ForEach(member => breedingPartners.Add(member)); // То же самое, что toList(), но не срёт в память. А у нас GC на секундочку, 17% производительности жрёт на момент когда я до этого места дорвался.

            while (breedingPartners.Count > 1)
            {   // breedingPartners.Remove(PartnerA) адски тяжёлая операция, потому что во-первых, надо этот элемент теперь в списке найти за линейное время. А потом ещё раз за линейное время сдвингуть все элементы, которые в списке идёт после него, между тем порядок в данном случае для нас абсолютно не важен. Одно только это место жрало 13% производительности когда я до него добрался.
                int index = randomizer.Next(breedingPartners.Count);
                int last = breedingPartners.Count - 1;
                Tribesman PartnerA = breedingPartners[index];
                breedingPartners[index] = breedingPartners[last];
                breedingPartners.RemoveAt(last);

                index = randomizer.Next(breedingPartners.Count);
                last = breedingPartners.Count - 1;
                Tribesman PartnerB = breedingPartners[index];
                breedingPartners[index] = breedingPartners[last];
                breedingPartners.RemoveAt(last);

                Tribesman child = Tribesman.Breed(randomizer, PartnerA, PartnerB, _memesCache);
                if (child != null)
                {
                    PartnerA.childrenCount++;
                    PartnerB.childrenCount++;
                    this.AcceptMember(child);
                }
            }
        }

        public Tribe Split()
        {
            if (WorldProperties.SplitTribeIfBiggerThen > 0)
            {
                if (members.Count <= WorldProperties.SplitTribeIfBiggerThen)
                {
                    return null;
                }
            }
            else
            {
                double sociability = 0;
                foreach (var member in members)
                    sociability += member.Phenotype.Sociability;
                if (members.Count * members.Count <= sociability)
                {
                    if (members.Count * members.Count * 2 > sociability)
                        foreach (var member in members)
                            member.UseMemeGroup(AvailableFeatures.Sociability, "Tribe bigger than half of maximum size");
                    return null;
                }
                
            }

            Tribe newTribe = new Tribe(randomizer.Next(int.MaxValue));
            var newTribeMembersCount = members.Count * WorldProperties.SplitTribeRatio; 
            for (int i = 0; i < newTribeMembersCount; i++)
            {
                Tribesman member = members[randomizer.Next(members.Count)];
                MemberLeaves(member);
                newTribe.AcceptMember(member);
            }
            return newTribe;
        }

        public List<Tribesman> WhoIsMigrating()
        {
            List<Tribesman> leavers = new List<Tribesman>();
            if (members.Count > WorldProperties.MigrateFromGroupsLargerThan)
            {
                foreach (Tribesman member in members)
                {
                    if (member.WantsToLeaveTribe())
                    {
                        leavers.Add(member);
                    }
                }
                foreach (Tribesman leaver in leavers)
                {
                    MemberLeaves(leaver);
                }
            }
            return leavers;
        }

        public void AttemptCulturalExchangeWith(Tribe tribeTo)
        {
            Tribesman man = members[randomizer.Next(members.Count)];
            {
                Tribesman student;
                student = members[randomizer.Next(members.Count)];
                while (student == man) // this cycle is just to make sure he's not trying to teach himself
                {
                    student = members[randomizer.Next(members.Count)];
                }
                man.TryToTeach(student,true);                
            }
        }

        public void PrepareForMigrationOutsideOfTheScope()
        {
            foreach (Tribesman man in members.ToArray())
            {
                man.PrepareForMigrationOutsideOfTheScope();
                MemberLeaves(man);
            }
        }

        public void ReportEndOfYearStatistics()
        {            
            int count = members.Count;
            for (int i=0; i<count; i++)
            {
                members[i].ReportEndOfYearStatistics();
            }
            if (count>1)
            {
                lastYearActive = World.Year;
            }
            if (keepsLog)
            {
                bool isMemeFirst = true; ;
                foreach (Tribesman man in logTribesmenList)
                {
                    foreach (Meme meme in man.knownMemes)
                    {
                        if (!logMemesList.Contains(meme))
                        {
                            logMemesList.Add(meme);
                            if (isMemeFirst) logMemes.Append(", ");
                            logMemes.Append(meme.GetJSONString());
                            isMemeFirst = false;
                        }
                    }
                }

                logMemes.Clear();
                logMembers.Clear();
                logMemesList.Clear();
                logTribesmenList.Clear();
            }
        }

        public IEnumerable<Meme> AllMemes() => members.SelectMany(member => member.knownMemes);
    }
}
