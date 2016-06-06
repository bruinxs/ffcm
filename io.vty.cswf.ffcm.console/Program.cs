﻿using io.vty.cswf.doc;
using io.vty.cswf.log;
using io.vty.cswf.netw.dtm;
using io.vty.cswf.netw.sck;
using io.vty.cswf.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace io.vty.cswf.ffcm.console
{
    class Program
    {
        private static readonly ILog L = Log.New();
        static void Main(string[] args)
        {
            var conf = "conf/ffcm_c.properties";
            if (args.Length > 0)
            {
                conf = args[0];
            }
            var cfg = new FCfg();
            cfg.Load(conf, true);
            cfg.Print();
            var addr = cfg.Val("srv_addr", "");
            if (addr.Length < 1)
            {
                Console.WriteLine("the srv_addr is not setted");
                Environment.Exit(1);
                return;
            }
            L.I("starting ffcm...");
            var ffcm = new DocCov("FFCM", cfg, new SckDailer(addr).Dail);
            var ffcmh = new FFCM(ffcm, ffcm.Srv);
            ffcm.InitConfig();
            ffcm.StartMonitor();
            ffcm.Start();
            ffcm.StartProcSrv();
            ThreadPool.QueueUserWorkItem(run_hb, ffcm);
            ffcm.Wait();
        }
        static void run_hb(object s)
        {
            var ffcm = (DocCov)s;
            while (true)
            {
                try
                {
                    L.D("DocCov start hb test...");
                    ffcm.hb("DocCov");
                    L.D("DocCov do hb success");
                }
                catch (Exception e)
                {
                    L.W("{0}", e.Message);
                }
                Thread.Sleep(16000);
            }
        }
    }
}
