using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using System.Text;
using System.IO;
using NetMf.CommonExtensions;



namespace NetduinoPlusCosmTempHumidity
{
    public class Program
    {

        //cosm/pachube identifying key
        private static string apiKey = "xLIm6-K3xyMYA89y5j5mR4Ba1FGSAKxldTZqWmpyMm9Xaz0g";
        //my feed
        private static string feedId = "50487";
        //sensor sleep for this amount of ms between posts
        //private static int frequency = 120000;//120000 is 2 minutes

        private static int frequency = 30000;//20000 is 20secs



        private static bool ledState = false;
        private static double humidity;
        private static double temperature;


        private static byte[] buffer;

        private static HttpWebRequest request;
        private static SHT21 sensor = new SHT21();//temp sensor
        //private static 
        private static OutputPort led;

        private static StringBuilder sample = new StringBuilder();
        private static StringBuilder message = new StringBuilder();

        public static void Main()
        {
            // write your code here
            //turn off garbage collection messages
            Debug.EnableGCMessages(false);
            Debug.Print("Cosm Connected humidity monitor started");

            string requestURI = "http://api.pachube.com/v2/feeds/" + feedId + ".csv";


            led = new OutputPort(Pins.ONBOARD_LED, ledState);



            //FlashLed(2);
            Thread.Sleep(2000);
            FlashLed(1);
            Thread.Sleep(2000);



            int index = 0;
            while (true)
            {

                humidity = sensor.GetHumidity();
                temperature = sensor.GetTemperature();

                //Debug.Print(temperature.ToString("N2"));

                sample.Clear();
                sample.Append("temperature,");
                sample.Append(temperature.ToString("N2"));
                sample.Append("\r\n");
                sample.Append("humidity,");

                sample.Append(humidity.ToString("N2"));

                Debug.Print(sample.ToString());
                sample.Append("\r\n");

                buffer = Encoding.UTF8.GetBytes(sample.ToString());

                using (request = (HttpWebRequest)WebRequest.Create(requestURI))
                {
                    request.Method = "PUT";
                    request.ContentType = "text/csv";
                    request.ContentLength = buffer.Length;
                    request.Headers.Add("X-PachubeApiKey", apiKey);

                    try
                    {
 
                        request.GetRequestStream().Write(buffer, 0, buffer.Length);


                        //create debugging message
                        message.Clear();
                        message.Append(index.ToString());
                        message.Append(": ");


                        
                        message.Append(DateTime.Now.ToString());
                        message.Append("  URI: ");
                        message.Append(requestURI);
                        Debug.Print(message.ToString());
                        //debugging message

                        //get the response, output it to debug window
                        using (var response = (HttpWebResponse)request.GetResponse())
                        {
                            Debug.Print("status: " + response.StatusCode + " : " + response.StatusDescription);
                        }

                        //flash LED twice to indicate success
                        FlashLed(2);
                        index++;
                    }
                    catch (Exception ex)
                    {
                        //flash LED 3 times to indicate problem
                        FlashLed(3);
                        //output problem message to debug window
                        Debug.Print(ex.Message);
                    }


                    Thread.Sleep(frequency);
                }
            }

        }


        private static void FlashLed(int count)
        {
            int index = 0;
            while (index < (count * 2))
            {
                // Sleep for 200 milliseconds
                Thread.Sleep(200);

                // toggle LED state
                ledState = !ledState;
                led.Write(ledState);
                index++;
            }
        }
    }
}
