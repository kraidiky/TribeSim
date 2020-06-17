using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TribeSim
{
    static class StatisticsCollector
    {
        private static ConcurrentDictionary<string, TribeDataSet> tribeDataSets = new ConcurrentDictionary<string, TribeDataSet>();

        public static void ReportCountEvent(string tribeName, string eventName)
        {
            if (!tribeDataSets.ContainsKey(tribeName))
            {
                tribeDataSets.TryAdd(tribeName, new TribeDataSet());
            }
            tribeDataSets[tribeName].ReportCountEvent(eventName);
            if (tribeName != "Global")
            {
                ReportCountEvent("Global", eventName);
            }
        }
        public static void ReportSumEvent(string tribeName, string eventName, double dValue)
        {
            float value = (float)dValue;
            if (!tribeDataSets.ContainsKey(tribeName))
            {
                tribeDataSets.TryAdd(tribeName, new TribeDataSet());
            }
            tribeDataSets[tribeName].ReportSumEvent(eventName, value);
            if (tribeName != "Global")
            {
                ReportSumEvent("Global", eventName, value);
            }
        }
        public static void ReportAverageEvent(string tribeName, string eventName, double dValue)
        {
            float value = (float)dValue;
            if (!tribeDataSets.ContainsKey(tribeName))
            {
                tribeDataSets.TryAdd(tribeName, new TribeDataSet());
            }
            tribeDataSets[tribeName].ReportAverageEvent(eventName, value);
            if (tribeName != "Global")
            {
                ReportAverageEvent("Global", eventName, value);
            }
        }        

        public static void ConsolidateNewYear()
        {
            List<string> toRemove = new List<string>();
            foreach (KeyValuePair<string, TribeDataSet> kvp in tribeDataSets)
            {
                if (World.TribeExists(kvp.Key) || kvp.Key == "Global")
                {
                    kvp.Value.ConsolidateNewYear(kvp.Key);
                }
                else if (WorldProperties.CollectFilesData>0.5)
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
        private Dictionary<string, string> fileRow = new Dictionary<string, string>();

        public void ConsolidateNewYear(string TribeName)
        {
            bool RowHeaderChanged = false;
            bool shouldCollectFiles = WorldProperties.CollectFilesData > 0.5 && World.Year % ((int)Math.Round(WorldProperties.CollectFilesData)) == 0;

            if (shouldCollectFiles)
            {
                if (!fileRow.ContainsKey("Year"))
                {
                    fileRow.Add("Year", World.Year.ToString());
                    RowHeaderChanged = true;
                }
                else
                {
                    fileRow["Year"] = World.Year.ToString();
                }
            }
            foreach (KeyValuePair<string, int> kvp in unconsolidatedCountEvents)
            {
                if (WorldProperties.CollectGraphData > 0.5)
                {
                    if (!consolidatedData.ContainsKey(kvp.Key))
                    {
                        consolidatedData.TryAdd(kvp.Key, new DataSet(kvp.Key));
                    }
                    consolidatedData[kvp.Key].AddPoint(World.Year, kvp.Value);
                }
                if (shouldCollectFiles)
                {
                    if (!fileRow.ContainsKey(kvp.Key))
                    {
                        fileRow.Add(kvp.Key, kvp.Value.ToString());
                        RowHeaderChanged = true;
                    }
                    else
                    {
                        fileRow[kvp.Key] = kvp.Value.ToString();
                    }
                }
                unconsolidatedCountEvents[kvp.Key] = 0;
            }

            foreach (KeyValuePair<string, float> kvp in unconsolidatedSumEvents)
            {
                if (WorldProperties.CollectGraphData > 0.5)
                {
                    if (!consolidatedData.ContainsKey(kvp.Key))
                    {
                        consolidatedData.TryAdd(kvp.Key, new DataSet(kvp.Key));
                    }
                    consolidatedData[kvp.Key].AddPoint(World.Year, kvp.Value);
                }
                if (shouldCollectFiles)
                {
                    if (!fileRow.ContainsKey(kvp.Key))
                    {
                        fileRow.Add(kvp.Key, kvp.Value.ToString("F3"));
                        RowHeaderChanged = true;
                    }
                    else
                    {
                        fileRow[kvp.Key] = kvp.Value.ToString("F3");
                    }
                }
            }
            unconsolidatedSumEvents.Clear();
            foreach (KeyValuePair<string, float> kvp in unconsolidatedAvgEventsSum)
            {
                if (WorldProperties.CollectGraphData > 0.5)
                {
                    if (!consolidatedData.ContainsKey(kvp.Key))
                    {
                        consolidatedData.TryAdd(kvp.Key, new DataSet(kvp.Key));
                    }
                    if (unconsolidatedAvgEventsCount[kvp.Key] != 0)
                    {
                        consolidatedData[kvp.Key].AddPoint(World.Year, kvp.Value / unconsolidatedAvgEventsCount[kvp.Key]);
                    }
                }
                if (shouldCollectFiles)
                {
                    if (!fileRow.ContainsKey(kvp.Key))
                    {
                        if (unconsolidatedAvgEventsCount[kvp.Key] != 0)
                        {
                            fileRow.Add(kvp.Key, (kvp.Value / unconsolidatedAvgEventsCount[kvp.Key]).ToString("F3"));
                        }
                        else
                        {
                            fileRow.Add(kvp.Key, "0");
                        }
                        RowHeaderChanged = true;
                    }
                    else
                    {
                        if (unconsolidatedAvgEventsCount[kvp.Key] != 0)
                        {
                            fileRow[kvp.Key] = (kvp.Value / unconsolidatedAvgEventsCount[kvp.Key]).ToString("F3");
                        }
                        else
                        {
                            fileRow[kvp.Key] = "0";
                        }
                    }
                }
            }
            unconsolidatedAvgEventsSum.Clear();
            unconsolidatedAvgEventsCount.Clear();

            if (shouldCollectFiles)
            {
                string filename = Path.Combine(World.SimDataFolder, TribeName + ".txt");
                StringBuilder outData = new StringBuilder();
                foreach (KeyValuePair<string, string> kvp in fileRow)
                {
                    outData.AppendFormat("{0}, ", kvp.Key);
                }
                if (RowHeaderChanged)
                {
                    if (File.Exists(filename))
                    {
                        bool firstRow = true;
                        string tempFilename = Path.Combine(Path.GetTempPath(), Guid.NewGuid()+".trsimtmp");
                        using (StreamReader reader = new StreamReader(filename))
                        {
                            using (StreamWriter writer = new StreamWriter(tempFilename))
                            {
                                writer.WriteLine(outData.ToString());
                                string line;
                                while ((line = reader.ReadLine()) != null)
                                {
                                    if (firstRow)
                                    {
                                        firstRow = false;
                                    }
                                    else
                                    {
                                        writer.WriteLine(line);
                                    }
                                }
                            }
                        }
                        File.Copy(tempFilename, filename, true);
                        File.Delete(tempFilename);
                    }
                    else
                    {
                        outData.AppendLine();
                        File.AppendAllText(filename, outData.ToString());
                    }
                }

                outData.Clear();
                foreach (KeyValuePair<string, string> kvp in fileRow)
                {
                    outData.AppendFormat("{0}, ", kvp.Value);
                }
                outData.AppendLine();
                File.AppendAllText(filename, outData.ToString());
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
}
