using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Plinking_Server
{
    public class DataProc
    {
        String _sFolder="";
        String _sFileBmp="";
        String _sFileJpg = "";
        String _sresultOcrLangSource = "";
        String _sresultOcrLangDest = "";
        String _sLang;
        bool _bProcessCompleted = false;
        bool _bCanDelete = false;
        byte _byTranslator;
        public DataProc(String sFolder, String sFileBmp, String sFileJpg, String sLang)
        {   _sFolder = sFolder;
            _sFileBmp = sFileBmp;
            _sFileJpg = sFileJpg;
            _sLang = sLang;
        }
        public bool getProcessCompleted()
        {   return _bProcessCompleted;
        }
        public void setProcessCompleted(String sResultOcrLangSource, String sResultOcrLangDest)
        {   _bProcessCompleted = true;
            _sresultOcrLangSource = sResultOcrLangSource;
            _sresultOcrLangDest = sResultOcrLangDest;
        }
        public void getResultOcr(ref String sResultOcrLangSource, ref String sResultOcrLangDest)
        {
            sResultOcrLangSource = _sresultOcrLangSource;
            sResultOcrLangDest = _sresultOcrLangDest;
        }
        public String getFolder()
        {   return _sFolder;
        }
        public String getFileBmp()
        {   return _sFileBmp;
        }
        public String getFileJpg()
        {
            return _sFileJpg;
        }
        public void setCanDelete(){
            _bCanDelete=true;
        }
        public bool getCanDelete(){
            return _bCanDelete;
        }
        public String getLang()
        { return _sLang;
        }
        public String getLangFrom()
        {
            int iIndex = _sLang.LastIndexOf('|');
            return _sLang.Substring(0, iIndex);
        }
        public String getLangTo()
        {
            int iIndex = _sLang.LastIndexOf('|');
            return _sLang.Substring(iIndex + 1, _sLang.Length-(iIndex+1));
        }
        public void setTranslator(byte byTranslator)
        {
            _byTranslator = byTranslator;
        }
        public byte getTranslator()
        {
            return _byTranslator;
        }
    }
}
