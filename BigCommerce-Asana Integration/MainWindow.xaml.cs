using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.Globalization;

namespace BigCommerce_Asana_Integration
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private delegate void UpdateProgressBarDelegate(
     System.Windows.DependencyProperty dp, Object value);
        class task_data
        {
            public string order_id { get; set; }
            public string c_name { get; set; }
            public string c_email { get; set; }
            public string c_phone { get; set; }
            public string total_cost { get; set; }
            public string shipping_address { get; set; }
            public string item_details { get; set; }
            public string c_message { get; set; }
            
        }
        private void button1_Click(object sender, RoutedEventArgs e)
        {
           
     
           var webClient = new WebClient();
           string server_time = webClient.DownloadString("http://fuzonmedia.com/BC_Service_Important/server_time.txt");
           
             //Configure the ProgressBar
            ProgressBar1.Minimum = 0;
           
            ProgressBar1.Value = 0;

            //Stores the value of the ProgressBar
            double p_value = 0;

            UpdateProgressBarDelegate updatePbDelegate =
              new UpdateProgressBarDelegate(ProgressBar1.SetValue);


            List<task_data> freport = new List<task_data>();

            process_status.Content = "Please wait while we creating task ....";
            button1.Content = "Wait...";
            button1.IsEnabled = false;
           
            if (MessageBox.Show("Are you sure want to proceed?", "Warning!", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                try
                {

                    ProgressBar1.Minimum = 0;

                    ProgressBar1.Value = 0;
                    ProgressBar1.Maximum = 50;

                    WebRequest req_big_order_count = WebRequest.Create(big_storeurl.Text + "orders?status_id=11");
                   // WebRequest req_big_order_count = WebRequest.Create(big_storeurl.Text + "orders/2338");
                    HttpWebRequest httpreq_order_count = (HttpWebRequest)req_big_order_count;
                    httpreq_order_count.Method = "GET";
                    httpreq_order_count.ContentType = "text/xml; charset=utf-8";
                    
                        double timestamp = Convert.ToDouble(server_time);

                        // First make a System.DateTime equivalent to the UNIX Epoch.
                        System.DateTime dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);

                        // Add the number of seconds in UNIX timestamp to be converted.
                        dateTime = dateTime.AddSeconds(timestamp);

                        // The dateTime now contains the right date/time so to format the string,
                        // use the standard formatting methods of the DateTime object.
                        string printDate = dateTime.GetDateTimeFormats()[104];
                        httpreq_order_count.IfModifiedSince = Convert.ToDateTime(printDate);
                    
                    httpreq_order_count.Credentials = new NetworkCredential(big_user.Text, big_pass.Text);
                    HttpWebResponse res_order = (HttpWebResponse)httpreq_order_count.GetResponse();
                   
                    StreamReader rdr_product_count = new StreamReader(res_order.GetResponseStream());
                    string result_order= rdr_product_count.ReadToEnd();
                    //textBox1.Text = result_order;
                   
                    if (res_order.StatusCode == HttpStatusCode.OK || res_order.StatusCode == HttpStatusCode.Accepted)
                    {
                            XDocument doc_orders = XDocument.Parse(result_order);
                            foreach (XElement order_data in doc_orders.Descendants("order"))
                           {
                               p_value += 1;

                               Dispatcher.Invoke(updatePbDelegate,
                                                System.Windows.Threading.DispatcherPriority.Background,
                                                new object[] { ProgressBar.ValueProperty, p_value });
                               task_data tsata = new task_data();
                               tsata.order_id = order_data.Element("id").Value.ToString();
                              // MessageBox.Show(tsata.order_id);
                               tsata.c_message = order_data.Element("customer_message").Value.ToString().Replace("\"", "\\\"").Replace("\r\n", "\\n").Replace("\n", "\\n");
                               tsata.c_name = order_data.Element("billing_address").Element("first_name").Value.ToString() +" "+ order_data.Element("billing_address").Element("last_name").Value.ToString();
                               tsata.total_cost = Convert.ToDouble(order_data.Element("total_inc_tax").Value.ToString()).ToString("0.00", CultureInfo.InvariantCulture);
                               tsata.c_email = order_data.Element("billing_address").Element("email").Value.ToString();
                               tsata.c_phone = order_data.Element("billing_address").Element("phone").Value.ToString();
                             //  MessageBox.Show("shiipping_Addes");

                               WebRequest req_big_shipping_count = WebRequest.Create(big_storeurl.Text + "orders/" + tsata.order_id + "/shippingaddresses");
                              HttpWebRequest httpreq_shipping_count = (HttpWebRequest)req_big_shipping_count;
                              httpreq_shipping_count.Method = "GET";
                              httpreq_shipping_count.ContentType = "text/xml; charset=utf-8";
                              httpreq_shipping_count.Credentials = new NetworkCredential(big_user.Text, big_pass.Text);
                              HttpWebResponse res_shipping = (HttpWebResponse)httpreq_shipping_count.GetResponse();
                              StreamReader rdr_shipping_count = new StreamReader(res_shipping.GetResponseStream());
                              string result_shipping = rdr_shipping_count.ReadToEnd();
                              if (res_shipping.StatusCode == HttpStatusCode.OK || res_shipping.StatusCode == HttpStatusCode.Accepted)
                              {
                                  XDocument doc_shippings = XDocument.Parse(result_shipping);
                                  foreach (XElement order_shipping in doc_shippings.Descendants("address"))
                                   {
                                       tsata.shipping_address = order_shipping.Element("street_1").Value.ToString()+ " "+order_shipping.Element("street_2").Value.ToString();
                                       tsata.shipping_address += "\\n" + order_shipping.Element("city").Value.ToString() + "," + order_shipping.Element("state").Value.ToString() + "," + order_shipping.Element("zip").Value.ToString();
                                       break;

                                   }
                              }
                            //  MessageBox.Show("Products");
                              WebRequest req_big_productcount = WebRequest.Create(big_storeurl.Text + "orders/" + tsata.order_id + "/products");
                              HttpWebRequest httpreq_product_count = (HttpWebRequest)req_big_productcount;
                              httpreq_product_count.Method = "GET";
                              httpreq_product_count.ContentType = "text/xml; charset=utf-8";
                              httpreq_product_count.Credentials = new NetworkCredential(big_user.Text, big_pass.Text);
                              HttpWebResponse res_product = (HttpWebResponse)httpreq_product_count.GetResponse();
                              StreamReader rdr_product_data = new StreamReader(res_product.GetResponseStream());
                              string result_product = rdr_product_data.ReadToEnd();
                             // MessageBox.Show(result_product);
                              if (res_product.StatusCode == HttpStatusCode.OK || res_product.StatusCode == HttpStatusCode.Accepted)
                              {
                                  XDocument doc_products = XDocument.Parse(result_product);
                                  foreach (XElement order_product in doc_products.Descendants("product"))
                                  {
                                      string pr_op="";
                                      foreach (XElement order_product_options in order_product.Descendants("product_options").Descendants("option"))
                                      {
                                         // MessageBox.Show(order_product_options.Element("display_value").Value.ToString());
                                          pr_op += order_product_options.Element("display_value").Value.ToString().Replace("\"", "\\\"") + " ";
                                          
                                      }


                                      tsata.item_details += order_product.Element("name").Value.ToString().Replace("\"","\\\"") + " X " + order_product.Element("quantity").Value.ToString() + " - " + pr_op + " - $" + Convert.ToDouble(order_product.Element("price_inc_tax").Value.ToString()).ToString("0.00", CultureInfo.InvariantCulture) + "\\n";
                                  }
 
                              }

                            //  MessageBox.Show(tsata.shipping_address);
                          //  MessageBox.Show(tsata.item_details);

                             // MessageBox.Show("Asana Set");

                              WebRequest req_asana = WebRequest.Create("https://app.asana.com/api/1.0/tasks");
                              HttpWebRequest httpreq_asana = (HttpWebRequest)req_asana;
                              httpreq_asana.Method = "POST";

                              httpreq_asana.ContentType = "application/json";
                              httpreq_asana.Headers.Add("Authorization", "Basic "+ asana_APIKey.Text);
                              Stream str_asana = httpreq_asana.GetRequestStream();
                              StreamWriter strwriter_asana = new StreamWriter(str_asana, Encoding.ASCII);
                            // string soaprequest_asana = "{\"data\":{\"workspace\":"+workSpaceID.Text+",\"name\":\""+"(#"+tsata.order_id+") "+tsata.c_name+" $"+tsata.total_cost+"\",\"notes\":\""+tsata.c_email+" - "+ tsata.c_phone +"\\n\\n" + tsata.shipping_address+"\\n\\n" +tsata.item_details+"\",\"assignee\":"+Assignee_ID.Text+"}}";
                              string soaprequest_asana = "{\"data\":{\"workspace\":" + workSpaceID.Text + ",\"name\":\"" + "(#" + tsata.order_id + ") " + tsata.c_name + " $" + tsata.total_cost + "\",\"notes\":\"" + tsata.c_email + " - " + tsata.c_phone + "\\n\\n" + tsata.shipping_address + "\\n\\n" + tsata.item_details + "\\n\\n" + tsata.c_message + "\",\"projects\":[812474455499]}}";
                           //   MessageBox.Show(soaprequest_asana);
                              textBox1.Text = soaprequest_asana.ToString();
                              strwriter_asana.Write(soaprequest_asana.ToString());
                              strwriter_asana.Close();
                              HttpWebResponse res_asana = (HttpWebResponse)httpreq_asana.GetResponse();
                              StreamReader rdr_asana = new StreamReader(res_asana.GetResponseStream());
                              string result_asana_task = rdr_asana.ReadToEnd();
                            //  MessageBox.Show(result_asana_task);
                              freport.Add(tsata);
                             
                           }
                        }
                    
                    WebRequest req_big_time = WebRequest.Create(big_storeurl.Text + "time");
                    HttpWebRequest httpreq_time = (HttpWebRequest)req_big_time;
                    httpreq_time.Method = "GET";
                    httpreq_time.ContentType = "text/xml; charset=utf-8";
                    httpreq_time.Credentials = new NetworkCredential(big_user.Text, big_pass.Text);
                    HttpWebResponse res_time = (HttpWebResponse)httpreq_time.GetResponse();
                    StreamReader rdr_time = new StreamReader(res_time.GetResponseStream());
                    string result_time = rdr_time.ReadToEnd();
                   // MessageBox.Show(result_time);
                    if (res_time.StatusCode == HttpStatusCode.OK || res_time.StatusCode == HttpStatusCode.Accepted)
                    {
                        XDocument doc_time = XDocument.Parse(result_time);

                        string stime = doc_time.Element("time").Element("time").Value.ToString();

                          var webClient_1 = new WebClient();
                          string readHtml_1 = webClient_1.DownloadString("http://fuzonmedia.com/BC_Service_Important/getData.php?time=" + stime);
                       // File.WriteAllText("server_time.txt", stime);

                    
                    }

                    p_value = 50;

                    Dispatcher.Invoke(updatePbDelegate,
                                     System.Windows.Threading.DispatcherPriority.Background,
                                     new object[] { ProgressBar.ValueProperty, p_value });
                    MessageBox.Show("Task Completed");
                    process_status.Content = "Task Completed";
                    button1.IsEnabled = true;

                    button1.Content = "Create Task";

                    
                  
                    }

                catch (Exception ex)
                {
                    if (ex.Message == "The remote server returned an error: (304) Not Modified.")
                    {
                        WebRequest req_big_time = WebRequest.Create(big_storeurl.Text + "time");
                        HttpWebRequest httpreq_time = (HttpWebRequest)req_big_time;
                        httpreq_time.Method = "GET";
                        httpreq_time.ContentType = "text/xml; charset=utf-8";
                        httpreq_time.Credentials = new NetworkCredential(big_user.Text, big_pass.Text);
                        HttpWebResponse res_time = (HttpWebResponse)httpreq_time.GetResponse();
                        StreamReader rdr_time = new StreamReader(res_time.GetResponseStream());
                        string result_time = rdr_time.ReadToEnd();
                        // MessageBox.Show(result_time);
                        if (res_time.StatusCode == HttpStatusCode.OK || res_time.StatusCode == HttpStatusCode.Accepted)
                        {
                            XDocument doc_time = XDocument.Parse(result_time);

                            string stime = doc_time.Element("time").Element("time").Value.ToString();
                            var webClient_1 = new WebClient();
                            string readHtml_1 = webClient_1.DownloadString("http://fuzonmedia.com/BC_Service_Important/getData.php?time=" + stime);
                           // File.WriteAllText("server_time.txt", stime);


                        }

                        MessageBox.Show("No New Orders yet ");
                        process_status.Content = "No New Orders yet";
                    }
                    else
                    {
                        MessageBox.Show(ex.Message.ToString());
                        process_status.Content = "Error :" + ex.Message.ToString();
                    }
                    button1.IsEnabled = true;
                    
                    button1.Content = "Create Task";
                  // textBox1.Text = ex.Message.ToString();
                }
            }
            else
            {
                button1.IsEnabled = true;
                process_status.Content = "";
                button1.Content = "Create Task";

            }

        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            WebRequest req_big_time = WebRequest.Create(big_storeurl.Text + "time");
                    HttpWebRequest httpreq_time = (HttpWebRequest)req_big_time;
                    httpreq_time.Method = "GET";
                    httpreq_time.ContentType = "text/xml; charset=utf-8";
                    httpreq_time.Credentials = new NetworkCredential(big_user.Text, big_pass.Text);
                    HttpWebResponse res_time = (HttpWebResponse)httpreq_time.GetResponse();
                    StreamReader rdr_time = new StreamReader(res_time.GetResponseStream());
                    string result_time= rdr_time.ReadToEnd();
                    if (res_time.StatusCode == HttpStatusCode.OK || res_time.StatusCode == HttpStatusCode.Accepted)
                    {
                    }

        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WebRequest req_big_time = WebRequest.Create(big_storeurl.Text + "time");
                HttpWebRequest httpreq_time = (HttpWebRequest)req_big_time;
                httpreq_time.Method = "GET";
                httpreq_time.ContentType = "text/xml; charset=utf-8";
                httpreq_time.Credentials = new NetworkCredential(big_user.Text, big_pass.Text);
                HttpWebResponse res_time = (HttpWebResponse)httpreq_time.GetResponse();
                StreamReader rdr_time = new StreamReader(res_time.GetResponseStream());
                string result_time = rdr_time.ReadToEnd();
                MessageBox.Show(result_time);
                if (res_time.StatusCode == HttpStatusCode.OK || res_time.StatusCode == HttpStatusCode.Accepted)
                {
                    XDocument doc_time = XDocument.Parse(result_time);

                    string stime = doc_time.Element("time").Element("time").Value.ToString();
                    // File.WriteAllText("server_time.txt", stime);
                    MessageBox.Show(stime);

                    double timestamp = Convert.ToDouble(stime);

                    // First make a System.DateTime equivalent to the UNIX Epoch.
                    System.DateTime dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);

                    // Add the number of seconds in UNIX timestamp to be converted.
                    dateTime = dateTime.AddSeconds(timestamp);

                    // The dateTime now contains the right date/time so to format the string,
                    // use the standard formatting methods of the DateTime object.
                    string printDate = dateTime.ToShortDateString() + " " + dateTime.ToShortTimeString();
                    MessageBox.Show(printDate);


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
                
            }
        
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            double timestamp = Convert.ToDouble("1363195860");

            // First make a System.DateTime equivalent to the UNIX Epoch.
            System.DateTime dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);

            // Add the number of seconds in UNIX timestamp to be converted.
            dateTime = dateTime.AddSeconds(timestamp);

            // The dateTime now contains the right date/time so to format the string,
            // use the standard formatting methods of the DateTime object.
            string printDate = dateTime.GetDateTimeFormats()[104];
            MessageBox.Show(printDate);
        }

        private void button2_Click_1(object sender, RoutedEventArgs e)
        {
            WebRequest req_asana = WebRequest.Create("https://app.asana.com/api/1.0/tasks");
            HttpWebRequest httpreq_asana = (HttpWebRequest)req_asana;
            httpreq_asana.Method = "POST";

            httpreq_asana.ContentType = "application/json";
            httpreq_asana.Headers.Add("Authorization", "Basic TOKEN");
            Stream str_asana = httpreq_asana.GetRequestStream();
            StreamWriter strwriter_asana = new StreamWriter(str_asana, Encoding.ASCII);
            StringBuilder soaprequest_asana = new StringBuilder("{\"data\":{\"workspace\":1146396336466,\"name\":\"(# 2318) Alexi Schnell $149.00\",\"notes\":\"jkdgasgdasgdka\nsldfhsdhfsjdhfsdk\",\"assignee\":4441868495110}}");
            MessageBox.Show(soaprequest_asana.ToString());
            textBox1.Text = soaprequest_asana.ToString();
            strwriter_asana.Write(soaprequest_asana.ToString());
            strwriter_asana.Close();
            HttpWebResponse res_asana = (HttpWebResponse)httpreq_asana.GetResponse();
            StreamReader rdr_asana = new StreamReader(res_asana.GetResponseStream());
            string result_asana_task = rdr_asana.ReadToEnd();
            MessageBox.Show(result_asana_task);
        }

        private void workSpaceID_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox1_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }

        private void button3_Click_1(object sender, RoutedEventArgs e)
        {
            WebRequest req_big_order_count = WebRequest.Create(big_storeurl.Text + "orders/");
            HttpWebRequest httpreq_order_count = (HttpWebRequest)req_big_order_count;
            httpreq_order_count.Method = "GET";
            httpreq_order_count.ContentType = "text/xml; charset=utf-8";
                                httpreq_order_count.Credentials = new NetworkCredential(big_user.Text, big_pass.Text);
                    HttpWebResponse res_order = (HttpWebResponse)httpreq_order_count.GetResponse();
                   
                    StreamReader rdr_product_count = new StreamReader(res_order.GetResponseStream());
                    string result_order= rdr_product_count.ReadToEnd();
                    //textBox1.Text = result_order;

                    if (res_order.StatusCode == HttpStatusCode.OK || res_order.StatusCode == HttpStatusCode.Accepted)
                    {

                    }
                    
        }

      

      
    }
}
