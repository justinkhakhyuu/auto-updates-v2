using AotForms;
using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace AotForms
{
    internal static class teliv2
    {
        private static Task teleTask;
        private static CancellationTokenSource cts; // Removed inline initialization
        private static bool isRunning = false;

        // ===== SETTINGS =====
        private static float maxDistance = 150f;
        private static float screenPullRange = 140f;
        private static int pullIntervalMs = 5;

        internal static void Work()
        {
            if (isRunning) return;

            // ⭐ FIX 1: Create a NEW token source every time we start
            cts = new CancellationTokenSource();
            isRunning = true;

            teleTask = Task.Run(async () =>
            {
                // Cache token locally for safety
                var token = cts.Token;

                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        if (!Config.teliv2 || !Core.HaveMatrix || Core.Width <= 0 || Core.Height <= 0)
                        {
                            await Task.Delay(pullIntervalMs, token);
                            continue;
                        }

                        // Check Aimbot Key
                        if ((WinAPI.GetAsyncKeyState(Config.AimbotKey) & 0x8000) == 0)
                        {
                            await Task.Delay(pullIntervalMs, token);
                            continue;
                        }

                        Entity bestTarget = null;
                        float bestScreenDist = float.MaxValue;
                        Vector2 screenCenter = new(Core.Width / 2f, Core.Height / 2f);

                        foreach (var entity in Core.Entities.Values)
                        {
                            if (!entity.IsKnown || entity.IsDead || (Config.IgnoreKnocked && entity.IsKnocked))
                                continue;

                            Vector3 bonePos = GetTargetBonePosition(entity);
                            float worldDist = Vector3.Distance(Core.LocalMainCamera, bonePos);

                            if (worldDist < 2f || worldDist > maxDistance)
                                continue;

                            Vector2 head2D = W2S.WorldToScreen(Core.CameraMatrix, bonePos, Core.Width, Core.Height);

                            if (head2D.X < 1 || head2D.Y < 1)
                                continue;

                            float screenDist = Vector2.Distance(head2D, screenCenter);
                            if (screenDist < bestScreenDist)
                            {
                                bestScreenDist = screenDist;
                                bestTarget = entity;
                            }
                        }

                        if (bestTarget != null && bestScreenDist <= screenPullRange)
                        {
                            // ===== PULL TARGET LOGIC =====
                            if (InternalMemory.Read(bestTarget.Address + (uint)Bones.Root, out uint rootBone) && rootBone != 0)
                            {
                                if (InternalMemory.Read(rootBone + 0x8, out uint t1) &&
                                    InternalMemory.Read(t1 + 0x8, out uint t2) &&
                                    InternalMemory.Read(t2 + 0x20, out uint matrixPtr))
                                {
                                    // Verify position offset (0x60 vs 0x80) if enemy appears big
                                    if (InternalMemory.Read(matrixPtr + 0x60, out Vector3 currentRoot))
                                    {
                                        Vector3 targetBone = GetTargetBonePosition(bestTarget);
                                        float depth = Vector3.Distance(Core.LocalMainCamera, targetBone);
                                        Vector3 targetHeadPos = ScreenToWorld(screenCenter, depth);
                                        Vector3 offset = targetHeadPos - targetBone;
                                        Vector3 newRootPos = currentRoot + offset;

                                        InternalMemory.Write(matrixPtr + 0x60, newRootPos);
                                    }
                                }
                            }
                        }

                        await Task.Delay(pullIntervalMs, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Clean exit when Stop() is called
                }
                catch (Exception ex)
                {
                    // Handle unexpected errors to keep thread alive
                    Console.WriteLine("TeliV2 Error: " + ex.Message);
                }
            }, cts.Token);
        }

        internal static void Stop()
        {
            if (!isRunning) return;

            isRunning = false; // Set this first to prevent loops
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
                cts = null;
            }
        }

        private static Vector3 GetTargetBonePosition(Entity entity)
        {
            if (Config.AimAtHead) return entity.Head;
            if (Config.AimAtBody) return entity.Spine;
            if (Config.AimAtHip) return entity.Hip;
            return entity.Head;
        }

        private static Vector3 ScreenToWorld(Vector2 screen, float depth)
        {
            float x = (2f * screen.X) / Core.Width - 1f;
            float y = 1f - (2f * screen.Y) / Core.Height;

            Vector4 ndc = new(x, y, 1f, 1f);

            if (!Matrix4x4.Invert(Core.CameraMatrix, out Matrix4x4 inv))
                return Core.LocalMainCamera;

            Vector4 world = Vector4.Transform(ndc, inv);
            if (world.W != 0f)
                world /= world.W;

            Vector3 cam = Core.LocalMainCamera;
            Vector3 dir = Vector3.Normalize(new Vector3(world.X, world.Y, world.Z) - cam);

            return cam + dir * depth;
        }
    }
}