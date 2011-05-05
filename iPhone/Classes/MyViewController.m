
#import "MyViewController.h"
#import "pikAppDelegate.h"

@implementation MyViewController

@synthesize textLabel, titolo;

+(float) calculateHeightOfTextFromWidth:(NSString*) text: (UIFont*)withFont: (float)width :(UILineBreakMode)lineBreakMode
{
	[text retain];
	[withFont retain];
	CGSize suggestedSize = [text sizeWithFont:withFont constrainedToSize:CGSizeMake(width, FLT_MAX) lineBreakMode:lineBreakMode];
	
	[text release];
	[withFont release];
	
	return suggestedSize.height;
}

- (id)initWithPageNumber:(int)page {
    if (self = [super initWithNibName:@"MyView" bundle:nil]) pageNumber = page;
    
    return self;
}

- (void)dealloc {
    [titolo release];
    [textLabel release];
    [super dealloc];
}

- (void)viewDidLoad {
	pikAppDelegate *delegate=  [UIApplication sharedApplication].delegate;
	NSDictionary *_jobInfo = [delegate jobInfo];

	// Icona della bandiera
	CGRect frameFlag = CGRectMake(262.0, 10.0, 48.0, 48.0);
	UIImageView *flagImage= [[UIImageView alloc] initWithFrame:frameFlag ];
	textLabel.alpha=0.75f;
	
	if (pageNumber == 0) { // Preparo la pagina che contiene la traduzione in testo finale
		textLabel.text=[ _jobInfo objectForKey:@"DestinationResult"  ];

		// Aggiusto le dimensioni del font
		CGSize suggestedSize = [textLabel.text sizeWithFont:[UIFont systemFontOfSize:22] constrainedToSize:CGSizeMake(300.0, FLT_MAX) lineBreakMode:UILineBreakModeWordWrap];
//NSLog(@"calcolato %f", suggestedSize.height);
		if (suggestedSize.height>60.0) suggestedSize.height=13;
		textLabel.font=[UIFont systemFontOfSize:suggestedSize.height];
		titolo.text = NSLocalizedString(@"Translated to",@"");

		// Se ho troppo testo da visualizzare allora invece che un UILabel uso un UITextView
		if (![self calculateFontSizeForLabel:textLabel]) {
			[textLabel setHidden:YES];

			UITextView *textComplete=[[UITextView alloc] initWithFrame:textLabel.frame];
			textComplete.text=[ _jobInfo objectForKey:@"DestinationResult"  ];
			[textComplete setAlpha:0.75f];
			[textComplete setScrollEnabled:YES];
			[textComplete setPagingEnabled:YES];
			[textComplete setIndicatorStyle:UIScrollViewIndicatorStyleDefault];
			[textComplete setEditable:NO];
			[textComplete setTextColor:[UIColor colorWithRed:0.18 green:0.24 blue:0.29 alpha:1.0]];
			[textComplete setTextAlignment:UITextAlignmentCenter];
			[textComplete setShowsVerticalScrollIndicator:YES];
			[textComplete setShowsHorizontalScrollIndicator:NO];
			[textComplete setFont:[UIFont systemFontOfSize:16]];
			[textComplete setBackgroundColor:[UIColor clearColor]];
			[self.view addSubview:textComplete];
		}

		flagImage.image = [UIImage imageWithContentsOfFile:[[NSBundle mainBundle] pathForResource:[_jobInfo objectForKey:@"DestinationLanguage"] ofType:@"png"]];
		flagImage.alpha=0.75;
		[self.view addSubview:flagImage];
		
		// A seconda del traduttore visualizzo la descrizione relativa
		pikAppDelegate *delegate=  [UIApplication sharedApplication].delegate;
		NSDictionary *_jobInfo = [delegate jobInfo];
		int traslatore =[[_jobInfo objectForKey:@"Translator"] intValue];
		
		NSString *bannerString;
		switch (traslatore) {
			case 0:
				bannerString=NSLocalizedString(@"7touch group, inc.",@"");
				break;
			case 1:
				bannerString=NSLocalizedString(@"Results provided by Google Translator",@"");
				break;
			case 2:
				bannerString=NSLocalizedString(@"Results provided by Yahoo! Babel Fish",@"");
				break;
			case 3:
				bannerString=NSLocalizedString(@"Results provided by Microsoft Translator",@"");
				break;
			default:
				bannerString=NSLocalizedString(@"7touch group, inc.",@"");
				break;
		}

		// Ho tradotto con google o yahoo quindi lo segnalo (in questo caso scrivo una label)
		if (traslatore) {
		    UILabel *banner=[[UILabel alloc] initWithFrame:CGRectMake(10.0, 382.0, 300.0, 14.0)];
		    banner.text=bannerString;
		    banner.textAlignment=UITextAlignmentCenter;
		    banner.font=[UIFont systemFontOfSize:12];
		    banner.textColor=[UIColor grayColor];
		    banner.backgroundColor=[UIColor clearColor];
		    [self.view addSubview:banner];
		    [banner release];
		} else {
		    // Ho tradotto col mio traduttore quindi mi faccio pubblicita (in questo caso metto un bottone)
		    UIButton *banner=[[UIButton buttonWithType:UIButtonTypeCustom] retain];
//		    UIButton *banner=[[UIButton buttonWithType:UIButtonTypeRoundedRect] retain];
//			[banner setAlpha:0.7];
		    banner.frame=CGRectMake(30.0, 372.0, 260.0, 24.0);
		    [banner setTitle:bannerString forState:UIControlStateNormal];
		    banner.backgroundColor = [UIColor clearColor];
		    banner.font=[UIFont systemFontOfSize:12];
		    [banner setTitleColor:[UIColor grayColor] forState:UIControlStateNormal];
		    [banner addTarget:self action:@selector(visit7Touch:) forControlEvents:UIControlEventTouchUpInside];
		    [self.view addSubview:banner];
		    [banner release];
		}
	} else if (pageNumber == 1) { // Preparo la pagina che contiene la traduzione in testo sorgente
		textLabel.text=[ _jobInfo objectForKey:@"OriginalResult"  ];
		titolo.text = NSLocalizedString(@"Translated from",@"");
		
		[self calculateFontSizeForLabel:textLabel];

		flagImage.image = [UIImage imageWithContentsOfFile:[[NSBundle mainBundle] pathForResource:[_jobInfo objectForKey:@"OriginalLanguage"] ofType:@"png"]];
		flagImage.alpha=0.75;
		[self.view addSubview:flagImage];
	} else if (pageNumber == 2) { // Preparo la videata che contiene l'immagine uploadata
		textLabel.text = @"No image";
		titolo.text = NSLocalizedString(@"Original image",@"");

		// Visualizzo l'immagine a colori
		UIImage *immag =[[UIImage alloc] initWithData:[_jobInfo objectForKey:@"image"]];
		
		CGRect imageRect = CGRectMake(20, 58, 280, 324);
		UIImageView *imageView=[[UIImageView alloc ] initWithFrame:( imageRect)];
		[imageView setContentMode:UIViewContentModeScaleAspectFit];
		imageView.image=immag;
		[self.view addSubview:imageView];

		[imageView release];
		[immag release];
	}
	
	[flagImage release];
}


- (void)visit7Touch:(id)sender
{
    [[UIApplication sharedApplication] openURL:[NSURL URLWithString:@"http://blog.7touchgroup.com"]];
}


//
// Calcola e adatta il testo alla label
// Riporta NO se c'è troppa roba da visualizzare
//         SI se il testo sta all'interno della label
//
- (BOOL)calculateFontSizeForLabel:(UILabel*)_label
{
#define MAX_FONT_SIZE 50
	// Guardo se col font minimo riesco a visualizzare tutto
	CGSize suggestedSize= [_label.text sizeWithFont:[UIFont systemFontOfSize:16] constrainedToSize:CGSizeMake(textLabel.frame.size.width, FLT_MAX) 
						   lineBreakMode:UILineBreakModeWordWrap];
	
	if (suggestedSize.height > _label.frame.size.height ) {
		[_label setLineBreakMode:UILineBreakModeTailTruncation];
		_label.font=[UIFont systemFontOfSize:16];
		return NO;
	}
	
	for (int i=11; i<=MAX_FONT_SIZE; i++) {
		suggestedSize= [_label.text sizeWithFont:[UIFont systemFontOfSize:i] 
						constrainedToSize:CGSizeMake(textLabel.frame.size.width, FLT_MAX) lineBreakMode:UILineBreakModeWordWrap];
		
		if ((suggestedSize.height >= textLabel.frame.size.height) || (i==MAX_FONT_SIZE)) {
			_label.font=[UIFont systemFontOfSize:i-1];
			//NSLog(@"dimensioni font: %d",i-1);
			break;
		}
	}
	
	return true;
}

@end
