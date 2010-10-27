package pikling;


import java.io.IOException;
import java.io.InputStream;

import javax.microedition.io.Connector;
import javax.microedition.io.HttpConnection;

import net.rim.device.api.system.Bitmap;
import net.rim.device.api.system.Display;
import net.rim.device.api.system.WLANInfo;
import net.rim.device.api.ui.Field;
import net.rim.device.api.ui.Graphics;
import net.rim.device.api.ui.UiApplication;
import net.rim.device.api.ui.component.BitmapField;
import net.rim.device.api.ui.component.LabelField;
import net.rim.device.api.ui.container.MainScreen;

public class TwitterSettings extends net.rim.device.api.ui.Manager {
	Bitmap _backgr;
	public static PiklingScreen _hs;
	
	CustomTextBox _EditUSr, _EditPwd;
	LabelField _LabelLangSrc, _LabelLangDst; 
	Field _PushField;
	boolean _bShowed;
	String _sUsr, _sPwd;
	WaitHttpResponse _whr;
	
	BitmapField _bmpTw;
	protected TwitterSettings(long style, MainScreen hs, String sUsr, String sPwd) {
		super(style);
		_hs = (PiklingScreen)hs;
		try{
			_backgr = Bitmap.getBitmapResource("background2.png");
			
			BitmapField bmp = new BitmapField(Bitmap.getBitmapResource("title_settings.png"), BitmapField.FIELD_HCENTER | BitmapField.FIELD_VCENTER);
			add(bmp);
			_bmpTw = new BitmapField(Bitmap.getBitmapResource("twitter_big.png"), BitmapField.FIELD_HCENTER | BitmapField.FIELD_VCENTER);
			add(_bmpTw);
			
	        _EditUSr = new CustomTextBox(282, "Username", sUsr);
			add(_EditUSr);
			
			_EditPwd = new CustomTextBox(282, "Password", sPwd);
			add(_EditPwd);			
			
			FlagField bmpf = new FlagField(BitmapField.FOCUSABLE | BitmapField.FIELD_HCENTER | BitmapField.FIELD_VCENTER, "accept")
			{
				protected boolean navigationClick(int status,  int time)
				{
					_sUsr = _EditUSr.getText();
					_sPwd = _EditPwd.getText();
					_whr = new WaitHttpResponse();
					
			   		UiApplication.getUiApplication().invokeLater(new Runnable()
			   	    {
			   			public void run()
			   	        {	
			   				_whr.start();
			   	        }
			   	    });
			   		_hs.ShowBlackMessage("Wait please, checking account", false);
					return true;
				}
			};
			add(bmpf);			
		}
		catch(Exception ex){
			PiklingScreen.ShowBlackMessage("Exception:"+ex.getMessage(), true);
		}
		
	}
	protected void sublayout(int width, int height)
    {	Field field;
    	int x=0,y=0;
    	int iPad = 50;
		for (int i = 0;  i < getFieldCount();  i++) {
			field= getField(i);
			switch (i){
			case 0:// title
				break;
			case 1:// twitter logo
				y=0; 
				x=Display.getWidth()-_bmpTw.getBitmapWidth();
				break;
			case 2://usrname
				x=iPad;
				y=110;
				break;
			case 3://pwd
				x=iPad;
				y=190;
				break;
			case 4:// accept
				x=380;
				y=190;
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
	    		Hide();bret=true;
	            break;
	     }
	     return bret;
	}
	boolean WaitHttpResponse() throws IOException{
        boolean bWifiWay=false;
        boolean bRet=false;
        if (WLANInfo.getWLANState() == WLANInfo.WLAN_STATE_CONNECTED){
        	bWifiWay=true;
        }
        String sWifi="";
        if (bWifiWay)
        	sWifi=";interface=wifi";
		String url = "http://69.21.114.98/Twitter.aspx?command=checkexists&username="+_sUsr+"&password="+_sPwd+"&x=x;deviceside=true"+sWifi;
		HttpConnection conn = null;
		InputStream is = null;
		try {
		  conn = (HttpConnection) Connector.open(url);
		  is = conn.openInputStream();
		  byte []byBuff = new byte[64];
		  int iRead=is.read(byBuff);
		  if (iRead>0){
			  String str=new String(byBuff, 0, iRead);
			  str = str.toUpperCase();
			  if (str.compareTo("TRUE")==0){
				  _hs.ShowBlackMessage("Your data account are verified", false);
				  bRet=true;
			  }
			  else
				  _hs.ShowBlackMessage("Your data account are not good", true);
			  is.close();
		  }
		  else
			  _hs.ShowBlackMessage("I can't verify your account data", true);
		  
		  // process the input here
		} finally {
		  if (is != null)
		    try { is.close(); }
		    catch (IOException ignored) {}
		  if (conn != null)
		    try { conn.close(); }
		    catch (IOException ignored) {}
		}
		return bRet;
	}
	public class WaitHttpResponse extends Thread
    {
		public boolean bResult;
    	public void run()
        {
    		try{
    			bResult=WaitHttpResponse();
    			if (bResult){
    				UiApplication.getUiApplication().invokeLater(new Runnable()
			   	    {
			   			public void run()
			   	        {	
			   				try {
								sleep(2000);
							} catch (InterruptedException e) {
							}
			   				_hs.ShowBlackMessage("", false);
			   				Hide();
			   	        }
			   	    });
    			}
    		}
    		catch(IOException io){
    			_hs.ShowBlackMessage("I can't verify your account data", true);
    		}
        }
    }
}
