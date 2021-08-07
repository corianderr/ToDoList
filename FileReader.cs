using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace exam_6
{
    class FileReader
    {
        public static List<T> ReadFile<T>(string path)
        {
            try
            {
                string info = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<List<T>>(info);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"File do not read {path}.\nError: {e.Message}");
                return new List<T>();
                throw;
            }
            catch (JsonReaderException e)
            {
                Console.WriteLine($"Error file to path: {path}.\nError: {e.Message}");
                return new List<T>();
            }
            catch (Exception e)
            {
                Console.WriteLine($"An initialization error: {e.Message}");
                return new List<T>();
            }
        }
    }
}
