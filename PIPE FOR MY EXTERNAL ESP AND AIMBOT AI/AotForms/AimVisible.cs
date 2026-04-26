using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace AotForms
{
    internal class AimVisible
    {
        internal static void Work()
        {
            while (true)
            {
                if (!Config.AimbotVisible)
                {
                    Thread.Sleep(5);
                    continue;
                }

                if (Core.Width == -1 || Core.Height == -1 || !Core.HaveMatrix)
                {
                    Thread.Sleep(5);
                    continue;
                }

                Entity target = FindBestTarget();
                if (target != null)
                {
                    AimAtTarget(target);
                }

                Thread.Sleep(5); // prevent 100% CPU usage
            }
        }

        private static Entity FindBestTarget()
        {
            Entity bestTarget = null;
            float closestDistance = float.MaxValue;
            var screenCenter = new Vector2(Core.Width / 2f, Core.Height / 2f);

            // Filter nearby entities
            var nearbyEntities = Core.Entities.Values
                .Where(entity =>
                    !entity.IsDead &&
                    (!Config.IgnoreKnocked || !entity.IsKnocked) &&
                    Vector3.Distance(Core.LocalMainCamera, entity.Head) <= Config.AimBotMaxDistance)
                .ToList();

            foreach (var entity in nearbyEntities)
            {
                var head2D = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
                if (head2D.X < 1 || head2D.Y < 1) continue; // off-screen

                var crosshairDistance = Vector2.Distance(screenCenter, head2D);

                // Use both FOV toggle (bool) and FOV radius (float)
                if (crosshairDistance < closestDistance && crosshairDistance <= Config.AimFov)

                {
                    closestDistance = crosshairDistance;
                    bestTarget = entity;
                }

                if (entity.IsDead) continue;
                if (Config.IgnoreKnocked && entity.IsKnocked) continue;

                float playerDistance = Vector3.Distance(Core.LocalMainCamera, entity.Head);
                if (playerDistance > Config.AimBotMaxDistance) continue;

                if (crosshairDistance < closestDistance && crosshairDistance <= Config.AimFov)

                {
                    closestDistance = crosshairDistance;
                    bestTarget = entity;
                }
            }

            return bestTarget;
        }

        private static void AimAtTarget(Entity target)
        {
            if (target == null || target.Address == 0) return;

            uint m_HeadCollider;
            var rHeadCollider = InternalMemory.Read<uint>(target.Address + 0x4A8, out m_HeadCollider);
            if (!rHeadCollider || m_HeadCollider == 0) return;

            const int repeatCount = 2;

            for (int i = 0; i < repeatCount; i++)
            {
                InternalMemory.Write(target.Address + 0x54, m_HeadCollider);
            }
        }
    }
}