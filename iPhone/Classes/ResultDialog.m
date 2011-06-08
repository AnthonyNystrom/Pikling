//
//  ResultDialog.m
//  Pikling
//
//  Created by acerone on 4/27/09.
//  Copyright 2009 7touch Group. All rights reserved.
//

#import "ResultDialog.h"

#import "pikAppDelegate.h"

#import "TextAlertView.h"
#import"MyViewController.h"

#define kNumberOfPages 3

#define kOpenInSearchEngines 1
#define kAskEmailDestination 2
#define kAskDestination 3

@implementation ResultDialog

@synthesize scrollView, pageControl, viewControllers;

/*
 // The designated initializer.  Override if you create the controller programmatically and want to perform customization that is not appropriate for viewDidLoad.
- (id)initWithNibName:(NSString *)nibNameOrNil bundle:(NSBundle *)nibBundleOrNil {
    if (self = [super initWithNibName:nibNameOrNil bundle:nibBundleOrNil]) {
        // Custom initialization
    }
    return self;
}
*/

// Implement viewDidLoad to do additional setup after loading the view, typically from a nib.
- (void)viewDidLoad {
	NSMutableArray *controllers = [[NSMutableArray alloc] init];
	for (unsigned i = 0; i < kNumberOfPages; i++) {
		[controllers addObject:[NSNull null]];
	}
	self.viewControllers = controllers;
	[controllers release];
	 
	// a page is the width of the scroll view
	scrollView.pagingEnabled = YES;
	scrollView.contentSize = CGSizeMake(scrollView.frame.size.width * kNumberOfPages, 312.0);
	scrollView.showsHorizontalScrollIndicator = NO;
	scrollView.showsVerticalScrollIndicator = NO;
	scrollView.scrollsToTop = NO;
	scrollView.delegate = self;
	 
	pageControl.numberOfPages = kNumberOfPages;
	pageControl.currentPage = 0;
	 
	[self loadScrollViewWithPage:0];
	[self loadScrollViewWithPage:1];
	
	[super viewDidLoad];
}


- (void)didReceiveMemoryWarning {
	// Releases the view if it doesn't have a superview.
	[super didReceiveMemoryWarning];
	
	// Release any cached data, images, etc that aren't in use.
}

- (void)viewDidUnload {
	// Release any retained subviews of the main view.
	// e.g. self.myOutlet = nil;
}


- (void)dealloc {
	[imageView release];
	[scrollView release];
	[pageControl release];
	[viewControllers release];
	[loadingView release];
	[emailAddr  release];
	[typeOfFile release];

    [super dealloc];
}


#pragma mark -
#pragma mark Gestiore della rotella di avanzamento processo

- (void)startProgressAnimationWithString:(NSString *)testo
{
	// Disegno a video la rotellina
	loadingView = [[UIView alloc] initWithFrame:CGRectMake(0.0, 0.0, 320.0, 460.0)];
	[loadingView setBackgroundColor:[UIColor colorWithRed:0.00 green:0.00 blue:0.00 alpha:0.70]];
	
	UIActivityIndicatorView *activityView = [[UIActivityIndicatorView alloc] initWithFrame:CGRectMake(152, 156, 16, 16)];
	UILabel *loadingLabel = [[UILabel alloc] initWithFrame:CGRectMake(10, 178, 300, 16)];
	
	[loadingLabel setBackgroundColor:[UIColor clearColor]];
	[loadingLabel setFont:[UIFont systemFontOfSize:14]];
	[loadingLabel setText:testo];
	[loadingLabel setTextColor:[UIColor whiteColor]];
	[loadingLabel setTextAlignment:UITextAlignmentCenter];
	[loadingLabel setShadowColor:[UIColor clearColor]];
	[loadingView addSubview:loadingLabel];
	[loadingLabel release];
	
	[activityView setActivityIndicatorViewStyle:UIActivityIndicatorViewStyleWhite];
	[activityView startAnimating];
	[loadingView addSubview:activityView];
	[activityView release];
	
	[self.view addSubview:loadingView];
}	

- (void)stopProgressAnimation
{
	// Chiudo la rotellina
	if ((loadingView != nil) && ( loadingView.hidden == NO )) [loadingView setHidden:YES];
}


#pragma mark -
#pragma mark Segnali legati ai bottoni

/*
 Chiudo la finestra
 */
- (IBAction)done {
	[self dismissModalViewControllerAnimated:YES];
}

/*
 Inizia la procedura di richiesta invio email
 */
- (IBAction)sendDocTo {
	typeOfFile = [[NSString alloc] initWithString:@"DOC"];

	// Verifico guardando i setting se devo mandare subito la richiesta o devo chiedere l'indirizzo del destinatario
	if ([[NSUserDefaults standardUserDefaults] boolForKey:@"switchAutoManCtl"]) {
		// Chiedo l'indirizzo di destinazione all'utente
		[self askEmailDestination];
	} else {
		emailAddr = [[NSString alloc] initWithString:[[NSUserDefaults standardUserDefaults] objectForKey:@"emailAddress"]];

		// Spedisco direttamente alla mail impostata nei setting
		[self performEmailRequestTo:emailAddr];
	}
}
- (IBAction)sendPdfTo {
	typeOfFile = [[NSString alloc] initWithString:@"PDF"];
	
	// Verifico guardando i setting se devo mandare subito la richiesta o devo chiedere l'indirizzo del destinatario
	if ([[NSUserDefaults standardUserDefaults] boolForKey:@"switchAutoManCtl"]) {
		// Chiedo l'indirizzo di destinazione all'utente
		[self askEmailDestination];
	} else {
		emailAddr = [[NSString alloc] initWithString:[[NSUserDefaults standardUserDefaults] objectForKey:@"emailAddress"]];
		
		// Spedisco direttamente alla mail impostata nei setting
		[self performEmailRequestTo:emailAddr];
	}
}

- (IBAction)sendAllTo {
	typeOfFile = [[NSString alloc] initWithString:@"ALL"];
	
	// Verifico guardando i setting se devo mandare subito la richiesta o devo chiedere l'indirizzo del destinatario
	if ([[NSUserDefaults standardUserDefaults] boolForKey:@"switchAutoManCtl"]) {
		// Chiedo l'indirizzo di destinazione all'utente
		[self askEmailDestination];
	} else {
		emailAddr = [[NSString alloc] initWithString:[[NSUserDefaults standardUserDefaults] objectForKey:@"emailAddress"]];
		
		// Spedisco direttamente alla mail impostata nei setting
		[self performEmailRequestTo:emailAddr];
	}
}

/*
 Esegue l'invio vero e proprio del pacchetto di richiesta al server
 */
- (void)performEmailRequestTo:(NSString *)_emailAddr
{
	if  (([_emailAddr length] > 2) && ( [_emailAddr rangeOfString:@"@"].location != NSNotFound )) {
		[self startProgressAnimationWithString:NSLocalizedString(@"sending request...",@"")];

		// Invio i dati al server
		WebServices *send = [[WebServices alloc] init];
		send.delegate=self;
		[send sendRequestoToServerFor:typeOfFile to:_emailAddr];

		[send release];
	} else {
		UIAlertView *alert = [[UIAlertView alloc] initWithTitle:NSLocalizedString(@"Error!",@"")
														message:NSLocalizedString(@"Invalid email address", @"")
													   delegate:nil
											  cancelButtonTitle:NSLocalizedString(@"Ok",@"")
											  otherButtonTitles:nil];                                                                                                                        
		[alert show];                                                                                                                        
		[alert release];    
	}
}


#pragma mark -
#pragma mark Salvataggio immagine in libreria
- (void)image:(UIImage *)image didFinishSavingWithError:(NSError *)error contextInfo:(void *)contextInfo
{
	// Chiudo la rotellina
	[self stopProgressAnimation];
}

- (IBAction)saveImageOnLibrary:(id)sender {
	[self startProgressAnimationWithString:NSLocalizedString(@"saving image...",@"")];
	
	pikAppDelegate *delegate=  [UIApplication sharedApplication].delegate;
	NSDictionary *_jobInfo = [delegate jobInfo];
	UIImage *immag =[[UIImage alloc] initWithData:[_jobInfo objectForKey:@"image"]];
	UIImageWriteToSavedPhotosAlbum(immag, self, @selector(image:didFinishSavingWithError:contextInfo:), nil );
//	UIImageWriteToSavedPhotosAlbum(immag, nil, @selector(image:didFinishSavingWithError:contextInfo:), nil );
	[immag release];
}


#pragma mark -
#pragma mark Visualizzazione rubrica utente
- (IBAction)showAddressBook:(id)sender {
	ABPeoplePickerNavigationController *addressBook =[[ABPeoplePickerNavigationController alloc] init];
	addressBook.peoplePickerDelegate = self;
	NSNumber* emailProp = [NSNumber numberWithInt:kABPersonEmailProperty];
	addressBook.displayedProperties = [NSArray arrayWithObject:emailProp];
	addressBook.modalTransitionStyle =  UIModalTransitionStyleFlipHorizontal;
	[self presentModalViewController:addressBook animated:YES];
	[addressBook release];
}

#pragma mark -
#pragma mark Scrolling Management

- (void)loadScrollViewWithPage:(int)page {
    if (page < 0) return;
    if (page >= kNumberOfPages) return;

    MyViewController *controller = [viewControllers objectAtIndex:page];
    if ((NSNull *)controller == [NSNull null]) {
        controller = [[MyViewController alloc] initWithPageNumber:page];
        [viewControllers replaceObjectAtIndex:page withObject:controller];
        [controller release];
    }
	
    if (nil == controller.view.superview) {
        CGRect frame = scrollView.frame;
        frame.origin.x = frame.size.width * page;
        frame.origin.y = 0;
        controller.view.frame = frame;
        [scrollView addSubview:controller.view];
    }
}

- (void)scrollViewDidScroll:(UIScrollView *)sender {
    if (pageControlUsed) {
        return;
    }
    CGFloat pageWidth = scrollView.frame.size.width;
    int page = floor((scrollView.contentOffset.x - pageWidth / 2) / pageWidth) + 1;
    pageControl.currentPage = page;
	
    [self loadScrollViewWithPage:page - 1];
    [self loadScrollViewWithPage:page];
    [self loadScrollViewWithPage:page + 1];
}

- (void)scrollViewDidEndDecelerating:(UIScrollView *)scrollView {
    pageControlUsed = NO;
}

- (IBAction)changePage:(id)sender {
    int page = pageControl.currentPage;

    [self loadScrollViewWithPage:page - 1];
    [self loadScrollViewWithPage:page];
    [self loadScrollViewWithPage:page + 1];

    //  Aggiorno la scrollview
    CGRect frame = scrollView.frame;
    frame.origin.x = frame.size.width * page;
    frame.origin.y = 0;
    [scrollView scrollRectToVisible:frame animated:YES];

    pageControlUsed = YES;
}


#pragma mark - Action sheet per decidere il tipo di motore di ricerca
- (IBAction)openInSearchEngines
{
	// Arpo lo sheet con la scelta tra due possibili motori di ricerca
	UIActionSheet *actionSheet = [[UIActionSheet alloc] initWithTitle:NSLocalizedString(@"Search engines...",@"")
			delegate:self cancelButtonTitle:nil destructiveButtonTitle:nil
			otherButtonTitles:@"Google", @"Wikipedia", NSLocalizedString(@"Cancel",@""), nil];
	actionSheet.tag = kOpenInSearchEngines;
	actionSheet.actionSheetStyle = UIActionSheetStyleDefault;
	actionSheet.destructiveButtonIndex = 2;
	[actionSheet showInView:self.view];
	[actionSheet release];
}


#pragma mark - Action sheet per decidere se prendere l'indirizzo email dalla rubrica o a mano
- (IBAction)askEmailDestination
{
	UIActionSheet *actionSheet = [[UIActionSheet alloc] initWithTitle:NSLocalizedString(@"Send this attachement...",@"")
															 delegate:self cancelButtonTitle:nil destructiveButtonTitle:nil
													otherButtonTitles:NSLocalizedString(@"From address book",@""), NSLocalizedString(@"Manual add",@""), NSLocalizedString(@"Cancel",@""), nil];
	actionSheet.tag = kAskEmailDestination;
	actionSheet.actionSheetStyle = UIActionSheetStyleDefault;
	actionSheet.destructiveButtonIndex = 2;
	[actionSheet showInView:self.view];
	[actionSheet release];
}


#pragma mark -
#pragma mark WebServices Delegate
- (void)streamEventErrorOccurred:(NSString *)errorString;
{
	// Chiudo la rotellina
	[self stopProgressAnimation];

	UIAlertView *alert = [[UIAlertView alloc] initWithTitle:NSLocalizedString(@"Communication error!",@"")
					message:[NSString stringWithFormat:errorString] 
					   delegate:self 
				  cancelButtonTitle:NSLocalizedString(@"Ok",@"")
				  otherButtonTitles:nil];                                                                                                                        
	[alert show];                                                                                                                        
	[alert release];    
}

- (void)WebServicesDidFinish
{
   	// Chiudo la rotellina
	[self stopProgressAnimation];
	
	UIAlertView *alert = [[UIAlertView alloc] initWithTitle:NSLocalizedString(@"Done",@"")
					message:[NSString stringWithFormat:NSLocalizedString(@"Email sent to %@!",@""),emailAddr]
					   delegate:nil 
				  cancelButtonTitle:NSLocalizedString(@"Ok",@"")
				  otherButtonTitles:nil];
	[alert show];
	[alert release];
}


//
// Aggiorno la progress bar di avanzamento upload
//
- (void)uploadPercent:(int)value
{
}

//
// Finito l'upload dell'immagine ora inizia la fase di OCR
//
- (void)uploadImageDidFinish
{
}

#pragma mark -
#pragma mark - AddressBook Deleagte

- (void)peoplePickerNavigationControllerDidCancel:(ABPeoplePickerNavigationController *)peoplePicker {
	[self dismissModalViewControllerAnimated:YES];	

	emailAddr=@"";
}

- (BOOL)peoplePickerNavigationController: (ABPeoplePickerNavigationController *)peoplePicker shouldContinueAfterSelectingPerson:(ABRecordRef)person 
{
	return YES;
}

- (BOOL)peoplePickerNavigationController:(ABPeoplePickerNavigationController *)peoplePicker shouldContinueAfterSelectingPerson:(ABRecordRef)person property:(ABPropertyID)property identifier:(ABMultiValueIdentifier)identifier{
	// Chiudo il dialog
	[self dismissModalViewControllerAnimated:YES];
	
	// Ricavo la email
	ABMultiValueRef emails = ABRecordCopyValue(person, kABPersonEmailProperty);
	CFStringRef email = ABMultiValueCopyValueAtIndex(emails, identifier);
	emailAddr=[[NSString alloc] initWithString:(NSString *) email];

	// Invio al web service la richiesta (un pò ritardata per fare chiudere lo sheet che l'ha chiamata)
	[self performSelector:@selector(performEmailRequestTo:) withObject:emailAddr afterDelay:0.2]; 
	
	return NO;	
}


#pragma mark - UIActionSheetDelegate
- (void)actionSheet:(UIActionSheet *)actionSheet clickedButtonAtIndex:(NSInteger)buttonIndex
{
	// Action per i motori di ricerca
	if (actionSheet.tag == kOpenInSearchEngines) {
		// Cancel
		if (buttonIndex == 2) return;
		
		pikAppDelegate *delegate=  [UIApplication sharedApplication].delegate;
		NSDictionary *_jobInfo = [delegate jobInfo];
		NSString *testo = [[[NSString alloc] initWithString:[_jobInfo objectForKey:@"DestinationResult"]] autorelease];
		
		NSString *link;
		if (buttonIndex == 0) {
			link = [[[NSString alloc] initWithFormat: @"http://www.google.com/search?q=%@&ie=UTF-8&oe=UTF-8&client=safari",[testo stringByAddingPercentEscapesUsingEncoding:NSUTF8StringEncoding]] autorelease];
		} else {
			link = [[[[NSString alloc] initWithFormat:@"http://mobile.wikipedia.org/transcode.php?go=\"%@\"",testo] stringByAddingPercentEscapesUsingEncoding:NSUTF8StringEncoding] autorelease];
		}
		[[UIApplication sharedApplication] openURL:[NSURL URLWithString:link]];
	}
	
	// Action per indirizzo email di destinazione
	if (actionSheet.tag == kAskEmailDestination) {
		// Cancel
		if (buttonIndex == 2) return;
		
		if (buttonIndex == 0) {
			// Mostro la rubrica
			[self showAddressBook:self];
		} else {
			// Invio al web service la richiesta (un pò ritardata per fare chiudere lo sheet che l'ha chiamata)
			[self performSelector:@selector(askDestination:) withObject:self afterDelay:0.3]; 
		}
	}
}


#pragma mark - Alert view per inserimento indirizzo email manuale
- (IBAction)askDestination:(id)sender {
/*
	UITextField *textField;
	UIAlertView *myAlert = [[UIAlertView alloc] initWithTitle:NSLocalizedString( @"Send email to", @"") 
													  message:@""
													 delegate:self cancelButtonTitle:NSLocalizedString(@"Cancel", @"") 
											otherButtonTitles:NSLocalizedString( @"Ok",@"") , nil];
	[myAlert addTextFieldWithValue:@"" label:@"email address"];
	textField = [myAlert textFieldAtIndex:0];
	textField.clearButtonMode = UITextFieldViewModeWhileEditing;
	textField.keyboardType = UIKeyboardTypeEmailAddress;
	textField.keyboardAppearance = UIKeyboardAppearanceAlert;
	textField.autocapitalizationType  = UITextAutocapitalizationTypeWords;
	textField.autocorrectionType = UITextAutocorrectionTypeNo;
	textField.textAlignment = UITextAlignmentCenter;
	textField.text = emailAddr;
	myAlert.tag = kAskDestination;
	[myAlert show];
	[myAlert release];
 */
	// Ask for Username and password.
	UIAlertView *myAlert = [[UIAlertView alloc] initWithTitle:NSLocalizedString( @"Send email to", @"") 
														message:@"\n \n" 
													   delegate:self 
											  cancelButtonTitle:NSLocalizedString(@"Cancel", @"")
											  otherButtonTitles:NSLocalizedString( @"Ok",@""), nil];
	// Adds a username Field
	emailTextField = [[UITextField alloc] initWithFrame:CGRectMake(12.0, 45.0, 260.0, 25.0)]; 
	emailTextField.placeholder = NSLocalizedString(@"email address", @"");
	emailTextField.backgroundColor=[UIColor whiteColor]; 
	emailTextField.clearButtonMode = UITextFieldViewModeWhileEditing;
	emailTextField.keyboardType = UIKeyboardTypeEmailAddress;
	emailTextField.keyboardAppearance = UIKeyboardAppearanceAlert;
	emailTextField.autocapitalizationType  = UITextAutocapitalizationTypeWords;
	emailTextField.autocorrectionType = UITextAutocorrectionTypeNo;
	emailTextField.textAlignment = UITextAlignmentCenter;
	emailTextField.text = emailAddr;
	[myAlert addSubview:emailTextField];
	myAlert.tag = kAskDestination;
	[myAlert show];
	[myAlert release];	
}

#pragma mark - Delegato dell'Alert view per l'inserimento dell'indirizzo email
- (void)alertView:(UIAlertView *)actionSheet clickedButtonAtIndex:(NSInteger)buttonIndex {
	TextAlertView *alertView = (TextAlertView*) actionSheet;
	// Richiesta
	if (alertView.tag == kAskDestination) {
		if(buttonIndex == 1) {
			emailAddr = [[NSString alloc] initWithString:emailTextField.text];
			if(emailAddr==nil) return; //

			// Invio la richiesta di email
			// Invio al web service la richiesta (un pò ritardata per fare chiudere lo sheet che l'ha chiamata)
			[self performSelector:@selector(performEmailRequestTo:) withObject:emailAddr afterDelay:0.2]; 
			[emailAddr release];
		}
	}
	[emailTextField release];
}

@end
