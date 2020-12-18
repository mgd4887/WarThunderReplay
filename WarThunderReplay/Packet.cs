using System;

namespace WarThunderReplay
{
    class Packet
    {
        private ByteStream _binaryData;
        private int _offset;
        private int length;
        private int _lengthOffset;

        public Packet(byte[] binaryData, int lengthOffset, int offset)
        {
            _binaryData = new ByteStream(binaryData);
            _lengthOffset = lengthOffset;
            _offset = offset;
            length = binaryData.Length;
        }

        /// <summary>
        /// Takes the packet binary and returns a packet of a defined packet type.
        /// </summary>
        /// <returns> A subclass of the TypedPacket superclass</returns>
        public TypedPacket Classify(int x)
        {
            var _ = _binaryData.GetBytes(_lengthOffset); //ignored header for packet length
            var time = _binaryData.GetBytes(4); // float representing match time
            // packet main types
            // 00 = PT_STOP_REPLAY
            // 01 = PT_BEGIN_PLAY
            // 02 = PT_AIRCRAFT_DATA_SMALL
            // 03 = PT_MPI -- has sub types
            var packetMainType = _binaryData.GetByte(); //byte representing the packet type;
            if (packetMainType > 0x03)
            {
                throw new ArgumentOutOfRangeException();
            }

            switch ((PacketType)packetMainType)
            {
                case PacketType.PT_STOP_REPLAY:
                    Console.WriteLine(PacketType.PT_STOP_REPLAY);
                    break;
                case PacketType.PT_BEGIN_PLAY:
                    Console.WriteLine(PacketType.PT_BEGIN_PLAY);
                    break;
                case PacketType.PT_AIRCRAFT_DATA_SMALL:
                    Console.WriteLine(PacketType.PT_AIRCRAFT_DATA_SMALL);
                    break;
                case PacketType.PT_MPI:
                    var objectID = _binaryData.GetBytes(4);
                    var mpiType = (MpiType)_binaryData.GetByte();
                    Console.WriteLine(PacketType.PT_MPI + "\t" + mpiType);
                    break;
            }
            return null;
        }
    }

    public enum PacketType
    {
        PT_STOP_REPLAY = 0x00,
        PT_BEGIN_PLAY = 0x01,
        PT_AIRCRAFT_DATA_SMALL = 0x02,
        PT_MPI = 0x03
    }

    public enum MpiType
    {
        InfMoveToPos = 0x1,
        InfMoveToUnit = 0x2,
        InfHalt = 0x3,
        InfHide = 0x4,
        InfSetTarget = 0x5,
        InfDie = 0x6,
        InfUpdateAnimState = 0x7,
        InfChangeState = 0x8,
        InfGroupState = 0x9,
        InfSetDest = 0x10,
        InfSetVel = 0xb,
        PlayerResultMsg = 0xd,
        ReplicatedUnitCreate = 0xe,
        LeaveMissionClientRequest = 0x10,
        UnitDoPartDamageOffender = 0x11,
        DoFlakExplosion = 0x13,
        UnitCreateNetShell = 0x14,
        GmGroundExplosion = 0x15,
        GmGroundExplosionSmokeEffect = 0x16,
        GmGroundExplosionFireEffect = 0x17,
        GmDoInactive = 0x18,
        GmOverrideGunTime = 0x19,
        GmDoSingleShotReliable = 0x1a,
        GmDoStartFire = 0x1b,
        GmDoStopFire = 0x1c,
        SemChangeAircraftRequest = 0x1d,
        PpChangeAircraftResponse = 0x1e,
        PlayFlakExplosionVisual = 0x1f,
        GmSetGunAngles = 0x20,
        RunTriggerAction = 0x28,
        PlayTrick = 0x2b,
        SetTimeSpeedEx = 0x2c,
        ReflectionData = 0x2d,
        UnitRequestRespawn = 0x2f,
        UnitRespawn = 0x30,
        FmwCutPartResponse = 0x31,
        UnitRequestRepair = 0x32,
        UnitRepair = 0x33,
        StartGroundFx = 0x34,
        UnitDetected = 0x37,
        OwnerPlayerViewDirection = 0x38,
        DeferredReplicatedUnitCreate = 0x39,
        HintPlayFromScript = 0x3b,
        HintStop = 0x3c,
        ScoresGain = 0x3d,
        ReplayCutsceneInfo = 0x3f,
        FmwTurretAt = 0x41,
        ReplayCameraParams = 0x43,
        ReplayCameraType = 0x44,
        ReplayLocalPlayerNo = 0x45,
        FmwReloadGun = 0x46,
        FmwRpm = 0x47,
        ReplayDangerUnit = 0x49,
        ReplayCockpitParams = 0x4a,
        OnUnitDead = 0x4b,
        RequestUnitDead = 0x4c,
        UnitDoExplosion = 0x4d,
        FmwDoDrown = 0x4e,
        FmwHitProp = 0x4f,
        UnitBailoutResponse = 0x50,
        ShellExplosion = 0x51,
        FmwTurretFire = 0x52,
        UnitRequestRearm = 0x53,
        UnitRearm = 0x54,
        MissionFailOrSuccess = 0x55,
        TextCriticalHitReport = 0x56,
        TextKillReport = 0x58,
        TextNewTeamLeader = 0x59,
        TeamScore = 0x5a,
        SpectatedUnitChanged = 0x5b,
        FadeToDebriefing = 0x5d,
        ServerQuit = 0x5e,
        FmwSwitchFireAndGears = 0x61,
        SpectateRequestId = 0x62,
        UnitTeleportTo = 0x64,
        FmwControlsPacket = 0x65,
        FmwAuthorityApprovedState = 0x66,
        RequestForReflection = 0x67,
        MissionEvent = 0x68,
        DialogMessageAction = 0x6a,
        UnitBulletHitPartCount = 0x6e,
        FmwStartBulletsTest = 0x6f,
        FmwTurretFireExcludingOwner = 0x70,
        UnitReloadGuns = 0x71,
        VoiceMessageActionResponse = 0x72,
        GroundModelPositions = 0x73,
        GroundModelPositionsServerReplay = 0x74,
        FmwRepairBlacknessStart = 0x75,
        FmwRepairBlacknessReached = 0x76,
        FmwRepairFinalize = 0x77,
        TextStreak = 0x78,
        ReplayUnitLoadingStatus = 0x79,
        MissionProgressData = 0x7a,
        MissionProgressDataReceived = 0x7b,
        MissionProgressTimeData = 0x7c,
        MissionProgressTimeDataReceived = 0x7d,
        FmwSpeakRadio = 0x7e,
        UnitSetSelectedTarget = 0x7f,
        UnreliableReflectionData = 0x80,
        UnreliableReflectionReply = 0x81,
        GmPlayGroundDamageEffects = 0x82,
        FmwDesyncStats = 0x83,
        FmwBailoutRequest = 0x85,
        VoiceMessageActionRequest = 0x88,
        UnitSetDecals = 0x89,
        GmShipControlsPacket = 0x8b,
        VehicleAuthorityApprovedState = 0x8c,
        FmwAuthorityApprovedPartialState = 0x8d,
        PlayExplosionVisual = 0x8e,
        UnitAttachEffectRelative = 0x8f,
        PpAcceptAircraftResponse = 0x90,
        UnitVersionMismatch = 0x91,
        UnitVersionMatch = 0x92,
        UnitVersionSpawnResync = 0x93,
        RbResurrect = 0x94,
        GmDesyncStats = 0x95,
        WaypointDestructor = 0x96,
        CzReset = 0x97,
        ScoreRespawnInfo = 0x98,
        PlayLandcrashExplosions = 0x99,
        DvmUnreliableHpReflectionData = 0x9a,
        DvmUnreliableHpReflectionReply = 0x9b,
        EconUpdateWeapon = 0x9c,
        EconUpdateModification = 0x9d,
        FmwRequestRearmOnAirfield = 0x9e,
        FmwGunReloaded = 0x9f,
        GmDoSingleShotUnreliable = 0xa0,
        FmwScheduleReload = 0xa1,
        RequestRespawnBaseResync = 0xa2,
        UnitDoPartRestore = 0xa4,
        UnitArtilleryShootAtPosRequest = 0xa5,
        UnitArtilleryShootingAtPos = 0xa6,
        UnitAntiAirSetPriorityTarget = 0xa7,
        UnitDesyncData = 0xa8,
        UnitSetControlsDisplacementToTheFuture = 0xa9,
        RedundancyReflectionData = 0xaa,
        DvmCutDecor = 0xab,
        GmDoAmmoExplode = 0xac,
        UnitDelayedStatusChange = 0xad,
        UnitRequestRepairTracks = 0xae,
        RbAsAirfieldZone = 0xaf,
        UnitStartBullet = 0xb0,
        UnitSingleShot = 0xb1,
        DebugLine = 0xb2,
        DebugSphere = 0xb3,
        InitialPlayerDllStat = 0xb4,
        TankRequestRepair = 0xb5,
        SmartCutsceneFinished = 0xb6,
        SendPlayerES = 0xb7,
        PlayAreaEffects = 0xb8,
        UnitExtinguisherActivate = 0xb9,
        PlayerDllStat = 0xba,
        UnitRequestDamageModelEvents = 0xbb,
        UnitResponseDamageModelEvents = 0xbc,
        UnitBulletRearm = 0xbd,
        OnRaceStarted = 0xbe,
        OnRaceWaypointPassed = 0xbf,
        MissionFailOrSuccessPersonal = 0xc0,
        EconInitAmmo = 0xc1,
    }
}