using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Threading.Tasks;

namespace futarok_perprog_zhgyak
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Etterem e = new Etterem();
            new Task(() => e.Work(), TaskCreationOptions.LongRunning).Start();
            Enumerable.Range(1, 4).Select(x => new TurboTeknos())
                .ToList()
                .Select(x => new Task(() => x.Work(), TaskCreationOptions.LongRunning))
                .ToList()
                .ForEach(x => x.Start());
            Enumerable.Range(1, 4).Select(x => new FurgeFutar())
                .ToList()
                .Select(x => new Task(() => x.Work(), TaskCreationOptions.LongRunning))
                .ToList()
                .ForEach(x => x.Start());
            new Task(() =>
            {
                while (TurboTeknos.osszes.Any(X=>X.Allapot!=FutarAllapot.Vegzett) || FurgeFutar.osszes.Any(X => X.Allapot != FutarAllapot.Vegzett))
                {
                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine($"Kiszállításra vár: {Etterem.Etelek.Count()}\nKiszallitott: {TurboTeknos.kiszallitott+FurgeFutar.kiszallitott}");
                    Console.WriteLine("FugeFutar");
                    foreach (var item in FurgeFutar.osszes)
                    {
                        Console.WriteLine($"Futar: {item.id} Status:{item.Allapot.ToString().PadRight(40,' ')}");
                    }
                    Console.WriteLine("TurboTeknos");
                    foreach (var item in TurboTeknos.osszes)
                    {
                        Console.WriteLine($"Futar: {item.id} Status:{item.Allapot.ToString().PadRight(40, ' ')}");
                    }
                    Console.WriteLine();
                    Console.WriteLine($"FurgeFutar penz: {FurgeFutar.OsszesPenz}   TurboTeknos: {TurboTeknos.OsszesPenz}");
                    Thread.Sleep(50);
                }
                Console.SetCursorPosition(0, 0);
                Console.WriteLine($"Kiszállításra vár: {Etterem.Etelek.Count()}\nKiszallitott: {TurboTeknos.kiszallitott + FurgeFutar.kiszallitott}");
                Console.WriteLine();
                foreach (var item in FurgeFutar.osszes)
                {
                    Console.WriteLine($"Futar: {item.id} Status:{item.Allapot.ToString().PadRight(40, ' ')}");
                }
                Console.WriteLine();
                foreach (var item in TurboTeknos.osszes)
                {
                    Console.WriteLine($"Futar: {item.id} Status:{item.Allapot.ToString().PadRight(40, ' ')}");
                }
                Console.WriteLine();
                Console.WriteLine($"FurgeFutar penz: {FurgeFutar.OsszesPenz}   TurboTeknos: {TurboTeknos.OsszesPenz}");
            },TaskCreationOptions.LongRunning).Start();
            Console.ReadLine();
        }
    }
    public static class Util
    {
        public static Random rnd = new Random();
    }
    public class Etel
    {
        public int Id { get; set; }
        public int Tavolsag { get; set; }
        public int Ar { get; set; }
    }
    public enum EtteremStatus { Var, RendelesOsszeallitas, Vege}
    public class Etterem
    {
        //public static ConcurrentBag<Etel> Etelek = new ConcurrentBag<Etel>();
        public static List<Etel> Etelek = new List<Etel>();
        public static EtteremStatus Status { get; set; }
        public static object lockObject=new object();
        public static int rendelesek { get; set; }
        public Etterem()
        {
            Status=EtteremStatus.Var;
            rendelesek = 0;
        }
        public void Work()
        {

            while (rendelesek<100)
            {
                Status = EtteremStatus.RendelesOsszeallitas;
                Thread.Sleep(Util.rnd.Next(100, 2000));
                Etelek.Add(new Etel()
                {
                    Ar = Util.rnd.Next(2000, 10000),
                    Tavolsag = Util.rnd.Next(500, 10001)
                });
                rendelesek++;
                
            }
            Status = EtteremStatus.Vege;
        }
    }
    public enum FutarAllapot { Var, Kiszallit, RendelesAtadasa,Visszaut,Vegzett}
    public class TurboTeknos
    {
        static int _id = 1;
        public int id { get; set; }
        public FutarAllapot Allapot { get; set; }
        public static double OsszesPenz=0;
        public Etel Sajat { get; set; }
        public static List<TurboTeknos> osszes = new List<TurboTeknos>();

        public static int kiszallitott = 0;
        public TurboTeknos()
        {
            id = _id++;
            osszes.Add(this);
        }
        public void Work()
        {
            while (FurgeFutar.kiszallitott + TurboTeknos.kiszallitott != 100)
            {
                Allapot = FutarAllapot.Var;
                if (Etterem.Etelek.Count()==0)
                {
                    Thread.Sleep(Util.rnd.Next(1000, 2001));
                    continue;
                }
                lock (Etterem.lockObject)
                {
                    if (Etterem.Etelek.Count() == 0)
                    {
                        Thread.Sleep(Util.rnd.Next(1000, 2001));
                        continue;
                    }
                    Etterem.Etelek.OrderByDescending(x => x.Tavolsag);
                    Sajat = Etterem.Etelek[0];
                    Etterem.Etelek.Remove(Sajat);
                }
                Allapot = FutarAllapot.Kiszallit;
                int ido = (Sajat.Tavolsag / 1000) + Util.rnd.Next(2000, 4000);
                Thread.Sleep(ido);
                Allapot = FutarAllapot.RendelesAtadasa;
                Thread.Sleep(Util.rnd.Next(2000, 5000));
                Allapot = FutarAllapot.Visszaut;
                Thread.Sleep(ido);
                kiszallitott++;
                OsszesPenz += (Sajat.Ar * 0.05) + (Sajat.Tavolsag / 1000);

            }
            Allapot = FutarAllapot.Vegzett;

        }
    }
    public class FurgeFutar
    {
        static int _id = 1;
        public int id { get; set; }
        public FutarAllapot Allapot { get; set; }
        public static double OsszesPenz = 0;
        public Etel Sajat { get; set; }
        public static List<FurgeFutar> osszes = new List<FurgeFutar>();
        public static int kiszallitott = 0;
        public FurgeFutar()
        {
            id = _id++;
            osszes.Add(this);
        }
        public void Work()
        {
            while (FurgeFutar.kiszallitott+TurboTeknos.kiszallitott!=100)
            {
                Allapot = FutarAllapot.Var;
                if (Etterem.Etelek.Count() == 0)
                {
                    Thread.Sleep(Util.rnd.Next(1000, 2001));
                    continue;
                }
                lock (Etterem.lockObject)
                {

                    if (Etterem.Etelek.Count() == 0)
                    {
                        Thread.Sleep(Util.rnd.Next(1000, 2001));
                        continue;
                    }
                    Etterem.Etelek.OrderBy(x => x.Tavolsag);
                    Sajat = Etterem.Etelek[0];
                    Etterem.Etelek.Remove(Sajat);
                }
                Allapot = FutarAllapot.Kiszallit;
                int ido = (Sajat.Tavolsag / 1000) + Util.rnd.Next(2000, 4000);
                Thread.Sleep(ido);
                Allapot = FutarAllapot.RendelesAtadasa;
                Thread.Sleep(Util.rnd.Next(2000, 5000));
                Allapot = FutarAllapot.Visszaut;
                Thread.Sleep(ido);
                kiszallitott++;
                OsszesPenz += 600;

            }
            Allapot = FutarAllapot.Vegzett;

        }
    }
}
