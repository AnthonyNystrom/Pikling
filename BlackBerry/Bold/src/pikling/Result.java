package pikling;

import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.IOException;
import java.io.InputStream;

import javax.microedition.io.Connector;
import javax.microedition.io.Datagram;
import javax.microedition.io.DatagramConnection;
import javax.microedition.io.HttpConnection;
import javax.microedition.io.SocketConnection;
import javax.microedition.io.file.FileConnection;

import net.rim.blackberry.api.browser.Browser;
import net.rim.blackberry.api.browser.BrowserSession;
import net.rim.device.api.system.Bitmap;
import net.rim.device.api.system.Display;
import net.rim.device.api.system.WLANInfo;
import net.rim.device.api.ui.*;
import net.rim.device.api.ui.component.BitmapField;
import net.rim.device.api.ui.component.EditField;
import net.rim.device.api.ui.component.LabelField;
import net.rim.device.api.ui.component.Menu;
import net.rim.device.api.ui.container.MainScreen;
import net.rim.device.api.ui.container.VerticalFieldManager;
import net.rim.device.api.ui.decor.BorderFactory;

public class Result extends VerticalFieldManager{
	Bitmap _backgr;
	boolean _bShowed;
	PiklingScreen _hs;
	Font _fnt;
	Field _PushField;
	String _sImgToProcess;
	EditField _lblSrc;
	EditField _lblDst;
	BitmapField _imageField;
	SocketConnection _sc  = null;
	DataInputStream _sci  = null;
	DataOutputStream _sco = null;
	byte []_byIDProcess = new byte[10];
	byte []_bySrc;
	byte []_byDest;
	byte _byTranslator;
	Uploader _uploader;
	RequestSMS _rsms;
	int _iHThumb;
	int _iWThumb;
	int _iHbmpFlg;
	int _iHeightEditField;
	int _iX;
	BitmapField _flgSrc, _flgDst;
	boolean _bUploadImage;
	
	void CloseConnection(){
    	if (_sc!=null){
    		try{_sci.close();
    			_sco.close();
    			_sc.close();}catch(IOException ex) {}
    		_sci=null;
    		_sco=null;
    		_sc=null;
    	}
    }
	boolean OpenConnection(){
        String sConnection;
        boolean bret=false;
        boolean bWifiWay=false;
        if (WLANInfo.getWLANState() == WLANInfo.WLAN_STATE_CONNECTED){
        	bWifiWay=true;
        }
        if (bWifiWay){
           sConnection = "socket://69.21.114.100:8080;DeviceSide=True;interface=wifi";
           PiklingScreen.ShowBlackMessage("Using WiFi Connection...", false);
        }
        else{
        	PiklingScreen.ShowBlackMessage("Using Carrier Connection...", false);
        	sConnection = "socket://69.21.114.100:8080;DeviceSide=True;";
        }
           
        try{
           _sc = (SocketConnection)Connector.open(sConnection,Connector.READ_WRITE);
           _sc.setSocketOption(SocketConnection.SNDBUF, 11264);
           _sci = _sc.openDataInputStream(); 
           _sco = _sc.openDataOutputStream();
           
           SetTwitter();

           bret=true;
        }catch(IOException ex) {
           PiklingScreen.ShowBlackMessage("Server not found. Please check your connection status", true);
        	_sc=null;
        }catch(IllegalArgumentException ex)
        {  _sc=null;
        }
        return bret;
    }
	
	/*boolean SetTwitter(){
        boolean bWifiWay=false;
        boolean bRet=false;
        if (WLANInfo.getWLANState() == WLANInfo.WLAN_STATE_CONNECTED){
        	bWifiWay=true;
        }
        String sWifi="";
        if (bWifiWay)
        	sWifi=";interface=wifi";
        String sStatus="CIAAAAOO";
		String url = "http://69.21.114.98/Twitter.aspx?command=updatestatus&username="+_hs._settings._sTwitterUsr+"&password="+_hs._settings._sTwitterPwd+"&status="+sStatus+"&x=x;deviceside=true"+sWifi;
		HttpConnection conn = null;
		try {
		  conn = (HttpConnection) Connector.open(url);
		  conn.close();
		  // process the input here
		} 
		catch (IOException ignored) {}
		
		return bRet;
	}*/
	boolean SetTwitter() throws IOException{
        boolean bWifiWay=false;
        boolean bRet=false;
        if (WLANInfo.getWLANState() == WLANInfo.WLAN_STATE_CONNECTED){
        	bWifiWay=true;
        }
        String sWifi="";
        if (bWifiWay)
        	sWifi=";interface=wifi";
        String sStatus="has just translated an image using pikling";
		String url = "http://69.21.114.98/Twitter.aspx?command=updatestatus&username="+_hs._settings._sTwitterUsr+"&password="+_hs._settings._sTwitterPwd+"&status="+sStatus+"&x=x;deviceside=true"+sWifi;
		HttpConnection conn = null;
		InputStream is = null;
		int iRead=0;
		String str="";
		try {
		  conn = (HttpConnection) Connector.open(url);
		  is = conn.openInputStream();
		  byte []byBuff = new byte[64];
		  iRead=is.read(byBuff);		 
		  if (iRead>0)
			  str=new String(byBuff, 0, iRead);

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
	
	protected Result(long style, MainScreen hs){		
		super(style);
		_hs = (PiklingScreen)hs;
		_backgr = Bitmap.getBitmapResource("background2.png");		
		_fnt = getFont("BBMillbank",16);
		LabelField lblDummy = new LabelField();
		_lblSrc = new EditField("", "",10000, EditField.READONLY);
		_lblDst = new EditField("", "",10000, EditField.READONLY);
		//_lblDst.setFont(_fnt);
		//_lblSrc.setFont(_fnt);
		_imageField = new BitmapField();
		_iWThumb=Display.getWidth()/4;
		_iHThumb=Display.getHeight()/4;
		
		Bitmap bm=Bitmap.getBitmapResource(_hs._settings._sLangSrc+".png");
		_iHbmpFlg = bm.getHeight();
		_flgSrc = new BitmapField(bm);
		_flgDst = new BitmapField(Bitmap.getBitmapResource(_hs._settings._sLangDst+".png"));
		
    	_iX=5;
    	_iHeightEditField = Display.getHeight()/2-_iX*2-_iHbmpFlg;
    	int iW = Display.getWidth()-_iX*4;
		ManagerList vm1 = new ManagerList(MainLayout.VERTICAL_SCROLLBAR | MainLayout.VERTICAL_SCROLL | MainLayout.USE_ALL_WIDTH | MainLayout.USE_ALL_HEIGHT, _iHeightEditField, iW);
		ManagerList vm2 = new ManagerList(MainLayout.VERTICAL_SCROLLBAR | MainLayout.VERTICAL_SCROLL | MainLayout.USE_ALL_WIDTH | MainLayout.USE_ALL_HEIGHT, _iHeightEditField, iW);
		vm1.setBorder(BorderFactory.createSimpleBorder(new XYEdges(1,1,1,1))); 
		vm2.setBorder(BorderFactory.createSimpleBorder(new XYEdges(1,1,1,1)));
		vm1.add(_lblSrc);
		vm2.add(_lblDst);
		
		add(lblDummy);
		//add(_imageField);
		add(_flgSrc);
		add(vm1);
		add(_flgDst);
		add(vm2);
	}
	
	public void paint(Graphics graphics)
    {	      
		graphics.drawBitmap(0, getVerticalScroll(), _backgr.getWidth(), _backgr.getHeight(), _backgr, 0, 0);		
		super.paint(graphics);
    }

	protected void sublayout(int width, int height)
    {	Field field;
    	
		for (int i = 0;  i < getFieldCount();  i++) {
			field= getField(i);
			switch (i){
			/*case 1:
				setPositionChild(field,10,Display.getHeight()/2-_iHThumb/2);
				break;*/
			case 1:
				setPositionChild(field,_iX,_iX);
				break;
			case 2:
				setPositionChild(field,_iX*2,_iX+_iHbmpFlg);
				break;
			case 3:
				setPositionChild(field,_iX,Display.getHeight()/2+_iX);
				break;
			case 4:
				setPositionChild(field,_iX*2,Display.getHeight()/2+_iX+_iHbmpFlg);
				break;
			}
	        layoutChild(field, width, height);            
		}
		setExtent(width,height);
    }
	public boolean isShowed()
    {
      return _bShowed;
    }
	public void Show(String sImgToProcess){
		if (sImgToProcess!=""){
			_sImgToProcess = sImgToProcess;
			_lblDst.setText("");
			_lblSrc.setText("");
		}
		_flgSrc.setBitmap(Bitmap.getBitmapResource(_hs._settings._sLangSrc+".png"));
		_flgDst.setBitmap(Bitmap.getBitmapResource(_hs._settings._sLangDst+".png"));
		
		/*Bitmap bm = Thumbnails.getBitmap(_sImgToProcess, _iWThumb, _iHThumb);
		_imageField.setBitmap(bm);*/
		_bShowed=true;
		_PushField = _hs.getField(0);
		_hs.delete(_PushField);
		_hs.add(this);
		invalidate();
		if (sImgToProcess!=""){
			_uploader = new Uploader();
			UiApplication.getUiApplication().invokeLater(new Runnable()
		    {
				public void run()
				{	_bUploadImage=true;
					_uploader.start();
		        }
		    });
		}
	}
	public void Hide(){
		if (_sImgToProcess!=""){
			try{
				FileConnection fconnsave = (FileConnection)Connector.open(_sImgToProcess);
				if (fconnsave.exists())
					fconnsave.delete();
				fconnsave.close();
			}
			catch (Exception ioe) {
			}
		}
		if (!_uploader.isAlive()){
			_bShowed=false;
			_hs.delete(this);
			_hs.add(_PushField);
		}
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
	
    public boolean invokeAction(int action){
    	boolean bret=false;
    	switch(action)
        {
            case ACTION_INVOKE: // Trackball click.
            	bret=true;
            	break;
        }
    	return bret;
    }
    public boolean RequestEmail()
	{	boolean bret=false;
		String sAddrTo=_hs._settings._sEmail;
	
		try{
			if (_hs._settings._bReqEmail || _hs._settings._sEmail==""){
				GetInfo getinfo=null;
				PiklingScreen.ShowBlackMessage("", false);
				synchronized(UiApplication.getUiApplication().getEventLock()) {
				    getinfo= new GetInfo(MainLayout.NO_VERTICAL_SCROLLBAR | MainLayout.NO_VERTICAL_SCROLL | MainLayout.USE_ALL_WIDTH | MainLayout.USE_ALL_HEIGHT, _hs, true, "Email", _hs._settings._sEmail);
				    getinfo.Show();
				}
			   // wait that getinfo is hidden
			   while (_hs.getField(0)!=this);
			   sAddrTo = getinfo._EditData.getText();
			}
			
			PiklingScreen.ShowBlackMessage("Sending email...", false);
			
        	byte byBuf[] = new byte[10];        	
        	byte byBufRx[] = new byte[10];
        	byBuf[0]=1;
        	_sco.write(byBuf, 0, 1);
        	int iRead=ReadSocket(_sci, byBuf, 1);
        	if (iRead==1){
        		
            	String sHeader= new String(_byIDProcess, "UTF-8");
            	sHeader+="|PDF|"+sAddrTo;
            	int iLen=(int)sHeader.length();
            	byBuf[0] = (byte)(iLen & 0x000000FF); 
            	byBuf[1] = (byte)((iLen>>8) & 0x000000FF);
            	_sco.write(byBuf,0, 2);
            	iRead=ReadSocket(_sci, byBufRx, 2);
            	if (iRead==2 && ( 
            		byBuf[0]==byBufRx[0] &&
            		byBuf[1]==byBufRx[1])){
            		_sco.write(sHeader.getBytes(),0, sHeader.length());
                	iRead=ReadSocket(_sci, byBuf, 1);
                	if (iRead==1 && byBuf[0]==1)
                		bret=true;
            	}
        	}
		}
		catch(Exception ex)
    	{	CloseConnection();
    	}
		if (!bret)
			PiklingScreen.ShowBlackMessage("Protocol error ", true);
		else
			PiklingScreen.ShowBlackMessage("", false);
		
		return bret;
	}
    boolean UploadImage(){
    	boolean bret=false;
    	FileConnection fi = null;
    	InputStream in = null;
    	try{
    		byte []buffIn = new byte[8192];
    		byte []buffSck = new byte[8192];
    		boolean bCompletedOK=false;
    		fi = (FileConnection)Connector.open(_sImgToProcess, Connector.READ);
    		in = fi.openDataInputStream();
    		// Select protocol type
    		buffSck[0]=0;
    		_sco.write(buffSck, 0, 1);
    		int iRead=ReadSocket(_sci, buffSck, 1);
    		if (iRead==1 && buffSck[0]==0){ //echo
    			String sHeader = DeviceInformation.getManuf()+"|"+DeviceInformation.getDeviceName()+"|"+DeviceInformation.getDeviceID()+"|"+DeviceInformation.getPhoneNumb()+"|"+DeviceInformation.getVer()+"|"+_hs._latitude+"|"+_hs._longitude+"|"+_hs._settings._sEmail+"|"+_hs._settings._sLangSrc+"|"+_hs._settings._sLangDst;
    			buffSck[0] = (byte)(sHeader.length() & 0x000000FF);
    			buffSck[1] = (byte)((sHeader.length() >> 8) & 0x000000FF);
    			_sco.write(buffSck, 0, 2);
        		
        		// Send language settings
        		System.arraycopy(sHeader.getBytes("UTF-8"),0,buffSck,0,sHeader.length());
        		_sco.write(buffSck, 0, sHeader.length());
        		iRead=ReadSocket(_sci, buffSck, 10);
        		if (iRead==10){
        			System.arraycopy(buffSck,0,_byIDProcess,0,10);
        			buffSck[3] = (byte)((fi.fileSize()>>24) & 0x000000FF);
        			buffSck[2] = (byte)((fi.fileSize()>>16) & 0x000000FF);
        			buffSck[1] = (byte)((fi.fileSize() >> 8) & 0x000000FF);
        			buffSck[0] = (byte)(fi.fileSize() & 0x000000FF);
            		_sco.write(buffSck, 0, 4);
            		iRead=ReadSocket(_sci, buffIn, 4);
            		if (iRead==4 && 
        				buffSck[0]==buffIn[0] && 
        				buffSck[1]==buffIn[1] &&
        				buffSck[2]==buffIn[2] &&
        				buffSck[3]==buffIn[3])
            		{
            			// send file
                		iRead=in.read(buffIn, 0, buffIn.length);
                		long l1, l2;
                		java.util.Date d = new java.util.Date();
                		l1 = l2= d.getTime();
                		String sSending="UPLOADING...";
                		PiklingScreen.ShowBlackMessage(sSending, false);
                		int iCnt=0;
                		while (iRead>0){
                    		_sco.write(buffIn, 0, iRead);
                    		_sco.flush();
                    		iRead=in.read(buffIn, 0, buffIn.length);
                    		try
                            {	synchronized(this){this.wait(200);}
                            }
                            catch (InterruptedException ioe) 
                            {
                            }
                            d = new java.util.Date();
                            l2= d.getTime();
                            if (l2-l1>200){
                            	l1=d.getTime();
                            	iCnt++;
                            	if (iCnt>8){
                            		iCnt=0;
                            		sSending="UPLOADING";
                            	}
                            	else
                            		sSending+=".";                      
                            	PiklingScreen.ShowBlackMessage(sSending, false);
                            }
                		}
                		PiklingScreen.ShowBlackMessage("WATING FOR RESULT", false);
                		iRead=ReadSocket(_sci, buffIn, 1);
                		if (iRead==1 && buffIn[0]==1){
                    		iRead=ReadSocket(_sci, buffIn, 5);
                    		if (iRead==5){
                    			_byTranslator=buffIn[0];
                                int iLen = (buffIn[4]  & 0x000000FF);iLen <<= 8;
                                iLen |= (buffIn[3] & 0x000000FF);iLen <<= 8;
                                iLen |= (buffIn[2] & 0x000000FF);iLen <<= 8;
                                iLen |= (buffIn[1] & 0x000000FF);
                                if (iLen>0){
                                	_bySrc=new byte[iLen];
                        			iRead=ReadSocket(_sci, _bySrc, iLen);
                                }
                                else
                                	_bySrc=null;
                        		buffSck[0]=1;
                        		_sco.write(buffSck, 0, 1);
                        		iRead=ReadSocket(_sci, buffIn, 4);
                        		if (iRead==4){
                                    iLen = (buffIn[3] & 0x000000FF);iLen <<= 8;
                                    iLen |=(buffIn[2] & 0x000000FF);iLen <<= 8;
                                    iLen |=(buffIn[1] & 0x000000FF);iLen <<= 8;
                                    iLen |=(buffIn[0] & 0x000000FF);
                                    if (iLen>0)
                                    {	_byDest=new byte[iLen];
                            			iRead=ReadSocket(_sci, _byDest, iLen);
                                    }
                                    else
                                    	_byDest=null;
                            		buffSck[0]=1;
                            		_sco.write(buffSck, 0, 1);
                            		bCompletedOK=true;
                        		}
                    		}
                		}
            		}
        		}	
    		}
    		if (!bCompletedOK){
    			CloseConnection();
    			PiklingScreen.ShowBlackMessage("Protocol error ", true);
    		}
    		else
    		{
    			/*boolean bEmailRes=true;
    			bEmailRes=RequestEmail();
    			if (bEmailRes)*/
    				PiklingScreen.ShowBlackMessage("", false);
    			
    			synchronized(UiApplication.getUiApplication().getEventLock()) {
    		       
	    			if (_bySrc!=null){
	    				String str=new String(_bySrc, "UTF-8");
	    				_lblSrc.setText(str);
	    			}
	    			else
	    				PiklingScreen.ShowBlackMessage("No text found", true);
	    			if (_byDest!=null){
	    				String str=new String(_byDest, "UTF-8");
	    				_lblDst.setText(str);
	    			}
    			}
    			CloseConnection();
    		}
    	}
    	catch(IOException ex) {
    		PiklingScreen.ShowBlackMessage("Error to load the image", true);
    	}
    	
		try {
			if (fi!=null)
				fi.close();
	    	if (in!=null)
	    		in.close();
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
    	return bret;
    }
    	
    protected int ReadSocket(DataInputStream sci, byte []byBuff, int iToRead)
    {
       int ir=0;
       boolean bexit=false;
       try{
          long it1 = System.currentTimeMillis();
          
          while (System.currentTimeMillis()-it1 < 120000 && bexit==false){
             int iby = sci.available();
             if (iby>0){
                if (iby+ir>iToRead)
                   iby=iToRead-ir;
                sci.read(byBuff, ir,iby);
                ir+=iby;
                if (ir>=iToRead)
                   bexit=true;
             }
          }
       }catch(IOException ex){}
       
       return ir;
    }
    public class Uploader extends Thread
    {
    	public void run()
        {
			// Socket connection
            if (OpenConnection()){
            	// socket connected, start of protocol
            	if (_bUploadImage)
            		UploadImage();
            	else
            		RequestEmail();
            }
            else
            	CloseConnection();
    		
        }
    }
    
    public class RequestSMS extends Thread
    {
    	public void run()
        {
    		SendSms();
        }
    }
    
    void SendSms(){
    	GetInfo getinfo=null;
		synchronized(UiApplication.getUiApplication().getEventLock()) {
		    getinfo= new GetInfo(MainLayout.NO_VERTICAL_SCROLLBAR | MainLayout.NO_VERTICAL_SCROLL | MainLayout.USE_ALL_WIDTH | MainLayout.USE_ALL_HEIGHT, _hs, false, "Number", _hs._settings._sNumMob);
		    getinfo.Show();
		}
	   // wait that getinfo is hidden
	   while (_hs.getField(0)!=this);
	   String sNum = getinfo._EditData.getText();
   	
	   try{
		   if (sNum!=""){
		       DatagramConnection dc = (DatagramConnection)Connector.open("sms://");
		       String sSrc=_lblSrc.getText();
		       if (sSrc.length()>160)
		    	   sSrc= sSrc.substring(0, 159);
		       if (sSrc!=""){
		    	   byte[] data = sSrc.getBytes("UTF-8");
		       	   Datagram d = dc.newDatagram(dc.getMaximumLength());
		       	   d.setAddress("//" + sNum);
		       	   d.setData(data, 0, data.length);
		       	   dc.send(d);
		       	}
		   }
	   }
	   catch(Exception ex){
		   
	   }
    }
    
	public void makeMenu(Menu menu, int instance)
	{
		if (_hs.getField(0)==this){
		   MenuItem mnuGetEmail = new MenuItem("Get Email",100,0)
		   {
		       public void run()
		       {
		    	   _uploader = new Uploader();
			   		UiApplication.getUiApplication().invokeLater(new Runnable()
			   	    {
			   			public void run()
			   	        {	_bUploadImage=false;
			   				_uploader.start();
			   	        }
			   	    });		           
		       }
		   };
		   MenuItem mnuGetSMS = new MenuItem("Send SMS",100,0){
			   public void run(){
				   _rsms = new RequestSMS();
			   	   UiApplication.getUiApplication().invokeLater(new Runnable()
			   	   {	public void run()
			   	        {	_rsms.start();			   				
			   	        }
			   	   });	
			   }
		   };
		   MenuItem mnuGetGoogleSrc = new MenuItem("Google (Origin)",100,0){
			   public void run(){
				   BrowserSession site = Browser.getDefaultSession();
				   try{
					   	String str=new String(_bySrc, "UTF-8");
		           		site.displayPage("http://www.google.com/search?q="+str+"&ie=UTF-8&oe=UTF-8&client=safari") ;
				   }
				   catch(Exception ex){
					   
				   }
			   }
		   };
		   MenuItem mnuGetGoogleDst = new MenuItem("Google (Transl.)",100,0){
			   public void run(){
				   BrowserSession site = Browser.getDefaultSession();
				   try{
					   	String str=new String(_byDest, "UTF-8");
		           		site.displayPage("http://www.google.com/search?q="+str+"&ie=UTF-8&oe=UTF-8&client=safari") ;
				   }
				   catch(Exception ex){
					   
				   }				   
			   }
		   };

		   MenuItem mnuGetWikiSrc = new MenuItem("WikiPedia (Origin)",100,0){
			   public void run(){				   
				   BrowserSession site = Browser.getDefaultSession();
				   try{
					   	String str=new String(_bySrc, "UTF-8");
		           		site.displayPage("http://mobile.wikipedia.org/transcode.php?go=" + str) ;
				   }
				   catch(Exception ex){
					   
				   }
			   }
		   };
		   MenuItem mnuGetWikiDst = new MenuItem("WikiPedia (Translated)",100,0){
			   public void run(){
				   BrowserSession site = Browser.getDefaultSession();
				   try{
					   	String str=new String(_byDest, "UTF-8");
		           		site.displayPage("http://mobile.wikipedia.org/transcode.php?go=" + str) ;
				   }
				   catch(Exception ex){
					   
				   }
			   }
		   };

		   menu.add(mnuGetEmail);
		   menu.add(mnuGetSMS);
		   menu.add(mnuGetGoogleSrc);
		   menu.add(mnuGetGoogleDst);
		   menu.add(mnuGetWikiSrc);
		   menu.add(mnuGetWikiDst);		   
		}
		
	}
}
