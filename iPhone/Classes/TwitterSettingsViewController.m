//
//  TwitterSettingsViewController.m
//  ABYF
//
//  Created by Alex Mantovani on 06/06/09.
//  Copyright 2009 7touch Group. All rights reserved.
//

#import "TwitterSettingsViewController.h"

#import "pikAppDelegate.h"

#define kUsernameTag 1
#define kPasswordTag 2

#define kTwitterIp @"69.21.114.130"

@implementation TwitterSettingsViewController

@synthesize username, password;
@synthesize twitterIpAddr;


+ (void)sendTwitterStatus:(NSString *)message onIpAddr:(NSString *)twitterIpAddr
{
	NSAutoreleasePool *pool = [[NSAutoreleasePool alloc] init];
	
	NSString *un=[[NSUserDefaults standardUserDefaults] objectForKey:@"twitterUsername"];
	NSString *pw=[[NSUserDefaults standardUserDefaults] objectForKey:@"twitterPassword"];
	
	// Invio il mio stato su TWITTER
	if ( ([un length] >2) && ([pw length] >2)) {
		// Se configurato sparo lo stato su twitter
		NSString *richiesta= [[[NSString alloc] initWithFormat:@"http://%@/Twitter.aspx?command=checkexists&username=%@&password=%@", twitterIpAddr, un, pw] autorelease];
		NSURL *twitterURL = [NSURL URLWithString:richiesta];
		
		NSString *block = [[NSString alloc] initWithContentsOfURL:twitterURL];
		if ([block  isEqual:@"True"]) {
			// Username/password corretti ora mando lo stato
			NSString *invioStato = [[[NSString alloc] initWithFormat:@"http://%@/Twitter.aspx?command=updatestatus&username=%@&password=%@&status=%@", twitterIpAddr, un, pw,
						 [message stringByAddingPercentEscapesUsingEncoding:NSUTF8StringEncoding]] autorelease];
			twitterURL = [NSURL URLWithString:invioStato];
			NSString *risposta = [[NSString alloc] initWithContentsOfURL:twitterURL];
			//NSLog(@"risposta %@", risposta);
			[risposta release];
		}
		[block release];
	}
	
	[pool release];
}


- (id)initWithIpAddress:(NSString *)_twitterIpAddr
{
	self = [super init];
	
	if (self) {
		twitterIpAddr = _twitterIpAddr;
	}
	
	return self;
}

/*
 // The designated initializer.  Override if you create the controller programmatically and want to perform customization that is not appropriate for viewDidLoad.
- (id)initWithNibName:(NSString *)nibNameOrNil bundle:(NSBundle *)nibBundleOrNil {
    if (self = [super initWithNibName:nibNameOrNil bundle:nibBundleOrNil]) {
        // Custom initialization
    }
    return self;
}
*/


// Implement loadView to create a view hierarchy programmatically, without using a nib.
- (void)loadView {
	accountInfoChanged=NO;

	// Sistemo la navigation bar
	self.navigationItem.title = NSLocalizedString(@"Settings",@"");
	UIBarButtonItem *doneButton = [[UIBarButtonItem alloc] initWithTitle:NSLocalizedString(@"Done",@"") style:UIBarButtonItemStyleBordered  target:self action:@selector(done:)];
	self.navigationItem.rightBarButtonItem=doneButton;
	
	// Table view definition
	UIView *contentView = [[[UIView alloc] initWithFrame:[UIScreen mainScreen].applicationFrame] autorelease];
	
	contentView.autoresizesSubviews = YES;
	self.view = contentView;
	
	CGRect frame = contentView.frame;
	frame.origin.x = 0;
	frame.origin.y = contentView.bounds.size.height - frame.size.height;
	frame.size.width = contentView.bounds.size.width;
	frame.size.height = contentView.bounds.size.height - 43;
	
	twitterSettingsTableView = [[UITableView alloc] initWithFrame:frame style:UITableViewStyleGrouped ];
	twitterSettingsTableView.delegate = self;
	twitterSettingsTableView.dataSource = self;
	twitterSettingsTableView.separatorStyle = UITableViewCellSeparatorStyleSingleLine;
	[twitterSettingsTableView setScrollEnabled:NO];

	[twitterSettingsTableView setBackgroundColor:[[UIColor alloc] initWithPatternImage:[UIImage imageWithContentsOfFile:[[NSBundle mainBundle] pathForResource:@"setting_pattern" ofType:@"png"]]]];

	// Show table
	[self.view addSubview:twitterSettingsTableView];
}


/*
// Implement viewDidLoad to do additional setup after loading the view, typically from a nib.
- (void)viewDidLoad {
 
 [super viewDidLoad];

 }
*/

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
    [super dealloc];
}


//
// Annullo il processo di creazione di un nuovo account
//
-(IBAction)backButton:(id *) sender {
	[self dismissModalViewControllerAnimated:YES];
}

//
// Chiudo il dialogo e salvo le impostazioni nei defaults
//
- (IBAction)done:(id)sender {
	pikAppDelegate *_delegate=  [UIApplication sharedApplication].delegate;

	if (accountInfoChanged) {
		// Se ho la tastiera aperta la chiudo e prendo il valore del campo
		if (isEditing != nil) [self inputTextDone:isEditing];
		
		// Ho svuotato i campi (significa: NON voglio più usare twitter)
		if (( [username length]==0 ) && ([password length]==0)) {
			// Disabilito il servizio di twitter
			[[NSUserDefaults standardUserDefaults] removeObjectForKey:@"twitterUsername"];
			[[NSUserDefaults standardUserDefaults] removeObjectForKey:@"twitterPassword"];
			[[NSUserDefaults standardUserDefaults] synchronize];

			if ( [[NSUserDefaults standardUserDefaults] boolForKey:@"switchSoundsCtl"] ) [_delegate playSound:@"/Pikling Twitt Off -8db.mp3" withDelta:-0.02f];
		} else {
			// Se configurato, controllo i dati dell'account su twitter
			if (self.twitterIpAddr==nil) self.twitterIpAddr=kTwitterIp;
			NSString *richiesta=[[NSString alloc] initWithFormat:@"http://%@/Twitter.aspx?command=checkexists&username=%@&password=%@", twitterIpAddr, username, password];
			NSURL *twitterURL = [NSURL URLWithString:richiesta];                                                                               
			NSString *block = [[NSString alloc] initWithContentsOfURL:twitterURL];                                                                                                                   
			NSLog(@"twitter test on: %@ %@ richiesta %@ risposta:%@", username, password, richiesta, block);
			[richiesta release];
			[block release];
			if ([block  isEqual:@"True"]) {
				// Utente verificato
				[[NSUserDefaults standardUserDefaults] setObject:username forKey:@"twitterUsername"];
				[[NSUserDefaults standardUserDefaults] setObject:password forKey:@"twitterPassword"];
				[[NSUserDefaults standardUserDefaults] synchronize];
				if ( [[NSUserDefaults standardUserDefaults] boolForKey:@"switchSoundsCtl"] ) [_delegate playSound:@"/Pikling Twitt On -8db.mp3" withDelta:-0.02f];
			} else if ( [block isEqual:@"False"] ) {
				// Dati account non accettati
				UIAlertView *alert = [[UIAlertView alloc] initWithTitle:NSLocalizedString(@"Error!",@"")
																message:NSLocalizedString(@"username/password not valid",@"")
															   delegate:nil
													  cancelButtonTitle:NSLocalizedString(@"Ok",@"")
													  otherButtonTitles:nil];
				[alert show];                                                                                                                        
				[alert release];
				if ( [[NSUserDefaults standardUserDefaults] boolForKey:@"switchSoundsCtl"] ) [_delegate playSound:@"/Pikling Bad 4 -8db.mp3" withDelta:-0.05f];
				
				return;
			}  else {
				// Non sono connesso, guardo i vecchi dati e in base a quelli decido la musichetta
				if (( [[[NSUserDefaults standardUserDefaults] objectForKey:@"twitterUsername"] length]==0 ) && ([[[NSUserDefaults standardUserDefaults] objectForKey:@"twitterPassword"] length]==0)) {
					if ( [[NSUserDefaults standardUserDefaults] boolForKey:@"switchSoundsCtl"] ) [_delegate playSound:@"/Pikling Twitt Off -8db.mp3" withDelta:-0.02f];
				} else {
					if ( [[NSUserDefaults standardUserDefaults] boolForKey:@"switchSoundsCtl"] ) [_delegate playSound:@"/Pikling Twitt On -8db.mp3" withDelta:-0.02f];
				}


			}
		}
	}

	// Torno ai settings
	[self.navigationController popViewControllerAnimated:YES];
}



#pragma mark -
#pragma mark Delegati del textfield

-(IBAction)setFieldValue:(UITextField *)textField
{
	UITextField *testo=textField;
	if (testo.tag == kUsernameTag) {
		username = [[NSString alloc] initWithString: testo.text];
	} else {
		password = [[NSString alloc] initWithString: testo.text];
	}
	accountInfoChanged=YES;
	
	isEditing=nil;
	
	[textField resignFirstResponder];
}

- (BOOL)textFieldShouldBeginEditing:(UITextField *)textField
{
	if (isEditing != nil) [self setFieldValue:(UITextField *)isEditing];
	
	isEditing=textField;
	return YES;
}

-(IBAction)inputTextDone:(id)sender
{
	[self setFieldValue:(UITextField *)sender];
}	


#pragma mark -
#pragma mark Delegati per le tabella

- (UITableViewCell *)tableView:(UITableView *)tableView cellForRowAtIndexPath:(NSIndexPath *)indexPath
{
	UITableViewCell *cell;
	
	UITextField *inputText;
	
	switch ([indexPath section]) {
		case 0: // Banner twitter
			cell = [tableView dequeueReusableCellWithIdentifier:@"twitterAccountCell"];
			if (cell == nil) {
				cell=[[[UITableViewCell alloc] initWithStyle:UITableViewCellStyleDefault reuseIdentifier:@"twitterAccountCell"] autorelease];
			}
			[cell.imageView	setImage: [UIImage imageNamed:@"twitter_big.png"]];
			
			// Cella trasparente
			UIView *transparentBackground = [[UIView alloc] initWithFrame:CGRectZero];
			transparentBackground.backgroundColor = [UIColor clearColor];
			cell.backgroundView = transparentBackground;
			
			[cell setSelected:NO];
			cell.selectionStyle=UITableViewCellSelectionStyleNone;
			break;
		case 1: // Sezione username e password
			cell = [tableView dequeueReusableCellWithIdentifier:@"twitterAccountCell"];
			if (cell == nil) {
				// Campo per l'inserimento del valore
				CGRect textRect = CGRectMake(110, 17, 180, 25);
				inputText = [[UITextField alloc] initWithFrame:textRect];
				inputText.textAlignment=UITextAlignmentRight;
				inputText.clearsOnBeginEditing=NO;
				inputText.delegate=self;
				[inputText addTarget:self action:@selector(inputTextDone:) forControlEvents:UIControlEventEditingDidEndOnExit];
				inputText.clearButtonMode=UITextFieldViewModeWhileEditing;
				inputText.autocorrectionType=UITextAutocorrectionTypeNo;
				inputText.autocapitalizationType=UITextAutocapitalizationTypeNone;
				inputText.keyboardType=UIKeyboardTypeDefault;
				inputText.clearsOnBeginEditing=NO;
				inputText.font=[UIFont systemFontOfSize:18];
				[inputText setTextColor:[UIColor colorWithRed:0.27 green:0.38 blue:0.54 alpha:1.0]];
				inputText.backgroundColor=[UIColor clearColor];
				cell=[[[UITableViewCell alloc] initWithStyle:UITableViewCellStyleDefault reuseIdentifier:@"twitterAccountCell"] autorelease];
			}
			
			[cell.textLabel setTextColor:[UIColor colorWithRed:0.17 green:0.20 blue:0.24 alpha:1.0]];
			if ([indexPath row]==0) {
				[cell.textLabel setText:@"Username"];
			} else {
				[cell.textLabel setText:@"Password"];
			}
			if ([indexPath row]==0) {
				// Personalizzazione campo username
				inputText.tag=kUsernameTag;
				inputText.placeholder=NSLocalizedString(@"user name",@"");
				username=[[NSUserDefaults standardUserDefaults] objectForKey:@"twitterUsername"];
				inputText.text=username;
			} else {
				// Personalizzazione campo password
				inputText.secureTextEntry=YES;
				inputText.tag=kPasswordTag;
				inputText.placeholder=NSLocalizedString(@"password",@"");
				password=[[NSUserDefaults standardUserDefaults] objectForKey:@"twitterPassword"];
				inputText.text=password;
			}
			
			[cell.contentView addSubview:inputText];
			[inputText release];
			
			// Setup della cella
			[cell setSelected:NO];
			[cell setSelectionStyle:UITableViewCellSelectionStyleNone];
			[cell setAccessoryType:UITableViewCellAccessoryNone];
			break;
	}
	
	return cell;
}

- (NSString *)tableView:(UITableView *)tableView titleForHeaderInSection:(NSInteger)section
{
//	if (section==1) return NSLocalizedString(@"Account",@"");
	return @"";
}

- (NSInteger)tableView:(UITableView *)tableView numberOfRowsInSection:(NSInteger)section
{
	switch (section) {
		case 0:
			return 1; 
			break;
		default:
			return 2; 
			break;
	}
}

- (void)tableView:(UITableView *)_tableView didSelectRowAtIndexPath:(NSIndexPath *)indexPath
{

	return;	
}

- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView 
{
	return 2;
}

- (NSString *)tableView:(UITableView *)tableView titleForFooterInSection:(NSInteger)section 
{
	return @"";
}


- (CGFloat)tableView:(UITableView *)tableView heightForRowAtIndexPath:(NSIndexPath *)indexPath 
{
	switch ([indexPath section]) {
		case 0:
			return 56.0;
			break;
		default:
			return 56.0;
			break;
	}
}

@end
