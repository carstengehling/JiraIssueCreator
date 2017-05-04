using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraIssueCreator
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Phrase Number
        
        private Int32 _phraseNumber;        
        public MainWindowViewModel()
        {
            PhraseNumber = 4;         
        }

        /// <summary>
        /// The dataprovider has a list of phrases and each phrase
        /// is numbered.  When the operator selects a phrase
        /// this property return the Phrase Number
        /// </summary>
        public Int32 PhraseNumber
        {
            get { return this._phraseNumber; }
            set {
                if (this._phraseNumber != value)
                {
                    this._phraseNumber = value;
                    this.NotifyPropertyChanged("PhraseNumber");
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
        
        public MyDataProviderEng MySearchProviderEng {get { return new MyDataProviderEng();}}
    }
}
