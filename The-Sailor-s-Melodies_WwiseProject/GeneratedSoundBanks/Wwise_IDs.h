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
        static const AkUniqueID PLAY_CHECKPOINT = 2962822744U;
        static const AkUniqueID PLAY_FINISH = 1793765179U;
        static const AkUniqueID PLAY_HORN_A = 957019037U;
        static const AkUniqueID PLAY_HORN_B = 957019038U;
        static const AkUniqueID PLAY_HORN_C = 957019039U;
        static const AkUniqueID PLAY_HORN_D = 957019032U;
        static const AkUniqueID PLAY_HORN_E = 957019033U;
        static const AkUniqueID PLAY_HORN_F = 957019034U;
        static const AkUniqueID PLAY_HORN_G = 957019035U;
        static const AkUniqueID PLAY_MOTORLOOP = 1855672063U;
        static const AkUniqueID PLAY_MOTORSTART = 1538229119U;
        static const AkUniqueID PLAY_MOTORSTOP = 1103577285U;
        static const AkUniqueID PLAY_MUSIC_WORLD = 3774920574U;
        static const AkUniqueID PLAY_OCARINA_A = 2686750109U;
        static const AkUniqueID PLAY_OCARINA_B = 2686750110U;
        static const AkUniqueID PLAY_OCARINA_C = 2686750111U;
        static const AkUniqueID PLAY_OCARINA_D = 2686750104U;
        static const AkUniqueID PLAY_OCARINA_E = 2686750105U;
        static const AkUniqueID PLAY_OCARINA_F = 2686750106U;
        static const AkUniqueID PLAY_OCARINA_G = 2686750107U;
        static const AkUniqueID PLAY_PICKUP_FUEL = 578142713U;
        static const AkUniqueID PLAY_PICKUP_QUEST = 2692550093U;
        static const AkUniqueID PLAY_QUEST_DIRECTION = 851505750U;
        static const AkUniqueID PLAY_SAILING = 1530826435U;
        static const AkUniqueID PLAY_STARTSCENEMUSIC = 3993991295U;
        static const AkUniqueID PLAY_VILLAGE_ISLAND = 2275961610U;
        static const AkUniqueID QUEST_ACCEPTED = 602563U;
        static const AkUniqueID STOP_MUSIC = 2837384057U;
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

        namespace MUSICSTATE
        {
            static const AkUniqueID GROUP = 1021618141U;

            namespace STATE
            {
                static const AkUniqueID EXPLORATION = 2582085496U;
                static const AkUniqueID ISLAND_01 = 2402435976U;
                static const AkUniqueID NONE = 748895195U;
            } // namespace STATE
        } // namespace MUSICSTATE

    } // namespace STATES

    namespace SWITCHES
    {
        namespace MUSICSWITCH
        {
            static const AkUniqueID GROUP = 1445037870U;

            namespace SWITCH
            {
                static const AkUniqueID EXPLORATION = 2582085496U;
                static const AkUniqueID ISLAND_01 = 2402435976U;
                static const AkUniqueID MENU = 2607556080U;
            } // namespace SWITCH
        } // namespace MUSICSWITCH

        namespace RACESTATE
        {
            static const AkUniqueID GROUP = 2495262365U;

            namespace SWITCH
            {
                static const AkUniqueID COUNTDOWN = 1505888634U;
                static const AkUniqueID FINISH = 2555741448U;
                static const AkUniqueID IDLE = 1874288895U;
                static const AkUniqueID RACING = 104887399U;
            } // namespace SWITCH
        } // namespace RACESTATE

    } // namespace SWITCHES

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

    namespace AUX_BUSSES
    {
        static const AkUniqueID REVERB = 348963605U;
    } // namespace AUX_BUSSES

    namespace AUDIO_DEVICES
    {
        static const AkUniqueID NO_OUTPUT = 2317455096U;
        static const AkUniqueID SYSTEM = 3859886410U;
    } // namespace AUDIO_DEVICES

}// namespace AK

#endif // __WWISE_IDS_H__
