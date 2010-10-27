package com.android.pikling;

import android.app.Activity;
import android.content.Intent;
import android.location.Location;
import android.location.LocationListener;
import android.location.LocationManager;
import android.location.LocationProvider;
import android.net.Uri;
import android.os.Bundle;
import android.os.Handler;
import android.os.Message;
import android.widget.ImageView; 
import android.widget.Toast;
import android.util.Log;
import android.view.KeyEvent;
import android.view.MenuItem;
import android.view.MotionEvent; 
import android.view.View; 
import android.content.SharedPreferences;
import android.content.res.Resources;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.provider.MediaStore.Images.Media;
import java.io.*;
import java.util.List;

import android.location.Criteria;

import android.view.Menu;

public class Pikling extends Activity implements LocationListener, View.OnFocusChangeListener, View.OnTouchListener, View.OnClickListener, Runnable
{
    /** Called when the activity is first created. */
	ImageView _imgSnapUp, _imgFile, _imgSettings, _imgLangSource, _imgLangDest, _imgFlipLang;
	boolean _bEndThread;
	Thread _thread; 
	Uri _uri, _uriFile;
	boolean _bLangChanged;
	int _iTypeWorker=WORKER_OFF;
	static final int WORKER_OFF  = 0;
	static final int WORKER_RUN_PREVIEW  = 1;
	boolean _bGPSOn;
	LocationManager _lm;
	LocationProvider _LocationProviderFine, _LocationProviderCoarse;
	
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.main);
        _imgSnapUp    = (ImageView) findViewById(R.id.ImageSnapUp); 
        _imgFile      = (ImageView) findViewById(R.id.ImageFile);
        _imgSettings  = (ImageView) findViewById(R.id.ImageSettings);
        _imgLangSource= (ImageView) findViewById(R.id.ImageLangSource);
        _imgLangDest  = (ImageView) findViewById(R.id.ImageLangDest);
        _imgFlipLang  = (ImageView) findViewById(R.id.ImageFlipLang);
        
        _imgSnapUp.setOnTouchListener(this);
        _imgSnapUp.setOnClickListener(this);
        _imgSnapUp.setOnFocusChangeListener(this);
        _imgFile.setOnTouchListener(this);
        _imgFile.setOnClickListener(this);
        _imgFile.setOnFocusChangeListener(this);
        _imgSettings.setOnTouchListener(this);
        _imgSettings.setOnClickListener(this);
        _imgSettings.setOnFocusChangeListener(this);
        _imgFlipLang.setOnTouchListener(this);
        _imgFlipLang.setOnFocusChangeListener(this);
        _imgFlipLang.setOnClickListener(this);
        _imgLangSource.setOnTouchListener(this);
        _imgLangDest.setOnTouchListener(this);
        _imgLangDest.setOnClickListener(this);
        _imgLangDest.setOnFocusChangeListener(this);
        _imgLangSource.setOnFocusChangeListener(this);
        _imgLangSource.setOnClickListener(this);
        
		Intent intImg = getIntent();
		Bundle extras = intImg.getExtras();
		
		if (Intent.ACTION_SEND.equals(intImg.getAction()) && (extras != null) && extras.containsKey(Intent.EXTRA_STREAM)) {
			_uriFile = _uri = (Uri)extras.getParcelable(Intent.EXTRA_STREAM);
			if (_uri!=null)
				_iTypeWorker=WORKER_RUN_PREVIEW;
		}
		else
			_uriFile = _uri = intImg.getData();
		
		_bEndThread=false;
		_thread  = new Thread(this);
		_thread.start();
		
		// check if email address is set
        Resources res = getResources();
        SharedPreferences settings = getSharedPreferences(res.getString(R.string.setting_file), 0);
        String sEmail = settings.getString(res.getString(R.string.setting_lang_sendto), "");
        
        if (sEmail==""){
			ShowSettings();
			Toast.makeText(this, "Please insert a valid email address", Toast.LENGTH_SHORT).show();
		}
        InitLM();
        
		// reset last location settings
        SharedPreferences.Editor editor = getSharedPreferences(res.getString(R.string.setting_file), 0).edit();
    	editor.putString(res.getString(R.string.setting_cur_lon), "");
    	editor.putString(res.getString(R.string.setting_cur_lat), "");
    	editor.commit();	                

		_bLangChanged=false;
    }
    
    void InitLM(){
    	// init location manager
        _lm = (LocationManager)getSystemService(LOCATION_SERVICE);
		Criteria criteria = new Criteria ();
        criteria.setAccuracy(Criteria.ACCURACY_FINE);
        List<String> providerIds=_lm.getProviders(criteria, true);
        if (!providerIds.isEmpty()) {
        	_LocationProviderFine =_lm.getProvider(providerIds.get(0));
        }
        criteria.setAccuracy(Criteria.ACCURACY_COARSE);
        providerIds=_lm.getProviders(criteria, true);
        if (!providerIds.isEmpty()) {
                _LocationProviderCoarse = _lm.getProvider(providerIds.get(0));
        }        
        if (_LocationProviderFine!=null){
			_lm.requestLocationUpdates(_LocationProviderFine.getName(),0, 0, this);
			Log.i("", "ACCURACY_FINE update Enabled");
        }
        else
        	Log.i("", "ACCURACY_FINE update Disabled");
		if (_LocationProviderCoarse!=null){
			_lm.requestLocationUpdates(_LocationProviderCoarse.getName(),1000L, 500.0f, this);
			Log.i("", "ACCURACY_COARSE update Enabled");
		}
		else
			Log.i("", "ACCURACY_COARSE update Disabled");
		
    }
    
	public void run() {
		while(!_bEndThread){
			try{
				switch (_iTypeWorker){
				case WORKER_RUN_PREVIEW:
					_thread.sleep(100);
					handler.sendEmptyMessage(SHOW_PREVIEW);
					_iTypeWorker=WORKER_OFF;
					break;
				}
				
			}
			catch (Exception ex){
				
			}
			
		}
		
	}
	
	protected void onDestroy() {
		super.onDestroy();

		_lm.removeUpdates(this); 
		_bEndThread=true;
	}
    
    private void UpdateLangSetting()
    {
        Resources res = getResources();
        SharedPreferences settings = getSharedPreferences(res.getString(R.string.setting_file), 0);

        int iIdSourceIcon = GetDrawableLangFromText(settings.getString(res.getString(R.string.setting_lang_source), "IT"));
        int iIdDestIcon   = GetDrawableLangFromText(settings.getString(res.getString(R.string.setting_lang_dest), "EN"));
        _imgLangSource.setImageResource(iIdSourceIcon);
        _imgLangDest.setImageResource(iIdDestIcon);
    }
    
    void ShowSettings(){
		Intent intent = new Intent(Pikling.this, MySettings.class);
        startActivityForResult(intent, RESULT_SETTING);
    }

    protected void onResume (){
    	super.onResume();
    	UpdateLangSetting();
    	if (_uri!=null){
        	/*Intent intent = new Intent(Pikling.this, Preview.class);
        	intent.setData(_uri);
            _uri=null;
            startActivityForResult(intent, RESULT_OCR);*/
    	}
    }
    @Override
	public void onPause() {
		super.onPause();
		//unregisterReceiver(receiver);
	}
    
    public boolean onKeyDown(int keyCode, KeyEvent event) {
        if (keyCode == KeyEvent.KEYCODE_BACK || keyCode == KeyEvent.KEYCODE_HOME) {
        	_lm.removeUpdates(this);
        }
        return super.onKeyDown(keyCode, event);
    }
    
    public void onClick(View v) {
    	if (v.getId()==_imgSettings.getId())
    		ShowSettings();
    	else if (v.getId()==_imgFile.getId())
    		PickUpImage();
    	else if (v.getId()==_imgSnapUp.getId())
    		CaptureImage();
    	else if (v.getId()==_imgFlipLang.getId())
    		FlipLanguages();
    	else if(v.getId()==_imgLangSource.getId())
    		ShowFlags(RESULT_FLAG_SOURCE);    	
    	else if(v.getId()==_imgLangDest.getId())
    		ShowFlags(RESULT_FLAG_DEST);
    	
    }
    public void onFocusChange (View v, boolean hasFocus){
    	ImageView img=null;
    	int idResF=0, idResUF=0;
    	if (v.getId()==_imgSnapUp.getId()){
    		img=_imgSnapUp;
    		idResF = R.drawable.snap_button_f;
    		idResUF = R.drawable.snap_button;
    	}
    	else if (v.getId()==_imgFile.getId()){
    		img=_imgFile;
    		idResF = R.drawable.camera_roll_f;
    		idResUF = R.drawable.camera_roll;
    	}
    	else if (v.getId()==_imgSettings.getId()){
    		img=_imgSettings;
    		idResF = R.drawable.pref_f;
    		idResUF = R.drawable.pref;
    	}
    	else if (v.getId()==_imgFlipLang.getId()){
    		img=_imgFlipLang;
    		idResF = R.drawable.arrow_for_flags_f;
    		idResUF = R.drawable.arrow_for_flags;
    	}
    	else if (v.getId()==_imgLangSource.getId()){    		
    		if (hasFocus)
    			_imgLangSource.setBackgroundDrawable(getResources().getDrawable(R.drawable.back_lang));
    		else
    			_imgLangSource.setBackgroundDrawable(null);
    	}
    	else if (v.getId()==_imgLangDest.getId()){    		
    		if (hasFocus)
    			_imgLangDest.setBackgroundDrawable(getResources().getDrawable(R.drawable.back_lang));
    		else
    			_imgLangDest.setBackgroundDrawable(null);
    	}
        if (img!=null)
        {	if (hasFocus)
        		img.setImageResource(idResF);
        	else
        		img.setImageResource(idResUF);
        }
    }
    
    public boolean onTouch(View v, MotionEvent event) {
        
    	ImageView img=null;
    	int idResUp=0, idResDn=0, idRes=0;
    	if (v.getId()==_imgSnapUp.getId()){
    		img=_imgSnapUp;
    		idResUp = R.drawable.snap_button;
    		idResDn = R.drawable.snap_button_press;
    	}
    	else if (v.getId()==_imgFile.getId()){
    		img=_imgFile;
    		idResUp = R.drawable.camera_roll;
    		idResDn = R.drawable.camera_roll_press;
    	}
    	else if (v.getId()==_imgSettings.getId()){
    		img=_imgSettings;
    		idResUp = R.drawable.pref;
    		idResDn = R.drawable.pref_press;
    	}
    	else if (v.getId()==_imgFlipLang.getId()){
    		img=_imgFlipLang;
    		idResUp = R.drawable.arrow_for_flags;
    		idResDn = R.drawable.arrow_for_flags_press;
    	}
    	else if (v.getId()==_imgLangSource.getId()){
    		img=_imgLangSource;
    		idResUp = -1;
    		idResDn = -1;
    	}
        
        switch (event.getAction()) { 
	        case MotionEvent.ACTION_DOWN:
	        	idRes = idResDn;
	        	break;
	        case MotionEvent.ACTION_UP:
	        	idRes = idResUp;
	        	if (v.getId()==_imgSettings.getId())
	        		ShowSettings();
	        	else if(v.getId()==_imgFlipLang.getId()){
	        		FlipLanguages();
	        	}
	        	else if(v.getId()==_imgLangSource.getId()){
	        		ShowFlags(RESULT_FLAG_SOURCE);
	        	}
	        	else if(v.getId()==_imgLangDest.getId()){
	        		ShowFlags(RESULT_FLAG_DEST);
	        	}
	        	else if(v.getId()==_imgFile.getId()){
	        		PickUpImage();
	        	}
	        	else if(v.getId()==_imgSnapUp.getId()){
	        		CaptureImage();
	        	}
	        		
	        	break;
	        default:
	        	return true;
        }
        if (img!=null && idRes>=0)
        	img.setImageResource(idRes);
         
        return true; 
    } 
    void FlipLanguages(){
        Resources res = getResources();
        SharedPreferences settings = getSharedPreferences(res.getString(R.string.setting_file), 0);
        String sLangSrc = settings.getString(res.getString(R.string.setting_lang_source), "IT");
        String sLangDst = settings.getString(res.getString(R.string.setting_lang_dest), "EN");
        SharedPreferences.Editor editor = getSharedPreferences(res.getString(R.string.setting_file), 0).edit();
    	editor.putString(res.getString(R.string.setting_lang_source), sLangDst);
    	editor.putString(res.getString(R.string.setting_lang_dest), sLangSrc);
    	editor.commit();	                
    	UpdateLangSetting();
    	_bLangChanged=true;
    }
    private void PickUpImage()
    {
    	Uri target = android.provider.MediaStore.Images.Thumbnails.EXTERNAL_CONTENT_URI;
        Intent intent = new Intent(Intent.ACTION_PICK, target);
        startActivityForResult(intent, RESULT_PICKUP_IMAGE);
    }
    private void CaptureImage()
    {	
    	/*ComponentName toLaunch;
    	toLaunch = new ComponentName("com.android.camera","com.android.camera.Camera");
    	Intent intent = new Intent(Intent.ACTION_MAIN);
        intent.addCategory(Intent.CATEGORY_LAUNCHER);
        intent.setComponent(toLaunch);
        intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK|Intent.FLAG_ACTIVITY_RESET_TASK_IF_NEEDED);
        startActivity(intent);*/
    	Intent intent = new Intent(this, CameraPreview.class);
    	intent.putExtra(getResources().getString(R.string.intent_preview), true);
    	startActivityForResult(intent, RESULT_CAPTURE_IMAGE);
    }
    private int GetDrawableLangFromText(String sLang)
    {
    	sLang = sLang.toLowerCase();
        int iRes = getResources().getIdentifier(getPackageName()+":drawable/"+sLang , null, null);
    	return iRes;
    }
    
    private void ShowFlags(int iFlagType)
    {
    	Intent intent = new Intent(Pikling.this, Languages.class);
        startActivityForResult(intent, iFlagType);
    }
    static final private int RESULT_FLAG_SOURCE  = 0;
    static final private int RESULT_FLAG_DEST    = 1;
    static final private int RESULT_PICKUP_IMAGE = 2;
    static final private int RESULT_CAPTURE_IMAGE= 3;
    static final private int RESULT_OCR			 = 4;
    static final private int RESULT_SETTING		 = 5;
    
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        if (resultCode != RESULT_CANCELED || requestCode==RESULT_SETTING) {
        	String sLang;
            Resources res;
            SharedPreferences.Editor editor;
        	ImageView img= (ImageView) findViewById(R.id.ImageTitle);
        	Intent intent;

            switch (requestCode)
            {
            	case RESULT_SETTING:
                    res = getResources();
                    SharedPreferences settings = getSharedPreferences(res.getString(R.string.setting_file), 0);
                    String sEmail = settings.getString(res.getString(R.string.setting_lang_sendto), "");
            		if (sEmail==""){
            			Toast.makeText(this, "You can't use Pikling without a valid email address.", Toast.LENGTH_SHORT).show();
            			_lm.removeUpdates(this);
            			finish();
            		}
            		break;
	            case RESULT_FLAG_DEST:
	            	sLang = data.getAction();
	                res = getResources();
	                editor = getSharedPreferences(res.getString(R.string.setting_file), 0).edit();
	            	editor.putString(res.getString(R.string.setting_lang_dest), sLang);
	            	editor.commit();
	            	UpdateLangSetting();
	            	_bLangChanged=true;
	            	break;
	            case RESULT_FLAG_SOURCE:
	            	sLang = data.getAction();
	                res = getResources();
	                editor = getSharedPreferences(res.getString(R.string.setting_file), 0).edit();
	            	editor.putString(res.getString(R.string.setting_lang_source), sLang);
	            	editor.commit();
	            	UpdateLangSetting();
	            	_bLangChanged=true;
	            	break;
	            case RESULT_PICKUP_IMAGE:
	            	if (data!=null){
		            	_uriFile = data.getData();
		            	_iTypeWorker=WORKER_RUN_PREVIEW;	            	
		            }
	            	break;
	            case RESULT_CAPTURE_IMAGE:
	            	if (data!=null){
	            		_uriFile = data.getData();
		            	handler.sendEmptyMessage(SHOW_RESULT_OCR);
	            	}
	            	break;
	            case RESULT_OCR:
	            	handler.sendEmptyMessage(SHOW_RESULT_OCR);
	            	break;
            }
        }
    }
    protected void LoadImg(Uri uri, ImageView img, String sFile){
    	try{

			Bitmap bm = Media.getBitmap(getContentResolver(), uri);
			ByteArrayOutputStream bytes = new ByteArrayOutputStream();
			bm.compress(Bitmap.CompressFormat.JPEG, 90, bytes);
			
	        BitmapFactory.Options opts = new BitmapFactory.Options();
	        opts.inJustDecodeBounds = true;                            
	        bm= BitmapFactory.decodeByteArray(bytes.toByteArray(), 0, bytes.toByteArray().length, opts);
	        opts.inJustDecodeBounds = false;
	        opts.inSampleSize = 16;
	        opts.outWidth=30;
	        bm= BitmapFactory.decodeByteArray(bytes.toByteArray(), 0, bytes.toByteArray().length, opts);
	        img.setImageBitmap(bm);
    		
    		if (sFile!=""){
	    		deleteFile(sFile);
	    		FileOutputStream fos = openFileOutput(sFile, MODE_WORLD_WRITEABLE);
	    		fos.write(bytes.toByteArray());
	    		fos.close();
    		}
    		bm=null;
    		bytes.close();
	    }
    	catch (IOException e) {
    		Log.i("ExptIOExceptionion", e.getMessage());
        }
    }
    
    private static final int MENU_LAST = Menu.FIRST+1;
    private static final int MENU_ABOUT = Menu.FIRST+2;
    @Override
	public boolean onCreateOptionsMenu(Menu menu) {
		menu.add(Menu.NONE, MENU_ABOUT, Menu.NONE, "")
		.setIcon(R.drawable.about);
		menu.add(Menu.NONE, MENU_LAST, Menu.NONE, "Last Result")
		.setIcon(R.drawable.previous);
		return(super.onCreateOptionsMenu(menu));
	}
    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        switch (item.getItemId()) {
        case MENU_ABOUT:
        	startActivity(new Intent(Intent.ACTION_VIEW, Uri.parse("http://m.7touchgroup.com")));
        	break;
        case MENU_LAST:
        	Intent intent = new Intent(Pikling.this, Result.class);
        	if (_bLangChanged)
        		intent.setAction("0");
        	else
        		intent.setAction("1");
        	_bLangChanged=false;
            startActivity(intent);
            return true;
        
        }
        return super.onOptionsItemSelected(item);
    }
    private static final int SHOW_RESULT_OCR = 1;
    private static final int SHOW_PREVIEW    = 2;
    
    private Handler handler = new Handler() {
		public void handleMessage(Message msg) {
			Intent intent;
			switch (msg.what)
			{                
			case SHOW_PREVIEW:
        		intent = new Intent(Pikling.this, CameraPreview.class);
            	intent.putExtra(getResources().getString(R.string.intent_preview), false);
            	intent.setData(_uriFile);
                startActivityForResult(intent, RESULT_OCR);            
				break;
			case SHOW_RESULT_OCR:
            	intent = new Intent(Pikling.this, Result.class);
            	intent.setAction("0");		            	
            	intent.setData(_uriFile);
            	startActivityForResult(intent, RESULT_OCR);
				break;
			}
			super.handleMessage(msg);
		}
	};


	public void onLocationChanged(Location location) {
		// TODO Auto-generated method stub
		if (location.getProvider().equals(LocationManager.NETWORK_PROVIDER) && _bGPSOn){
        	
        }
        else{
        	Resources res = getResources();
            SharedPreferences.Editor editor = getSharedPreferences(res.getString(R.string.setting_file), 0).edit();
        	editor.putString(res.getString(R.string.setting_cur_lon), location.getLongitude()+"");
        	editor.putString(res.getString(R.string.setting_cur_lat), location.getLatitude()+"");
        	editor.commit();	                
        }		
	}

	public void onProviderDisabled(String provider) {
		// TODO Auto-generated method stub
		
	}

	public void onProviderEnabled(String provider) {
		// TODO Auto-generated method stub
		
	}

	public void onStatusChanged(String provider, int status, Bundle extras) {
		
		// TODO Auto-generated method stub
    	switch (status){
    	case LocationProvider.OUT_OF_SERVICE:
    		if (provider.equals(LocationManager.GPS_PROVIDER))
    			_bGPSOn=false;
    		break;
    	case LocationProvider.AVAILABLE:
    		if (provider.equals(LocationManager.GPS_PROVIDER))
    			_bGPSOn=true;
    		break;
    	case LocationProvider.TEMPORARILY_UNAVAILABLE:
    		if (provider.equals(LocationManager.GPS_PROVIDER))
    			_bGPSOn=false;    		
    		break;
    	}		
	}
}
    