
#region Usings
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using UnSealer.Core;
using UnSealer.Core.PluginDiscovery;
using UnSealer.Core.Utils.AsmResolver;
#endregion

namespace UnSealer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow // : Window -?> We Use Wpf Theme :D 
    {
        #region Fields
        public Logger GetLog = null;
        public IList<Protection> ProtectionsAvailable = new List<Protection>();
        #endregion

        #region Initialization
        public MainWindow()
        {
            InitializeComponent();
            GetLog = new Logger(LoggerText);
            new LoadPlugins()
                .Discover(GetLog, ref ProtectionsAvailable);
            //GetLog.Debug("Hi xD"); nvm lol
            foreach (var p in ProtectionsAvailable)
                UserProtectionList.Items.Add(new ComboBoxItem() { Content = p.Name, ToolTip = p.Description });
        }
        #endregion

        #region Load|Dialogs
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog OpenDio = new OpenFileDialog()
            {
                Filter = "Exe Files|*.exe",
                Multiselect = false
            };
            if (OpenDio.ShowDialog() == true)
            {
                AssemblyLocation.Text = OpenDio.FileName; Refs.Items.Clear();
                foreach (var Ref in new GetRefs(AssemblyLocation.Text).CollectRefs())
                    Refs.Items.Add(new ListBoxItem() { Content = Ref });
            }
        }
        private void DiscoverDecMethod_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (AssemblyLocation.Text != string.Empty)
                {
                    Utils.DiscoverMethod(new string[] { AssemblyLocation.Text, DecName.Text, ParamsC.Text }, new Logger(StringLogger), (bool)IsMD.IsChecked, DecMDToken.Text);
                }
                else { MessageBox.Show("Load Assembly First !", "-_-", MessageBoxButton.OK, MessageBoxImage.Error); }
            }));
        }
        #endregion

        #region Misc
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (UserProtectionList.SelectedItem != null)
                {
                    ProtectionsToUse.Items.Add(new ListBoxItem() { Content = UserProtectionList.Text });
                    UserProtectionList.Text = string.Empty;
                }
                else
                {
                    MessageBox.Show("Error Protection Is Empty !", "Error..", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }));
        }
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (ProtectionsToUse.SelectedItem != null)
                {
                    ProtectionsToUse.Items.Remove(ProtectionsToUse.SelectedItem);
                }
                else
                {
                    MessageBox.Show("Error Select Item First !", "Error..", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }));
        }
        #endregion

        #region Execution Process
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (AssemblyLocation.Text == string.Empty) { MessageBox.Show("No Assembly Loaded !", "-_-"); return; }
            Context Context = null;
            List<string> TempProtects = new List<string>();
            TempProtects.Clear();
            /*Application.Current.Dispatcher.BeginInvoke(new Action(() => {*/
            Context = new Context(AssemblyLocation.Text, new Logger[] { GetLog, new Logger(StringLogger) }); Context.Log.Clear();
            foreach (var ListItem in ProtectionsToUse.Items)
                TempProtects.Add((string)((ListBoxItem)ListItem).Content);
            /*}));*/
            new Thread(new ThreadStart(() =>
            {
                /*try
                {*/
                    foreach (var Protection in TempProtects)
                        foreach (var ProtectExecute in ProtectionsAvailable)
                            if (ProtectExecute.Name.Equals(Protection))
                            {
                                Context.Log.Info($"Executing : {ProtectExecute.Name}");
                                ProtectExecute.Execute(Context);
                                Context.Log.Info($"Executed : {ProtectExecute.Name} Successfly !");
                            }
                    Context.SaveContext();
                /*}
                catch (Exception ex)
                {
                    MessageBox.Show($"Error : {ex.Message}", "Uhm -_-", MessageBoxButton.OK, MessageBoxImage.Error);
                }*/
            })).Start();
        }
        #endregion

        #region GithubPageMisc
        private void ImageBlock_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("https://github.com/CursedLand/UnSealer");
        }
        private void StackPanel_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("https://github.com/CursedLand/UnSealer");
        }
        #endregion

        #region Drag n Drop
        private void AssemblyLocation_DragEnter(object sender, DragEventArgs e) => e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        private void AssemblyLocation_PreviewDragOver(object sender, DragEventArgs e) => e.Handled = true;
        private void AssemblyLocation_Drop(object sender, DragEventArgs e)
        {
            try
            {
                Array EventArray = (Array)e.Data.GetData(DataFormats.FileDrop);
                if (EventArray != null)
                {
                    string ArrVal = EventArray.GetValue(0).ToString();
                    int HaveExt = ArrVal.LastIndexOf(".", StringComparison.Ordinal);
                    if (!HaveExt.Equals(-1))
                    {
                        if ((ArrVal.Substring(HaveExt).ToLower()) == ".exe" || (ArrVal.Substring(HaveExt).ToLower()) == ".dll")
                        {
                            AssemblyLocation.Text = ArrVal;
                            Refs.Items.Clear();
                            foreach (var Ref in new GetRefs(AssemblyLocation.Text).CollectRefs())
                                Refs.Items.Add(new ListBoxItem() { Content = Ref });
                        }
                    }
                }
            }
            catch
            {
                // Ignore :D
            }
        }
        #endregion

        #region SECRETREGIONS
        private void IsMN_Checked(object sender, RoutedEventArgs e)
        {
            DecName.IsEnabled = true;
            DecMDToken.IsEnabled = false;
        }
        private void IsMD_Checked(object sender, RoutedEventArgs e)
        {
            DecName.IsEnabled = false;
            DecMDToken.IsEnabled = true;
        }
        private void IsMN_Unchecked(object sender, RoutedEventArgs e)
        {
            DecName.IsEnabled = false;
            DecMDToken.IsEnabled = false;
        }
        private void IsMD_Unchecked(object sender, RoutedEventArgs e)
        {
            DecName.IsEnabled = false;
            DecMDToken.IsEnabled = false;
        }
        #endregion
    }
}