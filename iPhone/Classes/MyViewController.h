
#import <UIKit/UIKit.h>


@interface MyViewController : UIViewController {
	IBOutlet UILabel *textLabel;
	IBOutlet UILabel *titolo;
	int pageNumber;
}

@property (nonatomic, retain) UILabel *textLabel;
@property (nonatomic, retain) UILabel *titolo;

- (id)initWithPageNumber:(int)page;
- (void)visit7Touch:(id)sender;
- (BOOL)calculateFontSizeForLabel:(UILabel*)_label;

@end
