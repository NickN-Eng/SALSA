using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SALSA.WPF
{
    /// <summary>
    /// Interaction logic for DataTableDisplayControl.xaml
    /// </summary>
    public partial class DataTableDisplayControl : UserControl
    {
        private DataTableDisplayControlViewModel _viewModel;

        public DataTableDisplayControl()
        {
            InitializeComponent();
            _viewModel = new DataTableDisplayControlViewModel();
            this.DataContextChanged += DataTableDisplayControl_DataContextChanged;
            this.DataContext = _viewModel;
        }

        private void DataTableDisplayControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            DataTable dataTable = e.NewValue as DataTable;
            _viewModel.DataTable = dataTable;
        }

        private void DataGridColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            DataGridColumnHeader header = sender as DataGridColumnHeader;
            if (header != null && header.Column != null)
            {
                string columnName = header.Column.Header.ToString();
                _viewModel.SelectedColumnName = columnName;

                DataTable dataTable = _viewModel.DataTable;
                if (dataTable != null && dataTable.Columns.Contains(columnName))
                {
                    DataColumn column = dataTable.Columns[columnName];
                    _viewModel.SelectedColumnDataType = column.DataType.ToString();

                    // Get unique values and their counts
                    var valueCounts = dataTable.Rows
                        .Cast<DataRow>()
                        .GroupBy(row => row[columnName])
                        .Select(g => new ValueCount
                        {
                            Value = g.Key != null ? g.Key.ToString() : "NULL",
                            Count = g.Count()
                        })
                        .OrderByDescending(vc => vc.Count);

                    _viewModel.UniqueValues = new ObservableCollection<ValueCount>(valueCounts);

                    _viewModel.IsColumnDetailVisible = true;

                }
            }
        }
    }
}
