using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;
using SlimDX;

namespace Hello {
    class HeightMapExample1 :App {
        private float _angle;
        private int _image;

        public HeightMapExample1() {
            _angle = 0.0f;
            _image = 0;
        }

        public override Result Init(int width, int height, bool windowed) {
            CreateWindow(width, height);
            throw new NotImplementedException();
        }

        public override Result Update(float dt) {
            throw new NotImplementedException();
        }

        public override Result Render() {
            throw new NotImplementedException();
        }

        public override Result Cleanup() {
            throw new NotImplementedException();
        }

        public override string GetName() {
            return "HeightMap Example 1";
        }
    }
}
