package pikling;

import net.rim.device.api.ui.Field;
import net.rim.device.api.ui.Graphics;
import net.rim.device.api.ui.container.VerticalFieldManager;

public class ManagerList extends VerticalFieldManager{
	int _iHeight;
	int _iWidth;
	public ManagerList(long lstyle, int iHeight, int iWidth){
		super(lstyle);
		_iHeight=iHeight;
		_iWidth =iWidth;
	}
	protected void sublayout(int width, int height)
    {	Field field;
		for (int i = 0;  i < getFieldCount();  i++) {
			field= getField(i);
	        layoutChild(field, width, height);            
		}
		setExtent(_iWidth,_iHeight);
    }
	public void paint(Graphics graphics)
    {	      
		graphics.setBackgroundColor(0xFFFFFF);
		graphics.clear();
		super.paint(graphics);
    }

	

}
