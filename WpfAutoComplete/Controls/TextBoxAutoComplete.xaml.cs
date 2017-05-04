using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace WpfAutoComplete.Controls
{
    /// <summary>
    /// Interaction logic for TextBoxAutoCompleteProvider.xaml
    /// </summary>
    public partial class TextBoxAutoComplete : UserControl, INotifyPropertyChanged
    {
        /// <summary>
        /// The target control is the textbox which this extender wil be supporting.
        /// </summary>
        public TextBox TargetControl
        {
            get { return (TextBox)GetValue(TargetControlProperty); }
            set { SetValue(TargetControlProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TargetControl.
        public static readonly DependencyProperty TargetControlProperty =
            DependencyProperty.Register("TargetControl",
                                        typeof(TextBox),
                                        typeof(TextBoxAutoComplete),
                                        new UIPropertyMetadata(null,
                                                    new PropertyChangedCallback(TargetControl_Changed)),
                                                    new ValidateValueCallback(TargetControl_Validate)
                                        );

        /// <summary>
        /// Validate that the target is a textbox control
        /// </summary>
        /// <param name="value"></param>
        /// <returns>True if TargetControl is a textbox</returns>
        private static bool TargetControl_Validate(object value)
        {
            TextBox newv = value as TextBox;
            if (newv == null && value != null) return false;
            return true;
        }

        /// <summary>
        /// When we assign the target control we set up event handlers.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void TargetControl_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                TextBoxAutoComplete me = d as TextBoxAutoComplete;
                TextBox oldv = e.OldValue as TextBox;
                TextBox newv = e.NewValue as TextBox;
                if (oldv != null)
                {
                    oldv.LostFocus -= new RoutedEventHandler(me.TargetControl_LostFocus);
                    oldv.GotFocus -= new RoutedEventHandler(me.TargetControl_GotFocus);
                    oldv.KeyUp -= new KeyEventHandler(me.TargetControl_KeyUp);
                    oldv.PreviewKeyUp -= new KeyEventHandler(me.TargetControl_PreviewKeyUp);
                    oldv.PreviewKeyDown -= new KeyEventHandler(me.TargetControl_PreviewKeyDown);
                }
                if (newv != null)
                {
                    me.popup.PlacementTarget = newv;
                    newv.LostFocus += new RoutedEventHandler(me.TargetControl_LostFocus);
                    newv.GotFocus += new RoutedEventHandler(me.TargetControl_GotFocus);
                    newv.KeyUp += new KeyEventHandler(me.TargetControl_KeyUp);
                    newv.PreviewKeyUp += new KeyEventHandler(me.TargetControl_PreviewKeyUp);
                    newv.PreviewKeyDown += new KeyEventHandler(me.TargetControl_PreviewKeyDown);
                }
            }
        }

        private void TargetControl_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!this.popup.IsKeyboardFocusWithin)
                this.popup.IsOpen = false;
            this.IsBussy = false;
        }

        private void TargetControl_GotFocus(object sender, RoutedEventArgs e)
        {
            
            if (SelTextOnFocus) this.txtSearch.SelectAll();
            if (itemsSelected)
                itemsSelected = false;
           // else if  this.popup.IsOpen = true;
        }

        private void TargetControl_KeyUp(object sender, KeyEventArgs e)
        {            
            if (e.Key == Key.Up)
                {
                    if (this.listBox.SelectedIndex > 0)
                    {
                        this.listBox.SelectedIndex--;
                    }
                }
                if (e.Key == Key.Down)
                {
                    if (this.listBox.SelectedIndex < this.listBox.Items.Count - 1)
                    {
                        this.listBox.SelectedIndex++;
                    }
                }
                if (e.Key == Key.Enter)
                {
                    if (this.popup.IsOpen && this.listBox.Items.Count > 0)
                        SetTextAndHide();
                    else if (this.SearchText.Length < _parialSearchTextLength)
                        this.TextBoxEnterCommand.Execute(null);
                }
        }

        private void TargetControl_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.popup.IsOpen = false;
                this.listBox.SelectedItem = null;
                e.Handled = true;
            }
            if (IsTextChangingKey(e.Key))
            {
                Suggest();                
            }
        }

        private void TargetControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab && this.listBox.SelectedItem != null)
            {
                this.popup.IsOpen = false;
                TargetControl.Text = String.IsNullOrEmpty(DisplayMemberPath) ?
                                     this.listBox.SelectedItem.ToString() :
                                     this.listBox.SelectedItem.GetType().GetProperty(
                                     DisplayMemberPath).GetValue(this.listBox.SelectedItem, null).ToString();
                if (MovesFocus)
                    TargetControl.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                e.Handled = true;
            }
        }

        private bool IsTextChangingKey(Key key)
        {
            if (key == Key.Back || key == Key.Delete)
            {
                if (TargetControl.Text == "")
                {
                    this.listBox.SelectedItem = null;
                    this.popup.IsOpen = false;
                }
                return true;
            }
            else
            {
                KeyConverter conv = new KeyConverter();
                string keyString = (string)conv.ConvertTo(key, typeof(string));                

                return keyString.Length == 1;
            }
        }

        private void Suggest()
        {
                this.SearchText = TargetControl.Text;
                if (odp != null) this.IsBussy = true;
        }       

        /// <summary>
        /// Do we move focus to the next control, when we 
        /// have selected an item from the dropdown list?
        /// </summary>
        public bool MovesFocus
        {
            get { return (bool)GetValue(MovesFocusProperty); }
            set { SetValue(MovesFocusProperty, value); }
        }

        public static readonly DependencyProperty MovesFocusProperty =
            DependencyProperty.Register("MovesFocus", typeof(bool), typeof(TextBoxAutoComplete), new UIPropertyMetadata(true));

        /// <summary>
        /// Seleccionar todo el texto cuando el control recibe el foco        
        /// </summary>
        public bool SelTextOnFocus
        {
            get { return (bool)GetValue(SelTextOnFocusProperty); }
            set { SetValue(SelTextOnFocusProperty, value); }
        }

        public static readonly DependencyProperty SelTextOnFocusProperty =
            DependencyProperty.Register("SelTextOnFocus", typeof(bool), typeof(TextBoxAutoComplete), new UIPropertyMetadata(true));

        /// <summary>
        /// Indica que el control esta en el ciclo de busqueda de datos
        /// </summary>
        public bool IsBussy
        {
            get { return (bool)GetValue(IsBussyProperty); }
            set { SetValue(IsBussyProperty, value); }
        }

        public static readonly DependencyProperty IsBussyProperty =
            DependencyProperty.Register("IsBussy", typeof(bool), typeof(TextBoxAutoComplete), new UIPropertyMetadata(false));

        private static Int32 _parialSearchTextLength = 2;

        public TextBoxAutoComplete()
        {
            InitializeComponent();
            this.grid.DataContext = this;

            this.TargetControl = this.txtSearch;

            // ListBox inside the Popup
            this.listBox.SelectionChanged += new SelectionChangedEventHandler(listBox_SelectionChanged);
            this.listBox.SelectionMode = SelectionMode.Single;
            this.itemsSelected = false;            

            // Setup the command for the enter key on the textbox
            textBoxEnterCommand = new ReactiveRelayCommand(obj => { });

            // Listen to all property change events on SearchText
            var searchTextChanged = Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                ev => PropertyChanged += ev,
                ev => PropertyChanged -= ev
                )
                .Where(ev => ev.EventArgs.PropertyName == "SearchText");

            // Transform the event stream into a stream of strings (the input values)
            var input = searchTextChanged
                .Where(ev => SearchText == null || SearchText.Length < _parialSearchTextLength)
                .Throttle(TimeSpan.FromSeconds(2))
                .Merge(searchTextChanged
                    .Where(ev => SearchText != null && SearchText.Length >= _parialSearchTextLength)
                    .Throttle(TimeSpan.FromMilliseconds(400)))
                .Select(args => SearchText)
                .Merge(
                    textBoxEnterCommand.Executed.Select(e => SearchText))
                .DistinctUntilChanged();

            // Setup an Observer for the search operation
            var search = Observable.ToAsync<string, SearchResult>(DoSearch);

            // Chain the input event stream and the search stream, cancelling searches when input is received
            var results = from searchTerm in input
                          from result in search(searchTerm).TakeUntil(input)
                          select result;

            // Log the search result and add the results to the results collection
            results.ObserveOn(DispatcherScheduler.Current)
                .Subscribe(
                    result =>
                    {                        
                        searchResults.Clear();
                        //LogOutput.Insert(0, string.Format("Search for '{0}' returned '{1}' items", result.SearchTerm, result.Results.Count()));
                        if (result.Results == null) return;
                        result.Results.ToList().ForEach(item => searchResults.Add(item));
                        if (searchResults.Count == 1)
                        {
                            this.listBox.SelectedItem = listBox.Items[0];
                            SetTextAndHide();
                        }
                        //this.listBox.SelectedIndex = 0;
                        else
                        {
                            this.popup.VerticalOffset = TargetControl.ActualHeight;
                            this.popup.IsOpen = true;
                            this.IsBussy = false;
                        }
                    },
                    ex => {
                        string msg = string.Format("Exception {0} in OnError handler\nException.Message : {1}", ex.GetType().ToString(), ex.Message);
                        MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error); 
                    }
                );
        }

        public string DisplayMemberPath
        {
            get { return this.listBox.DisplayMemberPath; }
            set { this.listBox.DisplayMemberPath = value; }
        }

        public string SelectedValuePath
        {
            get { return this.listBox.SelectedValuePath; }
            set { this.listBox.SelectedValuePath = value; }
        }

        public DataTemplate ItemTemplate
        {
            get { return this.listBox.ItemTemplate; }
            set { this.listBox.ItemTemplate = value; }
        }

        public ItemsPanelTemplate ItemsPanel
        {
            get { return this.listBox.ItemsPanel; }
            set { this.listBox.ItemsPanel = value; }
        }

        public DataTemplateSelector ItemTemplateSelector
        {
            get { return this.listBox.ItemTemplateSelector; }
            set { this.listBox.ItemTemplateSelector = value; }
        }

        public Int32 SelectedListBoxIndex
        {
            get { return (Int32)GetValue(SelectedListBoxIndexProperty); }
            set { SetValue(SelectedListBoxIndexProperty, value); }
        }

        public static readonly DependencyProperty SelectedListBoxIndexProperty = DependencyProperty.Register("SelectedListBoxIndex", typeof(Int32), typeof(TextBoxAutoComplete), new UIPropertyMetadata(0));

        public object SelectedListBoxItem
        {
            get { return (object)GetValue(SelectedListBoxItemProperty); }
            set { SetValue(SelectedListBoxItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedListBoxItemProperty = 
            DependencyProperty.Register("SelectedListBoxItem", typeof(object), typeof(TextBoxAutoComplete), new UIPropertyMetadata(null));                


        public object SelectedListBoxValue
        {
            get { return (object)GetValue(SelectedListBoxValueProperty); }
            set { SetValue(SelectedListBoxValueProperty, value); }
        }

        public static readonly DependencyProperty SelectedListBoxValueProperty = 
            DependencyProperty.Register("SelectedListBoxValue", typeof(object), typeof(TextBoxAutoComplete), new UIPropertyMetadata(null, new PropertyChangedCallback(SelectedListBoxValue_Changed)));

        private static void SelectedListBoxValue_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBoxAutoComplete me = d as TextBoxAutoComplete;            
            if (e.NewValue != null)
            {
                if (!me.popup.IsOpen)
                {                 
                    var result = me.odp.SearchByKey(e.NewValue);
                    result.Results.ToList().ForEach(item => me.searchResults.Add(item));
                    me.SetTextAndHide();
                }
                else
                {                 
                    me.TargetControl.Text = String.IsNullOrEmpty(me.DisplayMemberPath) ?
                                     me.listBox.SelectedItem.ToString() :
                                     me.listBox.SelectedItem.GetType().GetProperty(
                                        me.DisplayMemberPath).GetValue(me.listBox.SelectedItem, null).ToString();
                }
                
            }
        }



        /// <summary>
        /// Set the width of the Popup
        /// </summary>
        public Int32 PopupWidth
        {
            get { return (Int32)GetValue(PopupWidthProperty); }
            set { SetValue(PopupWidthProperty, value); }
        }

        public static readonly DependencyProperty PopupWidthProperty = DependencyProperty.Register("PopupWidth", typeof(Int32), typeof(TextBoxAutoComplete), new UIPropertyMetadata(0));

        public PopupAnimation PopupAnimation
        {
            get { return this.popup.PopupAnimation; }
            set { this.popup.PopupAnimation = value; }
        }

        private string _searchText;
        /// <summary>
        /// The search text is populated from the filtered
        /// keystrokes observed in the target control.
        /// </summary>
        public string SearchText
        {
            get { return this._searchText; }
            set
            {
                //if (this._searchText != value)
                //{
                    this._searchText = value;
                    this.NotifyPropertyChanged("SearchText");
                //}
            }
        }

        private ReactiveRelayCommand textBoxEnterCommand;
        /// <summary>
        /// If the operator presses the enter key
        /// we immediately do a search.
        /// </summary>
        public ReactiveRelayCommand TextBoxEnterCommand
        {
            get { return textBoxEnterCommand; }
            set { textBoxEnterCommand = value; }
        }


        private bool itemsSelected;

        void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NotifyPropertyChanged("SelectedItem");
            NotifyPropertyChanged("SelectedValue");
            RaiseSelectionChangedEvent();

            if (this.popup.IsKeyboardFocusWithin)
            {
                SetTextAndHide();
            }
        }

        public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent("SelectionChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TextBoxAutoComplete));

        // Provide CLR accessors for the event
        public event RoutedEventHandler SelectionChanged
        {
            add { AddHandler(SelectionChangedEvent, value); }
            remove { RemoveHandler(SelectionChangedEvent, value); }
        }

        private void RaiseSelectionChangedEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(TextBoxAutoComplete.SelectionChangedEvent);
            RaiseEvent(newEventArgs);
        }

        protected void OnSelectionChanged()
        {
            RaiseSelectionChangedEvent();
        }

        private void SetTextAndHide()
        {
            this.popup.IsOpen = false;
            if (this.listBox.SelectedItem != null)
            {
                TargetControl.Text = String.IsNullOrEmpty(DisplayMemberPath) ?
                                     this.listBox.SelectedItem.ToString() :
                                     this.listBox.SelectedItem.GetType().GetProperty(
                                        DisplayMemberPath).GetValue(this.listBox.SelectedItem, null).ToString();
                itemsSelected = true;
                if (MovesFocus)
                    TargetControl.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }


        //private ObjectDataProvider odp;
        private ISearchDataProvider odp;

        protected SearchResult DoSearch(string searchTerm)
        {
            if (odp != null & !string.IsNullOrEmpty(searchTerm))
            {
                int nID = 0;
                SearchResult sr = new SearchResult();
                if (int.TryParse(searchTerm, out nID))
                {
                    sr = odp.SearchByKey(nID);                                        
                }
                else
                {                    
                    sr = odp.DoSearch(searchTerm);
                }
                
                return sr;
            }
            else return new SearchResult();
            /*{                
                SearchResult sr = new SearchResult();
                sr.SearchTerm = string.Empty;
                sr.Results = new Dictionary<object, string>();
                return sr;
            }*/
            
        }

        private readonly ObservableCollection<KeyValuePair<object, string>> searchResults = new ObservableCollection<KeyValuePair<object, string>>();

        public ObservableCollection<KeyValuePair<object, string>> SearchResults
        {
            get { return searchResults; }                        
        }


        public ISearchDataProvider SearchDataProvider
        {
            get { return (ISearchDataProvider)GetValue(SearchDataProviderProperty); }
            set { SetValue(SearchDataProviderProperty, value); }
        }

        public static readonly DependencyProperty SearchDataProviderProperty =
            DependencyProperty.Register("SearchDataProvider", typeof(ISearchDataProvider), typeof(TextBoxAutoComplete), new UIPropertyMetadata(null, new PropertyChangedCallback(SearchDataProvider_Changed)));


        private static void SearchDataProvider_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBoxAutoComplete me = d as TextBoxAutoComplete;            
            me.odp = e.NewValue as ISearchDataProvider;            
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
