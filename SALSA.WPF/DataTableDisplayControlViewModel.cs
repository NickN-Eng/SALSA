using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SALSA.WPF
{
    public class DataTableDisplayControlViewModel : INotifyPropertyChanged
    {

        private bool _isColumnDetailVisible;
        public bool IsColumnDetailVisible
        {
            get => _isColumnDetailVisible;
            set
            {
                if (_isColumnDetailVisible != value)
                {
                    _isColumnDetailVisible = value;
                    OnPropertyChanged(nameof(IsColumnDetailVisible));
                }
            }
        }

        private string _selectedColumnName;
        public string SelectedColumnName
        {
            get => _selectedColumnName;
            set
            {
                if (_selectedColumnName != value)
                {
                    _selectedColumnName = value;
                    OnPropertyChanged(nameof(SelectedColumnName));
                }
            }
        }

        private string _selectedColumnDataType;
        public string SelectedColumnDataType
        {
            get => _selectedColumnDataType;
            set
            {
                if (_selectedColumnDataType != value)
                {
                    _selectedColumnDataType = value;
                    OnPropertyChanged(nameof(SelectedColumnDataType));
                }
            }
        }

        private ObservableCollection<ValueCount> _uniqueValues;
        public ObservableCollection<ValueCount> UniqueValues
        {
            get => _uniqueValues;
            set
            {
                if (_uniqueValues != value)
                {
                    _uniqueValues = value;
                    OnPropertyChanged(nameof(UniqueValues));
                }
            }
        }

        private DataTable _dataTable;
        public DataTable DataTable
        {
            get => _dataTable;
            set
            {
                if (_dataTable != value)
                {
                    _dataTable = value;
                    OnPropertyChanged(nameof(DataTable));
                    OnPropertyChanged(nameof(DataView));
                }
            }
        }

        public void NotifySelectedColumnChanged(string columnName)
        {

        }

        public DataView DataView => DataTable != null ? DataTable.DefaultView : null;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
