package pikling;

import net.rim.device.api.io.file.FileSystemJournal;
import net.rim.device.api.io.file.FileSystemJournalEntry;
import net.rim.device.api.io.file.FileSystemJournalListener;
import net.rim.device.api.system.*;
import net.rim.device.api.ui.Keypad;
import javax.microedition.io.file.FileConnection;
import java.io.InputStream;
import javax.microedition.io.Connector;
import java.io.*;
import javax.microedition.io.*;


final class ListenImage implements FileSystemJournalListener
{
	private long _lastUSN = 0;
	private SocketConnection _sc;
	private DataInputStream _sci  = null;
	private DataOutputStream _sco = null;
	private byte []_byIDProcess = new byte[15];
	private byte []_bySrc;
	private byte []_byDest;
	private byte _byTranslator;
	
	public ListenImage(){
	}
	/**
     * Notified when FileSystem event occurs.
     */
    public void fileJournalChanged() 
    {
    	long nextUSN = FileSystemJournal.getNextUSN();
    	for (long lookUSN = nextUSN-1; lookUSN >= _lastUSN; lookUSN--) {
    		FileSystemJournalEntry entry = FileSystemJournal.getEntry(lookUSN);
    		if (entry == null)  // we didn't find an entry.
    			break;
    		String path = entry.getPath();
    		if (path != null) {
    			//if (path.endsWith("png") |path.endsWith("jpg") | path.endsWith("bmp") | path.endsWith("gif") ){
    			if (path.endsWith("jpg") ){
    				switch (entry.getEvent()) {
    				case FileSystemJournalEntry.FILE_ADDED:
    					//either a picture was taken or a or a picture was added to the BlackBerry device
    					/*CloseRecorder();
    					PiklingScreen.ShowBlackMessage("UPLOAD", false);
    					if (OpenConnection()){
    						CloseConnection();
    					}*/
    					break;
    					case FileSystemJournalEntry.FILE_DELETED:
    					//a picture was removed from the BlackBerry device;
    					break;
    				}
    			}
    		}
    	}
    	_lastUSN = nextUSN;
    }
    public void CloseRecorder()
    {
    	try{
    		wait(1000);
	    	EventInjector.KeyCodeEvent kd = new EventInjector.KeyCodeEvent ( EventInjector.KeyCodeEvent.KEY_DOWN , (char)Keypad.KEY_MENU, 0);	    	
	    	EventInjector.KeyCodeEvent ku = new EventInjector.KeyCodeEvent ( EventInjector.KeyCodeEvent.KEY_UP , (char)Keypad.KEY_MENU, 0);
	        EventInjector.invokeEvent(kd);
	        EventInjector.invokeEvent(ku);
	        EventInjector.invokeEvent( new EventInjector.NavigationEvent ( EventInjector.NavigationEvent.NAVIGATION_MOVEMENT, -400, 400, KeypadListener.STATUS_TRACKWHEEL));
	        EventInjector.invokeEvent( new EventInjector.NavigationEvent ( EventInjector.NavigationEvent.NAVIGATION_CLICK, -400, 400, KeypadListener.STATUS_TRACKWHEEL));
	        EventInjector.invokeEvent( new EventInjector.NavigationEvent ( EventInjector.NavigationEvent.NAVIGATION_UNCLICK, -400, 400, KeypadListener.STATUS_TRACKWHEEL ));
    	}
        catch(Exception ex)
        {
        	
        }
    }

    void CloseConnection(){
    	if (_sc!=null){
    		try{_sci.close();
    			_sco.close();
    			_sc.close();}catch(IOException ex) {}
    		_sci=null;
    		_sco=null;
    		_sc=null;
    	}
    }
    boolean OpenConnection(){
        String sConnection;
        boolean bret=false;
        boolean bWifiWay=true;
        if (bWifiWay)
           sConnection = "socket://69.21.114.100:8080;DeviceSide=True;interface=wifi";
        else
            sConnection = "socket://69.21.114.100:8080;DeviceSide=True;";
           
        try{
           _sc = (SocketConnection)Connector.open(sConnection,Connector.READ_WRITE);
           _sc.setSocketOption(SocketConnection.SNDBUF, 11264);
           _sci = _sc.openDataInputStream(); 
           _sco = _sc.openDataOutputStream();

           bret=true;
        }catch(IOException ex) {
           //_hs.ShowBlackMessage("Server not found. Please check your connection status", true);
        	_sc=null;
        }catch(IllegalArgumentException ex)
        {  _sc=null;
        }
        return bret;
    }
    boolean UploadImage(String sFileImg){
    	boolean bret=false;
    	try{
    		byte []buffIn = new byte[8192];
    		byte []buffSck = new byte[8192];
    		FileConnection fi = (FileConnection)Connector.open(sFileImg, Connector.READ);
    		InputStream in = fi.openDataInputStream();
    		// Select protocol type
    		buffSck[0]=0;
    		_sco.write(buffSck, 0, 1);
    		int iRead=ReadSocket(_sci, buffSck, 1);
    		if (iRead==1 && buffSck[0]!=0){ //echo
        		String sLang="IT|EN";
        		// Send language settings
        		System.arraycopy(sLang.getBytes("UTF-8"),0,buffSck,0,sLang.length());
        		_sco.write(buffSck, 0, sLang.length());
        		iRead=ReadSocket(_sci, buffSck, 10);
        		if (iRead==10){
        			System.arraycopy(buffSck,0,_byIDProcess,0,10);
        			buffSck[3] = (byte)(fi.fileSize()>>24);
        			buffSck[2] = (byte)((fi.fileSize()>>16) & 0xFF);
        			buffSck[1] = (byte)((fi.fileSize() >> 8) & 0xFF);
        			buffSck[0] = (byte)(fi.fileSize() & 0xFF);
            		_sco.write(buffSck, 0, 4);
            		iRead=ReadSocket(_sci, buffIn, 4);
            		if (iRead==4 && 
        				buffSck[0]==buffIn[0] && 
        				buffSck[1]==buffIn[1] &&
        				buffSck[2]==buffIn[2] &&
        				buffSck[3]==buffIn[3])
            		{
            			// send file
                		iRead=in.read(buffIn, 0, buffIn.length);
                		while (iRead>0){
                    		_sco.write(buffIn, 0, iRead);
                    		iRead=in.read(buffIn, 0, buffIn.length);
                		}
                		iRead=ReadSocket(_sci, buffIn, 1);
                		if (iRead==1 && buffIn[0]==1){
                    		iRead=ReadSocket(_sci, buffIn, 5);
                    		if (iRead==5){
                    			_byTranslator=buffIn[0];
                                int iLen = buffIn[4];iLen <<= 8;
                                iLen |= buffIn[3];iLen <<= 8;
                                iLen |= buffIn[2];iLen <<= 8;
                                iLen |= buffIn[1];
                                _bySrc=new byte[iLen];
                        		iRead=ReadSocket(_sci, _bySrc, iLen);
                        		buffSck[0]=1;
                        		_sco.write(buffSck, 0, 1);
                        		iRead=ReadSocket(_sci, buffIn, 4);
                        		if (iRead==4){
                                    iLen = buffIn[3];iLen <<= 8;
                                    iLen |= buffIn[2];iLen <<= 8;
                                    iLen |= buffIn[1];iLen <<= 8;
                                    iLen |= buffIn[0];
                                    _byDest=new byte[iLen];
                            		iRead=ReadSocket(_sci, _byDest, iLen);
                            		buffSck[0]=1;
                            		_sco.write(buffSck, 0, 1);
                        		}
                        		else
                        		{
                        			
                        		}
                    		}
                    		else
                    		{
                    			
                    		}
                		}
                		else
                		{
                			
                		}
            		}
            		else
            		{
            			
            		}
        		}	
        		else
        		{
        			
        		}
    		}
    		else
    		{
    			
    		}
    	}
    	catch(IOException ex) {
    		
    	}
    	return bret;
    }
    protected int ReadSocket(DataInputStream sci, byte []byBuff, int iToRead)
    {
       int ir=0;
       boolean bexit=false;
       try{
          long it1 = System.currentTimeMillis();
          
          while (System.currentTimeMillis()-it1 < 120000 && bexit==false){
             int iby = sci.available();
             if (iby>0){
                if (iby+ir>iToRead)
                   iby=iToRead-ir;
                sci.read(byBuff, ir,iby);
                ir+=iby;
                if (ir>=iToRead)
                   bexit=true;
             }
          }
          
          
       }catch(IOException ex){}
       
       return ir;
    }

}

