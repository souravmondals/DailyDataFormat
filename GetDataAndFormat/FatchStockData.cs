using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace GetDataAndFormat
{
    public class FatchStockData
    {

        public List<stocklistdata> StockList;
        public List<fatchstockdata> fatchstockdatas = new List<fatchstockdata>();
        public async Task GetStockData()
        {
            StockList = File.ReadAllLines("Fsample.txt")
                                            .Select(v => getStockList(v))
                                            .ToList();
            foreach (stocklistdata stock in StockList)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://fitnessgalaxy.in/");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage response = new HttpResponseMessage();
                    response = await client.GetAsync("getdata/?q=" + stock.Name).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                    {
                        string result = response.Content.ReadAsStringAsync().Result;
                        fatchstockdata responseObj = JsonConvert.DeserializeObject<fatchstockdata>(result);
                        fatchstockdatas.Add(responseObj);
                    }

                }
                System.Threading.Thread.Sleep(50);
            }

            string filedate = fatchstockdatas.FirstOrDefault().Date.ToString("yyyy-MM-dd");
            List<Datamodel> stockList = new List<Datamodel>();
            foreach (fatchstockdata fatchstockdata in fatchstockdatas)
            {
                Datamodel datamodel = new Datamodel();
                datamodel.Name = fatchstockdata.Symbol;
                datamodel.dateDate = fatchstockdata.Date.ToString("yyyyMMdd");
                datamodel.Open = fatchstockdata.Open.Replace(",","");
                datamodel.High = fatchstockdata.High.Replace(",", "");
                datamodel.Low = fatchstockdata.Low.Replace(",", "");
                datamodel.Close = fatchstockdata.Close.Replace(",", "");
                if (decimal.Parse(fatchstockdata.Volume.Replace(",", ""))>1)
                {
                    datamodel.Volume = fatchstockdata.Volume.Replace(",", "");
                }
                else
                {
                    datamodel.Volume = StockList.Where(row => row.Name == fatchstockdata.Symbol).Select(row => row.Volume).FirstOrDefault();
                }

                stockList.Add(datamodel);
            }

            var lines = new List<string>();
            var valueLines = stockList.Where(row => row.Name != null).Select(row => string.Join(",", new string[] { row.Name, row.dateDate, row.Open, row.High, row.Low, row.Close, row.Volume }));
            lines.AddRange(valueLines);
            File.WriteAllLines(filedate + "-NSE-EQ-FA.txt", lines.ToArray());
        }


        public stocklistdata getStockList(string csvLine)
        {
            stocklistdata stocklistdata = new stocklistdata();
            string[] values = csvLine.Split(',');
            stocklistdata.Name = values[0];
            stocklistdata.Volume = values[1];
            return stocklistdata;
        }
    }
}
