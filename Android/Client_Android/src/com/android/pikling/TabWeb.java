package com.android.pikling;

import android.app.TabActivity;
import android.content.Intent;
import android.os.Bundle;
import android.widget.TabHost;

public class TabWeb extends TabActivity {
	@Override
    protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		
		final TabHost tabHost = getTabHost();
		
		Intent intent = getIntent();
		
		String sUrlSrc  = intent.getStringExtra("urlsrc");
		String sUrlDst  = intent.getStringExtra("urldst");
		String sLangSrc = intent.getStringExtra("langsrc");
		String sLangDst = intent.getStringExtra("langdst");
		
		Intent inSrc = new Intent(this, MyBrowser.class);
		Intent inDst = new Intent(this, MyBrowser.class);
		inSrc.setAction(sUrlSrc);
		inDst.setAction(sUrlDst);
		
        tabHost.addTab(tabHost.newTabSpec("tab1")
                .setIndicator("", getResources().getDrawable(GetDrawableLangFromText(sLangSrc)))
                .setContent(inSrc));
        tabHost.addTab(tabHost.newTabSpec("tab2")
                .setIndicator("", getResources().getDrawable(GetDrawableLangFromText(sLangDst)))
                .setContent(inDst));

	}
	private int GetDrawableLangFromText(String sLang)
    {
		sLang = sLang.toLowerCase();
        int iRes = getResources().getIdentifier(getPackageName()+":drawable/"+sLang , null, null);
    	return iRes;
    }
}
