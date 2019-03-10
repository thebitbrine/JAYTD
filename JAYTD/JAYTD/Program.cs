using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace JAYTD
{
    class Program
    {
        static void Main(string[] args)
        {
            Program p = new Program();
            if (args.Length > 0)
            {
                if (args[0].ToLower().Contains("?v="))
                {
                    string ID = args[0].Split(new string[] {"?v="}, StringSplitOptions.None).LastOrDefault();
                    if (!string.IsNullOrWhiteSpace(ID))
                    {
                        p.Run(ID);
                    }
                }
                else
                {
                    p.Run(args[0]);
                }
            }
            else
            {
                Console.WriteLine("Note: Songs or any other heavily copyrighted materials might not work.");
                Console.WriteLine("Usage: JAYTD.exe https://www.youtube.com/watch?v=dQw4w9WgXcQ");
                Console.WriteLine("Usage: JAYTD.exe dQw4w9WgXcQ");
            }
        }

        public void Run(string ID)
        {
            var Response = HttpUtility.UrlDecode(new WebClient().DownloadString($"http://www.youtube.com/get_video_info?video_id={ID}&ps=default&eurl=&gl=US&hl=en"));
            var Exploded = ExplodeURL(Response);
            
            List<Stream> Streams = new List<Stream>();
            Stream InnerStream = new Stream();
            foreach (var Stream in Exploded)
            {
                if (Stream.Key.ToLower().Contains("quality")) InnerStream.Quality = Stream.Value.Replace("\"", "");
                if (Stream.Key.ToLower().Contains("type")) InnerStream.Type = Stream.Value.Replace("\"", "");
                if (Stream.Key.ToLower().Contains("url") && Stream.Value.ToLower().Contains("googlevideo") && Streams.Find(x => x.URL == Stream.Value) == null)
                {
                    InnerStream.URL = Stream.Value;
                    Streams.Add(InnerStream);
                    InnerStream = new Stream();
                }
            }
            Console.WriteLine(FormatJson(SimpleJson.SerializeObject(Streams, new PocoJsonSerializerStrategy())));
        }
        
        public string FormatJson(string json)
        {
            const string indentString = " ";
            int indentation = 0;
            int quoteCount = 0;
            var result =
                from ch in json
                let quotes = ch == '"' ? quoteCount++ : quoteCount
                let lineBreak = ch == ',' && quotes % 2 == 0 ? ch + Environment.NewLine + string.Concat(Enumerable.Repeat(indentString, indentation)) : null
                let openChar = ch == '{' || ch == '[' ? ch + Environment.NewLine + string.Concat(Enumerable.Repeat(indentString, ++indentation)) : ch.ToString()
                let closeChar = ch == '}' || ch == ']' ? Environment.NewLine + string.Concat(Enumerable.Repeat(indentString, --indentation)) + ch : ch.ToString()
                select lineBreak ?? (openChar.Length > 1 ? openChar : closeChar);

            return string.Concat(result);
        }

        public List<KeyValuePair<string, string>> ExplodeURL(string URL)
        {
            List<KeyValuePair<string, string>> List = new List<KeyValuePair<string, string>>();
            var Exploded = URL.Split(',');
            foreach (var _Part in Exploded)
            foreach (var Part in _Part.Split('&'))
            {
                if(Part.Contains("url") || Part.Contains("type") || Part.Contains("quality"))
                    List.Add(new KeyValuePair<string, string>(HttpUtility.UrlDecode(Part.Split('=')[0]), Part.Contains('=') ? (!Part.EndsWith("=") ? HttpUtility.UrlDecode(Part.Split('=')[1]) : "" ) : (!Part.EndsWith(":") ? HttpUtility.UrlDecode(Part.Split(':')[1]) : "")));
            }

            return List;
        }
        
        public class Stream
        {
            public string Quality;
            public string Type;
            public string URL;
        }
    }
}
