/*
 * FileExplorerDemoListFieldImpl.java
 *
 * Copyright © 1998-2008 Research In Motion Ltd.
 * 
 * Note: For the sake of simplicity, this sample application may not leverage
 * resource bundles and resource strings.  However, it is STRONGLY recommended
 * that application developers make use of the localization features available
 * within the BlackBerry development platform to ensure a seamless application
 * experience across a variety of languages and geographies.  For more information
 * on localizing your application, please refer to the BlackBerry Java Development
 * Environment Development Guide associated with this release.
 */

package pikling;

import java.util.Vector;
import net.rim.device.api.system.Characters;
import net.rim.device.api.ui.Manager;
import net.rim.device.api.ui.component.ListField;
import net.rim.device.api.ui.component.ListFieldCallback;
import net.rim.device.api.system.*;
import net.rim.device.api.ui.*;


/**
 * ListField that contains file holder information.
 */
/*package*/ class FileExplorerDemoListFieldImpl extends ListField implements ListFieldCallback
{
   
    private Vector _elements = new Vector();
    
    /**
     * Constructor.  Sets itself as the callback.
     */
    FileExplorerDemoListFieldImpl() 
    {
        setCallback(this);
    }

    
    /**
     * Adds the provided element to this list field.
     * 
     * @param element The element to be added.
     */
    void add(Object element) 
    {
        _elements.addElement(element);
        setSize(getSize());
    }

    
    /**
     * @see net.rim.device.api.ui.component.ListFieldCallback#drawListRow(ListField , Graphics , int , int , int)
     */
    public void drawListRow(ListField listField, Graphics graphics, int index, int y, int width) 
    {
        if (index < getSize()) 
        {
            FileExplorerDemoFileHolder fileholder = (FileExplorerDemoFileHolder)_elements.elementAt(index);
            
            String text;
            
            if (fileholder.isDirectory()) 
            {
                text = fileholder.getPath();
            } 
            else 
            {
                text = fileholder.getFileName();
            }
            
            graphics.drawText(text, 0, y);
        }
    }

    
    /**
     * @see net.rim.device.api.ui.component.ListFieldCallback#get(ListField , int)
     */
    public Object get(ListField listField, int index) 
    {
        if (index >= 0 && index < getSize())
        {
            return _elements.elementAt(index);
        }
        
        return null;
    }

    
   /**
    * @see net.rim.device.api.ui.component.ListFieldCallback#getPreferredWidth(ListField)
    */
    public int getPreferredWidth(ListField listField) 
    {
        return Display.getWidth();    	
    }

    /**
    * @see net.rim.device.api.ui.component.ListFieldCallback#indexOfList(ListField , String , int)
    */
    public int indexOfList(ListField listField, String prefix, int start) 
    {
        return listField.indexOfList(prefix,start);
    }

    
    /**
     * Allows space bar to page down.
     * 
     * @see net.rim.device.api.ui.Screen#keyChar(char , int , int)
     */
    public boolean keyChar(char key, int status, int time)
    {
        if (getSize() > 0 && key == Characters.SPACE) 
        {
            getScreen().scroll(Manager.DOWNWARD);
            return true;
        }
        
        return super.keyChar(key, status, time);
    }

    
    /**
     * Retrieves the number of elements in list field.
     * 
     * @return The number of elements in this list field.
     */
    public int getSize() 
    {
        return (_elements != null) ? _elements.size() : 0;
    }
    
    
    /**
     * Removes the element at the provided index from this list field.
     * 
     * @param index The index of the element to remove.
     */
    void remove(int index) 
    {
        _elements.removeElementAt(index);
        setSize(getSize());
    }

    
    /**
     * Removes all elements from this list field.
     */
    void removeAll() 
    {
        _elements.removeAllElements();
        setSize(0);
    }
}

