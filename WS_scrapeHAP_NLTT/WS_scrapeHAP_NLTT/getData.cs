using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using System.Timers;
using WS_scrapeHAP_NLTT;
using System.Net.Http;
using System.Data.SqlClient;
using HtmlAgilityPack;

namespace WS_scrapeHAP_NLTT
{
    public class getData
    {
        private readonly Timer timer;
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public getData()
        {
            timer = new Timer(5000) { AutoReset = true };
            timer.Elapsed += Run;
        }
        private async void Run(object sender, ElapsedEventArgs e)
        {
            string urltoken = "http://192.168.68.72:8888/xac-thuc/dang-nhap/";
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage message = client.GetAsync(urltoken).Result;
                if (message.IsSuccessStatusCode)
                {
                    string data = message.Content.ReadAsStringAsync().Result;
                    var htmlDocument = new HtmlDocument();
                    htmlDocument.LoadHtml(data);
                    var a = htmlDocument.DocumentNode.SelectSingleNode("//form[@id='login-form']/input");
                    token = a.Attributes["value"].Value;
                    //log.Info(token);
                    
                    client.DefaultRequestHeaders.Add("Referer", "http://192.168.68.72:8888/xac-thuc/dang-nhap");
                    List<KeyValuePair<string, string>> param = new List<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>("__RequestVerificationToken", token),
                        new KeyValuePair<string, string>("Username", "admin"),
                        new KeyValuePair<string, string>("Password", "123"),
                    };
                    FormUrlEncodedContent form = new FormUrlEncodedContent(param);
                    HttpResponseMessage message1 = client.PostAsync(urltoken, form).Result;

                    string data1 = message1.Content.ReadAsStringAsync().Result;
                    htmlDocument.LoadHtml(data1);
                                      
                    var GetElment = htmlDocument.DocumentNode.SelectNodes("//div[@class='m-widget1']/div").ToList();
                    var items = new List<dataConst>();
                    
                    foreach (var RawData in GetElment)
                    {
                        var dataType = RawData.SelectSingleNode("//div[@class,'row']/div[1]/h3/a").InnerText.ToString().Trim('\r', '\n');                                  
                        var currentCapacity = RawData.SelectSingleNode("//div[@class,'row']/div[2]/h3").InnerText.ToString().Trim('\r', '\n');
                        var maxcapacity = RawData.SelectSingleNode("//div[@class,'row']/div[3]/h3").InnerText.ToString().Trim('\r', '\n');
                        var defaulCapacity = RawData.SelectSingleNode("//div[@class,'row']/div[4]/h3").InnerText.ToString().Trim('\r', '\n');                      
                        var total = RawData.SelectSingleNode("//div[@class,'row']/div[5]/h3").InnerText.ToString().Trim('\r', '\n');
                        

                        var item = new dataConst
                        {
                            dataType = dataType,
                            currentCapacity = currentCapacity,
                            maxCapacity = maxCapacity,
                            defaulCapacity = defaulCapacity,
                            total = total,
                        };
                        items.Add(item);                       

                        SqlConnection sqlConnection;
                        string connectionString = @"Data Source=ADMIN\SQLEXPRESS;Initial Catalog=Data_EVNNLDC;User ID=sa;Password=030920";
                        sqlConnection = new SqlConnection(connectionString);
                        sqlConnection.Open();
                        DateTime dateTimeVariable = DateTime.Now;
                        using (SqlCommand sqlCmd = new SqlCommand { CommandText = "INSERT INTO statistical ([dataType],[currentCapacity],[maxCapacity],[defaulCapacity],[total],[createdAt]) VALUES (@dataType, @currentCapacity, @maxCapacity, @defaulCapacity, @total, @createdAt)", Connection = sqlConnection })
                        {
                            sqlCmd.Parameters.AddWithValue("@dataType", item.dataType);
                            sqlCmd.Parameters.AddWithValue("@currentCapacity", item.currentCapacity);
                            sqlCmd.Parameters.AddWithValue("@maxCapacity", item.maxCapacity);
                            sqlCmd.Parameters.AddWithValue("@defaulCapacity", item.defaulCapacity);
                            sqlCmd.Parameters.AddWithValue("@total", item.total);
                            sqlCmd.Parameters.AddWithValue("@createdAt", dateTimeVariable);
                            sqlCmd.ExecuteNonQuery();
                        }
                        sqlConnection.Close();                        
                    }
                    log.Info("Successfully archived!");                
                }
            }            
        }
        public void Start()
        {
            timer.Start();
        }
        public void Stop()
        {
            timer.Stop();
        }
    }
}
