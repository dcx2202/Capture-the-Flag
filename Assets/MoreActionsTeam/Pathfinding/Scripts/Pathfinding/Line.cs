using UnityEngine;

namespace ThunderRunners.Pathfinding.Scripts.Pathfinding
{
    /// <summary>
    /// Line used for the turn boundaries in the pathfinding algorithm
    /// </summary>
    public struct Line
    {
        /// <summary>
        /// Represents the vertical line gradient.
        /// </summary>
        private const float VerticalLineGradient = 1e5f; // 100000

        /// <summary>
        /// The gradient of this line
        /// </summary>
        private readonly float _gradient;
        /// <summary>
        /// Intercept point from this line to the perpendicular line in the point passed in the constructor
        /// </summary>
        private readonly float _yIntercept;
        /// <summary>
        /// Gradient of the Perpendicular line
        /// </summary>
        private readonly float _gradientPerpendicular;

        // we need two points on this line to use on the GetSide algorithm
        private Vector2 _pointOnLine1;
        private Vector2 _pointOnLine2;

        /// <summary>
        /// The side we are aproaching
        /// </summary>
        private readonly bool _approachSide;

        /// <summary>
        /// Creates a new line, given a point on the line and a point perpendicular to the line
        /// </summary>
        /// <param name="pointOnLine"></param>
        /// <param name="pointPerpendicularToLine"></param>
        public Line(Vector2 pointOnLine, Vector2 pointPerpendicularToLine)
        {
            // start by calculting the perpendicular gradient
            var dx = pointOnLine.x - pointPerpendicularToLine.x;
            var dy = pointOnLine.y - pointPerpendicularToLine.y;

            // if dx == 0, than it is a vertical line. Use the VerticalLineGradient to avoid division by 0.
            _gradientPerpendicular = dx == 0 ? VerticalLineGradient : dy / dx;

            // calculate the gradient: gradient * _perpendicularGradient = -1.
            _gradient = _gradientPerpendicular == 0 ? VerticalLineGradient : -1 / _gradientPerpendicular;

            // calculate y intercept (c = y - mx <=> c = m - mx)
            _yIntercept = pointOnLine.y - _gradient * pointOnLine.x;

            // get first point on line
            _pointOnLine1 = pointOnLine;
            // second point is just a little bit further away
            _pointOnLine2 = pointOnLine + new Vector2(1, _gradient);

            // We assume we arrive on the positive side. Booleans are initialized to false by default.
            // This initialization is done to avoid compilation error.
            // A struct must have all fields assigned before using the "this" 
            // keyword (meaning, calling a funcion using this.FunctionaName();).
            _approachSide = false;

            // we are leaving the this keyword explicit to showcase the compilation error. 
            // Try commenting the line assigning the approach side variable

            // get the correct approach side, the "pointPerpendicularToLine is the previous point on the path, so we will approach from there
            _approachSide = this.GetSide(pointPerpendicularToLine); 
        }

        /// <summary>
        /// Returns true if the given point p has crossed this line
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool HasCrossedLine(Vector2 p)
        {
            // it will return the same bool untill it has crossed the line.
            // Once it crossed, the sides will be diferent (on in positive and other on negative side)
            return GetSide(p) != _approachSide; 
        }

        /// <summary>
        /// Calculates the shortest distance between point P and this line.
        /// </summary>
        /// <returns></returns>
        public float DistanceFromPoint(Vector2 p)
        {
            // get the y intercept of the point P with our line.
            var yInterceptPerpendicular = p.y - _gradientPerpendicular * p.x;

            // get the X of intersection point
            // m1*x + c1 = m2*x + c2
            // x = (c2 - c1) / (m1 - m2)
            var intersectX = (yInterceptPerpendicular - _yIntercept) / (_gradient - _gradientPerpendicular);
            // get the Y intersection point ( y = mx + c)
            var intersectY = _gradient * intersectX + _yIntercept;

            return Vector2.Distance(p, new Vector2(intersectX, intersectY));
        }

        /// <summary>
        /// Vizualize the line with gizmos
        /// </summary>
        /// <param name="length"></param>
        public void DrawWithGizmos(float length)
        {
            var lineDir = new Vector3(1, 0, _gradient).normalized;
            var lineCenter = new Vector3(_pointOnLine1.x, 0, _pointOnLine1.y) + Vector3.up;

            Gizmos.DrawLine(lineCenter - lineDir * length / 20f, lineCenter + lineDir * length / 20f);
        }

        /// <summary>
        /// https://www.youtube.com/watch?v=KHuI9bXZS74
        /// 
        /// Determines which side point C lies of the line passing through points A and B
        /// sng((C.x - A.x) * (-B.y + A.y) + (C.y - A.y) * (B.x - A.x))
        /// if greater than 0, it is one side, if lesser than 0, it is the other side. If equals to 0, it lies on the line itself.
        /// 
        /// So if both (C.x - A.x) * (-B.y + A.y) > (C.y - A.y) * (B.x - A.x), we are on the positive side, if not we are on the negative side sides. (we combine negative and "on line" as the other side"
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private bool GetSide(Vector2 p)
        {
            return (p.x - _pointOnLine1.x) * (_pointOnLine2.y - _pointOnLine1.y) >
                   (p.y - _pointOnLine1.y) * (_pointOnLine2.x - _pointOnLine1.x);
        }
    }
}
