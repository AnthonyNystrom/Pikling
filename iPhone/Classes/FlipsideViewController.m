//
//  FlipsideViewController.m
//  Pikling
//
//  Created by Alex on 23/04/09.
//  Copyright 7touch Group 2009. All rights reserved.
//

#import "FlipsideViewController.h"
#import <AddressBook/AddressBook.h>

#import "pikAppDelegate.h"
#import "LanguagesViewController.h"

//#import "TwitterSettingsViewController.h"
#import "SA_OAuthTwitterEngine.h"
#import "SA_OAuthTwitterController.h"

// Questo gadget al momento non lo faccio ancora vedere
#undef SHOW_CROP_SWITCH

@implementation FlipsideViewController

@synthesize tableView, delegate;

/*
- (void)loadView {
    SettingViewController *settingViewController = [[SettingViewController alloc] initWithTitle:NSLocalizedString(@"Setting",@"") withNavigationTitle:NSLocalizedString(@"Setting",@"")];
    UINavigationController *settingNavController = [[UINavigationController alloc] initWithRootViewController:settingViewController];
    settingNavController.navigationBar.tintColor = [UIColor colorWithRed:0.14 green:0.18 blue:0.25 alpha:1.00];

	// Configure and show the window
	[window addSubview:tbController.view];
}
*/

- (void)viewWillAppear:(BOOL)animated
{
	if (aggiornaTabella) {
		aggiornaTabella=NO;
		[tableView reloadData];
	}
	
	langIn.text=[[NSUserDefaults standardUserDefaults] objectForKey:@"langIn"];
	flagIn.image = [UIImage imageWithContentsOfFile:[[NSBundle mainBundle] pathForResource:langIn.text ofType:@"png"]];
	langOut.text=[[NSUserDefaults standardUserDefaults] objectForKey:@"langOut"];
	flagOut.image = [UIImage imageWithContentsOfFile:[[NSBundle mainBundle] pathForResource:langOut.text ofType:@"png"]];

}


- (void)viewWillDisappear:(BOOL)animated
{
	// Nascondo la tastiera se aperta
	if (shiftTabella > 0) {
		[self inputTextDone:emailAddr];
	}
}


- (void)viewDidLoad {
	aggiornaTabella=NO;

	// Sistemo la navigation bar
	self.navigationItem.title = NSLocalizedString(@"Settings",@"");
	UIBarButtonItem *doneButton = [[UIBarButtonItem alloc] initWithTitle:NSLocalizedString(@"Done",@"") style:UIBarButtonItemStyleBordered  target:self action:@selector(done:)];
	self.navigationItem.rightBarButtonItem=doneButton;
		
	[tableView setBackgroundColor:[[UIColor alloc] initWithPatternImage:[UIImage imageWithContentsOfFile:[[NSBundle mainBundle] pathForResource:@"setting_pattern" ofType:@"png"]]]];
	
	// Assegno ai valori della tabella quelli letti dai defaults
	// Lingua sorgente
	langIn = [[UILabel alloc] initWithFrame:CGRectMake(170.0, 10.0, 50.0, 28.0)];
	langIn.textAlignment=UITextAlignmentRight;
	langIn.textColor=[UIColor colorWithRed:0.27 green:0.38 blue:0.54 alpha:1.0];
	langIn.text=[[NSUserDefaults standardUserDefaults] objectForKey:@"langIn"];
	
	flagIn= [[UIImageView alloc] initWithFrame:CGRectMake(225.0, 2.0, 48.0, 48.0) ];
	
	// Lingua di destinazione
	langOut = [[UILabel alloc] initWithFrame: CGRectMake(170.0, 10.0, 50.0, 28.0)];
	langOut.textAlignment=UITextAlignmentRight;
	langOut.textColor=[UIColor colorWithRed:0.27 green:0.38 blue:0.54 alpha:1.0];
	langOut.text=[[NSUserDefaults standardUserDefaults] objectForKey:@"langOut"];

	flagOut= [[UIImageView alloc] initWithFrame:CGRectMake(225.0, 2.0, 48.0, 48.0) ];

	// Selettore di "ask before send"
	switchAutoManCtl = [[UISwitch alloc] initWithFrame:CGRectMake(200.0, 8.0, 45.0, 20.0)];
	[switchAutoManCtl setOn:[[NSUserDefaults standardUserDefaults] boolForKey:@"switchAutoManCtl"]];
	
	// Indirizzo email
	// Campo per l'inserimento del valore
	emailAddr = [[[UITextField alloc] initWithFrame:CGRectMake(80, 12, 175, 25)] autorelease];
	emailAddr.textAlignment=UITextAlignmentRight;
	emailAddr.clearsOnBeginEditing=NO;
	emailAddr.delegate=self;
	[emailAddr addTarget:self action:@selector(inputTextDone:) forControlEvents:UIControlEventEditingDidEndOnExit];
	emailAddr.clearButtonMode=UITextFieldViewModeWhileEditing;
	emailAddr.autocorrectionType=UITextAutocorrectionTypeNo;
	emailAddr.autocapitalizationType=UITextAutocapitalizationTypeNone;
	emailAddr.keyboardType=UIKeyboardTypeEmailAddress;
	emailAddr.clearsOnBeginEditing=NO;
	emailAddr.font=[UIFont systemFontOfSize:18];
	[emailAddr setTextColor:[UIColor colorWithRed:0.27 green:0.38 blue:0.54 alpha:1.0]];
	emailAddr.backgroundColor=[UIColor clearColor];
	
	emailAddr.tag=1;
	emailAddr.placeholder=NSLocalizedString(@"email address",@"");
	emailAddr.text=[[NSUserDefaults standardUserDefaults] objectForKey:@"emailAddress"];
	

	// Slider relativo alla selezione del volume dei suoni
	sliderSounds = [[UISlider alloc] initWithFrame:CGRectMake(5.0, 5.0, 285.0, 35.0)];
	sliderSounds.backgroundColor = [UIColor clearColor];
	sliderSounds.minimumValue = 0.0;
	sliderSounds.maximumValue = 9.0;
	sliderSounds.continuous = NO;
	sliderSounds.minimumValueImage = [UIImage imageNamed:@"LowVol.png"];
	sliderSounds.maximumValueImage = [UIImage imageNamed:@"HighVol.png"];
	sliderSounds.value = [[[NSUserDefaults standardUserDefaults] objectForKey:@"volumeSounds"] floatValue];
	[sliderSounds addTarget:self action:@selector(testSoundAction:) forControlEvents:UIControlEventValueChanged];

	// Selettore di "effetti sonori"
	switchSoundsCtl = [[UISwitch alloc] initWithFrame:CGRectMake(200.0, 8.0, 45.0, 20.0)];
	[switchSoundsCtl addTarget:self action:@selector(switchSoundsCtlAction:) forControlEvents:UIControlEventValueChanged];
	[switchSoundsCtl setOn:[[NSUserDefaults standardUserDefaults] boolForKey:@"switchSoundsCtl"]];

	// Aggiorno lo stato dello slider del volume
	[self switchSoundsCtlAction:nil];
	
	
	// Selettore di "crop before request"
	switchCropCtl = [[UISwitch alloc] initWithFrame:CGRectMake(200.0, 8.0, 45.0, 20.0)];
	[switchCropCtl setOn:[[NSUserDefaults standardUserDefaults] boolForKey:@"switchCropCtl"]];
#ifndef SHOW_CROP_SWITCH
	// Senza il define di default permetto di tagliare l'immagine catturata
	[switchCropCtl setOn:YES];
#endif	

	[super viewDidLoad];
}


//
// Chiudo il dialogo e salvo le impostazioni nei defaults
//
- (IBAction)done:(id)sender {
	if ( ![self checkDoneButton] ) {
		UIAlertView *alert = [[UIAlertView alloc] initWithTitle:NSLocalizedString(@"Attention",@"")
														message:NSLocalizedString(@"You must insert a valid email address!",@"")
													   delegate:nil
											  cancelButtonTitle:NSLocalizedString(@"Ok",@"")
											  otherButtonTitles:nil];                                                                                                                        
		[alert show];                                                                                                                        
		[alert release];    

		pikAppDelegate *_delegate=  [UIApplication sharedApplication].delegate;
		if ( [[NSUserDefaults standardUserDefaults] boolForKey:@"switchSoundsCtl"] ) [_delegate playSound:@"/Pikling Bad 3 -8db.mp3" withDelta:-0.05f];

		return;
	}
	
	// Verifico che siano stati selezionati lingua sorg/dest altrimenti li impongo io (en/it)
	if ( [langIn.text length] != 2 ) langIn.text=@"en";
	if ( [langOut.text length] != 2 ) langIn.text=@"es"; 
	
	// Salvo i settaggi
	[[NSUserDefaults standardUserDefaults] setBool:switchAutoManCtl.on forKey:@"switchAutoManCtl"];
#ifdef SHOW_CROP_SWITCH
	[[NSUserDefaults standardUserDefaults] setBool:switchCropCtl.on forKey:@"switchCropCtl"];
#else 
	[[NSUserDefaults standardUserDefaults] setBool:YES forKey:@"switchCropCtl"];
#endif
	[[NSUserDefaults standardUserDefaults] setObject:langIn.text forKey:@"langIn"];
	[[NSUserDefaults standardUserDefaults] setObject:langOut.text forKey:@"langOut"];
	[[NSUserDefaults standardUserDefaults] setObject:emailAddr.text forKey:@"emailAddress"];
	
	// Variabili legate ai suoni
	NSString *sliderVal = [[[NSString alloc]initWithFormat:@"%f", sliderSounds.value] autorelease];
	[[NSUserDefaults standardUserDefaults] setObject:sliderVal forKey:@"volumeSounds"];
	[[NSUserDefaults standardUserDefaults] setBool:switchSoundsCtl.on forKey:@"switchSoundsCtl"];

	// Salvo i dati
	[[NSUserDefaults standardUserDefaults] synchronize];

	// Chiudo il dialogo
	[self dismissModalViewControllerAnimated:YES];
	
	pikAppDelegate *_delegate=  [UIApplication sharedApplication].delegate;
	if ( [[NSUserDefaults standardUserDefaults] boolForKey:@"switchSoundsCtl"] )  [_delegate playSound:@"/Pikling Tap 2 -8db.mp3" withDelta:0.0f];
}

//
// Apro il dialogo con le voci della rubrica (solo quelle che contengono indirizzi email)
//
- (IBAction)showAddressBook:(id)sender {
	ABPeoplePickerNavigationController *addressBook =[[ABPeoplePickerNavigationController alloc] init];
	addressBook.peoplePickerDelegate = self;
	NSNumber* emailProp = [NSNumber numberWithInt:kABPersonEmailProperty];
	addressBook.displayedProperties = [NSArray arrayWithObject:emailProp];
	addressBook.modalTransitionStyle =  UIModalTransitionStyleFlipHorizontal;
	[self presentModalViewController:addressBook animated:YES];
	[addressBook release];
}


/*
 // Override to allow orientations other than the default portrait orientation.
 - (BOOL)shouldAutorotateToInterfaceOrientation:(UIInterfaceOrientation)interfaceOrientation {
 // Return YES for supported orientations
 return (interfaceOrientation == UIInterfaceOrientationPortrait);
 }
*/

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
	[tableView release];
	[switchAutoManCtl release];
	[langIn release];
	[langOut release];
	[flagIn release];
	[flagOut release];
	[emailAddr release];
	[sliderSounds release];

    [super dealloc];
}


#pragma mark -
#pragma mark Chiamata a twitter

#define kOAuthConsumerKey			@"7kvogUtgrqC85dwzO68Ayg"
#define kOAuthConsumerSecret		@"rIZXOhnX4NctCH396ZU132IT1574Tnf7LyGQp1DFKA"

- (IBAction)showTwitterSettings 
{
	SA_OAuthTwitterEngine *_engine = [[SA_OAuthTwitterEngine alloc] initOAuthWithDelegate: self];
	_engine.consumerKey = kOAuthConsumerKey;
	_engine.consumerSecret = kOAuthConsumerSecret;
	
	if( ! [_engine isAuthorized] ){  
		UIViewController *controller = [SA_OAuthTwitterController controllerToEnterCredentialsWithTwitterEngine:_engine delegate:self];
		[self presentModalViewController:controller animated:YES];
	} else {
 		[[NSUserDefaults standardUserDefaults] removeObjectForKey:@"authData"];
		[[NSUserDefaults standardUserDefaults] synchronize];
	}
	[_engine release];
	
	// Aggiorno i dati della cella della tabella
	NSArray *indexPaths = [NSArray arrayWithObjects:[NSIndexPath indexPathForRow:0 inSection:2], nil]; 
	[tableView reloadRowsAtIndexPaths:indexPaths withRowAnimation:UITableViewRowAnimationFade];
}


#pragma mark -
#pragma mark Delegati per le tabella

- (UITableViewCell *)tableView:(UITableView *)_tableView cellForRowAtIndexPath:(NSIndexPath *)indexPath
{
	UITableViewCell *cell=nil;

	switch ([indexPath section]) {
		case 0: // Sezione lingue
			switch ([indexPath row]) {
				case 0:
					cell = [_tableView dequeueReusableCellWithIdentifier:@"langCell_0"];
					if (cell == nil) {
						cell=[[[UITableViewCell alloc] initWithStyle:UITableViewCellStyleDefault reuseIdentifier:@"langCell_0"] autorelease];
						// Flag draw
//						flagIn= [[UIImageView alloc] initWithFrame:CGRectMake(225.0, 2.0, 48.0, 48.0) ];
//						flagIn.image = [UIImage imageWithContentsOfFile:[[NSBundle mainBundle] pathForResource:langIn.text ofType:@"png"]];

						cell.textLabel.text = NSLocalizedString(@"Translate from",@"");
						[cell.contentView addSubview:flagIn];
						[cell.contentView addSubview:langIn];
					}
//					cell.textLabel.text = NSLocalizedString(@"Translate from",@"");
					// Metto la freccetta
					[cell setSelectionStyle:UITableViewCellSelectionStyleBlue];
					[cell setAccessoryType:UITableViewCellAccessoryDisclosureIndicator];

					break;

				case 1:
					cell = [_tableView dequeueReusableCellWithIdentifier:@"langCell_1"];
					if (cell == nil) {
						cell=[[[UITableViewCell alloc] initWithStyle:UITableViewCellStyleDefault reuseIdentifier:@"langCell_1"] autorelease];
						// Flag draw
//						flagOut= [[UIImageView alloc] initWithFrame:CGRectMake(225.0, 2.0, 48.0, 48.0) ];
//						flagOut.image = [UIImage imageWithContentsOfFile:[[NSBundle mainBundle] pathForResource:langOut.text ofType:@"png"]];

						cell.textLabel.text = NSLocalizedString(@"Translate to",@"");
						[cell.contentView addSubview:flagOut];
						[cell.contentView addSubview:langOut];
					}
					
					// Metto la freccetta
					[cell setSelectionStyle:UITableViewCellSelectionStyleBlue];
					[cell setAccessoryType:UITableViewCellAccessoryDisclosureIndicator];

					break;
			}
			
			break;

		case 1: // Sezione invio notifiche via email
			switch ([indexPath row]) {
				case 0:
					cell = [_tableView dequeueReusableCellWithIdentifier:@"askBeforeCell"];
					if (cell == nil) {
						cell=[[[UITableViewCell alloc] initWithStyle:UITableViewCellStyleDefault reuseIdentifier:@"askBeforeCell"] autorelease];

						cell.textLabel.text = NSLocalizedString(@"Ask before send",@"");
						switchAutoManCtl.backgroundColor = [UIColor clearColor];
						[cell.contentView addSubview:switchAutoManCtl];
					}

					[cell setSelectionStyle:UITableViewCellSelectionStyleNone ];
					[cell setAccessoryType:UITableViewCellAccessoryNone];
			
					break;
				case 1:
					cell = [_tableView dequeueReusableCellWithIdentifier:@"emailCell"];
					if (cell == nil) {
						cell=[[[UITableViewCell alloc] initWithStyle:UITableViewCellStyleDefault reuseIdentifier:@"emailCell"] autorelease];
												
						// Metto la freccetta
						[cell setSelectionStyle:UITableViewCellSelectionStyleNone ];
						[cell setAccessoryType:UITableViewCellAccessoryNone];
						
						// Icona address book
						UIButton *ABImage = [[UIButton alloc] initWithFrame:CGRectMake(260.0, 6.0, 32.0, 32.0) ];
						[ABImage setImage:[UIImage imageNamed:@"address_book.png"] forState:UIControlStateNormal];
						[ABImage addTarget:self action:@selector(showAddressBook:) forControlEvents:UIControlEventTouchUpInside];
						
						cell.textLabel.text = NSLocalizedString(@"Email",@"");

						[cell.contentView addSubview:emailAddr];
						[cell.contentView addSubview:ABImage];
					}

					break;
			}
			break;
		case 2: // Sezione twitter
			cell = [tableView dequeueReusableCellWithIdentifier:@"twitterCell"];
			if (cell == nil) {
				cell=[[[UITableViewCell alloc] initWithStyle:UITableViewCellStyleDefault reuseIdentifier:@"twitterCell"] autorelease];
			
				[cell.textLabel setText:@"Twitter"];
				[cell.imageView setImage: [UIImage imageNamed:@"twitter_small.png"]];
			
				statoTwitter = [[UILabel alloc] initWithFrame:CGRectMake(130, 12, 140, 32)];
				[statoTwitter setTextColor:[UIColor colorWithRed:0.27 green:0.38 blue:0.54 alpha:1.0]];
				[statoTwitter setFont:[UIFont systemFontOfSize:18]];
				[statoTwitter setTextAlignment:UITextAlignmentRight];
			}

			if ( [[NSUserDefaults standardUserDefaults] objectForKey:@"authData"] != nil ) [statoTwitter setText:NSLocalizedString(@"Enabled",@"")];
			else [statoTwitter setText:NSLocalizedString(@"Disabled",@"")];
			[cell.contentView addSubview:statoTwitter];
			
			// Metto la freccetta
			[cell setSelectionStyle:UITableViewCellSelectionStyleBlue];
			[cell setAccessoryType:UITableViewCellAccessoryDisclosureIndicator];
			break;

		case 3: // Image
			cell = [_tableView dequeueReusableCellWithIdentifier:@"cropCell"];
			if (cell == nil) {
				cell=[[[UITableViewCell alloc] initWithStyle:UITableViewCellStyleDefault reuseIdentifier:@"cropCell"] autorelease];
				cell.text = NSLocalizedString(@"Manual crop",@"");
				[cell setSelectionStyle:UITableViewCellSelectionStyleNone ];
				[cell setAccessoryType:UITableViewCellAccessoryNone];
				
				switchCropCtl.backgroundColor = [UIColor clearColor];
				[cell.contentView addSubview:switchCropCtl];
			}
			break;

		case 4: // Sezione audio/suoni
			switch ([indexPath row]) {
				case 0:
					cell = [_tableView dequeueReusableCellWithIdentifier:@"audioCell"];
					if (cell == nil) {
						cell=[[[UITableViewCell alloc] initWithStyle:UITableViewCellStyleDefault reuseIdentifier:@"audioCell"] autorelease];

						cell.textLabel.text = NSLocalizedString(@"Sounds effects",@"");
						
						switchSoundsCtl.backgroundColor = [UIColor clearColor];
						[cell.contentView addSubview:switchSoundsCtl];
					}
//					cell.textLabel.text = NSLocalizedString(@"Translate from",@"");
					// Metto la freccetta
					[cell setSelectionStyle:UITableViewCellSelectionStyleNone];
					[cell setAccessoryType:UITableViewCellAccessoryNone];

					break;
				case 1:
					cell = [_tableView dequeueReusableCellWithIdentifier:@"volumeCell"];
					if (cell == nil) {
						cell=[[[UITableViewCell alloc] initWithStyle:UITableViewCellStyleDefault reuseIdentifier:@"volumeCell"] autorelease];
						
						[cell setSelectionStyle:UITableViewCellSelectionStyleNone ];
						[cell setAccessoryType:UITableViewCellAccessoryNone];
						
						[cell.contentView addSubview:sliderSounds];
					}
					break;
			}
			break;
		default:
			break;
	}
		
	return cell;
}

- (NSString *)tableView:(UITableView *)tableView titleForHeaderInSection:(NSInteger)section
{
	switch (section) {
		case 0:
			return NSLocalizedString(@"Languages...",@"");
			break;
		case 1:
			return NSLocalizedString(@"Send results to...",@"");
			break;
//		case 3:
//			return NSLocalizedString(@"Image...",@"");
//			break;
//		case 4:
//			return NSLocalizedString(@"Sounds...",@"");
//			break;
		default:
			return @" ";
			break;
	}
}

- (NSInteger)tableView:(UITableView *)tableView numberOfRowsInSection:(NSInteger)section
{
	switch (section) {
		case 0: // Lingue
			return 2;
			break;
		case 1: // Send to
			return 2;
			break;
		case 2: // Twitter
			return 1;
			break;
		case 3: // Immagine
			return 1;
			break;
		case 4: // Sounds
			return 2;
			break;
		default:
			break;
	} 
	return 0;
}

- (void)tableView:(UITableView *)_tableView didSelectRowAtIndexPath:(NSIndexPath *)indexPath
{
	// Disevidenzio la cella che ho selezionato
	[_tableView deselectRowAtIndexPath:indexPath animated:NO];

	// Sezione 0 - Scelta lingue
	if ([indexPath section] == 0) {
	    // Source Language
	    if ( [indexPath row] == 0) {
			LanguagesViewController *controller = [[LanguagesViewController alloc] initWithLanguage:langIn.text withTag:kSourceLanguage withNavigationTitle:NSLocalizedString(@"Translate from",@"") ];
			controller.delegate=self;
			[self.navigationController pushViewController:controller animated:YES];
			[controller release];
	    }
	    // Destination Language
	    if ( [indexPath row] == 1) {
			LanguagesViewController *controller = [[LanguagesViewController alloc] initWithLanguage:langOut.text withTag:kDestinationLanguage withNavigationTitle:NSLocalizedString(@"Translate to",@"") ];
			controller.delegate=self;
			[self.navigationController pushViewController:controller animated:YES];
			[controller release];
	    }
	}
	if ([indexPath section] == 2) {
		[self showTwitterSettings];
		aggiornaTabella=YES;
	}
	
	return;	
}

- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView 
{
	return 5;
}

- (NSString *)tableView:(UITableView *)tableView titleForFooterInSection:(NSInteger)section 
{
//	if (section==3) return @"Enabling this option you can send result directly on your preferred email address";
	return @"";
}


- (CGFloat)tableView:(UITableView *)tableView heightForRowAtIndexPath:(NSIndexPath *)indexPath 
{
	// Faccio tutte le celle alte uguali
	switch ( [indexPath section]) {
		case 0:
			// Sezione lingue
			return 56.0;
			break;
		case 2:
			// Sezione twitter
			return 56.0;
			break;
		case 3:
			// Sezione Image
			return 48.0;
			break;
		case 4:
			// Sezione suoni
			return 44.0;
			break;
	}
	
	// Di default imposto come altezza 44
	return 44.0;
}

- (CGFloat)tableView:(UITableView *)tableView heightForHeaderInSection:(NSInteger)section
{
	if ((section==2) || (section==3) || (section==4)) return 3.0; else return 26;
}


#pragma mark -
#pragma mark Delegati per la LanguagesViewController

//
// Alla pressione delle righe delle lingue lancio i relativi dialog per l'impostazione delle lingue
//
- (void)languageDidSelected:(NSString *)abbreviation forTag:(int)tag
{
    if (tag == kSourceLanguage) {
		[langIn setText:abbreviation];
		flagIn.image = [UIImage imageWithContentsOfFile:[[NSBundle mainBundle] pathForResource:abbreviation ofType:@"png"]];
    } else if (tag == kDestinationLanguage) {
		[langOut setText:abbreviation];
		flagOut.image = [UIImage imageWithContentsOfFile:[[NSBundle mainBundle] pathForResource:abbreviation ofType:@"png"]];
    }
}


#pragma mark -
#pragma mark Azioni varie

//
// Controlla e gestisce l'abilitazione del tasto DONE per l'invio del poll.
// 
- (BOOL)checkDoneButton
{
	BOOL canEnable=YES;
	
	// Controllo sommario della email
	if  (([emailAddr.text length] < 6) || ( [emailAddr.text rangeOfString:@"@"].location == NSNotFound ) || ( [emailAddr.text rangeOfString:@"."].location == NSNotFound )) canEnable=NO;	
	
	// Riporto anche il risultato del controllo
	return canEnable;
}


//
// Chiamata per abilitare o meno lo slider del volume
//
- (void)switchSoundsCtlAction:(id)sender
{
	if (switchSoundsCtl.on) {
		sliderSounds.enabled=YES;
	} else {
		sliderSounds.enabled=NO;
	}
}



- (void)testSoundAction:(id)sender
{
	pikAppDelegate *_delegate=  [UIApplication sharedApplication].delegate;
	if (switchSoundsCtl.on) {
		float valore=sliderSounds.value/10.0;
		[_delegate playSound:@"/Pikling Good 2 -8db.mp3" withVolume:&valore];
	}
}

#pragma mark -
#pragma mark - AddressBook Deleagte

- (void)peoplePickerNavigationControllerDidCancel:(ABPeoplePickerNavigationController *)peoplePicker {
	[self dismissModalViewControllerAnimated:YES];
}

- (BOOL)peoplePickerNavigationController: (ABPeoplePickerNavigationController *)peoplePicker shouldContinueAfterSelectingPerson:(ABRecordRef)person 
{
	return YES;
}

- (BOOL)peoplePickerNavigationController:(ABPeoplePickerNavigationController *)peoplePicker shouldContinueAfterSelectingPerson:(ABRecordRef)person property:(ABPropertyID)property identifier:(ABMultiValueIdentifier)identifier{
	ABMultiValueRef emails = ABRecordCopyValue(person, kABPersonEmailProperty);
	CFStringRef email = ABMultiValueCopyValueAtIndex(emails, identifier);
	emailAddr.text = (NSString *)email;
	[self dismissModalViewControllerAnimated:YES];
	
	return NO;	
}

#pragma mark -
#pragma mark Delegati del textfield

#define kOFFSET_FOR_KEYBOARD 216

- (BOOL)textFieldShouldBeginEditing:(UITextField *)textField
{
	UITableViewCell *cell = (UITableViewCell*) [[textField superview] superview];
//	NSLog(@"%f  %f  %f",cell.frame.origin.y,  cell.frame.size.height, tableView.bounds.origin.y);
	
	if (shiftTabella != 0.0f) return YES;
	
	CGFloat fondoCellaY= cell.frame.origin.y + cell.frame.size.height - tableView.bounds.origin.y;
	if ( fondoCellaY > (416 - kOFFSET_FOR_KEYBOARD)) shiftTabella = fondoCellaY - kOFFSET_FOR_KEYBOARD +15.0; else shiftTabella=0.0f;
//	NSLog(@"shift %f fondocellaY=%f ",shiftTabella, fondoCellaY);
	
	[self setViewMovedUp:&shiftTabella];

	return YES;
}

-(IBAction)inputTextDone:(id)sender
{
	shiftTabella*=-1.0;
	[self setViewMovedUp:(&shiftTabella)];
	shiftTabella=0.0;
	
	[sender resignFirstResponder];
}

//
// Gestione up/down della pagina per evitare che la tastiera copra il campo che sto editando
//
- (void)setViewMovedUp:(CGFloat *)movedUp
{
	[UIView beginAnimations:nil context:NULL];
	[UIView setAnimationDuration:0.3];
	
	CGRect rect = self.view.frame;
	rect.origin.y -= *movedUp;
	rect.size.height += *movedUp;
	self.view.frame = rect;
	
	[UIView commitAnimations];
}



//=============================================================================================================================
#pragma mark -
#pragma mark SA_OAuthTwitterEngineDelegate

- (void) storeCachedTwitterOAuthData: (NSString *) data forUsername: (NSString *) username 
{
	NSUserDefaults			*defaults = [NSUserDefaults standardUserDefaults];
	
	[defaults setObject: data forKey: @"authData"];
	[defaults synchronize];
}

- (NSString *) cachedTwitterOAuthDataForUsername: (NSString *) username 
{
	return [[NSUserDefaults standardUserDefaults] objectForKey: @"authData"];
}

#pragma mark -
#pragma mark TwitterEngineDelegate

- (void) requestSucceeded: (NSString *) requestIdentifier {
//	NSLog(@"Request %@ succeeded", requestIdentifier);
}

- (void) requestFailed: (NSString *) requestIdentifier withError: (NSError *) error {
//	NSLog(@"Request %@ failed with error: %@", requestIdentifier, error);
}


@end
