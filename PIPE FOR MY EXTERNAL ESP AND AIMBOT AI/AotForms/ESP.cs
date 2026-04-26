using ClickableTransparentOverlay;

using ImGuiNET;

using System.Numerics;

using System.Security.Cryptography.Xml;

using Vortice;

using static AotForms.WinAPI;



namespace AotForms

{

    internal class ESP : ClickableTransparentOverlay.Overlay

    {

        private const short DefaultMaxHealth = 200; // Default maximum health



        protected override unsafe void Render()

        {

           

            if (!Core.HaveMatrix) return;

            CreateHandle();





            string text = "BLACK CORPS </>";

            var windowWidth = Core.Width;

            var windowHeight = Core.Height;

            var textSize = ImGui.CalcTextSize(text);

            var textPosX = (windowWidth - textSize.X) / 2;

            var textPosY = 80;

            uint textColor = ImGui.ColorConvertFloat4ToU32(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));

            uint shadowColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.0f, 0.0f, 0.0f, 0.5f));

            var drawList = ImGui.GetForegroundDrawList();



            // Draw main title with shadow

            ImGui.GetForegroundDrawList().AddText(new Vector2(textPosX + 1, textPosY + 1), shadowColor, text);

            ImGui.GetForegroundDrawList().AddText(new Vector2(textPosX, textPosY), textColor, text);



           



           

            int playerCounter = 1;

            int enemyCount = 0; // Initialize enemy count

            var tmp = Core.Entities;





            if (Config.FOVEnabled)

            {

                DrawFOVCircle(Config.AimFov);

            }

            foreach (var entity in tmp.Values)

            {



                if (entity.IsDead || !entity.IsKnown)

                {

                    continue;

                }



                var dist = Vector3.Distance(Core.LocalMainCamera, entity.Head);



                if (dist > Config.espran) continue;

                enemyCount++; 

                var headScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);



                var bottomScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Root, Core.Width, Core.Height);



                if (headScreenPos.X < 1 || headScreenPos.Y < 1) continue;

                if (bottomScreenPos.X < 1 || bottomScreenPos.Y < 1) continue;



                float CornerHeight = Math.Abs(headScreenPos.Y - bottomScreenPos.Y);

                float CornerWidth = (float)(CornerHeight * 0.65);

                if (Config.ESPLine)

                {

                    // Check if the entity is "Knocked"

                    uint lineColor;



                    if (entity.IsKnocked)

                    {

                        lineColor = ColorToUint32(Color.Red); // Red for "Knocked" state

                    }

                    else

                    {

                        lineColor = ColorToUint32(Config.ESPLineColor); // Normal color

                    }



                    // Draw the line with the appropriate color

                    ImGui.GetBackgroundDrawList().AddLine(

                        new Vector2(Core.Width / 2f, 0f),

                        headScreenPos,

                        lineColor,

                        1f

                    );

                }

                if (Config.ESPFillBox)

                {

                    Color topColor = Color.FromArgb((int)(0.1f * 255), Config.ESPFillBoxColor);

                    Color bottomColor = Color.FromArgb((int)(0.75f * 255), Config.ESPFillBoxColor);



                    DrawGradientBox(

                        headScreenPos.X - (CornerWidth / 2),

                        headScreenPos.Y,

                        CornerWidth,

                        CornerHeight,

                        topColor,

                        bottomColor

                    );

                }

                

                if (Config.ESPBox)

                {

                    uint boxColor = ColorToUint32(Config.ESPBoxColor);

                    // Use the Config.ESPLineThickness for line thickness

                    DrawBox(headScreenPos.X - (CornerWidth / 2), headScreenPos.Y, CornerWidth, CornerHeight, boxColor, Config.ESPBoxThickness);

                }

                if (Config.ESPSkeleton)

                {

                    DrawSkeleton(entity);

                }



                if (Config.ESPBox2)

                {

                    uint boxColor = ColorToUint32(Config.ESPBoxColor);



                    DrawCorneredBox(headScreenPos.X - (CornerWidth / 2), headScreenPos.Y, CornerWidth, CornerHeight, boxColor, 1f);

                }

                string namefuck = $"";

                float namefuckTextWidths = namefuck.Length * 6f;

                var namePosition = new Vector2(headScreenPos.X - (CornerWidth / 2), headScreenPos.Y - 20);



                string customName = $"Player{playerCounter}";

                playerCounter++;

                var nameText = string.IsNullOrWhiteSpace(entity.Name) ? "BOT" : entity.Name;

                var nameSize = ImGui.CalcTextSize($"      {MathF.Round(dist)}M" + nameText);

                if (Config.ESPName)

                {



                    // Clean the name text by removing unsupported characters

                    string cleanName = RemoveUnsupportedCharacters(nameText);



                    // Draw the cleaned name in white

                    drawList.AddText(namePosition, ColorToUint32(Color.White), cleanName);



                    // Calculate the size of the cleaned nameText to determine where to position the distance text

                    Vector2 nameTextSize = ImGui.CalcTextSize(cleanName);



                    // Adjust the position for the distance text, shifting it to the right of the nameText

                    Vector2 distancePosition = new Vector2(namePosition.X + nameTextSize.X, namePosition.Y);



                    // Draw the distance in yellow

                    drawList.AddText(distancePosition, ColorToUint32(Color.Yellow), $"     {MathF.Round(dist)}M");



                }



                if (Config.ESPDistance)

                 {

                string distanceText = $"{dist.ToString("F1")}m"; // 1 decimal place



                float estimatedTextWidth = distanceText.Length * 6f;



                Vector2 distancePosition = new Vector2(bottomScreenPos.X + 1 - (estimatedTextWidth / 2), bottomScreenPos.Y + 5f);



                ImGui.GetForegroundDrawList().AddText(distancePosition, ColorToUint32(Config.ESPDistanceColor), distanceText);

                }



                if (Config.ESPHealth)

                {

                    DrawHealthBar(entity.Health, 200, headScreenPos.X - (CornerWidth / 2) - 5, headScreenPos.Y, CornerHeight);

                }

            }

        }

        public void DrawGlowingBall(Vector2 position, Color color, float radius)

        {

            var drawList = ImGui.GetBackgroundDrawList();

            uint ballColor = ColorToUint32(color);



            for (int i = 0; i < 5; i++)

            {

                float glowRadius = radius + (i * 2);

                float alpha = 1.0f - (i * 0.2f);



                drawList.AddCircleFilled(

                    position,

                    glowRadius,

                    ImGui.ColorConvertFloat4ToU32(new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, alpha)),

                    50

                );

            }



            drawList.AddCircleFilled(position, radius, ballColor, 50);

        }

        private string RemoveUnsupportedCharacters(string input)

        {

            // Allow only printable ASCII characters (basic English letters, numbers, and symbols)

            return new string(input.Where(c => c >= 32 && c <= 126).ToArray());

        }



        public void DrawGradientBox(float X, float Y, float W, float H, Color topColor, Color bottomColor)

        {

            var vList = ImGui.GetForegroundDrawList();



            int slices = 50; // Number of slices for gradient

            float sliceHeight = H / slices;



            for (int i = 0; i < slices; i++)

            {

                float t = (float)i / slices; // Interpolation factor

                Color sliceColor = Color.FromArgb(

                    (int)(topColor.A * (1 - t) + bottomColor.A * t), // Interpolating opacity

                    (int)(topColor.R * (1 - t) + bottomColor.R * t), // Interpolating Red

                    (int)(topColor.G * (1 - t) + bottomColor.G * t), // Interpolating Green

                    (int)(topColor.B * (1 - t) + bottomColor.B * t)  // Interpolating Blue

                );



                uint sliceColorUint = ColorToUint32(sliceColor);



                // Draw each slice

                vList.AddRectFilled(

                    new Vector2(X, Y + i * sliceHeight),

                    new Vector2(X + W, Y + (i + 1) * sliceHeight),

                    sliceColorUint

                );

            }

        }

        private void DrawBox(float X, float Y, float W, float H, uint color, float thickness)

        {

            var drawList = ImGui.GetForegroundDrawList();

            drawList.AddLine(new Vector2(X, Y), new Vector2(X + W, Y), color, thickness); // Top line

            drawList.AddLine(new Vector2(X, Y + H), new Vector2(X + W, Y + H), color, thickness); // Bottom line

            drawList.AddLine(new Vector2(X, Y), new Vector2(X, Y + H), color, thickness); // Left line

            drawList.AddLine(new Vector2(X + W, Y), new Vector2(X + W, Y + H), color, thickness); // Right line

        }

        void DrawESPHeadLine(Vector2 head, Vector2 aim)

        {

            // Calculate the maximum distance we want the line to go

            float maxDistance = 200f; // Maximum distance the line can extend



            // Calculate the vector from the head to the aim

            Vector2 direction = aim - head;

            float distance = direction.Length();



            // If the distance is greater than the max distance, cap it

            if (distance > maxDistance)

            {

                direction = Vector2.Normalize(direction) * maxDistance;

                aim = head + direction; // Set the aim point to the capped value

            }



            // Draw the line to the new aim

            var drawList = ImGui.GetForegroundDrawList();

            drawList.AddLine(head, aim, ColorToUint32(Config.ESPLineColor), 1f);

        }

        public void DrawCorneredBox(float X, float Y, float W, float H, uint color, float thickness)

        {

            var drawList = ImGui.GetForegroundDrawList();



            float lineW = W / 3;

            float lineH = H / 3;



            drawList.AddLine(new Vector2(X, Y - thickness / 2), new Vector2(X, Y + lineH), color, thickness);

            drawList.AddLine(new Vector2(X - thickness / 2, Y), new Vector2(X + lineW, Y), color, thickness);

            drawList.AddLine(new Vector2(X + W - lineW, Y), new Vector2(X + W + thickness / 2, Y), color, thickness);

            drawList.AddLine(new Vector2(X + W, Y - thickness / 2), new Vector2(X + W, Y + lineH), color, thickness);

            drawList.AddLine(new Vector2(X, Y + H - lineH), new Vector2(X, Y + H + thickness / 2), color, thickness);

            drawList.AddLine(new Vector2(X - thickness / 2, Y + H), new Vector2(X + lineW, Y + H), color, thickness);

            drawList.AddLine(new Vector2(X + W - lineW, Y + H), new Vector2(X + W + thickness / 2, Y + H), color, thickness);

            drawList.AddLine(new Vector2(X + W, Y + H - lineH), new Vector2(X + W, Y + H + thickness / 2), color, thickness);

        }

        private void DrawLine(ImDrawListPtr drawList, Vector2 startPos, Vector2 endPos, uint color)

        {

            if (startPos.X > 0 && startPos.Y > 0 && endPos.X > 0 && endPos.Y > 0)

            {

                drawList.AddLine(startPos, endPos, color, 1f); // Adjust thickness as needed

            }

        }

        private void DrawSkeleton(Entity entity)

        {

            var drawList = ImGui.GetForegroundDrawList();

            uint lineColor = ColorToUint32(Config.ESPSkeletonColor); // Color for the skeleton lines

            uint circleColor = ColorToUint32(Color.Red); // Color for the circle around the head



            // Convert entity positions to screen space

            var headScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);

            var leftWristScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightWrist, Core.Width, Core.Height); // Adjust as per actual mapping

            var spineScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Spine, Core.Width, Core.Height);

            var hipScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Hip, Core.Width, Core.Height); // Adjust as per actual mapping

            var rootScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Root, Core.Width, Core.Height);

            var rightCalfScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightCalf, Core.Width, Core.Height);

            var leftCalfScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftCalf, Core.Width, Core.Height);

            var rightFootScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightFoot, Core.Width, Core.Height);

            var leftFootScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftFoot, Core.Width, Core.Height);

            var rightWristScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightWrist, Core.Width, Core.Height);

            var leftHandScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftHand, Core.Width, Core.Height);

            var leftShoulderScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftShoulder, Core.Width, Core.Height);

            var rightShoulderScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightShoulder, Core.Width, Core.Height);

            var rightWristJointScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightWristJoint, Core.Width, Core.Height);

            var leftWristJointScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftWristJoint, Core.Width, Core.Height);

            var leftElbowScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftElbow, Core.Width, Core.Height);

            var rightElbowScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightElbow, Core.Width, Core.Height); // Adjust if needed



            // Draw skeleton lines





            DrawLine(drawList, spineScreenPos, rightShoulderScreenPos, lineColor); // Spine to Right Shoulder

            DrawLine(drawList, spineScreenPos, hipScreenPos, lineColor);// Spine to hip





            DrawLine(drawList, spineScreenPos, leftShoulderScreenPos, lineColor); // Spine to Left Shoulder

            DrawLine(drawList, leftShoulderScreenPos, rightElbowScreenPos, lineColor); // Left Shoulder to Left Elbow

            DrawLine(drawList, leftElbowScreenPos, rightWristJointScreenPos, lineColor); // Left Elbow to Left Wrist Joint

            // Left Wrist Joint to Left Wrist



            DrawLine(drawList, rightShoulderScreenPos, leftElbowScreenPos, lineColor); // Right Shoulder to Left Elbow

                                                                                       //  DrawLine(drawList, rightElbowScreenPos, leftWristJointScreenPos, lineColor); // Right Elbow to Left Wrist Joint

                                                                                       // Right Wrist Joint to Left Wrist



            DrawLine(drawList, hipScreenPos, rightFootScreenPos, lineColor);// Hip to Right Calf

            DrawLine(drawList, hipScreenPos, leftFootScreenPos, lineColor);// Hip to Left Calf





            // Draw a small circle around the head

            float distance = entity.Distance; // Assume entity.Distance is the distance to the player in game units



            // Calculate the circle radius based on distance (e.g., closer = larger, farther = smaller)

            float baseRadius = 50.0f; // Adjust this base value as needed

            float circleRadius = baseRadius / distance;



            // Draw the circle on the head if the head is visible on screen

            if (headScreenPos.X > 0 && headScreenPos.Y > 0)

            {

                drawList.AddCircle(headScreenPos, circleRadius, circleColor, 30); // 30 segments for the circle

            }



            // Add additional code here to draw the rest of the skeleton using the updated bone positions

        }



        public void DrawHealthBar(short health, short maxHealth, float X, float Y, float height)

        {

         

            var vList = ImGui.GetForegroundDrawList();

            float healthPercentage = (float)health / maxHealth;

            float barHeight = height * healthPercentage;



            vList.AddRectFilled(new Vector2(X, Y), new Vector2(X + 3, Y + height), ColorToUint32(Color.Black));

            vList.AddRectFilled(new Vector2(X, Y + (height - barHeight)), new Vector2(X + 3, Y + height), ColorToUint32(Config.ESPHealthColor));

        }

        public void DrawFOVCircle(float radius)

        {

            var drawList = ImGui.GetBackgroundDrawList();

            var center = new Vector2(Core.Width / 2f, Core.Height / 2f);

            uint color = ColorToUint32(Config.FOVColor);



            drawList.AddCircle(center, radius, color, 0, 1f);

        }

        static uint ColorToUint32(Color color)

        {

            return ImGui.ColorConvertFloat4ToU32(new Vector4(

                (float)(color.R / 255.0),

                (float)(color.G / 255.0),

                (float)(color.B / 255.0),

                (float)(color.A / 255.0)));

        }



        void CreateHandle()

        {

            RECT rect;

            GetWindowRect(Core.Handle, out rect);

            int x = rect.Left;

            int y = rect.Top;

            int width = rect.Right - rect.Left;

            int height = rect.Bottom - rect.Top;

            ImGui.SetWindowSize(new Vector2((float)width, (float)height));

            Size = new Size(width, height);

            Position = new Point(x, y);

            Core.Width = width;

            Core.Height = height;

        }

    }

}

