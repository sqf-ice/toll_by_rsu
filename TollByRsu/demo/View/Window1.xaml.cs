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

namespace demo.View
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }

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
                showBox.AppendText("连接\r\n");
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
            showBox.AppendText("断开\r\n");
        }

        delegate void appendText(string msg);

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                appendText adt = new appendText(showBox.AppendText);
                int n = int.Parse(Jiaoyi_count.Text);
                while (n-- > 0)
                {
                    showBox.Clear();

                    pr.Jiaoyi();

                    adt.BeginInvoke("交易\t" + pr.ktLane.Jiaoyi_jieguo.ToString() + "\t" +
                        pr.ktLane.Jiaoyi_jieguo_message + "\r\n", null, null);

                    foreach (byte[] bs in pr.pcrsu_data)
                    {
                        adt.BeginInvoke("\t" + TollByRsu.ViaHere.ByteArraryToHexString(bs) + "\r\n", null, null);
                    }
                }

            }
            catch (Exception ex)
            {
                showBox.AppendText("ex:" + ex.Message + "\r\n");
                showBox.AppendText(pr.ktLane.TS.DisplayName + "\r\n");
            }
            finally
            {
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            showBox.Clear();
        }

    }
}
