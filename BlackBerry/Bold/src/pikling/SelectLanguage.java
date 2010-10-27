package pikling;

import java.io.InputStream;
import java.io.InputStreamReader;
import java.util.Vector;

import net.rim.device.api.system.Bitmap;
import net.rim.device.api.system.Display;
import net.rim.device.api.ui.*;
import net.rim.device.api.ui.component.BitmapField;
import net.rim.device.api.ui.component.LabelField;
import org.w3c.dom.Document;
import org.w3c.dom.NodeList;
import org.w3c.dom.Node;
import net.rim.device.api.ui.container.MainScreen;
import net.rim.device.api.xml.parsers.DocumentBuilder;
import net.rim.device.api.xml.parsers.DocumentBuilderFactory;

public class SelectLanguage extends net.rim.device.api.ui.Manager{
	Bitmap _backgr;
	boolean _bShowed;
	PiklingScreen _hs;
	Font _fnt;
	int _iHFlags, _iWFlags;
	Vector _Languages = new Vector();
	Field _PushField;
	String _sLangSelected="";
	
	protected SelectLanguage(long style, MainScreen hs) {		
		super(style);
		_hs = (PiklingScreen)hs;
		_backgr = Bitmap.getBitmapResource("background2.png");		
		//_fnt = getFont("BBMillbank",16);
		
		LabelField lf = new LabelField();
		FontFamily theFam=null;
		try {
			theFam = FontFamily.forName(getFont().getFontFamily().getName());
			_fnt = theFam.getFont(Font.PLAIN, 18);
		} catch (ClassNotFoundException e) {
		}
        

		
		LoadXML();
		int i;
		int iLangs =_Languages.size();
		String sPng="";
		for (i=0; i<iLangs; i++){
			DataLang dLang = (DataLang)_Languages.elementAt(i);
			sPng = dLang._sAbbrev+".png";
			Bitmap bmp=Bitmap.getBitmapResource(sPng);
			if (bmp!=null){
				MyBitmapField bmf = new MyBitmapField(bmp, BitmapField.FOCUSABLE | BitmapField.FIELD_HCENTER | BitmapField.FIELD_VCENTER, dLang._sAbbrev)
				{
					protected void MyClicked(String sTag){
						_sLangSelected=sTag;
						if (_hs._mainframe==_PushField)
							_hs._mainframe.UpdateBmpLang(sTag);
						else if (_hs._settings==_PushField)
							_hs._settings.UpdateBmpLang(sTag);
						Hide();
					}
				};
				_iHFlags = bmf.getBitmapHeight();
				_iWFlags = bmf.getBitmapWidth();
				add(bmf);
				LabelField txt = new LabelField(dLang._sEnglish);
				txt.setFont(_fnt);
				add(txt);
			}
			else
				System.out.println("Miss png:"+sPng);
		}
		
	}
	
	void LoadXML(){
		try {
			Class classs = Class.forName("pikling.Pikling");
			InputStream is = classs.getResourceAsStream("/languages.xml");
			InputStreamReader isr = new InputStreamReader(is);
			DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
		    DocumentBuilder builder = factory.newDocumentBuilder();
		    Document doc = builder.parse(is);
		    NodeList list=doc.getElementsByTagName("abbreviation");
		    
		    int iNItems = list.getLength();
		    for (int i=0;i<iNItems;i++){
                Node value=list.item(i).getChildNodes().item(0);
		    	DataLang dLang = new DataLang();
		    	_Languages.addElement(dLang);
		    	dLang._sAbbrev =value.getNodeValue(); 
            }
		    
		    list=doc.getElementsByTagName("english");
		    iNItems = list.getLength();
		    for (int i=0;i<iNItems;i++){
                Node value=list.item(i).getChildNodes().item(0);
                DataLang dLang = (DataLang)_Languages.elementAt(i);
		    	dLang._sEnglish =value.getNodeValue(); 
            }
		    list=doc.getElementsByTagName("original");
		    iNItems = list.getLength();
		    for (int i=0;i<iNItems;i++){
                Node value=list.item(i).getChildNodes().item(0);
                DataLang dLang = (DataLang)_Languages.elementAt(i);
		    	dLang._sOriginal =value.getNodeValue(); 
            }
		    
			isr.close();
			is.close();			
	    }
	    catch (Exception e) {
	 
	    }
	}
	
	public void paint(Graphics graphics)
    {	      
		graphics.drawBitmap(0, 0, _backgr.getWidth(), _backgr.getHeight(), _backgr, 0, 0);
		super.paint(graphics);
    }
	protected void sublayout(int width, int height)
    {	
		Field field;
    	int iPad=10;
    	int x=0,y=0, iNCol=6;
    	int iStepX, iStepY;
    	int iWScreen = Display.getWidth();
    	iStepX = iWScreen/iNCol;
    	int iNFields= getFieldCount()/2;
    	if (iNCol>iNFields)
    		iNCol=iNFields;
    	float fNRow = ((float)iNFields/iNCol)+0.5f;
    	int iNRow   = (int)fNRow;
    	int iLastRow = iNFields%iNCol;
    	iStepY= Display.getHeight()/iNRow;
    	
    	System.out.println("iNRow:"+iNRow);
    	System.out.println("iNCol:"+iNCol);
    	
    	int i, ii, iField=0;
    	for (ii=0;ii<iNRow;ii++){
    		if (ii+1>=iNRow)
    			iNCol = iLastRow;
    		for(i=0;i<iNCol;i++, iField++){
    			field= getField(iField);
    			if (field!=null){
	    			
	    			x=i*iStepX+iPad;
	    			y=ii*iStepY;
	    			if (ii==0)
	    				y+=iPad;
	    			setPositionChild(field,x,y);
	                layoutChild(field, width, height);
	                
	                iField++;
	    			field= getField(iField);
	    			if (field!=null){
		    			y+=_iHFlags+5;
		    			LabelField f = (LabelField)field;
		    			int iC = _fnt.getAdvance(f.getText())/2; 
		                x+=(_iWFlags/2-iC);
		    			setPositionChild(field,x,y);
		                layoutChild(field, width, height);		                		                		                		               
	    			}
    			}
    		}
    	}
    			
		setExtent(width,height);
    }
	public boolean isShowed()
    {
      return _bShowed;
    }
	public void Show(){
		_bShowed=true;
		_PushField = _hs.getField(0);
		_hs.delete(_PushField);
		_hs.add(this);
	}
	public void Hide(){
		_bShowed=false;
		_hs.delete(this);
		_hs.add(_PushField);
	}
	public boolean MykeyDown( int keycode, int time ) 
	{
		boolean bret=false;
	    switch (keycode)
	    {
	    	case 1769472: //back butt.
	    		Hide();bret=true;break;
	     }
	     return bret;
	}
	public Font getFont(String sFontName, int iSize) 
    {
      try 
      {  FontFamily theFam = FontFamily.forName(sFontName);   
         return theFam.getFont(Font.PLAIN, iSize);
      } 
      catch (ClassNotFoundException ex) {
         ex.printStackTrace();
      }
      return null;
    }
	
	class DataLang{
		String _sAbbrev;
		String _sEnglish;
		String _sOriginal;
	}
}
