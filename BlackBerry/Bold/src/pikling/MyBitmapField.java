package pikling;


import net.rim.device.api.system.Bitmap;
import net.rim.device.api.ui.Color;
import net.rim.device.api.ui.Graphics;
import net.rim.device.api.ui.component.BitmapField;

public  class MyBitmapField extends BitmapField {

	boolean _hasFocus;
	String _sTag;
	protected MyBitmapField(Bitmap bitmap, long style, String sTag){
		super(bitmap,style);
		_sTag = sTag;
	}
	
	protected void drawFocus(Graphics graphics,boolean on){
		if (on){
            graphics.setColor(Color.WHITE);
            graphics.drawRect(0,0,getWidth(),getHeight());
            graphics.setColor(Color.BLACK);
            graphics.setStipple(0xcccccccc);
            //graphics.drawRect(0,0,getWidth(),getHeight());
            graphics.drawRoundRect(0, 0, getWidth(),getHeight(), 10, 10);
        }
	}
	public void onFocus(int direction)
	{	_hasFocus=true;
		invalidate();
	}
	public void onUnfocus()
	{	_hasFocus=false;
		invalidate();
	}
	protected void MyClicked(String sTag){
		
	}
	
	protected boolean navigationClick(int status, int time){
		MyClicked(_sTag);
		return true;
	}
	public void paint(Graphics graphics)
    {
		if (_hasFocus){
			graphics.setColor(0xEAA103);
	    	graphics.fillRoundRect(0, 0, getWidth(),getHeight(), 10, 10);
	    	//graphics.drawRect(0, 0, getWidth(),getHeight());
		}
		super.paint(graphics);
    }    

}
