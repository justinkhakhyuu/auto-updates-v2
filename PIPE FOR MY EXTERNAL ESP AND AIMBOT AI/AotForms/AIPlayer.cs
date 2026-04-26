using AotForms;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace AotForms
{
    internal static class XyanPull
    {
        private static Task magnetTask;
        private static CancellationTokenSource cts;
        private static bool isRunning = false;

        private static Dictionary<uint, Vector3> originalPositions = new();
        private static uint currentTargetId = 0;
        private static bool wasFiringLastFrame = false;

        private static bool IsLocalFiring()
        {
            try
            {
                if (Core.LocalPlayer == 0) return false;
                if (InternalMemory.Read(Core.LocalPlayer + Offsets.IS_FIRING, out bool isFiring))
                    return isFiring;
                return false;
            }
            catch { return false; }
        }

        internal static void Work()
        {
            if (isRunning) return;

            // ⭐ Reset CTS so it can be re-enabled
            cts = new CancellationTokenSource();
            isRunning = true;

            magnetTask = Task.Run(() =>
            {
                try
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        if (!Config.PullXZY)
                        {
                            RestoreOriginalPosition();
                            Thread.Sleep(50);
                            continue;
                        }

                        if (IsLocalFiring())
                        {
                            Entity closestEntity = FindClosestEntity();
                            if (closestEntity != null)
                            {
                                UpdateTargetPositionToCrosshair(closestEntity);
                            }
                            wasFiringLastFrame = true;
                        }
                        else if (wasFiringLastFrame)
                        {
                            RestoreOriginalPosition();
                            wasFiringLastFrame = false;
                        }
                        Thread.Sleep(1);
                    }
                }
                finally
                {
                    RestoreOriginalPosition();
                    isRunning = false;
                }
            }, cts.Token);
        }

        private static Entity FindClosestEntity()
        {
            if (Core.Width == -1 || Core.Height == -1 || !Core.HaveMatrix || Core.Entities == null)
                return null;

            var centerX = Core.Width / 2;
            var centerY = Core.Height / 2;
            var fov = 450;

            Entity closestEntity = null;
            float closestDistanceSqr = float.MaxValue;

            foreach (var entity in Core.Entities.Values)
            {
                if (entity.IsDead || entity.IsKnocked || !entity.IsKnown) continue;
                var head2D = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
                if (head2D.X < 1 || head2D.Y < 1) continue;

                float dx = head2D.X - centerX;
                float dy = head2D.Y - centerY;
                float distanceSqr = dx * dx + dy * dy;

                if (distanceSqr <= fov * fov && distanceSqr < closestDistanceSqr)
                {
                    closestEntity = entity;
                    closestDistanceSqr = distanceSqr;
                }
            }
            return closestEntity;
        }

        private static void UpdateTargetPositionToCrosshair(Entity entity)
        {
            try
            {
                if (!InternalMemory.Read(entity.Address + (uint)Bones.Root, out uint bonePtr) || bonePtr == 0) return;
                if (!InternalMemory.Read(bonePtr + 0x8, out uint transformVal) || transformVal == 0) return;
                if (!InternalMemory.Read(transformVal + 0x8, out uint transformObj) || transformObj == 0) return;
                if (!InternalMemory.Read(transformObj + 0x20, out uint matrixVal) || matrixVal == 0) return;

                if (!InternalMemory.Read(matrixVal + 0x60, out Vector3 originalPos)) return;

                if (!originalPositions.ContainsKey(entity.Address))
                    originalPositions[entity.Address] = originalPos;

                Vector3 camPos = Core.LocalMainCamera;
                Vector3 viewDir = Vector3.Normalize(new Vector3(Core.CameraMatrix.M13, Core.CameraMatrix.M23, Core.CameraMatrix.M33));

                // ⭐ Target only the Head bone
                float distance = Vector3.Distance(camPos, entity.Head);

                Vector3 lockedTargetPosition = camPos + (viewDir * distance);
                lockedTargetPosition.Y -= Config.Xyz;

                InternalMemory.Write(matrixVal + 0x60, lockedTargetPosition);
                currentTargetId = entity.Address;
            }
            catch { }
        }

        private static void RestoreOriginalPosition()
        {
            if (currentTargetId == 0) return;

            if (originalPositions.TryGetValue(currentTargetId, out Vector3 originalPos))
            {
                try
                {
                    if (InternalMemory.Read(currentTargetId + (uint)Bones.Root, out uint bonePtr) && bonePtr != 0 &&
                        InternalMemory.Read(bonePtr + 0x8, out uint t1) && t1 != 0 &&
                        InternalMemory.Read(t1 + 0x8, out uint t2) && t2 != 0 &&
                        InternalMemory.Read(t2 + 0x20, out uint matrixVal) && matrixVal != 0)
                    {
                        InternalMemory.Write(matrixVal + 0x60, originalPos);
                    }
                }
                catch { }
            }
            originalPositions.Remove(currentTargetId);
            currentTargetId = 0;
        }

        internal static void Stop()
        {
            if (!isRunning) return;
            cts?.Cancel();
            isRunning = false;
        }
    }
}