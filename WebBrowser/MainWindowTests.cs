using NUnit.Framework;
using System.IO;
using System;
using System.Net;

namespace HelloWorldTests
{
    public class Tests
    {
        private const string Expected = "Hello World!";

        public String createHttpRequest(String url)
        {
            // create request object
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            // enable decompressoin
            request.AutomaticDecompression = DecompressionMethods.GZip;
            HttpWebResponse response;
            String statusCode;
            try
            {
                // get the response
                response = (HttpWebResponse)request.GetResponse();
                // get the status code
                statusCode = response.StatusCode.ToString();
            }
            catch (WebException we)
            {
                // error handling
                statusCode = ((HttpWebResponse)we.Response).StatusCode.ToString();
            }

            return statusCode;
        }

        [Test]
        public void HttpRequestOKTest()
        {
            String ok = createHttpRequest("https://www.google.com");
            Assert.AreEqual("OK", ok);
        }
        
        [Test]
        public void HttpRequestErrorsTests()
        {
            String badRequest = createHttpRequest("https://httpstat.us/400");
            String forbidden = createHttpRequest("https://httpstat.us/403");
            String notFound = createHttpRequest("https://httpstat.us/404");
            String internalServerError = createHttpRequest("https://httpstat.us/500");
            String badGateway = createHttpRequest("https://httpstat.us/502");
            String serviceUnavailable = createHttpRequest("https://httpstat.us/503");
            Assert.AreEqual("BadRequest", badRequest);
            Assert.AreEqual("Forbidden", forbidden);
            Assert.AreEqual("NotFound", notFound);
            Assert.AreEqual("InternalServerError", internalServerError);
            Assert.AreEqual("BadGateway", badGateway);
            Assert.AreEqual("ServiceUnavailable", serviceUnavailable);
        }
        
        [Test]
        public void FileReadTest()
        {
            using (var sw = new StringWriter())
            {
                StreamReader reader = new StreamReader(System.Environment.CurrentDirectory + "/readTest.txt");
                String expected = "Read test!";
                Assert.AreEqual(expected, "Read test!");
            }
        }
        
        [Test]
        public void FileWriteTest()
        {
            using (var sw = new StringWriter())
            {
                // Delete file if it exists before testing
                File.Delete(System.Environment.CurrentDirectory + "/test.txt");
                
                StreamWriter writer = new StreamWriter(System.Environment.CurrentDirectory + "/test.txt", true);
                StreamReader reader = new StreamReader(System.Environment.CurrentDirectory + "/test.txt");
                String expected = "Testing read/write for files!";
                writer.WriteLine(expected);
                writer.Flush();
                String actual = reader.ReadLine();
                Assert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void FileEditTest()
        {
            // Delete file if it exists before testing
            File.Delete(System.Environment.CurrentDirectory + "/testFile1.txt");
            
            StreamWriter writer1 = new StreamWriter(System.Environment.CurrentDirectory + "/testFile1.txt", true);
            writer1.WriteLine("Output 1");
            writer1.Flush();
            StreamWriter writer2 = new StreamWriter(System.Environment.CurrentDirectory + "/testFile2.txt", true);
            writer2.WriteLine("Output 2");
            writer2.Flush();
            StreamReader reader = new StreamReader(System.Environment.CurrentDirectory + "/testFile1.txt");
            String beforeEdit = reader.ReadLine();
            File.Replace(System.Environment.CurrentDirectory + "/testFile2.txt", System.Environment.CurrentDirectory + "/testFile1.txt", System.Environment.CurrentDirectory + "/testFileBackup.txt");
            File.Delete(System.Environment.CurrentDirectory + "/testFileBackup.txt");
            reader = new StreamReader(System.Environment.CurrentDirectory + "/testFile1.txt");
            String afterEdit = reader.ReadLine();
            Assert.AreEqual("Output 1", beforeEdit);
            Assert.AreEqual("Output 2", afterEdit);
        }
        
    }
}