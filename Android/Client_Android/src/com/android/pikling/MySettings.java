package com.android.pikling;

import android.app.Activity;
import android.app.Dialog;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.view.MotionEvent;
import android.view.View;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.EditText;
import android.widget.Toast;
import android.widget.ToggleButton;
import android.content.res.Resources;

public class MySettings extends Activity implements View.OnFocusChangeListener, View.OnTouchListener, View.OnClickListener {
    
    ImageView _imgSourceLang, _imgDestLang;
    TextView _txtSourceLang, _txtDestLang, _txtEmail, _txtSms;
    EditText _edSendTo, _edNumbMob;
    ToggleButton _togAskSend;
    ImageView _imgAccept;
    boolean _bAskSend, _bShowEmail;
    
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.settings);
        
        _imgSourceLang = (ImageView) findViewById(R.id.ImageSourceLang);
        _imgDestLang   = (ImageView) findViewById(R.id.ImageDestLang);
        _txtSourceLang = (TextView) findViewById(R.id.TextSourceLang);
        _txtDestLang   = (TextView) findViewById(R.id.TextDestLang);
        _txtEmail      = (TextView) findViewById(R.id.txtEmail);
        _txtSms        = (TextView) findViewById(R.id.txtSms);
        _edSendTo      = (EditText) findViewById(R.id.EditEmail);
        _edNumbMob     = (EditText) findViewById(R.id.EditNumMob);
        _togAskSend    = (ToggleButton) findViewById(R.id.ToggleAskSend);
        _imgAccept     = (ImageView) findViewById(R.id.ImageAccept);
        
        
        _imgDestLang.setOnTouchListener(this);
        _imgSourceLang.setOnTouchListener(this);
        _imgAccept.setOnClickListener(this);
        _imgAccept.setOnTouchListener(this);
        _imgAccept.setOnFocusChangeListener(this);
        _togAskSend.setOnClickListener(this);
        _txtSms.setOnClickListener(this);
        _txtSms.setOnTouchListener(this);
        
        Resources res = getResources();
        SharedPreferences settings = getSharedPreferences(res.getString(R.string.setting_file), 0);
        _txtSourceLang.setText(settings.getString(res.getString(R.string.setting_lang_source), "IT"));
        _txtDestLang.setText(settings.getString(res.getString(R.string.setting_lang_dest), "EN"));
        _edSendTo.setText(settings.getString(res.getString(R.string.setting_lang_sendto), ""));
        _txtEmail.setOnClickListener(this);
        _txtEmail.setOnTouchListener(this);
        _edNumbMob.setText(settings.getString(res.getString(R.string.setting_lang_nummob), ""));
        _bAskSend = settings.getBoolean(res.getString(R.string.setting_lang_asksend), false);
        _togAskSend.setChecked(_bAskSend);
        _imgSourceLang.setImageResource(GetDrawableLangFromText(_txtSourceLang.getText().toString()));
        _imgDestLang.setImageResource(GetDrawableLangFromText(_txtDestLang.getText().toString()));
    }
    public void onFocusChange (View v, boolean hasFocus){
    	if (v.getId()==_imgAccept.getId())
    	{
    		if (hasFocus)
    			_imgAccept.setImageResource(R.drawable.accept_f);
    		else
    			_imgAccept.setImageResource(R.drawable.accept);
    	}
    }
    private void Accept(){
    	
    	String sEmail = _edSendTo.getText().toString();
    	if (!sEmail.contains("@") || !sEmail.contains("."))
    	{	Toast.makeText(this, "Please insert a valid email address", Toast.LENGTH_SHORT).show();
    		return;
    	}    	
    	Resources res = getResources();
    	SharedPreferences.Editor editor = getSharedPreferences(res.getString(R.string.setting_file), 0).edit();
    	editor.putString(res.getString(R.string.setting_lang_source), _txtSourceLang.getText().toString());
    	editor.putString(res.getString(R.string.setting_lang_dest), _txtDestLang.getText().toString());
    	editor.putString(res.getString(R.string.setting_lang_sendto), _edSendTo.getText().toString());
    	editor.putString(res.getString(R.string.setting_lang_nummob), _edNumbMob.getText().toString());
    	editor.putBoolean(res.getString(R.string.setting_lang_asksend), _bAskSend);
    	editor.commit();
    	finish();
    }
    
    public void onClick(View v) {
    	if (v.getId()==_togAskSend.getId())
    		_bAskSend = !_bAskSend;
    	else if (v.getId()==_imgAccept.getId())
    		Accept();
    	else if (v.getId()==_txtEmail.getId())
    		ShowContactsList(true);
    	else if (v.getId()==_txtSms.getId())
        	ShowContactsList(false);    	
        
    }
    private void ShowFlags(int iFlagType)
    {
    	Intent intent = new Intent(MySettings.this, Languages.class);
        startActivityForResult(intent, iFlagType);
    }
    
    static final private int RESULT_FLAG_SOURCE = 0;
    static final private int RESULT_FLAG_DEST   = 1;
    static final private int RESULT_CONTACT_EMAIL = 2;
    static final private int RESULT_CONTACT_NUM   = 3;
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        if (resultCode != RESULT_CANCELED) {
        	String sLang = data.getAction();
        	if (requestCode == RESULT_FLAG_SOURCE) {
                _txtSourceLang.setText(sLang);
                _imgSourceLang.setImageResource(GetDrawableLangFromText(sLang));
        	}
        	else if (requestCode == RESULT_FLAG_DEST) {
        		_txtDestLang.setText(sLang);
        		_imgDestLang.setImageResource(GetDrawableLangFromText(sLang));
        	}
        	else if (requestCode == RESULT_CONTACT_EMAIL) {
        		String sEmailAddr = data.getStringExtra(getResources().getString(R.string.intent_email));
        		if (sEmailAddr!=null)
        			_edSendTo.setText(sEmailAddr);
        	}
        	else if (requestCode == RESULT_CONTACT_NUM) {
        		String sNum = data.getStringExtra(getResources().getString(R.string.intent_number));
        		if (sNum!=null)
        			_edNumbMob.setText(sNum);
        	}
        }
    }
    public boolean onTouch(View v, MotionEvent event) {
    	if (event.getAction()==MotionEvent.ACTION_DOWN)
	    {	if (v.getId()==_imgSourceLang.getId())
	    	{
	    		ShowFlags(RESULT_FLAG_SOURCE);
	    	}
	    	else if (v.getId()==_imgDestLang.getId())
	    	{
	    		ShowFlags(RESULT_FLAG_DEST);
	    	}
	    	else if (v.getId()==_imgAccept.getId())
	    		Accept();
	    	else if (v.getId()==_txtEmail.getId())
	    		ShowContactsList(true);	    	
	    	else if (v.getId()==_txtSms.getId())
	    		ShowContactsList(false);
	    }
    	return true;
    }
    void ShowContactsList(boolean bShowEmail){
    	_bShowEmail = bShowEmail;
		Intent in = new Intent(MySettings.this, ListContacts.class);
		in.putExtra(getResources().getString(R.string.intent_email), bShowEmail);
		if (bShowEmail)
			startActivityForResult(in, RESULT_CONTACT_EMAIL);
		else
			startActivityForResult(in, RESULT_CONTACT_NUM);
    }
    
    private int GetDrawableLangFromText(String sLang)
    {
    	sLang = sLang.toLowerCase();
        int iRes = getResources().getIdentifier(getPackageName()+":drawable/"+sLang , null, null);
    	return iRes;
    }
    
}
