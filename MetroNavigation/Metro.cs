using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MetroNavigation
{
    class Metro
    {
        public int linesNbr;
        public Line[] lines;

        public Metro()
        {
            //Reading map from file
            StreamReader streamReader = new StreamReader("Map/Map.txt", Encoding.GetEncoding(1251));

            //Reading number of MetroLines
            linesNbr = Convert.ToInt16(Regex.Replace(streamReader.ReadLine(), "LineNumder=", String.Empty));

            lines = new Line[linesNbr];
            //Reading all MetroLines
            for (int i = 0; i < linesNbr; i++)
            {
                lines[i] = new Line(streamReader, i + 1);
            }
        }

        public PathFigure findPath(Point start, Point end)
        {
            Station startStation = null;
            Station endStation = null;

            //find start station
            for (int i = 0; i < linesNbr && startStation == null; i++)
            {
                startStation = lines[i].findStation(start);
            }

            //find end station
            for (int i = 0; i < linesNbr && endStation == null; i++)
            {
                endStation = lines[i].findStation(end);
            }

            if (startStation == null || endStation == null)
                return null;

            //create path
            PathFigure pFigure = new PathFigure();
            //set start point
            pFigure.StartPoint = startStation.stationCoordinates;

            if ((int)(startStation.stationNbr / 100) == (int)(endStation.stationNbr / 100))
            {
                PathSegmentCollection pathCollection = new PathSegmentCollection();

                int line = (int)(startStation.stationNbr / 100) - 1;
                lines[line].writeLineSegment(pathCollection, startStation, endStation);
                if (pathCollection == null)
                    return null;
                pFigure.Segments = pathCollection;
                return pFigure;
            }

            return pFigure;
        }
    }

    class Line
    {
        public int lineNbr;
        public int stationsNbr;
        public Station[] stations;

        public Line(StreamReader streamReader, int line)
        {
            lineNbr = line;
            //Reading number of MetroLines
            stationsNbr = Convert.ToInt16(Regex.Replace(streamReader.ReadLine(), "Line" + line + "Stations=", String.Empty));

            stations = new Station[stationsNbr];
            //Reading all stations
            for (int i = 0; i < stationsNbr; i++)
            {
                stations[i] = new Station(streamReader.ReadLine(), i);
            }
        }

        public Station findStation(Point p)
        {
            Station station = null;

            for (int i = 0; i < stationsNbr && station == null; i++)
            {
                station = stations[i].isInRange(p);
            }
            return station;
        }

        public void writeLineSegment(PathSegmentCollection pathCollection, Station from, Station till)
        {
            int fromNbr = from.arrayNbr;

            if (fromNbr < till.arrayNbr)
            {
                //adding all points
                while (fromNbr <= till.arrayNbr)
                {
                    LineSegment lineSegment = new LineSegment();
                    lineSegment.Point = stations[fromNbr].stationCoordinates;
                    pathCollection.Add(lineSegment);
                    fromNbr++;
                }
            }
            else if (fromNbr > till.arrayNbr)
            {
                while (fromNbr >= till.arrayNbr)
                {
                    LineSegment lineSegment = new LineSegment();
                    lineSegment.Point = stations[fromNbr].stationCoordinates;
                    pathCollection.Add(lineSegment);
                    fromNbr--;
                }
            }
            else
                pathCollection = null;
        }
    }

    class Station
    {
        public int arrayNbr;
        public int stationNbr;
        public String stationName;
        public Point stationCoordinates;
        public int transferStation = 0;

        public Station(String input, int i)
        {
            arrayNbr = i;
            if (input == null)
                MessageBox.Show("Помилка зчитування файлу карти");
            else
            {
                string[] array = input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                stationNbr = Convert.ToInt16(array[0]);
                stationName = array[1];
                stationCoordinates = new Point { X = Convert.ToInt16(array[2]), Y = Convert.ToInt16(array[3]) };
                if (array.Length == 5)
                    transferStation = Convert.ToInt16(array[4]);
            }
        }

        public Station isInRange(Point p)
        {
            if (Point.Subtract(p, stationCoordinates).Length < 10)
                return this;
            return null;
        }
    }
}
