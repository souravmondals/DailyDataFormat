using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;



namespace DailyDataFormat
{
    public class Program
    {
        public static string filedate;
        public static List<string> StockList;
        public static List<string> SkipList;

        public static List<string> Notfounddata = new List<string>();
        public static List<Datamodel> Newdata = new List<Datamodel>();
        public static async Task Main(string[] args)
        {

            Console.WriteLine("Select following option \n 1) Get data from NSE \n 2) Process data.\n");
            string input = Console.ReadLine();
            if (input == "1")
            {
                await FetchStockData();
            }
            else if (input == "2")
            {
                processStockData();
            }
        }

        public static async Task FetchStockData()
        {
            Console.WriteLine("Enter from date (dd/mm/yyyy)");
            string fromdate = Console.ReadLine();
            Console.WriteLine("Enter to date (dd/mm/yyyy)");
            string todate = Console.ReadLine();

            List<string> dateRange = new List<string>();

            DateTime from = DateTime.ParseExact(fromdate, "dd/MM/yyyy", null);
            if (todate!="")
            {
                DateTime to = DateTime.ParseExact(todate, "dd/MM/yyyy", null);

                for (DateTime date = from; date <= to; date = date.AddDays(1))
                {
                    if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                    {
                        dateRange.Add(date.ToString("ddMMyyyy"));
                    }
                }
            }
            else
            {
                dateRange.Add(from.ToString("ddMMyyyy"));
            }

                foreach (string date in dateRange)
                {
                    try
                    {
                        string FileUrl = $"https://nsearchives.nseindia.com/products/content/sec_bhavdata_full_{date}.csv";

                        HttpClient client = new HttpClient();
                        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
                        byte[] fileBytes = await client.GetByteArrayAsync(FileUrl);
                        File.WriteAllBytes($"sec_bhavdata_full_{date}.csv", fileBytes);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error downloading file for date {date} not found");
                    }
                }
            Console.WriteLine("File downloaded successfully!");
            Console.ReadLine();
        }

        public static void processStockData()
        {
            StockList = File.ReadAllLines("sample.txt")
                                           .Select(v => getStockList(v))       //get all stock list from sample file
                                           .ToList();

            SkipList = File.ReadAllLines("skiplist.txt")
                                            .Select(v => getStockList(v))        //get all stock list from skiplist file
                                            .ToList();

            DirectoryInfo d = new DirectoryInfo(@".\");
            FileInfo[] Files = d.GetFiles("*.csv");

            foreach (FileInfo file in Files)
            {
                List<Datamodel> Stockdata = File.ReadAllLines(file.Name)
                                           .Skip(1)
                                           .Select(v => getNsedata(v))       //get all stock list from NSE csv file which is match with sample file list
                                           .ToList();

                Stockdata = RemoveDuplicate(Stockdata);         //Removing duplicate stock name from NSE CSV file list

                foreach (string stock in StockList)
                {
                    if (!Stockdata.Any(r => r.Name == stock))
                    {
                        Notfounddata.Add(stock);              // Addinf stock name which is present in sample file but not present in NSE CSV file list
                    }
                }

                if (!Directory.Exists("Output"))
                {
                    Directory.CreateDirectory("Output");
                    Directory.CreateDirectory("temp");
                }


                var lines = new List<string>();
                var valueLines = Stockdata.Where(row => row.Name != null).Select(row => string.Join(",", new string[] { row.Name, row.dateDate, row.Open, row.High, row.Low, row.Close, row.Volume }));
                lines.AddRange(valueLines);
                File.WriteAllLines("Output\\" + filedate + "-NSE-EQ.txt", lines.ToArray());     //adding NSE csv data which is matching with sample file list

                var linesN = new List<string>();
                var valueLinesN = Newdata.Where(row => row.Name != null).Select(row => string.Join(",", new string[] { row.Name, row.dateDate, row.Open, row.High, row.Low, row.Close, row.Volume }));
                linesN.AddRange(valueLinesN);
                if (Newdata.Count > 0)
                {
                    Console.WriteLine("New stock found...................");
                    Console.WriteLine(string.Join("\n", linesN));
                    Console.WriteLine("Press 1 for add into Sample, 2 for skiplist, 3 for ignore :- ");
                    string input = Console.ReadLine();
                    if (input == "1")
                    {
                        File.AppendAllLines("sample.txt", linesN.ToArray());
                    }
                    else if (input == "2")
                    {
                        File.AppendAllLines("skiplist.txt", linesN.ToArray());
                    }
                    else
                    {
                        File.WriteAllLines("temp\\" + filedate + "-New-Stock.txt", linesN.ToArray());
                    }


                    StockList = File.ReadAllLines("sample.txt")
                                            .Select(v => getStockList(v))
                                            .ToList();
                    SkipList = File.ReadAllLines("skiplist.txt")
                                            .Select(v => getStockList(v))
                                            .ToList();
                }
                if (Notfounddata.Count > 0)
                {
                    File.WriteAllLines("temp\\" + filedate + "-NotFound-Stock.txt", Notfounddata.ToArray());
                }
                Newdata = new List<Datamodel>();
                Notfounddata = new List<string>();
            }


            Files = d.GetFiles("*NSE-EQ.txt");
            foreach (FileInfo file in Files)
            {
                List<Datamodel> Stockdata = File.ReadAllLines(file.Name)
                                           .Select(v => getBavecopydata(v))
                                           .ToList();

                Stockdata = RemoveDuplicate(Stockdata);

                foreach (string stock in StockList)
                {
                    if (!Stockdata.Any(r => r.Name == stock))
                    {
                        Notfounddata.Add(stock);
                    }
                }


                var lines = new List<string>();
                var valueLines = Stockdata.Where(row => row.Name != null).Select(row => string.Join(",", new string[] { row.Name, row.dateDate, row.Open, row.High, row.Low, row.Close, row.Volume }));
                lines.AddRange(valueLines);
                File.WriteAllLines("Output\\" + filedate + "-NSE-EQ.txt", lines.ToArray());

                var linesN = new List<string>();
                var valueLinesN = Newdata.Where(row => row.Name != null).Select(row => string.Join(",", new string[] { row.Name, row.dateDate, row.Open, row.High, row.Low, row.Close, row.Volume }));
                linesN.AddRange(valueLinesN);
                if (Newdata.Count > 0)
                {
                    Console.WriteLine("New stock found...................");
                    Console.WriteLine(string.Join("\n", linesN));
                    Console.WriteLine("Press 1 for add into Sample, 2 for skiplist, 3 for ignore :- ");
                    string input = Console.ReadLine();
                    if (input == "1")
                    {
                        File.AppendAllLines("sample.txt", linesN.ToArray());
                    }
                    else if (input == "2")
                    {
                        File.AppendAllLines("skiplist.txt", linesN.ToArray());
                    }
                    else
                    {
                        File.WriteAllLines("temp\\" + filedate + "-New-Stock.txt", linesN.ToArray());
                    }


                    StockList = File.ReadAllLines("sample.txt")
                                            .Select(v => getStockList(v))
                                            .ToList();
                    SkipList = File.ReadAllLines("skiplist.txt")
                                            .Select(v => getStockList(v))
                                            .ToList();
                }
                if (Notfounddata.Count > 0)
                {
                    File.WriteAllLines("temp\\" + filedate + "-NotFound-Stock.txt", Notfounddata.ToArray());
                }
                Newdata = new List<Datamodel>();
                Notfounddata = new List<string>();
            }

            Console.WriteLine("All done enter for exit...................");
            Console.ReadLine();
        }

        public static string getStockList(string csvLine)
        {
            string[] values = csvLine.Split(',');
            return values[0];
        }

        public static Datamodel getNsedata(string csvLine)
        {
            Datamodel data = new Datamodel();
            string[] values = csvLine.Split(',');
            data.Name = values[0];
            DateTime date_Date = Convert.ToDateTime(values[2]);
            filedate = date_Date.ToString("yyyy-MM-dd");
            data.dateDate = date_Date.ToString("yyyyMMdd");
            data.Open = values[4];
            data.High = values[5];
            data.Low = values[6];
            data.Close = values[7];
            data.Volume = values[10];
            if (StockList.Contains(values[0]))
            {
                return data;
            }
            else
            {
                Newdata.Add(data);
                return new Datamodel();
            }

        }

        public static Datamodel getBavecopydata(string csvLine)
        {
            if (!string.IsNullOrEmpty(csvLine))
            {
                Datamodel data = new Datamodel();
                string[] values = csvLine.Split(',');
                data.Name = values[0];
                filedate = values[1].Substring(0, 4) + "-" + values[1].Substring(4, 2) + "-" + values[1].Substring(6, 2);
                data.dateDate = values[1];
                data.Open = values[2];
                data.High = values[3];
                data.Low = values[4];
                data.Close = values[5];
                data.Volume = values[6];
                if (StockList.Contains(values[0]))
                {
                    return data;
                }
                else
                {
                    if (!SkipList.Contains(values[0]))
                    {
                        Newdata.Add(data);
                    }
                    return new Datamodel();
                }
            }
            return new Datamodel();

        }


        public static List<Datamodel> RemoveDuplicate(List<Datamodel> Stockdata)
        {
            List<Datamodel> newStockdata = new List<Datamodel>();
            List<string> Stocks = new List<string>();

            foreach (Datamodel stdt in Stockdata)
            {
                if (!Stocks.Contains(stdt.Name))
                {
                    Stocks.Add(stdt.Name);
                    newStockdata.Add(stdt);
                }
            }

            return newStockdata;
        }


    }
}
