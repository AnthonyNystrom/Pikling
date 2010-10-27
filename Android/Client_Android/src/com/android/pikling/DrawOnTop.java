package com.android.pikling;

import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.DashPathEffect;
import android.graphics.Paint;
import android.graphics.PathEffect;
import android.graphics.Rect;
import android.graphics.Region;
import android.util.DisplayMetrics;
import android.util.Log;
import android.view.View;

class DrawOnTop extends View { 
	Bitmap _bmCmdCrop, _bmCmdRotate, _bmCropLeftRight, _bmCropTopBottom, _bmCropMove;
	Bitmap _bmImg;
	Rect _rcTop, _rcRight, _rcBottom, _rcLeft, _rcInternal;	
    int _iMinBorder=30, _iScreenHeight, _iScreenWidth;
	boolean _bEditMode, _bShowCropLeft, _bShowCropTop, _bShowCropRight, _bShowCropBottom, _bAnimPath, _bEnableCrop, _bShowCropMove, _bDrawViewPort;
	CameraPreview _context;
	float _fPhase;
	
    public DrawOnTop(Context context, boolean bEditMode) { 
        super(context); 
    	_context=(CameraPreview)context;
    	_bDrawViewPort=true;
    	
    	// get screen size
    	DisplayMetrics dm = new DisplayMetrics();
    	_context.getWindowManager().getDefaultDisplay().getMetrics(dm); 
    	_iScreenWidth  = dm.widthPixels; 
    	_iScreenHeight = dm.heightPixels;

    	_rcTop     = new Rect();
        _rcRight   = new Rect();
        _rcBottom  = new Rect();
        _rcLeft    = new Rect();
        _rcInternal= new Rect();
        
        ResetCoord();
        
        _bEditMode = bEditMode;
        _bmImg = null;
        if (_bEditMode)
        	EnableCrop(true);
        
        _bmCropLeftRight = BitmapFactory.decodeResource(getResources(), R.drawable.crop_left_right);
        _bmCropTopBottom = BitmapFactory.decodeResource(getResources(), R.drawable.crop_top_bottom);
        _bmCropMove      = BitmapFactory.decodeResource(getResources(), R.drawable.crop_move);
        _bShowCropLeft=_bShowCropTop=_bShowCropRight=_bShowCropBottom=false;
    } 
    
    @Override 
    protected void onDraw(Canvas canvas) { 

        Paint paint = new Paint();
        if (_bmImg!=null)
        	canvas.drawBitmap(_bmImg, 0,0, paint);        
        
        if (_bDrawViewPort){
	        paint.setAlpha(0x30);
	        canvas.clipRect(0,0,_iScreenWidth, _iScreenHeight);
	        canvas.clipRect(_rcInternal,Region.Op.DIFFERENCE);
	        canvas.drawRect(0,0,_iScreenWidth, _iScreenHeight,paint);
	        canvas.clipRect(0,0,_iScreenWidth, _iScreenHeight, Region.Op.REPLACE);
        }
	    if (_bEnableCrop){
	        paint.setStyle(Paint.Style.STROKE);
	        paint.setStrokeWidth(2);
	        paint.setColor(Color.RED);
	        PathEffect pe = new DashPathEffect(new float[] {10, 5, 5, 5}, _fPhase);
	        _fPhase++;
	        paint.setPathEffect(pe);
	        canvas.drawRect(_rcInternal, paint);
	        if (_bAnimPath && !_bShowCropMove && !_bShowCropLeft && !_bShowCropRight && !_bShowCropTop && !_bShowCropBottom)
	        	invalidate();
	        //Log.i("onDraw", "H:"+_rcInternal.height()+" W:"+_rcInternal.width());
	        
	        if (_bShowCropLeft)
	        	canvas.drawBitmap(_bmCropLeftRight, _rcInternal.left-_bmCropLeftRight.getWidth()/2, _rcInternal.top+_rcInternal.height()/2-_bmCropLeftRight.getHeight()/2, paint);
	        if (_bShowCropRight)
	        	canvas.drawBitmap(_bmCropLeftRight, _rcInternal.right-_bmCropLeftRight.getWidth()/2, _rcInternal.top+_rcInternal.height()/2-_bmCropLeftRight.getHeight()/2, paint);
	        if (_bShowCropTop)
	        	canvas.drawBitmap(_bmCropTopBottom, _rcInternal.left+_rcInternal.width()/2-_bmCropTopBottom.getWidth()/2, _rcInternal.top-_bmCropTopBottom.getHeight()/2, paint);
	        if (_bShowCropBottom)
	        	canvas.drawBitmap(_bmCropTopBottom, _rcInternal.left+_rcInternal.width()/2-_bmCropTopBottom.getWidth()/2, _rcInternal.bottom-_bmCropTopBottom.getHeight()/2, paint);
	        if (_bShowCropMove)
	        	canvas.drawBitmap(_bmCropMove, _rcInternal.left+_rcInternal.width()/2-_bmCropMove.getWidth()/2, _rcInternal.top+_rcInternal.height()/2-_bmCropMove.getHeight()/2, paint);
	        	
	    }
    	super.onDraw(canvas);
    }
    void EnableViewPort(boolean bEnable){
    	_bDrawViewPort=bEnable;
    }
    public Rect getLeftRc(){
    	return _rcLeft;
    }
    public Rect getToptRc(){
    	return _rcTop;
    }
    public Rect getRightRc(){
    	return _rcRight;
    }
    public Rect getBottomRc(){
    	return _rcBottom;
    }
    public Rect getInRc(){
    	return _rcInternal;
    }
    boolean CalcAreas(Rect rcInRect)
    {
    	if (_rcInternal.left<_iMinBorder)
    		_rcInternal.left=_iMinBorder;
    	if (_rcInternal.top<_iMinBorder)
    		_rcInternal.top=_iMinBorder;
    	if (_rcInternal.bottom>_iScreenHeight-_iMinBorder)
    		_rcInternal.bottom=_iScreenHeight-_iMinBorder;
    	if (_rcInternal.right>_iScreenWidth-_iMinBorder)
    		_rcInternal.right=_iScreenWidth-_iMinBorder;
    	
    	if (rcInRect.height()<_iMinBorder || rcInRect.width()<_iMinBorder || rcInRect.left<0 || 
    		rcInRect.top<0 || rcInRect.right>_iScreenWidth || rcInRect.bottom>_iScreenHeight){
    		Log.i("CalcAreas", "ret 1");
    		return false;
    	}    		
    	if (rcInRect.left<_iMinBorder){
    		Log.i("CalcAreas", "ret 2");
    		return false;
    	}    		
    	if (rcInRect.top<_iMinBorder){
    		Log.i("CalcAreas", "ret 3");
    		return false;
    	}    		
    	if (rcInRect.right>_iScreenWidth-_iMinBorder){
    		Log.i("CalcAreas", "ret 4");
    		return false;
    	}    		
    	if (rcInRect.bottom>_iScreenHeight-_iMinBorder){
    		Log.i("CalcAreas", "ret 5");
    		return false;
    	}    		
    	if (rcInRect.left>rcInRect.right){
    		Log.i("CalcAreas", "ret 6");
    		return false;
    	}    		
    	if (rcInRect.top>rcInRect.bottom){
    		Log.i("CalcAreas", "ret 7");
    		return false;
    	}    		
    	
    	Log.i("CalcAreas", "return true.H:"+rcInRect.height()+" W:"+rcInRect.width());   	
    	_rcInternal = rcInRect;

    	_rcTop.set(0,0,_iScreenWidth,_rcInternal.top);
        _rcRight.set(_rcInternal.left+_rcInternal.width(),0,_iScreenWidth,_iScreenHeight);        
        _rcBottom.set(0,_rcInternal.top+_rcInternal.height(),_iScreenWidth,_iScreenHeight);
        _rcLeft.set(0,0,_rcInternal.left,_iScreenHeight);
        
        return true;
    }
    public void MoveInRect(int iDiffX, int iDiffY){
    	Rect rc = _rcInternal;
    	rc.right  += iDiffX;
    	rc.bottom += iDiffY;
    	rc.left   += iDiffX;
    	rc.top    += iDiffY;
    	if(CalcAreas(rc)){
    		_bShowCropMove=true;
    		invalidate();
    	}
    }
    public void UpdateBorderTop(int iDiff){
    	Rect rc = _rcInternal;
    	rc.top+=iDiff;
    	if (CalcAreas(rc)){
    		//Log.i("UpdateBorderTop","CALC TRUE");
    		_bShowCropTop=true;
    		invalidate();
    	}/*
    	else
    		Log.i("UpdateBorderTop","CALC FALSE");*/
    	
    }
    public void UpdateBorderBottom(int iDiff){
    	Rect rc = _rcInternal;
    	rc.bottom+=iDiff;
    	if (CalcAreas(rc)){
    		_bShowCropBottom=true;
    		invalidate();
    	}
    }
    public void UpdateBorderRight(int iDiff){
    	Rect rc = _rcInternal;
    	rc.right+=iDiff;
    	if (CalcAreas(rc)){
    		_bShowCropRight=true;
    		invalidate();
    	}
    }
    public void UpdateBorderLeft(int iDiff){
    	Rect rc = _rcInternal;
    	rc.left+=iDiff;
    	if (CalcAreas(rc)){
    		_bShowCropLeft=true;
    		invalidate();
    	}
    }
    public void SetImage(Bitmap bm){
    	_bmImg = bm;
    	invalidate();
    }
    void ShowCropLeft(boolean bShowCropLeft){
    	_bShowCropLeft=bShowCropLeft;    	
    }
    void ShowCropTop(boolean bShowCropTop){
    	_bShowCropTop=bShowCropTop;    	
    }
    void ShowCropBottom(boolean bShowCropBottom){
    	_bShowCropBottom=bShowCropBottom;    	
    }
    void ShowCropRight(boolean bShowCropRight){
    	_bShowCropRight=bShowCropRight;    	
    }
    void ShowCropMove(boolean bShowCropMove){
    	_bShowCropMove=bShowCropMove;    	
    }
    public void HideIconsCrop(){
    	ShowCropLeft(false);
    	ShowCropRight(false);
    	ShowCropTop(false);
    	ShowCropBottom(false);
    	ShowCropMove(false);    
    	ShowAnimCropArea(true);
    }
    private void ShowAnimCropArea(boolean bAnimCropArea){
    	_bAnimPath = bAnimCropArea;
    	if (bAnimCropArea)
    		invalidate();
    }
    void EnableCrop(boolean bEnable){
    	_bEnableCrop = bEnable;
    	ShowAnimCropArea(_bEnableCrop);
    }
    public void ResetCoord(){
    	Rect rc = new Rect(_iMinBorder,_iMinBorder,_iScreenWidth-_iMinBorder,_iScreenHeight-_iMinBorder);
        CalcAreas(rc);
    }
} 
