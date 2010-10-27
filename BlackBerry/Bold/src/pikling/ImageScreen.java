/*
 * ImageScreen.java
 *
 * Copyright � 1998-2008 Research In Motion Ltd.
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

import java.io.OutputStream;

import javax.microedition.io.Connector;
import javax.microedition.io.file.FileConnection;

import net.rim.device.api.system.Bitmap;
import net.rim.device.api.ui.Field;
import net.rim.device.api.ui.FieldChangeListener;
import net.rim.device.api.ui.UiApplication;
import net.rim.device.api.ui.component.BitmapField;
import net.rim.device.api.ui.component.ButtonField;
import net.rim.device.api.ui.component.Dialog;
import net.rim.device.api.ui.component.LabelField;
import net.rim.device.api.ui.container.HorizontalFieldManager;
import net.rim.device.api.ui.container.MainScreen;

public final class ImageScreen extends MainScreen
{
	/** The down-scaling ratio applied to the snapshot Bitmap. */
	private static final int IMAGE_SCALING = 7;

	/** The base file name used to store pictures */
	private static final String FILE_NAME = System.getProperty("fileconn.dir.photos") + "IMAGE";

	/** The extension of the pictures to be saved */
	private static final String EXTENSION = ".bmp";

	/** A counter for the number of snapshots taken. */
	private static int _counter;

	/** A reference to the current screen for listeners. */
	private ImageScreen _imageScreen;

	/**
	 * Constructor.
	 * @param raw A byte array representing an image.
	 */
	public ImageScreen( final byte[] raw )
	{
		//A reference to this object, to be used in listeners.
		_imageScreen = this;

		setTitle( new LabelField( "IMAGE " + _counter, LabelField.ELLIPSIS | LabelField.USE_ALL_WIDTH ) );

		//Convert the byte array to a Bitmap image.
		Bitmap image = Bitmap.createBitmapFromBytes( raw, 0, -1, IMAGE_SCALING );

		//Create two field managers to center the screen's contents.
		HorizontalFieldManager hfm1 = new HorizontalFieldManager( Field.FIELD_HCENTER );
		HorizontalFieldManager hfm2 = new HorizontalFieldManager( Field.FIELD_HCENTER );

		//Create the field that contains the image.
		BitmapField imageField = new BitmapField( image );
		hfm1.add( imageField );

		//Create the SAVE button which returns the user to the main camera.
		//screen and saves the picture as a file
		ButtonField photoButton = new ButtonField( "Save" );
		photoButton.setChangeListener( new SaveListener(raw) );
		hfm2.add(photoButton);

		//Create the CANCEL button which returns the user to the main camera
		//screen without saving the picture.
		ButtonField cancelButton = new ButtonField( "Cancel" );
		cancelButton.setChangeListener( new CancelListener() );
		hfm2.add(cancelButton);

		//Add the field managers to the screen.
		add( hfm1 );
		add( hfm2 );
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
    				return true;
    			}
			}
		}        
		return handled;          
	}

	/**
	 * A listener used for the "Save" button.
	 */
	private class SaveListener implements FieldChangeListener
	{
	    /** A byte array representing an image. */
		private byte[] _raw;

		/**
		 * Constructor.
		 * @param raw A byte array representing an image.
		 */
		public SaveListener(byte[] raw)
		{
			_raw = raw;
		}

		/**
		 * Saves the image as a file in the BlackBerry filesystem.
		 */
		public void fieldChanged(Field field, int context)
		{
			try
			{       
				//Create the connection to a file that may or
				//may not exist.
				FileConnection file = (FileConnection)Connector.open( FILE_NAME + _counter + EXTENSION );

				//If the file exists, increment the counter until we find
				//one that hasn't been created yet.
				while( file.exists() )
				{
					file.close();
					++_counter;
					file = (FileConnection)Connector.open( FILE_NAME + _counter + EXTENSION );
				}

				//We know the file doesn't exist yet, so create it.
				file.create();

				//Write the image to the file.
				OutputStream out = file.openOutputStream();
				out.write(_raw);

				//Close the connections.
				out.close();
				file.close();
			}
			catch(Exception e)
			{
				Dialog.alert( "ERROR " + e.getClass() + ":  " + e.getMessage() );
			}

			//Inform the user where the file has been saved.
			Dialog.inform( "Saved to " + FILE_NAME + _counter + EXTENSION );

			//Increment the image counter.
			++_counter;

			//Return to the main camera screen.
			UiApplication.getUiApplication().popScreen( _imageScreen );
		}
	}

	/**
	 * A listener used for the "Cancel" button.
	 */
	private class CancelListener implements FieldChangeListener
	{
	    /**
	     * Return to the main camera screen.
	     */
		public void fieldChanged(Field field, int context)
		{
			UiApplication.getUiApplication().popScreen( _imageScreen );
		}
	}
}
