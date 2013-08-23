using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace gitzip.api.repo.util
{
    public class Messaging
    {
        delegate void WriteToScreenDelegate(string str);
        public static void WriteToScreen(string str)
        {
            Debug.WriteLine(str);
            /*if (this.InvokeRequired)
            {
                this.Invoke(new WriteToScreenDelegate(WriteToScreen), str);
                return;
            }

            this.richTextBox1.AppendText(str + "\n");
            this.richTextBox1.ScrollToCaret();*/
        }
    }
}