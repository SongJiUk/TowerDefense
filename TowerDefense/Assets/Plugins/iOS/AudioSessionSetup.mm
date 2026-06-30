#import <AVFoundation/AVFoundation.h>

__attribute__((constructor))
static void SetupAudioSession() {
    [[AVAudioSession sharedInstance]
        setCategory:AVAudioSessionCategoryPlayback
        error:nil];
    [[AVAudioSession sharedInstance] setActive:YES error:nil];
}
