using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Client
{
    /// <summary>
    /// Interaction logic for InfoDetail.xaml
    /// </summary>
    public partial class InfoDetail : Window
    {
        InvPostes info = new InvPostes();
        IMClient im;

        public InfoDetail(InvPostes select, IMClient imc)
        {
            im = imc;
            DataContext = App.appData;
            info = select;
            InitializeComponent();
            ObservableCollection<InvPostes> data = new ObservableCollection<InvPostes>();
            data.Add(info);
            viewInfo.ItemsSource = data;

            infoSortie.ItemsSource = info.infoSortie;
            infoRetour.ItemsSource = info.infoRetour;
            infoEmplacement.ItemsSource = info.infoEmp;
            infoEnvoieLab.ItemsSource = info.infoEnvoieClone;
            infoConfirm.ItemsSource = info.infoValidClone;
        }

        private void btnX_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void ListViewData_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void copyAll_Click(object sender, RoutedEventArgs e)
        {
            if (viewInfo.SelectedIndex == -1) return;

            var temp = viewInfo.SelectedItems;
            List<string> list = new List<string>();

            foreach (InvPostes item in temp)
            {
                list.Add(item.type + " " + item.model + " " + item.serial + " " + item.statut + " " + item.RF + " " + item.dateSortie + " " + item.RFretour + " " + item.dateRetour + " " + item.emplacement + " " + item.dateEntry + " " + item.dateEntryLab + " " + item.dateClone);
            }

            Clipboard.SetText(String.Join(Environment.NewLine, list));
        }

        private void copyRF_Click(object sender, RoutedEventArgs e)
        {
            if (viewInfo.SelectedIndex == -1) return;

            List<string> list = new List<string>();
            var temp = viewInfo.SelectedItems;

            foreach (InvPostes item in temp)
            {
                list.Add(item.RF);
            }

            Clipboard.SetText(String.Join(Environment.NewLine, list));
        }

        private void copyRFretour_Click(object sender, RoutedEventArgs e)
        {
            if (viewInfo.SelectedIndex == -1) return;
            List<string> list = new List<string>();
            var temp = viewInfo.SelectedItems;

            foreach (InvPostes item in temp)
            {
                list.Add(item.RFretour);
            }

            Clipboard.SetText(String.Join(Environment.NewLine, list));
        }

        private void copySN_Click(object sender, RoutedEventArgs e)
        {
            if (viewInfo.SelectedIndex == -1) return;
            List<string> list = new List<string>();
            var temp = viewInfo.SelectedItems;

            foreach (InvPostes item in temp)
            {
                list.Add(item.serial);
            }

            Clipboard.SetText(String.Join(Environment.NewLine, list));
        }

        private void tb_ajout_Loaded(object sender, RoutedEventArgs e)
        {
            tb_ajout.Text = info.dateEntry + " - " + info.infoAjout;
        }

        private void listModel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void tb_comment_Loaded(object sender, RoutedEventArgs e)
        {
            tb_comment.Text = info.comment;
        }

        private void tb_comment_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tb_comment.Text != info.comment) btn_comment.IsEnabled = true;
            else btn_comment.IsEnabled = false;
        }

        private void btn_comment_Click(object sender, RoutedEventArgs e)
        {
            btn_comment.IsEnabled = false;  
            if (string.IsNullOrWhiteSpace(tb_comment.Text)) tb_comment.Text = "";

            im.MainComment(info.serial, tb_comment.Text);
            info.comment = tb_comment.Text;
        }
    }
}
