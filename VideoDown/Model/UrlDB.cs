namespace VideoDown.Model
{
    public class UrlDB
    {
        public string Url { get; set; }
        public string TypeCode { get; set; }
        public string TypeName { get; set; }
        public string SavePath { get; set; }
        public string FileName { get; set; }
        public string ID { get; set; }
        public string Explain { get; set; }
        public string DownUrl { get; set; }
        public string DownUrl1 { get; set; }
    }

    public class UrlDBShow : UrlDB
    {
        public bool Check { get; set; } = false;
        public int Num { get; set; }
    }
}