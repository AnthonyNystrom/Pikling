package com.android.pikling;

import android.content.BroadcastReceiver; 
import android.content.ComponentName;
import android.content.Context; 
import android.content.Intent; 
import android.util.Log; 

public class NewPhotoReceiver extends BroadcastReceiver 
{ 
	private static final String     TAG = "NewPhotoReceiver"; 
    @Override 
	public void onReceive(Context context, Intent intent) 
	{ 
		Log.i(TAG, "Received new photo"); 
    	ComponentName toLaunch;
    	toLaunch = new ComponentName("com.android.pikling","com.android.pikling.Pikling");
    	Intent intentp = new Intent(Intent.ACTION_MAIN);
    	intentp.addCategory(Intent.CATEGORY_LAUNCHER);
        intentp.setComponent(toLaunch);
        intentp.setFlags(Intent.FLAG_ACTIVITY_PREVIOUS_IS_TOP|Intent.FLAG_ACTIVITY_NEW_TASK);
        intentp.setData(intent.getData());
        context.startActivity(intentp);
	} 
} 