using UnityEngine;
using UnityEngine.Scripting;
using Quantum;
using Photon.Deterministic;

namespace Tomorrow.Quantum
{
    [Preserve]
    public unsafe class CollisionSystem : SystemSignalsOnly, ISignalOnCollisionEnter3D
    {
        public void OnCollisionEnter3D(Frame f, CollisionInfo3D info)
        {
            // if(!f.IsVerified) return;

            if (f.Unsafe.TryGetPointer<Ball>(info.Entity, out Ball *ball))
            {
                if (f.Unsafe.TryGetPointer<Transform3D>(info.Entity, out Transform3D *ballTransform))
                {
                    if (f.Unsafe.TryGetPointer<Transform3D>(info.Other, out Transform3D *otherTransform))
                    {
                        // collide with paddles
                        if (f.Unsafe.TryGetPointer<Paddle>(info.Other, out Paddle *paddle))
                        {
                            var direction = FPVector3.Normalize(
                                info.ContactNormal + 
                                (paddle->Index == 0 ? 1 : -1) * (ballTransform->Position.X - otherTransform->Position.X) * FPVector3.Right
                            );
                            ball->Velocity = f.RuntimeConfig.BallSpeed * paddle->BallSpeedMultiplier * direction;
                            ball->Paddle = info.Other;
                        }
                        // collide with walls
                        if (f.Unsafe.TryGetPointer<Wall>(info.Other, out _))
                        {
                            var direction = FPVector3.Normalize(
                                FPVector3.Reflect(ball->Velocity, info.ContactNormal)
                            );

                            FP multiplier = 1;
                            if (f.Unsafe.TryGetPointer<Paddle>(ball->Paddle, out Paddle* lastPaddle))
                            {
                                multiplier = lastPaddle->BallSpeedMultiplier;
                            }
                            // var direction = new FPVector3(info.ContactNormal.X, 0, -info.ContactNormal.Z);
                            ball->Velocity = f.RuntimeConfig.BallSpeed * multiplier * direction;
                        }

                        // collide with goals
                        if (f.Unsafe.TryGetPointer<Goal>(info.Other, out _))
                        {
                            ball->Velocity = FPVector3.Zero;
                            f.Signals.OnScoreChanged(info.Entity, info.Other);
                        }
                    }
                }
            }
        }
    }
}
