using System;
using Raylib_cs;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASTARION.A_STAR
{
    public class MenuSystem
    {
        private int _selectedIndex;
        private readonly List<string> _options = new List<string>
        {
            "1. Shortest Path Test",
            "2. Sequential Test",
            "3. Dynamic Paths Test"
        };

        public void Update()
        {
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_DOWN))
                _selectedIndex = (_selectedIndex + 1) % _options.Count;

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_UP))
                _selectedIndex = (_selectedIndex - 1 + _options.Count) % _options.Count;
        }

        public void Draw()
        {
            const int startY = 300;
            const int optionHeight = 50;

            for (int i = 0; i < _options.Count; i++)
            {
                Color color = i == _selectedIndex ? Color.RED : Color.BLACK;
                Raylib.DrawText(_options[i], 300, startY + i * optionHeight, 40, color);
            }

            Raylib.DrawText("Use ARROW KEYS to select, ENTER to confirm", 250, 600, 20, Color.BLACK);
        }

        public int SelectedIndex => _selectedIndex;
    }
}
