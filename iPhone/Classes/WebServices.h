//
//  WebServices.h
//  Pikling
//
//  Created by acerone on 4/30/09.
//  Copyright 2009 7touch Group. All rights reserved.
//

#import <Foundation/Foundation.h>


#define kPiklingServerIp @"servicesnodeone.pikling.com"
//#define kPiklingServerIp @"69.21.14.102"
//#define kPiklingServerIp @"184.106.162.190"

#define kPacketSize 4096

 enum {
	 kWSIdleState = 0,
	 kWSSendPacketType=1,
	 kWSReceivePacketTypeAck = 2,
	 kWSSendLangInfo = 3,
	 kWSReceiveJob = 4,
	 kWSSendImageSize = 5,
	 kWSReceiveImageSize = 6,
	 kWSSendImageData = 7,
	 kWSReceiveImageAnswer = 8,
	 kWSReceiveLangInDataLen = 9,
	 kWSReceiveLangInData = 10,
	 kWSSendLangInAck = 11,
	 kWSReceiveLangOutDataLen = 12,
	 kWSReceiveLangOutData = 13,
	 kWSSendLangOutAck = 14
  };
 typedef NSUInteger WSMasEvent;


@protocol WebServicesDelegate
// Viene generato a fine trasmissione 
- (void)WebServicesDidFinish;
// Viene generato in caso di problemi durante lo streaming
- (void)streamEventErrorOccurred:(NSString *)errorString;
// Viene generato quando sisvuota il buffer di trasmissione
- (void)uploadPercent:(int)value;
// Viene generato a fine upload con esito OK 
- (void)uploadImageDidFinish;
@end

@interface WebServices : NSObject {
	id <WebServicesDelegate> delegate;

	NSOutputStream *oStream;
	NSInputStream *iStream;

	uint byteIndex;
	NSData *txData; // Buffer contenente il pacchetto da trasmettere (TX)

	NSData *header; // Dimensione dell'immagine da spedire
	NSString *richiesta;

	// Variabili di gestione delle MAS
	WSMasEvent uploadImageMAS;	// MAS per l'upload dell'immagine da OCRizzare
	WSMasEvent uploadRequestMAS;	// MAS per la richiesta di invio risposta a email
	BOOL errorAlreadySent;		// Flag per per limitare la generazione continua di errori di comunicazione
	BOOL streamError;

	NSTimer *timer;
}

@property (nonatomic, assign) NSInputStream * iStream;
@property (nonatomic, assign) NSOutputStream * oStream;
@property(nonatomic, assign) id <WebServicesDelegate> delegate;
@property(nonatomic, retain) NSData *header;
@property(nonatomic, retain) NSData *txData;


- (void)sendImageToServer:(id)sender;
- (void)sendRequestoToServerFor:(NSString *)tipo to:(NSString *)destinatario;

- (NSData *)receiveDataWithLength:(size_t)lengthByte;
- (NSData *)receiveData ;
- (void)sendData:(NSData *)_data;

@end


