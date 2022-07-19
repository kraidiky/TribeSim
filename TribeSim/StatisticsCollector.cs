using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TribeSim
{
    static class StatisticsCollector
    {
        public const string GLOBAL = "Global";
        private static ConcurrentDictionary<string, TribeDataSet> tribeDataSets = new ConcurrentDictionary<string, TribeDataSet>();
        private static ConcurrentDictionary<Type, IDetaliedData> dataSets = new ConcurrentDictionary<Type, IDetaliedData>();


        public static void ReportEvent<T>(T dValue) where T : struct, IDetaliedEvent
        {
            var type = dValue.GetType();
            var storage = (DetaliedData<T>)dataSets.GetOrAdd(type, t => new DetaliedData<T>());
            storage.Store(dValue);
        }

        private static object _individualSucessLocker = new object();

        public static Type[] detaliedStatisticTypes = new Type[] { typeof(Tribesman.IndividualSucess), typeof(Meme.SucessStatistic) };
        public static Func<string>[] detaliedStatisticFiles = new Func<string>[] {
            () => Path.Combine(World.TribesmanLogFolder, $"IndividualSucess.txt"),
            () => Path.Combine(World.MemesLogFolder, $"MemesSucess.txt"),
        };

        public static void SaveDetaliedStatistic()
        {
            for (int i = 0; i < detaliedStatisticTypes.Length; i++)
            {
                if (dataSets.ContainsKey(detaliedStatisticTypes[i]))
                {
                    string filename = detaliedStatisticFiles[i]();
                    if (dataSets.TryRemove(detaliedStatisticTypes[i], out var queue))
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
        }

        public static TribeDataSet GetOrCreateTribeDataSet(string tribeName){
            if (tribeDataSets.TryGetValue(tribeName, out var tribeDataSet))
                return tribeDataSet;
            if (tribeDataSets.TryAdd(tribeName, tribeDataSet = new TribeDataSet()))
                return tribeDataSet;
            return tribeDataSets[tribeName];
        }
        
        public static void RemoveTribeDataSet(string tribeName)
        {
            tribeDataSets.TryRemove(tribeName, out _);
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
        private static List<string> fileColumnNames;

        static TribeDataSet() {
            fileColumnNames = new List<string>();
            fileColumnNames.Add("Year");
        }

        public void SaveFileFrom(TribeStatistic statistics, string TribeFileName)
        {
            Dictionary<string, string> fileRow = new Dictionary<string, string>();
            fileRow.Add("Year", World.Year.ToString());
            for (int i = 0; i < statistics.elements.Length; i++)
                if (statistics.elements[i] != null)
                {
                    lock (fileColumnNames)
                    {
                        if (!fileColumnNames.Contains(TribeStatistic.eventFullNames[i]))
                            fileColumnNames.Add(TribeStatistic.eventFullNames[i]);
                    }
                    fileRow.Add(TribeStatistic.eventFullNames[i], statistics.elements[i].ValueString);
                }
            string filename = Path.Combine(World.SimDataFolder, $"{TribeFileName}.txt");
            Task.Run(() => AddLineToFile(filename, fileRow));
        }

        public void AppendGraphStatistic(TribeStatistic statistic) {
            for (int i = 0; i < statistic.elements.Length; i++) {
                var element = statistic.elements[i];
                var eventName = TribeStatistic.eventFullNames[i];
                if (element != null) {
                    if (!consolidatedData.ContainsKey(eventName))
                        consolidatedData.TryAdd(eventName, new DataSet(eventName));
                    consolidatedData[eventName].AddPoint(World.Year, element.ValueFloat);
                }
            }
        }

        private string[] formats = new[] {"F0","F1","F2","F3","F4","F5","F6","F7","F8","F9","F10"};
        private string SignificantSigns(float value, int signs)
        {
            int significantSigns = value != 0 ? (int)Math.Ceiling(Math.Log10(Math.Abs(value))) : 3;
            significantSigns = (significantSigns > 0 ? 0 : -significantSigns) + signs;
            significantSigns = significantSigns < formats.Length ? significantSigns : formats.Length - 1;
            return value.ToString(formats[significantSigns], CultureInfo.InvariantCulture);
        }

        private void AddLineToFile(string filename, Dictionary<string, string> fileRow) {
            lock(this) {
                StringBuilder outData = StringBuilderPool.Get();
                lock (fileColumnNames)
                    foreach (string columnHeader in fileColumnNames) {
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
                lock(fileColumnNames) {
                    foreach (string column in fileColumnNames) {
                        if (fileRow.ContainsKey(column)) {
                            outData.AppendFormat("{0}, ", fileRow[column]);
                        } else {
                            outData.Append("0, ");
                        }
                    }
                    outData.AppendLine();
                }
                File.AppendAllText(filename, outData.Release());
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
