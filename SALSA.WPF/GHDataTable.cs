using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.ComponentModel;
using System.Data;

namespace SALSA.WPF
{
    /// <summary>
    /// Class for displaying a DataTable and its path.
    /// </summary>
    public class GHDataTable : INotifyPropertyChanged
    {
        public GHDataTable(DataTable dataTable, string path)
        {
            DataTable = dataTable;
            Path = path;
        }


        private DataTable _dataTable;
        private string _path;


        public DataTable DataTable
        {
            get => _dataTable;
            set
            {
                if (_dataTable != value)
                {
                    _dataTable = value;
                    OnPropertyChanged(nameof(DataTable));
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public string Path
        {
            get => _path;
            set
            {
                if (_path != value)
                {
                    _path = value;
                    OnPropertyChanged(nameof(Path));
                }
            }
        }

        public string Name
        {
            get
            {
                return DataTable != null ? DataTable.TableName : string.Empty;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

}
