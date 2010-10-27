package com.android.pikling;

import android.app.Activity;
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.BaseAdapter;
import android.widget.GridView;
import android.widget.ImageView;
import android.content.Context;
import android.content.Intent;
import android.widget.TextView;

public class Languages extends Activity implements  AdapterView.OnItemClickListener {
	LangAdapter _adap;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.languages); 
        GridView grid = (GridView) findViewById(R.id.grid);
        _adap = new LangAdapter(this);
        grid.setAdapter(_adap);
        grid.setOnItemClickListener(this);
    } 
    public void onItemClick(AdapterView parent, View v, int position, long id) {
    	Intent i = new Intent();
    	i.setAction(_adap._ld.getAbbrev(position));
    	setResult(RESULT_OK, i);
    	finish();
    }
    
    public class LangAdapter extends BaseAdapter {
    	private LayoutInflater _Inflater;
    	LanguagesData _ld;
    	
    	public LangAdapter(Context c){
    		_ld = new LanguagesData(c);
    		_Inflater = LayoutInflater.from(c);
    	}
    	
        public View getView(int position, View convertView, ViewGroup parent) {
        	ViewHolder holder;
        	
        	if (convertView == null) {  // if it's not recycled, initialize some attributes
        		convertView = _Inflater.inflate(R.layout.flag_text, null);
        		holder = new ViewHolder();
                holder.text = (TextView) convertView.findViewById(R.id.text);
                holder.icon = (ImageView) convertView.findViewById(R.id.icon);
                convertView.setLayoutParams(new GridView.LayoutParams(48, 68));
                convertView.setTag(holder); 
            } else {
            	holder = (ViewHolder) convertView.getTag();
            }        		
            //holder.text.setText(_LangText[position]);
            //holder.icon.setImageResource(_ImageIds[position]);
            holder.text.setText(_ld.getEnglish(position));
            String sAbbr = _ld.getAbbrev(position);
            int iRes = getResources().getIdentifier(getPackageName()+":drawable/"+sAbbr , null, null);
            if (iRes!=0)
            	holder.icon.setImageResource(iRes); 
            else
            	Log.i("getView", "MISS IMAGE:"+sAbbr+".png");
        	
            return convertView;
        }

        public final int getCount() {
            return _ld.getCount();
        }

        public final Object getItem(int position) {
            return position;
        }

        public final long getItemId(int position) {
            return position;
        }
        class ViewHolder {
            TextView text;
            ImageView icon;
        }
        
    }

}
