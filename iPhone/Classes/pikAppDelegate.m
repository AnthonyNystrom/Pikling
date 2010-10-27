//
//  pikAppDelegate.m
//  Pikling
//
//  Created by Alex on 23/04/09.
//  Copyright 7touch Group 2009. All rights reserved.
//

#import "pikAppDelegate.h"
#import "MainViewController.h"

#import <AVFoundation/AVFoundation.h>
#import <AudioToolbox/AudioToolbox.h>

#define DOCSFOLDER [NSHomeDirectory() stringByAppendingPathComponent:@"Documents"]

@implementation pikAppDelegate

@synthesize window;
@synthesize mainViewController, jobInfo, deviceInfoString;
@synthesize currentPosition;

- (void)applicationDidFinishLaunching:(UIApplication *)application {
	[[AVAudioSession sharedInstance ] setCategory: AVAudioSessionCategoryAmbient error: nil];
	UInt32 doSetProperty = 1;
	AudioSessionSetProperty (
							 kAudioSessionProperty_OtherMixableAudioShouldDuck,
							 sizeof (doSetProperty),
							 &doSetProperty
							 );
	
	if ( [[NSUserDefaults standardUserDefaults] boolForKey:@"switchSoundsCtl"] )  [self playSound:@"/Pikling Good 1 -8db.mp3" withDelta:-0.05f];

	// Carico il dizionario del job in corso
	[self loadJobDictionary];

	// Ricavo le info del cellulare da usare nel protocollo
	[self getDeviceInfo];
	
	MainViewController *aController = [[MainViewController alloc] initWithNibName:@"MainView" bundle:nil];
	self.mainViewController = aController;
	[aController release];
	
	mainViewController.view.frame = [UIScreen mainScreen].applicationFrame;
	[window addSubview:[mainViewController view]];
	[window makeKeyAndVisible];
}

- (void)dealloc {
	[mainViewController release];
	[jobInfo release];
	[deviceInfoString release];
	[currentPosition release];
	
	[window release];
	[super dealloc];
}

#pragma mark -
#pragma mark Gestione del dizionario del JOB

//
// Carico i dati dell'ultimo JOB salvato
//
- (void)loadJobDictionary
{
	NSString *plistPath = [DOCSFOLDER stringByAppendingPathComponent:@"job.plist"]; 
	jobInfo=[[NSMutableDictionary alloc] initWithContentsOfFile:plistPath];
	if (jobInfo == nil) jobInfo=[[NSMutableDictionary alloc] initWithCapacity:10];  // 10 oggetti max nel dizionario
}


//
// Salvo i dati del JOB in corso
//
- (void)saveJobDictionary
{
	NSString *plistPath = [DOCSFOLDER stringByAppendingPathComponent:@"job.plist"];
	[jobInfo writeToFile:plistPath atomically:YES];
}


#pragma mark -
#pragma mark Recupero stringa identificativa telefono
//
// Ricavo le informazione del dispositivo su cui gira pikling e preparo la stringa da inserire nel protocollo
// BRAND PHONE | MODEL ID | SERIAL | NUMBER PHONE | OS VERSION            | GEO LAT | GEO LONG | EMAIL | LANG SRC | LANG DST
//
- (void) getDeviceInfo 
{ 
	deviceInfoString = [[NSString alloc] initWithFormat:@"%@|%@|%@|%@|%@",@"Apple", [[UIDevice currentDevice] model], [[UIDevice currentDevice] uniqueIdentifier],
						(NSString *) [[NSUserDefaults standardUserDefaults] objectForKey:@"SBFormattedPhoneNumber"], [[UIDevice currentDevice] systemVersion]];
//	NSLog(@"info string: %@", deviceInfoString);
/*	
	NSMutableString *results = [[NSMutableString alloc] init]; 
	[results appendFormat:@"Identifier:\n %@\n", [[UIDevice currentDevice] uniqueIdentifier]]; 
	[results appendFormat:@"Model: %@\n", [[UIDevice currentDevice] model]]; 
	[results appendFormat:@"Localized Model: %@\n", [[UIDevice currentDevice] localizedModel]]; 
	[results appendFormat:@"Name: %@\n", [[UIDevice currentDevice] name]]; 
	[results appendFormat:@"System Name: %@\n", [[UIDevice currentDevice] systemName]]; 
	[results appendFormat:@"System Version: %@\n", [[UIDevice currentDevice] systemVersion]]; 
	NSLog(@"info: %@", results);
*/
 } 

-(IBAction) playSound:(NSString*)fileName withDelta:(const float)delta
{
	NSURL *url = [NSURL fileURLWithPath:[NSString stringWithFormat:@"%@%@", [[NSBundle mainBundle] resourcePath], fileName]];

	NSError *error;
	AVAudioPlayer *audioPlayer = [[AVAudioPlayer alloc] initWithContentsOfURL:url error:&error];
	audioPlayer.delegate=self;

	float	volumeValue = [[[NSUserDefaults standardUserDefaults] objectForKey:@"volumeSounds"] floatValue] / 10.0f;

	[audioPlayer setVolume:volumeValue+delta];
	audioPlayer.numberOfLoops = 0;
	[audioPlayer setMeteringEnabled:YES];

	if (audioPlayer == nil) NSLog([error description]);
	else [audioPlayer play];
}

-(IBAction) playSound:(NSString*)fileName withVolume:(float*)volumeValue
{	
	NSURL *url = [NSURL fileURLWithPath:[NSString stringWithFormat:@"%@%@", [[NSBundle mainBundle] resourcePath], fileName]];
	
	NSError *error;
	AVAudioPlayer *audioPlayer = [[AVAudioPlayer alloc] initWithContentsOfURL:url error:&error];
	audioPlayer.delegate=self;

	[audioPlayer setVolume:*volumeValue];
	audioPlayer.numberOfLoops = 0;
	[audioPlayer setMeteringEnabled:YES];
	
	if (audioPlayer == nil) NSLog([error description]);
	else [audioPlayer play];
}

- (void)audioPlayerDidFinishPlaying:(AVAudioPlayer *)player successfully:(BOOL)flag;
{
	[[AVAudioSession sharedInstance] setActive: NO error: nil];
}


@end
