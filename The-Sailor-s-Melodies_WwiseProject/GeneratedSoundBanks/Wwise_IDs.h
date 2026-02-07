/////////////////////////////////////////////////////////////////////////////////////////////////////
//
// Audiokinetic Wwise generated include file. Do not edit.
//
/////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef __WWISE_IDS_H__
#define __WWISE_IDS_H__

#include <AK/SoundEngine/Common/AkTypes.h>

namespace AK
{
    namespace EVENTS
    {
        static const AkUniqueID PLAY_MOTORLOOP = 1855672063U;
        static const AkUniqueID PLAY_MOTORSTART = 1538229119U;
        static const AkUniqueID PLAY_MOTORSTOP = 1103577285U;
        static const AkUniqueID PLAY_SAILING = 1530826435U;
        static const AkUniqueID PLAY_STARTSCENEMUSIC = 3993991295U;
        static const AkUniqueID STOP_SAILING = 1258201081U;
        static const AkUniqueID UI_CLICK = 2249769530U;
        static const AkUniqueID UI_ERROR = 1009189048U;
        static const AkUniqueID UI_START = 1219048826U;
    } // namespace EVENTS

    namespace STATES
    {
        namespace BOATMODE
        {
            static const AkUniqueID GROUP = 3546073390U;

            namespace STATE
            {
                static const AkUniqueID MOTORACTIVE = 2524403114U;
                static const AkUniqueID NONE = 748895195U;
                static const AkUniqueID SAILING = 1921590194U;
            } // namespace STATE
        } // namespace BOATMODE

    } // namespace STATES

    namespace BANKS
    {
        static const AkUniqueID INIT = 1355168291U;
        static const AkUniqueID MAIN = 3161908922U;
        static const AkUniqueID MUSIC = 3991942870U;
    } // namespace BANKS

    namespace BUSSES
    {
        static const AkUniqueID MAIN_AUDIO_BUS = 2246998526U;
    } // namespace BUSSES

    namespace AUDIO_DEVICES
    {
        static const AkUniqueID NO_OUTPUT = 2317455096U;
        static const AkUniqueID SYSTEM = 3859886410U;
    } // namespace AUDIO_DEVICES

}// namespace AK

#endif // __WWISE_IDS_H__
