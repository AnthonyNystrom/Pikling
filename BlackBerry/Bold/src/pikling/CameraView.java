package pikling;

import java.io.IOException;
import java.io.OutputStream;
import java.util.Vector;

import javax.microedition.io.Connector;
import javax.microedition.io.file.FileConnection;
import javax.microedition.media.Manager;
import javax.microedition.media.MediaException;
import javax.microedition.media.Player;
import javax.microedition.media.control.VideoControl;

import net.rim.device.api.system.Bitmap;
import net.rim.device.api.system.Display;
import net.rim.device.api.ui.Field;
import net.rim.device.api.ui.UiApplication;
import net.rim.device.api.ui.component.BitmapField;
import net.rim.device.api.ui.component.Dialog;
import net.rim.device.api.ui.component.ObjectChoiceField;
import net.rim.device.api.ui.container.MainScreen;
import net.rim.device.api.util.StringUtilities;

public class CameraView extends net.rim.device.api.ui.Manager {
    VideoControl _videoControl;
    Field _videoField;
    Player _player;
    ObjectChoiceField _encodingField;
    EncodingProperties[] _encodings;
    boolean _bShowed;
    Field _PushField;
    PiklingScreen _hs;
    BitmapField _imageField=null;
    FlagField _butAccept, _butCancel;
    byte []_byRaw;
    Bitmap _image, _backgr;
    boolean _bLayoutCamera;
    RefreshDelay _rd;
    
	/** The down-scaling ratio applied to the snapshot Bitmap. */
	private static final int IMAGE_SCALING = 2;
	private static final String FILE_NAME = System.getProperty("fileconn.dir.photos") + "pikling";
	private static final String EXTENSION = ".jpg";

    
	protected CameraView(long style, MainScreen hs){
		super(style);
		_hs = (PiklingScreen)hs;
		_backgr = Bitmap.getBitmapResource("background2.png");
		_bLayoutCamera=false;

        //Initialize the list of possible encodings.
        initializeEncodingList();
        _butAccept = new FlagField(BitmapField.FOCUSABLE | BitmapField.FIELD_LEFT, "accept")
		{
			protected boolean navigationClick(int status,  int time)
			{	try 
				{	SaveImage();
					Hide();
					_hs.ShowResult(FILE_NAME+EXTENSION);
				} 
				catch (Exception e) 
				{	System.out.println("Exception:"+e.getMessage()); 
                }
                return true;
			}
		};
		_butCancel = new FlagField(BitmapField.FOCUSABLE | BitmapField.FIELD_LEFT, "cancel")
		{
			protected boolean navigationClick(int status,  int time)
			{	try 
				{	_bLayoutCamera=true;
					delete(_imageField);
					delete(_butAccept);
					delete(_butCancel);
					add(_videoField);
					_videoControl.setVisible(true);
	
					if (_videoField!=null){
						try{
							_player.start();
						}
				    	catch(MediaException e){	    	
				    	}
					}

				} 
				catch (Exception e) 
				{	System.out.println("Exception:"+e.getMessage()); 
                }
                return true;
			}
		};
		
	}
	protected void sublayout(int width, int height)
    {	Field field;
    	int x=0,y=0;
		for (int i = 0;  i < getFieldCount();  i++) {
			field= getField(i);
			switch (i){
			case 0:
				if (!_bLayoutCamera){
					x=Display.getWidth()/2-_image.getWidth()/2;
					y=0;
				}
				else{
					x=y=0;
				}
				break;
			case 1:
				x=Display.getWidth()/2-50*2;
				y=Display.getHeight()-50;
				break;
			case 2:
				x=Display.getWidth()/2+50;
				y=Display.getHeight()-50;
				break;
			}
			setPositionChild(field,x,y);
            layoutChild(field, width, height);
            x+=field.getWidth();
		}
		setExtent(width,height);
    }

	/**
     * Initializes the Player, VideoControl and VideoField.
     */
    private void initializeCamera()
    {
        try
        {
            //Create a player for the Blackberry's camera.
            _player = Manager.createPlayer( "capture://video" );
            _player.realize();

            //Grab the video control and set it to the current display.
            _videoControl = (VideoControl)_player.getControl( "VideoControl" );

            if (_videoControl != null)
            {
                //Create the video field as a GUI primitive (as opposed to a
                //direct video, which can only be used on platforms with
                //LCDUI support.)
                _videoField = (Field) _videoControl.initDisplayMode (VideoControl.USE_GUI_PRIMITIVE, "net.rim.device.api.ui.Field");
                _videoControl.setDisplayFullScreen(true);
                //Display the video control
                _videoControl.setVisible(true);
            }
        }
        catch(Exception e)
        {
            Dialog.alert( "ERROR " + e.getClass() + ":  " + e.getMessage() );
        }
    }
    /**
     * Initialize the list of encodings.
     */
    private void initializeEncodingList()
    {
        try
        {
            //Retrieve the list of valid encodings.
            String encodingString = System.getProperty("video.snapshot.encodings");
            
            //Extract the properties as an array of words.
            String[] properties = StringUtilities.stringToKeywords(encodingString);
            
            //The list of encodings;
            Vector encodingList = new Vector();
            
            //Strings representing the four properties of an encoding as
            //returned by System.getProperty().
            String encoding = "encoding";
            String width = "width";
            String height = "height";
            String quality = "quality";
            
            EncodingProperties temp = null;
            
            for(int i = 0; i < properties.length ; ++i)
            {
                if( properties[i].equals(encoding))
                {
                    if(temp != null && temp.isComplete())
                    {
                        //Add a new encoding to the list if it has been
                        //properly set.
                        encodingList.addElement( temp );
                    }
                    temp = new EncodingProperties();
                    
                    //Set the new encoding's format.
                    ++i;
                    temp.setFormat(properties[i]);
                }
                else if( properties[i].equals(width))
                {
                    //Set the new encoding's width.
                    ++i;
                    temp.setWidth(properties[i]);
                }
                else if( properties[i].equals(height))
                {
                    //Set the new encoding's height.
                    ++i;
                    temp.setHeight(properties[i]);
                }
                else if( properties[i].equals(quality))
                {
                    //Set the new encoding's quality.
                    ++i;
                    temp.setQuality(properties[i]);
                }
            }
            
            //If there is a leftover complete encoding, add it.
            if(temp != null && temp.isComplete())
            {
                encodingList.addElement( temp );
            }
            
            //Convert the Vector to an array for later use.
            _encodings = new EncodingProperties[ encodingList.size() ];
            encodingList.copyInto((Object[])_encodings);
        }
        catch (Exception e)
        {
            //Something is wrong, indicate that there are no encoding options.
            _encodings = null;
        }
    }
    void Show(){
		_bShowed=true;
		_PushField = _hs.getField(0);
		_bLayoutCamera=true;
		_hs.delete(_PushField);
		_hs.add(this);
		
        //Initialize the camera object and video field.
        initializeCamera();
        if(_videoField != null){
        	add(_videoField);
        }
		
    	try{
	        _player.start();
    	}
    	catch(MediaException e){
    		
    	}
    }
    void Close(){
    	StopCamera();
    }
    
    void Hide (){
		_bShowed=false;
		_hs.delete(this);
		_hs.add(_PushField);
    	StopCamera();
    	_player.close();
    	
		if (_imageField!=null) delete(_imageField);
		if (_butAccept!=null && !_bLayoutCamera) delete(_butAccept);
		if (_butCancel!=null && !_bLayoutCamera) delete(_butCancel);
		
    }
    public boolean MykeyDown( int keycode, int time ) 
	{
		boolean bret=false;
		 
	    switch (keycode)
	    {
	    	case 1769472: //back butt.
	    		UiApplication.getUiApplication().invokeLater(new Runnable()
			    {
					public void run()
					{	
						_hs.invalidate();
			        }
			    });
	    		bret=true;
	    		Hide();
	    		break;
	     }
	     return bret;
	}
    public class RefreshDelay extends Thread
    {
    	public void run()
        {
    		try {
				sleep(1000);
				_hs.invalidate();
				Dialog.alert("FINE");
			} catch (InterruptedException e) {
				e.printStackTrace();
			}
        }
    }
    public boolean isShowed()
    {
      return _bShowed;
    }
    void Capture(){
    	try
        {
            //A null encoding indicates that the camera should
            //use the default snapshot encoding.
            String encoding = null;
            
            //If there are encoding options available:
            if( _encodings != null && _encodingField != null )
            {
                //Use the user-selected encoding instead.
                encoding = _encodings[_encodingField.getSelectedIndex()].getFullEncoding();
            }
            _byRaw = _videoControl.getSnapshot( _encodings[7].getFullEncoding() );
            //_byRaw = _videoControl.getSnapshot(null);
            //_byRaw = _videoControl.getSnapshot( "encoding=jpeg&width=640&height=480&quality=normal" );
            StopCamera();
            
            //Convert the byte array to a Bitmap image.
    		_image = Bitmap.createBitmapFromBytes(_byRaw, 0, -1, IMAGE_SCALING );
    		if (_imageField==null)
    			_imageField = new BitmapField(_image);
    		else
    			_imageField.setBitmap(_image);
    		add(_imageField);
    		add(_butAccept);
    		add(_butCancel);
        }
        catch(Exception e)
        {
            Dialog.alert( "ERROR " + e.getClass() + ":  " + e.getMessage() );
        }
    }
    
    void StopCamera(){
    	try{
    		if (_bLayoutCamera){
    			_bLayoutCamera=false;
    			_player.stop();
    			_videoControl.setVisible(false);
    			delete(_videoField);
    		}
    	}
        catch(Exception e)
        {
        	
        }
    }
    
    void SaveImage()
    {
    	try
		{       
			//Create the connection to a file that may or may not exist.
			FileConnection file = (FileConnection)Connector.open( FILE_NAME + EXTENSION );
			if (file.exists())
				file.delete();
			//We know the file doesn't exist yet, so create it.
			file.create();

			//Write the image to the file.
			OutputStream out = file.openOutputStream();
			out.write(_byRaw);
			//Close the connections.
			out.close();
			file.close();
		}
		catch(Exception e)
		{
			Dialog.alert( "ERROR " + e.getClass() + ":  " + e.getMessage() );
		}
    }
    public boolean invokeAction(int action){
    	boolean bret=false;
    	switch(action)
        {
            case ACTION_INVOKE: // Trackball click.
            	Capture();
            	bret=true;
            	break;
        }
    	return bret;
    }
    /*
    public void paint(Graphics graphics)
    {	      
		graphics.drawBitmap(0, getVerticalScroll(), _backgr.getWidth(), _backgr.getHeight(), _backgr, 0, 0);		
		super.paint(graphics);
    }*/
}
