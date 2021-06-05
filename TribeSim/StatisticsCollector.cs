using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DocumentFormat.OpenXml.Wordprocessing;

namespace TribeSim
{
    static class StatisticsCollector
    {
        public const string GLOBAL = "Global";
        private static ConcurrentDictionary<string, TribeDataSet> tribeDataSets = new ConcurrentDictionary<string, TribeDataSet>();
        private static ConcurrentDictionary<Type, IDetaliedData> dataSets = new ConcurrentDictionary<Type, IDetaliedData>();

        public static void ReportCountEvent(string tribeName, string eventName)
        {
            if (!tribeDataSets.TryGetValue(tribeName, out var dataset))
                tribeDataSets.TryAdd(tribeName, dataset = new TribeDataSet());
            dataset.ReportCountEvent(eventName);
            ReportGlobalCountEvent(eventName);
        }
        public static void ReportGlobalCountEvent(string eventName) {
            if (!tribeDataSets.TryGetValue(GLOBAL, out var dataset))
                tribeDataSets.TryAdd(GLOBAL, dataset = new TribeDataSet());
            dataset.ReportCountEvent(eventName);
        }

        public static void ReportSumEvent(string tribeName, string eventName, double dValue)
        {
            float value = (float)dValue;
            if (!tribeDataSets.TryGetValue(tribeName, out var dataset))
                tribeDataSets.TryAdd(tribeName, dataset = new TribeDataSet());
            dataset.ReportSumEvent(eventName, value);
            ReportGlobalSumEvent(eventName, value);
        }
        public static void ReportGlobalSumEvent(string eventName, double dValue) {
            if (!tribeDataSets.TryGetValue(GLOBAL, out var dataset))
                tribeDataSets.TryAdd(GLOBAL, dataset = new TribeDataSet());
            dataset.ReportSumEvent(eventName, (float)dValue);
        }

        public static void ReportAverageEvent(string tribeName, string eventName, double dValue)
        {
            float value = (float)dValue;
            if (!tribeDataSets.TryGetValue(tribeName, out var dataset))
                tribeDataSets.TryAdd(tribeName, dataset = new TribeDataSet());
            dataset.ReportAverageEvent(eventName, value);
            ReportGlobalAverageEvent(eventName, value);
        }
        public static void ReportGlobalAverageEvent(string eventName, double dValue) {
            float value = (float)dValue;
            if (!tribeDataSets.TryGetValue(GLOBAL, out var dataset))
                tribeDataSets.TryAdd(GLOBAL, dataset = new TribeDataSet());
            dataset.ReportAverageEvent(eventName, value);
        }

        public static void ReportEvent<T>(T dValue) where T : struct, IDetaliedEvent
        {
            var type = dValue.GetType();
            var storage = (DetaliedData<T>)dataSets.GetOrAdd(type, t => new DetaliedData<T>());
            storage.Store(dValue);
        }

        private static object _individualSucessLocker = new object();
        public static void ConsolidateNewYear()
        {
            List<string> toRemove = new List<string>();
            foreach (KeyValuePair<string, TribeDataSet> kvp in tribeDataSets)
            {
                if (kvp.Key == StatisticsCollector.GLOBAL) {
                    kvp.Value.ConsolidateNewYear(kvp.Key);
                }
                if (World.TribeExists(kvp.Key))
                {
                    kvp.Value.ConsolidateNewYear(kvp.Key + '-' + World.GetTribeId(kvp.Key));
                }
                else if (kvp.Key != StatisticsCollector.GLOBAL && WorldProperties.CollectFilesData>0.5)
                {
                    toRemove.Add(kvp.Key);
                }
            }
            if (WorldProperties.CollectFilesData > 0.5)
            {
                foreach (string extinctTribe in toRemove)
                {
                    TribeDataSet tds;
                    tribeDataSets.TryRemove(extinctTribe, out tds);
                }
            }

            if (dataSets.ContainsKey(typeof(Tribesman.IndividualSucess))) {
                string filename = Path.Combine(World.TribesmanLogFolder, $"IndividualSucess.txt");
                if (dataSets.TryRemove(typeof(Tribesman.IndividualSucess), out var queue))
                {
                    Task.Run(() => {
                        lock (_individualSucessLocker)
                        {
                            var fileExists = File.Exists(filename);
                            while (queue.ReStore(out var data))
                            {
                                if (!fileExists)
                                {
                                    fileExists = true;
                                    File.AppendAllText(filename, data.Header());
                                }

                                File.AppendAllText(filename, data.Data());
                                data.Clear();
                            }

                        }
                    });
                }
            }
        }

        public static DataSet GetDataSet(string tribeName, string eventName)
        {
            if (tribeDataSets.ContainsKey(tribeName))
            {
                return tribeDataSets[tribeName].GetDataSet(eventName);
            }
            else
            {
                return null;
            }
        }

        public static List<string> GetTribeNames()
        {
            return tribeDataSets.Keys.ToList();
        }
        public static List<string> GetEventNames(string tribe)
        {
            if (tribeDataSets.ContainsKey(tribe))
            {
                return tribeDataSets[tribe].GetEventNames();
            }
            else
            {
                return null;
            }
        }

        public static void Reset()
        {
            tribeDataSets.Clear();
            dataSets.Clear();
        }

        public static void ConsolidateAllRuns()
        {
           
        }
    }


    class DataSet
    {
        ObservableDataSource<Point> pointsCollection = new ObservableDataSource<Point>();

        private string name;

        public string Name
        {
            get { return name; }
        }
        public ObservableDataSource<Point> DataSource
        {
            get { return pointsCollection; }
        }
        public DataSet(string dataSetName)
        {
            name = dataSetName;
        }

        //ConcurrentDictionary<int, float> dataPoints = new ConcurrentDictionary<int, float>();

        public void AddPoint(int x, float y)
        {
            /*if (dataPoints.ContainsKey(x))
            {
                throw new Exception("Overlapping data point in dataset " + Name + ". x=" + x);
            }*/
            //dataPoints.TryAdd(x, y);
            pointsCollection.Collection.Add(new Point(x, y));
        }
    }
    class TribeDataSet
    {
        private ConcurrentDictionary<string, DataSet> consolidatedData = new ConcurrentDictionary<string, DataSet>();
        private ConcurrentDictionary<string, int> unconsolidatedCountEvents = new ConcurrentDictionary<string, int>();
        private ConcurrentDictionary<string, float> unconsolidatedSumEvents = new ConcurrentDictionary<string, float>();
        private ConcurrentDictionary<string, float> unconsolidatedAvgEventsSum = new ConcurrentDictionary<string, float>();
        private ConcurrentDictionary<string, int> unconsolidatedAvgEventsCount = new ConcurrentDictionary<string, int>();
        private static List<string> fileColumnNames;        

        static TribeDataSet() {
            fileColumnNames = new List<string>();
            fileColumnNames.Add("Year");
        }

        public void ConsolidateNewYear(string TribeFileName)
        {
            Dictionary<string, string> fileRow = new Dictionary<string, string>();
            List<string> columnsToAdd = new List<string>();            
            bool shouldCollectFiles = WorldProperties.CollectFilesData > 0.5 && World.Year % ((int)Math.Round(WorldProperties.CollectFilesData)) == 0;
            bool shouldCollectGraphs = WorldProperties.CollectGraphData > 0.5 && World.Year % ((int)Math.Round(WorldProperties.CollectGraphData)) == 0;

            if (shouldCollectFiles)
            {
                fileRow.Add("Year", World.Year.ToString());             
            }
            foreach (KeyValuePair<string, int> kvp in unconsolidatedCountEvents)
            {
                if (shouldCollectGraphs)
                {
                    if (!consolidatedData.ContainsKey(kvp.Key))
                    {
                        consolidatedData.TryAdd(kvp.Key, new DataSet(kvp.Key));
                    }
                    consolidatedData[kvp.Key].AddPoint(World.Year, kvp.Value);
                }
                if (shouldCollectFiles)
                {
                    if (!fileColumnNames.Contains(kvp.Key)) {
                        columnsToAdd.Add(kvp.Key);
                    }                    
                    fileRow.Add(kvp.Key, kvp.Value.ToString());
                }
                unconsolidatedCountEvents[kvp.Key] = 0;
            }

            foreach (KeyValuePair<string, float> kvp in unconsolidatedSumEvents)
            {
                if (shouldCollectGraphs)
                {
                    if (!consolidatedData.ContainsKey(kvp.Key))
                    {
                        consolidatedData.TryAdd(kvp.Key, new DataSet(kvp.Key));
                    }
                    consolidatedData[kvp.Key].AddPoint(World.Year, kvp.Value);
                }
                if (shouldCollectFiles)
                {
                    if (!fileColumnNames.Contains(kvp.Key)) {
                        columnsToAdd.Add(kvp.Key);
                    }           
                    fileRow.Add(kvp.Key, kvp.Value.ToString("F3", CultureInfo.InvariantCulture));                    
                }
            }
            unconsolidatedSumEvents.Clear();
            foreach (KeyValuePair<string, float> kvp in unconsolidatedAvgEventsSum)
            {
                float avgValue = 0;
                if (unconsolidatedAvgEventsCount[kvp.Key] != 0) {
                    avgValue = kvp.Value / unconsolidatedAvgEventsCount[kvp.Key];
                }
                if (shouldCollectGraphs)
                {
                    if (!consolidatedData.ContainsKey(kvp.Key))
                    {
                        consolidatedData.TryAdd(kvp.Key, new DataSet(kvp.Key));
                    }
                    if (unconsolidatedAvgEventsCount[kvp.Key] != 0)
                    {
                        consolidatedData[kvp.Key].AddPoint(World.Year, avgValue);
                    }
                }
                if (shouldCollectFiles)
                {
                    if (!fileColumnNames.Contains(kvp.Key))
                    {                        
                         columnsToAdd.Add(kvp.Key);
                    }
                    fileRow.Add(kvp.Key, avgValue.ToString("F3", CultureInfo.InvariantCulture));                    
                }
            }
            unconsolidatedAvgEventsSum.Clear();
            unconsolidatedAvgEventsCount.Clear();

            if (shouldCollectFiles)
            {
                // по идее должно быть быстрее чем Count считать
                if (columnsToAdd.Any()) {
                    lock(fileColumnNames) {
                        // увеличиваем шанс повторяемости
                        columnsToAdd.Sort(); 
                        foreach (string newColumn in columnsToAdd) {
                            // тут нужна повторная проверка потому что список мог измениться в другом потоке
                            if (!fileColumnNames.Contains(newColumn)) {
                                fileColumnNames.Add(newColumn);
                            }
                        }
                    }
                }
                string filename = Path.Combine(World.SimDataFolder, $"{TribeFileName}.txt");
                Task.Run(() => AddLineToFile(filename, fileRow));                
            }
        }

        private void AddLineToFile(string filename, Dictionary<string, string> fileRow) {
            lock(this) {
                StringBuilder outData = StringBuilderPool.Get();
                foreach (string columnHeader in fileColumnNames)
                {
                    outData.AppendFormat("{0}, ", columnHeader);
                }
                if (!File.Exists(filename)) {
                    outData.AppendLine();
                    File.AppendAllText(filename, outData.ToString());
                } else {
                    string tempFilename = Path.Combine(Path.GetTempPath(), Guid.NewGuid()+".trsimtmp");    
                    bool RowHeaderChanged;
                    using (StreamReader reader = new StreamReader(filename)) {
                        string firstRow = reader.ReadLine();
                        RowHeaderChanged = !firstRow.Equals(outData.ToString());
                        if (RowHeaderChanged) {                                                                                                            
                            using (StreamWriter writer = new StreamWriter(tempFilename)) {
                                writer.WriteLine(outData.ToString());
                                string line;
                                while ((line = reader.ReadLine()) != null) {                                    
                                    writer.WriteLine(line);                                 
                                }
                            }                                                                               
                        }
                    }
                    if (RowHeaderChanged) {
                        File.Copy(tempFilename, filename, true);
                        File.Delete(tempFilename);     
                    }
                }               

                outData.Clear();
                foreach (string column in fileColumnNames) {
                    if (fileRow.ContainsKey(column)) {
                        outData.AppendFormat("{0}, ", fileRow[column]);
                    } else {
                        outData.Append("0, ");
                    }
                }
                outData.AppendLine();
                File.AppendAllText(filename, outData.Release());
            }
        }

        public void ReportCountEvent(string eventName)
        {
            lock (unconsolidatedCountEvents)
            {
                if (!unconsolidatedCountEvents.ContainsKey(eventName))
                {
                    unconsolidatedCountEvents.TryAdd(eventName, 1);
                }
                else
                {
                    unconsolidatedCountEvents[eventName]++;
                }
            }
        }
        public void ReportSumEvent(string eventName, float value)
        {
            lock (unconsolidatedSumEvents)
            {
                if (!unconsolidatedSumEvents.ContainsKey(eventName))
                {
                    unconsolidatedSumEvents.TryAdd(eventName, value);
                }
                else
                {
                    unconsolidatedSumEvents[eventName] += value;
                }
            }
        }
        public void ReportAverageEvent(string eventName, float value)
        {
            lock (unconsolidatedAvgEventsSum)
            {
                if (!unconsolidatedAvgEventsSum.ContainsKey(eventName) || !unconsolidatedAvgEventsCount.ContainsKey(eventName))
                {
                    unconsolidatedAvgEventsSum.TryAdd(eventName, value);
                    unconsolidatedAvgEventsCount.TryAdd(eventName, 1);
                }
                else
                {
                    unconsolidatedAvgEventsSum[eventName] += value;
                    unconsolidatedAvgEventsCount[eventName]++;
                }
            }
        }
        public DataSet GetDataSet(string eventName)
        {
            if (consolidatedData.ContainsKey(eventName))
            {
                return consolidatedData[eventName];
            }
            else
            {
                return null;
            }
        }

        public List<string> GetEventNames()
        {
            return consolidatedData.Keys.ToList();
        }
    }

    public interface IDetaliedEvent
    {
        string Header();
        string Data();
        void Clear();
    }

    public interface IDetaliedData
    {
        bool ReStore(out IDetaliedEvent tevent);
    }

    public class DetaliedData<TEvent> : IDetaliedData where TEvent: struct, IDetaliedEvent
    {
        private ConcurrentQueue<TEvent> dataSets = new ConcurrentQueue<TEvent>();
        public void Store(TEvent tevent)
        {
            dataSets.Enqueue(tevent);
        }
        public bool ReStore(out IDetaliedEvent tevent)
        {
            if (dataSets.TryDequeue(out var t))
            {
                tevent = t;
                return true;
            }

            tevent = default;
            return false;
        }
    }
}
