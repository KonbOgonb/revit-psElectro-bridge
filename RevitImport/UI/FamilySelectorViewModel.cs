using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Autodesk.Revit.DB;
using RevitSerialization;
using PSImport.Annotations;

namespace PSImport
{
    public class FamilySelectorViewModel:INotifyPropertyChanged
    {
        private List<Tuple<FamilySymbol, string>> _familySymbolsList;
        private bool _createCopy;
        public ExportedDataType DataType { get; set; }
        public List<Tuple<FamilySymbol, string>> FamilySymbolsList
        {
            get { return _familySymbolsList; }
            set 
            { 
                _familySymbolsList = value;
            }
        }

        public bool CreateCopy
        {
            get { return _createCopy; }
            set
            {
                if (_createCopy != value)
                {
                    _createCopy = value;
                    OnPropertyChanged();
                }
            }
        }

        public Tuple<FamilySymbol, string> SelectedSymbol { get; set; }

        #region NotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler SelectedFamilyChanged;  

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) 
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
