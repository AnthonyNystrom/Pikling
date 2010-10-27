package com.android.pikling;

import java.io.IOException;
import java.util.ArrayList;

import org.xmlpull.v1.XmlPullParser;
import org.xmlpull.v1.XmlPullParserException;

import android.content.Context;
import android.content.res.XmlResourceParser;

public class LanguagesData {
	
	XmlResourceParser _xml;	
	ArrayList _Langs = new ArrayList();
	int _iKey=KEY_NONE;
	
	static final int KEY_NONE 			= -1;
	static final int KEY_ABBREVIATION 	= 0;
	static final int KEY_ENGLISH 		= 1;
	static final int KEY_ORIGINAL 		= 2;
	
	public LanguagesData(Context c)
	{
		try
		{
			Data d=null;
			_xml= c.getResources().getXml(R.xml.languages);
			int eventType = _xml.getEventType();
	        while (eventType != XmlPullParser.END_DOCUMENT)
	        {
	        	if(eventType == XmlPullParser.START_DOCUMENT) {
	        		System.out.println("Start document");
	        	} else if(eventType == XmlPullParser.END_DOCUMENT) {
	        		System.out.println("End document");
	        	} else if(eventType == XmlPullParser.START_TAG) {
	        		if (_xml.getName().compareTo("item")==0){	        			
	        			d = new Data();
	        			_Langs.add(d);
	        		}
	        		else if (_xml.getName().compareTo("abbreviation")==0)
	        			_iKey=KEY_ABBREVIATION;
	        		else if (_xml.getName().compareTo("english")==0)
	        			_iKey=KEY_ENGLISH;
	        		else if (_xml.getName().compareTo("original")==0)
	        			_iKey=KEY_ORIGINAL;	        		
	        			 
	        		System.out.println("Start tag "+_xml.getName());
	        	} else if(eventType == XmlPullParser.END_TAG) {
	        		System.out.println("End tag "+_xml.getName());
	        	} else if(eventType == XmlPullParser.TEXT) {
	        		System.out.println("Text "+_xml.getText());
	        		if (d!=null){
		        		switch (_iKey){
		        		case KEY_ABBREVIATION:
		        			d._sAbbrev=_xml.getText();
		        			break;
		        		case KEY_ENGLISH:
		        			d._sEnglish=_xml.getText();
		        			break;
		        		case KEY_ORIGINAL:
		        			d._sOriginal=_xml.getText();
		        			break;
		        		}
	        		}
	        		_iKey=KEY_NONE;
	        	}
	        	eventType = _xml.next();	        	
	        }
	        
		}
		catch(XmlPullParserException ex){
			
		}
		catch(IOException ex){
			
		}
	}
	public int getCount(){
		return _Langs.size();
	}
	public String getAbbrev(int iPosition){
		Data d = (Data)_Langs.get(iPosition);
		return d._sAbbrev;
	}
	public String getEnglish(int iPosition){
		Data d = (Data)_Langs.get(iPosition);
		return d._sEnglish;
	}
	public String getOriginal(int iPosition){
		Data d = (Data)_Langs.get(iPosition);
		return d._sOriginal;
	}
	public class Data {
		public String _sAbbrev;
		public String _sEnglish;
		public String _sOriginal;
	}
	
}
