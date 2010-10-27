//
//  ResultDialog.h
//  Pikling
//
//  Created by acerone on 4/27/09.
//  Copyright 2009 7touch Group. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "WebServices.h"

#import <AddressBook/AddressBook.h>
#import <AddressBookUI/AddressBookUI.h>

@interface ResultDialog : UIViewController <WebServicesDelegate, UIScrollViewDelegate, UIActionSheetDelegate, ABPeoplePickerNavigationControllerDelegate> {
	IBOutlet UIImageView *imageView;
	
	IBOutlet UIScrollView *scrollView;
	IBOutlet UIPageControl *pageControl;
	NSMutableArray *viewControllers;
	BOOL pageControlUsed;
		
	UIView *loadingView;
	
	NSString *emailAddr;
	NSString *typeOfFile;
}

@property (nonatomic, retain) UIScrollView *scrollView;
@property (nonatomic, retain) UIPageControl *pageControl;
@property (nonatomic, retain) NSMutableArray *viewControllers;

- (IBAction)changePage:(id)sender;
- (void)loadScrollViewWithPage:(int)page;

// Funzioni interne alla classe
- (IBAction)askEmailDestination;
- (void)performEmailRequestTo:(NSString *)emailAddr;


- (IBAction)done;
- (IBAction)sendDocTo;
- (IBAction)sendPdfTo;
- (IBAction)sendAllTo;
- (IBAction)saveImageOnLibrary:(id)sender;
- (IBAction)openInSearchEngines;

@end
