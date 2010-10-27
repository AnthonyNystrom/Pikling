//
//  FlipsideViewController.h
//  Pikling
//
//  Created by Alex on 23/04/09.
//  Copyright 7touch Group 2009. All rights reserved.
//

#import "LanguagesViewController.h"

#import <AddressBook/AddressBook.h>
#import <AddressBookUI/AddressBookUI.h>

@protocol FlipsideViewControllerDelegate;

@interface FlipsideViewController : UIViewController <UITableViewDelegate, UITableViewDataSource, LanguagesViewControllerDelegate, ABPeoplePickerNavigationControllerDelegate, UITextFieldDelegate> {
	id <FlipsideViewControllerDelegate> delegate;
	
	IBOutlet UITableView *tableView;
	
	// Variabili setting da spostare nel main
	UISwitch *switchAutoManCtl;
	UISwitch *switchCropCtl;

	UILabel *langIn;
	UILabel *langOut;
	UIImageView *flagIn;
	UIImageView *flagOut;
	UITextField *emailAddr;
	
	UISlider *sliderSounds;
	UISwitch *switchSoundsCtl;
	
	UILabel *statoTwitter;
	
	CGFloat shiftTabella;
	bool aggiornaTabella;
}

@property (nonatomic, assign) id <FlipsideViewControllerDelegate> delegate;
@property (nonatomic, retain) UITableView *tableView;
@property (nonatomic, copy) UITextField *emailAddr;

- (IBAction)done:(id)sender;
- (IBAction)showAddressBook:(id)sender;

- (void)switchSoundsCtlAction:(id)sender;

@end


@protocol FlipsideViewControllerDelegate
- (void)flipsideViewControllerDidFinish:(FlipsideViewController *)controller;
@end

