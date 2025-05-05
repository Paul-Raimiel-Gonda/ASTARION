using ASTARION;
using ASTARION.A_STAR;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace ASTARION
{
    class Program
    {
        const int CELL_SIZE = 100;
        static TestCase _currentTest;
        static MenuSystem _menu = new MenuSystem();
        static bool _inMenu = true;

        static void Main(string[] args)
        {
            Raylib.InitWindow(1000, 1000, "A* Pathfinding Tests");
            Raylib.SetTargetFPS(60);

            while (!Raylib.WindowShouldClose())
            {
                if (_inMenu)
                {
                    HandleMenuInput();
                }
                else
                {
                    _currentTest.Update();
                }

                RenderFrame();
            }

            Raylib.CloseWindow();
        }

        static void HandleMenuInput()
        {
            _menu.Update();
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_ENTER))
            {
                _inMenu = false;
                _currentTest = CreateSelectedTest(_menu.SelectedIndex);
                _currentTest.Initialize();
            }
        }

        static TestCase CreateSelectedTest(int index)
        {
            return index switch
            {
                0 => new ShortestPathTest(),
                1 => new SequentialTest(), // Implement similarly
                2 => new DynamicTest(),    // Implement similarly
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        static void RenderFrame()
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.WHITE);

            if (_inMenu)
            {
                _menu.Draw();
            }
            else
            {
                _currentTest.Draw(CELL_SIZE);
                DrawGlobalControls();
            }

            Raylib.EndDrawing();
        }

        static void DrawGlobalControls()
        {
            Raylib.DrawText("Press ESC to return to menu", 10, 970, 20, Color.BLACK);
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_ESCAPE))
            {
                _inMenu = true;
                _currentTest = null;
            }
        }
    }
}