using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Leadtools.Forms.Ocr;
using Leadtools.Forms.DocumentWriters;
using Leadtools.Codecs;
using Leadtools;
using System.Globalization;
using Leadtools.Forms;

namespace Plinking_Server
{
    class LeadWorker
    {
        private String _sFolder;
        private String _sImgFile;
        private String _sLang;
        // The current OCR document
        private IOcrDocument _ocrDocument;
        private IOcrEngine _ocrEngine;
        private RasterCodecs _codecs;

        public LeadWorker(IOcrEngine ocrEngine, RasterCodecs codecs)
        {
            _ocrEngine = ocrEngine;
            _codecs = codecs;
        }
        ~LeadWorker()
        {
            //if (_ocrDocument != null)
            //{
            //    _ocrDocument.Dispose();
            //    _ocrDocument = null;                
           // }
        }

        /// <summary>
        /// Start a new worker thread for ocr decode
        /// </summary>
        /// <param name="sImg">Image file path. Only bmp files</param>
        /// <param name="sLang">Language source es:IT EN FR..</param>
        public String Start(String sFolder, String sImg, String sLang)
        {
            String sRet="";
            try
            {
                sLang = sLang.ToLower();
                _sImgFile = sImg;
                if (sLang.ToLower() == "zh")
                    sLang = "zh-Hant";
                _sLang = sLang;
                _sFolder = sFolder;

                _ocrEngine.LanguageManager.EnableLanguages(new string[] { sLang });
                string[] enabledLanguages = _ocrEngine.LanguageManager.GetEnabledLanguages();
                Console.WriteLine("Current enabled languages in the engine are:");
                foreach (string enabledLanguage in enabledLanguages)
                {
                    // Get the friendly name of this language using the .NET CultureInfo class 
                    CultureInfo ci = new CultureInfo(enabledLanguage);
                    Console.WriteLine("  {0} ({1})", enabledLanguage, ci.EnglishName);
                }
                // spell check
                IOcrSpellCheckManager spellCheckManager = _ocrEngine.SpellCheckManager;
                // Get the spell language supported (languages with a dictionary) 
                string[] spellLanguages = spellCheckManager.GetSupportedSpellLanguages();
                foreach (string spellLanguage in spellLanguages)
                    Console.WriteLine(spellLanguage);
                if (spellCheckManager.IsSpellLanguageSupported(sLang))
                {
                    // Yes, set it 
                    spellCheckManager.SpellLanguage = sLang;
                    spellCheckManager.Enabled = true; 
                    Console.WriteLine("Current spell language: {0}", spellCheckManager.SpellLanguage);
                }
                else
                    spellCheckManager.Enabled = false;

                

                _ocrDocument = _ocrEngine.DocumentManager.CreateDocument();

                RasterImage image = _codecs.Load(sImg);

                _ocrDocument.Pages.Clear();
                _ocrDocument.Pages.AddPage(image, null);
                _ocrDocument.Pages.AutoZone(null);
                _ocrDocument.Pages.UpdateFillMethod();
                sRet = Worker();
            }
            catch (Exception ex)
            {
                Program.MainForm.AddLog(String.Format("Exception Worker:{0}", ex.Message));
            }
            return sRet;
        }
        /// <summary>
        /// Worker Ocr
        /// </summary>
        String Worker()
        {
            _ocrDocument.Pages[0].Recognize(null);            
            //_ocrDocument.Save(_sFolder + "\\src.html", DocumentFormat.Html, null);
            IOcrPageCharacters ocrPageCharacters = _ocrDocument.Pages[0].GetRecognizedCharacters();
            String sRet = "";
            foreach (IOcrZoneCharacters zoneCharacters in ocrPageCharacters)
            {
                ICollection<OcrWord> recogWords = zoneCharacters.GetWords(_ocrDocument.Pages[0].DpiX, _ocrDocument.Pages[0].DpiY, LogicalUnit.Pixel);
                foreach (OcrWord word in recogWords)
                {
                    Console.WriteLine("word {0}", word.Value);
                    sRet += word.Value+" ";
                }
            }
            return sRet;
            //return _ocrDocument.Pages[0].RecognizeText(null);
        }
            
    }
}
