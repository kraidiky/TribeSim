using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Windows.Threading;
using System.Diagnostics;
using System.Linq;

namespace TribeSim
{
    static class World
    {
        private static Random randomizer;

        private static string baseFolder = null;

        public static bool IsExtinct { get { return tribes.Count == 0; } }

        public static string BaseFolder
        {            
            set { World.baseFolder = value; }
        }

        private static string logFolder;

        public static string LogFolder
        {
            get { return World.logFolder; }            
        }

        private static string memesLogFolder;

        public static string MemesLogFolder
        {
            get { return World.memesLogFolder; }            
        }

        private static string tribesmanLogFolder;

        public static string TribesmanLogFolder
        {
            get { return World.tribesmanLogFolder; }            
        }

        private static string tribesLogFolder;

        public static string TribesLogFolder
        {
            get { return World.tribesLogFolder; }            
        }

        private static string simDataFolder;

        public static string SimDataFolder
        {
            get { return World.simDataFolder; }
        }
        private static List<Tribe> tribes = new List<Tribe>();

        public static int Year { get; private set; }

        public static void InitializeNext(Dispatcher d) {
            Initialize(d, randomizer.Next(int.MaxValue));
        }
        public static void Initialize(Dispatcher d, int randomSeed)
        {
            WorldProperties.ResetFeatureDescriptions();
            randomizer = new Random(randomSeed);
            StatisticsCollector.Reset();
            Meme.ClearMemePool();
            tribes.Clear();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (!string.IsNullOrWhiteSpace(baseFolder))
            {
                logFolder = Path.Combine(baseFolder, DateTime.Now.ToString("yyyyMMdd_HHmm"));                
            }
            else
            {
                baseFolder = Properties.Settings.Default.LogBaseFolder;
                if (string.IsNullOrWhiteSpace(baseFolder))
                {
                    logFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Tribe Sim Results", DateTime.Now.ToString("yyyyMMdd_HHmm"));
                }
                else
                {
                    logFolder = Path.Combine(baseFolder, DateTime.Now.ToString("yyyyMMdd_HHmm"));    
                }
            }
            Directory.CreateDirectory(logFolder);
            tribesLogFolder = Path.Combine(logFolder, "Tribes");
            memesLogFolder = Path.Combine(logFolder, "Memes");
            tribesmanLogFolder = Path.Combine(logFolder, "Tribesmen");
            simDataFolder = Path.Combine(logFolder, "Results");
            Directory.CreateDirectory(tribesmanLogFolder);
            Directory.CreateDirectory(tribesLogFolder);
            Directory.CreateDirectory(memesLogFolder);
            if (WorldProperties.CollectFilesData>0.5)
            {
                Directory.CreateDirectory(simDataFolder);
            }
            Year = 0;
            for (int i = 0; i < WorldProperties.StartingNumberOfTribes; i++ )
            {
                Tribe t = new Tribe(randomizer.Next(int.MaxValue));
                int numberOfTribesmen = (int)Math.Round(randomizer.NormalRandom(WorldProperties.StartingTribePopulationMean, WorldProperties.StartingTribePopulationStdDev));
                for (int ii = 0; ii < numberOfTribesmen; ii++)
                {
                    Tribesman man = Tribesman.GenerateTribesmanFromAStartingSet(randomizer);
                    t.AcceptMember(man);
                }
                tribes.Add(t);
            }

            ReportEndOfYearStatistics();

            d.Invoke(new Action(delegate ()
            {
                StatisticsCollector.ConsolidateNewYear();
            }));
        }

        public static bool TribeExists(string key)
        {
            foreach (Tribe t in tribes)
            {
                if (t.TribeName == key) return true;
            }
            return false;
        }

        private static Stopwatch stopwatch;
        public static TimeSpan spendedTime;

        public static void SimulateYear(Dispatcher d)
        {
            Year++;

            if (Year == 1000) // Первые 1000 циклов пропускаем. Там всякое медленное делается.
                stopwatch = Stopwatch.StartNew();
            if (Year % 11000 == 0) {
                stopwatch.Stop();
                spendedTime = stopwatch.Elapsed;
            }

            PrepareForANewYear();
            if (WorldProperties.SkipLifeSupportStep < 0.5) LifeSupport();
            if (WorldProperties.SkipSpontaneousMemeInventionStep < 0.5) InventMemes();
            if (WorldProperties.SkipForgettingMemesStep < 0.5) ForgetUnusedMemes();
            if (WorldProperties.SkipTeachingStep < 0.5) Teach();
            if (WorldProperties.SkipFreeRiderPunishmentStep < 0.5) PunishFreeRiders();
            if (WorldProperties.SkipHuntingStep < 0.5) HuntAndShare();
            if (WorldProperties.SkipUselessActionsStep < 0.5) UselessAction();
            if (WorldProperties.SkipStudyingStep < 0.5) Study();
            if (WorldProperties.SkipDeathStep < 0.5) Die();

            foreach (Tribe t in tribes.ToArray())
            {
                if (t.IsExtinct)
                {
                    tribes.Remove(t);
                }
            }

            if (WorldProperties.SkipBreedingStep < 0.5) Breed();
            if (WorldProperties.SkipMigrationStep < 0.5) Migrate();
            if (WorldProperties.SkipGroupSplittingStep < 0.5) SplitGroups();
            if (WorldProperties.SkipCulturalExchangeStep < 0.5) CulturalExchange();

            OverpopulationPrevention();

            if (WorldProperties.CollectGraphData > 0.5 || (WorldProperties.CollectFilesData > 0 && (Year % (int)WorldProperties.CollectFilesData == 0 || (Year + 1) % (int)WorldProperties.CollectFilesData == 0)))
            {
                ReportEndOfYearStatistics();

                d.Invoke(new Action(delegate ()
                {
                    StatisticsCollector.ConsolidateNewYear();
                }));
            }

            if (Year % 11000 == 1) {
                Console.WriteLine($"========== year:{Year} ==========");
                for (int i = 0; i < tribes.Count; i++)
                    Console.WriteLine($"tribe[{tribes[i].seed}] rnd:{tribes[i].randomizer.Next(10000)}");
            }
        }

        private static void ReportEndOfYearStatistics()
        {   
            foreach (Tribe t in tribes)
            {
                StatisticsCollector.ReportCountEvent(t.TribeName, "Tribes in the world.");                
                StatisticsCollector.ReportSumEvent(t.TribeName, "Population", t.Population);
            }
            
            World.tribes.Parallel((tribe) => { tribe.ReportEndOfYearStatistics(); });
            if (WorldProperties.CollectLiveMemes > 0.5)
            {
                StatisticsCollector.ReportGlobalSumEvent("Live memes", Meme.CountLiveMemes());
            }
        }

        private static void OverpopulationPrevention()
        {
            while (tribes.Count > WorldProperties.MaximumTribesAllowedInTheWorld)
            {
                Tribe removingTribe = tribes[randomizer.Next(tribes.Count)];
                removingTribe.PrepareForMigrationOutsideOfTheScope();
                tribes.Remove(removingTribe);
            }
        }

        private static void PrepareForANewYear()
        {
             World.tribes.Parallel((tribe) => { tribe.PrepareForANewYear(); });          
        }

        private static void CulturalExchange()
        {
            if (tribes.Count < 2) return;
            while (randomizer.Chance(WorldProperties.CulturalExchangeTeachingChance))
            {
                int t1n, t2n;
                t1n = randomizer.Next(tribes.Count);
                do
                {
                    t2n = randomizer.Next(tribes.Count);
                }
                while (t1n == t2n);
                Tribe tribeFrom = tribes[t1n];
                Tribe tribeTo = tribes[t2n];
                tribeFrom.AttemptCulturalExchangeWith(tribeTo);

                if (WorldProperties.AllowRepetitiveCulturalExchange < 0.5) break;
            }
        }

        private static void Migrate()
        {
            if (tribes.Count == 1) return; // There's nowhere to migrate.
            ConcurrentDictionary<Tribesman, Tribe> migratingTribesmen = new ConcurrentDictionary<Tribesman, Tribe>();
            World.tribes.Parallel((tribe) =>
            {
                List<Tribesman> migratingMembers = tribe.WhoIsMigrating();
                foreach (Tribesman member in migratingMembers)
                {
                    migratingTribesmen.TryAdd(member, tribe);
                }
            });
            var immigrants = migratingTribesmen.ToList();// Предполагаю, что мигрирующих всегда будет мало, так что делаю как попало.
            immigrants.Sort((m1, m2) => m1.Value != m2.Value ? Math.Sign(m1.Value.seed - m2.Value.seed) : Math.Sign(m1.Key.TribeMemberId - m2.Key.TribeMemberId));

            foreach (var m in immigrants)
            {
                Tribe newTribe;
                do
                {
                    newTribe = tribes[randomizer.Next(tribes.Count)];
                } while (newTribe == m.Value);
                newTribe.AcceptMember(m.Key);
            }
        }

        private static void SplitGroups()
        {
            // Тут потенциальный источник невоспроизводимости. Если в один год разделится больше одного лемени они могут оказаться в tribes в произвольном порядке, а порядок используется в некоторых местах например когда племена взаимодействуют между собой. Например при переселении или культурном обмене.
            ConcurrentDictionary<Tribe, Tribe> newTribes = new ConcurrentDictionary<Tribe, Tribe>();
            World.tribes.Parallel((tribe) =>
            {
                Tribe newTribe = tribe.Split();
                if (newTribe != null)
                {
                    newTribes.TryAdd(tribe, newTribe);
                }
            });
            if (newTribes.Count > 0) {
                for(int i = 0; i < tribes.Count; i++) // Эта надстройка фиксирует порядок чтения
                    if (newTribes.TryGetValue(tribes[i], out var child))
                        tribes.Add(child);
            }
        }

        private static void Breed()
        {
            World.tribes.Parallel((tribe) => { if (tribe.Population >= 2) { tribe.Breed(); } });  
        }

        private static void Die()
        {
            World.tribes.Parallel((tribe) => { tribe.Die(); });  
        }

        private static void Study()
        {
            World.tribes.Parallel((tribe) => { if (tribe.Population >= 2) { tribe.Study(); } });  
        }

        private static void UselessAction()
        {
            World.tribes.Parallel((tribe) => { tribe.PerformUselessActions(); });  
        }

        private static void HuntAndShare()
        {
            ConcurrentDictionary<Tribe, double> tribeHuntingEfforts = new ConcurrentDictionary<Tribe, double>(); // Тут порядок всё равно не важен, используется только сумма, поэтому тут ConcurrentDictionary не трогаем
            World.tribes.Parallel((tribe) => { tribeHuntingEfforts.TryAdd(tribe, tribe.GoHunting()); });
            double totalHuntingEffort = 0;
            foreach (Tribe t in tribes)
            {
                totalHuntingEffort += tribeHuntingEfforts[t];
            }            
            Dictionary<Tribe, double> resourcesRecievedByTribes = new Dictionary<Tribe, double>(); // Тут запись идёт в основном потоке, а из тредов только чтение. Чтение не может нарушить целостность, так что нефиг тут ConcurrentDictionary плодить.
            if (totalHuntingEffort <= WorldProperties.ResourcesAvailableFromEnvironmentOnEveryStep || WorldProperties.ResourcesAvailableFromEnvironmentOnEveryStep < 0)
            {
                foreach (Tribe t in tribes)
                {
                    resourcesRecievedByTribes.Add(t, tribeHuntingEfforts[t]);
                }
            }
            else
            {
                var resourcePerHuntingEffort = WorldProperties.ResourcesAvailableFromEnvironmentOnEveryStep / totalHuntingEffort; // Деление - медленная операция. Это, конечно, копейки, но зачем их в пустую тратить.
                foreach (Tribe t in tribes)
                {
                    resourcesRecievedByTribes.Add(t, resourcePerHuntingEffort * tribeHuntingEfforts[t]);
                }
            }
            World.tribes.Parallel((tribe) => { tribe.ReceiveAndShareResource(resourcesRecievedByTribes[tribe]); });
        }

        private static void PunishFreeRiders()
        {
            World.tribes.Parallel((tribe) => { if (tribe.Population >= 2) { tribe.PunishFreeRider(); } });  
        }

        private static void Teach()
        {
            World.tribes.Parallel((tribe) => { if (tribe.Population >= 2) { tribe.Teach(); } });   
        }

        private static void ForgetUnusedMemes()
        {
            World.tribes.Parallel((tribe) => { tribe.ForgetUnusedMeme(); });   
        }

        private static void InventMemes()
        {
            World.tribes.Parallel((tribe) => { tribe.SpontaneousMemeInvention(); });   
        }

        private static void LifeSupport()
        {
            World.tribes.Parallel((tribe) => { tribe.ConsumeLifeSupport(); });            
        }     
  
        
    }
}
