package pikling;


import net.rim.device.api.system.Bitmap;
import net.rim.device.api.ui.Graphics;
import net.rim.device.api.ui.component.BitmapField;

public  class FlagField extends BitmapField {

	boolean _hasFocus;
	String _sAbbrev;
	boolean _bRoundRect=true;
	String _sTag;
	
	public void SetTag(String sTag){
		_sTag = sTag;
	}
	public String GetTag(){
		return _sTag;
	}
	protected FlagField(long style, String sAbbrev){
		super(Bitmap.getBitmapResource(sAbbrev+".png"),style);
		_sAbbrev = sAbbrev;
	}
	public void onFocus(int direction)
	{	_hasFocus=true;
		invalidate();
	}
	public void onUnfocus()
	{	_hasFocus=false;
		invalidate();
	
	}
	protected void drawFocus(Graphics graphics, boolean on){}
	
	public void paint(Graphics graphics)
    {
		if (_bRoundRect){
			
			if (_hasFocus){
				graphics.setColor(0xEAA103);
		    	graphics.fillRoundRect(0, 0, getWidth(),getHeight(), 10, 10);
			}
			super.paint(graphics);
		}
		else{
			super.paint(graphics);
			if (_hasFocus){
				graphics.setColor(0xEAA103);
				
				int x, y, w, h;
				x=0;y=0; w=getWidth()-1; h=getHeight()-1;
				int i=0;
				for (i=0; i<3; i++){
					graphics.drawLine(x, y, x+w, y);
					graphics.drawLine(x+w, y, x+w, y+h);
					graphics.drawLine(x+w, y+h, x, y+h);
					graphics.drawLine(x, y+h, x, y);
					
					x++; y++; w-=2; h-=2;
				}
			}
		}
    }    

}
