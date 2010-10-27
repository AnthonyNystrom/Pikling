package pikling;

import net.rim.device.api.ui.*;

/*
 * BlackBerry applications that provide a user interface
 * must extend UiApplication.
 */
public class Pikling extends UiApplication
{
        public static void main(String[] args)
        {
                //create a new instance of the application
                //and start the application on the event thread
        		Pikling theApp = new Pikling();
                theApp.enterEventDispatcher();
        }
        public Pikling()
        {
                //display a new screen
                pushScreen(new PiklingScreen());
        }
}


