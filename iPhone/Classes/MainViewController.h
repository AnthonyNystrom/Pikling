//
//  MainViewController.h
//  Pikling
//
//  Created by Alex on 23/04/09.
//  Copyright 7touch Group 2009. All rights reserved.
//

#import "FlipsideViewController.h"
#import "WebServices.h"
#import <CoreLocation/CoreLocation.h>
#import "SA_OAuthTwitterEngine.h"

@interface MainViewController : UIViewController <FlipsideViewControllerDelegate, UIImagePickerControllerDelegate, UINavigationControllerDelegate, WebServicesDelegate, LanguagesViewControllerDelegate, UIActionSheetDelegate, CLLocationManagerDelegate> {
	// Icone relativi alle impostazioni lingua
	IBOutlet UILabel *piklingDesc;
	IBOutlet UIButton *flagIn;
	IBOutlet UIButton *flagOut;
	IBOutlet UIButton *settingsButton;
	IBOutlet UIButton *swapButton;

	UIButton *dropButton; //  quando diverso da nil fa cadere il bottone dal cielo

	// Rotellina
	UIActionSheet *baseSheet;
	UIProgressView *progbar;
	NSString *uploadStatusDesc;
		
	// GPS
	CLLocationManager *locationManager;
	
	// Flag che indica che l'immagine viene dalla camera (per attivare il bottone save to lib in caso di errore)
	bool sourceFromCamera;
	
	// Dialogs
	UIImagePickerController *picker;
	FlipsideViewController *settingViewController;
}
@property (retain, nonatomic) UIButton *flagIn;
@property (retain, nonatomic) UIButton *flagOut;
@property (retain, nonatomic) UIButton *settingsButton;
@property (retain, nonatomic) UIButton *swapButton;

@property (nonatomic, retain) CLLocationManager *locationManager;

// Azioni legate ai bottoni presenti sullo schermo
- (IBAction)showSetting;	// Visualizzazione/gestione settaggi
- (IBAction)snapFromCamera;	// Visualizzazione/gestione scatti da camera
- (IBAction)snapFromLibrary;	// Visualizzazione/gestione scatti da libreria
- (IBAction)showResult; // Visualizza il dialog contenenti i risultati dell'OCR

- (IBAction)swapLanguages;	// Swap delle lingue di lavoro IN <-> OUT
- (IBAction)changeLang:(id)sender;

// Altre funzioni
- (void)updateFlags;		// Aggiornamento flag sul dialog
- (void)startProgressAnimation;	// Visualizzazione dialogo rotellina
//- (void)startUploadImageToServer;	// Invio del JOB al server

@end
