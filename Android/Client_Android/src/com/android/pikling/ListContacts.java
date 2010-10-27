package com.android.pikling;

import android.app.ListActivity;
import android.content.Context;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.widget.ListView;
import android.view.ViewGroup;
import android.widget.BaseAdapter;
import android.widget.TextView;
import android.database.Cursor;
import android.provider.Contacts.ContactMethods;
import android.provider.Contacts.People;
import android.content.Intent;

public class ListContacts extends ListActivity {
	public Cursor _cur;
	boolean _bSearchEmail;
    private static class EfficientAdapter extends BaseAdapter {
        private LayoutInflater mInflater;
        ListContacts _context;
        boolean _bSearchEmail;
        
        public EfficientAdapter(Context context, boolean bSearchEmail) {
        	_context = (ListContacts)context;
            // Cache the LayoutInflate to avoid asking for a new one each time.
            mInflater     = LayoutInflater.from(context);
            _bSearchEmail = bSearchEmail;
            if (_bSearchEmail)
            	_context._cur = context.getContentResolver().query(ContactMethods.CONTENT_URI, null, null,null,ContactMethods.DISPLAY_NAME + " ASC");
            else
            	_context._cur = context.getContentResolver().query(People.CONTENT_URI, null, null,null,People.NAME + " ASC");
            	
            _context.startManagingCursor(_context._cur);
        }

        /**
         * The number of items in the list is determined by the number of speeches
         * in our array.
         *
         * @see android.widget.ListAdapter#getCount()
         */
        public int getCount() {
            return _context._cur.getCount();
        }

        /**
         * Since the data comes from an array, just returning the index is
         * sufficent to get at the data. If we were using a more complex data
         * structure, we would return whatever object represents one row in the
         * list.
         *
         * @see android.widget.ListAdapter#getItem(int)
         */
        public Object getItem(int position) {
            return position;
        }

        /**
         * Use the array index as a unique id.
         *
         * @see android.widget.ListAdapter#getItemId(int)
         */
        public long getItemId(int position) {
            return position;
        }

        /**
         * Make a view to hold each row.
         *
         * @see android.widget.ListAdapter#getView(int, android.view.View,
         *      android.view.ViewGroup)
         */
        public View getView(int position, View convertView, ViewGroup parent) {
            // A ViewHolder keeps references to children views to avoid unneccessary calls
            // to findViewById() on each row.
            ViewHolder holder;

            // When convertView is not null, we can reuse it directly, there is no need
            // to reinflate it. We only inflate a new View when the convertView supplied
            // by ListView is null.
            if (convertView == null) {
                convertView = mInflater.inflate(R.layout.list_contacts, null);

                // Creates a ViewHolder and store references to the two children views
                // we want to bind data to.
                holder = new ViewHolder();
                holder.txtName  = (TextView) convertView.findViewById(R.id.txtName);
                holder.txtEmail = (TextView) convertView.findViewById(R.id.txtEmail);

                convertView.setTag(holder);
            } else {
                // Get the ViewHolder back to get fast access to the TextView
                // and the ImageView.
                holder = (ViewHolder) convertView.getTag();
            }

            // Bind the data efficiently with the holder.
            _context._cur.moveToPosition(position);
            int iIDName, iIDEmail, iIDNumb;
            if (_bSearchEmail)
            {	iIDName = _context._cur.getColumnIndex(ContactMethods.DISPLAY_NAME);
            	iIDEmail= _context._cur.getColumnIndex(ContactMethods.DATA);
                if (iIDEmail>=0)
                	holder.txtEmail.setText(_context._cur.getString(iIDEmail));
        	}
            else{
            	iIDName = _context._cur.getColumnIndex(People.NAME);
            	iIDNumb = _context._cur.getColumnIndex(People.NUMBER);
            	if(iIDNumb>=0)
            		holder.txtEmail.setText(_context._cur.getString(iIDNumb));
            }
            if (iIDName>=0)
            	holder.txtName.setText(_context._cur.getString(iIDName));
            

            return convertView;
        }

        static class ViewHolder {
            TextView txtName;
            TextView txtEmail;
        }
    }

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        
        _bSearchEmail = getIntent().getBooleanExtra(getResources().getString(R.string.intent_searchemail), false);
        if (_bSearchEmail)
        	_cur = getContentResolver().query(ContactMethods.CONTENT_URI, null, null,null,ContactMethods.ISPRIMARY + " DESC"); 
        else
        	_cur = getContentResolver().query(People.CONTENT_URI, null, null,null,People.ISPRIMARY + " DESC");
        startManagingCursor(_cur);
        
        setListAdapter(new EfficientAdapter(this, _bSearchEmail));
    }
    @Override
    protected void onListItemClick (ListView l, View v, int position, long id){
    	_cur.moveToPosition(position);
    	Intent in = new Intent();
        if (_bSearchEmail){
            int iIDEmail = _cur.getColumnIndex(ContactMethods.DATA);
            if (iIDEmail>=0){
            	String sEmail = _cur.getString(iIDEmail);
            	in.putExtra(getResources().getString(R.string.intent_email), sEmail);
            }
        }
        else{
        	int iIDNumb = _cur.getColumnIndex(People.NUMBER);
            if (iIDNumb>=0){
            	String sNumb = _cur.getString(iIDNumb);
            	in.putExtra(getResources().getString(R.string.intent_number), sNumb);
            }
        }
    	setResult(RESULT_OK, in);
    	finish();
    }
}

