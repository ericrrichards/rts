using System;
using System.Drawing;
using System.Threading;

namespace Core {
    public class HeightMapEditorSelection {
        private Rectangle _selectRect;
        private readonly HeightMapRenderer _renderer;
        public Rectangle SelectionRect { get { return _selectRect; } set { _selectRect = value; } }

        public HeightMapEditorSelection(HeightMap hm, HeightMapRenderer r) {
            _selectRect = new Rectangle(hm.Size.X / 2 - 5, hm.Size.Y / 2 - 5, 10, 10);
            _renderer = r;
        }
        // Editor functions
        public void MoveRect(Direction dir) {
            switch (dir) {
                case Direction.Left:
                    _selectRect.X--;
                    break;
                case Direction.Right:
                    _selectRect.X++;
                    break;
                case Direction.Up:
                    _selectRect.Y--;
                    break;
                case Direction.Down:
                    _selectRect.Y++;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("dir");
            }
            Thread.Sleep(100);
            _renderer.CreateParticles();
        }
    }
}