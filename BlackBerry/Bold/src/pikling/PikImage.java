package pikling;

import java.util.Enumeration;
import java.util.Vector;
import net.rim.device.api.math.Fixed32;
import net.rim.device.api.system.Bitmap;
import net.rim.device.api.system.Display;
import net.rim.device.api.system.EncodedImage;
import net.rim.device.api.ui.*;
import net.rim.device.api.ui.component.BitmapField;
import net.rim.device.api.ui.container.MainScreen;
import net.rim.device.api.ui.container.VerticalFieldManager;

import javax.microedition.io.Connector;
import javax.microedition.io.file.FileConnection;
import javax.microedition.io.file.FileSystemRegistry;

public class PikImage extends VerticalFieldManager{
	Bitmap _backgr;
	boolean _bShowed;
	PiklingScreen _hs;
	Font _fnt;
	int _iHFlags, _iWFlags;
	Vector _Languages = new Vector();
	Field _PushField;
	private String _parentRoot;
	FileExplorerDemoListFieldImpl _list;
	boolean hasFocus = false;
	BitmapField _bmPreview;
	
	protected PikImage(long style, MainScreen hs){		
		super(style);
		_hs = (PiklingScreen)hs;
		_backgr = Bitmap.getBitmapResource("background2.png");		
		_fnt = getFont("BBMillbank",16);
		_bmPreview = new BitmapField(Bitmap.getBitmapResource("icon.png"),BitmapField.FIELD_HCENTER | BitmapField.FIELD_VCENTER);
		_list = new FileExplorerDemoListFieldImpl(){
			public int moveFocus(int amount, int status, int time)
		    {
		        invalidate(getSelectedIndex());
		        selectAction(false);
		        return super.moveFocus(amount, status, time);
		    }
			// Invoked when this field receives the focus.
		    public void onFocus(int direction)
		    {
		        hasFocus = true;
		        super.onFocus(direction);
		        invalidate();
		        //selectAction();
		    }
		    // Invoked when a field loses the focus.
		    public void onUnfocus()
		    {
		        hasFocus = false;
		        super.onUnfocus();
		        invalidate();
		    }		
		};
		//_list.setFont(_fnt);
		add(_list);
		//ManagerList vm = new ManagerList(MainLayout.VERTICAL_SCROLLBAR | MainLayout.VERTICAL_SCROLL | MainLayout.USE_ALL_WIDTH | MainLayout.USE_ALL_HEIGHT, Display.getHeight()/2, 100);
		
		//vm.add(_list);
		//add(vm);
        //add(_bmPreview);
        readRoots(System.getProperty("fileconn.dir.photos"));
	}
	
	
	public void paint(Graphics graphics)
    {	      
		graphics.drawBitmap(0, getVerticalScroll(), _backgr.getWidth(), _backgr.getHeight(), _backgr, 0, 0);		
		super.paint(graphics);
    }

	protected void sublayout(int width, int height)
    {	Field field;
    	int x=0,y=0;
		for (int i = 0;  i < getFieldCount();  i++) {
			field= getField(i);
			switch (i){
			case 0:
				setPositionChild(field,x,y);
				break;
			case 1:
				setPositionChild(field,Display.getWidth()/3,Display.getHeight()/2);
				break;
			}
	        layoutChild(field, width, height);            
		}
		setExtent(width,height);
    }
	 public int getPreferredHeight()
	    {
	        return Display.getHeight();
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
	
	 /**
     * Reads the path that was passed in and enumerates through it.
     * 
     * @param root Path to be read.
     */
    private void readRoots(String root)
    {
        _parentRoot = root;
        
        // Clear whats in the list.
        _list.removeAll();
        _list.add("..");

        FileConnection fc = null;
        Enumeration rootEnum = null;

        if (root != null) 
        {
            // Open the file system and get the list of directories/files.
            try 
            {
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
            fc = (FileConnection)Connector.open(file);

            // Create a file holder from the FileConnection so that the connection is not left open.
            FileExplorerDemoFileHolder fileholder = new FileExplorerDemoFileHolder(file);
            fileholder.setDirectory(fc.isDirectory());
            _list.add(fileholder);
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
    /**
     * Displays information on the selected file.
     * 
     * @return True.
     */
    private boolean selectAction(boolean bEntryDirectory) 
    {
        FileExplorerDemoFileHolder fileholder = (FileExplorerDemoFileHolder)_list.get(_list, _list.getSelectedIndex());
        FileConnection fconn=null;
        
        if (fileholder != null) 
        {
            // If it's a directory then show what's in the directory.
            if (fileholder.isDirectory()) 
            {
            	if (bEntryDirectory)
            		readRoots(fileholder.getPath());
            } 
            else 
            { 
            }
        }
        
        return true;
    }
    public static EncodedImage ScaleImageToSize(EncodedImage image, int targetWidth, int targetHeight)
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
    public boolean invokeAction(int action){
    	boolean bret=false;
    	switch(action)
        {
            case ACTION_INVOKE: // Trackball click.
            	selectAction(true);
            	bret=true;
            	break;
        }
    	return bret;
    }    
}
