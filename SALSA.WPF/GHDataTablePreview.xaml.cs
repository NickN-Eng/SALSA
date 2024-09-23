using System;
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
using System.Windows.Shapes;

namespace SALSA.WPF
{
    /// <summary>
    /// Interaction logic for GHDataTablePreview.xaml
    /// </summary>
    public partial class GHDataTablePreview : Window
    {
        /// <summary>
        /// Initializes a new instance of the GHDataTablePreview window.
        /// Reminder: Ensure that the DataTables provided by refreshDataFunction are copies,
        /// not the original DataTables used by GH.
        /// </summary>
        /// <param name="refreshDataFunction">Function to refresh the List<GHDataTable></param>
        public GHDataTablePreview(Func<List<GHDataTable>> refreshDataFunction)
        {
            InitializeComponent();
            DataContext = new GHDataTablePreviewViewModel(refreshDataFunction);
        }


    }
}
