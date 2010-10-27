//
//  TwitterSettingsViewController.h
//  ABYF
//
//  Created by Alex Mantovani on 06/06/09.
//  Copyright 2009 7touch Group. All rights reserved.
//

#import <UIKit/UIKit.h>


@interface TwitterSettingsViewController : UIViewController <UITableViewDelegate, UITableViewDataSource, UITextFieldDelegate> {
	IBOutlet UITableView *twitterSettingsTableView;
	NSString *username;
	NSString *password;
	UITextField *isEditing;
	BOOL accountInfoChanged;
	NSString *twitterIpAddr;
}

@property(nonatomic, retain) NSString *username;
@property(nonatomic, retain) NSString *password;
@property(nonatomic, retain) NSString *twitterIpAddr;

- (id)initWithIpAddress:(NSString *)_twitterIpAddr;
+ (void)sendTwitterStatus:(NSString *)message onIpAddr:(NSString *)twitterIpAddr;

-(IBAction)inputTextDone:(id)sender;

@end
