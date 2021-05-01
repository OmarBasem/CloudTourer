using System;
using Gtk;

namespace WebBrowser
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Application.Init();
            MainWindow win = new MainWindow();
            win.Title = "Cloud Tourer";
            win.SetSizeRequest(1000, 500);
            win.Show();
            Application.Run();
        }
    }
}
