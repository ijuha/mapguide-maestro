﻿#region Disclaimer / License

// Copyright (C) 2010, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
//

#endregion Disclaimer / License

using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems.Transformations;
using System;

namespace OSGeo.MapGuide.MaestroAPI.CoordinateSystem
{
    internal class ActualCoordinateSystem : MeterBasedCoordinateSystem
    {
        private readonly ICoordinateTransformation m_transform;
        private const string XY_M = "LOCAL_CS[\"Non-Earth (Meter)\",LOCAL_DATUM[\"Local Datum\",0],UNIT[\"Meter\", 1],AXIS[\"X\",EAST],AXIS[\"Y\",NORTH]]"; //NOXLATE

        internal ActualCoordinateSystem(ProjNet.CoordinateSystems.CoordinateSystem coordinateSystem)
        {
            if (coordinateSystem == null)
                throw new ArgumentNullException(nameof(coordinateSystem)); //NOXLATE

            var f = new CoordinateTransformationFactory();
            var cf = new ProjNet.CoordinateSystems.CoordinateSystemFactory();

            m_transform = f.CreateFromCoordinateSystems(coordinateSystem, cf.CreateFromWkt(XY_M));
        }

        protected override double CalculateScale(Envelope bbox, System.Drawing.Size size)
        {
            double[] points = m_transform.MathTransform.Transform(new double[] { bbox.MinX, bbox.MinY, bbox.MaxX, bbox.MaxY });
            var localEnv = new Envelope(points[0], points[2], points[1], points[3]);
            return base.CalculateScale(localEnv, size);
        }

        protected override Envelope AdjustBoundingBox(Envelope bbox, double scale, System.Drawing.Size size)
        {
            double[] points = m_transform.MathTransform.Transform(new double[] { bbox.MinX, bbox.MinY, bbox.MaxX, bbox.MaxY });
            var localEnv = new Envelope(points[0], points[2], points[1], points[3]);
            localEnv = base.AdjustBoundingBox(localEnv, scale, size);
            points = m_transform.MathTransform.Inverse().Transform(new double[] { localEnv.MinX, localEnv.MinY, localEnv.MaxX, localEnv.MaxY });
            return new Envelope(points[0], points[2], points[1], points[3]);
        }

        protected override double DistanceInMeters(Point p1, Point p2)
        {
            double[] points = m_transform.MathTransform.Transform(new double[] { p1.X, p1.Y, p2.X, p2.Y });
            return base.DistanceInMeters(new Point(points[0], points[1]), new Point(points[2], points[3]));
        }
    }
}