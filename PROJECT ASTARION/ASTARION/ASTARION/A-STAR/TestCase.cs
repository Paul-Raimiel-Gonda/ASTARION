using System;
using Raylib_cs;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ASTARION.A_STAR
{
    public abstract class TestCase
    {
        public Grid Grid { get; protected set; }
        public Node Start { get; protected set; }
        public List<Node> Targets { get; protected set; } // Changed to list
        public List<Node> Path { get; protected set; }
        public bool IsOptimal { get; protected set; }
        public long ExecutionTimeMs { get; protected set; }
        public long MemoryUsageMB { get; protected set; }
        protected int _currentStep; // Added for shared step tracking

        public abstract void Initialize();
        public abstract void Update();
        public abstract void Draw(int cellSize);

        protected void CalculateMetrics()
        {
            var process = Process.GetCurrentProcess();
            MemoryUsageMB = process.WorkingSet64 / 1024 / 1024;
        }
    }
}
