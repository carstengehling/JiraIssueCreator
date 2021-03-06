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

namespace JiraIssueCreator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            string baseUrl = "";
            string username = "";
            string password = "";

            var jiraApiRequestFactory = new JiraApiRequestFactory(
                new RestRequestFactory()
            );

            var jiraClient = new JiraClient(
                jiraApiRequestFactory,
                new JiraApiRequester(
                    new RestClientFactory(),
                    jiraApiRequestFactory
                )
            );




            DataContext = new MainWindowViewModel();
        }
    }
}
