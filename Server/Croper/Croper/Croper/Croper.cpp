//
// The full "Square Detector" program.
// It loads several images subsequentally and tries to find squares in
// each image
//
#ifdef _CH_
#pragma package <opencv>
#endif

#include "stdafx.h"
#include "cv.h"
#include "highgui.h"
#include <stdio.h>
#include <math.h>
#include <string.h>

void drawSquaresAndCrop(char *pFileName, IplImage* imgFilter, IplImage* img, CvSeq* squares );
void drawCircleAndCrop(char *pFileName,  IplImage* img);
CvSeq* findSquares4( IplImage* img, CvMemStorage* storage );
CvRect GetRect(CvPoint *pt);
IplImage* GetImageFilteredForSquareDetect(IplImage* img);

#define POINTS_NEAR 30

int thresh = 50;
IplImage* img = 0;
IplImage* img0 = 0;
CvMemStorage* storage = 0;
const char* wndname = "Croper Square Detection";
bool bPreviewFilter;

int main(int argc, char* argv[])
{
    int i=0, c;
	bPreviewFilter=false;
    // create memory storage that will contain all the dynamic data
    storage = cvCreateMemStorage(0);
	if (argc>1 && !strcmp(argv[1],"f"))
		bPreviewFilter=true;

	char names[255];
    while(1)
    {
		sprintf(names, "pic%d.png", i+1);
        // load i-th image
        img0 = cvLoadImage( names, 1 );
        if( !img0 )
        {
            printf("Couldn't load %s\n", names );
            break;
        }
        img = cvCloneImage( img0 );
        
        // create window and a trackbar (slider) with parent "image" and set callback
        // (the slider regulates upper threshold, passed to Canny edge detector) 
        cvNamedWindow( wndname, 1 );
        
        // find and draw the squares
		IplImage* imgFilter = GetImageFilteredForSquareDetect(img);
		IplImage* imgShow;
		if (bPreviewFilter)
			imgShow = imgFilter;
		else
			imgShow = img;
		drawSquaresAndCrop(names, imgShow, img, findSquares4( imgFilter, storage ) );
		//drawCircleAndCrop(names, img);
		cvReleaseImage(&imgFilter);
        
        // wait for key.
        // Also the function cvWaitKey takes care of event processing
        c = cvWaitKey(0);
        // release both images
        cvReleaseImage( &img );
        cvReleaseImage( &img0 );
        // clear memory storage - reset free space position
        cvClearMemStorage( storage );
        if( (char)c == 27 )
            break;
		i++;
    }
    
    cvDestroyWindow( wndname );
    
    return 0;
}


// helper function:
// finds a cosine of angle between vectors
// from pt0->pt1 and from pt0->pt2 
double angle( CvPoint* pt1, CvPoint* pt2, CvPoint* pt0 )
{
    double dx1 = pt1->x - pt0->x;
    double dy1 = pt1->y - pt0->y;
    double dx2 = pt2->x - pt0->x;
    double dy2 = pt2->y - pt0->y;
    return (dx1*dx2 + dy1*dy2)/sqrt((dx1*dx1 + dy1*dy1)*(dx2*dx2 + dy2*dy2) + 1e-10);
}

IplImage* GetImageFilteredForSquareDetect(IplImage* img)
{
	IplImage* imgEroded = cvCloneImage( img ); // make a copy of input image
	IplImage* imgSmooth = cvCloneImage( img ); // make a copy of input image
	
	cvSmooth(img, imgSmooth, CV_MEDIAN,3);
	cvErode( imgSmooth, imgEroded, NULL, 2);
	cvErode( imgSmooth, imgEroded, NULL, 2);

    cvReleaseImage( &imgSmooth );
	return imgEroded;
}

// returns sequence of squares detected on the image.
// the sequence is stored in the specified memory storage
CvSeq* findSquares4( IplImage* img, CvMemStorage* storage )
{
    CvSeq* contours;
    int i, c, l, N = 11;
	
    CvSize sz = cvSize( img->width & -2, img->height & -2 );
    IplImage* timg = cvCloneImage( img ); // make a copy of input image
    IplImage* gray = cvCreateImage( sz, 8, 1 ); 
    IplImage* pyr = cvCreateImage( cvSize(sz.width/2, sz.height/2), 8, 3 );
    IplImage* tgray;
    CvSeq* result;
    double s, t;
    // create empty sequence that will contain points -
    // 4 points per square (the square's vertices)
    CvSeq* squares = cvCreateSeq( 0, sizeof(CvSeq), sizeof(CvPoint), storage );
    
    // select the maximum ROI in the image
    // with the width and height divisible by 2
    cvSetImageROI( timg, cvRect( 0, 0, sz.width, sz.height ));
    
    // down-scale and upscale the image to filter out the noise
    cvPyrDown( timg, pyr, 7 );
    cvPyrUp( pyr, timg, 7 );
    tgray = cvCreateImage( sz, 8, 1 );
    
    // find squares in every color plane of the image
    for( c = 0; c < 3; c++ )
    {
        // extract the c-th color plane
        cvSetImageCOI( timg, c+1 );
        cvCopy( timg, tgray, 0 );
        
        // try several threshold levels
        for( l = 0; l < N; l++ )
        {
            // hack: use Canny instead of zero threshold level.
            // Canny helps to catch squares with gradient shading   
            if( l == 0 )
            {
                // apply Canny. Take the upper threshold from slider
                // and set the lower to 0 (which forces edges merging) 
                cvCanny( tgray, gray, 0, thresh, 5 );
                // dilate canny output to remove potential
                // holes between edge segments 
                cvDilate( gray, gray, 0, 1 );
            }
            else
            {
                // apply threshold if l!=0:
                //     tgray(x,y) = gray(x,y) < (l+1)*255/N ? 255 : 0
                cvThreshold( tgray, gray, (l+1)*255/N, 255, CV_THRESH_BINARY );
            }
            
            // find contours and store them all as a list
            cvFindContours( gray, storage, &contours, sizeof(CvContour),
                CV_RETR_LIST, CV_CHAIN_APPROX_SIMPLE, cvPoint(0,0) );
            
            // test each contour
            while( contours )
            {
                // approximate contour with accuracy proportional
                // to the contour perimeter
                result = cvApproxPoly( contours, sizeof(CvContour), storage,
                    CV_POLY_APPROX_DP, cvContourPerimeter(contours)*0.02, 0 );
                // square contours should have 4 vertices after approximation
                // relatively large area (to filter out noisy contours)
                // and be convex.
                // Note: absolute value of an area is used because
                // area may be positive or negative - in accordance with the
                // contour orientation
                if( result->total == 4 &&
                    fabs(cvContourArea(result,CV_WHOLE_SEQ)) > 1000 &&
                    cvCheckContourConvexity(result) )
                {
                    s = 0;
                    
                    for( i = 0; i < 5; i++ )
                    {
                        // find minimum angle between joint
                        // edges (maximum of cosine)
                        if( i >= 2 )
                        {
                            t = fabs(angle(
                            (CvPoint*)cvGetSeqElem( result, i ),
                            (CvPoint*)cvGetSeqElem( result, i-2 ),
                            (CvPoint*)cvGetSeqElem( result, i-1 )));
                            s = s > t ? s : t;
                        }
                    }
                    
                    // if cosines of all angles are small
                    // (all angles are ~90 degree) then write quandrange
                    // vertices to resultant sequence 
					if( s < 0.3 ){
						
						CvPoint *pt1 = (CvPoint*)cvGetSeqElem( result, 0 );
						CvPoint *pt2 = (CvPoint*)cvGetSeqElem( result, 1 );
						CvPoint *pt3 = (CvPoint*)cvGetSeqElem( result, 2 );
						CvPoint *pt4 = (CvPoint*)cvGetSeqElem( result, 3 );
						CvSeqReader reader;
						cvStartReadSeq( squares, &reader, 0 );
						CvPoint ptM[4];
						if (!squares->total)
						{	for( i = 0; i < 4; i++ )
								cvSeqPush( squares, (CvPoint*)cvGetSeqElem( result, i ));
						}
						else{
							// search if there are already similar square with POINTS_NEAR of tollerance
							int iNSquare = squares->total;
							bool bFound=false;
							for(i = 0; i < iNSquare && !bFound; i += 4)
							{
								// read 4 vertices
								CV_READ_SEQ_ELEM( ptM[0], reader );
								CV_READ_SEQ_ELEM( ptM[1], reader );
								CV_READ_SEQ_ELEM( ptM[2], reader );
								CV_READ_SEQ_ELEM( ptM[3], reader );
								if (abs(pt1->x-ptM[0].x)<POINTS_NEAR && abs(pt1->y-ptM[0].y)<POINTS_NEAR &&
									abs(pt2->x-ptM[1].x)<POINTS_NEAR && abs(pt2->y-ptM[1].y)<POINTS_NEAR &&
									abs(pt3->x-ptM[2].x)<POINTS_NEAR && abs(pt3->y-ptM[2].y)<POINTS_NEAR &&
									abs(pt4->x-ptM[3].x)<POINTS_NEAR && abs(pt4->y-ptM[3].y)<POINTS_NEAR)
								{	bFound=true;
								}
							}
							if (!bFound)
							{	cvSeqPush( squares, pt1);
								cvSeqPush( squares, pt2);
								cvSeqPush( squares, pt3);
								cvSeqPush( squares, pt4);
							}
						}
					}
                }
                
                // take the next contour
                contours = contours->h_next;
            }
        }
    }
    
    // release all the temporary images
    cvReleaseImage( &gray );
    cvReleaseImage( &pyr );
    cvReleaseImage( &tgray );
    cvReleaseImage( &timg );
	
    return squares;
}

void drawCircleAndCrop(char *pFileName,  IplImage* img)
{
	IplImage* cpy = cvCloneImage( img );
	IplImage* gray = cvCreateImage( cvGetSize(img), 8, 1 );
	cvCvtColor( img, gray, CV_BGR2GRAY );
	cvSmooth( gray, gray, CV_GAUSSIAN, 9, 9 );
	CvMemStorage* storage = cvCreateMemStorage(0);
	CvSeq* circles = cvHoughCircles(gray, storage, CV_HOUGH_GRADIENT, 2, gray->height/4, 200, 200);
	int i;
    for( i = 0; i < circles->total; i++ )
    {
         float* p = (float*)cvGetSeqElem( circles, i );
         cvCircle( gray, cvPoint(cvRound(p[0]),cvRound(p[1])), 3, CV_RGB(0,255,0), -1, 8, 0 );
         cvCircle( gray, cvPoint(cvRound(p[0]),cvRound(p[1])), cvRound(p[2]), CV_RGB(255,0,0), 3, 8, 0 );
    }
    // show the resultant image
    cvShowImage( wndname, gray );
	cvReleaseImage(&cpy);
	cvReleaseImage(&gray);
}

// the function draws all the squares in the image and crop and save images
void drawSquaresAndCrop(char *pFileName, IplImage* imgFilter, IplImage* img, CvSeq* squares )
{
    CvSeqReader reader;
    IplImage* cpy = cvCloneImage( imgFilter );
	IplImage* cpyc = cvCloneImage( imgFilter );
    int i;
	char sFileNameCroped[255];
    
    // initialize reader of the sequence
    cvStartReadSeq( squares, &reader, 0 );
    
    // read 4 sequence elements at a time (all vertices of a square)
    for(int iCnt=0, i = 0; i < squares->total; i += 4,iCnt++ )
    {
        CvPoint pt[4], *rect = pt;
        int count = 4;
        
        // read 4 vertices
        CV_READ_SEQ_ELEM( pt[0], reader );
        CV_READ_SEQ_ELEM( pt[1], reader );
        CV_READ_SEQ_ELEM( pt[2], reader );
        CV_READ_SEQ_ELEM( pt[3], reader );
        
        // draw the square as a closed polyline 
        cvPolyLine( cpy, &rect, &count, 1, 1, CV_RGB(0,255,0), 3, CV_AA, 0 );

		// Get Area to crop
		CvRect rc = GetRect(pt);
		// Filter the area full image
		if (abs(rc.width-img->width)>POINTS_NEAR ||
			abs(rc.height-img->height)>POINTS_NEAR){

			// Draw area
			CvPoint pt1, pt2;
			pt1.x = rc.x;
			pt1.y = rc.y;
			pt2.x = pt1.x+rc.width;
			pt2.y = pt1.y+rc.height;
			cvRectangle(cpy, pt1, pt2, CV_RGB(0,0,255),2);

			
			// sets the Region of Interest 
			// Note that the rectangle area has to be __INSIDE__ the image 
			cvSetImageROI(cpyc, rc);
			// create destination image 
			// Note that cvGetSize will return the width and the height of ROI 
			IplImage *img1 = cvCreateImage(cvGetSize(cpyc), 
										   cpyc->depth, 
										   cpyc->nChannels);
			 
			// copy subimage 
			cvCopy(cpyc, img1, NULL);		 
			// save file
			char stype[32];
			char sFile[255];
			strcpy(sFile, pFileName);
			strcpy(stype, &(pFileName[strlen(pFileName)-3]));
			sFile[strlen(pFileName)-4]=NULL;
			
			sprintf(sFileNameCroped, "%s_%d.%s", sFile,iCnt,stype);
			cvSaveImage(sFileNameCroped, img1);

			// always reset the Region of Interest
			cvResetImageROI(img1);    
		}
	}
    
    // show the resultant image
    cvShowImage( wndname, cpy );
    cvReleaseImage( &cpy );
	cvReleaseImage( &cpyc );
}
// Return a rect area to crop the image
CvRect GetRect(CvPoint *pt)
{
	CvRect rc;
	CvPoint lt, lb, rt, rb;
	// find most left top point
	lt.x=pt[1].x;
	lt.y=pt[1].y;
	for (int i=0; i<4; i++)
	{	if (pt[i].x < lt.x)
			lt.x = pt[i].x;
		if (pt[i].y < lt.y)
			lt.y = pt[i].y;
	}
	// find most right top point
	rt.x=pt[1].x;
	rt.y=pt[1].y;
	for (int i=0; i<4; i++)
	{	if (pt[i].x > rt.x)
			rt.x = pt[i].x;
		if (pt[i].y < rt.y)
			rt.y = pt[i].y;
	}
	// find most left bottom point
	lb.x=pt[1].x;
	lb.y=pt[1].y;
	for (int i=0; i<4; i++)
	{	if (pt[i].x < lb.x)
			lb.x = pt[i].x;
		if (pt[i].y > lb.y)
			lb.y = pt[i].y;
	}
	// find most right bottom point
	rb.x=pt[1].x;
	rb.y=pt[1].y;
	for (int i=0; i<4; i++)
	{	if (pt[i].x > rb.x)
			rb.x = pt[i].x;
		if (pt[i].y > rb.y)
			rb.y = pt[i].y;
	}

	rc.x = lt.x;
	rc.y = lt.y;
	rc.width = rt.x - lt.x;
	rc.height= rb.y - rt.y;

	
	return rc;
}

