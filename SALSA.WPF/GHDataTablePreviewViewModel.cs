using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Input;
using SALSA.WPF;
using System.Data;


namespace SALSA.WPF
{
    /// <summary>
    /// 
    /// </summary>
    public class GHDataTablePreviewViewModel : INotifyPropertyChanged
    {
        private List<GHDataTable> _ghDataTables;
        private GHDataTable _selectedGHDataTable;
        private readonly Func<List<GHDataTable>> _refreshDataFunction;

        public GHDataTablePreviewViewModel(Func<List<GHDataTable>> refreshDataFunction)
        {
            _refreshDataFunction = refreshDataFunction;
            RefreshCommand = new RelayCommand(RefreshData);
            RefreshData();
        }

        public List<GHDataTable> GHDataTables
        {
            get => _ghDataTables;
            set
            {
                if (_ghDataTables != value)
                {
                    _ghDataTables = value;
                    OnPropertyChanged(nameof(GHDataTables));
                }
            }
        }

        public GHDataTable SelectedGHDataTable
        {
            get => _selectedGHDataTable;
            set
            {
                if (_selectedGHDataTable != value)
                {
                    _selectedGHDataTable = value;
                    OnPropertyChanged(nameof(SelectedGHDataTable));
                    OnPropertyChanged(nameof(SelectedDataTable));
                }
            }
        }

        public DataTable SelectedDataTable => SelectedGHDataTable?.DataTable;

        public ICommand RefreshCommand { get; }

        private void RefreshData()
        {
            GHDataTables = _refreshDataFunction.Invoke();
            OnPropertyChanged(nameof(GHDataTables));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
