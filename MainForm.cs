using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BitcoinRichList
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        static HttpClient client = new HttpClient();

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var result = Task.Run(() => DownloadPageAsync(backgroundWorker.ReportProgress));

            var sw = new StreamWriter($"{DateTime.Now.ToString("dd MM yyyy HH mm ss")}_{Guid.NewGuid()}.csv", false);
            // write header
            sw.Write("#,Address,Balance 1w/1m,% of coins,First In,Last In,Ins,First Out,Last Out,Outs");
            sw.Write(sw.NewLine);
            foreach (var item in result.Result)
            {
                sw.Write($"{item.Index},{item.Address},{string.Format("\"{0}\"", item.Balance)},{item.OfCoin},{item.FirstIn},{item.LastIn},{item.Ins},{item.FirstOut},{item.LastOut},{item.Outs}");
                sw.Write(sw.NewLine);
            }
            sw.Close();
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar.Value = 100;
            MessageBox.Show("It's done!");
        }

        private void button_Click(object sender, EventArgs e)
        {
            backgroundWorker.RunWorkerAsync();
        }

        public delegate void ReportProgressDelegate(int percentage);

        private async Task<List<BitcoinAddress>> DownloadPageAsync(ReportProgressDelegate reportProgress)
        {
            var rows = new List<BitcoinAddress>();
            for (int i = 0; i < 100; i++)
            {
                var result = await DownloadPageAsync(i);
                reportProgress(i);
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(result.Result);
                HtmlNode tableOne = doc.DocumentNode.SelectSingleNode("//table[@id='tblOne']");
                HtmlNode tableOne2 = doc.DocumentNode.SelectSingleNode("//table[@id='tblOne2']");
                var tableDoc = new HtmlAgilityPack.HtmlDocument();
                tableDoc.LoadHtml(tableOne.InnerHtml + tableOne2.InnerHtml);
                var trNodes = tableDoc.DocumentNode.SelectNodes("//tr");

                foreach (var trNode in trNodes)
                {
                    var trDoc = new HtmlAgilityPack.HtmlDocument();
                    trDoc.LoadHtml(trNode.InnerHtml);
                    var tdNodes = trDoc.DocumentNode.SelectNodes("//td");
                    if (tdNodes != null)
                    {
                        var row = new BitcoinAddress
                        {
                            Index = int.Parse(tdNodes[0].InnerText),
                            Address = tdNodes[1].SelectSingleNode("//a").Attributes["href"].Value.Replace("https://bitinfocharts.com/bitcoin/address/", ""),
                            Balance = Regex.Replace(tdNodes[2].InnerText, @"\sBTC\s.*", ""),
                            OfCoin = tdNodes[3].InnerText,
                            FirstIn = tdNodes[4].InnerText.Length > 10 ? tdNodes[4].InnerText.Substring(0, 10) : "",
                            LastIn = tdNodes[5].InnerText.Length > 10 ? tdNodes[5].InnerText.Substring(0, 10) : "",
                            Ins = tdNodes[6].InnerText,
                            FirstOut = tdNodes[7].InnerText.Length > 10 ? tdNodes[7].InnerText.Substring(0, 10) : "",
                            LastOut = tdNodes[8].InnerText.Length > 10 ? tdNodes[8].InnerText.Substring(0, 10) : "",
                            Outs = tdNodes[9].InnerText,
                        };
                        rows.Add(row);
                    }
                }
            }
            return rows;
        }

        private async Task<DownloadPageAsyncResult> DownloadPageAsync(int page)
        {
            var uri = new Uri(textBox.Text);
            if (page > 0)
            {
                uri = new Uri(textBox.Text.Replace(".html", $"-{page}.html"));
            }
            var result = new DownloadPageAsyncResult();
            try
            {
                using (HttpResponseMessage response = await client.GetAsync(uri))
                {
                    using (HttpContent content = response.Content)
                    {
                        string resultString = await content.ReadAsStringAsync();
                        string reasonPhrase = response.ReasonPhrase;
                        HttpResponseHeaders headers = response.Headers;
                        HttpStatusCode code = response.StatusCode;

                        result.Result = resultString;
                        result.ReasonPhrase = reasonPhrase;
                        result.Headers = headers;
                        result.Code = code;
                    }
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
            }
            return result;
        }

    }
}
