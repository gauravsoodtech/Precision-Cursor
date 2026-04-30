using System;
using System.Drawing;

namespace PrecisionCursor.Core
{
    public sealed class RelativeLineLock
    {
        private const double UnitDiagonal = 0.70710678118654757;
        private const double DirectionDecisionPixels = 2.0;
        private const double TurnMemoryDecay = 0.65;
        private const double SwitchAdvantagePixels = 1.25;

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

        private double _positionX;
        private double _positionY;
        private double _pendingX;
        private double _pendingY;
        private double _turnX;
        private double _turnY;
        private Direction? _currentDirection;

        public RelativeLineLock(Point start)
            : this(new PointF(start.X, start.Y))
        {
        }

        public RelativeLineLock(PointF start)
        {
            Reset(start);
        }

        public void Reset(Point start)
        {
            Reset(new PointF(start.X, start.Y));
        }

        public void Reset(PointF start)
        {
            _positionX = start.X;
            _positionY = start.Y;
            _pendingX = 0.0;
            _pendingY = 0.0;
            _turnX = 0.0;
            _turnY = 0.0;
            _currentDirection = null;
        }

        public Point ApplyDelta(int deltaX, int deltaY)
        {
            PointF point = ApplyDelta((float)deltaX, (float)deltaY);
            return new Point(RoundToInt(point.X), RoundToInt(point.Y));
        }

        public PointF ApplyDelta(float deltaX, float deltaY)
        {
            if (Math.Abs(deltaX) < float.Epsilon && Math.Abs(deltaY) < float.Epsilon)
            {
                return CurrentPosition;
            }

            if (!_currentDirection.HasValue)
            {
                _pendingX += deltaX;
                _pendingY += deltaY;

                if (SquaredLength(_pendingX, _pendingY) < DirectionDecisionPixels * DirectionDecisionPixels)
                {
                    return CurrentPosition;
                }

                _currentDirection = FindClosestDirection(_pendingX, _pendingY);
                ApplyProjectedDelta(_pendingX, _pendingY, _currentDirection.Value);
                _turnX = _pendingX;
                _turnY = _pendingY;
                _pendingX = 0.0;
                _pendingY = 0.0;
                return CurrentPosition;
            }

            Direction direction = UpdateDirection(deltaX, deltaY);
            ApplyProjectedDelta(deltaX, deltaY, direction);
            return CurrentPosition;
        }

        private PointF CurrentPosition
        {
            get { return new PointF((float)_positionX, (float)_positionY); }
        }

        private Direction UpdateDirection(double deltaX, double deltaY)
        {
            Direction current = _currentDirection.Value;

            if (SquaredLength(deltaX, deltaY) >= DirectionDecisionPixels * DirectionDecisionPixels)
            {
                Direction immediateCandidate = FindClosestDirection(deltaX, deltaY);

                if (ShouldSwitch(current, immediateCandidate, deltaX, deltaY))
                {
                    _currentDirection = immediateCandidate;
                    _turnX = deltaX;
                    _turnY = deltaY;
                    return immediateCandidate;
                }
            }

            _turnX = _turnX * TurnMemoryDecay + deltaX;
            _turnY = _turnY * TurnMemoryDecay + deltaY;

            if (SquaredLength(_turnX, _turnY) < DirectionDecisionPixels * DirectionDecisionPixels)
            {
                return current;
            }

            Direction candidate = FindClosestDirection(_turnX, _turnY);

            if (candidate.Equals(current))
            {
                return current;
            }

            if (ShouldSwitch(current, candidate, _turnX, _turnY))
            {
                _currentDirection = candidate;
                _turnX = deltaX;
                _turnY = deltaY;
                return candidate;
            }

            return current;
        }

        private static bool ShouldSwitch(Direction current, Direction candidate, double deltaX, double deltaY)
        {
            if (candidate.Equals(current))
            {
                return false;
            }

            double currentProjection = Dot(deltaX, deltaY, current);
            double candidateProjection = Dot(deltaX, deltaY, candidate);

            return currentProjection < 0.0 || candidateProjection > currentProjection + SwitchAdvantagePixels;
        }

        private void ApplyProjectedDelta(double deltaX, double deltaY, Direction direction)
        {
            double projection = Dot(deltaX, deltaY, direction);

            _positionX += projection * direction.X;
            _positionY += projection * direction.Y;
        }

        private static Direction FindClosestDirection(double deltaX, double deltaY)
        {
            Direction best = Directions[0];
            double bestProjection = Dot(deltaX, deltaY, best);

            for (int i = 1; i < Directions.Length; i++)
            {
                double projection = Dot(deltaX, deltaY, Directions[i]);

                if (projection > bestProjection)
                {
                    best = Directions[i];
                    bestProjection = projection;
                }
            }

            return best;
        }

        private static double Dot(double deltaX, double deltaY, Direction direction)
        {
            return deltaX * direction.X + deltaY * direction.Y;
        }

        private static double SquaredLength(double deltaX, double deltaY)
        {
            return deltaX * deltaX + deltaY * deltaY;
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
