package com.android.pikling;


import java.io.ByteArrayOutputStream;
import java.io.FileOutputStream;
import java.io.IOException;

import android.app.Activity;
import android.os.Bundle;
import android.os.Handler;
import android.os.Message;
import android.provider.MediaStore.Images.Media;
import android.util.Log;
import android.widget.ImageView;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Matrix;
import android.graphics.drawable.BitmapDrawable;
import android.net.Uri;
import android.app.ProgressDialog;
import android.widget.Button;
import android.widget.ImageView.ScaleType;
import android.view.View.OnClickListener;
import android.view.MotionEvent;
import android.view.View;


public class Preview extends Activity implements View.OnFocusChangeListener, View.OnTouchListener, View.OnClickListener, Runnable {
	Thread _thread=null;
	ProgressDialog _dialog;
	Bitmap _bm;
	BitmapDrawable _bmd;
	ImageView _imgPreview, _imgRotate;
	int _iRotate, _iMyRotate;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
    	super.onCreate(savedInstanceState);
        setContentView(R.layout.preview);
        Button butDone=(Button) findViewById(R.id.butDone);
        butDone.setOnClickListener(OnDone);

        _imgRotate  =(ImageView) findViewById(R.id.imgRotate);
        _imgRotate.setOnClickListener(this);
        _imgRotate.setOnFocusChangeListener(this);
        _imgRotate.setOnTouchListener(this);
        
        _imgPreview=(ImageView) findViewById(R.id.imgPreview);
        
        StartProgressDialog();
        _iMyRotate=_iRotate=0;
    	_thread = new Thread(this);
    	_thread.start();
    }
    public void onClick(View v) {
    	if (v.getId()==_imgRotate.getId())
    		Rotate();
    }
    public boolean onTouch(View v, MotionEvent event) {
        
    	ImageView img=null;
    	int idResUp=0, idResDn=0, idRes=0;
    	if (v.getId()==_imgRotate.getId()){
    		img=_imgRotate;
    		idResUp = R.drawable.rotate_f;
    		idResDn = R.drawable.rotate_press;
    	}
        
        switch (event.getAction()) { 
	        case MotionEvent.ACTION_DOWN:
	        	idRes = idResDn;
	        	break;
	        case MotionEvent.ACTION_UP:
	        	idRes = idResUp;
	        	if (v.getId()==_imgRotate.getId())
	        		Rotate();
	        	break;
	        default:
	        	return true;
        }
        if (img!=null && idRes>=0)
        	img.setImageResource(idRes);
         
        return true; 
    } 
    public void onFocusChange (View v, boolean hasFocus){
    	ImageView img=null;
    	int idResF=0, idResUF=0;
    	if (v.getId()==_imgRotate.getId()){
    		img=_imgRotate;
    		idResF = R.drawable.rotate_f;
    		idResUF = R.drawable.rotate;
    	}
    	
        if (img!=null)
        {	if (hasFocus)
        		img.setImageResource(idResF);
        	else
        		img.setImageResource(idResUF);
        }
    }
    	

    private OnClickListener OnDone = new OnClickListener()
    {
        public void onClick(View v)
        {	Intent intent = new Intent();
        	setResult(RESULT_OK, intent);
        	finish();
        }
    };    
    private void Rotate()
    {
        StartProgressDialog();
        _iMyRotate+=90;
        _iRotate=_iMyRotate;
        _thread.stop();
        _thread = new Thread(this);
    	_thread.start();
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
        Intent IntPrev= getIntent();
        Uri uri=IntPrev.getData();
        LoadImg(uri, 800, "tmp.jpg");
        handler.sendEmptyMessage(0);
	}
	
	private Handler handler = new Handler() {
		public void handleMessage(Message msg) {
	        _imgPreview.setImageDrawable(_bmd);
	        _imgPreview.setScaleType(ScaleType.FIT_CENTER);
	        _bm.recycle();
			_bm=null;
			EndProgressDialog();
			super.handleMessage(msg);
		}
	};
	
	@Override
	protected void onDestroy() {
		super.onDestroy();
		if (_thread!=null)
			_thread.stop();
		_imgPreview.destroyDrawingCache();
		_bmd = null;
	}
	
		
	protected void LoadImg(Uri uri, int iWidth, String sFile){
    	try{
			_bm = Media.getBitmap(getContentResolver(), uri);
			ByteArrayOutputStream bytes = new ByteArrayOutputStream();
			//_bm.compress(Bitmap.CompressFormat.JPEG, 90, bytes);
			float iW, iH;
			iW = _bm.getWidth();
			iH = _bm.getHeight();
			Log.i("LoadImg", "Jpg Width:" + iW + " Height:" + iH + " Width Dest:"+iWidth);
			/*
	        BitmapFactory.Options opts = new BitmapFactory.Options();
	        opts.inJustDecodeBounds = true;                            
	        _bm= BitmapFactory.decodeByteArray(bytes.toByteArray(), 0, bytes.toByteArray().length, opts);
	        opts.inJustDecodeBounds = false;
	        opts.inSampleSize = 4;
	        _bm= BitmapFactory.decodeByteArray(bytes.toByteArray(), 0, bytes.toByteArray().length, opts);
*/
	        float fratio = iH/iW;
	        float fH = fratio*iWidth;
	        // calculate the scale - in this case = 0.4f 
	        float scaleWidth = ((float) iWidth) / iW; 
	        float scaleHeight = ((float) fH) / iH;
	        // createa matrix for the manipulation 
	        Matrix matrix = new Matrix(); 
	        // resize the bit map 
	        matrix.postScale(scaleWidth, scaleHeight); 
	        // rotate the Bitmap 
	        if (_iRotate!=0)
	        	matrix.postRotate(_iRotate); 
	        _iRotate=0;
	        
	        // recreate the new Bitmap 
	        Bitmap resizedBitmap = Bitmap.createBitmap(_bm, 0, 0, (int)iW, (int)iH, matrix, true); 
	        // make a Drawable from Bitmap to allow to set the BitMap 
	        // to the ImageView, ImageButton or what ever 
	        _bmd = new BitmapDrawable(resizedBitmap); 
	        bytes.close();
	        bytes=null;
    		if (sFile!=""){
	    		deleteFile(sFile);
	    		FileOutputStream fos = openFileOutput(sFile, MODE_WORLD_WRITEABLE);
	    		bytes = new ByteArrayOutputStream();
	    		resizedBitmap.compress(Bitmap.CompressFormat.JPEG, 90, bytes);
	    		fos.write(bytes.toByteArray());
	    		fos.close();
    		}
    		
	    }
    	catch (IOException e) {
    		Log.i("ExptIOExceptionion", e.getMessage());
        }        
	}
}
