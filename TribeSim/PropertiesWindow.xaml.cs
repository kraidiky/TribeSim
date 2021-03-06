﻿using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using Microsoft.Win32;
using System.IO;
using Path = System.IO.Path;

namespace TribeSim
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int tabStopIndex = 10;
        public MainWindow()
        {
            InitializeComponent();
            PropertyTree properties = WorldProperties.PropertyTree;
            foreach (string name in properties.treeBranches.Keys)
            {
                PropertiesTree.Items.Add(CreateTreeViewItemFor(properties.treeBranches[name], name));                
            }
            ReadPersistedExpandedItems();

            var userPrefs = new UserPreferences();

            this.Height = userPrefs.WindowHeight;
            this.Width = userPrefs.WindowWidth;
            this.Top = userPrefs.WindowTop;
            this.Left = userPrefs.WindowLeft;
            this.WindowState = userPrefs.WindowState;

            SetTitle();
        }

        private void SetTitle() {
            if (!string.IsNullOrEmpty(currentSavedFileName)) {
                this.Title = "saved: " + Path.GetFileName(currentSavedFileName) + " - Simulation Properties";
            } else if (!string.IsNullOrEmpty(currentOpenedFileName)) {
                this.Title = "opened: " + Path.GetFileName(currentOpenedFileName) + " - Simulation Properties";
            } else {
                this.Title = "Simulation Properties";
            }
        }

        private TreeViewItem CreateTreeViewItemFor(PropertyTree properties, string header)
        {            
            TreeViewItem newItem = new TreeViewItem();
            Label headerLabel = new Label();
            headerLabel.Content = header;            
            headerLabel.Margin = new Thickness(0);
            headerLabel.Padding = new Thickness(0, 5, 0, 0);
            headerLabel.MinWidth = 300;
            headerLabel.Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));
            
            
            newItem.Header = headerLabel;
            newItem.Tag = header;
            foreach (string name in properties.treeLeafs.Keys)
            {
                newItem.Items.Add(CreateTreeViewItemForProperty(properties.treeLeafs[name], name));
            }
            foreach (string name in properties.treeBranches.Keys)
            {
                newItem.Items.Add(CreateTreeViewItemFor(properties.treeBranches[name], name));                
            }            
            KeyboardNavigation.SetTabNavigation(newItem, KeyboardNavigationMode.Continue);
            newItem.Expanded += TreeItem_Expanded;
            newItem.Collapsed += TreeItem_Collapsed;
            return newItem;
        }

        private List<string> expandedItems = new List<string>();

        private void TreeItem_Expanded(object sender, RoutedEventArgs e) {
            if (suppressExpansionEvents)
                return;
            TreeViewItem item = (TreeViewItem)sender;
            string pathString = getPathString(item);
            if (!expandedItems.Contains(pathString)) {
                expandedItems.Add(pathString);
                PersistExpandedItems();
            }
        }

        private void PersistExpandedItems() {
            string filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Tribe Sim", "expandedItems.txt");
            File.WriteAllLines(filename, expandedItems);
        }

        private void ReadPersistedExpandedItems() {
            string filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Tribe Sim", "expandedItems.txt");
            if (File.Exists(filename)) {
                expandedItems.Clear();
                expandedItems.AddRange(File.ReadAllLines(filename));
                ExpandAsPerList();
            } else {
                ExpandFirstLevel();
            }
        }


        private void ExpandAsPerList() {
            ExpandItems(PropertiesTree.Items);
        }

        private void ExpandFirstLevel() {
            suppressExpansionEvents = true;
            foreach (TreeViewItem item in PropertiesTree.Items) {
                item.IsExpanded = true;
            }
            suppressExpansionEvents = false;
        }

        private bool suppressExpansionEvents = false;
        private void ExpandItems(ItemCollection items) {
            if (items == null || items.IsEmpty)
                return;
            suppressExpansionEvents = true;
            foreach (TreeViewItem item in items) {
                string pathString = getPathString(item);                
                if (pathString != null && expandedItems.Contains(pathString)) {
                    item.IsExpanded = true;
                    ExpandItems(item.Items);
                }
            }
            suppressExpansionEvents = false;
        }

        private string getPathString(TreeViewItem item) {
            if (item.Tag == null)
                return null;
            string retVal = '[' + item.Tag.ToString() + ']';
            while (item.Parent is TreeViewItem) {
                item = (TreeViewItem)item.Parent;
                retVal = '[' + item.Tag.ToString() + ']' + retVal;
            }
            return retVal;
        }
        

        private void TreeItem_Collapsed(object sender, RoutedEventArgs e) {
            if (suppressExpansionEvents)
                return;
            TreeViewItem item = (TreeViewItem)sender;
            string pathString = getPathString(item);
            if (expandedItems.Contains(pathString)) {
                expandedItems.Remove(pathString);
                PersistExpandedItems();
            }
        }

        private object CreateTreeViewItemForProperty(PropertyInfo propertyInfo, string name)
        {
            TreeViewItem newItem = new TreeViewItem();
            WrapPanel stack = new WrapPanel();
            stack.MaxWidth = 400;
            stack.Orientation = Orientation.Horizontal;

            DisplayableProperty propertyData = propertyInfo.GetCustomAttribute<DisplayableProperty>();

            Label propertyLabel = new Label();
            propertyLabel.Content = propertyData.name;
            propertyLabel.Width = 200;
            propertyLabel.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right;
            propertyLabel.Margin = new Thickness(0, 0, 5, 0);

            TextBox propertyText = new TextBox();
            propertyText.Text = propertyInfo.GetValue(null).ToString();
            propertyText.Tag = propertyInfo;
            propertyText.Width = 100;
            propertyText.Margin = new Thickness(2);
            propertyText.TextChanged += propertyText_TextChanged;
            propertyText.IsTabStop = true;
            propertyText.TabIndex = tabStopIndex++;

            TextBlock descriptionLabel = new TextBlock();            
            descriptionLabel.Text = propertyData.description;            
            descriptionLabel.Width = 300;
            descriptionLabel.TextAlignment = TextAlignment.Justify;
            descriptionLabel.Margin = new Thickness(0, 0, 5, 0);
            descriptionLabel.FontSize = 10;
            descriptionLabel.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            descriptionLabel.TextWrapping = TextWrapping.Wrap;
            descriptionLabel.Padding = new Thickness(3, 0, 3, 10);            

            stack.Children.Add(propertyLabel);
            stack.Children.Add(propertyText);
            if (!String.IsNullOrEmpty(propertyData.description))
            {
                stack.Children.Add(descriptionLabel);
            }
            stack.Background = new SolidColorBrush(Color.FromRgb(220, 220, 220));
            stack.Margin = new Thickness(0);
            newItem.Header = stack;
            KeyboardNavigation.SetTabNavigation(newItem, KeyboardNavigationMode.Continue);
            return newItem;
        }      

        void propertyText_TextChanged(object sender, TextChangedEventArgs e)
        {            
            PropertyInfo property = (PropertyInfo)(((TextBox)sender).Tag);
            try
            {
                property.SetValue(null, ((TextBox)sender).Text.ConvertToDouble());
                ((TextBox)sender).Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            }
            catch (Exception)
            {
                ((TextBox)sender).Background = new SolidColorBrush(Color.FromRgb(255, 200, 200));
            }
        }

        

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var userPrefs = new UserPreferences();

            userPrefs.WindowHeight = this.Height;
            userPrefs.WindowWidth = this.Width;
            userPrefs.WindowTop = this.Top;
            userPrefs.WindowLeft = this.Left;
            userPrefs.WindowState = this.WindowState;

            userPrefs.Save();
        }

        private void MenuItem_OpenClicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();                     
            if (!String.IsNullOrEmpty(Properties.Settings.Default.UsualFolder))
            {
                dialog.InitialDirectory = Properties.Settings.Default.UsualFolder;
            }
            dialog.Multiselect = false;
            dialog.DefaultExt = "trsim";
            dialog.AddExtension = true;
            dialog.Filter = "Tribe Sim Settings files|*.trsim";
            if (dialog.ShowDialog() == true)
            {
                WorldProperties.LoadPersistance(dialog.FileName);

                currentOpenedFileName = dialog.SafeFileName;
                if (currentSavedFileName != currentOpenedFileName)
                    currentSavedFileName = "";
                SetTitle();

                Properties.Settings.Default.UsualFolder = System.IO.Path.GetDirectoryName(dialog.FileName);

                PropertyTree properties = WorldProperties.PropertyTree;
                suppressExpansionEvents = true;
                PropertiesTree.Items.Clear();
                foreach (string name in properties.treeBranches.Keys)
                {
                    PropertiesTree.Items.Add(CreateTreeViewItemFor(properties.treeBranches[name], name));
                }
                ExpandAsPerList();
                suppressExpansionEvents = false;
            }
        }

        private void MenuItem_ExitClicked(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(0);
        }

        private static string currentOpenedFileName = "";
        private static string currentSavedFileName="";
        private void SaveAsClicked(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (!String.IsNullOrEmpty(Properties.Settings.Default.UsualFolder))
            {
                sfd.InitialDirectory = Properties.Settings.Default.UsualFolder;
            }
            sfd.DefaultExt = "trsim";
            sfd.AddExtension = true;
            sfd.Filter = "Tribe Sim Settings files|*.trsim";
            if (sfd.ShowDialog() == true)
            {
                WorldProperties.PersistChanges(sfd.FileName);
                Properties.Settings.Default.UsualFolder = System.IO.Path.GetDirectoryName(sfd.FileName);
                currentOpenedFileName = currentSavedFileName = sfd.SafeFileName;
                SetTitle();
            }
        }

        private void SaveClicked(object sender, RoutedEventArgs e)
        {
            if (currentSavedFileName != "")
            {
                WorldProperties.PersistChanges(currentSavedFileName);
            }
            else
            {
                SaveAsClicked(sender, e);
            }
        }

        private void TestGenerateMeme(object sender, RoutedEventArgs e)
        {
            Random randomizer = new Random();
            StringBuilder sb = new StringBuilder();
            for (int i=1; i<10000; i++)
            {
                AvailableFeatures af = AvailableFeatures.HuntingEfficiency;
                Meme newMeme = Meme.InventNewMeme(i, af);
                sb.AppendFormat("{0}\t{1}\tHunting Efficiency\n", newMeme.Efficiency, newMeme.Price);
            }
            File.WriteAllText("c:\\temp\\memes.csv",sb.ToString());
        }

        private void TestGenerateNames(object sender, RoutedEventArgs e)
        {
            DateTime start = DateTime.Now;
            for (int i = 1; i < 10000001; i++ )
            {
                string name = NamesGenerator.GenerateName();
                
                if (i % 500000 == 0)
                {
                    Console.WriteLine(i + ".\t" + name+"\t"+(DateTime.Now-start).TotalMilliseconds);
                    start = DateTime.Now;
                }
            }
        }

        private void InitiateSimulation(object sender, RoutedEventArgs e)
        {
            World.Initialize(Dispatcher, (int)WorldProperties.InitialStateRandomSeed >= 0 ? (int)WorldProperties.InitialStateRandomSeed : new Random().Next());
            Simulation simWindow = new Simulation();
            simWindow.ShowActivated = true;
            simWindow.Show();
            this.Close();
        }

        private void SetLogFolderClicked(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.SelectedPath = Properties.Settings.Default.LogBaseFolder;
            fbd.ShowNewFolderButton = true;
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Properties.Settings.Default.LogBaseFolder = fbd.SelectedPath;
                World.BaseFolder = fbd.SelectedPath;
            }
        }

        private void TestGenerateExcuses(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine(NamesGenerator.GenerateName() + " didn't go hunting. He told others that " + NamesGenerator.GenerateLameExcuse());
            }
        }

        private void ConsolidateResults(object sender, RoutedEventArgs e)
        {
            ConsolidateFiles consolidateWindow = new ConsolidateFiles();
            consolidateWindow.ShowActivated = true;
            consolidateWindow.Show();
        }
    }
}
