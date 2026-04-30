using System;
using System.Drawing;

namespace DpiAssistant.Core
{
    public sealed class AdaptiveLineLock
    {
        private const double UnitDiagonal = 0.70710678118654757;
        private const double MinimumMovementPixels = 0.75;
        private static readonly Direction[] Directions =
        {
            new Direction(1.0, 0.0),
            new Direction(-1.0, 0.0),
            new Direction(0.0, -1.0),
            new Direction(0.0, 1.0),
            new Direction(UnitDiagonal, UnitDiagonal),
            new Direction(-UnitDiagonal, UnitDiagonal),
            new Direction(-UnitDiagonal, -UnitDiagonal),
            new Direction(UnitDiagonal, -UnitDiagonal)
        };

        private PointF _segmentAnchor;
        private PointF _lastSnappedPoint;
        private Direction? _currentDirection;

        public AdaptiveLineLock(Point anchor)
            : this(new PointF(anchor.X, anchor.Y))
        {
        }

        public AdaptiveLineLock(PointF anchor)
        {
            Reset(anchor);
        }

        public void Reset(Point anchor)
        {
            Reset(new PointF(anchor.X, anchor.Y));
        }

        public void Reset(PointF anchor)
        {
            _segmentAnchor = anchor;
            _lastSnappedPoint = anchor;
            _currentDirection = null;
        }

        public Point Snap(Point rawPoint)
        {
            PointF snapped = Snap(new PointF(rawPoint.X, rawPoint.Y));
            return new Point(RoundToInt(snapped.X), RoundToInt(snapped.Y));
        }

        public PointF Snap(PointF rawPoint)
        {
            double intentDx = rawPoint.X - _lastSnappedPoint.X;
            double intentDy = rawPoint.Y - _lastSnappedPoint.Y;

            if (SquaredLength(intentDx, intentDy) < MinimumMovementPixels * MinimumMovementPixels)
            {
                return _lastSnappedPoint;
            }

            Direction intendedDirection = FindClosestDirection(intentDx, intentDy);

            if (!_currentDirection.HasValue || !_currentDirection.Value.Equals(intendedDirection))
            {
                _segmentAnchor = _lastSnappedPoint;
                _currentDirection = intendedDirection;
            }

            PointF snapped = ProjectOntoDirection(_segmentAnchor, rawPoint, _currentDirection.Value);
            _lastSnappedPoint = snapped;
            return snapped;
        }

        private static Direction FindClosestDirection(double dx, double dy)
        {
            Direction best = Directions[0];
            double bestProjection = Dot(dx, dy, best);

            for (int i = 1; i < Directions.Length; i++)
            {
                double projection = Dot(dx, dy, Directions[i]);

                if (projection > bestProjection)
                {
                    best = Directions[i];
                    bestProjection = projection;
                }
            }

            return best;
        }

        private static PointF ProjectOntoDirection(PointF anchor, PointF point, Direction direction)
        {
            double dx = point.X - anchor.X;
            double dy = point.Y - anchor.Y;
            double projection = Dot(dx, dy, direction);

            return new PointF(
                (float)(anchor.X + projection * direction.X),
                (float)(anchor.Y + projection * direction.Y));
        }

        private static double Dot(double dx, double dy, Direction direction)
        {
            return dx * direction.X + dy * direction.Y;
        }

        private static double SquaredLength(double dx, double dy)
        {
            return dx * dx + dy * dy;
        }

        private static int RoundToInt(float value)
        {
            return (int)Math.Round(value, MidpointRounding.AwayFromZero);
        }

        private struct Direction
        {
            public Direction(double x, double y)
            {
                X = x;
                Y = y;
            }

            public double X { get; private set; }

            public double Y { get; private set; }
        }
    }
}
