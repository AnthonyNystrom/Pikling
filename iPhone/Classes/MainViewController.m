//
//  MainViewController.m
//  Pikling
//
//  Created by Alex on 23/04/09.
//  Copyright 7touch Group 2009. All rights reserved.
//

#import <QuartzCore/CoreAnimation.h>

#import "MainViewController.h"
#import "MainView.h"

#import "pikAppDelegate.h"

#import "ResultDialog.h"
#import "LanguagesViewController.h"
#import "TwitterSettingsViewController.h"

#import "SA_OAuthTwitterEngine.h"
#import "SA_OAuthTwitterController.h"

//#import "AudioToolbox/AudioServices.h"

//#define DEBUG_BUTTON

#define kRetryButton 1
#define kSaveButton 2

@implementation MainViewController

@synthesize flagIn, flagOut, settingsButton, swapButton ;
@synthesize locationManager;

- (id)initWithNibName:(NSString *)nibNameOrNil bundle:(NSBundle *)nibBundleOrNil {
    if (self = [super initWithNibName:nibNameOrNil bundle:nibBundleOrNil]) {
        // Custom initialization
    }
    return self;
}


// Implement viewDidLoad to do additional setup after loading the view, typically from a nib.
- (void)viewDidLoad {
	piklingDesc.text=NSLocalizedString(@"Snap or select a photo of your foreign text and Pikling will translate it for you.",@"");

	flagIn.tag=kSourceLanguage;
	flagOut.tag=kDestinationLanguage;
	dropButton=nil;
	
	// Faccio partire il GPS
	self.locationManager = [[[CLLocationManager alloc] init] autorelease]; 
	self.locationManager.delegate = self; 

	if ( ! self.locationManager.locationServicesEnabled ) { 
		// Segnalo all'utente che il GPS e' disattivato 
	} else { 
		// Attivo il gps 
		[self.locationManager startUpdatingLocation]; 
	} 
	
	// Preparo in sottofondo i dialog che posso chiamare dalla videata principale (snap & settings)
	picker = [[UIImagePickerController alloc] init];
	picker.delegate = self;
	picker.modalTransitionStyle=UIModalTransitionStyleFlipHorizontal;
	
	settingViewController = [[FlipsideViewController alloc] initWithNibName:@"FlipsideView" bundle:nil];
	settingViewController.delegate = self;
	//settingViewController.modalTransitionStyle=UIModalTransitionStyleCoverVertical;
	settingViewController.modalTransitionStyle=10;
	
	
	[super viewDidLoad];
}

- (void)viewDidAppear:(BOOL)animated
{
	// Verifico che siano stati effettuati i settaggi (langIn e langOut e email altrimenti apro io il dialogo)
	if ( ( ![[[NSUserDefaults standardUserDefaults] objectForKey:@"langIn"] length] ) || ( ![[[NSUserDefaults standardUserDefaults] objectForKey:@"langOut"] length] )  
	   || ( ![[[NSUserDefaults standardUserDefaults] objectForKey:@"emailAddress"] length] ) ) {
		// E' la prima esecuzione, imposto io dei valori di deafults
		[[NSUserDefaults standardUserDefaults] setObject:@"en" forKey:@"langIn"];
		[[NSUserDefaults standardUserDefaults] setObject:@"es" forKey:@"langOut"];
		[[NSUserDefaults standardUserDefaults] setBool:YES forKey:@"switchAutoManCtl"];

		[[NSUserDefaults standardUserDefaults] setBool:YES forKey:@"switchSoundsCtl"];
		[[NSUserDefaults standardUserDefaults] setObject:@"5.0" forKey:@"volumeSounds"];
		
		[self showSetting];
	}
	
	// Nel caso abbia una bandiera da far cadere sullo schermo, lo faccio
	if (dropButton != nil) {
		[self show_flag:dropButton];
		dropButton = nil;
	}
}

- (void)viewWillAppear:(BOOL)animated
{
	// Disegno le bandierine sulla videata princiaple
	[self updateFlags];
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
	[picker release];
	[settingViewController release];
	
	[piklingDesc release];
	[flagIn release];
	[flagOut release];
	[baseSheet release];
	
    [super dealloc];
}

- (void)flipsideViewControllerDidFinish:(FlipsideViewController *)controller {
	[self dismissModalViewControllerAnimated:YES];
}

#pragma mark -

#define kOAuthConsumerKey			@"7kvogUtgrqC85dwzO68Ayg"
#define kOAuthConsumerSecret		@"rIZXOhnX4NctCH396ZU132IT1574Tnf7LyGQp1DFKA"

//
// Attaccato ad un thread per l'invio dello stato su twitter
//
- (void)sendTwitterStatus:(id)sender
{
	NSAutoreleasePool* pool = [NSAutoreleasePool new];   

	SA_OAuthTwitterEngine *	_engine = [[SA_OAuthTwitterEngine alloc] initOAuthWithDelegate: self];
	_engine.consumerKey = kOAuthConsumerKey;
	_engine.consumerSecret = kOAuthConsumerSecret;	

	if( [_engine isAuthorized] ) {
		[_engine performSelectorOnMainThread:@selector(sendUpdate:) withObject:(NSString *)sender waitUntilDone:YES];
		[_engine release];	
	}

	[pool release];
}


//
// Disegno sullo schermo la progress bar per indicare l'avanzamento dell'upload
//
- (void)startProgressAnimation
{
	if (!baseSheet) {
		baseSheet = [[UIActionSheet alloc] initWithTitle:NSLocalizedString(@"Please Wait" ,@"")
												delegate:self
                                       cancelButtonTitle:nil
                                  destructiveButtonTitle: nil
                                       otherButtonTitles: nil];
		
        progbar = [[UIProgressView alloc] initWithFrame:CGRectMake(50.0f, 45.0f, 220.0f, 20.0f)];
        [progbar setProgressViewStyle: UIProgressViewStyleDefault];
        [baseSheet addSubview:progbar];
        [progbar release];
		
		UILabel *message = [[UILabel alloc] initWithFrame:CGRectMake(10.0f, 65.0f, 300.0f, 30.0f)];
		[message setText:NSLocalizedString(@"Uploading image...",@"")];
		[message setTextAlignment:UITextAlignmentCenter];
		[message setBackgroundColor:[UIColor clearColor]];
		[message setFont:[UIFont systemFontOfSize:13]];
		[message setTextColor:[UIColor whiteColor]];
		[baseSheet addSubview:message];
		[message release];
		
		[baseSheet addButtonWithTitle:@"HIDE"];
		for (UIView *vista in baseSheet.subviews) {
			if (([vista isKindOfClass:[UILabel class]]) || ([vista isKindOfClass:[UIProgressView class]])) {
				//NSLog(@"Mostro %@", [vista class]);
				[vista setAlpha:1.0f];
			} else {
				[vista setAlpha:0.0f];
			}
			
		}
		[progbar setProgress: 0.0];
	}
	
	pikAppDelegate * _delegate = (pikAppDelegate *)[UIApplication sharedApplication].delegate;
    [baseSheet showInView:_delegate.window];
} 


//
// Tolgo la progress bar dallo schermo
//
- (void)stopProgressAnimation
{
	// Chiudo la rotellina
	if (baseSheet) {
		[baseSheet dismissWithClickedButtonIndex:0 animated:YES];
		[baseSheet release];
		baseSheet=nil;
	}
}


//
// Aggiorno a video il PNG delle bandierine
//
- (void)updateFlags
{
	// Disegno le bandierine sulla videata princiaple
	[flagIn setBackgroundImage:[UIImage imageWithContentsOfFile:[[NSBundle mainBundle] pathForResource:[[NSUserDefaults standardUserDefaults] objectForKey:@"langIn"] ofType:@"png"]] forState:UIControlStateNormal];
	[flagIn setBackgroundImage:[UIImage imageWithContentsOfFile:[[NSBundle mainBundle] pathForResource:[[NSUserDefaults standardUserDefaults] objectForKey:@"langIn"] ofType:@"png"]] forState:UIControlStateHighlighted];

	[flagOut setBackgroundImage:[UIImage imageWithContentsOfFile:[[NSBundle mainBundle] pathForResource:[[NSUserDefaults standardUserDefaults] objectForKey:@"langOut"] ofType:@"png"]] forState:UIControlStateNormal];
	[flagOut setBackgroundImage:[UIImage imageWithContentsOfFile:[[NSBundle mainBundle] pathForResource:[[NSUserDefaults standardUserDefaults] objectForKey:@"langOut"] ofType:@"png"]] forState:UIControlStateHighlighted];
}


//
// Inizio l'invio dell'immagine al server (e disegno a video la rotellina che gira)
//
- (void)startUploadImageToServer:(id)sender
{
	// Faccio partire la rotellina
//	[self startProgressAnimation];

	// Invio i dati al server
	WebServices *send = [[WebServices alloc] init];
	send.delegate=self;
	[send sendImageToServer:nil];
//	[NSThread detachNewThreadSelector:@selector(sendImageToServer:) toTarget:send withObject:nil];

	[send release];
}

#pragma mark -
#pragma mark TEST GRAYSCALE

#define SUBSET_SIZE 600.0
- (void) prepareSubset:(NSDictionary *)info
{
	UIImage *image= [info objectForKey:UIImagePickerControllerOriginalImage];
	CGSize size = [image size];

	CGRect cropRect;
	if ( [info objectForKey:UIImagePickerControllerCropRect] != nil) {
		cropRect=[[info objectForKey:UIImagePickerControllerCropRect] CGRectValue];
	} else {
		cropRect.origin.x=0;
		cropRect.origin.y=0;
		cropRect.size=size;
	}
	
	float scale = fminf(1.0f, fmaxf(SUBSET_SIZE / cropRect.size.width, SUBSET_SIZE / cropRect.size.height));
	CGPoint offset = CGPointMake(-cropRect.origin.x, -cropRect.origin.y);
	
	size_t subsetWidth = cropRect.size.width * scale;
	size_t subsetHeight = cropRect.size.height * scale;
//	size_t subsetBytesPerRow = ((subsetWidth + 0xf) >> 4) << 4;
	size_t subsetBytesPerRow = subsetWidth*4;
	
	unsigned char * subsetData = (unsigned char *)malloc(subsetBytesPerRow * subsetHeight);
	
	CGColorSpaceRef grayColorSpace = CGColorSpaceCreateDeviceRGB();
//	NSLog(@"nb of components = %d (subsetData %d)", CGColorSpaceGetNumberOfComponents(grayColorSpace), subsetBytesPerRow * subsetHeight);
	CGContextRef ctx = CGBitmapContextCreate(subsetData, subsetWidth, subsetHeight, 8, subsetBytesPerRow, grayColorSpace, kCGImageAlphaNoneSkipFirst);
	CGColorSpaceRelease(grayColorSpace);
	CGContextSetInterpolationQuality(ctx, kCGInterpolationNone);
	CGContextSetAllowsAntialiasing(ctx, false);

	CGContextTranslateCTM(ctx, 0.0, subsetHeight);
	CGContextScaleCTM(ctx, 1.0, -1.0);	
	
	UIGraphicsPushContext(ctx);
	CGRect rect = CGRectMake(offset.x * scale, offset.y * scale, scale * size.width, scale * size.height);
	[image drawInRect:rect];
	UIGraphicsPopContext();
	
	CGContextFlush(ctx);
    
	CGImageRef subsetImageRef = CGBitmapContextCreateImage(ctx);
	
	UIImage *subsetImage;
	subsetImage = [UIImage imageWithCGImage:subsetImageRef];
	CGImageRelease(subsetImageRef);
	
	CGContextRelease(ctx);

	pikAppDelegate *delegate=  [UIApplication sharedApplication].delegate;
	NSDictionary *_jobInfo = [delegate jobInfo];
	[_jobInfo setObject:UIImageJPEGRepresentation(subsetImage, 0.80f) forKey:@"image"];
}  


- (void) prepareSubsetGray:(NSDictionary *)info
{
	UIImage *image= [info objectForKey:UIImagePickerControllerOriginalImage];
	CGSize size = [image size];
	
	CGRect cropRect;
	if ( [info objectForKey:UIImagePickerControllerCropRect] != nil) {
		cropRect=[[info objectForKey:UIImagePickerControllerCropRect] CGRectValue];
	} else {
		cropRect.origin.x=0;
		cropRect.origin.y=0;
		cropRect.size=size;
	}
	
	float scale = fminf(1.0f, fmaxf(SUBSET_SIZE / cropRect.size.width, SUBSET_SIZE / cropRect.size.height));
	CGPoint offset = CGPointMake(-cropRect.origin.x, -cropRect.origin.y);
	
	size_t subsetWidth = cropRect.size.width * scale;
	size_t subsetHeight = cropRect.size.height * scale;
	size_t subsetBytesPerRow = ((subsetWidth + 0xf) >> 4) << 4;
	
	unsigned char * subsetData = (unsigned char *)malloc(subsetBytesPerRow * subsetHeight);
	
	CGColorSpaceRef grayColorSpace = CGColorSpaceCreateDeviceGray();
	CGContextRef ctx = CGBitmapContextCreate(subsetData, subsetWidth, subsetHeight, 8, subsetBytesPerRow, grayColorSpace,   kCGImageAlphaNone);
	CGColorSpaceRelease(grayColorSpace);
	CGContextSetInterpolationQuality(ctx, kCGInterpolationNone);
	CGContextSetAllowsAntialiasing(ctx, false);
	
	CGContextTranslateCTM(ctx, 0.0, subsetHeight);
	CGContextScaleCTM(ctx, 1.0, -1.0);	
	
	UIGraphicsPushContext(ctx);
	CGRect rect = CGRectMake(offset.x * scale, offset.y * scale, scale * size.width, scale * size.height);
	[image drawInRect:rect];
	UIGraphicsPopContext();
	
	CGContextFlush(ctx);
    
	CGImageRef subsetImageRef = CGBitmapContextCreateImage(ctx);
	
	UIImage *subsetImage;
	subsetImage = [UIImage imageWithCGImage:subsetImageRef];
	CGImageRelease(subsetImageRef);
	
	CGContextRelease(ctx);
	
	pikAppDelegate *delegate=  [UIApplication sharedApplication].delegate;
	NSDictionary *_jobInfo = [delegate jobInfo];
	[_jobInfo setObject:UIImageJPEGRepresentation(subsetImage, 0.80f) forKey:@"image"];
}  


#pragma mark -
#pragma mark Segnali collegati a bottoni sullo schermo

//
// Apro il dispositivo di cattura immagini (CAMERA - LIBRARY)
//
-(IBAction)snapFromCamera {
	sourceFromCamera=YES;
	
	[picker setAllowsImageEditing:[[NSUserDefaults standardUserDefaults] boolForKey:@"switchCropCtl"]];
	if ([UIImagePickerController isSourceTypeAvailable:UIImagePickerControllerSourceTypeCamera]) picker.sourceType = UIImagePickerControllerSourceTypeCamera;
	else picker.sourceType = UIImagePickerControllerSourceTypePhotoLibrary;
	
	[self presentModalViewController:picker animated:YES];
}

-(IBAction)snapFromLibrary {
	sourceFromCamera=NO;
	[picker setAllowsImageEditing:[[NSUserDefaults standardUserDefaults] boolForKey:@"switchCropCtl"]];
	picker.sourceType = UIImagePickerControllerSourceTypePhotoLibrary; 
	
	[self presentModalViewController:picker animated:YES];
}


//
// Visualizzo il dialog di impostazione dei settings
//
- (IBAction)showSetting {   
	UINavigationController *settingNavController = [[UINavigationController alloc] initWithRootViewController:settingViewController];
	[self presentModalViewController:settingNavController animated:YES];
	[settingNavController release];
}


//
// Apro il dialogo per la selezione della lingua (from/to)
//
- (IBAction)changeLang:(id)sender
{
	UIButton *bottone=sender;
	NSString *titolo;
	
	if (bottone.tag==kDestinationLanguage) titolo=NSLocalizedString(@"Translate to",@"") ;
	else titolo=NSLocalizedString(@"Translate from",@"") ;
	
	LanguagesViewController *controller = [[LanguagesViewController alloc] initWithLanguage: [[NSUserDefaults standardUserDefaults] objectForKey:@"langOut"]
																					withTag:bottone.tag 
																		withNavigationTitle:titolo ];
	controller.delegate=self;
	[controller showCancelButton];
	UINavigationController *settingNavController = [[UINavigationController alloc] initWithRootViewController:controller];
	
	//	controller.modalTransitionStyle=UIModalTransitionStyleFlipHorizontal;
	controller.modalTransitionStyle=11;
	
	[self presentModalViewController:settingNavController animated:YES];
	
	[settingNavController release];
	[controller release];
}

//
// Gestione up/down della pagina per evitare che la tastiera copra il campo che sto editando
//
- (void)switchFlags
{
	pikAppDelegate *_delegate=  [UIApplication sharedApplication].delegate;
	if ( [[NSUserDefaults standardUserDefaults] boolForKey:@"switchSoundsCtl"] ) [_delegate playSound:@"/Pikling Flag Swap -8db.mp3" withDelta:-0.05f];
	
	[UIView beginAnimations:nil context:NULL];
	[UIView setAnimationDuration:0.3];
	
	CGFloat xIn=flagIn.frame.origin.x;
	CGFloat xOut=flagOut.frame.origin.x;
	
	// Sposto il flagIn
	CGRect rect = flagIn.frame;
	rect.origin.x=xOut;
	flagIn.frame = rect;
	
	// Sposto il flagOut
	rect = flagOut.frame;
	rect.origin.x=xIn;
	flagOut.frame = rect;
	
	[UIView commitAnimations];
	
	UIButton *tmp=flagIn;
	flagIn=flagOut;
	flagOut=tmp;
	
	if (xIn<xOut) {
		flagIn.tag=kSourceLanguage;
		flagOut.tag=kDestinationLanguage;
	} else {
		flagIn.tag=kDestinationLanguage;
		flagOut.tag=kSourceLanguage;
	}
	
}

- (IBAction)swapLanguages 
{
#ifdef DEBUG_BUTTON
	// Chiamate di prova
	[self showResult]; return;
#endif
	// Esegui l'animazione swllo swap delle lingue
	[self switchFlags];

	NSString *appo=[[NSString alloc] initWithString: [[NSUserDefaults standardUserDefaults] objectForKey:@"langIn"]] ;
	// Metto la lingua Out -> In
	[[NSUserDefaults standardUserDefaults] setObject:[[NSUserDefaults standardUserDefaults] objectForKey:@"langOut"]   forKey:@"langIn"];
	// Metto la lingua In (appo) -> Out
	[[NSUserDefaults standardUserDefaults] setObject:appo forKey:@"langOut"];

	// Salvo i nuovi setting
	[[NSUserDefaults standardUserDefaults] synchronize];

	[appo release];
	
	// Disegno le bandierine sulla videata princiaple
//	[self updateFlags];
}


//
// Visualizzo il risultato ricevuto dal server
//
- (IBAction)showResult 
{   
//	[self stopProgressAnimation];

	// Apro il dialog che conterrà il risultato dell'OCR
	ResultDialog *controller = [[ResultDialog alloc] initWithNibName:@"ResultDialog" bundle:nil];
	
	controller.modalTransitionStyle=UIModalTransitionStyleFlipHorizontal;

	[self presentModalViewController:controller animated:YES];
	
	[controller release];
}

#pragma mark -
#pragma mark Salvataggio immagine in libreria
- (void)image:(UIImage *)image didFinishSavingWithError:(NSError *)error contextInfo:(void *)contextInfo
{
}

//
// Salvo l'immagine nella libreria
//
- (IBAction)saveImageOnLibrary:(id)sender {
	pikAppDelegate *delegate=  [UIApplication sharedApplication].delegate;
	NSDictionary *_jobInfo = [delegate jobInfo];
	UIImage *immag =[[UIImage alloc] initWithData:[_jobInfo objectForKey:@"image"]];
	UIImageWriteToSavedPhotosAlbum(immag, self, @selector(image:didFinishSavingWithError:contextInfo:), nil );
	[immag release];
}

#pragma mark -
#pragma mark Delegati del picker

//
// Delegato del picker chiamato quando spingo cancel nella classe della camera
//
- (void)imagePickerControllerDidCancel:(UIImagePickerController *)picker 
{
	[picker dismissModalViewControllerAnimated:YES];
}

//
// Delegato del picker chiamato quando spingo ok nella classe della camera
//
- (void)imagePickerController:(UIImagePickerController *)picker didFinishPickingMediaWithInfo:(NSDictionary *)info
{
	//NSLog(@"didFinishPickingMediaWithInfo");
	
	[NSThread detachNewThreadSelector:@selector(transformImageAndSend:) toTarget:self withObject:info];
	
	// Chiudo la finestra della fotocamera
	[picker dismissModalViewControllerAnimated:YES];
	
	// Torno alla videata principale
	[self dismissModalViewControllerAnimated:YES];
	
	
	[self startProgressAnimation];
}

-(void)transformImageAndSend:(NSDictionary *)info
{
	NSAutoreleasePool *pool = [[NSAutoreleasePool alloc] init];
	
	// Salvo l'immagine catturata nel Job
	pikAppDelegate * delegate = [UIApplication sharedApplication].delegate;
	NSMutableDictionary *_jobInfo = [delegate jobInfo];
	
	UIImage *originalImage = [info objectForKey:UIImagePickerControllerEditedImage];
	if (originalImage==nil) {
		// L'immagine non è stata editata quindi la prendo per intero
		originalImage = [info objectForKey:UIImagePickerControllerOriginalImage];
	}
	
	
	//NSLog(@" Campi nel dizionario: %d",[info count ]);
	//for (id key in info) NSLog(@"key: %@, value: %@", key, [info objectForKey:key]);
	
	// Default parametri per riduzione dell'immagine
	float rapportoCompressione=0.7f;
	
	if (  [[NSUserDefaults standardUserDefaults] boolForKey:@"switchCropCtl"] ) {
		// Converto l'immagine a 640 pixel e in GrayScale
		[self prepareSubset:info];
		
		rapportoCompressione=0.7f;
	} else {
		#define MAX_DIMENSION 800.0	
		CGFloat width=originalImage.size.width;
		CGFloat height=originalImage.size.height;
		CGFloat lower=(MAX_DIMENSION * MIN(width, height)) / MAX(width, height);
		CGSize targetSize;
		if (width > height) { 
			targetSize.width  = MAX_DIMENSION; 
			targetSize.height = lower; 
		} else { 
			targetSize.width  = lower; 
			targetSize.height = MAX_DIMENSION;
		}
		
		//originalImage = [originalImage imageByScalingAndCroppingForSize:targetSize];
		
		originalImage = [originalImage scaleImageToSize:targetSize];
		
		rapportoCompressione=0.7f;
		
		// Salvo l'immagine
		[_jobInfo setObject:UIImageJPEGRepresentation(originalImage, rapportoCompressione) forKey:@"image"];
//		NSLog(@"Immagine a colori [%d bytes]",  [[_jobInfo objectForKey:@"image"] length]);
	}
	
	// Invio l'immagine al server
	[self startUploadImageToServer:nil];
	
	[pool release];
}


#pragma mark - UIAlertViewDelegate

- (void)alertView:(UIAlertView *)actionSheet clickedButtonAtIndex:(NSInteger)buttonIndex
{
	if (buttonIndex==kRetryButton) { // Tasto RETRY
//		[self startProgressAnimation];
		[self performSelector:@selector(startProgressAnimation) withObject:nil afterDelay:0.2f];

		// Provo a rimandare l'immagine al server
		[self performSelector:@selector(startUploadImageToServer:) withObject:nil afterDelay:0.2f];
	} else if (buttonIndex==kSaveButton) {
		// Salvo l'immagine in libreria
		[self performSelector:@selector(saveImageOnLibrary:) withObject:nil afterDelay:0.2f];
	}		
}


#pragma mark -
#pragma mark WebServices Delegate

//
// Segnale generato in caso di errore durante l'upload dell'immagine
//
- (void)streamEventErrorOccurred:(NSString *)errorString;
{
	[self stopProgressAnimation];

	// Salvo i risultati del processo in un file
	pikAppDelegate *_delegate=  [UIApplication sharedApplication].delegate;
	[_delegate saveJobDictionary];
	
	if (sourceFromCamera) {
		UIAlertView *alert = [[UIAlertView alloc] initWithTitle:NSLocalizedString(@"Communication error!",@"") 
														message:[NSString stringWithFormat:errorString]
													   delegate:self
											  cancelButtonTitle:NSLocalizedString(@"Cancel",@"")
											  otherButtonTitles:NSLocalizedString(@"Retry",@""), NSLocalizedString(@"Save in library",@""), nil];
		[alert show];                                                                                                                        
		[alert release];
	} else {
		UIAlertView *alert = [[UIAlertView alloc] initWithTitle:NSLocalizedString(@"Communication error!",@"") 
														message:[NSString stringWithFormat:errorString]
													   delegate:self
											  cancelButtonTitle:NSLocalizedString(@"Cancel",@"")
											  otherButtonTitles:NSLocalizedString(@"Retry",@""), nil];
		[alert show];                                                                                                                        
		[alert release];
	}

	if ( [[NSUserDefaults standardUserDefaults] boolForKey:@"switchSoundsCtl"] ) [_delegate playSound:@"/Pikling Bad 1 -8db.mp3" withDelta:-0.03f];
}


//
// Segnale generato a fine upload e a ricezione dati avvenuta
//
- (void)WebServicesDidFinish
{	
	pikAppDelegate *_delegate=  [UIApplication sharedApplication].delegate;

	// Invio lo stato su twitter
	[NSThread detachNewThreadSelector:@selector(sendTwitterStatus:) toTarget:self withObject:NSLocalizedString(@"has just translated an image using pikling",@"")];
//	[self sendTwitterStatus:NSLocalizedString(@"has just translated an image using pikling",@"")];

	// Chiudo la rotellina
	[self performSelectorOnMainThread:@selector(stopProgressAnimation) withObject:nil waitUntilDone:YES];

	// Salvo i risultati del processo in un file
	[_delegate saveJobDictionary];

	NSDictionary *_jobInfo = [_delegate jobInfo];

	// Emetto un souno
	if ( [[NSUserDefaults standardUserDefaults] boolForKey:@"switchSoundsCtl"]) [_delegate playSound:@"/Pikling Good 3 -8db.mp3" withDelta:0.0f];

	// Mostro il risultati di pikling (solo se ne ha trovati)
	if ( [[_jobInfo objectForKey:@"OriginalResult"] length] ) {
		[self performSelectorOnMainThread:@selector(showResult) withObject:nil waitUntilDone:YES];
	} else {
	    UIAlertView *alert = [[UIAlertView alloc] initWithTitle:NSLocalizedString(@"Attention",@"")
						message:NSLocalizedString(@"Pikling has not found texts\nin your picture!",@"")
				delegate:nil
				  cancelButtonTitle:NSLocalizedString(@"Ok",@"")
				  otherButtonTitles:nil];                                                                                                                        
	    [alert show];                                                                                                                        
	    [alert release];    
	}
}

//
// Aggiorno la progress bar di avanzamento upload
//
- (void)uploadPercent:(int)value
{
	[progbar setProgress: (value / 100.0)];
}

//
// Finito l'upload dell'immagine ora inizia la fase di OCR
//
- (void)uploadImageDidFinish
{
	[baseSheet setMessage:@"Translating text..."]; 
}

#pragma mark -
#pragma mark Delegati per la LanguagesViewController

//
// Alla pressione delle bandierine sullo schermo lancio i relativi dialog per l'impostazione delle lingue
//
- (void)languageDidSelected:(NSString *)abbreviation forTag:(int)tag
{
	UIButton *buttonSelected;

	if (tag == kSourceLanguage) {
		[[NSUserDefaults standardUserDefaults] setObject:abbreviation  forKey:@"langIn"];
		buttonSelected=flagIn;
	} else if (tag == kDestinationLanguage) {
		[[NSUserDefaults standardUserDefaults] setObject:abbreviation  forKey:@"langOut"];
		buttonSelected=flagOut;
	}
	
	// Aggiorno le icone sulla videata principale
	[self updateFlags];

	// Chiudo il dialogo delle lingue
	[self dismissModalViewControllerAnimated:YES];
	
	// Faccio un pò di scena con le animazioni
	[buttonSelected setAlpha:0.0f];
	CATransform3D transform = CATransform3DMakeScale(20.0f, 20.0f, 1.0f);
	[buttonSelected.layer setTransform:transform];

	dropButton=buttonSelected;	
	
	// Salvo la nuova impostazione delle lingue
	[[NSUserDefaults standardUserDefaults] synchronize];
}


-(void)show_flag:(UIButton*)button
{
	pikAppDelegate *_delegate=  [UIApplication sharedApplication].delegate;
	if ( [[NSUserDefaults standardUserDefaults] boolForKey:@"switchSoundsCtl"] )  [_delegate playSound:@"/Pikling Flag Drop Down.mp3" withDelta:0.0f];
	
	[UIView beginAnimations:nil context:NULL];
	[UIView setAnimationDuration:0.35f];
	
	CATransform3D transform = CATransform3DMakeScale(1.0f, 1.0f, 1.0f);
	[button.layer setTransform:transform];
	[button setAlpha:0.7f];
	
	[UIView commitAnimations];
}


#pragma mark -
#pragma mark Delegati per il localizzatore

- (void)locationManager:(CLLocationManager *)manager didUpdateToLocation:(CLLocation *)newLocation fromLocation:(CLLocation *)oldLocation 
{ 
	// Utilizzare passati dal metodo per gestire le posizioni 
	pikAppDelegate *delegate=  [UIApplication sharedApplication].delegate;
	[delegate setCurrentPosition:newLocation];
} 

// Called when there is an error getting the location 
- (void)locationManager:(CLLocationManager *)manager didFailWithError:(NSError *)error 
{ 
	// Gestione degli errori 
//	NSLog(@"GPS: didFailWithError");
}


//=============================================================================================================================
/*
#pragma mark -
#pragma mark TwitterEngineDelegate

// Attivare sta roba solo per debug
- (void) requestSucceeded: (NSString *) requestIdentifier {
	NSLog(@"Request %@ succeeded", requestIdentifier);
}

- (void) requestFailed: (NSString *) requestIdentifier withError: (NSError *) error {
	NSLog(@"Request %@ failed with error: %@", requestIdentifier, error);
}
*/

#pragma mark SA_OAuthTwitterEngineDelegate

- (void) storeCachedTwitterOAuthData: (NSString *) data forUsername: (NSString *) username 
{
	[[NSUserDefaults standardUserDefaults] setObject: data forKey: @"authData"];
	[[NSUserDefaults standardUserDefaults] synchronize];
}

- (NSString *) cachedTwitterOAuthDataForUsername: (NSString *) username 
{
	return [[NSUserDefaults standardUserDefaults] objectForKey: @"authData"];
}

@end
