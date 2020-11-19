using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TribeSim
{
    /// <summary>
    /// Interaction logic for ConsolidateFiles.xaml
    /// </summary>
    public partial class ConsolidateFiles : Window
    {
        public ConsolidateFiles()
        {
            InitializeComponent();
        }

        private void BrowseForFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            if (dialog.ShowDialog() == true)
            {
                foreach (string fName in dialog.FileNames)
                {
                    AddFileToTheList(fName);
                }
            }
        }

        private Brush preDropBorderBrush = null;
        private Thickness preDropBorserThickness = new Thickness(0);        
        private void EnterDrag(object sender, DragEventArgs e)
        {
            
            preDropBorderBrush = this.BorderBrush;
            preDropBorserThickness = this.BorderThickness;                        
            this.BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Black);
            this.BorderThickness = new Thickness(3);
        }

        private void LeaveDrag(object sender, DragEventArgs e)
        {            
            if (preDropBorderBrush != null)
            {
                this.BorderBrush = preDropBorderBrush;
            }
            if (preDropBorserThickness != null)
            {
                this.BorderThickness = preDropBorserThickness;
            }
        }

        private void DropFile(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string fName in files)
                {
                    if (File.Exists(fName))
                    {
                        AddFileToTheList(fName);
                    }
                    else
                    {
                        if (Directory.Exists(fName))
                        {
                            TraverseDirectoryAndAddAllGlobalTxts(fName);
                        }
                    }
                }
            }            
            if (preDropBorderBrush != null)
            {
                this.BorderBrush = preDropBorderBrush;
            }
            if (preDropBorserThickness != null)
            {
                this.BorderThickness = preDropBorserThickness;
            }
        }

        private void TraverseDirectoryAndAddAllGlobalTxts(string fName)
        {
            List<string> subdirs = new List<string>(Directory.GetDirectories(fName));
            foreach (string subdir in subdirs)
            {
                TraverseDirectoryAndAddAllGlobalTxts(subdir);
            }
            string probablePathToGlobaltxt = System.IO.Path.Combine(fName, "global.txt");
            if (File.Exists(probablePathToGlobaltxt))
            {
                AddFileToTheList(probablePathToGlobaltxt);
            }
        }

        private void AddFileToTheList(string fName)
        {
            if (!lstFileList.Items.Contains(fName))
            {
                lstFileList.Items.Add(fName);
            }
        }

        private void ClearList(object sender, RoutedEventArgs e)
        {
            lstFileList.Items.Clear();
        }

        private void LstItemsKeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                List<string> objectsToRemove = new List<string>();
                foreach (object o in lstFileList.SelectedItems)
                {
                    objectsToRemove.Add(o.ToString());
                }
                foreach (string str in objectsToRemove)
                {
                    lstFileList.Items.Remove(str);
                }
            }
        }

        private void Consolidate(object sender, RoutedEventArgs e)
        {
            if (lstFileList.Items.Count == 0) return;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = "xlsx";
            if (sfd.ShowDialog()==true)
            {
                string filename = sfd.FileName;

                Dictionary<string, Dictionary<string, Dictionary<string, string>>> MetricValuesByYearByMetricByFile = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
                    int index = 1;
                    foreach (object oFileName in lstFileList.Items)
                    {
                        string FileTitle = "Run " + index++;
                        MetricValuesByYearByMetricByFile.Add(FileTitle, ProcessFile(oFileName.ToString()));
                    }

                    Dictionary<string, Dictionary<string, Dictionary<string, string>>> MetricValuesByFileByYearByMetric = GetMetricValuesByFileByYearByMetric(MetricValuesByYearByMetricByFile);
                   

                using (SpreadsheetDocument document = SpreadsheetDocument.Create(filename, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
                {
                    WorkbookPart workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet();

                    Sheets sheets = document.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

                    uint MetricID = 1;
                    foreach (string metric in MetricValuesByFileByYearByMetric.Keys)
                    {
                        WorksheetPart worksheetPart1 = workbookPart.AddNewPart<WorksheetPart>();
                        Worksheet worksheet1 = new Worksheet();
                        SheetData sheetData1 = new SheetData();

                        // the data for sheet 1
                        Row headerRow = new Row();
                        headerRow.Append(ConstructCell("Year",CellValues.String));
                        List<string> runFiles = new List<string>();
                        foreach (string file in MetricValuesByYearByMetricByFile.Keys)
                        {
                            headerRow.Append(ConstructCell(file, CellValues.String));
                            runFiles.Add(file);
                        }
                        headerRow.Append(ConstructCell("Average", CellValues.String));
                        sheetData1.Append(headerRow);

                        foreach (string year in MetricValuesByFileByYearByMetric[metric].Keys)
                        {
                            Row yearRow = new Row();
                            yearRow.Append(ConstructCell(year, CellValues.Number));
                            sheetData1.Append(yearRow);

                            double sum = 0;
                            int num = 0;
                            foreach (string filecursor in runFiles)
                            {
                                if (MetricValuesByFileByYearByMetric[metric][year].ContainsKey(filecursor) && !String.IsNullOrWhiteSpace(MetricValuesByFileByYearByMetric[metric][year][filecursor]))
                                {
                                    yearRow.Append(ConstructCell(MetricValuesByFileByYearByMetric[metric][year][filecursor], CellValues.Number));
                                    num++;
                                    sum += MetricValuesByFileByYearByMetric[metric][year][filecursor].ConvertToDouble();
                                }
                                else
                                {
                                    yearRow.Append(ConstructCell("", CellValues.Number));
                                }
                            }
                            if (num>0)
                            {
                                yearRow.Append(ConstructCell((sum / num).ToString(), CellValues.Number));
                            }
                        }

                        

                        worksheet1.AppendChild(sheetData1);
                        worksheetPart1.Worksheet = worksheet1;

                        Sheet sheet1 = new Sheet()
                        {
                            Id = document.WorkbookPart.GetIdOfPart(worksheetPart1),
                            SheetId = MetricID,
                            Name = metric
                        };
                        MetricID++;
                        sheets.Append(sheet1);

                        worksheetPart1.Worksheet.Save();                        
                    }
                    // End: Code block for Excel sheet 1


                    





                    /*foreach (string metricName in MetricValuesByFileByYearByMetric.Keys)
                      {
                          string relId = workbookPart.Workbook.Descendants<Sheet>().First(s => metricName.Equals(s.Name)).Id;
                          WorksheetPart wsp = (WorksheetPart)workbookPart.GetPartById(relId);

                          SheetData sheetData = wsp.Worksheet.AppendChild(new SheetData());

                          Row headRow = new Row();

                          headRow.Append(
                              ConstructCell("Year", CellValues.String));

                          foreach (string file in MetricValuesByYearByMetricByFile.Keys)
                          {
                              headRow.Append(ConstructCell(file, CellValues.String));
                          }


                          sheetData.AppendChild(headRow);
                          worksheetPart.Worksheet.Save();
                      }*/



                    //workbookPart.Workbook.Save();


                }

                System.Diagnostics.Process.Start(filename);
            }
        }

        private Cell ConstructCell(string value, CellValues dataType)
        {
            return new Cell()
            {
                CellValue = new CellValue(value),
                DataType = new EnumValue<CellValues>(dataType)               
            };
        }

        private Dictionary<string, Dictionary<string, Dictionary<string, string>>> GetMetricValuesByFileByYearByMetric(Dictionary<string, Dictionary<string, Dictionary<string, string>>> metricValuesByYearByMetricByFile)
        {
            Dictionary<string, Dictionary<string, Dictionary<string, string>>> MetricValuesByFileByYearByMetric = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
            
            foreach (string file in metricValuesByYearByMetricByFile.Keys)
            {
                foreach (string metric in metricValuesByYearByMetricByFile[file].Keys)
                {
                    foreach (string year in metricValuesByYearByMetricByFile[file][metric].Keys)
                    {
                        string value = metricValuesByYearByMetricByFile[file][metric][year];
                        if (!MetricValuesByFileByYearByMetric.ContainsKey(metric))
                        {
                            MetricValuesByFileByYearByMetric.Add(metric, new Dictionary<string, Dictionary<string, string>>());
                        }
                        if (!MetricValuesByFileByYearByMetric[metric].ContainsKey(year))
                        {
                            MetricValuesByFileByYearByMetric[metric].Add(year, new Dictionary<string, string>());
                        }
                        if (!MetricValuesByFileByYearByMetric[metric][year].ContainsKey(file))
                        {
                            MetricValuesByFileByYearByMetric[metric][year].Add(file, value);
                        }
                        else
                        {
                            throw new Exception("Something is wrong with the file set");
                        }
                    }
                }
            }

            return MetricValuesByFileByYearByMetric;
        }

        private void AddMetricsToSheets(WorkbookPart workbookPart, WorksheetPart worksheetPart, Sheets sheets, ItemCollection files)
        {
            List<string> MetricList = new List<string>();
            foreach (object oFileName in files)
            {
                string fname = oFileName.ToString();
                string linenames = File.ReadAllLines(fname)[0];
                string[] names = linenames.Split(',');
                foreach (string metric in names)
                {
                    if (metric.ToLower().Trim() != "year" && !string.IsNullOrWhiteSpace(metric))
                    {
                        if (!MetricList.Contains(metric))
                        {
                            MetricList.Add(metric);
                        }
                    }
                }
                uint index = 1;
                foreach (string metric in MetricList)
                {
                    string bmetric = BeautifyMetricName(metric);
                    Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = index++, Name = bmetric };
                    sheets.Append(sheet);
                }
            }
        }

        private string BeautifyMetricName(string metric)
        {
            return metric.Trim().Replace("Avg. phenotype value", "APhV").Replace("Avg. genotype value", "AGeV").Replace("punishment likelyhood", "punish chance").Replace("determintaion efficiency", "locate chance").Replace("% memory:", "%mem").Replace("FreeRider", "F.R. ").Replace("DeterminationEfficiency", "Det.Eff.");

        }

        private Random rnd = new Random();
        private Dictionary<string, Dictionary<string, string>> ProcessFile(string filename)
        {
            string[] lines = File.ReadAllLines(filename);
            string[] lineNames = lines[0].Split(',');
            Dictionary<string, Dictionary<string, string>> MetricValuesByYearByMetric = new Dictionary<string, Dictionary<string, string>>();
            Dictionary<string, int> MetricNameIndices = new Dictionary<string, int>();
            
            for (int index=0; index<lineNames.Length; index++)
            {
                string metricName = lineNames[index];
                string bMetricName = BeautifyMetricName(metricName);
                if (metricName.Trim().ToLower()!="year" && !string.IsNullOrWhiteSpace(metricName))
                {      
                    if (!MetricValuesByYearByMetric.ContainsKey(bMetricName))
                    {
                        MetricValuesByYearByMetric.Add(bMetricName, new Dictionary<string, string>());                        
                    }                    
                }
                MetricNameIndices.Add(bMetricName, index);
            }
            bool skipped = false;
            foreach (string line in lines)
            {
                if (!skipped)
                {
                    skipped = true;
                    continue;
                }
                string[] values = line.Split(',');
                string year = values[MetricNameIndices["Year"]];
                for (int i=0; i< values.Length;i++)
                {
                    string bMetricName = BeautifyMetricName(lineNames[i]);
                    if (MetricValuesByYearByMetric.ContainsKey(bMetricName))
                    {
                        if (!MetricValuesByYearByMetric[bMetricName].ContainsKey(year))
                        {
                            MetricValuesByYearByMetric[bMetricName].Add(year, values[i]);
                        }
                    }
                }
            }

            /*foreach (string metric in MetricValuesByYearByMetric.Keys)
            {
                string relId = workbookPart.Workbook.Descendants<Sheet>().First(s => metric.Equals(s.Name)).Id;
                WorksheetPart wsp = (WorksheetPart)workbookPart.GetPartById(relId);
                Cell cYearCell = GetCell(wsp.Worksheet, "A", 1);
                cYearCell.CellValue = new CellValue("Year");
                cYearCell.DataType = new EnumValue<CellValues>(CellValues.String);
                string filesColumn = GetExcelColumnNameFromNumber(fileIndex + 1);
                Cell cFileHeaderCell = GetCell(wsp.Worksheet, filesColumn, 1);
                cFileHeaderCell.CellValue = new CellValue("Run " + fileIndex);
                cFileHeaderCell.DataType = new EnumValue<CellValues>(CellValues.String);
            }*/

            return MetricValuesByYearByMetric;
        }

        public static string GetExcelColumnNameFromNumber(int num)
        {
            num--;
            string retval = "";
            string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            int weight = alphabet.Length;
            do
            {
                retval = alphabet[num % weight] + retval;
                num = (int)Math.Truncate((double)num / weight);
            }
            while (num > 0);
            return retval;
        }

        // Given a worksheet, a column name, and a row index, 
        // gets the cell at the specified column and 
        private static Cell GetCell(Worksheet worksheet,
                  string columnName, uint rowIndex)
        {
            Row row = GetRow(worksheet, rowIndex);

            if (row == null)
                return null;

            return row.Elements<Cell>().Where(c => string.Compare
                   (c.CellReference.Value, columnName +
                   rowIndex, true) == 0).First();
        }


        // Given a worksheet and a row index, return the row.
        private static Row GetRow(Worksheet worksheet, uint rowIndex)
        {
            return worksheet.GetFirstChild<SheetData>().
              Elements<Row>().Where(r => r.RowIndex == rowIndex).First();
        }
    }
}
