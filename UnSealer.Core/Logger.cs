using System;
using System.Windows;
using System.Windows.Controls;

namespace UnSealer.Core
{
    public class Logger
    {
        /// <summary>
        /// Logger It Self
        /// </summary>
        private TextBox TextBox { set; get; }
        /// <summary>
        /// Initialize Logger 
        /// </summary>
        /// <param name="LogBox"> Loades Textbox Which We Log On. </param>
        public Logger(TextBox LogBox)
        {
            this.TextBox = LogBox;
        }
        /// <summary>
        /// DEBUG A Message
        /// </summary>
        /// <param name="m"> User Msg </param>
        public void Debug(string m) => Application.Current.Dispatcher.BeginInvoke(new Action(() => { 
            TextBox.AppendText($"[DEBUG] {m}{Environment.NewLine}");
        }));
        /// <summary>
        /// INFO A Message
        /// </summary>
        /// <param name="m"> User Msg </param>
        public void Info(string m) => Application.Current.Dispatcher.BeginInvoke(new Action(() => {
            TextBox.AppendText($"[INFO] {m}{Environment.NewLine}");
        }));
        /// <summary>
        /// WARN A Message
        /// </summary>
        /// <param name="m"> User Msg </param>
        public void Warn(string m) => Application.Current.Dispatcher.BeginInvoke(new Action(() => {
            TextBox.AppendText($"[WARN] {m}{Environment.NewLine}");
        }));
        /// <summary>
        /// Error A Message
        /// </summary>
        /// <param name="m"> User Msg </param>
        public void Error(string m) => Application.Current.Dispatcher.BeginInvoke(new Action(() => {
            TextBox.AppendText($"[Error] {m}{Environment.NewLine}");
        }));
        /// <summary>
        /// Log Custom Message
        /// </summary>
        /// <param name="h"> Custom Header </param>
        /// <param name="m"> User Msg </param>
        public void Custom(string h, string m) => Application.Current.Dispatcher.BeginInvoke(new Action(() => {
            TextBox.AppendText($"[{h}] {m}{Environment.NewLine}");
        }));
        /// <summary>
        /// Clears Logs
        /// </summary>
        public void Clear() => Application.Current.Dispatcher.BeginInvoke(new Action(() => {
            TextBox.Text = string.Empty;
        }));
    }
}