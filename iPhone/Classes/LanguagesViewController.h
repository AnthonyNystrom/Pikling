//
//  LanguagesViewController.h
//  yoGomee
//
//  Created by Alex on 21/04/09.
//  Copyright 2009 7touch Group. All rights reserved.
//

#import <UIKit/UIKit.h>


@protocol LanguagesViewControllerDelegate;

@interface LanguagesViewController : UIViewController <UITableViewDelegate, UITableViewDataSource> {
	id <LanguagesViewControllerDelegate> delegate;

	NSArray *listData;
	UITableView *languageTableView;

	NSString *languageSelected;
	int tag;
	BOOL showCancel;
}

@property (nonatomic, assign) id <LanguagesViewControllerDelegate> delegate;
@property (nonatomic, retain) NSArray *listData;
@property (nonatomic, retain) NSString *languageSelected;

- (id)initWithLanguage:(NSString *)_language withTag:(int)_tag withNavigationTitle:(NSString *)navTitle;
- (void)showCancelButton;

@end


@protocol LanguagesViewControllerDelegate
- (void)languageDidSelected:(NSString *)abbreviation forTag:(int)tag;
@end
