package pikling;

import java.io.InputStream;
import java.io.OutputStream;
import java.util.Enumeration;
import java.util.Vector;
import javax.microedition.io.Connector;
import javax.microedition.io.file.FileConnection;
import javax.microedition.io.file.FileSystemRegistry;
import net.rim.device.api.math.Fixed32;
import net.rim.device.api.system.Bitmap;
import net.rim.device.api.system.Display;
import net.rim.device.api.system.EncodedImage;
import net.rim.device.api.system.JPEGEncodedImage;
import net.rim.device.api.ui.Field;
import net.rim.device.api.ui.Font;
import net.rim.device.api.ui.FontFamily;
import net.rim.device.api.ui.Graphics;
import net.rim.device.api.ui.MenuItem;
import net.rim.device.api.ui.UiApplication;
import net.rim.device.api.ui.component.BitmapField;
import net.rim.device.api.ui.component.Menu;
import net.rim.device.api.ui.container.MainScreen;

public class Thumbnails extends net.rim.device.api.ui.Manager{
	Bitmap _backgr;
	boolean _bShowed;
	PiklingScreen _hs;
	Font _fnt;
	int _iHThumb, _iWThumb;
	Vector _list = new Vector();
	Vector _listBmpField = new Vector();
	Field _PushField;
	String _parentRoot;
	int N_COL = 4;
	int N_ROW = 3;
	int BORDER = 4;
	int _iPage=0;
	public String _sImage2Process;
	String _sPathImg;
	FileExplorerDemoListFieldImpl _BrowserFolder;
	SelectFolder _bf;
	
	boolean _bScrollHidden;
	protected Thumbnails(long style, MainScreen hs){		
		super(style);
		_hs = (PiklingScreen)hs;
		_backgr = Bitmap.getBitmapResource("background2.png");		
		_fnt = getFont("BBMillbank",16);
		_sPathImg = System.getProperty("fileconn.dir.photos");//"store/home/user/pictures/";
		
		int i;
		int iNFilesPerPage =N_COL*N_ROW;
		String sPng="";
		for (i=0; i<iNFilesPerPage; i++){			
			Bitmap bmp=Bitmap.getBitmapResource("icon.png");
			if (bmp!=null){
				FlagField bmf = new FlagField(BitmapField.FOCUSABLE | BitmapField.FIELD_HCENTER | BitmapField.FIELD_VCENTER, "icon")
				{
					protected boolean navigationClick(int status,  int time){
						_sImage2Process = GetTag();
						String sFileResized = System.getProperty("fileconn.dir.photos") + "pikling.jpg";
						getBitmap(_sImage2Process,800, 600, sFileResized);
						_sImage2Process = sFileResized;
						Hide();
						_hs.ShowResult(_sImage2Process);
						return true;
					}
				};				
				bmf._bRoundRect=false;
				_listBmpField.addElement(bmf);
				_iWThumb = (Display.getWidth()-BORDER)/N_COL;
				_iHThumb = (Display.getHeight()-BORDER)/N_ROW;
				add(bmf);
			}
			else
				System.out.println("Miss png:"+sPng);
		}
		FlagField bmfup = new FlagField(BitmapField.FOCUSABLE | BitmapField.FIELD_HCENTER | BitmapField.FIELD_VCENTER, "up")
		{
			protected boolean navigationClick(int status,  int time){
				if (_iPage-1>=0){
					_iPage--;
					LoadFiles(false);
				}
				return true;
			}
		};
		bmfup._bRoundRect=false;
		FlagField bmfdn = new FlagField(BitmapField.FOCUSABLE | BitmapField.FIELD_HCENTER | BitmapField.FIELD_VCENTER, "down")
		{
			protected boolean navigationClick(int status,  int time){
				_iPage++;
				if (!LoadFiles(false))
					_iPage--;
				return true;
			}
		};
		bmfdn._bRoundRect=false;
		if (!LoadFiles(true)){
			add(bmfup);
			add(bmfdn);
			_bScrollHidden=false;
		}
		else
			_bScrollHidden=true;
	}
	
	boolean LoadFiles(boolean bLoadList){
		boolean bRet=false;
		try {
			if (bLoadList)
				readRoots(_sPathImg);
			int iNelem = N_COL*N_ROW;
	    	int i, iItem;
	    	int iBmp=0;
	    	for (i=0; i<iNelem; i++){
	    		iItem = _iPage*iNelem+i;
	    		if (iItem<_list.size()){
		    		FileExplorerDemoFileHolder f=(FileExplorerDemoFileHolder) _list.elementAt(iItem);
		    		if (f.isDirectory()==false && iBmp<_listBmpField.size()){
		    			FlagField bmf = (FlagField)_listBmpField.elementAt(iBmp);
		    			Bitmap bmp = getBitmap(f.getPath()+f.getFileName(),_iWThumb, _iHThumb, "");
		    			bmf.SetTag(f.getPath()+f.getFileName());
		    			if (bmp!=null){
		    				bmf.setBitmap(bmp);
		    				System.out.println("thumb "+iBmp+" file:"+f.getFileName());
		    			}
		    			else
		    				bmf.setBitmap(null);
		    			iBmp++;
		    		}
	    		}
	    		else if (iBmp<_listBmpField.size()){
	    			FlagField bmf = (FlagField)_listBmpField.elementAt(iBmp);
	    			bmf.setBitmap(null);
	    			iBmp++;	    			
	    		}
	    	}
	    	if (i==iNelem)
	    		bRet=true;
	    }
	    catch (Exception e) {
	 
	    }
	    return bRet;
	}
	
	static Bitmap getBitmap(String sFileName, int iW, int iH, String sSaveFile){
		FileConnection fconn=null;
		Bitmap bmpRet=null;
		try {
    		//fconn = (FileConnection)Connector.open("file:///"+sFileName);
			fconn = (FileConnection)Connector.open(sFileName);
            // If no exception is thrown, then the URI is valid, but the file may or may not exist.
            if (fconn.exists()) {
            	InputStream input = fconn.openInputStream();
            	int available = (int)fconn.fileSize();
            	byte[] data = new byte[available];
            	input.read(data, 0, available);
            	EncodedImage image = EncodedImage.createEncodedImage(data,0,data.length);
        		EncodedImage imageR = ScaleImageToSize(image, iW, iH);
        		bmpRet = imageR.getBitmap();
        		if (sSaveFile!=""){
        			JPEGEncodedImage jpegImage = JPEGEncodedImage.encode(bmpRet,70); 
        			byte []byData= jpegImage.getData();
        			FileConnection fconnsave = (FileConnection)Connector.open(sSaveFile);
        			if (fconnsave.exists())
        				fconnsave.delete();
        			fconnsave.create();  
        			OutputStream output = fconnsave.openOutputStream();
        			output.write(byData, 0, byData.length);
        			output.flush();
        			output.close();
        			fconnsave.close();
        		}
        		
            	input.close();
            	input=null;
            	data=null;
            	image=null;
            }
            else {
            	System.out.println("File not found:"+sFileName);
            	
            }
        }
        catch (Exception ioe) {
        	System.out.println("selectAction Exception:"+ioe.getMessage());
        }
        finally{
        	try {fconn.close();}catch (Exception ioe) {}
            fconn=null;
        }		
        return bmpRet;
	}
	static EncodedImage ScaleImageToSize(EncodedImage image, int targetWidth, int targetHeight)
    {
        if (targetWidth > 0 && targetHeight > 0)
        {
            int imageWidth = image.getWidth();
            int imageHeight = image.getHeight();
            float scaleX = ((float) imageWidth / (float) targetWidth) * 10000;
            float scaleY = ((float) imageHeight / (float) targetHeight) * 10000;
            int fixedX = Fixed32.tenThouToFP( (int) scaleX);         
            int fixedY = Fixed32.tenThouToFP( (int) scaleY);
            EncodedImage retImage = image.scaleImage32(fixedX,fixedY);
            return retImage;
        }     
        return image;
    }
	/**
     * Reads the path that was passed in and enumerates through it.
     * 
     * @param root Path to be read.
     */
    private void readRoots(String root)
    {
        _parentRoot = root;
        
        // Clear whats in the list.
        clearList();
        

        FileConnection fc = null;
        Enumeration rootEnum = null;

        if (root != null) 
        {
            // Open the file system and get the list of directories/files.
            try 
            {
                //fc = (FileConnection)Connector.open("file:///" + root);
            	fc = (FileConnection)Connector.open(root);
                rootEnum = fc.list();
            } 
            catch (Exception ioex) 
            {
            } 
            finally 
            {
                
                if (fc != null) 
                {   
                    // Everything is read, make sure to close the connection.
                    try 
                    {
                        fc.close();
                        fc = null;
                    } 
                    catch (Exception ioex) 
                    {
                    }
                }
            }
        }

        // There was no root to read, so now we are reading the system roots.
        if (rootEnum == null) 
        {
            rootEnum = FileSystemRegistry.listRoots();
        }

        // Read through the list of directories/files.
        while (rootEnum.hasMoreElements()) 
        {
            String file = (String)rootEnum.nextElement();
            
            if (root != null) 
            {
                file = root + file;
            }
            
            readSubroots(file);
        }
    }
    
    void clearList(){
    	int iNelem = _list.size();
    	int i;
    	for (i=0; i<iNelem; i++){
    		FileExplorerDemoFileHolder f=(FileExplorerDemoFileHolder) _list.elementAt(i);
    		f=null;
    	}
    	_list.removeAllElements();
    }

    /**
     * Reads all the directories and files from the provided path.
     * 
     * @param file Upper directory to be read.
     */
    private void readSubroots(String file) 
    {
        FileConnection fc = null;
        
        try 
        {
            //fc = (FileConnection)Connector.open("file:///" + file);
        	fc = (FileConnection)Connector.open(file);

            // Create a file holder from the FileConnection so that the connection is not left open.
            FileExplorerDemoFileHolder fileholder = new FileExplorerDemoFileHolder(file);
            fileholder.setDirectory(fc.isDirectory());
            _list.addElement(fileholder);
        } 
        catch (Exception ioex) 
        {
        } 
        finally 
        {
            if (fc != null) 
            {
                // Everything is read, make sure to close the connection.
                try 
                {
                    fc.close();
                    fc = null;
                } 
                catch (Exception ioex) 
                {
                }
            }
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
    	int iPad=BORDER/2;
    	int x=0,y=0, iNCol=N_COL;
    	int iStepX, iStepY;
    	int iWScreen = Display.getWidth();
    	iStepX = iWScreen/iNCol;
    	int iNFields= getFieldCount()/2;
    	if (iNCol>iNFields)
    		iNCol=iNFields;
    	float fNRow = ((float)iNFields/iNCol)+0.5f;
    	//int iNRow   = (int)fNRow;
    	int iNRow    = N_ROW;
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
	    			setPositionChild(field,x,y);
	                layoutChild(field, width, height);
    			}
    		}
    		y+=iPad;
    	}
    	
    	if (!_bScrollHidden){
	    	iField++;
	    	field= getField(iField);
	    	x=Display.getWidth()-85;
	    	y=Display.getHeight()-100;
			setPositionChild(field,x,y);
			layoutChild(field, width, height);
			y+=48;
			iField++;
			field= getField(iField);
			setPositionChild(field,x,y);
			layoutChild(field, width, height);
			iField++;
    	}
    			
		setExtent(width,height);
    }
	public boolean isShowed()
    {
      return _bShowed;
    }
	public void Show(){
		_sImage2Process="";
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
    /*public boolean invokeAction(int action){
    	boolean bret=false;
    	switch(action)
        {
            case ACTION_INVOKE: // Trackball click.
            	//Hide();
            	bret=true;
            	break;
        }
    	return bret;
    } 		*/
	public void makeMenu(Menu menu, int instance){
	   if (_hs.getField(0)==this){
		   MenuItem m1 = new MenuItem("Folders...",100,0)
		   {
		       public void run()
		       {
		    	   _bf = new SelectFolder();
		    	   UiApplication.getUiApplication().invokeLater(new Runnable()
			   	    {
			   			public void run()
			   	        {	_bf.start();
			   	        }
			   	    });
		       }
		   };
		   menu.add(m1);
	   }
	}
	void ShowBrowserFolder(){
		PikImage pk = new PikImage(MainLayout.NO_VERTICAL_SCROLLBAR | MainLayout.NO_VERTICAL_SCROLL | MainLayout.USE_ALL_WIDTH | MainLayout.USE_ALL_HEIGHT, _hs);
		synchronized(UiApplication.getUiApplication().getEventLock()) {
			pk.Show();
		}
	}
	public class SelectFolder extends Thread
    {
    	public void run()
        {
    		ShowBrowserFolder();
        }
    }
}
