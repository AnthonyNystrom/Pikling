package pikling;

import java.io.InputStream;

import net.rim.device.api.ui.FontFamily;
import net.rim.device.api.ui.Manager;
import net.rim.device.api.ui.Field;
import net.rim.device.api.ui.component.EditField;
import net.rim.device.api.ui.component.LabelField;
import net.rim.device.api.ui.container.HorizontalFieldManager;
import net.rim.device.api.ui.container.VerticalFieldManager;
import net.rim.device.api.math.Fixed32;
import net.rim.device.api.system.Bitmap;
import net.rim.device.api.system.Characters;
import net.rim.device.api.system.EncodedImage;
import net.rim.device.api.ui.Graphics;
import net.rim.device.api.ui.Font;

public class CustomTextBox extends VerticalFieldManager 
{
    private int _width;
    private int _height;
    
    Bitmap _bmpBack;
    
    private EditField editField;
    private LabelField labelField;
    
    public CustomTextBox(int iWidth, String sLabel, String sValue) throws Exception
    {
        super(NO_VERTICAL_SCROLL);
        _width=iWidth;
        
        InputStream input = Class.forName("pikling.Pikling").getResourceAsStream("/field.png");
        int len = input.available();
        byte[] data = new byte[len];
        input.read(data);
        EncodedImage backgr = EncodedImage.createEncodedImage(data,0,data.length);
        _bmpBack = this.scaleToFactor(backgr, _width, backgr.getHeight()).getBitmap();
		FontFamily theFam = FontFamily.forName(getFont().getFontFamily().getName());
        Font fnt_thin = theFam.getFont(Font.PLAIN, 14);
        _height = backgr.getHeight();
        
        HorizontalFieldManager hfm =new HorizontalFieldManager(Manager.HORIZONTAL_SCROLL);

        editField = new EditField("", sValue){
            public void paint(Graphics g) {
            getManager().invalidate();
            super.paint(g);
          }
        };

        hfm.add(editField);
        add(hfm);
        
        labelField = new LabelField(sLabel);
        labelField.setFont(fnt_thin);
        add(labelField);

    }    
    
    
    protected void sublayout(int width, int height)
    {
    	int iPadX = 10;
    	int iPadY = 20;
        Field field = getField(0);
        layoutChild(field, _width-iPadX*2, _height);
        setPositionChild(field, iPadX, iPadY);
        setExtent(width, height);
        
        field = getField(1);
        layoutChild(field, _width, _height);
        setPositionChild(field, iPadX/2, 2);
        setExtent(width, height);
    }
    
    public static EncodedImage scaleToFactor(EncodedImage encoded, int iW, int iH) {
    	int width = iW;
        int height = iH;        
        
        int currentWidthFixed32 = Fixed32.toFP(encoded.getWidth());
        int currentHeightFixed32 = Fixed32.toFP(encoded.getHeight());
        
        int requiredWidthFixed32 = Fixed32.toFP(width);
        int requiredHeightFixed32 = Fixed32.toFP(height);
        
        int scaleXFixed32 = Fixed32.div(currentWidthFixed32, requiredWidthFixed32);
        int scaleYFixed32 = Fixed32.div(currentHeightFixed32, requiredHeightFixed32);
        return encoded.scaleImage32(scaleXFixed32, scaleYFixed32);
    }
   
    protected void paint(Graphics graphics)
    {
    	graphics.drawBitmap(0, 0, _width, _height, _bmpBack, 0, 0);
        super.paint(graphics);        
    }
    
    protected boolean keyChar(char ch, int status, int time)
    {
        if (ch == Characters.ENTER)
        {
            return true;
        }
        else
        {
            return super.keyChar(ch, status, time);
        }
    }
    
    public String getText()
    {
        return editField.getText();
    }
    
    public void setText(final String text)
    {
    	editField.setText(text);
    }    
}