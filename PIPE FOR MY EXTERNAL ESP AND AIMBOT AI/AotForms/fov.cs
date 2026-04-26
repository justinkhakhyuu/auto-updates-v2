using AotForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AotForms
{
    internal static class FOV360
    {
        private static Thread aimbotextreme;
        private static CancellationTokenSource cancellationTokenSource;

        internal static void Start()
        {
            cancellationTokenSource = new CancellationTokenSource();
            aimbotextreme = new Thread(() => Work(cancellationTokenSource.Token));
            aimbotextreme.IsBackground = true;
            aimbotextreme.Start();
        }

        internal static void Stop()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
            }
        }

        internal static void Work(CancellationToken cancellationToken)
        {
            Entity target = null;
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            while (true)
            {
                if (!Config.FOV360)
                {
                    Thread.Sleep(Config.AIMBOTDELAY);
                    continue;
                }

                if ((WinAPI.GetAsyncKeyState(Config.AimbotKey) & 0x8000) == 0)
                {
                    Thread.Sleep(Config.AIMBOTDELAY);
                    continue;
                }

                if (Core.Width == -1 || Core.Height == -1) continue;
                if (!Core.HaveMatrix) continue;

                float minDistance = float.MaxValue;
                target = null;

                foreach (var entity in Core.Entities.Values)
                {
                    if (!entity.IsKnown || entity.IsDead || (Config.IgnoreKnocked && entity.IsKnocked))
                        continue;

                    float playerDistance = Vector3.Distance(Core.LocalMainCamera, entity.Head);
                    if (playerDistance > 300) continue;

                    if (playerDistance < minDistance)
                    {
                        minDistance = playerDistance;
                        target = entity;
                    }
                }

                if (target != null)
                {
                    // استخدم Quaternion لكتابة التوجيه بدقة للهدف (الرأس)
                    Quaternion playerLook = MathUtils.GetRotationToLocation(target.Head, 0.1f, Core.LocalMainCamera);
                    InternalMemory.Write(Core.LocalPlayer + Offsets.SientAim, playerLook);
                }

                while (stopwatch.ElapsedMilliseconds < Config.AIMBOTDELAY)
                {
                    Thread.Sleep(1);
                }
                stopwatch.Restart();
            }
        }
    }
}