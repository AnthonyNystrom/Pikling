//
//  pikAppDelegate.h
//  Pikling
//
//  Created by Alex on 23/04/09.
//  Copyright 7touch Group 2009. All rights reserved.
//
#import "WebServices.h"
#import <CoreLocation/CoreLocation.h>
#import <AVFoundation/AVFoundation.h>


#define kSourceLanguage 0
#define kDestinationLanguage 1

@class MainViewController;

@interface pikAppDelegate : NSObject <UIApplicationDelegate, AVAudioPlayerDelegate> {
	UIWindow *window;
	MainViewController *mainViewController;

	NSMutableDictionary *jobInfo;
	NSString *deviceInfoString;
	CLLocation *currentPosition;
}
@property (nonatomic, retain) IBOutlet UIWindow *window;
@property (nonatomic, retain) MainViewController *mainViewController;
@property (nonatomic, retain) NSMutableDictionary *jobInfo;
@property (nonatomic, retain) NSString *deviceInfoString;
@property (nonatomic, retain) CLLocation *currentPosition;

// Gestione del dizionario del JOB
- (void)loadJobDictionary;
- (void)saveJobDictionary;
- (void) getDeviceInfo;

-(IBAction) playSound:(NSString*)fileName withDelta:(const float)delta;

@end
