using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.SqlServer.Types;
using System.Data.Spatial;
namespace NKD.Helpers
{
    public static class SpatialHelper
    {
        public static DbGeography GetCentre(this DbGeography spatial)
        {
            if (spatial == null)
                return null;
            return DbGeography.FromText(SqlGeography.Parse(spatial.AsText()).MakeValid().EnvelopeCenter().ToString());
        }

        public static DbGeography GetCentre(this SqlGeography spatial)
        {
            if (spatial == null)
                return null;
            return DbGeography.FromText(spatial.MakeValid().EnvelopeCenter().ToString());
        }


        //LONGITUDE=item1, LATITUDE=item2
        public static SqlGeography CreatePolygon(this List<Tuple<double, double>> coordinates, int srid = 4326)
        {
            var list = coordinates.Distinct().ToList();
            var b = new SqlGeographyBuilder();
            b.SetSrid(srid);
            b.BeginGeography(OpenGisGeographyType.Polygon);
            b.BeginFigure(list[0].Item2, list[0].Item1);

            for (var i = 1; i < list.Count; i++)
                b.AddLine(list[i].Item2, list[i].Item1);

            b.AddLine(list[0].Item2, list[0].Item1);
            b.EndFigure();
            b.EndGeography();

            return b.ConstructedGeography.EnvelopeAngle() > 90
                ? b.ConstructedGeography.ReorientObject()
                : b.ConstructedGeography;
        }

        //TOP LEFT LONGITUDE=item1, LATITUDE=item2, //BOTTOM RIGHT LONGITUDE=item3, LATITUDE=item4
        public static SqlGeography CreateMultiRectangle(this List<Tuple<double, double, double, double>> coordinates, int srid = 4326)
        {
            var list = coordinates.Distinct().ToList();
            var b = new SqlGeographyBuilder();
            b.SetSrid(srid);
            b.BeginGeography(OpenGisGeographyType.MultiPolygon);

            for (var i = 0; i < list.Count; i++)
            {
                b.BeginGeography(OpenGisGeographyType.Polygon);
                b.BeginFigure(list[i].Item1, list[i].Item2);
                b.AddLine(list[i].Item1, list[i].Item4);
                b.AddLine(list[i].Item3, list[i].Item4);
                b.AddLine(list[i].Item3, list[i].Item2);
                b.AddLine(list[i].Item1, list[i].Item2);
                b.EndFigure();
                b.EndGeography();
            }
            b.EndGeography();

            return b.ConstructedGeography.EnvelopeAngle() > 90
                ? b.ConstructedGeography.ReorientObject()
                : b.ConstructedGeography;
        }

        //LONGITUDE=item1, LATITUDE=item2
        public static SqlGeography CreatePoint(this Tuple<double, double> coordinates, int srid = 4326)
        {
            var b = new SqlGeographyBuilder();
            b.SetSrid(srid);
            b.BeginGeography(OpenGisGeographyType.Point);
            b.BeginFigure(coordinates.Item2, coordinates.Item1);
            b.EndFigure();
            b.EndGeography();
            return b.ConstructedGeography;
        }

        public static SqlGeography CreateGeography(this string points, out OpenGisGeographyType geographyType, int srid = 4326)
        {
            geographyType = OpenGisGeographyType.Point;
            SqlGeography firstPoint = null;
            var pts = points.Trim().Split(new string[] { "\n" }, StringSplitOptions.None).ToList();
            pts.Add(null);
            var b = new SqlGeographyBuilder();
            b.SetSrid(srid);
            b.BeginGeography(OpenGisGeographyType.GeometryCollection);
            var coordinates = new List<Tuple<double, double>>();
            for (int i = 0; i < pts.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(pts[i]))
                {
                    if (coordinates.Count == 1)
                    {
                        if (firstPoint == null)
                            firstPoint = coordinates[0].CreatePoint();
                        b.BeginGeography(OpenGisGeographyType.Point);
                        b.BeginFigure(coordinates[0].Item2, coordinates[0].Item1);
                        b.EndFigure();
                        b.EndGeography();
                    }
                    else if (coordinates.Count > 1)
                    {
                        geographyType = OpenGisGeographyType.GeometryCollection;
                        var list = coordinates.Distinct().ToList();
                        b.BeginGeography(OpenGisGeographyType.Polygon);
                        b.BeginFigure(list[0].Item2, list[0].Item1);
                        for (var j = 1; j < list.Count; j++)
                            b.AddLine(list[j].Item2, list[j].Item1);
                        b.AddLine(list[0].Item2, list[0].Item1);
                        b.EndFigure();
                        b.EndGeography();
                    }
                    coordinates = new List<Tuple<double, double>>();
                    continue;
                }
                var pt = pts[i].Trim().Split(new string[] { " ", ",", ";", "\t", ":", "\r" }, StringSplitOptions.RemoveEmptyEntries);
                coordinates.Add(new Tuple<double, double>(double.Parse(pt[0]), double.Parse(pt[1])));
            }
            b.EndGeography();
            if (geographyType == OpenGisGeographyType.GeometryCollection)
                return b.ConstructedGeography.EnvelopeAngle() > 90 ? b.ConstructedGeography.ReorientObject() : b.ConstructedGeography;
            else
                return firstPoint;
        }
    }

}