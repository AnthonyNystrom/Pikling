package pikling;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.IOException;

import javax.microedition.pim.PIMException;
import javax.microedition.rms.RecordEnumeration;
import javax.microedition.rms.RecordStore;
import javax.microedition.rms.RecordStoreException;
import javax.microedition.rms.RecordStoreNotFoundException;
import net.rim.blackberry.api.pdap.BlackBerryContact;
import net.rim.blackberry.api.pdap.BlackBerryContactList;
import net.rim.blackberry.api.pdap.BlackBerryPIM;
import net.rim.device.api.system.Bitmap;
import net.rim.device.api.system.Display;
import net.rim.device.api.ui.Field;
import net.rim.device.api.ui.Font;
import net.rim.device.api.ui.FontFamily;
import net.rim.device.api.ui.Graphics;
import net.rim.device.api.ui.UiApplication;
import net.rim.device.api.ui.component.BitmapField;
import net.rim.device.api.ui.component.Dialog;
import net.rim.device.api.ui.component.LabelField;
import net.rim.device.api.ui.container.MainScreen;


public class Settings extends net.rim.device.api.ui.Manager {
	Bitmap _backgr;
	public static PiklingScreen _hs;

	public String _sLangSrc="", _sLangDst="", _sEmail="", _sNumMob="", _sTwitterUsr="", _sTwitterPwd="";
	public boolean _bReqEmail;
	
	String _sFileNameSettings = "pikling.cfg";
	boolean _bShowed, _bUpadteLangSrc;
	FlagField _bmFieldLangDest, _bmFieldLangSrc, _bmOnOff;
	
	int _iLenLangStr, _iLenSendTo;
	String _sEmailContact="", _sMobileNumberContact="";
	CustomTextBox _EditNumMob;
	CustomTextBox _EditEmail;
	
	Field _PushField;
	LabelField _LabelLangSrc, _LabelLangDst; 
	String _sTmpSrcLang, _sTmpDstLng;
	boolean _bTmpReqEmail;
	String _sMsgEmail="You can't use Pikling without a valid email address";
	TwitterSettings _tw;
	WaitComeBack _wcb;
	
	protected Settings(long style, MainScreen hs) {
		super(style);
		_hs = (PiklingScreen)hs;
		try{
			DefaultValues();
			Load();
			_backgr = Bitmap.getBitmapResource("background2.png");
			
			BitmapField bmp = new BitmapField(Bitmap.getBitmapResource("title_settings.png"), BitmapField.FIELD_HCENTER | BitmapField.FIELD_VCENTER);
			add(bmp);
			bmp = new BitmapField(Bitmap.getBitmapResource("field.png"), BitmapField.FIELD_HCENTER | BitmapField.FIELD_VCENTER);
			add(bmp);
			bmp = new BitmapField(Bitmap.getBitmapResource("field.png"), BitmapField.FIELD_HCENTER | BitmapField.FIELD_VCENTER);
			add(bmp);
			_bmFieldLangSrc = new FlagField(BitmapField.FOCUSABLE | BitmapField.FIELD_LEFT, _sLangSrc.toLowerCase())
			{
				protected boolean navigationClick(int status,  int time)
				{	try 
					{	PiklingScreen ps = (PiklingScreen)_hs;
						_bUpadteLangSrc = true;
						ps.ShowLanguage();
					} 
					catch (Exception e) 
					{	System.out.println("Exception:"+e.getMessage()); 
	                }
	                return true;
				}
			};
			add(_bmFieldLangSrc);
			_bmFieldLangDest = new FlagField(BitmapField.FOCUSABLE | BitmapField.FIELD_LEFT, _sLangDst.toLowerCase())
			{
				protected boolean navigationClick(int status,  int time)
				{	try 
					{	PiklingScreen ps = (PiklingScreen)_hs;
						_bUpadteLangSrc = false;
						ps.ShowLanguage();
					} 
					catch (Exception e) 
					{	System.out.println("Exception:"+e.getMessage()); 
	                }
	                return true;
				}
			};
			add(_bmFieldLangDest);			
			
			/*FontFamily theFam = FontFamily.forName("BBAlpha Sans");   
	        Font fnt = theFam.getFont(Font.BOLD, 30);
	        Font fnt_thin = theFam.getFont(Font.PLAIN, 22);*/
	        
				        
			_LabelLangSrc = new LabelField(_sLangSrc.toUpperCase());

			FontFamily theFam = FontFamily.forName(_LabelLangSrc.getFont().getFontFamily().getName());   
	        Font fnt = theFam.getFont(Font.BOLD, 30);
	        Font fnt_thin = theFam.getFont(Font.PLAIN, 22);
			
			_LabelLangSrc.setFont(fnt);
			add(_LabelLangSrc);
			_LabelLangDst = new LabelField(_sLangDst.toUpperCase());
			_LabelLangDst.setFont(fnt);
			add(_LabelLangDst);
			LabelField txt = new LabelField("Source");
			//txt.setFont(fnt_thin);
			add(txt);
			txt = new LabelField("Dest.");
			//txt.setFont(fnt_thin);
			add(txt);
			String sL="Languages";
			txt = new LabelField(sL);
			_iLenLangStr = fnt_thin.getAdvance(sL)/2;
			txt.setFont(fnt_thin);
			add(txt);
			
			bmp = new BitmapField(Bitmap.getBitmapResource("field.png"), BitmapField.FIELD_HCENTER | BitmapField.FIELD_VCENTER);
			add(bmp);
			String sBmp="";
			if (_bReqEmail)
				sBmp="on";
			else
				sBmp="off";
			_bmOnOff = new FlagField(BitmapField.FOCUSABLE | BitmapField.FIELD_HCENTER | BitmapField.FIELD_VCENTER, sBmp)
			{
				protected boolean navigationClick(int status,  int time)
				{
					_bTmpReqEmail=!_bTmpReqEmail;
					String sBmp;
					if (_bTmpReqEmail)
						sBmp="on.png";
					else
						sBmp="off.png";
					_bmOnOff.setBitmap(Bitmap.getBitmapResource(sBmp));
					return true;
				}
			};
			add(_bmOnOff);
			
			String sT="Send To";
			txt = new LabelField(sT);
			_iLenSendTo = fnt_thin.getAdvance(sT)/2;
			txt.setFont(fnt_thin);
			add(txt);
			txt = new LabelField("Ask Before");
			//txt.setFont(fnt_thin);
			add(txt);
			
			_EditNumMob = new CustomTextBox(213, "Mobile Numb.", _sNumMob)
			{
				protected boolean navigationClick(int status,  int time)
				{	ShowAddressBook();
					if (_sMobileNumberContact.compareTo("")!=0)
						_EditNumMob.setText(_sMobileNumberContact);					
					return true;
				}				
			};			
			add(_EditNumMob);
			
			_EditEmail = new CustomTextBox(282, "Email", _sEmail)
			{
				protected boolean navigationClick(int status,  int time)
				{
					ShowAddressBook();
					if (_sEmailContact.compareTo("")!=0)
						_EditEmail.setText(_sEmailContact);
					return true;
				}
			};
			add(_EditEmail);			
			
			FlagField bmpf = new FlagField(BitmapField.FOCUSABLE | BitmapField.FIELD_HCENTER | BitmapField.FIELD_VCENTER, "twitter_small")
			{
				protected boolean navigationClick(int status,  int time)
				{
					_tw = new TwitterSettings(MainLayout.NO_VERTICAL_SCROLLBAR | MainLayout.NO_VERTICAL_SCROLL | MainLayout.USE_ALL_WIDTH | MainLayout.USE_ALL_HEIGHT, _hs, _sTwitterUsr, _sTwitterPwd);
					_tw.Show();
					_wcb = new WaitComeBack();
			   		UiApplication.getUiApplication().invokeLater(new Runnable()
			   	    {
			   			public void run()
			   	        {	
			   				_wcb.start();
			   	        }
			   	    });
					return true;
				}
			};
			add(bmpf);
			
			bmpf = new FlagField(BitmapField.FOCUSABLE | BitmapField.FIELD_HCENTER | BitmapField.FIELD_VCENTER, "accept")
			{
				protected boolean navigationClick(int status,  int time)
				{
					if (_sEmail=="" || _sEmail.indexOf(".")<0 || _sEmail.indexOf("@")<0){
		          	  Dialog.alert(_sMsgEmail);
		          	  
		          	  UiApplication.getUiApplication().invokeLater(new Runnable()
			   	      {public void run(){System.exit( 1 );}});
		            }
					else{
						_sLangSrc=_sTmpSrcLang;
						_sLangDst=_sTmpDstLng;
						_bReqEmail=_bTmpReqEmail;
						_sNumMob = _EditNumMob.getText();
						_sEmail = _EditEmail.getText();
						_hs._mainframe.UpdateSrcBmpLang(_sLangSrc);
						_hs._mainframe.UpdateDstBmpLang(_sLangDst);
						Save();
						Hide();
					}
					return true;
				}
			};
			add(bmpf);
			
			_sTmpDstLng=_sLangSrc;
			_sTmpDstLng=_sLangDst;
			_bTmpReqEmail=_bReqEmail;

		}
		catch(Exception ex){
			PiklingScreen.ShowBlackMessage("Exception:"+ex.getMessage(), true);
		}
		
	}
	protected void sublayout(int width, int height)
    {	Field field;
    	int x=0,y=0;
    	int iPad = 10;
		for (int i = 0;  i < getFieldCount();  i++) {
			field= getField(i);
			switch (i){
			case 0:// title
				break;
			case 1://field sx
				x=iPad;
				y=80;
				break;
			case 2://field dx
				x=250;
				y=80;
				break;
			case 3://flag src
				x=160;
				y=82;
				break;
			case 4://flag dst
				x=400;
				y=82;
				break;
			case 5:// lang src abbrev
				x=120;y=90;
				break;
			case 6:// lang dst abbrev
				x=360;
				break;
			case 7:// label source
				x=iPad*2;
				y=95;
				break;
			case 8:// label dest
				x=260;				
				break;
			case 9:// label language
				x=Display.getWidth()/2-_iLenLangStr;
				y=50;
				break;
			case 10: // field sx
				y=160;
				x=iPad;
				break;
			case 11: // switch on-off
				y=166;
				x=165;
				break;
			case 12: // send to label
				x=Display.getWidth()/2-_iLenSendTo;
				y=135;
				break;
			case 13:// ask before
				x=iPad*2;
				y=175;
				break;
			case 14:// email
				y=160;
				x=250;
				break;
			case 15: // numb
				y=230;
				x=iPad;
				break;
			case 16:// twitter
				x=320;
				y=240;
				break;
			case 17:// accept
				x=400;
				y=240;
				break;

			}
			setPositionChild(field,x,y);
			layoutChild(field, width, height);            
		}
		setExtent(width,height);
    }
	public void paint(Graphics graphics)
    {	      
		graphics.drawBitmap(0, 0, _backgr.getWidth(), _backgr.getHeight(), _backgr, 0, 0);
		super.paint(graphics);
    }
	public void Load()
	{
		RecordStore rs = null;
	    try {
	         rs = RecordStore.openRecordStore(_sFileNameSettings,false);
	         RecordEnumeration e = rs.enumerateRecords(null,null,false);
	         if(e.hasNextElement()) {
	            byte[] cfg = rs.getRecord(e.nextRecordId());
	            ByteArrayInputStream buf = new ByteArrayInputStream(cfg);
	            DataInputStream dat = new DataInputStream(buf);
	            _sLangSrc  = dat.readUTF(); if(_sLangSrc.length() < 1) _sLangSrc="EN";
	            _sLangDst  = dat.readUTF(); if(_sLangDst.length() < 1) _sLangDst="IT";
				_sTmpSrcLang = _sLangSrc; 
				_sTmpDstLng  = _sLangDst;
	            _bReqEmail = dat.readBoolean();
	            _sEmail    = dat.readUTF(); if(_sEmail.length() < 1) _sEmail="";
	            _sNumMob   = dat.readUTF(); if(_sNumMob.length() < 1) _sNumMob="";
	            _sTwitterUsr = dat.readUTF(); if(_sTwitterUsr.length() < 1) _sTwitterUsr="";
	            _sTwitterPwd = dat.readUTF(); if(_sTwitterPwd.length() < 1) _sTwitterPwd="";
	         }
	      } catch(RecordStoreNotFoundException ex) {DefaultValues();
	      } catch(RecordStoreException ex) {DefaultValues();
	      } catch(IOException ex) {DefaultValues();
	      } finally {
	         if(rs != null) try { rs.closeRecordStore(); } catch(RecordStoreException ex) {}
	   }		   
	}
	void DefaultValues(){
		_sLangSrc="en";
		_sLangDst="it";
		_bReqEmail=false;
		_sEmail="";
		_sNumMob="";
	}
	public void Save()
	{
		RecordStore rs = null;
	    try 
	    {
	    	ByteArrayOutputStream buf = new ByteArrayOutputStream(512);
	        DataOutputStream dat = new DataOutputStream(buf);
	        dat.writeUTF(_sLangSrc == null ? ""  : _sLangSrc);
	        dat.writeUTF(_sLangDst == null ? ""  : _sLangDst);
	        dat.writeBoolean(_bReqEmail);
	        dat.writeUTF(_sEmail == null ? ""  : _sEmail);
	        dat.writeUTF(_sNumMob == null ? ""  : _sNumMob);
	        dat.writeUTF(_sTwitterUsr == null ? ""  : _sTwitterUsr);
	        dat.writeUTF(_sTwitterPwd == null ? ""  : _sTwitterPwd);
	        
	        dat.close();
	        byte[] cfg = buf.toByteArray();
	        rs = RecordStore.openRecordStore(_sFileNameSettings,true);
	        RecordEnumeration e = rs.enumerateRecords(null,null,false);
	        if(e.hasNextElement()) 
	        	rs.setRecord(e.nextRecordId(),cfg,0,cfg.length);
	        else 
	            rs.addRecord(cfg,0,cfg.length);
	        
	      } catch(RecordStoreNotFoundException ex) {
	      } catch(RecordStoreException ex) {
	      } catch(IOException ex) {
	      } finally {
	         if(rs != null) try { rs.closeRecordStore(); } catch(RecordStoreException ex) {}
	   }
	}		
	public boolean isShowed()
    {
      return _bShowed;
    }
	public void Show(){
		_bShowed=true;
		_bmFieldLangDest.setBitmap(Bitmap.getBitmapResource(_sLangDst+".png"));
		_bmFieldLangSrc.setBitmap(Bitmap.getBitmapResource(_sLangSrc+".png"));
		_LabelLangSrc.setText(_sLangSrc.toUpperCase());
		_LabelLangDst.setText(_sLangDst.toUpperCase());
		_EditEmail.setText(_sEmail);
		_EditNumMob.setText(_sNumMob);
		if (_bReqEmail)
			_bmOnOff.setBitmap(Bitmap.getBitmapResource("on.png"));
		else
			_bmOnOff.setBitmap(Bitmap.getBitmapResource("off.png"));

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
		
		if (_wcb!=null && _wcb.isAlive()){
			return _tw.MykeyDown(keycode, time);
		}
		else
		{
			switch (keycode)
		    {
		    	case 1769472: //back butt.
		    		Hide();bret=true;
		            if (_sEmail=="" || _sEmail.indexOf(".")<0 || _sEmail.indexOf("@")<0){
		          	  Dialog.alert(_sMsgEmail);
		          	  System.exit( 1 );
		            }
		            break;
		     }
			return bret;
		}
	     
	}
	public void UpdateBmpSrcLang(String sLangAbbrev){
		_bmFieldLangSrc.setBitmap(Bitmap.getBitmapResource(sLangAbbrev+".png"));
		_LabelLangSrc.setText(_sTmpSrcLang.toUpperCase());
	}
	public void UpdateBmpDstLang(String sLangAbbrev){
		_bmFieldLangDest.setBitmap(Bitmap.getBitmapResource(sLangAbbrev+".png"));
		_LabelLangDst.setText(_sTmpDstLng.toUpperCase());
	}
	public void UpdateBmpLang(String sLangAbbrev){
		
		if (_bUpadteLangSrc){
			_sTmpSrcLang = sLangAbbrev;
			UpdateBmpSrcLang(sLangAbbrev);
		}
		else{
			_sTmpDstLng = sLangAbbrev;
			UpdateBmpDstLang(sLangAbbrev);
		}			
	}
	private void ShowAddressBook() 
	{
        try {
        	_sEmailContact="";
        	_sMobileNumberContact="";
            BlackBerryContactList contacts = (BlackBerryContactList)BlackBerryPIM.getInstance().openPIMList(BlackBerryPIM.CONTACT_LIST, BlackBerryPIM.READ_WRITE);
            BlackBerryContact contact = (BlackBerryContact)contacts.choose(null, BlackBerryContactList.AddressTypes.EMAIL,true);
            
            if (contact!=null){
                int numValues = 0;
                numValues = contact.countValues(BlackBerryContact.TEL);
                _sEmailContact = contact.getString(BlackBerryContact.EMAIL, 0);

                for (int i = 0; i < numValues; i++) {
                    if (contact.getAttributes(BlackBerryContact.TEL, i) == BlackBerryContact.ATTR_MOBILE) {
                        _sMobileNumberContact = contact.getString(BlackBerryContact.TEL, i);                    
                        break;
                    }
                }
            }
        } catch (PIMException ex) {
            ex.printStackTrace();
        }    
    }
	void WaitComeBack(){
		while (_hs.getField(0)!=this);	
		_sTwitterUsr = _tw._sUsr;
		_sTwitterPwd = _tw._sPwd;
	}
	public class WaitComeBack extends Thread
    {
    	public void run()
        {
    		WaitComeBack();
        }
    }
}
