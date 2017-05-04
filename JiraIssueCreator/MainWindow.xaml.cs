using System;
using System.Windows;
using System.Xml;

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

            actbProject.Filter = Filter;
        }
        private bool Filter(object obj, string text)
        {
            XmlElement element = (XmlElement)obj;
            string StationName = element.SelectSingleNode("station_name").InnerText.ToLower();
            if (StationName.Contains(text.ToLower()))
                return true;
            return false;
        }

        private void XmlDataProvider_DataChanged(object sender, EventArgs e)
        {
            StatusLabel.Text = "Xml Data Loaded.";
        }}
}
