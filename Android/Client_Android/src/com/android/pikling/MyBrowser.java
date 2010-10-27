package com.android.pikling;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.webkit.WebView;

public class MyBrowser extends Activity {
	@Override
	public void onCreate(Bundle icicle) {
		super.onCreate(icicle);
		setContentView(R.layout.mybrowser);
		Intent in = getIntent();
		String sUrl = in.getAction();

		WebView browser=(WebView)findViewById(R.id.webkit);
		if (browser!=null)
			browser.loadUrl(sUrl);
		
	}
}