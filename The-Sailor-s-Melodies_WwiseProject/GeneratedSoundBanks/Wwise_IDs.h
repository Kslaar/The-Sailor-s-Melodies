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
        static const AkUniqueID PLAY_PICKUP_FUEL = 578142713U;
        static const AkUniqueID PLAY_PICKUP_QUEST = 2692550093U;
        static const AkUniqueID PLAY_QUEST_DIRECTION = 851505750U;
        static const AkUniqueID PLAY_SAILING = 1530826435U;
        static const AkUniqueID PLAY_STARTSCENEMUSIC = 3993991295U;
        static const AkUniqueID PLAY_VILLAGE_ISLAND = 2275961610U;
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

        namespace GAMESTATE
        {
            static const AkUniqueID GROUP = 4091656514U;

            namespace STATE
            {
                static const AkUniqueID DIALOGUE = 3930136735U;
                static const AkUniqueID DOCKED = 2742168187U;
                static const AkUniqueID NONE = 748895195U;
                static const AkUniqueID PAUSED = 319258907U;
                static const AkUniqueID QUESTLOG = 1214987797U;
                static const AkUniqueID RACING = 104887399U;
                static const AkUniqueID SAILING = 1921590194U;
            } // namespace STATE
        } // namespace GAMESTATE

        namespace MUSIC_STATE
        {
            static const AkUniqueID GROUP = 3826569560U;

            namespace STATE
            {
                static const AkUniqueID EXPLORATION = 2582085496U;
                static const AkUniqueID ISLAND_01 = 2402435976U;
                static const AkUniqueID NONE = 748895195U;
            } // namespace STATE
        } // namespace MUSIC_STATE

    } // namespace STATES

    namespace BANKS
    {
        static const AkUniqueID INIT = 1355168291U;
        static const AkUniqueID MAIN = 3161908922U;
        static const AkUniqueID MUSIC = 3991942870U;
    } // namespace BANKS

    namespace BUSSES
    {
        static const AkUniqueID BOAT_AUDIO = 2945336769U;
        static const AkUniqueID ISLAND_AUDIO = 779236412U;
        static const AkUniqueID MASTER = 4056684167U;
    } // namespace BUSSES

    namespace AUDIO_DEVICES
    {
        static const AkUniqueID NO_OUTPUT = 2317455096U;
        static const AkUniqueID SYSTEM = 3859886410U;
    } // namespace AUDIO_DEVICES

}// namespace AK

#endif // __WWISE_IDS_H__
