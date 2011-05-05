//
//  WebServices.m
//  Pikling
//
//  Created by acerone on 4/30/09.
//  Copyright 2009 7touch Group. All rights reserved.
//

#import "WebServices.h"
#import "UIImageExtras.h"
#import "pikAppDelegate.h"

@interface NSStream(Host)

+(void)getStreamsToHostNamed:(NSString *)hostName 
						 port:(NSInteger)port 
				  inputStream:(NSInputStream **)inputStreamPtr 
				 outputStream:(NSOutputStream **)outputStreamPtr;
@end

@implementation NSStream(Host)

+(void)getStreamsToHostNamed:(NSString *)hostName 
						 port:(NSInteger)port 
				  inputStream:(NSInputStream **)inputStreamPtr 
				 outputStream:(NSOutputStream **)outputStreamPtr
{
    CFReadStreamRef     readStream;
    CFWriteStreamRef    writeStream;
	
    assert(hostName != nil);
    assert( (port > 0) && (port < 65536) );
    assert( (inputStreamPtr != NULL) || (outputStreamPtr != NULL) );
	
    readStream = NULL;
    writeStream = NULL;
	
    CFStreamCreatePairWithSocketToHost(
									   NULL, 
									   (CFStringRef) hostName, 
									   port, 
									   ((inputStreamPtr  != nil) ? &readStream : NULL),
									   ((outputStreamPtr != nil) ? &writeStream : NULL)
									   );
	
    if (inputStreamPtr != NULL) {
        *inputStreamPtr  = [NSMakeCollectable(readStream) autorelease];
    }
    if (outputStreamPtr != NULL) {
        *outputStreamPtr = [NSMakeCollectable(writeStream) autorelease];
    }
}
@end


@implementation WebServices

@synthesize iStream, oStream, delegate;
@synthesize header, txData;

//
// Apro la connessione con il server
//
- (void)openConnection
{
	streamError=NO;
	[NSStream getStreamsToHostNamed:kPiklingServerIp port:8080 inputStream:&iStream outputStream:&oStream] ;
	if(self.iStream && self.oStream) {
		self.iStream.delegate = self;
		[self.iStream scheduleInRunLoop:[NSRunLoop currentRunLoop] forMode:NSRunLoopCommonModes];
		[self.iStream open];
		
		self.oStream.delegate = self;
		[self.oStream scheduleInRunLoop:[NSRunLoop currentRunLoop] forMode:NSRunLoopCommonModes];
		[self.oStream open];
	}
	
	[UIApplication sharedApplication].networkActivityIndicatorVisible = YES;

	// Inizializzo le MAS
	uploadImageMAS = kWSIdleState;
	uploadRequestMAS = kWSIdleState;
	
	// Azzero il flag che vincola la generazione dei messaggi di errore
	// Al primo errore segnalo l'anomalia e chiuso la connessione
	errorAlreadySent=NO;
}


//
// Chiudo la connessione col server
//
- (void) closeConnection
{
	// Distruggo il timer di sicurezza
	[timer invalidate];
	timer=nil;

	// Chiudo gli stream 
	[self.iStream close];
//	[self.iStream removeFromRunLoop:[NSRunLoop currentRunLoop] forMode:NSDefaultRunLoopMode];
//	[self.iStream release];

	[self.oStream close];
//	[self.oStream removeFromRunLoop:[NSRunLoop currentRunLoop] forMode:NSDefaultRunLoopMode];
//	[self.oStream release];
	
	// Chiudo tutte le eventuali MAS in corso
	uploadImageMAS = kWSIdleState;
	uploadRequestMAS = kWSIdleState;

	[UIApplication sharedApplication].networkActivityIndicatorVisible = NO;
}


//
// Genero il segnale di errore e chiudo la connessione con il server
//
- (void) errorOccuredWithString:(NSString *)errorDescription
{
	if (!errorAlreadySent) {
		errorAlreadySent=YES;

		// Segnalo l'errore
		[self.delegate streamEventErrorOccurred:errorDescription];

		// Chiudo la connessione con il server
		[self closeConnection];
	}
}


//
// Macchina a stati per la gestione dell'invio del pacchetto
// Viene chiamata del NSStream delegate ad ogni segnale RX/TX ricevuto
//
- (void)sendImageMAS
{
	//NSLog(@"sendImageMAS %d", uploadImageMAS);

	pikAppDelegate * _delegate = [UIApplication sharedApplication].delegate;
	NSMutableDictionary *_jobInfo = [_delegate jobInfo];

	NSData * answer;
	NSUInteger dimensioneImmagine;
	uint8_t esito;
	static uint32_t arriveranno;
	NSString *lingue;
	NSData *imageData;
	NSMutableData * message;

	switch ( uploadImageMAS ) {
		case kWSIdleState: // Stato di riposo della MAS
			break;

		case kWSSendPacketType: // Segnalo il tipo di pacchetto che sto per inviare
			esito=0; // 0 = Invio immagine
			[self.oStream write:&esito maxLength:1];
			uploadImageMAS=kWSReceivePacketTypeAck;
			break;
		case kWSReceivePacketTypeAck:
			answer = [self receiveDataWithLength:1];
			[answer getBytes:&esito length:1];
			if (esito == 0) {
			    uploadImageMAS = kWSSendLangInfo;
			} else {
			    [self errorOccuredWithString:NSLocalizedString(@"Pikling protocol error",@"")];
			}
		case kWSSendLangInfo: // Invio i primi 5 bytes contenenti le lingue di lavoro (sorgente e destinazione)
			// Assegno al job che sto processando le due lingue di lavoro
			[_jobInfo setObject:[[NSUserDefaults standardUserDefaults] objectForKey:@"langIn"] forKey:@"OriginalLanguage"];
			[_jobInfo setObject:[[NSUserDefaults standardUserDefaults] objectForKey:@"langOut"] forKey:@"DestinationLanguage"];

			// Invio la richiesta al server relativa ai linguaggi di codifica
			lingue = [[[NSString alloc] initWithFormat:@"%@|%f|%f|%@|%2@|%2@",
					  [_delegate deviceInfoString], _delegate.currentPosition.coordinate.latitude , _delegate.currentPosition.coordinate.longitude, 
					  [[NSUserDefaults standardUserDefaults] objectForKey:@"emailAddress"], 
					  [_jobInfo objectForKey:@"OriginalLanguage"], [_jobInfo objectForKey:@"DestinationLanguage"]] autorelease];

			// Sparo i dati
			dimensioneImmagine = [lingue length];

			message = [[[NSMutableData alloc] initWithCapacity:([lingue length]+2)] autorelease];
			[message appendBytes:(const void *)&dimensioneImmagine length:2];
			[message appendData:[lingue dataUsingEncoding:NSASCIIStringEncoding]];

			//NSLog(@"1. Spedito pacchetto con info lingue %@", [message description]);

			[self.oStream write:[message bytes] maxLength:[message length]];
			uploadImageMAS = kWSReceiveJob;

			break;

		case kWSReceiveJob:	// Ricevo l'identificativo del JOB (10 bytes)
			answer = [self receiveDataWithLength:10];
			//NSLog(@"2. Ricevuto identificativo del Job dal server %@", [answer description]);

			// Imposto il nuovo identificativo del JOB
			[_jobInfo setObject:answer forKey:@"JobIdentifier"];

			// Rimuovo vecchie chiavi che potrebbero essermi rimaste dal''ultimo JOB eseguito
			[_jobInfo removeObjectForKey:@"OriginalResult"];
			[_jobInfo removeObjectForKey:@"DestinationResult"];

			uploadImageMAS = kWSSendImageSize;
			//break; Commentato perchè sparo subito il pacchetto successivo
		case kWSSendImageSize: // Invio i primi 4 bytes contenenti la dimensione dell'immagine da inviare
			dimensioneImmagine = [[_jobInfo objectForKey:@"image"] length];
			header = [[ NSData alloc] initWithBytes:&dimensioneImmagine length:4 ];

			// Sparo i dati
			[self.oStream write:[header bytes] maxLength:[header length]];
//			NSLog(@"Upload di immagine [%d bytes]", dimensioneImmagine);
			uploadImageMAS = kWSReceiveImageSize;
			break;
		case kWSReceiveImageSize:	// Ho ricevuto l'header lo confronto e mando l'immagine
			answer = [self receiveDataWithLength:4];
			//NSLog(@"4. Ricevuto pacchetto header dal server %@  header %@", [answer description], [header description]);

			// Controllo il pacchetto ricevuto e se OK vado avanti
			if ([answer isEqualToData:header ]) {
				[header release];
			    uploadImageMAS = kWSSendImageData;
			} else {
			    // Segnalo l'errore e chiudo la connessione
			    [self errorOccuredWithString:NSLocalizedString(@"Pikling protocol error",@"")];

			    break;
			}

		case kWSSendImageData:	// Invio l'immagine al server
			// Invio l'immagine
			imageData = [_jobInfo objectForKey:@"image"];
			//NSLog(@"5. Spedizione in uscita di %d", [imageData length]);
			[self sendData:imageData];

			uploadImageMAS = kWSReceiveImageAnswer;
			break;
		case kWSReceiveImageAnswer:	// Ricevo 1 byte con l'esito della trasmissione
			answer = [self receiveDataWithLength:1];
			[answer getBytes:&esito length:1];
			if (esito == 1) {
				uploadImageMAS = kWSReceiveLangInDataLen;

				// Genero un segnale per dire che l'upload è finito positivamente
				[self.delegate uploadPercent:100];// Upload completo
				[self.delegate uploadImageDidFinish];
			} else {
			    // Segnalo l'errore e chiudo la connessione
			    [self errorOccuredWithString:NSLocalizedString(@"Communication problems during image upload",@"")];
			}

			break;

		case kWSReceiveLangInDataLen:	// Ricevo 4 bytes che mi dicono quanto sarà lungo il prossimo pacchetto
			answer = [self receiveDataWithLength:5];
			[answer getBytes:&arriveranno range:(NSRange){1, 4} ];
			//NSLog(@"7. Ricevuto lunghezza risposta %d", arriveranno);

			[answer getBytes:&esito length:1 ];
			//NSLog(@"engine %d", esito);

			// Motore di ricerca fuori dai limiti consentiti
			if ((esito > 3) && ( esito != 0xFF)) {
				// Segnalo l'errore e chiudo la connessione
				[self errorOccuredWithString:NSLocalizedString(@"Pikling protocol error",@"")];
			} else {
				// Motore traduzioni corretto 
				[_jobInfo setObject:[NSString stringWithFormat:@"%d", esito ] forKey:@"Translator"];

				// Esamino il numero di bytes che mi dovranno arrivare in risposta
				if (arriveranno) {
					uploadImageMAS = kWSReceiveLangInData;
				} else {
					// Segnalo la fine del processo agli interessati
					[self.delegate WebServicesDidFinish];

					[self closeConnection];
				}
			}
			break;
		case kWSReceiveLangInData:	// Ricevo la traduzione dal server in lingua sorgente
			answer = [self receiveDataWithLength:arriveranno];

			NSString *dataString = [[[NSString alloc] initWithData:answer encoding:NSUTF8StringEncoding] autorelease];
//			NSLog(@"OCR in lingua sorgente: %@",dataString);

			[_jobInfo setObject:dataString forKey:@"OriginalResult"];
		case kWSSendLangInAck: // Mando il risultato della ricezione del pacchetto
			esito=1;
			[self.oStream write:&esito maxLength:1];
			uploadImageMAS=kWSReceiveLangOutDataLen;
			break;

		case kWSReceiveLangOutDataLen:
			answer = [self receiveDataWithLength:4];
			[answer getBytes:&arriveranno length:4 ];

			uploadImageMAS = kWSReceiveLangOutData;
			break;
		case kWSReceiveLangOutData:	// Ricevo la traduzione dal server in lingua sorgente
			answer = [self receiveDataWithLength:arriveranno];

			dataString = [[[NSString alloc] initWithData:answer encoding:NSUTF8StringEncoding] autorelease];
			//NSLog(@"dataString len %d", [dataString length]);
			[_jobInfo setObject:dataString forKey:@"DestinationResult"];

//			NSLog(@"OCR in lingua destinazione: %@", dataString);
			uploadImageMAS=kWSReceiveLangOutData;
		case kWSSendLangOutAck: // Mando il risultato della ricezione del pacchetto
			esito=1;
			[self.oStream write:&esito maxLength:1];

			// Segnalo la fine del processo agli interessati
			[self.delegate WebServicesDidFinish];

			// Chiudo la connessione con il server
			[self closeConnection];
			break;
		default:
			// Segnalo l'errore e chiudo la connessione
			[self errorOccuredWithString:NSLocalizedString(@"Pikling protocol error",@"")];
    }
}


//
// Macchina a stati per la gestione della richiesta invio email
// Viene chiamata del NSStream delegate ad ogni segnale RX/TX ricevuto
//
- (void)sendRequestMAS
{
	//NSLog(@"sendRequestMAS %d", uploadRequestMAS);

	NSData * answer;
	uint16_t lunghezzaPacchetto;
	uint8_t esito;

	switch ( uploadRequestMAS ) {
		case kWSIdleState: // Stato di riposo della MAS
			break;

		case kWSSendPacketType: // Segnalo il tipo di pacchetto che sto per inviare
			esito=1; // 1 = Richiesta spedizione mail
			[self.oStream write:&esito maxLength:1];
			uploadRequestMAS=kWSReceivePacketTypeAck;
			break;
		case kWSReceivePacketTypeAck:
			answer = [self receiveDataWithLength:1];
			[answer getBytes:&esito length:1];
			if (esito == 1) {
			    uploadRequestMAS = kWSSendLangInfo;
			} else {
				// Segnalo l'errore e chiudo la connessione
				[self errorOccuredWithString:NSLocalizedString(@"Pikling protocol error",@"")];
				break;
			}

		case kWSSendLangInfo: // Invio 2 bytes contenenti la lunghezza del pacchetto di richiesta
			lunghezzaPacchetto = [richiesta length];
			header = [[ NSData alloc] initWithBytes:&lunghezzaPacchetto length:2 ];

			// Sparo i dati
			[self.oStream write:[header bytes] maxLength:[header length]];

			uploadRequestMAS = kWSReceiveImageSize;
			break;

		case kWSReceiveImageSize:	// Ricevo l'echo del pacchetto inviato
			answer = [self receiveDataWithLength:2];

			// Controllo il pacchetto ricevuto e se OK vado avanti
			if ([answer isEqualToData:header]) {
				uploadRequestMAS = kWSSendImageData;
				[header release];
			} else {
				// Segnalo l'errore e chiudo la connessione
				[self errorOccuredWithString:NSLocalizedString(@"Pikling protocol error",@"")];
				break;
			}
		case kWSSendImageData:	// Invio la richiesta al server
			[self.oStream write:[[richiesta dataUsingEncoding:NSASCIIStringEncoding] bytes] maxLength:[richiesta length]];
			//NSLog(richiesta);
			uploadRequestMAS = kWSReceiveImageAnswer;
			break;
		case kWSReceiveImageAnswer:
			answer = [self receiveDataWithLength:1];
			[answer getBytes:&esito length:1];
			if (esito == 1) {
			    // Segnalo la fine del processo agli interessati
			    [self.delegate WebServicesDidFinish];

			    // Chiudo la connessione con il server
			    [self closeConnection];
			} else {
			    // Segnalo l'errore e chiudo la connessione
			    [self errorOccuredWithString:NSLocalizedString(@"Pikling protocol error",@"")];
			}
			break;
		default:
			// Segnalo l'errore e chiudo la connessione
			[self errorOccuredWithString:NSLocalizedString(@"Pikling protocol error",@"")];
    }
}


//
// Funzione da chiamare per l'upload dell'immagine (l'immagine deve essere presente all'interno del jobInfo)
//
- (void)sendImageToServer:(id)sender
{
	NSAutoreleasePool* pool = [NSAutoreleasePool new];   

//	NSLog(@"Inizio trasmissione immagine al server");
	// Mi collego con il server
	[self openConnection];

	pikAppDelegate * _delegate = [UIApplication sharedApplication].delegate;
	NSMutableDictionary *_jobInfo = [_delegate jobInfo];
	[_jobInfo removeObjectForKey:@"JobIdentifier"];

	// Attivo la macchina a stati per l'invio dell'immagine
	uploadImageMAS = kWSSendPacketType;
	[self sendImageMAS]; // Innesco la MAS

	// Faccio partire un timer di sicurezza che dopo 30 sec. chiude il collegamento nel caso non sia ancora arrivata la risposta
	timer = [NSTimer scheduledTimerWithTimeInterval:30.0f target:self selector:@selector(connectionTimeout:) userInfo:nil repeats:NO];

	// Se non ho avuto problemi durante la connessione inizio l'invio
	if (!streamError) {
		do {
			[[NSRunLoop currentRunLoop] runMode:NSDefaultRunLoopMode beforeDate:[NSDate distantFuture]];
		} while (uploadImageMAS != kWSIdleState);
	}
//	NSLog(@"web services finished");
	[pool release];
}


//
// Funzione da chiamare per la richiesta di invio email
//
- (void)sendRequestoToServerFor:(NSString *)tipo to:(NSString *)destinatario;
{
	// Preparo la richiesta
	pikAppDelegate * _delegate = [UIApplication sharedApplication].delegate;
	NSMutableDictionary *_jobInfo = [_delegate jobInfo];

	NSString *idJob = [[[NSString alloc] initWithData:[_jobInfo objectForKey:@"JobIdentifier"] encoding:NSASCIIStringEncoding ] autorelease];
	richiesta = [[NSString alloc] initWithFormat:@"%@|%@|%@", idJob, tipo, destinatario];

	// Mi collego con il server
	[self openConnection];

	// Attivo la macchina a stati per l'invio dell'immagine
	uploadRequestMAS = kWSSendPacketType;
	[self sendRequestMAS]; // Innesco la MAS

	if (!streamError) {
		NSAutoreleasePool* pool = [NSAutoreleasePool new];   
		do {
			[[NSRunLoop currentRunLoop] runMode:NSDefaultRunLoopMode beforeDate:[NSDate distantFuture]];
		} while (uploadRequestMAS != kWSIdleState);
		[pool release];
	}
}	


- (NSData *)receiveData {
	int len = 0;
	size_t lengthByte = kPacketSize;
	NSMutableData *retBlob = nil;

	retBlob = [NSMutableData dataWithLength:lengthByte];

	len = [self.iStream read:(uint8_t *)[retBlob mutableBytes] maxLength:lengthByte];
	//NSLog(@"Stream read: [%d bytes] %@", len, [retBlob description]);

	return retBlob;
}


- (NSData *)receiveDataWithLength:(size_t)lengthByte {
	int len = 0;
	NSMutableData *retBlob = nil;
	retBlob = [NSMutableData dataWithLength:lengthByte];

	len = [self.iStream read:(uint8_t *)[retBlob mutableBytes] maxLength:lengthByte];
	//NSLog(@"letti presenti: %d attesi: %d %@", len, lengthByte, [retBlob description]);	

	return retBlob;
}


- (void)sendData:(NSData *)_data
{
    txData = _data;

    uint8_t *readBytes = (uint8_t *)[txData bytes];
    int data_len = [txData length];
    unsigned int len = ((data_len >= kPacketSize) ? kPacketSize : data_len);
    uint8_t buf[len];
    (void)memcpy(buf, readBytes, len);
    len = [self.oStream write:(const uint8_t *)buf maxLength:len];
    byteIndex += len;
    //NSLog(@"sendData length:%d", byteIndex);

    // Verifico di aver trasmesso tutto il pacchetto in modo da poter distruggere txData
    if (byteIndex == data_len) txData=nil;
}

@end


@implementation WebServices (NSStreamDelegate)
- (void)stream:(NSStream *)stream handleEvent:(NSStreamEvent)eventCode
{
	switch(eventCode) {
		case NSStreamEventHasSpaceAvailable: // TX
		{
		    if (txData != nil) {
				uint8_t *readBytes = (uint8_t *)[txData bytes];
				readBytes += byteIndex; // instance variable to move pointer
				int data_len = [txData length];
				unsigned int len = ((data_len - byteIndex >= kPacketSize) ? kPacketSize : (data_len-byteIndex));
				uint8_t buf[len];
				(void)memcpy(buf, readBytes, len);
				len = [stream write:(const uint8_t *)buf maxLength:len];
				byteIndex += len;
				//NSLog(@"NSStreamEvent (TX) DL:%d L:%d BI:%d %d", data_len, len, byteIndex, (byteIndex*100)/data_len);
				[self.delegate uploadPercent:((byteIndex*97)/data_len)]; // 97 e non 100 in modo da non arrivare mai al 100%

				// Verifico di aver trasmesso tutto il pacchetto in modo da poter distruggere txData
				if (byteIndex == data_len) txData=nil;
		    }
			break;
		}
		case NSStreamEventHasBytesAvailable: // RX
		{
			if(stream == self.iStream) {
				//NSLog(@"NSStreamEventHasBytesAvailable (RX)");

				// Mi è arrivata roba, quindi chiamo le relative MAS
				if (uploadImageMAS) [self sendImageMAS];
				if (uploadRequestMAS) [self sendRequestMAS];
			}
			break;
		}
		case NSStreamEventErrorOccurred:
		{
			//NSLog(@"streamError=YES");
			streamError=YES;

			NSError *theError = [stream streamError]; 

//			NSLog(@"stream NSStreamEventErrorOccurred [ERR]: %@", stream);

			// Segnalo l'errore e chiudo la connessione
			[self errorOccuredWithString:[theError localizedDescription]];

			break;
		}
		default:
			break;
	}
}


//
// E' scattato il timer di sicurezza
//
-(void)connectionTimeout:(NSTimer*)timer
{
	// Segnalo l'errore
	[self.delegate streamEventErrorOccurred:NSLocalizedString(@"Pikling services are down for maintenance.\nPlease try again in a few minutes.",@"")];
	
	// Chiudo la connessione con il server
	[self closeConnection];

    [timer invalidate];
	timer = nil;
}

@end
