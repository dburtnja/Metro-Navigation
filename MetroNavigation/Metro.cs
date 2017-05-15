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
                startStation = lines[i].findStation(start, 0);
            }

            //find end station
            for (int i = 0; i < linesNbr && endStation == null; i++)
            {
                endStation = lines[i].findStation(end, 0);
            }

            if (startStation == null || endStation == null)
                return null;

            //create path
            PathFigure pFigure = new PathFigure();
            //set start point
            pFigure.StartPoint = startStation.stationCoordinates;

            if ((int)(startStation.stationNbr / 100) == (int)(endStation.stationNbr / 100))
            {
                //when start and end station are on same line

                PathSegmentCollection pathCollection = new PathSegmentCollection();

                int line = (int)(startStation.stationNbr / 100) - 1;
                lines[line].writeLineSegment(pathCollection, startStation, endStation);
                if (pathCollection == null)
                    return null;
                pFigure.Segments = pathCollection;
                return pFigure;
            }
            else
            {
                //when start and end station are on different lines

                Station transferStationFrom;
                Station transferStationTill;
                PathSegmentCollection pathCollection = new PathSegmentCollection();

                int line1 = (int)(startStation.stationNbr / 100) - 1;
                int line2 = (int)(endStation.stationNbr / 100) - 1;
                if ((transferStationFrom = lines[line1].findTransferStation(line2 + 1)) == null)
                    return null;
                if ((transferStationTill = lines[line2].findStation(new Point(), transferStationFrom.transferStation)) == null)
                    return null;
                lines[line1].writeLineSegment(pathCollection, startStation, transferStationFrom);
                lines[line2].writeLineSegment(pathCollection, transferStationTill, endStation);
                if (pathCollection == null)
                    return null;
                pFigure.Segments = pathCollection;
                return pFigure;
            }
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
            string str = streamReader.ReadLine();
            stationsNbr = Convert.ToInt16(Regex.Replace(str, "Line" + line + "Stations=", String.Empty));

            stations = new Station[stationsNbr];
            //Reading all stations
            for (int i = 0; i < stationsNbr; i++)
            {
                stations[i] = new Station(streamReader.ReadLine(), i);
            }
        }

        public Station findStation(Point p, int nbr)
        {
            Station station = null;

            for (int i = 0; i < stationsNbr && station == null; i++)
            {
                if (p != null)
                    station = stations[i].isInRange(p);
                if (nbr != 0 && nbr == stations[i].stationNbr)
                    return stations[i];
            }
            return station;
        }

        //when start and end station are on same line
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
                //adding all points
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

        public Station findTransferStation(int lineToChange)
        {
            for (int i = 0; i < stationsNbr; i++)
            {
                if (lineToChange == (int)(stations[i].transferStation / 100))
                    return stations[i];
            }
            return null;
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
