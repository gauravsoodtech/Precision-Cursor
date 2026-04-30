using System;
using System.Drawing;

namespace PrecisionCursor.Core
{
    public static class LineSnapper
    {
        private const double UnitDiagonal = 0.70710678118654757;
        private const double TieTolerance = 0.000000001;

        private static readonly Direction[] Directions =
        {
            new Direction(1.0, 0.0),
            new Direction(0.0, 1.0),
            new Direction(-1.0, 0.0),
            new Direction(0.0, -1.0),
            new Direction(UnitDiagonal, UnitDiagonal),
            new Direction(-UnitDiagonal, UnitDiagonal),
            new Direction(-UnitDiagonal, -UnitDiagonal),
            new Direction(UnitDiagonal, -UnitDiagonal)
        };

        public static Point Snap(Point anchor, Point current)
        {
            PointF snapped = Snap(
                new PointF(anchor.X, anchor.Y),
                new PointF(current.X, current.Y));

            return new Point(RoundToInt(snapped.X), RoundToInt(snapped.Y));
        }

        public static PointF Snap(PointF anchor, PointF current)
        {
            double dx = current.X - anchor.X;
            double dy = current.Y - anchor.Y;

            if (Math.Abs(dx) < TieTolerance && Math.Abs(dy) < TieTolerance)
            {
                return current;
            }

            Projection best = Project(anchor, dx, dy, Directions[0]);

            for (int i = 1; i < Directions.Length; i++)
            {
                Projection candidate = Project(anchor, dx, dy, Directions[i]);

                if (candidate.DistanceSquared + TieTolerance < best.DistanceSquared)
                {
                    best = candidate;
                }
            }

            return new PointF((float)best.X, (float)best.Y);
        }

        private static Projection Project(PointF anchor, double dx, double dy, Direction direction)
        {
            double magnitude = dx * direction.X + dy * direction.Y;
            double projectedDx = magnitude * direction.X;
            double projectedDy = magnitude * direction.Y;
            double rejectedDx = dx - projectedDx;
            double rejectedDy = dy - projectedDy;

            return new Projection(
                anchor.X + projectedDx,
                anchor.Y + projectedDy,
                rejectedDx * rejectedDx + rejectedDy * rejectedDy);
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

        private struct Projection
        {
            public Projection(double x, double y, double distanceSquared)
            {
                X = x;
                Y = y;
                DistanceSquared = distanceSquared;
            }

            public double X { get; private set; }

            public double Y { get; private set; }

            public double DistanceSquared { get; private set; }
        }
    }
}
