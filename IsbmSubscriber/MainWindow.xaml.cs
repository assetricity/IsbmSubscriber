using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using IsbmClient;

namespace IsbmSubscriber
{
    public partial class MainWindow : Window
    {
        private ConsumerPublicationServiceClient _consumerClient;
        private Timer _timer;
        private string _sessionId;

        public MainWindow()
        {
            InitializeComponent();

            txtChannel.Text = Properties.Settings.Default.DefaultChannel;
            txtTopic.Text = Properties.Settings.Default.DefaultTopic;

            _consumerClient = new ConsumerPublicationServiceClient();

            _timer = new Timer(Properties.Settings.Default.PollInterval);
            _timer.Elapsed += new ElapsedEventHandler(TimerElapsed);
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                _timer.Stop();
                Dispatcher.Invoke((Action)(() =>
                    {
                        Cursor = Cursors.Wait;
                        btnDisconnect.IsEnabled = false;
                    }), null);

                PublicationMessage message = _consumerClient.ReadPublication(_sessionId);
                if (message != null && message.MessageContent != null)
                {
                    Log("Received message with id " + message.MessageID);

                    string fileName = string.Format("{0} - {1}.xml", DateTime.Now.ToString("yyyyMMddHHmmss"), message.MessageID);
                    using (XmlWriter writer = XmlWriter.Create(fileName))
                    {
                        Log("Writing message content to file " + fileName);
                        message.MessageContent.WriteTo(writer);
                    }

                    _consumerClient.RemovePublication(_sessionId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Dispatcher.Invoke((Action)(() =>
                    {
                        Cursor = Cursors.Arrow;
                        btnDisconnect.IsEnabled = true;
                    }), null);
                _timer.Start();
            }
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            try
            {
                _sessionId = _consumerClient.OpenSubscriptionSession(txtChannel.Text, new List<string> { txtTopic.Text }, null, null, null);
                Log("Connected to ISBM with session id " + _sessionId);

                btnConnect.Visibility = Visibility.Collapsed;
                btnDisconnect.Visibility = Visibility.Visible;
                txtChannel.IsReadOnly = true;
                txtTopic.IsReadOnly = true;

                _timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        private void btnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            try
            {
                _consumerClient.CloseSubscriptionSession(_sessionId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _timer.Stop();
                _sessionId = null;

                btnConnect.Visibility = Visibility.Visible;
                btnDisconnect.Visibility = Visibility.Collapsed;
                txtChannel.IsReadOnly = false;
                txtTopic.IsReadOnly = false;
                Log("Disconnected from the ISBM");

                Cursor = Cursors.Arrow;
            }
        }

        private void Log(string entry)
        {
            Dispatcher.Invoke((Action)(() =>
                {
                    string line = string.Format("{0}: {1}", DateTime.Now.ToString(), entry);
                    if (string.IsNullOrEmpty(txtLog.Text))
                    {
                        txtLog.Text = line;
                    }
                    else
                    {
                        txtLog.Text += Environment.NewLine + line;
                    }
                }), null);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Clean up any sessions that are still open
            if (_sessionId != null)
            {
                try
                {
                    _consumerClient.CloseSubscriptionSession(_sessionId);
                }
                catch { }
            }
        }
    }
}
