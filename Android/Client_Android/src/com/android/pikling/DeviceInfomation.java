package com.android.pikling;

import java.io.IOException; 
import java.io.InputStream; 
import android.content.Context; 
import android.os.Build; 
import android.telephony.TelephonyManager; 
import android.telephony.gsm.GsmCellLocation;
import android.provider.Settings;
import android.provider.Settings.System;

public class DeviceInfomation { 
    private Context mon; 
    /** 
     * 
     * @param monAct 
     *              the monitor activity. 
     */ 
    public DeviceInfomation(Context monAct) { 
        this.mon = monAct; 
    } 
    public String getDeviceInfo() { 
        String result = "\t<device>\n"; 
        result += getSoftwareRevision(); 
        result += getHardwareRevision(); 
        result += "\t\t<loc>" + getCellID() + "</loc>\n"; 
        result += "\t\t<msisdn>" + getMsisdn() + "</msisdn>\n"; 
        result += "\t\t<imei>" + getImei() + "</imei>\n"; 
        result += "\t</device>\n"; 
        return result; 
    } 
    /** 
     * 
     * @return Returns the IMEI. 
     */ 
    public String getImei() { 
        TelephonyManager mTelephonyMgr = (TelephonyManager) 
                mon.getSystemService(Context.TELEPHONY_SERVICE); 
        return mTelephonyMgr.getDeviceId(); 
    } 
    /** 
     * 
     * @return Returns the MSISDN. 
     */ 
    public String getMsisdn() { 
    	String sRet="";
    	try{
    		TelephonyManager mTelephonyMgr = (TelephonyManager)mon.getSystemService(Context.TELEPHONY_SERVICE);
        	sRet = mTelephonyMgr.getLine1Number();
    	}
    	catch (Exception ex){
    		
    	}
        return  sRet;
    } 
    
    public String getDeviceID(){
    	String deviceId="";
    	try{
	    	deviceId = Settings.System.getString(mon.getContentResolver(), Settings.System.ANDROID_ID);
    	}
    	catch(Exception ex){
    		
    	}
    	
    	return deviceId;
    }
    /** 
     * 
     * @return Returns the cell ID. 
     */ 
    public int getCellID() { 
        TelephonyManager mTelephonyMgr = (TelephonyManager) 
                mon.getSystemService(Context.TELEPHONY_SERVICE); 
        GsmCellLocation location = (GsmCellLocation) 
                mTelephonyMgr.getCellLocation(); 
        return location.getCid(); 
    } 
    /** 
     * 
     * @return Returns the software revision. 
     */ 
    public String getSoftwareRevision() { 
        String result = "\t<soft>\n"; 
        Runtime runtime = Runtime.getRuntime(); 
        try { 
            Process proc = runtime.exec("cat /proc/version"); 
            int exit = proc.waitFor(); 
            if (exit == 0) { 
                String content = getContent(proc.getInputStream()); 
                int index = content.indexOf(')'); 
                if (index >= 0) { 
                    result += "\t\t<kernel>" + content.substring(0, 
index +1) 
                            + "</kernel>\n"; 
                } 
            } 
        } catch (IOException e) { 
            e.printStackTrace(); 
        } catch (InterruptedException e) { 
            e.printStackTrace(); 
        } 
        result += "\t\t<buildNumber>" + Build.PRODUCT + 
Build.VERSION.RELEASE 
                + "</buildNumber>\n"; 
        result += "\t</soft>\n"; 
        return result; 
    } 
    public String getHardwareRevision() { 
        String result = "\t<hard>\n"; 
        Runtime runtime = Runtime.getRuntime(); 
        try { 
            Process proc = runtime.exec("cat /proc/cpuinfo"); 
            int exit = proc.waitFor(); 
            if (exit == 0) { 
                String content = getContent(proc.getInputStream()); 
                String [] lines = content.split("\n"); 
                String [] hInfo = { 
                        "Processor", "Hardware", "Revision" 
                }; 
                if (lines != null) { 
                    for (String line: lines) { 
                        for (String info: hInfo) { 
                            int index = line.indexOf(info); 
                            if (index >= 0) { 
                                result += "\t\t<" + info.toLowerCase() 
+ ">"; 
                                int vIndex = line.indexOf(':'); 
                                result += line.substring(vIndex + 1); 
                                result += "\t\t</" + info.toLowerCase 
() + ">"; 
                            } 
                        } 
                    } 
                } 
            } 
        } catch (IOException e) { 
            e.printStackTrace(); 
        } catch (InterruptedException e) { 
            e.printStackTrace(); 
        } 
        result += "\t</hard>\n"; 
        return result; 
    } 
    /** 
     * 
     * @param input 
     *              the input stream. 
     * @return Returns the content string of the input stream. 
     * 
     * @throws IOException 
     *              the Java exception. 
     */ 
    public static String getContent(InputStream input) throws 
IOException { 
        if (input == null) { 
            return null; 
        } 
        byte [] b = new byte [1024]; 
        int readBytes = 0; 
        String result = ""; 
        while ((readBytes = input.read(b)) >= 0) { 
            result += new String(b, 0, readBytes); 
        } 
        return result; 
    } 
} 
