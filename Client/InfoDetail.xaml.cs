using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client
{
    /// <summary>
    /// Interaction logic for InfoDetail.xaml
    /// </summary>
    public partial class InfoDetail : Window
    {
        InvPostes info = new InvPostes();
        IMClient im;
        public int ID;

        public InfoDetail(InvPostes select, IMClient imc, int id)
        {
            ID = id;
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
                list.Add(item.type + " " + item.model + item.serial + " " + item.statut + " " + item.RF + " " + item.dateSortie + " " + item.RFretour + " " + item.dateRetour + " " + item.emplacement + " " + item.dateEntry + " " + item.dateEntryLab + " " + item.dateClone);
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

        private void ListBills_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void ListBills_Loaded(object sender, RoutedEventArgs e)
        {
            string toSend = "";

            if (!string.IsNullOrEmpty(info.RF)) toSend = info.RF;

            if (!string.IsNullOrEmpty(info.RFretour))
            {
                if (toSend == "") toSend = info.RFretour;
                else toSend = toSend + Environment.NewLine + info.RFretour;
            }

            if (toSend != "") im.CheckWB(ID, toSend);
        }

        private void ListBills_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (ListBills.SelectedIndex == -1)
                {
                    e.Handled = true;
                    return;
                }

                var selected = ListBills.SelectedItem as Waybills;
                Clipboard.SetText(selected.RF + Environment.NewLine + selected.wayb);
                e.Handled = true;
            }
        }

        private void copyRFwb_Click(object sender, RoutedEventArgs e)
        {
            if (ListBills.SelectedIndex == -1) return;
            if (ListBills.SelectedItem == null) return;

            Waybills item = ListBills.SelectedItem as Waybills;
            Clipboard.SetText(item.RF);
        }

        private void copyBills_Click(object sender, RoutedEventArgs e)
        {
            if (ListBills.SelectedIndex == -1) return;
            if (ListBills.SelectedItem == null) return;

            Waybills item = ListBills.SelectedItem as Waybills;
            var info = item.wayb.Split(new[] { '\r', '\n' });

            for (int i = 0; i < info.Count(); i++)
            {
                info[i] = info[i].Split(" - ")[0];
            }

            Clipboard.SetText(string.Join(Environment.NewLine, info));
        }

        private void copyRFetBills_Click(object sender, RoutedEventArgs e)
        {
            if (ListBills.SelectedIndex == -1) return;
            if (ListBills.SelectedItem == null) return;

            Waybills item = ListBills.SelectedItem as Waybills;
            var info = item.wayb.Split(new[] { '\r', '\n' });

            for (int i = 0; i < info.Count(); i++)
            {
                info[i] = info[i].Split(" - ")[0];
            }

            Clipboard.SetText(item.RF + Environment.NewLine + string.Join(Environment.NewLine, info));
        }

        private void copyBillsRetour_Click(object sender, RoutedEventArgs e)
        {
            if (ListBills.SelectedIndex == -1) return;
            if (ListBills.SelectedItem == null) return;

            Waybills item = ListBills.SelectedItem as Waybills;
            var info = item.wbRetour.Split(new[] { '\r', '\n' });

            for (int i = 0; i < info.Count(); i++)
            {
                info[i] = info[i].Split(" - ")[0];
            }

            Clipboard.SetText(item.RF + Environment.NewLine + string.Join(Environment.NewLine, info));
        }

        private void copyRFetBillsRetour_Click(object sender, RoutedEventArgs e)
        {
            if (ListBills.SelectedIndex == -1) return;
            if (ListBills.SelectedItem == null) return;

            Waybills item = ListBills.SelectedItem as Waybills;
            var info = item.wbRetour.Split(new[] { '\r', '\n' });

            for (int i = 0; i < info.Count(); i++)
            {
                info[i] = info[i].Split(" - ")[0];
            }

            Clipboard.SetText(item.RF + Environment.NewLine + string.Join(Environment.NewLine, info));
        }

        private void copyRFAllBill_Click(object sender, RoutedEventArgs e)
        {
            if (ListBills.SelectedIndex == -1) return;
            if (ListBills.SelectedItem == null) return;

            Waybills item = ListBills.SelectedItem as Waybills;

            if (string.IsNullOrEmpty(item.wbRetour)) Clipboard.SetText(item.RF + Environment.NewLine + item.wayb);
            else if (string.IsNullOrEmpty(item.wayb)) Clipboard.SetText(item.RF + Environment.NewLine + item.wbRetour);
            else Clipboard.SetText(item.RF + Environment.NewLine + item.wayb + Environment.NewLine + Environment.NewLine + item.wbRetour);
        }

        private void copyAllBill_Click(object sender, RoutedEventArgs e)
        {
            if (ListBills.SelectedIndex == -1) return;
            if (ListBills.SelectedItem == null) return;

            Waybills item = ListBills.SelectedItem as Waybills;

            if (string.IsNullOrEmpty(item.wbRetour)) Clipboard.SetText(item.wayb);
            else if (string.IsNullOrEmpty(item.wayb)) Clipboard.SetText(item.wbRetour);
            else Clipboard.SetText(item.wayb + Environment.NewLine + Environment.NewLine + item.wbRetour);
        }

        private void puro_Click(object sender, RoutedEventArgs e)
        {
            if (ListBills.SelectedIndex == -1 || ListBills.SelectedItem is null) return;

            var puroNum = (ListBills.SelectedItem as Waybills).wayb;
            var result = puroNum.ToUpper().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            for (int i = 0; i < result.Count(); i++)
            {
                result[i] = result[i].Split(" - ")[0];
            }

            var final = String.Join(",", result);
            var link = "https://www.purolator.com/en/shipping/tracker?pins=" + final;

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = link,
                UseShellExecute = true
            };

            Process.Start(psi);
        }

        private void puroRetour_Click(object sender, RoutedEventArgs e)
        {
            if (ListBills.SelectedIndex == -1 || ListBills.SelectedItem is null) return;

            var puroNum = (ListBills.SelectedItem as Waybills).wbRetour;
            var result = puroNum.ToUpper().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            for (int i = 0; i < result.Count(); i++)
            {
                result[i] = result[i].Split(" - ")[0];
            }

            var final = String.Join(",", result);
            var link = "https://www.purolator.com/en/shipping/tracker?pins=" + final;

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = link,
                UseShellExecute = true
            };

            Process.Start(psi);
        }
    }
}
