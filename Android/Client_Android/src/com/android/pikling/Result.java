package com.android.pikling;

import android.app.Activity;
import android.app.AlertDialog;
import android.app.Dialog;
import android.app.PendingIntent;
import android.app.ProgressDialog;
import android.os.Bundle;
import android.os.Handler;
import android.os.Message;
import java.net.Socket;
import java.io.OutputStream;
import java.io.InputStream;
import java.io.IOException;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.SharedPreferences;
import android.content.res.Resources;
import android.telephony.gsm.SmsManager;
import android.util.Log;
import android.view.*;
import android.widget.ImageView;
import android.widget.RadioButton;
import android.widget.ScrollView;
import android.widget.EditText;
import android.widget.TextView;

import java.io.*;
import android.os.Looper;


public class Result extends Activity implements Runnable, View.OnClickListener, View.OnTouchListener, View.OnFocusChangeListener{
	String _sLang;
	String _sProc;	// id processo
	byte _byEngineTranslator;
	byte _byOcrSrc[];
	byte _byOcrDest[];
	Socket _sc;
	OutputStream _out;
	InputStream _insc;
	Thread _thread=null;
	ProgressDialog _dialog;
	boolean _bEndThread, _bRequestEmail;
	TextView _txtDest, _txtSrc, _txtZoom; 
	EditText _edEmail, _edNumb;
	String _sErr, _sSrc, _sDst, _sLangSrc, _sLangDst, _sEmailAddrTo, _sTxtShow, _sNumbMobile;
	Bitmap _bm, _bmZoom;
	ViewGroup _Container;
	ImageView _imgPreview, _imgShow;
	ScrollView _scrollView;
	View addView;
	RadioButton r1, r2;
	int _iIdSourceIcon, _iIdDestIcon;
	
	static final int WORKER_OFF        = 0;
	static final int WORKER_SEND_IMAGE = 1;
	static final int WORKER_GET_EMAIL  = 2;
	int _iTypeWorker=WORKER_OFF;
	
	private static final int DIALOG_ERR_PROTO     = 1;
	private static final int DIALOG_REQUEST_EMAIL = 2;
	private static final int DIALOG_TEXT          = 3;
	private static final int DIALOG_REQUEST_NUM   = 4;
	
	@Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.result); 
        
        // language settings
        Resources res = getResources();
        SharedPreferences settings = getSharedPreferences(res.getString(R.string.setting_file), 0);
        _sNumbMobile  = settings.getString(res.getString(R.string.setting_lang_nummob), "");
        _sEmailAddrTo = settings.getString(res.getString(R.string.setting_lang_sendto), "");
        _sLangSrc     = settings.getString(res.getString(R.string.setting_lang_source), "IT");
        _sLangDst     = settings.getString(res.getString(R.string.setting_lang_dest), "EN");
        _bRequestEmail=settings.getBoolean(res.getString(R.string.setting_lang_asksend), false);
        _sLang        = _sLangSrc+"|"+_sLangDst;
        
        _iIdSourceIcon = GetDrawableLangFromText(_sLangSrc);
        _iIdDestIcon   = GetDrawableLangFromText(_sLangDst);
        ImageView imgSrc=(ImageView) findViewById(R.id.ImgSrc);
        ImageView imgDst=(ImageView) findViewById(R.id.ImgDst);
        imgSrc.setBackgroundResource(_iIdSourceIcon);
        imgDst.setBackgroundResource(_iIdDestIcon);
        //imgSrc.setImageResource(iIdSourceIcon);
        //imgDst.setImageResource(iIdDestIcon);
        
        _Container  = (ViewGroup)findViewById(R.id.Container);
        _imgPreview = (ImageView)findViewById(R.id.imgToSend);
        _imgShow    = (ImageView)findViewById(R.id.imgShow);
        _scrollView = (ScrollView)findViewById(R.id.ScrollViewContainer);
        _Container.setPersistentDrawingCache(ViewGroup.PERSISTENT_ANIMATION_CACHE);
        _imgPreview.setOnClickListener(this);
        _imgPreview.setOnTouchListener(this);
        _imgShow.setOnClickListener(this);
        _imgShow.setOnTouchListener(this);
        

        _Container.setOnClickListener(this);
        _Container.setOnTouchListener(this);

		String sFile = getResources().getString(R.string.filename_img_tmp);
		Bitmap bm = BitmapFactory.decodeFile(getFilesDir().getPath()+"/"+sFile);
		_bm=CameraPreview.ResizeImg(bm, 100, 0);
		if (_bm!=null)
			_imgPreview.setImageBitmap(_bm);
		_bmZoom=CameraPreview.ResizeImg(bm, 480, 0);
		bm.recycle();
        
		_txtZoom =(TextView) findViewById(R.id.txtZoom);
		_txtZoom.setOnClickListener(this);
		_txtZoom.setOnTouchListener(this);
		
		_txtSrc=(TextView) findViewById(R.id.edSrc);
		_txtSrc.setOnClickListener(this);
		_txtSrc.setOnTouchListener(this);
    	_txtDest=(TextView) findViewById(R.id.edDst);
    	_txtDest.setOnClickListener(this);
    	_txtDest.setOnTouchListener(this);
                
        Intent i = getIntent();
        String sAction =i.getAction();
        
        _iTypeWorker = WORKER_OFF;
    	_thread = new Thread(this);
    	_thread.start();
        if (sAction.compareTo("0")==0)
        {	_iTypeWorker = WORKER_SEND_IMAGE;    	
	    	StartProgressDialog();
        }
    }
	
    public void onClick(View v) {
    	if (v.getId()==_txtZoom.getId()){
    		_txtZoom.setVisibility(View.GONE);
    	}
    	else if (v.getId()==_imgShow.getId()){
    		_imgShow.setVisibility(View.GONE);
    		//_scrollView.setVisibility(View.VISIBLE);
    	}
    	else if (v.getId()==_imgPreview.getId()){
    		_imgShow.setImageBitmap(_bmZoom);
    		_imgShow.setVisibility(View.VISIBLE);
    		//_scrollView.setVisibility(View.GONE);
    	}
    	else if (v.getId()==_txtSrc.getId()){
    		_sTxtShow = _txtSrc.getText().toString();
    		_txtZoom.setText(_sTxtShow);
    		_txtZoom.setVisibility(View.VISIBLE);
    		//showDialog(DIALOG_TEXT);
    	}
    	else if (v.getId()==_txtDest.getId()){
    		_sTxtShow = _txtDest.getText().toString();
    		_txtZoom.setText(_sTxtShow);
    		_txtZoom.setVisibility(View.VISIBLE);
    		//showDialog(DIALOG_TEXT);    		
    	}
    }
    public boolean onTouch(View v, MotionEvent event) {
    	if (v.getId()==_txtZoom.getId()){
    		_txtZoom.setVisibility(View.GONE);
    	}
    	else if (v.getId()==_imgShow.getId()){
    		_imgShow.setVisibility(View.GONE);
    		//_scrollView.setVisibility(View.VISIBLE);
    	}
    	else if (v.getId()==_imgPreview.getId()){
    		_imgShow.setImageBitmap(_bmZoom);
    		_imgShow.setVisibility(View.VISIBLE);
    		//_scrollView.setVisibility(View.GONE);
    	}
    	else if (v.getId()==_txtSrc.getId()){
    		_sTxtShow = _txtSrc.getText().toString();
    		_txtZoom.setText(_sTxtShow);
    		_txtZoom.setVisibility(View.VISIBLE);
    		//showDialog(DIALOG_TEXT);
    	}
    	else if (v.getId()==_txtDest.getId()){
    		_sTxtShow = _txtDest.getText().toString();
    		_txtZoom.setText(_sTxtShow);
    		_txtZoom.setVisibility(View.VISIBLE);
    		//showDialog(DIALOG_TEXT);    		
    	}
    	return true;
    }
    public void onFocusChange (View v, boolean hasFocus){
    }
	
	
    @Override
    protected void onResume() {
        super.onResume();

        Intent i = getIntent();
        if (i.getAction().compareTo("1")==0)
        {	SharedPreferences prefs = getPreferences(0); 
        	String sSrc = prefs.getString("src", null);
        	String sDst = prefs.getString("dst", null);
        	_txtDest.setText(sDst);
        	_txtSrc.setText(sSrc);
        	_sProc = prefs.getString("process", null);
        }
    }
    @Override
    protected void onPause() {
        super.onPause();
        CloseSocket();
        SharedPreferences.Editor editor = getPreferences(0).edit();
        editor.putString("src", _txtSrc.getText().toString());
        editor.putString("dst", _txtDest.getText().toString());
        editor.putString("process", _sProc);
        editor.commit();
    }

	
	void StartProgressDialog()
    {
    	_dialog = new ProgressDialog(this);
        _dialog.setMessage("Please wait ...");
        _dialog.setIndeterminate(true);
        _dialog.show(); 
    }
    void EndProgressDialog()
	{	if (_dialog!=null){
			_dialog.cancel();
			_dialog=null;
		}
	}
	public void run() {
		Looper.prepare();

		while (!_bEndThread){
			
			switch (_iTypeWorker){
			case WORKER_SEND_IMAGE:
				try{
					SendImage();
					/*if (SendImage()){
						if (!_bRequestEmail && _sEmailAddrTo.compareTo("")!=0)
							_iTypeWorker=WORKER_GET_EMAIL;
						else{
							handler.sendEmptyMessage(2);
						}
					}*/
				}
				catch (Exception ex){
					_sErr=ex.getMessage();
					handler.sendEmptyMessage(1);
				}
				if (_iTypeWorker!=WORKER_GET_EMAIL){
					_iTypeWorker=WORKER_OFF;
					EndProgressDialog();
				}
				break;
				
			case WORKER_GET_EMAIL:
				try{
					RequestEmail();
				}
				catch (Exception ex){
					_sErr=ex.getMessage();
					handler.sendEmptyMessage(1);
				}
				_iTypeWorker=WORKER_OFF;
				EndProgressDialog();
				break;
			}
		}
	}
	
	@Override
	protected void onDestroy() {
		super.onDestroy();
		if (_bm!=null)
			_bm.recycle();
		if (_bmZoom!=null)
			_bmZoom.recycle();
		_bEndThread=true;
		CloseSocket();
	}
	
	void CloseSocket(){
		if (_out!=null){
			try{
		    	_out.close();
		    	_sc.close();
		    	_sc   = null;
		    	_out  = null;
		    	_insc = null;
			}
			catch(IOException ex){
				
			}
		}
	}
		
	boolean ConnectSocket(){
		boolean bret=false;
		try{
			_sc = new Socket("69.21.114.136",8080); 
			//_sc = new Socket("192.168.1.3",8080);
			_sc.setSoTimeout(120000);
	    	_out = _sc.getOutputStream();
	    	_insc  = _sc.getInputStream();
	    	bret=true;
		}
		catch(IOException ex){
			
		}
		return bret;
	}
	
	public boolean RequestEmail()
	{	boolean bret=false;
		try{
			if (_out==null){
				if (!ConnectSocket())
					throw new IllegalArgumentException("Error Protocol Step 0");
			}
        	// request email mode selection
        	byte byBuf[] = new byte[10];
        	byBuf[0]=1;
        	_out.write(byBuf, 0, 1);
        	int iRead=_insc.read(byBuf, 0, 1);
        	if (iRead!=1)
        		 throw new IllegalArgumentException("Error Protocol Step 1");
        	
        	String sHeader=_sProc+"|PDF|"+_sEmailAddrTo;
        	int iLen=(int)sHeader.length();
        	byBuf[0] = (byte)(iLen & 0x000000FF); 
        	byBuf[1] = (byte)((iLen>>8) & 0x000000FF);
        	_out.write(byBuf,0, 2);
        	iRead=_insc.read(byBuf, 2, 2);
        	if (iRead!=2 || ( 
        		byBuf[0]!=byBuf[2] ||
        		byBuf[1]!=byBuf[4])){
        		throw new IllegalArgumentException("Error Protocol Step 2");
        	}
        	_out.write(sHeader.getBytes(),0, sHeader.length());
        	iRead=_insc.read(byBuf, 0, 1);
        	if (iRead!=1 || byBuf[0]!=1)
        		throw new IllegalArgumentException("Error Protocol Step 3");
        	
        	bret=true;
		}
		catch(IOException ex)
    	{	_sErr = ex.getMessage();
    		CloseSocket();
    	}
		catch(Exception ex){
			_sErr = ex.getMessage();
			CloseSocket();
		}
		if (!bret)
			handler.sendEmptyMessage(1);
		CloseSocket();
		return bret;
	}
	public boolean SendImage()
    {
		boolean bret=false;
    	try{
			//File f = new File(sFileName);
			String sFileName=getResources().getString(R.string.filename_img);
			File f = getFileStreamPath(sFileName);
			if (!f.exists())
				throw new IllegalArgumentException("Error Protocol Step -1");
			
			FileInputStream in = this.openFileInput(sFileName);
			
			if (_out==null){
				if (!ConnectSocket())
					if (!ConnectSocket())
						if (!ConnectSocket())
							throw new IllegalArgumentException("Error Protocol Step 0");
			}
			
        	// send load file mode selection
        	byte byBuf[] = new byte[10];
        	byBuf[0]=0;
        	_out.write(byBuf, 0, 1);
        	int iRead=_insc.read(byBuf, 0, 1);
        	if (iRead!=1)
        		 throw new IllegalArgumentException("Error Protocol Step 1");
        	
        	String sHeader;
        	//sHeader = "MAGIC|MODEL ID|SERIAL|NUMBER PHONE|OS VERSION|GEO LAT|GEO LONG|EMAIL|LANG SRC|LANG DST";
        	DeviceInfomation devinfo = new DeviceInfomation(this);
        	Resources res = getResources();
        	SharedPreferences settings = getSharedPreferences(res.getString(R.string.setting_file), 0);
        	String sLat  = settings.getString(res.getString(R.string.setting_cur_lat), "");
        	String sLon  = settings.getString(res.getString(R.string.setting_cur_lat), "");
        	String sEmail= settings.getString(res.getString(R.string.setting_lang_sendto), "");
        	sHeader = "MAGIC|"+devinfo.getDeviceID()+"||"+devinfo.getMsisdn()+"|"+devinfo.getSoftwareRevision()+"|"+sLat+"|"+sLon+"|"+sEmail+"|"+_sLang;
        	
        	byBuf[0] = (byte)(sHeader.length() & 0x000000FF); 
        	byBuf[1] = (byte)((sHeader.length()>>8) & 0x000000FF);
        	_out.write(byBuf, 0, 2);
        	
        	Log.i("Header", sHeader);
        	
        	byte []byHeader = sHeader.getBytes();
        	_out.write(byHeader, 0, sHeader.length());
        	byte byIDProc[]=new byte[10];
        	iRead=_insc.read(byIDProc, 0, 10);
        	_sProc = new String(byIDProc);
        	
        	if (iRead!=10)
        		 throw new IllegalArgumentException("Error Protocol Step 2");
        	
        	int iLen=(int)f.length();//byImage.toByteArray().length;
        	byBuf[0] = (byte)(iLen & 0x000000FF); 
        	byBuf[1] = (byte)((iLen>>8) & 0x000000FF);
        	byBuf[2] = (byte)((iLen>>16) & 0x000000FF);
        	byBuf[3] = (byte)((iLen>>24) & 0x000000FF);
        	_out.write(byBuf,0, 4);
        	iRead=_insc.read(byBuf, 4, 4);
        	if (iRead!=4 || ( 
        		byBuf[0]!=byBuf[4] ||
        		byBuf[1]!=byBuf[5] ||
        		byBuf[2]!=byBuf[6] ||
        		byBuf[3]!=byBuf[7] )){
        		throw new IllegalArgumentException("Error Protocol Step 3");
        	}
        	
        	int iBlk=8192;
        	byte []byImg = new byte[iBlk];
        	iRead = in.read(byImg);
        	while(iRead>0){
        		_out.write(byImg, 0, iRead);
        		iRead = in.read(byImg);
        	}
        	in.close();
        	
        	iRead=_insc.read(byBuf, 0, 1);
        	if (iRead!=1)
        		 throw new IllegalArgumentException("Error Protocol Step 4");
        	iRead=_insc.read(byBuf, 0, 5);
        	if (iRead!=5)
        		 throw new IllegalArgumentException("Error Protocol Step 5");

        	_byEngineTranslator = byBuf[0];
            iLen = (byBuf[4] & 0x000000FF); iLen <<= 8; 
            iLen |= (byBuf[3] & 0x000000FF);iLen <<= 8; 
            iLen |= (byBuf[2] & 0x000000FF);iLen <<= 8; 
            iLen |= (byBuf[1] & 0x000000FF);
            if (iLen>0){
                _byOcrSrc = new byte[iLen];
                iRead=_insc.read(_byOcrSrc, 0, iLen);
	        	if (iRead!=iLen)
	        		 throw new IllegalArgumentException("Error Protocol Step 6");
            }
        	byBuf[0]=1;
        	_out.write(byBuf, 0, 1);

        	iRead=_insc.read(byBuf, 0, 4);
        	if (iRead!=4)
        		 throw new IllegalArgumentException("Error Protocol Step 7");
            iLen =  (byBuf[3] & 0x000000FF); iLen <<= 8;
            iLen |= (byBuf[2] & 0x000000FF);iLen <<= 8;
            iLen |= (byBuf[1] & 0x000000FF);iLen <<= 8;
            iLen |= (byBuf[0] & 0x000000FF);
            if (iLen>0){
            _byOcrDest = new byte[iLen];
                iRead=_insc.read(_byOcrDest, 0, iLen);
	        	if (iRead!=iLen)
	        		 throw new IllegalArgumentException("Error Protocol Step 8");
            }
	        byBuf[0]=1;
        	_out.write(byBuf, 0, 1);
        	handler.sendEmptyMessage(0);
        	
        	//byImage.close();
        	//byImage= null;
        	byBuf  = null;
        	bret=true;
    	}
    	catch(IOException ex)
    	{	_sErr = ex.getMessage();
    		CloseSocket();
    	}
		catch(Exception ex){
			_sErr = ex.getMessage();
			CloseSocket();
		}
		if (!bret)
			handler.sendEmptyMessage(1);
		CloseSocket();
		
    	return bret;
    }
	private Handler handler = new Handler() {
		public void handleMessage(Message msg) {
			super.handleMessage(msg);
			try{
				switch (msg.what)
				{
				case 0:
		        	_sSrc = new String(_byOcrSrc,"UTF-8");
		        	_sDst = new String(_byOcrDest,"UTF-8");
		        	_txtSrc.setText(_sSrc);
		        	_txtDest.setText(_sDst);
		        	break;
				case 1:
					showDialog(DIALOG_ERR_PROTO);
					break;
				case 2:
		            SharedPreferences settings = getSharedPreferences(getResources().getString(R.string.setting_file), 0);
		            _sEmailAddrTo= settings.getString(getResources().getString(R.string.setting_lang_sendto), "");
					showDialog(DIALOG_REQUEST_EMAIL);
					break;
				}
			}
			catch(Exception ex){
				
			}
		}
	};
	static final int RESULT_EMAIL     = 0;
	static final int RESULT_PHONE_NUM = 1;
	
	protected void onActivityResult(int requestCode, int resultCode, Intent data) {
		if (resultCode != RESULT_CANCELED) {
			switch (requestCode)
            {
			case RESULT_PHONE_NUM:
				String sNum = data.getStringExtra(getResources().getString(R.string.intent_number));
				_sNumbMobile=sNum;
				showDialog(DIALOG_REQUEST_NUM);
				break;
			case RESULT_EMAIL:
				String sEmail = data.getStringExtra(getResources().getString(R.string.intent_email));
				_sEmailAddrTo=sEmail;
				showDialog(DIALOG_REQUEST_EMAIL);
				break;
            }
		}
	}

	@Override
    protected Dialog onCreateDialog(int id) {
		LayoutInflater inflater=LayoutInflater.from(this);
		AlertDialog.Builder dlg;
		switch (id) {
		case DIALOG_REQUEST_NUM:
			addView=inflater.inflate(R.layout.request_num, null);
			_edNumb = (EditText)addView.findViewById(R.id.edNum);			
			_edNumb.setText(_sNumbMobile);
			ImageView imgSrc = (ImageView)addView.findViewById(R.id.imgSrc);
			imgSrc.setImageResource(_iIdSourceIcon);
			ImageView imgDst = (ImageView)addView.findViewById(R.id.imgDst);
			imgDst.setImageResource(_iIdDestIcon);
			
			dlg = new AlertDialog.Builder(this);
			dlg.setTitle("Number");
			dlg.setNeutralButton(R.string.Contacts, new DialogInterface.OnClickListener() {
				public void onClick(DialogInterface dialog,int whichButton) {
					removeDialog(DIALOG_REQUEST_NUM);
					Intent in = new Intent(Result.this, ListContacts.class);
					in.putExtra(getResources().getString(R.string.intent_searchemail), false);
		        	startActivityForResult(in, RESULT_PHONE_NUM);
				}
			});
			dlg.setPositiveButton(android.R.string.ok, new DialogInterface.OnClickListener() {
				public void onClick(DialogInterface dialog,int whichButton) {
			    	removeDialog(DIALOG_REQUEST_EMAIL);
	            	Intent sentIntent = new Intent();
	                Intent deliverIntent = new Intent();
	                PendingIntent sentPendingIntent =PendingIntent.getBroadcast(Result.this, 0, sentIntent,PendingIntent.FLAG_CANCEL_CURRENT);
	                PendingIntent deliverPendingIntent =PendingIntent.getBroadcast(Result.this, 0, deliverIntent,PendingIntent.FLAG_CANCEL_CURRENT);
	                
		        	SmsManager sm = SmsManager.getDefault();
		        	SharedPreferences settings = getSharedPreferences(getResources().getString(R.string.setting_file), 0);
		        	String sSmsNum= settings.getString(getResources().getString(R.string.setting_lang_nummob), "");
		        	_sSrc=_txtSrc.getText().toString();
		        	_sDst=_txtDest.getText().toString();
		        	
		        	if (r1.isChecked())	
			        	sm.sendTextMessage(sSmsNum,null, _sSrc.substring(0, 160), sentPendingIntent,deliverPendingIntent);
		        	else
		        		sm.sendTextMessage(sSmsNum,null, _sDst.substring(0, 160), sentPendingIntent,deliverPendingIntent);
				}
			});

			dlg.setNegativeButton("Cancel", null);
			dlg.setView(addView);
			return dlg.create();
			
		case DIALOG_TEXT:
			addView=inflater.inflate(R.layout.show_text, null);
			TextView txt = (TextView)addView.findViewById(R.id.edText);
			txt.setText(_sTxtShow);
			dlg = new AlertDialog.Builder(this);
			dlg.setPositiveButton(android.R.string.ok, new DialogInterface.OnClickListener() {
				public void onClick(DialogInterface dialog,int whichButton) {
			    	removeDialog(DIALOG_TEXT);
				}
			});
			dlg.setView(addView);
			return dlg.create();
			
		case DIALOG_REQUEST_EMAIL:
			addView=inflater.inflate(R.layout.request_email, null);
			_edEmail = (EditText)addView.findViewById(R.id.edEmail);
			_edEmail.setText(_sEmailAddrTo);
			
			dlg = new AlertDialog.Builder(this);
			dlg.setTitle("Email");
			dlg.setIcon(R.drawable.email);
			dlg.setNeutralButton(R.string.Contacts, new DialogInterface.OnClickListener() {
				public void onClick(DialogInterface dialog,int whichButton) {
					removeDialog(DIALOG_REQUEST_EMAIL);
					Intent in = new Intent(Result.this, ListContacts.class);
					in.putExtra(getResources().getString(R.string.intent_searchemail), true);
		        	startActivityForResult(in, RESULT_EMAIL);
				}
			});
			dlg.setPositiveButton(android.R.string.ok, new DialogInterface.OnClickListener() {
				public void onClick(DialogInterface dialog,int whichButton) {
		        	_iTypeWorker = WORKER_GET_EMAIL;
			    	StartProgressDialog();
			    	removeDialog(DIALOG_REQUEST_EMAIL);
				}
			});

			dlg.setNegativeButton("Cancel", null);
			dlg.setView(addView);
			return dlg.create();
		case DIALOG_ERR_PROTO:
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            //builder.setMessage("Comunication Error:"+_sErr);
            builder.setMessage("Comunication Error");
            builder.setIcon(R.drawable.alert_dialog_icon);
            builder.setPositiveButton(android.R.string.ok, null);
            builder.setCancelable(true);
            return builder.create();
		}
		return null;
	}

    private int GetDrawableLangFromText(String sLang)
    {
    	sLang = sLang.toLowerCase();
        int iRes = getResources().getIdentifier(getPackageName()+":drawable/"+sLang , null, null);
    	return iRes;
    }

    private static final int MENU_SENDTO_EMAIL  = Menu.FIRST+1;
    private static final int MENU_SENDTO_SMS    = Menu.FIRST+2;
    private static final int MENU_SENDTO_GOOGLE = Menu.FIRST+3;
    private static final int MENU_SENDTO_WIKI   = Menu.FIRST+4;
    private static final int MENU_RELOAD        = Menu.FIRST+5;
    
    @Override
	public boolean onCreateOptionsMenu(Menu menu) {
		menu.add(Menu.NONE, MENU_SENDTO_EMAIL, Menu.NONE, "Email")
		.setIcon(R.drawable.email);
		menu.add(Menu.NONE, MENU_SENDTO_SMS, Menu.NONE, "SMS")
		.setIcon(R.drawable.sms);
		menu.add(Menu.NONE, MENU_SENDTO_GOOGLE, Menu.NONE, "Google")
		.setIcon(R.drawable.google);
		menu.add(Menu.NONE, MENU_SENDTO_WIKI, Menu.NONE, "Wikipedia")
		.setIcon(R.drawable.wikipedia);
		menu.add(Menu.NONE, MENU_RELOAD, Menu.NONE, "Reload")
		.setIcon(R.drawable.reload);
		
		return(super.onCreateOptionsMenu(menu));
	}
    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
    	String sUrlSrc, sUrlDst;
    	Intent in;
    	SharedPreferences settings = getSharedPreferences(getResources().getString(R.string.setting_file), 0);
        switch (item.getItemId()) {
        case MENU_RELOAD:
        	_iTypeWorker = WORKER_SEND_IMAGE;
	    	StartProgressDialog();	    	
        	return true;
        	
        case MENU_SENDTO_EMAIL:
            _sEmailAddrTo= settings.getString(getResources().getString(R.string.setting_lang_sendto), "");
            if(_bRequestEmail) 
            	showDialog(DIALOG_REQUEST_EMAIL);
            else
            {	_iTypeWorker = WORKER_GET_EMAIL;
		    	StartProgressDialog();

            }
            return true;
            
        case MENU_SENDTO_SMS:
        	try{
                _sNumbMobile= settings.getString(getResources().getString(R.string.setting_lang_nummob), "");
            	showDialog(DIALOG_REQUEST_NUM);
        	}
	        catch(Exception e){
	        	Log.i(getClass().getSimpleName(), e.getMessage());
	        }
            return true;
            
        case MENU_SENDTO_GOOGLE:
        	try{	        	
            	_sSrc=_txtSrc.getText().toString();
            	_sDst=_txtDest.getText().toString();
	        	sUrlSrc = "http://www.google.com/search?q="+_sSrc+"&ie=UTF-8&oe=UTF-8&client=safari";
	        	sUrlDst = "http://www.google.com/search?q="+_sDst+"&ie=UTF-8&oe=UTF-8&client=safari";
	    		in = new Intent(Result.this, TabWeb.class);
	    		in.putExtra("urlsrc", sUrlSrc);
	    		in.putExtra("urldst", sUrlDst);
	    		in.putExtra("langsrc", _sLangSrc);
	    		in.putExtra("langdst", _sLangDst);
	            startActivity(in);
        	}
            catch (Exception ex){
            	
            }
            return true;
            
        case MENU_SENDTO_WIKI:
        	_sSrc=_txtSrc.getText().toString();
        	_sDst=_txtDest.getText().toString();
        	sUrlSrc = "http://mobile.wikipedia.org/transcode.php?go=" + _sSrc;
        	sUrlDst = "http://mobile.wikipedia.org/transcode.php?go=" + _sDst;
    		in = new Intent(Result.this, TabWeb.class);
    		in.putExtra("urlsrc", sUrlSrc);
    		in.putExtra("urldst", sUrlDst);
    		in.putExtra("langsrc", _sLangSrc);
    		in.putExtra("langdst", _sLangDst);
            startActivity(in);
            return true;
        }
        return super.onOptionsItemSelected(item);
    }    
}
        
