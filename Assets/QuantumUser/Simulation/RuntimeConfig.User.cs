﻿using System;
using System.Collections.Generic;
using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public partial class RuntimeConfig
    {
        public AssetRef<EntityPrototype> PaddlePrototype;
        public AssetRef<EntityPrototype> BallPrototype;
        public FP PaddleSpeed;
        public FP AIPaddleSpeed;
        public FP BallSpeed;
        public FPVector2 GameSize;
        public int PlayersCount;
        public int CountdownTime;
        public int GameTime;
        public int FinishedTime;
        [Range(0, 100)] public int PowerUpChance;
        public AssetRef<BasePowerUp>[] PowerUps;
    }
}
