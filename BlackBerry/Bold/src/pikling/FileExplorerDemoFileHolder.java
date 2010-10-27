/*
 * FileExplorerDemoFileHolder.java
 *
 * Copyright © 1998-2008 Research In Motion Ltd.
 * 
 * Note: For the sake of simplicity, this sample application may not leverage
 * resource bundles and resource strings.  However, it is STRONGLY recommended
 * that application developers make use of the localization features available
 * within the BlackBerry development platform to ensure a seamless application
 * experience across a variety of languages and geographies.  For more information
 * on localizing your application, please refer to the BlackBerry Java Development
 * Environment Development Guide associated with this release.
 */

package pikling;

/**
 * Helper class to store information about directories and files that are being
 * read from the system.
 */
/*package*/ final class FileExplorerDemoFileHolder
{
    private String _filename;
    private String _path;
    private boolean _isDir;
    
    
    /**
     * Constructor.  Pulls the path and file name from the provided string.
     * 
     * @param fileinfo The path and file name provided from the FileConnection.
     */
    FileExplorerDemoFileHolder(String fileinfo) 
    {
        // Pull the information from the URI provided from the original FileConnection.
        int slash = fileinfo.lastIndexOf('/');
        
        if ( slash == -1 ) 
        {
            throw new IllegalArgumentException( "fileinfo must have a slash" );
        }
        
        _path = fileinfo.substring(0, ++slash);
        _filename = fileinfo.substring(slash);
    }

    
    /**
     * Retrieves the file name.
     * 
     * @return Name of the file, or null if it's a directory.
     */
    String getFileName() 
    {
        return _filename;
    }

    /**
     * Retrieves the path of the directory or file.
     * 
     * @return Fully qualified path.
     */
    String getPath() 
    {
        return _path;
    }

    /**
     * Determins if the FileHolder is a directory.
     * @return true if FileHolder is directory, otherwise false.
     */
    boolean isDirectory() 
    {
        return _isDir;
    }

    /**
     * Enables setting of directory for FileHolder.
     * @param isDir true if FileHolder should be a directory.
     */
    void setDirectory(boolean isDir) 
    {
        _isDir = isDir;
    }
}
