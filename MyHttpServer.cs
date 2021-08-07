using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using RazorEngine;
using RazorEngine.Templating;

namespace exam_6
{
    class MyHttpServer
    {
        private Thread _serverThread;
        private string _siteDirectory;
        private string _filesDirectory;
        private HttpListener _listener;
        private int _port;
        public MyHttpServer(int port)
        {
            this.Initialize(port);
        }
        private void Initialize(int port)
        {
            string currentDir = Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).ToString()).ToString()).ToString();
            _siteDirectory = currentDir + @"\site";
            _filesDirectory = currentDir + @"\files";
            _port = port;
            _serverThread = new Thread(Listen);
            _serverThread.Start();
            Console.WriteLine($"Сервер запущен на порту: {port}");
            Console.WriteLine($"Файлы сайта лежат в папке: {_siteDirectory}");
            Console.WriteLine($"Файлы с данными лежат в папке: {_filesDirectory}");
        }
        public void Stop()
        {
            _serverThread.Abort();
            _listener.Stop();
        }
        private void Listen()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://localhost:" + _port.ToString() + "/");
            _listener.Start();
            while (true)
            {
                try
                {
                    HttpListenerContext context = _listener.GetContext();
                    Process(context);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
        private void Process(HttpListenerContext context)
        {
            string filename = context.Request.Url.AbsolutePath;
            Console.WriteLine(filename);
            filename = filename.Trim('/');
            string absFilename = Path.Combine(_siteDirectory, filename);
            string content;
            switch (filename)
            {
                case "index.html":
                    var tasks = FileReader.ReadFile<Task>(_filesDirectory + @"\tasks.json");
                    content = BuildHtml(absFilename, tasks);
                    break;
                default:
                    content = File.ReadAllText(absFilename);
                    break;
            }

            if (File.Exists(absFilename))
            {
                try
                {
                    byte[] htmlBytes = System.Text.Encoding.UTF8.GetBytes(content);
                    Stream fileStream = new MemoryStream(htmlBytes);
                    context.Response.ContentType = GetContentType(absFilename);
                    context.Response.ContentLength64 = fileStream.Length;
                    byte[] buffer = new byte[16 * 1024];
                    int dataLength;
                    do
                    {
                        dataLength = fileStream.Read(buffer, 0, buffer.Length);
                        context.Response.OutputStream.Write(buffer, 0, dataLength);
                    } while (dataLength > 0);
                    fileStream.Close();
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    context.Response.OutputStream.Flush();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }
            context.Response.OutputStream.Close();
        }
        private string ReadBodyFromPostRequest(HttpListenerRequest request)
        {
            string bodyText = string.Empty;
            if (request.HasEntityBody)
            {
                using (Stream body = request.InputStream) // here we have data
                {
                    using (var reader = new StreamReader(body, request.ContentEncoding))
                    {
                        bodyText = reader.ReadToEnd();
                    }
                }
            }
            return bodyText;
        }
        private string GetContentType(string filename)
        {
            var dictionary = new Dictionary<string, string> {
            {".css",  "text/css"},
            {".html", "text/html"},
            {".ico",  "image/x-icon"},
            {".js",   "application/x-javascript"},
            {".json", "application/json"},
            {".png",  "image/png"}
        };
            string contentType = String.Empty;
            string fileExtension = Path.GetExtension(filename);
            dictionary.TryGetValue(fileExtension, out contentType);
            return contentType;
        }
        private string BuildHtml<T>(string filename, T model)
        {
            string html = String.Empty;
            string layoutPath = Path.Combine(_siteDirectory, "layout.html");
            string filePath = Path.Combine(_siteDirectory, filename);
            var razorService = Engine.Razor;
            if (!razorService.IsTemplateCached("layout", null))
                razorService.AddTemplate("layout", File.ReadAllText(layoutPath));
            if (!razorService.IsTemplateCached(filename, typeof(T)))
            {
                razorService.AddTemplate(filename, File.ReadAllText(filePath));
                razorService.Compile(filename, typeof(T));
            }
            html = razorService.Run(filename, typeof(T), model);
            return html;
        }

    }
}
