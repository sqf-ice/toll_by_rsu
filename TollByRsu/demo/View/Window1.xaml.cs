using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace demo.View
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window, INotifyPropertyChanged
    {
        public Window1()
        {
            InitializeComponent();
        }

        #region INotifyPropertyChanged 成员

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        TollByRsu.PcRsu pr = new TollByRsu.PcRsu();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string s = (cBox.SelectedItem as ComboBoxItem).Content as string;
                switch (s)
                {
                    case "网口":
                        pr.ConnectRsu(ipAddress.Text, int.Parse(port.Text));
                        break;
                    case "串口":
                        pr.ConnectRsu(serialPortName.Text);
                        break;
                    default:
                        break;
                }
                showBox.AppendText("连接\t" + pr.IsRsuConnected.ToString() + "\r\n");
            }
            catch (Exception ex)
            {
                showBox.AppendText(ex.Message + "\r\n");
            }
            finally { }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            pr.DisConnectRsu();
            showBox.AppendText("断开\t" + pr.IsRsuConnected.ToString() + "\r\n");
        }

        delegate void appendText(string msg);
        delegate void clearBox();

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                JiaoyiDemo();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ex:" + ex.Message + "\r\n");
            }
            finally
            {
                showBox.AppendText("end\r\n");
            }
        }

        private void JiaoyiDemo()
        {
            int n = int.Parse(Jiaoyi_count.Text);
            while (n-- > 0)
            {
                showBox.Clear();
                pr.Jiaoyi();

                showBox.AppendText( "交易结果\t" + pr.jiaoyi_rt_id.ToString() + "\t" +
                    pr.jiaoyi_rt_message + "\r\n");
                showBox.AppendText("通信方式\t" + pr.DisplayConnect + "\r\n"); 

                foreach (byte[] bs in pr.pcrsu_data)
                {
                    showBox.AppendText("\t" + TollByRsu.ViaHere.ByteArraryToHexString(bs) + "\r\n");
                }
            }
 
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            showBox.Clear();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (pr.IsRsuConnected)
            {
                pr.DisConnectRsu();
            }
        }

    }
}
