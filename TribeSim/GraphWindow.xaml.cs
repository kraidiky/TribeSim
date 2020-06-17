using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class ChartWindow : Window
    {       
        
        public ChartWindow()
        {
            InitializeComponent();
        }
        List<int> usedColors = new List<int>();

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            TribeNamesSelector.Items.Clear();
            TribeNamesSelector.Items.Add("Global");
            foreach (string tribeName in StatisticsCollector.GetTribeNames())
            {
                if (tribeName != "Global")
                {
                    TribeNamesSelector.Items.Add(tribeName);
                }
            }
            
        }

        void cb_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var v in ChartArea.Children.ToArray())
            {
                if (v is Microsoft.Research.DynamicDataDisplay.LineGraph)
                {
                    if (((Microsoft.Research.DynamicDataDisplay.LineGraph)v).ToString() == ((CheckBox)sender).Content.ToString())
                    {                      
                        
                        ChartArea.Children.Remove(v);
                    }
                }                
            }
        }

        private void cb_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox me = (CheckBox)sender;            
            ChartArea.AddLineGraph((ObservableDataSource<Point>)(me.Tag), me.Content.ToString());
        }

        private void TribeNamesSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object currentItem = GraphLinesSelector.SelectedValue;
            GraphLinesSelector.Items.Clear();
            if (TribeNamesSelector.SelectedValue == null) return;
            bool previousItemFound=false;
            if (StatisticsCollector.GetEventNames(TribeNamesSelector.SelectedValue.ToString()) == null) return;
            foreach (string lineName in StatisticsCollector.GetEventNames(TribeNamesSelector.SelectedValue.ToString()).ToArray().OrderBy(s => s))
            {
                if (currentItem != null && currentItem.ToString() == lineName) previousItemFound = true;
                GraphLinesSelector.Items.Add(lineName);
            }
            if (previousItemFound)
            {
                GraphLinesSelector.SelectedValue = currentItem;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (TribeNamesSelector.SelectedValue == null) return;
            if (GraphLinesSelector.SelectedValue == null) return;
            foreach (var v in ChartArea.Children.ToArray())
            {
                if (v is Microsoft.Research.DynamicDataDisplay.LineGraph)
                {
                    if (((Microsoft.Research.DynamicDataDisplay.LineGraph)v).ToString() == TribeNamesSelector.SelectedValue.ToString() + ": " + GraphLinesSelector.SelectedValue.ToString())
                    {
                        ChartArea.Children.Remove(v);
                        return;
                    }
                }
            }
            ChartArea.AddLineGraph((ObservableDataSource<Point>)(StatisticsCollector.GetDataSet(TribeNamesSelector.SelectedValue.ToString(), GraphLinesSelector.SelectedValue.ToString()).DataSource), TribeNamesSelector.SelectedValue.ToString() + ": " + GraphLinesSelector.SelectedValue.ToString());
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            if (TribeNamesSelector.SelectedValue == null) return;
            if (GraphLinesSelector.SelectedValue == null) return;
            ChartArea.AddLineGraph((ObservableDataSource<Point>)(StatisticsCollector.GetDataSet(TribeNamesSelector.SelectedValue.ToString(), GraphLinesSelector.SelectedValue.ToString()).DataSource), TribeNamesSelector.SelectedValue.ToString() + ": " + GraphLinesSelector.SelectedValue.ToString());
            foreach (var v in ChartArea.Children.ToArray())
            {
                if (v is Microsoft.Research.DynamicDataDisplay.LineGraph)
                {                    
                    ChartArea.Children.Remove(v);                    
                }
            }
        }
    }
}
