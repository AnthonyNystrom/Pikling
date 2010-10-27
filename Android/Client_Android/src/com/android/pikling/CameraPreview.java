package com.android.pikling;

import android.app.Activity;
import android.app.ProgressDialog;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Matrix;
import android.graphics.Rect;
import android.hardware.Camera;
import android.media.AudioManager;
import android.media.ToneGenerator;
import android.net.Uri;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.os.Message;
import android.provider.MediaStore.Images.Media;
import android.view.MotionEvent;
import android.view.SurfaceHolder;
import android.view.SurfaceView;
import android.view.View;
import android.view.Window;
import android.view.WindowManager;
import android.view.ViewGroup.LayoutParams;

import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.OutputStream;
import java.net.URI;
import android.view.KeyEvent;
import android.widget.ImageView;
import android.util.DisplayMetrics;
import android.util.Log;

// ----------------------------------------------------------------------

public class CameraPreview extends Activity implements View.OnFocusChangeListener, View.OnTouchListener, View.OnClickListener, Runnable, SurfaceHolder.Callback {    
    Uri uri;
    OutputStream _filoutputStream;
    Camera.PictureCallback _cb;
    float _fPreviousX, _fPreviousY;
    DrawOnTop _Draw;
    Uri _uriImg;
    ProgressDialog _dialog;
    Bitmap _bm;
    int _iTypeWorker=WORKER_OFF;
    boolean _bEndThread=false, _bPreview, _bFocussed=false, _bCameraPressed, _bPreviewRunning, bCropEnable;
	Thread _thread=null;
    Camera _Camera;
    SurfaceView _surfaceView;
    ImageView _imgCamera, _imgAccept, _imgCancel, _imgView, _imgRotate, _imgCrop;
    int _iBorder, _iScreenWidth, _iScreenHeight, _iRotate, _iMyRotate;
    
    static final int WORKER_OFF               = 0;
	static final int WORKER_RESIZE_IMAGE_URI  = 1;
	static final int WORKER_RESIZE_IMAGE_FILE = 2;
	static final int WORKER_CROP_ROTATE_IMAGE = 3;
		
	static final int TOUCH_BORDER_TOP    = 0x01;
	static final int TOUCH_BORDER_RIGHT  = 0x02;
	static final int TOUCH_BORDER_BOTTOM = 0x04;
	static final int TOUCH_BORDER_LEFT   = 0x08;
	static final int TOUCH_BORDER_INTO   = 0x10;
	
	static final int CAPTURE_SIZE_X = 800;
	static final int CAPTURE_SIZE_Y = 600;
	
    @Override
	protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        
        // Hide the window title.
        requestWindowFeature(Window.FEATURE_NO_TITLE);
        getWindow().setFlags(WindowManager.LayoutParams.FLAG_FULLSCREEN,WindowManager.LayoutParams.FLAG_FULLSCREEN);
        
    	// get screen size
    	DisplayMetrics dm = new DisplayMetrics();
    	getWindowManager().getDefaultDisplay().getMetrics(dm); 
    	_iScreenWidth  = dm.widthPixels; 
    	_iScreenHeight = dm.heightPixels;

                        
        Intent in = getIntent();
        _bPreview = in.getBooleanExtra(getResources().getString(R.string.intent_preview), true);
        _uriImg   = in.getData();
        // Create our Preview view and set it as the content of our activity.
        _Draw = new DrawOnTop(this, !_bPreview);
        
        setContentView(R.layout.camerapreview);
        addContentView(_Draw, new LayoutParams(LayoutParams.WRAP_CONTENT, LayoutParams.WRAP_CONTENT));
        
        _imgCamera=(ImageView) findViewById(R.id.imgCamera);
        _imgCamera.setOnFocusChangeListener(this);
        _imgCamera.setOnTouchListener(this);
        _imgCamera.setOnClickListener(this);
        
        _imgAccept=(ImageView) findViewById(R.id.imgAccept);
        _imgAccept.setOnFocusChangeListener(this);
        _imgAccept.setOnTouchListener(this);
        _imgAccept.setOnClickListener(this);
    	_imgAccept.setVisibility(View.INVISIBLE);

        _imgCancel=(ImageView) findViewById(R.id.imgCancel);
        _imgCancel.setOnFocusChangeListener(this);
        _imgCancel.setOnTouchListener(this);
        _imgCancel.setOnClickListener(this);
    	_imgCancel.setVisibility(View.INVISIBLE);
    	
    	_imgRotate=(ImageView) findViewById(R.id.imgRotate);
    	_imgRotate.setOnFocusChangeListener(this);
    	_imgRotate.setOnTouchListener(this);
    	_imgRotate.setOnClickListener(this);
    	_imgRotate.setVisibility(View.INVISIBLE);
    	
    	_imgCrop=(ImageView) findViewById(R.id.imgCrop);
    	_imgCrop.setOnFocusChangeListener(this);
    	_imgCrop.setOnTouchListener(this);
    	_imgCrop.setOnClickListener(this);
    	_imgCrop.setVisibility(View.INVISIBLE);
    	        
    	_imgView=(ImageView) findViewById(R.id.imgImage);
    	_surfaceView = (SurfaceView)findViewById(R.id.surface);
    	
        if (!_bPreview){
        	_surfaceView.setVisibility(View.INVISIBLE);
			_imgCamera.setVisibility(View.INVISIBLE);
	    	_imgAccept.setVisibility(View.VISIBLE);
	    	_imgCancel.setVisibility(View.VISIBLE);
	    	_imgRotate.setVisibility(View.VISIBLE);
	    	_imgCrop.setVisibility(View.VISIBLE);
	    	_Draw.EnableViewPort(false);
	    	bCropEnable=false;
    		EnableCrop(bCropEnable);
	    	StartProgressDialog();
        	_iTypeWorker = WORKER_RESIZE_IMAGE_URI;
        }
        else{
        	_imgView.setVisibility(View.INVISIBLE);
        	_imgCamera.setVisibility(View.VISIBLE);
        	_imgAccept.setVisibility(View.INVISIBLE);
        	_imgCancel.setVisibility(View.INVISIBLE);
        	
        	_iTypeWorker = WORKER_OFF;
            SurfaceHolder surfaceHolder = _surfaceView.getHolder();
            surfaceHolder.addCallback(this);
            surfaceHolder.setType(SurfaceHolder.SURFACE_TYPE_PUSH_BUFFERS);            
        }
    	_thread = new Thread(this);
    	_thread.start();
    }
    public void onClick(View v) {
    	if (v.getId()==_imgCamera.getId())    		
    		AutoFocusClick();
    	else if (v.getId()==_imgCancel.getId())
    		Cancel();
    	else if (v.getId()==_imgRotate.getId())
    		RotateImgDisp();    	
    	else if (v.getId()==_imgCrop.getId())
    	{	bCropEnable=!bCropEnable;
    		EnableCrop(bCropEnable);
    	}
    	else if (v.getId()==_imgAccept.getId())
    		Accept();
    }
    void EnableCrop(boolean bEnable){
    	_Draw.EnableCrop(bEnable);    	
    	if (bEnable)    	
    		_imgCrop.setImageResource(R.drawable.crop_off);
    	else{
    		_imgCrop.setImageResource(R.drawable.crop);
    		_Draw.ResetCoord();
    	}
    }
    
    void RotateImgDisp(){
    	if (_iTypeWorker!=WORKER_OFF)
    		return;
    	StartProgressDialog();
        _iMyRotate+=90;
        if (_iMyRotate>=360)
        	_iMyRotate=0;
        _iRotate=_iMyRotate;
        if (_bPreview)
        	_iTypeWorker=WORKER_RESIZE_IMAGE_FILE;
        else
        	_iTypeWorker=WORKER_RESIZE_IMAGE_URI;
    }
    void Accept(){
    	_iTypeWorker=WORKER_CROP_ROTATE_IMAGE;
    }
    void Cancel(){
		_Draw.EnableCrop(false);
		if (_bPreview){
			_imgCamera.setVisibility(View.VISIBLE);
	    	_imgAccept.setVisibility(View.INVISIBLE);
	    	_imgCancel.setVisibility(View.INVISIBLE);
	    	_imgRotate.setVisibility(View.INVISIBLE);
	    	_imgCrop.setVisibility(View.INVISIBLE);
	    	_imgView.setVisibility(View.INVISIBLE);
	    	StartStopPreviewCamera(true);
		}
		else{
    		Intent intent = new Intent();
   	        setResult(RESULT_CANCELED, intent);
   	        finish();
		}
    }
    void StartStopPreviewCamera(boolean bStart){
    	if (bStart)
    		_Camera.startPreview();
    	else if (_bPreviewRunning)
    		_Camera.stopPreview();
    	_bPreviewRunning = bStart;
    }
    public void FileCopy() {
	  
    	try{
    		deleteFile(getResources().getString(R.string.filename_img));
    		FileOutputStream out = openFileOutput(getResources().getString(R.string.filename_img), MODE_WORLD_WRITEABLE);
		    FileInputStream in   = this.openFileInput(getResources().getString(R.string.filename_img_tmp));
		    
		    int c;
		    byte []byBuff = new byte[8192];
		    while ((c=in.read(byBuff,0,byBuff.length))>0)
		      out.write(byBuff,0,c);
		
		    in.close();
		    out.close();
    	}
    	catch(FileNotFoundException ex){
    		
    	}
    	catch(IOException ex){
    		
    	}
    }
    
    public void onFocusChange (View v, boolean hasFocus){
    	if (v.getId()==_imgCamera.getId()){    		
    		if (hasFocus)
    			_imgCamera.setBackgroundDrawable(getResources().getDrawable(R.drawable.camera_click_f));
    		else
    			_imgCamera.setBackgroundDrawable(null);
    	}
    	else if (v.getId()==_imgAccept.getId()){    		
    		if (hasFocus)
    			_imgAccept.setImageResource(R.drawable.accept_f);
    		else
    			_imgAccept.setImageResource(R.drawable.accept);
    	}
    	else if (v.getId()==_imgCancel.getId()){    		
    		if (hasFocus)
    			_imgCancel.setImageResource(R.drawable.cancel_f);
    		else
    			_imgCancel.setImageResource(R.drawable.cancel);
    	}
    	else if (v.getId()==_imgRotate.getId()){    		
    		if (hasFocus)
    			_imgRotate.setImageResource(R.drawable.rotate_f);
    		else
    			_imgRotate.setImageResource(R.drawable.rotate);
    	}
    	else if (v.getId()==_imgCrop.getId()){    		
    		if (hasFocus)
    		{	if (bCropEnable)
    				_imgCrop.setImageResource(R.drawable.crop_off_f);
    			else
    				_imgCrop.setImageResource(R.drawable.crop_f);
    		}
    		else{
    			if (bCropEnable)
    				_imgCrop.setImageResource(R.drawable.crop_off);
    			else
    				_imgCrop.setImageResource(R.drawable.crop);
    		}
    	}

    }
    public boolean onTouch(View v, MotionEvent event) {
    	if (v.getId()==_imgCamera.getId()){
            switch (event.getAction()) {
	        case MotionEvent.ACTION_DOWN:
	        	_imgCamera.setBackgroundDrawable(getResources().getDrawable(R.drawable.camera_click_press));
	        	break;
	        case MotionEvent.ACTION_UP:
	        	_imgCamera.setBackgroundDrawable(getResources().getDrawable(R.drawable.camera_click_f));
	        	Capture();	        	
	        	break;
            }
    	}
    	else if (v.getId()==_imgAccept.getId()){
            switch (event.getAction()) {
	        case MotionEvent.ACTION_DOWN:
	        	_imgAccept.setImageResource(R.drawable.accept_f);
	        	break;
	        case MotionEvent.ACTION_UP:
	        	_imgAccept.setImageResource(R.drawable.accept);
	        	Accept();
	        	break;
            }
    	}
    	else if (v.getId()==_imgCancel.getId()){
            switch (event.getAction()) {
	        case MotionEvent.ACTION_DOWN:
	        	_imgCancel.setImageResource(R.drawable.cancel_f);
	        	break;
	        case MotionEvent.ACTION_UP:
	        	_imgCancel.setImageResource(R.drawable.cancel);
	        	Cancel();
	        	break;
            }
    	}
    	else if (v.getId()==_imgRotate.getId()){
            switch (event.getAction()) {
	        case MotionEvent.ACTION_DOWN:
	        	_imgRotate.setImageResource(R.drawable.rotate_press);
	        	break;
	        case MotionEvent.ACTION_UP:
	        	_imgRotate.setImageResource(R.drawable.rotate);
	        	RotateImgDisp();
	        	break;
            }
    	}
    	else if (v.getId()==_imgCrop.getId()){
            switch (event.getAction()) {
	        case MotionEvent.ACTION_DOWN:
	        	if (bCropEnable)
	        		_imgCrop.setImageResource(R.drawable.crop_off_press);
	        	else
	        		_imgCrop.setImageResource(R.drawable.crop_off);
	        	break;
	        case MotionEvent.ACTION_UP:
	        	if (bCropEnable)
	        		_imgCrop.setImageResource(R.drawable.crop_off);
	        	else
	        		_imgCrop.setImageResource(R.drawable.crop);
	        	bCropEnable=!bCropEnable;
	        	EnableCrop(bCropEnable);
	        	break;
            }
    	}
    	return true;
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
			case WORKER_CROP_ROTATE_IMAGE:
				try{
					if (!_bPreview)
						CreateTmpImgFile();
			        if (_iMyRotate!=0){
		        		RotateImg();
			        }
			        if (bCropEnable){
			        	CropImg();
			        }
				}
				catch (Exception ex){
				}
				handler.sendEmptyMessage(2);
				_iTypeWorker=WORKER_OFF;				
				break;
			case WORKER_RESIZE_IMAGE_URI:
				try{
			        ResizeImg(_uriImg);
			        //_Draw.SetImage(_bm);
			        handler.sendEmptyMessage(0);
				}
				catch (Exception ex){
					handler.sendEmptyMessage(1);
				}
				_iTypeWorker=WORKER_OFF;				
				break;
			case WORKER_RESIZE_IMAGE_FILE:
				try{
			        ResizeImg();
			        //_Draw.SetImage(_bm);
			        handler.sendEmptyMessage(0);
				}
				catch (Exception ex){
					handler.sendEmptyMessage(1);
				}
				_iTypeWorker=WORKER_OFF;				
				break;			
			}
		}
	}
	private Handler handler = new Handler() {
		public void handleMessage(Message msg) {
			
			switch (msg.what){
			case 0:
				EndProgressDialog();
				_imgView.setImageBitmap(_bm);
				/*BitmapDrawable bd = new BitmapDrawable(); 
				_imgView.setBackgroundDrawable(bd);*/
				_imgView.setVisibility(View.VISIBLE);
				break;
			case 1:
				EndProgressDialog();
				break;
			case 2:
				EndProgressDialog();
		    	FileCopy();
				End();
				break;
			}
				
			super.handleMessage(msg);
		}
	};
	void RotateImg(Uri uri){
		try{
			Bitmap bm = Media.getBitmap(getContentResolver(), uri);
			RotateImg(bm);
			bm.recycle();
		}
    	catch (Exception e) {
    		Log.i(getClass().getSimpleName(), e.getMessage());
        }
	}
	
	void RotateImg(){
		String sFile = getResources().getString(R.string.filename_img_tmp);
		Bitmap bm = BitmapFactory.decodeFile(getFilesDir().getPath()+"/"+sFile);
		RotateImg(bm);
		bm.recycle();
	}
	void RotateImg(Bitmap bm){
		try{
			Matrix matrix = new Matrix();
			matrix.postRotate(_iMyRotate);				
	        Bitmap rotatedBitmap = Bitmap.createBitmap(bm, 0, 0, (int)bm.getWidth(), (int)bm.getHeight(), matrix, true);
	        ByteArrayOutputStream bytes = new ByteArrayOutputStream();
	        rotatedBitmap.compress(Bitmap.CompressFormat.JPEG, 80, bytes);     
	        String sFile = getResources().getString(R.string.filename_img_tmp);
	        deleteFile(sFile);    		
    		FileOutputStream fos = openFileOutput(sFile, MODE_WORLD_WRITEABLE);
    		fos.write(bytes.toByteArray());
    		fos.close();
    		
    		bytes.close();
    		bm.recycle();
    		bm=null;
    		rotatedBitmap.recycle();
	    	rotatedBitmap=null;
		}
		catch (IOException e) {
	    }
	}
	void CropImg(Uri uri){
		try{
			Bitmap bm = Media.getBitmap(getContentResolver(), uri);
			CropImg(bm);
			bm.recycle();
		}
    	catch (Exception e) {
    		Log.i(getClass().getSimpleName(), e.getMessage());
        }
	}
	void CropImg(){
		String sFile = getResources().getString(R.string.filename_img_tmp);
		Bitmap bm = BitmapFactory.decodeFile(getFilesDir().getPath()+"/"+sFile);
		CropImg(bm);
		bm.recycle();
	}
	void CropImg(Bitmap bm){
		try{
			Rect rc = _Draw.getInRc();
			// transform coord disp to coord real image
			if (_iMyRotate==90 || _iMyRotate==270)
			{
				rc.left   = rc.left*CAPTURE_SIZE_Y/_iScreenWidth;;
				rc.right  = rc.right*CAPTURE_SIZE_Y/_iScreenWidth;
				rc.top    = rc.top*CAPTURE_SIZE_X/_iScreenHeight;
				rc.bottom = rc.bottom *CAPTURE_SIZE_X/_iScreenHeight;
			}
			else{
				rc.left   = rc.left*CAPTURE_SIZE_X/_iScreenWidth;
				rc.right  = rc.right*CAPTURE_SIZE_X/_iScreenWidth;
				rc.top    = rc.top*CAPTURE_SIZE_Y/_iScreenHeight;
				rc.bottom = rc.bottom *CAPTURE_SIZE_Y/_iScreenHeight;
			}
			
	        Bitmap cropedBitmap = Bitmap.createBitmap(bm, rc.left, rc.top, (int)rc.width(), (int)rc.height(), null, true);
	        ByteArrayOutputStream bytes = new ByteArrayOutputStream();
	        cropedBitmap.compress(Bitmap.CompressFormat.JPEG, 80, bytes);
	        String sFile = getResources().getString(R.string.filename_img_tmp);
	        deleteFile(sFile);    		
			FileOutputStream fos = openFileOutput(sFile, MODE_WORLD_WRITEABLE);
			fos.write(bytes.toByteArray());
			fos.close();
			
			bytes.close();
			bm.recycle();
			bm=null;
			cropedBitmap.recycle();
			cropedBitmap=null;		
		}
		catch (IOException e) {
	    }
	}
	void ResizeImg(){
		try{
			String sFile = getResources().getString(R.string.filename_img_tmp);
			Bitmap bm = BitmapFactory.decodeFile(getFilesDir().getPath()+"/"+sFile);
			_bm=ResizeImg(bm, 480, _iRotate);
			_iRotate=0;
			bm.recycle();
		}
    	catch (Exception e) {
    		Log.i(getClass().getSimpleName(), e.getMessage());
        }  
	}
	void ResizeImg(Uri uri){
		try{
			Bitmap bm = Media.getBitmap(getContentResolver(), uri);
			_bm=ResizeImg(bm, 480, _iRotate);
			_iRotate=0;
			bm.recycle();
		}
    	catch (Exception e) {
    		Log.i(getClass().getSimpleName(), e.getMessage());
        }  
	}
	static Bitmap ResizeImg(Bitmap bm, int iWidth, int iRotate){
		Bitmap bmRet=null;
    	try{
			float iW, iH;
			iW = bm.getWidth();
			iH = bm.getHeight();
	        float fratio = iH/iW;
	        float fH = fratio*iWidth;
	        // calculate the scale - in this case = 0.4f 
	        float scaleWidth = ((float) iWidth) / iW; 
	        float scaleHeight = ((float) fH) / iH;
	        // create a matrix for the manipulation 
	        Matrix matrix = new Matrix(); 
	        // resize the bit map 
	        matrix.postScale(scaleWidth, scaleHeight); 
	        if (iRotate!=0)
	        	matrix.postRotate(iRotate); 
	        // recreate the new Bitmap
	        /*if (_bm!=null)
	        	_bm.recycle();*/
	        bmRet = Bitmap.createBitmap(bm, 0, 0, (int)iW, (int)iH, matrix, true);
	    }
    	catch (Exception e) {
    		//Log.i(getClass().getSimpleName(), e.getMessage());
        }        
    	return bmRet;
	}   
	void Capture(){
    	try {
    		_cb = new Camera.PictureCallback() {
    			public void onPictureTaken(byte[] data, Camera c) {
    				try{
    					FileOutputStream fos = openFileOutput(getResources().getString(R.string.filename_img_tmp), MODE_WORLD_WRITEABLE);
    					fos.write(data);
    					fos.close();
	    				
	    				_imgCamera.setVisibility(View.INVISIBLE);
	    		    	_imgAccept.setVisibility(View.VISIBLE);
	    		    	_imgCancel.setVisibility(View.VISIBLE);
	    		    	_imgRotate.setVisibility(View.VISIBLE);
	    		    	_imgCrop.setVisibility(View.VISIBLE);	    		    		    		    
    				}
    				catch(Exception ex)
    				{
    				}
    			}
    		};
    		
    	} catch(Exception ex ){
    		ex.printStackTrace();
    		//Log.e(getClass().getSimpleName(), ex.getMessage(), ex);
    	}
    	_Camera.takePicture(null, mPictureCallbackJpeg, _cb);
    	_bFocussed     =false;
    	_bCameraPressed=false;
	}
	void AutoFocus(){
		_bCameraPressed = true;
        _Camera.autoFocus(mAutoFocus);
	}
	void AutoFocusClick(){
		if(_bFocussed)  
            Capture();  
    	else
    		AutoFocus();
    	
	}
	@Override 
    public boolean onKeyDown(int keyCode, KeyEvent event)
    {
    	if(keyCode == KeyEvent.KEYCODE_CAMERA) {
    		AutoFocusClick();
    		return true;
	    }
    	else if(keyCode == KeyEvent.KEYCODE_FOCUS){
    		AutoFocus();
    		return true;
    	}
    	return super.onKeyDown(keyCode, event);
    }

    private void End(){
    	if (_bPreview){
	    	StartStopPreviewCamera(false);
	        _Camera.release();        
	        File f = new File(getResources().getString(R.string.filename_img));
	        URI javaUri = f.toURI();
	        Uri uri = Uri.parse(javaUri.toString());
	        Intent intent = new Intent();
	        setResult(RESULT_OK, intent);
	        intent.setData(uri);
    	}
    	else{
    		Intent intent = new Intent();
   	        setResult(RESULT_OK, intent);
    	}
	    finish();
    }
    
    @Override 
    public boolean onTouchEvent(MotionEvent e) {
        float x  = e.getX();
        float y  = e.getY();
        float dx = x - _fPreviousX;
        float dy = y - _fPreviousY;
        Rect rcTop, rcBottom, rcRight, rcLeft, rcInto;
        super.onTouchEvent(e);
        switch (e.getAction()) {        	
        case MotionEvent.ACTION_UP:
        	_iBorder=0;
        	_Draw.HideIconsCrop();        	
        	break;
        case MotionEvent.ACTION_DOWN:
        	_fPreviousY=y;
        	_fPreviousX=x;
            rcTop    = _Draw.getToptRc();
            rcBottom = _Draw.getBottomRc();
            rcRight  = _Draw.getRightRc();
            rcLeft   = _Draw.getLeftRc();
            rcInto   = _Draw.getInRc();
            if(rcTop.contains((int)x, (int)y)){
                //Log.i(getClass().getSimpleName(), "IN TOP"+"DX:"+dx+ " DY:"+dy, null);
            	_iBorder=TOUCH_BORDER_TOP;
            }
            if(rcBottom.contains((int)x, (int)y)){
                //Log.i(getClass().getSimpleName(), "IN BOTTOM"+"DX:"+dx+ " DY:"+dy, null);
            	_iBorder|=TOUCH_BORDER_BOTTOM;
            }
            if(rcLeft.contains((int)x, (int)y)){
                //Log.i(getClass().getSimpleName(), "IN LEFT"+"DX:"+dx+ " DY:"+dy, null);
            	_iBorder|=TOUCH_BORDER_LEFT;
            }
            if(rcRight.contains((int)x, (int)y)){
                //Log.i(getClass().getSimpleName(), "IN RIGHT"+"DX:"+dx+ " DY:"+dy, null);
            	_iBorder|=TOUCH_BORDER_RIGHT;
            }
            if(rcInto.contains((int)x, (int)y)){
                //Log.i(getClass().getSimpleName(), "INTO"+"DX:"+dx+ " DY:"+dy, null);
            	_iBorder|=TOUCH_BORDER_INTO;
            }
        	break;
        case MotionEvent.ACTION_MOVE:
        	if (dy>=1.0 || dy<1.0){
                //Log.i(getClass().getSimpleName(), "X:"+x+ " Y:"+y, null);
        		if ((_iBorder & TOUCH_BORDER_TOP)!=0){
        			_Draw.UpdateBorderTop((int)dy);
                	_fPreviousX = x;
                	_fPreviousY = y;
                	Log.i("onTouchEvent", "TOUCH_BORDER_TOP");
        		}
        		if ((_iBorder & TOUCH_BORDER_BOTTOM)!=0){
                	_Draw.UpdateBorderBottom((int)dy);
                	_fPreviousX = x;
                	_fPreviousY = y;
                	Log.i("onTouchEvent", "TOUCH_BORDER_BOTTOM");
        		}
        		if ((_iBorder & TOUCH_BORDER_LEFT)!=0){
                	_Draw.UpdateBorderLeft((int)dx);
                	_fPreviousX = x;
                	_fPreviousY = y;
                	Log.i("onTouchEvent", "TOUCH_BORDER_LEFT");
        		}
        		if ((_iBorder & TOUCH_BORDER_RIGHT)!=0){
                	_Draw.UpdateBorderRight((int)dx);
                	_fPreviousX = x;
                	_fPreviousY = y;
                	Log.i("onTouchEvent", "TOUCH_BORDER_RIGHT");
        		}
        		if ((_iBorder & TOUCH_BORDER_INTO)!=0){
                	_Draw.MoveInRect((int)dx, (int)dy);
                	_fPreviousX = x;
                	_fPreviousY = y;                	
                	Log.i("onTouchEvent", "TOUCH_BORDER_INTO");
        		}
        	}
        }
    	return true;
    }
    
	@Override
	protected void onDestroy() {
		super.onDestroy();
		
		_bEndThread=true;
		if (_bm!=null){
			_bm.recycle();
			_bm=null;
		}
	}
	
	Camera.PictureCallback mPictureCallbackJpeg= new Camera.PictureCallback() {
        public void onPictureTaken(byte[] data, Camera c) {
            Log.e(getClass().getSimpleName(), "PICTURE CALLBACK JPEG: data.length = " + data);
        }
    };
    Camera.AutoFocusCallback mAutoFocus = new Camera.AutoFocusCallback(){
        public void onAutoFocus(boolean success,Camera c){      
        	if (success) 
            {	
        		ToneGenerator tg = new ToneGenerator(AudioManager.STREAM_SYSTEM, 100);
        		if (tg != null)
        			tg.startTone(ToneGenerator.TONE_PROP_BEEP2);
        		_bFocussed=true;
    			if(_bCameraPressed)
    				Capture();
             }
             else
             {	ToneGenerator tg = new ToneGenerator(AudioManager.STREAM_SYSTEM, 100);
            	 if (tg != null)
            		 tg.startTone(ToneGenerator.TONE_PROP_BEEP2);
                 if(_bCameraPressed)
                	 Capture();
             }
                 
        }
    };
    public void surfaceCreated(SurfaceHolder holder) {
        // The Surface has been created, acquire the camera and tell it where
        // to draw.
	
        _Camera = Camera.open();
        try {
           _Camera.setPreviewDisplay(holder);
        } catch (IOException exception) {
            _Camera.release();
            _Camera = null;
            // TODO: add more exception handling logic here
        }
    }

    public void surfaceDestroyed(SurfaceHolder holder) {
        // Surface will be destroyed when we return, so stop the preview.
        // Because the CameraDevice object is not a shared resource, it's very
        // important to release it when the activity is paused.    	
    	if (_bPreview && _Camera!=null && _bPreviewRunning)
    		StartStopPreviewCamera(false);
        _Camera = null;
    }

    
    public void surfaceChanged(SurfaceHolder holder, int format, int w, int h) {
        // Now that the size is known, set up the camera parameters and begin
        // the preview.
        Camera.Parameters parameters = _Camera.getParameters();
        parameters.setPreviewSize(w, h);
        parameters.setPictureSize(CAPTURE_SIZE_X, CAPTURE_SIZE_Y);
        _Camera.setParameters(parameters);
        
        if (_bPreview){
        	StartStopPreviewCamera(true);
        }
    }
    void CreateTmpImgFile(){
    	try{    		
			Bitmap bm = Media.getBitmap(getContentResolver(), _uriImg);
			Bitmap bmResized=ResizeImg(bm, 800, 0);
			BitmapSavetoFile(bmResized, 80, getResources().getString(R.string.filename_img_tmp));
			bm.recycle();
			bmResized.recycle();
	    }
		catch (Exception e) {
			Log.i(getClass().getSimpleName(), e.getMessage());
	    }
	}
    public void BitmapSavetoFile(Bitmap newBitmap, int nQuality, String BitmapName) throws IOException{ 
	    OutputStream os = openFileOutput(BitmapName, MODE_WORLD_WRITEABLE); 
	    try { 
	    	Bitmap.CompressFormat format = Bitmap.CompressFormat.JPEG; 
    		BitmapName = BitmapName.toLowerCase(); 
    		if (BitmapName.endsWith("jpg") || BitmapName.endsWith("jpeg")) 
    		{ 
    			format = Bitmap.CompressFormat.JPEG; 
    	    } 
    	    else if (BitmapName.endsWith("png")) 
    	    { 
    	    	format = Bitmap.CompressFormat.PNG; 
    	    } 
    		newBitmap.compress(format, nQuality, os); 
	    }catch(Exception e){ 
	    	//showAlert("Save Bitmap", e.toString(), "Close", false); 
    	}finally { 
    		os.close(); 
    	} 
    } 
}
