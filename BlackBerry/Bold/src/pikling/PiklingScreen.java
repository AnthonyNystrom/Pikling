package pikling;

import net.rim.device.api.applicationcontrol.ApplicationPermissions;
import net.rim.device.api.applicationcontrol.ApplicationPermissionsManager;
import net.rim.device.api.system.Display;
import net.rim.device.api.ui.container.MainScreen;
import net.rim.device.api.ui.*;
import javax.microedition.location.*;

//create a new screen that extends MainScreen, which provides
//default standard behavior for BlackBerry applications
final class PiklingScreen extends MainScreen
{
	MainLayout _mainframe;
	public Settings _settings;
	SelectLanguage _selectlang;
	CameraView _cameraview;
	Thumbnails _thumbnails;
	Result _result;
	static MainScreen _hs;
    private int[]          _blackImage=null;
    private static boolean _showBlack=false;
    public  static String  _sMsgBlack;
    public  static boolean _toggleHide;
    int _iW, _iH;    
    Font _fontBlackMsg;
    LocationProvider _locationProvider;
    public double _longitude, _latitude;

    public PiklingScreen()
	{
          //invoke the MainScreen constructor
		super(NO_VERTICAL_SCROLL|NO_VERTICAL_SCROLLBAR);
		//addKeyListener(new TestKeyPadListener());
		
		try{
	         boolean bcallp=false;
	         ApplicationPermissions permissions = ApplicationPermissionsManager.getInstance().getApplicationPermissions();
	         ApplicationPermissions ap2 = new ApplicationPermissions();
	         
	         if (permissions.getPermission(ApplicationPermissions.PERMISSION_FILE_API)!=ApplicationPermissions.VALUE_ALLOW){
	            ap2.addPermission(ApplicationPermissions.PERMISSION_FILE_API);bcallp=true;}
	         
	         if (permissions.getPermission(ApplicationPermissions.PERMISSION_INTERNET)!=ApplicationPermissions.VALUE_ALLOW){
	            ap2.addPermission(ApplicationPermissions.PERMISSION_INTERNET);bcallp=true;}
	         
	         if (permissions.getPermission(ApplicationPermissions.PERMISSION_LOCATION_DATA)!=ApplicationPermissions.VALUE_ALLOW){
	            ap2.addPermission(ApplicationPermissions.PERMISSION_LOCATION_DATA);bcallp=true;}
	         
	         if (permissions.getPermission(ApplicationPermissions.PERMISSION_WIFI)!=ApplicationPermissions.VALUE_ALLOW){
	            ap2.addPermission(ApplicationPermissions.PERMISSION_WIFI);bcallp=true;}
	         
	         if (permissions.getPermission(ApplicationPermissions.PERMISSION_MEDIA)!=ApplicationPermissions.VALUE_ALLOW){
		            ap2.addPermission(ApplicationPermissions.PERMISSION_MEDIA);bcallp=true;}

	         if (permissions.getPermission(ApplicationPermissions.PERMISSION_ORGANIZER_DATA)!=ApplicationPermissions.VALUE_ALLOW){
		            ap2.addPermission(ApplicationPermissions.PERMISSION_ORGANIZER_DATA);bcallp=true;}

	         if (permissions.getPermission(ApplicationPermissions.PERMISSION_RECORDING)!=ApplicationPermissions.VALUE_ALLOW){
		            ap2.addPermission(ApplicationPermissions.PERMISSION_RECORDING);bcallp=true;}
	         
	         if (permissions.getPermission(ApplicationPermissions.PERMISSION_PHONE)!=ApplicationPermissions.VALUE_ALLOW){
		            ap2.addPermission(ApplicationPermissions.PERMISSION_PHONE);bcallp=true;}
	         
	         if (bcallp){
	            ApplicationPermissionsManager.getInstance().invokePermissionsRequest( ap2 );
	         }
	      }catch(Exception e)
	      {System.out.println("Exception while setting permissions "+e);}  
	      		
		
		_hs=this;
		_fontBlackMsg  = getFont();
		ShowBlackMessage("Wait please...", false);
		
		UiApplication.getUiApplication().invokeLater(new Runnable()
		{
			public void run()
	        {
				_settings  = new Settings(MainLayout.NO_VERTICAL_SCROLLBAR | MainLayout.NO_VERTICAL_SCROLL | MainLayout.USE_ALL_WIDTH | MainLayout.USE_ALL_HEIGHT, _hs);
				_mainframe = new MainLayout(MainLayout.NO_VERTICAL_SCROLLBAR | MainLayout.NO_VERTICAL_SCROLL | MainLayout.USE_ALL_WIDTH | MainLayout.USE_ALL_HEIGHT, _hs, _settings);
				_selectlang= new SelectLanguage(MainLayout.NO_VERTICAL_SCROLLBAR | MainLayout.NO_VERTICAL_SCROLL | MainLayout.USE_ALL_WIDTH | MainLayout.USE_ALL_HEIGHT, _hs);
				_cameraview= new CameraView(MainLayout.NO_VERTICAL_SCROLLBAR | MainLayout.NO_VERTICAL_SCROLL | MainLayout.USE_ALL_WIDTH | MainLayout.USE_ALL_HEIGHT, _hs);
				_thumbnails = new Thumbnails(MainLayout.VERTICAL_SCROLLBAR | MainLayout.VERTICAL_SCROLL | MainLayout.USE_ALL_WIDTH | MainLayout.USE_ALL_HEIGHT, _hs);
				_result = new Result(MainLayout.VERTICAL_SCROLLBAR | MainLayout.VERTICAL_SCROLL | MainLayout.USE_ALL_WIDTH | MainLayout.USE_ALL_HEIGHT, _hs);				
				add(_mainframe);
				// check if email is set
				if (_settings._sEmail=="" || _settings._sEmail.indexOf(".")<0 || _settings._sEmail.indexOf("@")<0){
					ShowSettings();
				}
				startLocationUpdate();
				ShowBlackMessage("", false);
	        }
		});
	}
	public void close() 
    {  
	    if ( _locationProvider != null ) 
	    {	_locationProvider.reset();
	        _locationProvider.setLocationListener(null, -1, -1, -1);
	    }
		_mainframe.Close();
		super.close();
    }
	public void ShowSettings(){
		_settings.Show();
	}
	public void ShowLanguage(){
		_selectlang.Show();
	}
	public void ShowCamera(){
		_cameraview.Show();
	}
	public void ShowResult(String sImgToProcess){
		_result.Show(sImgToProcess);
	}
	public void ShowThumbnails(){
		_thumbnails.Show();
	}
	//override the onClose() method to display a dialog box to the user
	//with "Goodbye!" when the application is closed
	public boolean onClose()
	{	_cameraview.Close();		
		System.exit(0);
		return true;
	}
	public void paint(Graphics graphics)
    {
		if (_blackImage==null)
			CreateBlackImage(Display.getWidth(), Display.getHeight());
	    
		if (_showBlack){
	         super.paint(graphics);	         
	         graphics.drawARGB(_blackImage, 0, _iW, 0, 0, _iW, _iH);
	         //graphics.setFont(_fontBlackMsg);
	         graphics.setColor(0xffffffff);
	         	         
	         int iNCharsforLine = 43;
	         int iNrow            = (int)_sMsgBlack.length()/iNCharsforLine+1;
	         int iY               =_iH/2-(_fontBlackMsg.getHeight()*(iNrow/2));
	         String sLine;
	         int iIndexch, iNch=iNCharsforLine;
	         int iHFont = _fontBlackMsg.getHeight();
	         for (int i=0; i<iNrow && iY+iHFont <_iH; i++){
	            iIndexch = i*iNCharsforLine;
	            if (iIndexch+iNch>_sMsgBlack.length())
	               iNch=0;
	            if (iNch!=0)
	               sLine = _sMsgBlack.substring(iIndexch, iIndexch+iNch);
	            else
	               sLine = _sMsgBlack.substring(iIndexch);
	            graphics.drawText(sLine, _iW/2-(_fontBlackMsg.getAdvance(sLine)/2), iY);
	            iY+=_fontBlackMsg.getHeight()+3;
	         }
	         if (_toggleHide)
	            graphics.drawText("Press a button to continue", 0, _iH-iHFont);
	    }
	    else
	    	super.paint(graphics);
	    	    	
    }
	/*private class TestKeyPadListener implements KeyListener {
		public boolean keyChar( char key, int status, int time ) {
			   
    		return false;
        }
        
    	public boolean keyDown(int keycode, int time) { 
    		System.out.println("KeyDown: " + keycode);
    		return false; 
        }
    	public boolean keyRepeat(int keycode, int time) { return false; }
    	public boolean keyStatus(int keycode, int time) { return false; }
    	public boolean keyUp(int keycode, int time) { return false; }
    }*/
	// Show full screen black with message. Set sMsg="" to remove black screen
	public static void ShowBlackMessage(String sMsg, boolean toggleHide)
	{
	      _sMsgBlack = sMsg;
	      if (sMsg.equals(""))
	         _showBlack=false;
	      else
	         _showBlack=true;
	      _toggleHide = toggleHide;
	      _hs.invalidate();
   }
	void CreateBlackImage(int iW, int iH)
	{
		_iW=iW; _iH=iH;
		int iSzPlanes = _iW * _iH;
		_blackImage = new int[iSzPlanes];
		int pixel = 0xaa000000;
		for(int i = 0; i < iSzPlanes; ++i)
			_blackImage [i] = pixel;
    }
	/**
    * @see net.rim.device.api.system.KeyListener#keyDown(int,int)
    */
   public boolean keyDown( int keycode, int time ) 
   {
      boolean bret=false;
      if (_toggleHide)
         ShowBlackMessage("",false);
      else if (_settings.isShowed()){
          bret = _settings.MykeyDown(keycode, time);
      }
      else if (_selectlang.isShowed())
    	  bret= _selectlang.MykeyDown(keycode, time);
      else if (_cameraview.isShowed())
    	  bret= _cameraview.MykeyDown(keycode, time);
      else if (_thumbnails.isShowed())
    	  bret= _thumbnails.MykeyDown(keycode, time);
      else if (_result.isShowed())
    	  bret= _result.MykeyDown(keycode, time);    	  
      
      if (!bret)
         return super.keyDown(keycode, time);
      
      return bret;
   }
   /**
    * Handle trackball click events.
    * @see net.rim.device.api.ui.Screen#invokeAction(int)
    */   
   protected boolean invokeAction(int action)
   {
       boolean handled = super.invokeAction(action); 
       
       if(!handled)
       {
           switch(action)
           {
               case ACTION_INVOKE: // Trackball click.
               {         
				  if (_toggleHide)
				     ShowBlackMessage("",false);
				  else if (_cameraview.isShowed())
					  _cameraview.invokeAction(action);
				  /*else if (_thumbnails.isShowed())
					  _thumbnails.invokeAction(action);*/
				  return true;
               }
           }
       }        
       return handled;                
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
    * Invokes the Location API with the default criteria.
    * 
    * @return True if the Location Provider was successfully started; false otherwise.
    */
   private boolean startLocationUpdate()
   {
       boolean retval = false;
       
       try
       {
           _locationProvider = LocationProvider.getInstance(null);
           
           if ( _locationProvider == null ){
        	   System.err.println("Failed to instantiate the LocationProvider object, exiting...");
        	   //ShowBlackMessage("Init GPS KO", true);
           }
           else
           {
               // Only a single listener can be associated with a provider, and unsetting it 
               // involves the same call but with null, therefore, no need to cache the listener
               // instance request an update every second.
               _locationProvider.setLocationListener(new LocationListenerImpl(), 1, 1, 1);
               retval = true;
               //ShowBlackMessage("Init GPS OK", true);
           }
       }
       catch (LocationException le)
       {
    	   //ShowBlackMessage("Init GPS KO Exc.", true);
           System.err.println("Failed to instantiate the LocationProvider object, exiting...");
           System.err.println(le); 
       }        
       return retval;
   }
   /**
    * Implementation of the LocationListener interface.
    */
   private class LocationListenerImpl implements LocationListener
   {

	public void locationUpdated(LocationProvider provider, Location location) {
		// TODO Auto-generated method stub
		
        _longitude = location.getQualifiedCoordinates().getLongitude();
        _latitude = location.getQualifiedCoordinates().getLatitude();
	}

	public void providerStateChanged(LocationProvider provider, int newState) {
		// TODO Auto-generated method stub
		
	}
	   
   }
}