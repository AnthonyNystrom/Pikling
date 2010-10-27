package pikling;

import net.rim.blackberry.api.browser.Browser;
import net.rim.blackberry.api.browser.BrowserSession;
import net.rim.device.api.system.Bitmap;
import net.rim.device.api.ui.*;
import net.rim.device.api.ui.component.BitmapField;
import net.rim.device.api.ui.component.Menu;

import javax.microedition.media.*;
import net.rim.device.api.ui.container.MainScreen;

public class MainLayout extends net.rim.device.api.ui.Manager {
	Bitmap _backgr;
	Bitmap _snap_button, _snap_button_press, _files, _files_press;
	FlagField _bmFieldLangSrc, _bmFieldLangDest;
	Player _player;
	ListenImage _fileListener=null;
    public static MainScreen _hs;
    Settings _settings;
    boolean _bUpadteLangSrc;
    
	protected MainLayout(long style, MainScreen hs, Settings settings){
		super(style);
		_settings=settings;        
		
		_backgr 		   = Bitmap.getBitmapResource("background.png");
		_snap_button       = Bitmap.getBitmapResource("snap_button.png");
		_snap_button_press = Bitmap.getBitmapResource("snap_button_f.png");
		_files			   = Bitmap.getBitmapResource("camera_roll.png");
		_files_press 	   = Bitmap.getBitmapResource("camera_roll_f.png");
		_hs                = hs;

		
		BitmapField bmFieldSnapUp = new BitmapField(_snap_button,BitmapField.FOCUSABLE | BitmapField.FIELD_HCENTER | BitmapField.FIELD_VCENTER)
		{	public void onFocus(int direction)
			{	setBitmap(_snap_button_press);}
			public void onUnfocus()
			{	setBitmap(_snap_button);}
			protected void drawFocus(Graphics graphics, boolean on){}
			protected boolean navigationClick(int status,  int time)
			{	try 
				{   
					PiklingScreen ps = (PiklingScreen)_hs;
					ps.ShowCamera();
				} 
				catch (Exception e) 
				{	System.out.println("Oh noes!!!"); 
                }
                return true;
			}
		};
		BitmapField bmFieldFiles = new BitmapField(_files,BitmapField.FOCUSABLE | BitmapField.FIELD_HCENTER | BitmapField.FIELD_VCENTER)
		{	public void onFocus(int direction)
			{	setBitmap(_files_press);}
			public void onUnfocus()
			{   setBitmap(_files);}
				protected void drawFocus(Graphics graphics, boolean on){}
				protected boolean navigationClick(int status,  int time){
					PiklingScreen ps = (PiklingScreen)_hs;
					ps.ShowThumbnails();					
					return true;
				}
		};
		FlagField bmFieldPref = new FlagField(BitmapField.FOCUSABLE |  BitmapField.FIELD_LEFT, "pref")
		{	
			protected boolean navigationClick(int status,  int time)
			{	try 
				{   PiklingScreen ps = (PiklingScreen)_hs;
					ps.ShowSettings();
				} 
				catch (Exception e) 
				{	System.out.println("Oh noes!!!"); 
                }
                return true;
			}
		};
		_bmFieldLangSrc = new FlagField(BitmapField.FOCUSABLE | BitmapField.FIELD_LEFT, _settings._sLangSrc.toLowerCase())
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
		_bmFieldLangDest = new FlagField(BitmapField.FOCUSABLE | BitmapField.FIELD_RIGHT, _settings._sLangDst.toLowerCase())
		{	
			protected boolean navigationClick(int status,  int time)
			{	try 
				{   PiklingScreen ps = (PiklingScreen)_hs;
					_bUpadteLangSrc = false;
					ps.ShowLanguage();
				} 
				catch (Exception e) 
				{	System.out.println("Oh noes!!!"); 
                }
                return true;
			}
		};
		FlagField bmFieldArrow = new FlagField(BitmapField.FOCUSABLE | BitmapField.FIELD_RIGHT | BitmapField.FIELD_VCENTER, "arrow_for_flags")
		{	
			protected boolean navigationClick(int status,  int time)
			{	try 
				{  _bUpadteLangSrc=true;
					String sTmp = _settings._sLangSrc;
					UpdateBmpLang(_settings._sLangDst);
					_bUpadteLangSrc=false;
					UpdateBmpLang(sTmp);
				} 
				catch (Exception e) 
				{	System.out.println("Oh noes!!!"); 
                }
                return true;
			}
		};		
		
		add(_bmFieldLangSrc);
		add(bmFieldArrow);
		add(_bmFieldLangDest);
		add(bmFieldSnapUp);
		add(bmFieldFiles);
		add(bmFieldPref);
	}
	protected void sublayout(int width, int height)
    {	Field field;
    	int x=0,y=0;
		for (int i = 0;  i < getFieldCount();  i++) {
			field= getField(i);
			switch (i){
			case 0:
	        	x=320;
	        	y=10;
				break;
			case 1:
				x+=5;
				y+=12;
				break;
			case 2:
				x+=5;
				y=10;
				break;
			case 3:
				x=width/2-96;
				y=height/2-30;
				break;
			case 4:
				x=width/2-96;
				y+=60+10;
				break;
			case 5:
				x=20;
				y=height-52-20;
				break;
			case 6:
				x=y=0;
				break;
			}
			setPositionChild(field,x,y);
            layoutChild(field, width, height);
            x+=field.getWidth();
		}
		setExtent(width,height);
    }
	private boolean initCamera() {
        /*try {
            _player = javax.microedition.media.Manager.createPlayer("capture://video");
            _player.realize();
            _player.start();
            VideoControl vc = (VideoControl) _player.getControl("VideoControl");
            Field viewFinder = (Field) vc.initDisplayMode(VideoControl.USE_GUI_PRIMITIVE, "net.rim.device.api.ui.Field");
            add(viewFinder);
            vc.setVisible(true);
            vc.setDisplayFullScreen(true);
        } catch (Exception me) {
            return false;
        }*/
        return true;
    }
	public void Close(){
		UiApplication.getUiApplication().removeFileSystemJournalListener(_fileListener);
        _fileListener = null;
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
	public void paint(Graphics graphics)
    {	      
		graphics.drawBitmap(0, 0, _backgr.getWidth(), _backgr.getHeight(), _backgr, 0, 0);
		super.paint(graphics);
    }
	
	public void UpdateSrcBmpLang(String sLangAbbrev){
		_bmFieldLangSrc.setBitmap(Bitmap.getBitmapResource(sLangAbbrev+".png"));
	}
	public void UpdateDstBmpLang(String sLangAbbrev){
		_bmFieldLangDest.setBitmap(Bitmap.getBitmapResource(sLangAbbrev+".png"));
	}
	public void UpdateBmpLang(String sLangAbbrev){
		if (_bUpadteLangSrc){
			UpdateSrcBmpLang(sLangAbbrev);
			_settings._sLangSrc = sLangAbbrev;
		}
		else{
			UpdateDstBmpLang(sLangAbbrev);
			_settings._sLangDst = sLangAbbrev;
		}	
		_settings.Save();
	}
   protected void makeMenu(Menu menu, int instance)
   {
	   if (_hs.getField(0)==this){
		   PiklingScreen ps = (PiklingScreen)_hs;
		   if (ps._result._sImgToProcess!=""){
			   MenuItem mnuLastResult = new MenuItem("Last Result",100,0)
			   {
			       public void run()
			       {   PiklingScreen ps = (PiklingScreen)_hs;
			    	   ps._result.Show("");
			       }
			   };       
			   menu.add(mnuLastResult);
			   menu.addSeparator();
		   }
		   MenuItem mnu7T = new MenuItem("7Touch",100,0)
		   {
		       public void run()
		       {
		    	   BrowserSession site = Browser.getDefaultSession();
		           site.displayPage("http://m.7touchgroup.com") ;
		       }
		   };      
		   menu.add(mnu7T);
	   }
   }
	
}
