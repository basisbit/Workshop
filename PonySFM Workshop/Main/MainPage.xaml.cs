﻿using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using CoreLib;
using CoreLib.Impl;

namespace PonySFM_Workshop.Main
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        readonly ConfigFile _configFile;
        MainWindow _window;
        readonly MainWindowPresenter _presenter;
        SFMDirectoryParser _sfmDirParser;
        private readonly RevisionManager _revisionManager;

        public MainPage(MainWindow window, ConfigFile configFile, RevisionManager revisionManager)
        {
            _window = window;
            _configFile = configFile;
            _revisionManager = revisionManager;
            _sfmDirParser = new SFMDirectoryParser(_configFile.SfmDirectoryPath, WindowsFileSystem.Instance);
            _presenter = new MainWindowPresenter(revisionManager);
            _presenter.View = this;
            InitializeComponent();
        }

        private void UninstallButton_Click(object sender, RoutedEventArgs e)
        {
            _presenter.OnUninstall();
            dataGrid.Items.Refresh();
            CheckAllBox.IsChecked = false;
        }

        private void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            _presenter.OnVerify();
            dataGrid.Items.Refresh();
            CheckAllBox.IsChecked = false;
        }

        private void MenuViewOnSite_Click(object sender, RoutedEventArgs e)
        {
            var item = dataGrid.CurrentItem;
            if (item == null)
                return;

            var url = PonySFMAPIConnector.Instance.GetRevisionUrl(((RevisionListItem)item).Revision);
            Process.Start(url);
        }

        public void RefreshListData()
        {
            _presenter.PopulateListData();
        }

        private void dataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var item = dataGrid.CurrentItem;
            if (item == null)
                return;

            MainWindow.Instance.SetPage(new ModPage(_revisionManager, ((RevisionListItem)item).Revision));
        }

        private void CheckAllBox_Checked(object sender, RoutedEventArgs e)
        {
            _presenter.CheckAll(true);
        }

        private void CheckAllBox_Unchecked(object sender, RoutedEventArgs e)
        {
            _presenter.CheckAll(false);
        }
    }
}
