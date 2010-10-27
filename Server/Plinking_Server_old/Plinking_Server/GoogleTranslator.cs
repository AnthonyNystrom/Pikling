using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using System.Web;

namespace Plinking_Server
{
    public class TranslateDir
    {
        public TranslateDir() { }
        public TranslateDir(string from, string to)
        {
            _from = from;
            _to = to;
        }

        public string from
        {
            get { return _from; }
            set { _from = value; }
        }
        public string to
        {
            get { return _to; }
            set { _to = value; }
        }
        protected string _from, _to;

    }

    interface Translator
    {
        bool CanTranslate(TranslateDir dir);
        void SetDirection(TranslateDir dir);
        string Translate(string text);
        string Translate(string text, TranslateDir dir);
    }

    public abstract class BaseTranslator : Translator
    {
        public bool CanTranslate(TranslateDir dir)
        {
            String s;
            if (((s = Translate("Hello", new TranslateDir("en", dir.from))) != "") &&
                (Translate(s, new TranslateDir(dir.from, dir.to)) != "")) return true;
            else return false;
        }

        public void SetDirection(TranslateDir dir)
        {
            CurrentDirection = dir;
        }

        public string Translate(string text)
        {
            return Translate(text, CurrentDirection);
        }

        public string Translate(string text, TranslateDir dir)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(TranslateUrl);

            // Encode the text to be translated
            string postSourceData = GetPostSourceData(text, dir);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postSourceData.Length;
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1)";

            HttpWebResponse response;

            try
            {
                using (Stream writeStream = request.GetRequestStream())
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(postSourceData);
                    writeStream.Write(bytes, 0, bytes.Length);
                    writeStream.Close();
                }
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception)
            {
                throw new Exception("Couldn't connect to the translation web service");
            }
            StreamReader readStream = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string page = readStream.ReadToEnd();
            response.Close();

            Regex reg = new Regex(RegexpResult, RegexOptions.IgnoreCase);
            Match m = reg.Match(page);
            string s;
            if (m.Success)
            {
                s = m.Result("${text}");
            }
            else throw new Exception("Couldn't parse a web service response. Please, update software");

            return s;
        }

        protected abstract string GetPostSourceData(string text, TranslateDir dir);
        protected abstract string TranslateUrl { get; }
        protected abstract string RegexpResult { get; }

        protected TranslateDir CurrentDirection;
    }

    public class GoogleTranslator : BaseTranslator
    {
        protected override string GetPostSourceData(string text, TranslateDir dir)
        {
            return string.Format("hl={0}&ie=UTF8&text={1}&sl={2}&tl={3}", dir.from, HttpUtility.UrlEncode(text), dir.from, dir.to);
        }

        protected override string RegexpResult
        {
            get
            {
                return @"<div[^>]+id=result_box[^>]+>(?<text>[^>]+)</div>";
            }
        }

        protected override string TranslateUrl { get { return "http://translate.google.com/translate_t"; } }
    }
    public class TranslatedTranslator : BaseTranslator
    {
        protected override string GetPostSourceData(string text, TranslateDir dir)
        {
            return string.Format("f=mt&s={0}&t={1}&text={2}&cid={3}&p={4}",
                dir.from, dir.to, HttpUtility.UrlEncode(text), @"a.nystrom@genetibase.com", "NnZgBxUzeh");

        }
        protected override string RegexpResult
        {
            get
            {
                return "[0-1]\nOK\n[0-9]+\n(?<text>[^>]+)\n\r\n\r\n";
            }
        }
        protected override string TranslateUrl { get { return "http://www.translated.net/hts/"; } }
    }
}
