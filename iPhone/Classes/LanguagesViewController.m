//
//  LanguagesViewController.m
//  yoGomee
//
//  Created by Alex on 21/04/09.
//  Copyright 2009 7touch Group. All rights reserved.
//

#import "LanguagesViewController.h"
#import "pikAppDelegate.h"

@implementation LanguagesViewController

@synthesize listData, languageSelected, delegate;

- (id)initWithLanguage:(NSString *)_language withTag:(int)_tag withNavigationTitle:(NSString *)navTitle {
	self = [super init];
	
	if (self) {
		self.languageSelected = _language;
		self.navigationItem.title = navTitle;
	}
	tag = _tag;
	
	// Di default non mostro il bottone cancel
	showCancel=NO;

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
	// Carico dal file languages.plist l'elenco delle lingue disponibili
	NSBundle *bundle = [NSBundle mainBundle];
	NSString *plistPath = [bundle pathForResource:@"languages" ofType:@"plist"];
	NSArray *lista=[[NSArray alloc] initWithContentsOfFile:plistPath];

	self.listData = lista;
	[lista release];
	
	// Table view definition
	UIView *contentView = [[[UIView alloc] initWithFrame:[UIScreen mainScreen].applicationFrame] autorelease];
	
	contentView.autoresizesSubviews = YES;
	self.view = contentView;
	[self.view setBackgroundColor:[UIColor groupTableViewBackgroundColor]];
	
	CGRect frame = contentView.frame;
	frame.origin.x = 0;
	frame.origin.y = contentView.bounds.size.height - frame.size.height;
	frame.size.width = contentView.bounds.size.width;
	frame.size.height = contentView.bounds.size.height - 43;

	languageTableView = [[UITableView alloc] initWithFrame:frame style:UITableViewStylePlain ];
	languageTableView.delegate = self;
	languageTableView.dataSource = self;
	languageTableView.separatorStyle = UITableViewCellSeparatorStyleSingleLine;

	// Altezza di default caselle
	[languageTableView setRowHeight:52.0];

	// Visualizzo il bottone cancel
	if (showCancel) {
	    UIBarButtonItem *currentPositionButton = [[UIBarButtonItem alloc] initWithTitle:NSLocalizedString(@"Cancel",@"") style:UIBarButtonItemStyleBordered  target:self action:@selector(backButton:)];
	    [[self navigationItem] setLeftBarButtonItem:currentPositionButton];
	    [currentPositionButton setEnabled:YES];
	    [currentPositionButton release];
	}
	
	// Show table
	[self.view addSubview:languageTableView];
}

-(IBAction)backButton:(id *) sender {
	[self dismissModalViewControllerAnimated:YES];
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
	[listData release];
	[languageTableView release];
	[languageSelected release];

    [super dealloc];
}

- (void)showCancelButton
{
	showCancel=YES;
}

#pragma mark -
#pragma mark Delegati per le tabella
- (UITableViewCell *)tableView:(UITableView *)tableView cellForRowAtIndexPath:(NSIndexPath *)indexPath
{
//	NSString *SimpleTableIdentifier = [NSString stringWithFormat:@"Lingua_%d", [indexPath row]];
	
	NSUInteger row=[indexPath row];
	
	UITableViewCell *cell = [tableView dequeueReusableCellWithIdentifier:@"linguaCell"];
	
	if (cell == nil) {
		cell=[[[UITableViewCell alloc] initWithStyle:UITableViewCellStyleSubtitle reuseIdentifier:@"linguaCell"] autorelease];
	}
	[cell.textLabel setText:[[[listData objectAtIndex:row] objectForKey:@"original"] capitalizedString]];
	[cell.detailTextLabel setText:[[[listData objectAtIndex:row] objectForKey:@"english"] capitalizedString]];
	cell.detailTextLabel.textColor=[UIColor colorWithRed:0.27 green:0.38 blue:0.54 alpha:1.0];

	// Metto i dati nella cella
	NSString *abbreviation = [[listData objectAtIndex:row] objectForKey:@"abbreviation"];
	NSString *imageFileName = [[[NSString alloc] initWithFormat:@"%@.png",abbreviation] autorelease];
	cell.imageView.image =[UIImage imageNamed:imageFileName];

	return cell;
}

- (NSInteger)tableView:(UITableView *)tableView numberOfRowsInSection:(NSInteger)section
{
	return [self.listData count]; 
}

- (void)tableView:(UITableView *)tableView didSelectRowAtIndexPath:(NSIndexPath *)indexPath
{
	// Disevidenzio la cella che ho selezionato
	[tableView deselectRowAtIndexPath:indexPath animated:YES];

	[self.navigationController popViewControllerAnimated:YES];

	NSDictionary *voce=[listData  objectAtIndex:[indexPath row]] ; 
	// Genero un segnale per dire che ho selezionato la lingua
	[self.delegate languageDidSelected:[voce objectForKey:@"abbreviation"] forTag:tag];
	
//	pikAppDelegate *_delegate=  [UIApplication sharedApplication].delegate;
//	if ( [[NSUserDefaults standardUserDefaults] boolForKey:@"switchSoundsCtl"] )  [_delegate playSound:@"/Pikling Tap -8db.mp3" withDelta:-0.05f];

	return;
}

- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView 
{
	return 1;
}
/*
- (NSString *)tableView:(UITableView *)tableView titleForHeaderInSection:(NSInteger)section 
{
    return @"";
}

- (NSString *)tableView:(UITableView *)tableView titleForFooterInSection:(NSInteger)section 
{
    return @"";
}
*/

- (CGFloat)tableView:(UITableView *)tableView heightForRowAtIndexPath:(NSIndexPath *)indexPath 
{
	// Faccio tutte le celle alte uguali
	return 60.0;
}


@end
