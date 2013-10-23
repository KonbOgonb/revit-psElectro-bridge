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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PSImport.UI
{
    /// <summary>
    /// Interaction logic for FamilySelector.xaml
    /// </summary>
    public partial class FamilySelector 
    {
        public FamilySelector()
        {
            InitializeComponent();
        }

        private void _okButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
