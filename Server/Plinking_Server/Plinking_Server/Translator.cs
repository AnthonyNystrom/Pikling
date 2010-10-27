using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;

namespace Plinking_Server
{
    class Translator
    {
        /// <summary>
        /// Translates the text.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="languagePair">The language pair.</param>
        /// <returns></returns>
        public string TranslateText(string input, string languagePair)
        {
            String sResult = input;
            try
            {
                sResult = TranslateText(input, languagePair, System.Text.Encoding.UTF7);
            }
            catch (Exception )
            {
                Program.MainForm.AddLog("TranslateText Failed");
            }
            return sResult;
        }

        /// <summary>
        /// Translate Text using Google Translate
        /// </summary>
        /// <param name="input">The string you want translated</param>
        /// <param name="languagePair">2 letter Language Pair, delimited by "|". 
        /// e.g. "en|da" language pair means to translate from English to Danish</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns>Translated to String</returns>
        private string TranslateText(string input, string languagePair, Encoding encoding)
        {
            Program.MainForm.AddLog("Starting translator : " + languagePair);
            string url = String.Format("http://www.google.com/translate_t?hl=en&ie=UTF8&text={0}&langpair={1}", input, languagePair);

            string result = String.Empty;

            using (WebClient webClient = new WebClient())
            {
                webClient.Encoding = encoding;
                result = webClient.DownloadString(url);
            }

            Match m = Regex.Match(result, "(?<=<div id=result_box dir=\"ltr\">)(.*?)(?=</div>)");

            if (m.Success)
                result = m.Value;

            return result;
        }
    }
}
