package pikling;
import javax.microedition.pim.PIMException;

import net.rim.blackberry.api.pdap.BlackBerryContact;
import net.rim.blackberry.api.pdap.BlackBerryContactList;
import net.rim.blackberry.api.pdap.BlackBerryPIM;
import net.rim.device.api.system.Bitmap;
import net.rim.device.api.system.Display;
import net.rim.device.api.ui.Field;
import net.rim.device.api.ui.Graphics;
import net.rim.device.api.ui.component.BitmapField;
import net.rim.device.api.ui.container.MainScreen;

public class GetInfo extends net.rim.device.api.ui.Manager {
	Bitmap _backgr;
	public static PiklingScreen _hs;

	boolean _bShowed;
	CustomTextBox _EditData;
	Field _PushField;
	boolean _bContactEmail;
	
	protected GetInfo(long style, MainScreen hs, boolean bContactEmail, String sLabel, String sValue) {
		super(style);
		_hs = (PiklingScreen)hs;
		_bContactEmail = bContactEmail;
		try{
			_backgr = Bitmap.getBitmapResource("background2.png");
	        
			_EditData = new CustomTextBox(350, sLabel, sValue)
			{
				protected boolean navigationClick(int status,  int time)
				{
					String sRet = ShowAddressBook(_bContactEmail);
					if (sRet!="")
						_EditData.setText(sRet);
					return true;
				}
			};
			add(_EditData);
			FlagField bmpf = new FlagField(BitmapField.FOCUSABLE | BitmapField.FIELD_HCENTER | BitmapField.FIELD_VCENTER, "accept")
			{
				protected boolean navigationClick(int status,  int time)
				{
					Hide();
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
    	int iStep=30;
		for (int i = 0;  i < getFieldCount();  i++) {
			field= getField(i);
			switch (i){
			case 0:// field
				x=15;
				y=Display.getHeight()/2-iStep;
				break;
			case 1:// accept
				x=390;
				y=Display.getHeight()/2-iStep;
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
	    		Hide();bret=true;break;
	     }
	     return bret;
	}
	private String ShowAddressBook(boolean bGetEmail) 
	{	String sRet="";
        try {
        	
        	String sEmailContact;
        	String sMobileNumberContact;
        	
        	sEmailContact="";
        	sMobileNumberContact="";
            BlackBerryContactList contacts = (BlackBerryContactList)BlackBerryPIM.getInstance().openPIMList(BlackBerryPIM.CONTACT_LIST, BlackBerryPIM.READ_WRITE);
            BlackBerryContact contact = (BlackBerryContact)contacts.choose(null, BlackBerryContactList.AddressTypes.EMAIL,true);
            
            if (contact!=null){
                int numValues = 0;
                numValues = contact.countValues(BlackBerryContact.TEL);
                sEmailContact = contact.getString(BlackBerryContact.EMAIL, 0);

                for (int i = 0; i < numValues; i++) {
                    if (contact.getAttributes(BlackBerryContact.TEL, i) == BlackBerryContact.ATTR_MOBILE) {
                        sMobileNumberContact = contact.getString(BlackBerryContact.TEL, i);                    
                        break;
                    }
                }
            }
            if (bGetEmail)
            	sRet=sEmailContact;
            else
            	sRet=sMobileNumberContact;
        } catch (PIMException ex) {
            ex.printStackTrace();
        }    
        return sRet;
    }
}
