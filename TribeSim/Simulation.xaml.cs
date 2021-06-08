using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TribeSim
{
    /// <summary>
    /// Interaction logic for Simulation.xaml
    /// </summary>
    public partial class Simulation : System.Windows.Window
    {
        private Thread endlessSimulationThread;
        public Simulation()
        {
            InitializeComponent();
            this.DataContext = this;
            endlessSimulationThread = new Thread(new ThreadStart(EndlessSimultaion));
            endlessSimulationThread.Start();
        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            endlessSimulationRunning = false;
            
            endlessSimulationThread.Abort();
            foreach (ChartWindow cw in openCharts)
            {
                cw.Close();
            }
            MainWindow mw = new MainWindow();
            mw.Show();
        }

        private void btnStep_Clicked(object sender, RoutedEventArgs e)
        {
            if (World.IsExtinct)
            {
                return;
            }
            endlessSimulationRunning = false;
            while (stepCalculationInProgress)
            {
                System.Threading.Timer timer = null;
                timer = new System.Threading.Timer((obj) =>
                {
                    btnStep_Clicked(sender, e);
                    timer.Dispose();
                }, null, 300, System.Threading.Timeout.Infinite);
                return;
            }
            stepCalculationInProgress = true;
            World.SimulateYear(Dispatcher);
            stepCalculationInProgress = false;
            Dispatcher.Invoke(new System.Action(delegate()
            {
                lblYear.Content = World.Year.ToString() + "  " + World.spendedTime;
            }));
        }

        private void btnReset_Clicked(object sender, RoutedEventArgs e)
        {
            endlessSimulationRunning = false;
            if (System.Windows.MessageBox.Show("Are you sure you want to reset the simulator?", "Tribe Sim", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                World.Initialize(Dispatcher, (int)WorldProperties.InitialStateRandomSeed >= 0 ? (int)WorldProperties.InitialStateRandomSeed : new Random().Next());
            }
            lblYear.Content = World.Year;
        }

        private bool endlessSimulationRunning = false;
        private bool stepCalculationInProgress = false;

        private void EndlessSimultaion()
        {
            while (true)
            {
                if (World.IsExtinct)
                {
                    endlessSimulationRunning = false;
                    Thread.Sleep(100);
                }
                while (!endlessSimulationRunning)
                {
                    Thread.Sleep(100);                    
                }
                while (stepCalculationInProgress)
                {
                    Thread.Sleep(10);
                }
                stepCalculationInProgress = true;
                World.SimulateYear(Dispatcher);                
                Dispatcher.Invoke(new System.Action(delegate()
                {
                    int restartAfter = Convert.ToInt32(txtRestartSim.Text);
                    if (restartAfter > 0 && World.Year >= restartAfter)
                    {
                        World.CollectFinalState();
                        StatisticsCollector.SaveDetaliedStatistic();
                        World.InitializeNext(Dispatcher);
                    }
                    lblYear.Content = World.Year.ToString() + "  " + World.spendedTime;
                }));

                stepCalculationInProgress = false;
            }
        }

        private void btnRun_Clicked(object sender, RoutedEventArgs e)
        {
            endlessSimulationRunning = !endlessSimulationRunning;
        }

        private void btnStop_Clicked(object sender, RoutedEventArgs e)
        {
            endlessSimulationRunning = false;
        }

        List<ChartWindow> openCharts = new List<ChartWindow>();
        private void btnGraph_Clicked(object sender, RoutedEventArgs e)
        {
            ChartWindow cw = new ChartWindow();
            cw.Show();
            openCharts.Add(cw);
        }

        private string filenameToSave;
        
        PleaseWaitWindow pww;
        

        private int PutTreeLevelToWorksheet(string branchName, PropertyTree tree, Worksheet ws, int curRow, int curCol, int level)
        {
            if (level > 1)
            {
                Range mainHeader = ws.Cells[curRow, curCol];
                mainHeader.Value = branchName;
                mainHeader.Style = "Heading " + ((level < 4) ? level : 4);
                curCol++;
            }
            int rowsUsed = 0;
            foreach (string leafName in tree.treeLeafs.Keys)
            {
                rowsUsed++;
                Range settingHeader = ws.Cells[curRow, 5];
                settingHeader.Value = leafName;
                settingHeader.Style = "Normal";
                Range settingValue= ws.Cells[curRow, 6];
                settingValue.Value = tree.treeLeafs[leafName].GetValue(null).ToString();
                settingValue.Style = "Normal";
                curRow++;                
            }
            foreach (string subBranchName in tree.treeBranches.Keys)
            {
                int subBranchRows = PutTreeLevelToWorksheet(subBranchName, tree.treeBranches[subBranchName], ws, curRow, curCol, level + 1);
                rowsUsed += subBranchRows;
                curRow += subBranchRows;
            }
            if (level > 1)
            {
                Range mainHeader = ws.Range[ws.Cells[curRow-rowsUsed, curCol-1], ws.Cells[curRow-1, curCol-1]];
                mainHeader.Merge();
                mainHeader.Orientation = 90;
                mainHeader.EntireColumn.AutoFit();
                mainHeader.VerticalAlignment = XlVAlign.xlVAlignTop;
                Range entireBox = ws.Range[ws.Cells[curRow - rowsUsed, curCol - 1], ws.Cells[curRow - 1, 6]];
                entireBox.BorderAround(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium, XlColorIndex.xlColorIndexAutomatic);
            }
            return rowsUsed;
        }
    }
}
