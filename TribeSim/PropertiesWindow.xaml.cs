using System;
using System.Collections;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;

namespace TribeSim
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private class TreeViewModel {
            public TreeViewModel parent;
            public PropertyTree properties;
            public ItemsControl nest;
            public TreeViewItem view;
            public List<TreeViewModel> branches = new List<TreeViewModel>();
            public List<LeafViewModel> leafs = new List<LeafViewModel>();
            public Label background;
            public int changed;

            public void RefreshChildren() {
                branches.ForEach(branch => branch.RefreshChildren());
                leafs.ForEach(leaf => leaf.RefreshChanges());
                RefreshChanges();
                RefreshBackground();
            }
            public void RefreshParent() {
                RefreshChanges();
                RefreshBackground();
                parent?.RefreshParent();
            }
            public void RefreshChanges() {
                changed = branches.Sum(branch => branch.changed) + leafs.Count(leaf => leaf.changed);
            }

            public static SolidColorBrush[] palette = new[] {
                new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                new SolidColorBrush(Color.FromRgb(230, 255, 255)),
            };
            public void RefreshBackground() {
                if (background != null)
                    background.Background = palette[Math.Min(changed, palette.Length - 1)];
            }
        }
        private class LeafViewModel {
            public TreeViewModel parent;
            public PropertyInfo propertyInfo;
            public Panel background;
            public bool changed;

            public void TextChanged(object sender, TextChangedEventArgs e) {
                try {
                    var value = ((TextBox)sender).Text.ConvertToDouble();
                    propertyInfo.SetValue(null, value);
                    ((TextBox)sender).Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                    changed = value != 0;
                    RefreshBackground();
                    parent.RefreshParent();
                } catch (Exception) {
                    ((TextBox)sender).Background = new SolidColorBrush(Color.FromRgb(255, 200, 200));
                }
            }

            public void RefreshChanges() {
                changed = 0 != (double)propertyInfo.GetValue(null);
                RefreshBackground();
            }
            private void RefreshBackground() {
                if (changed)
                    background.Background = new SolidColorBrush(Color.FromRgb(210, 255, 210));
                else
                    background.Background = new SolidColorBrush(Color.FromRgb(220, 220, 220));
            }
        }

        private int tabStopIndex = 10;
        public MainWindow()
        {
            InitializeComponent();
            FillPropertyWindow();
            ReadPersistedExpandedItems();
            rootModel.RefreshChildren();

            var userPrefs = new UserPreferences();

            this.Height = userPrefs.WindowHeight;
            this.Width = userPrefs.WindowWidth;
            this.Top = userPrefs.WindowTop;
            this.Left = userPrefs.WindowLeft;
            this.WindowState = userPrefs.WindowState;

            SetTitle();
        }

        private TreeViewModel rootModel;
        private void FillPropertyWindow() {
            PropertiesTree.Items.Clear();
            rootModel = new TreeViewModel() {
                properties = WorldProperties.PropertyTree,
                nest = PropertiesTree
            };
            CreateTreeViewItemChildren(rootModel);
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

        private TreeViewItem CreateTreeViewItem(TreeViewModel model, string header) {
            var view = new TreeViewItem();
            Label headerLabel = new Label();
            headerLabel.Content = header;
            headerLabel.Margin = new Thickness(0);
            headerLabel.Padding = new Thickness(0, 5, 0, 0);
            headerLabel.MinWidth = 300;
            model.background = headerLabel;
            model.RefreshBackground();

            view.Header = headerLabel;
            view.Tag = header;
            view.Expanded += TreeItem_Expanded;
            view.Collapsed += TreeItem_Collapsed;
            KeyboardNavigation.SetTabNavigation(view, KeyboardNavigationMode.Continue);

            model.view = view;
            model.nest = view;
            return view;
        }

        private void CreateTreeViewItemChildren(TreeViewModel model)
        {
            foreach (var leaf in model.properties.treeLeafs) {
                var child = new LeafViewModel() {
                    parent = model,
                    propertyInfo = leaf.Value
                };
                model.leafs.Add(child);
                model.view.Items.Add(CreateTreeViewItemForProperty(child));
            }

            foreach (var branch in model.properties.treeBranches) {
                var child = new TreeViewModel() {
                    parent = model,
                    properties = branch.Value
                };
                model.branches.Add(child);
                model.nest.Items.Add(CreateTreeViewItem(child, branch.Key));
                CreateTreeViewItemChildren(child);
            }            
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

        private object CreateTreeViewItemForProperty(LeafViewModel model)
        {
            TreeViewItem newItem = new TreeViewItem();
            WrapPanel stack = new WrapPanel();
            stack.MaxWidth = 500;
            stack.Orientation = Orientation.Horizontal;


            DisplayableProperty propertyData = model.propertyInfo.GetCustomAttribute<DisplayableProperty>();

            Label propertyLabel = new Label();
            propertyLabel.Content = propertyData.name;
            propertyLabel.Width = 300;
            propertyLabel.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right;
            propertyLabel.Margin = new Thickness(0, 0, 5, 0);

            TextBox propertyText = new TextBox();
            propertyText.Text = model.propertyInfo.GetValue(null).ToString();
            propertyText.Tag = model.propertyInfo;
            propertyText.Width = 100;
            propertyText.Margin = new Thickness(2);
            propertyText.TextChanged += model.TextChanged;
            propertyText.IsTabStop = true;
            propertyText.TabIndex = tabStopIndex++;

            TextBlock descriptionLabel = new TextBlock();            
            descriptionLabel.Text = propertyData.description;            
            descriptionLabel.Width = 400;
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
            model.background = stack;
            stack.Margin = new Thickness(0);
            newItem.Header = stack;
            KeyboardNavigation.SetTabNavigation(newItem, KeyboardNavigationMode.Continue);
            return newItem;
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
                WorldProperties.ResetProperties();
                WorldProperties.LoadPersistance(dialog.FileName);

                currentOpenedFileName = dialog.SafeFileName;
                if (currentSavedFileName != currentOpenedFileName)
                    currentSavedFileName = "";
                SetTitle();

                Properties.Settings.Default.UsualFolder = System.IO.Path.GetDirectoryName(dialog.FileName);

                suppressExpansionEvents = true;
                FillPropertyWindow();
                ExpandAsPerList();
                suppressExpansionEvents = false;
                rootModel.RefreshChildren();
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
            World.Initialize(Dispatcher, (int)WorldProperties.InitialStateRandomSeed > 0 ? (int)WorldProperties.InitialStateRandomSeed : new Random().Next());
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
