using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace ATCSUblScreenshotCapture
{
    class Program
    {
       static MySqlConnection conn = new MySqlConnection("");
        private static Bitmap bitmap2;
        static void Main(string[] args)
        {


            Console.WriteLine("esc to close..");
            do
            {
                while (!Console.KeyAvailable)
                {
                    processing();
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
        }

        private static void processing()
        {
            try
            {
                int cctvcode = 0;
                string[] lines = System.IO.File.ReadAllLines(@"config.txt");
                foreach (string line in lines)
                {
                    string[] words = line.Split('|');
                    string directory = words[0];
                    string imagelink = words[1];
                    Console.WriteLine(directory);
                    Console.WriteLine(imagelink);

                    try
                    {
                        getimagefromurl(cctvcode,imagelink, "ATCS/screenshoot/"+directory+"/",directory);
                    }
                    catch (Exception)
                    {

                        Console.WriteLine("proccess failed please check connection");
                    }

                }
            }
            catch (Exception)
            {

                Console.WriteLine("file not found");
            }
           

            
          
            Console.WriteLine("Sleep......................................................................");
            Console.WriteLine("esc to close..");
            Thread.Sleep(5000);
        }

        private static void getimagefromurl(int cctv_code, String url, String path, String tempname)
        {

            System.Net.WebRequest request = System.Net.WebRequest.Create(url);
            request.Credentials = new NetworkCredential("admin", "12345");


            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    bitmap2 = new Bitmap(responseStream);
                }
            }

            Console.WriteLine("success getting image..");
            uploadtoftp(cctv_code, bitmap2, path, tempname);

        }
        private static void uploadtoftp(int cctv_code, Bitmap image, String path, String tempname)
        {

            string filename = tempname + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".jpg";
            // Get the object used to communicate with the server.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://167.205.7.226:60328/" + path + filename);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // This example assumes the FTP site uses anonymous logon.
            request.Credentials = new NetworkCredential("edo", "Admin@123");

            // Copy the contents of the file to the request stream.
            byte[] fileContents = ImageToByte2(image);

            request.ContentLength = fileContents.Length;

            Stream requestStream = request.GetRequestStream();
            requestStream.Write(fileContents, 0, fileContents.Length);
            requestStream.Close();

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);

            response.Close();
            Console.WriteLine("Success Upload Image to FTP");
            uploadtodb(cctv_code, path, filename);
        }
        public static byte[] ImageToByte2(Image img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }
        private static void uploadtodb(int cctv_code, string path, string name)
        {
            using (conn)
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "INSERT INTO atcs_screenshot (path, name, cctv_code) VALUES('" + path + "', '" + name + "', " + cctv_code + ")";
                cmd.Prepare();
                cmd.ExecuteNonQuery();
                //Execute command
                cmd.ExecuteNonQuery();

                //close connection
            }
            Console.WriteLine("Success Upload Image to DB");
        }

    }
}
