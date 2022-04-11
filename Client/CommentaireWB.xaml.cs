using System.Windows;
using System.Windows.Input;

namespace Client
{
    /// <summary>
    /// Interaction logic for CommentaireWB.xaml
    /// </summary>
    public partial class CommentaireWB : Window
    {
        IMClient im;
        string RF;
        string WB;
        string info;

        public CommentaireWB(IMClient imc,string R, string w, string inf)
        {
            im = imc;
            RF = R;
            WB = w;
            info = inf;
            DataContext = App.appData;
            InitializeComponent();
        }

		private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
            this.DragMove();
		}

		private void btnX_Click(object sender, RoutedEventArgs e)
		{
            this.Close();
		}

		private void btn_comment_Click(object sender, RoutedEventArgs e)
		{
            im.EnvoieComment(RF, WB, tb_comment.Text);
            this.Close();
		}

		private void tb_comment_Loaded(object sender, RoutedEventArgs e)
		{
            tb_comment.Text = info;
            tb_comment.Focus();
		}

		private void tb_comment_KeyDown(object sender, KeyEventArgs e)
		{
            if (e.Key == Key.Enter) btn_comment_Click(null, null);
		}
	}
}
