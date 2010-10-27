package pikling;

import net.rim.blackberry.api.phone.Phone;
import net.rim.device.api.system.DeviceInfo;

public class DeviceInformation {

	public static String getDeviceName(){
		return DeviceInfo.getDeviceName();
	}
	public static String getDeviceID(){
		return DeviceInfo.getDeviceId()+"";
	}
	public static String getManuf(){
		return DeviceInfo.getManufacturerName();
	}
	public static String getVer(){
		return DeviceInfo.getPlatformVersion() + ";" + DeviceInfo.getSoftwareVersion();
	}
	public static String getPhoneNumb(){
		String sRet="";
		try{
			return Phone.getDevicePhoneNumber(false);
		}
		catch(Exception ex){
			
		}
		return sRet;
	}
}
